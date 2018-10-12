/*
 * ChryslerCCDSCIScanner (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
 * Copyright (C) 2018, László Dániel
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * 
 * UART code is based on original library by Andy Gock:
 * https://github.com/andygock/avr-uart
 */

// Board setting: Arduino/Genuino Mega or Mega 2560
// Processor setting: ATmega2560 (Mega 2560)

#include <avr/interrupt.h>
#include <avr/io.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <util/atomic.h>
#include <TimerOne.h>  // https://github.com/PaulStoffregen/TimerOne
#include <extEEPROM.h> // https://github.com/JChristensen/extEEPROM
#include <Wire.h>
#include "ccdsciuart.h"

#ifndef F_CPU
#define F_CPU 16000000UL // 16 MHz system clock
#endif

#define INT4      2  // CCD-bus idle interrupt
#define INT5      3  // CCD-bus active byte interrupt
#define RX_LED    35 // status LED, message received
#define TX_LED    36 // status LED, message sent
#define ACT_LED   37 // status LED, activity

// Construct an object called "eep" for the external 24LC32A EEPROM chip
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

uint32_t current_millis_blink = 0;
uint32_t previous_millis_blink = 0;
uint16_t interval = 250; // milliseconds
bool act_led_state = false;
uint8_t cycles = 0;

void setup()
{
    /* In case of WATCHDOG reset */
    ccd_clock_generator(STOP); // stop listening to the ccd-bus by cutting off the 1 MHz clock signal to the CCD-transceiver chip
    cli(); // disable interrupts thus stop serial communication in every possible way
    usb_rx_flush(); // reset serial buffers
    usb_tx_flush();
    ccd_rx_flush();
    ccd_tx_flush();
    pcm_rx_flush();
    pcm_tx_flush();
    tcm_rx_flush();
    tcm_tx_flush();
    ccd_bytes_buffer_ptr = 0; // reset buffer pointers / indexers
    pcm_bytes_buffer_ptr = 0;
    tcm_bytes_buffer_ptr = 0;
    ccd_msg_pending = false;  // delete any pending message
    pcm_msg_pending = false;
    tcm_msg_pending = false;
    ccd_msg_to_send_ptr = 0;
    pcm_msg_to_send_ptr = 0;
    tcm_msg_to_send_ptr = 0;
    ccd_idle = false;         // wait for the next ID-byte
    pcm_idle = false;         // wait for the next byte following an idle-time
    tcm_idle = false;         // ---||---
    connected_to_vehicle = false;
    /* END */
    
    // Initialize serial interfaces with default speeds and interrupt control enabled
    usb_init(USBBAUD);// 115200 baud, an external serial monitor should have the same speed (115200 baud)
    ccd_init(LOBAUD); // 7812.5 baud
    pcm_init(LOBAUD); // 7812.5 baud
    tcm_init(LOBAUD); // 7812.5 baud

    // Define digital pin states
    // No need to re-define input states...
    //pinMode(INT4, INPUT);         // D2 (INT4), CCD-bus idle detector
    //pinMode(INT5, INPUT);         // D3 (INT5), CCD-bus active byte detector
    //digitalWrite(INT4, HIGH);     // Enable internal pull-up resistor on D2 (INT4), it's pulled up by hardware
    //digitalWrite(INT5, HIGH);     // Enable internal pull-up resistor on D3 (INT5), it's pulled up by hardware
    pinMode(RX_LED,  OUTPUT);     // This LED flashes whenever data is received by the scanner
    pinMode(TX_LED,  OUTPUT);     // This LED flashes whenever data is transmitted from the scanner
    pinMode(ACT_LED, OUTPUT);     // This LED flashes when some "action" takes place in the scanner
    digitalWrite(RX_LED, HIGH);   // LEDs are grounded through the microcontroller, so HIGH/HI-Z = OFF, LOW = ON
    digitalWrite(TX_LED, HIGH);   // ---||---
    digitalWrite(ACT_LED, HIGH);  // ---||---

    // SCI-bus A/B-configuration selector outputs
    pinMode(PA0, OUTPUT);   // Set PA0 pin to output
    pinMode(PA1, OUTPUT);   // |
    pinMode(PA2, OUTPUT);   // |
    pinMode(PA3, OUTPUT);   // |
    pinMode(PA4, OUTPUT);   // |
    pinMode(PA5, OUTPUT);   // |
    pinMode(PA6, OUTPUT);   // |
    pinMode(PA7, OUTPUT);   // |
    digitalWrite(PA0, LOW); // Set PA0 low to disable SCI-bus communication by default
    digitalWrite(PA1, LOW); // |
    digitalWrite(PA2, LOW); // |
    digitalWrite(PA3, LOW); // |
    digitalWrite(PA4, LOW); // |
    digitalWrite(PA5, LOW); // |
    digitalWrite(PA6, LOW); // |
    digitalWrite(PA7, LOW); // |

    sei(); // enable interrupts, serial interrupt control resumes working
    attachInterrupt(INT4, ccd_eom, FALLING); // execute "ccd_eom" function if the CCD-transceiver pulls D2 pin low indicating an "End of Message" condition so the byte reader ISR can flag the next byte as ID-byte
    attachInterrupt(INT5, ccd_active_byte, FALLING); // execute "ccd_active_byte" function if the CCD-transceiver pulls D3 pin low indicating a byte being transmitted on the CCD-bus
    // We don't know the byte's value right away, we have to wait for all 8 data bits and a few other bits for framing to arrive.
  
    wdt_enable(WDTO_2S); // reset program if the watchdog timer reaches 2 seconds
    
    // Initialize external EEPROM
    uint8_t eep_status = eep.begin(extEEPROM::twiClock400kHz); // go fast!
    if (eep_status) { ext_eeprom_present = false; }
    else { ext_eeprom_present = true; }

    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    check_battery_volts(); // calculate battery voltage from OBD16 pin
    ccd_clock_generator(START); // start listening to the CCD-bus
    get_bus_config(); // figure out how to talk to the vehicle
}

void loop()
{
    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    check_battery_volts(); // calculate battery voltage from OBD16 pin
    handle_usb_data(); // check if a command has been received over the USB connection
    if (ccd_enabled) { handle_ccd_data(); } // do CCD-bus stuff if it's enabled
    if (sci_enabled) { handle_sci_data(); } // do SCI-bus stuff if it's enabled

    // Blink activity LED to show looping is OK and didn't freeze somewhere.
    current_millis_blink = millis(); // check current time

    // Check if enough time has elapsed (interval) to invert LED state
    if (current_millis_blink - previous_millis_blink >= interval)
    {
        previous_millis_blink = current_millis_blink; // save current time
        if (act_led_state) act_led_state = false;
        else act_led_state = true;
        digitalWrite(ACT_LED, act_led_state);
        
//        cycles++;
//        if (cycles == 10)
//        {
//            cycles = 0;
//            while(true); // test watchdog reset, blink should freeze for 2 seconds, reset occurs, led resumes blinking
//        }
    }
}


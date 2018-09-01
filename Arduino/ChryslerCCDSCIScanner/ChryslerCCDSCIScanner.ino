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

#include <avr/eeprom.h>
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

void setup()
{
    cli(); // disable interrupts
    
    stop_ccd_clock_generator(); // stop listening to the ccd-bus
    
    // Reset UART buffers in case of watchdog-reset occurs
    usb_rx_flush(); 
    usb_tx_flush();
    ccd_rx_flush();
    ccd_tx_flush();
    pcm_rx_flush();
    pcm_tx_flush();
    tcm_rx_flush();
    tcm_tx_flush();
    ccd_bytes_buffer_ptr = 0;
    pcm_bytes_buffer_ptr = 0;
    tcm_bytes_buffer_ptr = 0;
    ccd_msg_to_send_ptr = 0;
    pcm_msg_to_send_ptr = 0;
    tcm_msg_to_send_ptr = 0;
    
    // Initialize serial interfaces with default speeds
    usb_init(USBBAUD);// 115200 baud, serial monitor should have the same speed
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
    digitalWrite(TX_LED, HIGH);   //
    digitalWrite(ACT_LED, HIGH);

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

    attachInterrupt(INT4, ccd_eom, FALLING); // execute "ccd_eom" function if the CCD-transceiver pulls D2 pin low indicating an "End of Message" condition
    attachInterrupt(INT5, ccd_active_byte, FALLING); // execute "ccd_active_byte" function if the CCD-transceiver pulls D3 pin low indicating an active byte on the CCD-bus 

    sei(); // enable interrupts

    // Copy handshake bytes from flash to ram
    for (uint8_t i = 0; i < 21; i++)
    {
        handshake_array[i] = pgm_read_byte(&handshake_progmem[i]);
    }
    
    wdt_enable(WDTO_2S); // reset program if the watchdog timer reaches 2 seconds
    start_ccd_clock_generator(); // start listening to the CCD-bus
    
    // Initialize external EEPROM
    uint8_t eep_status = eep.begin(extEEPROM::twiClock400kHz); // go fast!
    if (eep_status) { ext_eeprom_present = false; }
    else { ext_eeprom_present = true; }

    check_battery_volts(); // calculate battery voltage from OBD16 pin
    wdt_reset(); // reset watchdog timer to 0 seconds so no intentional restart occurs
    discover_bus_configuration(); // figure out how to talk to the vehicle
}

void loop()
{
    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    handle_usb_rx_bytes(); // check if a command has been received over the USB connection
    if (ccd_enabled) { handle_ccd_rx_bytes(); } // do CCD-bus stuff if it's enabled
    if (sci_enabled) { handle_sci_rx_bytes(); } // do SCI-bus stuff if it's enabled
    check_battery_volts(); // calculate battery voltage from OBD16 pin
}


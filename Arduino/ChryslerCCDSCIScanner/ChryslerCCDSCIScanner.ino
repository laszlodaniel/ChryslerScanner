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
#define BATT      A0 // battery voltage sensor

// Construct an object called "eep" for the external 24LC32A EEPROM chip
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

void setup()
{
    // Initialize serial interfaces with default speeds
    usb_init(USBBAUD);// 115200 baud, serial monitor should have the same speed
    ccd_init(LOBAUD); // 7812.5 baud
    pcm_init(LOBAUD); // 7812.5 baud
    tcm_init(LOBAUD); // 7812.5 baud

    // Define digital pin states
    pinMode(INT4, INPUT);      // D2 (INT4), CCD-bus idle detector
    pinMode(INT5, INPUT);      // D3 (INT5), CCD-bus active byte detector
    digitalWrite(INT4, HIGH);  // Enable internal pull-up resistor on D2 (INT4), it's pulled up by hardware so let's waste resources
    digitalWrite(INT5, HIGH);  // Enable internal pull-up resistor on D3 (INT5), it's pulled up by hardware so let's waste resources
    pinMode(RX_LED,  OUTPUT);
    pinMode(TX_LED,  OUTPUT);
    pinMode(ACT_LED, OUTPUT);
    digitalWrite(RX_LED, HIGH); // LEDs are grounded through the microcontroller, so HIGH/HI-Z = OFF, LOW = ON
    digitalWrite(TX_LED, HIGH);
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
    
    start_ccd_clock_generator(); // start listening to the CCD-bus

    // Initialize external EEPROM
    uint8_t eep_status = eep.begin(extEEPROM::twiClock400kHz);   // go fast!
    if (eep_status)
    {
        // ERROR, TODO
    } 
}

void loop()
{
    // Check if a command has been received over the USB connection
    if (usb_enabled) { handle_usb_rx_bytes(); }
    
    // Do CCD-bus stuff if it's enabled
    if (ccd_enabled) { handle_ccd_rx_bytes(); }

    // Do SCI-bus stuff if it's enabled
    if (sci_enabled) { handle_sci_rx_bytes(); }
}


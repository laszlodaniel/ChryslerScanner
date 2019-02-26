/*
 * ChryslerCCDSCIScanner (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
 * Copyright (C) 2018-2019, László Dániel
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
#include <extEEPROM.h>         // https://github.com/JChristensen/extEEPROM
#include <LiquidCrystal_I2C.h> // https://bitbucket.org/fmalpartida/new-liquidcrystal/downloads/
#include <Wire.h>
#include "ccdsciuart.h"

#ifndef F_CPU
#define F_CPU 16000000UL // 16 MHz system clock
#endif

// Construct an object called "eep" for the external 24LC32A EEPROM chip
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

// Construct an object called "lcd" for the external display (optional)
LiquidCrystal_I2C lcd(0x27, 2, 1, 0, 4, 5, 6, 7, 3, POSITIVE);

void setup()
{
    // Define digital pin states
    pinMode(INT4, INPUT_PULLUP); // D2 (INT4), CCD-bus idle detector
    pinMode(INT5, INPUT_PULLUP); // D3 (INT5), CCD-bus active byte detector
    pinMode(RX_LED, OUTPUT);     // This LED flashes whenever data is received by the scanner
    pinMode(TX_LED, OUTPUT);     // This LED flashes whenever data is transmitted from the scanner
    // PWR LED is tied to +5V directly, stays on when the scanner has power, draws about 2mA current
    pinMode(ACT_LED, OUTPUT);    // This LED flashes when some "action" takes place in the scanner
    pinMode(BATT, INPUT);        // This analog input pin measures battery voltage through a resistor divider (it tolerates 24V batteries!)
    blink_led(RX_LED);
    blink_led(TX_LED);
    blink_led(ACT_LED);

    // SCI-bus A/B-configuration selector outputs
    pinMode(PA0, OUTPUT);   // Set PA0 pin to output
    pinMode(PA1, OUTPUT);   // |
    pinMode(PA2, OUTPUT);   // |
    pinMode(PA3, OUTPUT);   // |
    pinMode(PA4, OUTPUT);   // |
    pinMode(PA5, OUTPUT);   // |
    pinMode(PA6, OUTPUT);   // |
    pinMode(PA7, OUTPUT);   // |

    digitalWrite(PA0, HIGH); // Default settings: SCI-bus "A" configuration, PCM only
    digitalWrite(PA1, HIGH);
    digitalWrite(PA2, LOW);
    digitalWrite(PA3, LOW);
    digitalWrite(PA4, LOW);
    digitalWrite(PA5, LOW);
    digitalWrite(PA6, LOW);
    digitalWrite(PA7, LOW);
    
    wdt_enable(WDTO_2S); // enable watchdog timer that resets program if its timer reaches 2 seconds (useful if the prorgam hangs for some reason and needs auto-reset)
    attachInterrupt(digitalPinToInterrupt(INT4), ccd_eom, FALLING); // execute "ccd_eom" function if the CCD-transceiver pulls D2 pin low indicating an "End of Message" condition
    attachInterrupt(digitalPinToInterrupt(INT5), ccd_active_byte, FALLING); // execute "ccd_active_byte" function if the CCD-transceiver pulls D3 pin low indicating a byte being transmitted on the CCD-bus
    // active byte = we don't know the byte's value right away, we have to wait for all 8 data bits and a few other bits for framing to arrive.

    // Initialize serial interfaces with default speeds
    usb_init(USBBAUD);// 250000 baud, an external serial monitor should have the same speed
    ccd_init(LOBAUD); // 7812.5 baud
    pcm_init(LOBAUD); // 7812.5 baud
    tcm_init(LOBAUD); // 7812.5 baud
    
    // Initialize external EEPROM, read hardware version/date, assembly date, firmware date and a checksum byte for all of this
    eep_status = eep.begin(extEEPROM::twiClock400kHz); // go fast!
    if (eep_status) // non-zero = bad
    { 
        eep_present = false;
        uint8_t err[1];
        err[0] = 0xFF;
        send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);

        hw_version[0] = 0; // zero out values
        hw_version[1] = 0;
        hw_date[0] = 0; // zero out values
        hw_date[1] = 0;
        hw_date[2] = 0;
        hw_date[3] = 0;
        hw_date[4] = 0;
        hw_date[5] = 0;
        hw_date[6] = 0;
        hw_date[7] = 0;
        assembly_date[0] = 0; // zero out values
        assembly_date[1] = 0;
        assembly_date[2] = 0;
        assembly_date[3] = 0;
        assembly_date[4] = 0;
        assembly_date[5] = 0;
        assembly_date[6] = 0;
        assembly_date[7] = 0;
        eep_checksum[0] = 0; // zero out value
        eep_calculated_checksum = 0;
    }
    else // zero = good
    {
        eep_present = true;
        eep_result = eep.read(0, hw_version, 2); // read first 2 bytes and store it in the hw_version array
        if (eep_result)
        {
            uint8_t err[1];
            err[0] = eep_result;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);

            hw_version[0] = 0; // zero out values
            hw_version[1] = 0;
        }
        eep_result = eep.read(2, hw_date, 8); // read following 8 bytes in the hw_date array
        if (eep_result)
        {
            uint8_t err[1];
            err[0] = eep_result;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);

            hw_date[0] = 0; // zero out values
            hw_date[1] = 0;
            hw_date[2] = 0;
            hw_date[3] = 0;
            hw_date[4] = 0;
            hw_date[5] = 0;
            hw_date[6] = 0;
            hw_date[7] = 0;
            
        }
        eep_result = eep.read(10, assembly_date, 8); // read following 8 bytes in the assembly_date array
        if (eep_result)
        {
            uint8_t err[1];
            err[0] = eep_result;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);

            assembly_date[0] = 0; // zero out values
            assembly_date[1] = 0;
            assembly_date[2] = 0;
            assembly_date[3] = 0;
            assembly_date[4] = 0;
            assembly_date[5] = 0;
            assembly_date[6] = 0;
            assembly_date[7] = 0;
        }
        eep_result = eep.read(255, eep_checksum, 1); // read 255th byte for the checksum byte (total of 256 bytes are reserved for hardware description)
        if (eep_result)
        {
            uint8_t err[1];
            err[0] = eep_result;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);

            eep_checksum[0] = 0; // zero out value
        }

        eep_calculated_checksum = 0;
        for (uint8_t i = 0; i < 255; i++) // add all 255 bytes together and skip last byte (where checksum byte is located) by setting the second parameter to 255 instead of 256
        {
            eep_calculated_checksum += eep.read(i); // checksum variable will roll over several times but it's okay, this is its purpose
        }
    }

    // Initialize external display (optional)
    lcd.begin(20, 4); // start LCD with 20 columns and 4 rows
    lcd.backlight();  // backlight on
    lcd.clear();      // clear display
    lcd.home();       // set cursor in home position (0, 0)
    lcd.print(F("--------------------")); // F(" ") makes the compiler store the string inside flash memory instead of RAM, good practice if system is low on RAM
    lcd.setCursor(0, 1);
    lcd.print(F("  CHRYSLER CCD/SCI  "));
    lcd.setCursor(0, 2);
    lcd.print(F(" SCANNER V1.40 2018 "));
    lcd.setCursor(0, 3);
    lcd.print(F("--------------------"));

    analogReference(DEFAULT); // use default voltage reference applied to AVCC (+5V)
    check_battery_volts(); // calculate battery voltage from OBD16 pin
    ccd_clock_generator(START); // start listening to the CCD-bus; the transceiver chip only works if it receives this continuos clock signal; clever way to turn it on/off

    for (uint8_t i = 0; i < 21; i++) // copy handshake bytes from flash to ram
    {
        handshake_array[i] = pgm_read_byte(&handshake_progmem[i]);
    }

    usb_rx_flush(); // flush all uart buffers
    usb_tx_flush();
    ccd_rx_flush();
    ccd_tx_flush();
    pcm_rx_flush();
    pcm_tx_flush();
    tcm_rx_flush();
    tcm_tx_flush();

    get_bus_config(); // figure out how to talk to the vehicle
    
    uint8_t scanner_ready[1];
    scanner_ready[0] = 0x01;
    send_usb_packet(from_usb, to_usb, reset, ok, scanner_ready, 1); // Scanner ready
}

void loop()
{
    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    check_battery_volts(); // calculate battery voltage from OBD16 pin
    handle_usb_data(); // look for commands over USB connection
    handle_ccd_data(); // do CCD-bus stuff
    handle_sci_data(); // do SCI-bus stuff
    handle_leds(); // do LED stuff
}

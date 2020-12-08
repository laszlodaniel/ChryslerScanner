/*
 * ChryslerCCDSCIScanner (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
 * Copyright (C) 2018-2020, László Dániel
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
 *
 * Board: Arduino/Genuino Mega or Mega 2560
 * Processor: ATmega2560 (Mega 2560)
 * Fuse bytes:
 * - LF: 0xFF
 * - HF: 0xD0
 * - EF: 0xFD
 * - Lock: 0x3F
 */

#include <avr/boot.h>
#include <avr/eeprom.h>
#include <avr/interrupt.h>
#include <avr/io.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <util/atomic.h>
#include <CCDLibrary.h>        // https://github.com/laszlodaniel/CCDLibrary
#include <extEEPROM.h>         // https://github.com/JChristensen/extEEPROM
#include <LiquidCrystal_I2C.h> // https://github.com/fdebrabander/Arduino-LiquidCrystal-I2C-library
#include <Wire.h>
#include "main.h"

#ifndef F_CPU
#define F_CPU 16000000UL // 16 MHz system clock
#endif

// Construct an object called "eep" for the external 24LC32A EEPROM chip.
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

// Construct an object called "lcd" for the external display.
LiquidCrystal_I2C lcd(0x27, 20, 4);

void setup()
{
    // Define digital pin states.
    pinMode(RX_LED, OUTPUT);      // Data received LED
    pinMode(TX_LED, OUTPUT);      // Data transmitted LED
    // PWR LED is tied to +5V directly, stays on when the scanner has power, draws about 2mA current.
    pinMode(ACT_LED, OUTPUT);     // Activity (heartbeat) LED
    pinMode(BATT, INPUT);         // This analog input pin measures battery voltage through a resistor divider (it tolerates 24V batteries!)
    pinMode(CCD_POSITIVE, INPUT); // 
    pinMode(CCD_NEGATIVE, INPUT); // 
    pinMode(TBEN, OUTPUT);        // 
    digitalWrite(TBEN, HIGH);     // disable CCD-bus termination and bias
    blink_led(RX_LED);            // 
    blink_led(TX_LED);            // 
    blink_led(ACT_LED);           // 

    // SCI-bus A/B-configuration selector outputs.
    pinMode(PA0, OUTPUT);
    pinMode(PA1, OUTPUT);
    pinMode(PA2, OUTPUT);
    pinMode(PA3, OUTPUT);
    pinMode(PA4, OUTPUT);
    pinMode(PA5, OUTPUT);
    pinMode(PA6, OUTPUT);
    pinMode(PA7, OUTPUT);

    exteeprom_init(); // initialize external EEPROM chip (24LC32A)

    ccd.bus_settings = 0x41; // CCD-bus disabled, non-inverted, termination/bias disabled, 7812.5 baud
    ccd.repeated_msg_increment = 2;
    pcm.bus_settings = 0x91; // PCM enabled, non-inverted, configuration "A", 7812.5 baud
    pcm.enabled = true;
    tcm.bus_settings = 0xC1; // TCM disabled, non-inverted, configuration "A", 7812.5 baud

    // Initialize serial interfaces with default speeds.
    usb_init(USBBAUD);// 250000 baud, an external serial monitor should have the same speed
    ccd_init(LOBAUD); // 7812.5 baud
    pcm_init(LOBAUD); // 7812.5 baud
    tcm_init(LOBAUD); // 7812.5 baud

    analogReference(DEFAULT);   // use default voltage reference applied to AVCC (+5V)
    check_battery_volts();      // measure battery voltage from OBD16 pin
    check_ccd_volts();          // measure CCD-bus wire voltages
    randomSeed(analogRead(3));  // use A3 analog input pin's floatling noise to generate random numbers

    read_avr_signature(avr_signature); // read AVR signature bytes that identifies the microcontroller

    usb_rx_flush(); // flush all uart buffers
    usb_tx_flush();
    //ccd_rx_flush();
    //ccd_tx_flush();
    pcm_rx_flush();
    pcm_tx_flush();
    tcm_rx_flush();
    tcm_tx_flush();

    lcd_init(); // initialize external LCD
    delay(2000);

    if (lcd_enabled)
    {
        switch (lcd_units)
        {
            case 0:
            {
                print_display_layout_1_imperial();
                break;
            }
            case 1:
            {
                print_display_layout_1_metric();
                break;
            }
            default:
            {
                print_display_layout_1_imperial();
                break;
            }
        }
    }

    send_usb_packet(from_usb, to_usb, reset, reset_done, ack, 1); // scanner ready
    configure_sci_bus(0xB1); // force sending an sci-bus settings packet
    send_hwfw_info(); // send hardware/firmware information to laptop
    cmd_status(); // send status
    wdt_enable(WDTO_2S); // enable watchdog timer that resets program if its timer reaches 2 seconds (useful if the code hangs for some reason and needs auto-reset)
}

void loop()
{
    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    handle_usb_data(); // look for commands over USB connection
    handle_ccd_data(); // do CCD-bus stuff
    handle_sci_data(); // do SCI-bus stuff
    handle_leds(); // do LED stuff
}

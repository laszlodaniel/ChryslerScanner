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

// Board: Arduino/Genuino Mega or Mega 2560
// Processor: ATmega2560 (Mega 2560)
// Fuse bytes:
// - HF: 0xD0
// - LF: 0xFF
// - EF: 0xFD
// - Lock: 0xFF

#include <avr/interrupt.h>
#include <avr/io.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <util/atomic.h>
#include <extEEPROM.h>         // https://github.com/JChristensen/extEEPROM
#include <LiquidCrystal_I2C.h> // https://bitbucket.org/fmalpartida/new-liquidcrystal/downloads/
#include <Wire.h>
#include "main.h"

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
    pinMode(PA0, OUTPUT);
    pinMode(PA1, OUTPUT);
    pinMode(PA2, OUTPUT);
    pinMode(PA3, OUTPUT);
    pinMode(PA4, OUTPUT);
    pinMode(PA5, OUTPUT);
    pinMode(PA6, OUTPUT);
    pinMode(PA7, OUTPUT);

    configure_sci_bus(0x02); // SCI-bus "A" configuration, 7812.5 baud, PCM only

    attachInterrupt(digitalPinToInterrupt(INT4), ccd_eom, FALLING); // execute "ccd_eom" function if the CCD-transceiver pulls D2 pin low indicating an "End of Message" condition
    attachInterrupt(digitalPinToInterrupt(INT5), ccd_active_byte, FALLING); // execute "ccd_active_byte" function if the CCD-transceiver pulls D3 pin low indicating a byte being transmitted on the CCD-bus
    
    // Initialize serial interfaces with default speeds
    usb_init(USBBAUD);// 250000 baud, an external serial monitor should have the same speed
    ccd_init(LOBAUD); // 7812.5 baud
    pcm_init(LOBAUD); // 7812.5 baud
    tcm_init(LOBAUD); // 7812.5 baud
    
    exteeprom_init();
    lcd_init();
    
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
    
    delay(2000);
    print_display_layout_1_metric();
    
    uint8_t scanner_ready[1];
    scanner_ready[0] = 0x01;
    send_usb_packet(from_usb, to_usb, reset, ok, scanner_ready, 1); // Scanner ready

    wdt_enable(WDTO_2S); // enable watchdog timer that resets program if its timer reaches 2 seconds (useful if the prorgam hangs for some reason and needs auto-reset)
}

void loop()
{
    wdt_reset(); // reset watchdog timer to 0 seconds so no accidental restart occurs
    check_battery_volts(); // calculate battery voltage from OBD16 pin
    handle_usb_data(); // look for commands over USB connection
    handle_ccd_data(); // do CCD-bus stuff
    handle_sci_data(); // do SCI-bus stuff
    handle_leds(); // do LED stuff
    handle_lcd();  // do LCD stuff
}

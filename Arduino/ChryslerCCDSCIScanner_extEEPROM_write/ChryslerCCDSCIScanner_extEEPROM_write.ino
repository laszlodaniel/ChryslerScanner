/*
 * ChryslerCCDSCIScanner_extEEPROM_write (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
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
 * PrintHex8 and PrintHex16 functions are the intellectual products of Kairama Inc.
 * https://forum.arduino.cc/index.php?topic=38107.0
 */

// Board setting: Arduino/Genuino Mega or Mega 2560
// Processor setting: ATmega2560 (Mega 2560)

#include <extEEPROM.h> // https://github.com/JChristensen/extEEPROM
#include <Wire.h>

#ifndef F_CPU
#define F_CPU 16000000UL // 16 MHz system clock
#endif

// Construct an object called "eep" for the external 24LC32A EEPROM chip
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

// First 256 bytes of the total 4096 bytes are reserved for identification purposes
// Memory map:
// $00-$01: hardware version (times 100, example: V1.40 = 140(DEC) = $008C)
// $02-$09: hardware date (PCB order date, 64-bit UNIX time)
// $0A-$11: assembly date (Scanner assembly date, 64-bit UNIX time)
// $12-$FE: reserved for future data
uint8_t eeprom_mem_00_ff[256] =
{
    0x00, 0x8C, 0x00, 0x00, 0x00, 0x00, 0x5B, 0x7B, 
    0xEA, 0x33, 0x00, 0x00, 0x00, 0x00, 0x5C, 0x04, 
    0xF9, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 // last entry is the checksum byte which is calculated automatically before eeprom write
};

void PrintHex8(uint8_t *data, uint8_t len) // prints 8-bit data in hex with leading zeroes
{
    char tmp[16];
    for (int i = 0; i < len; i++)
    { 
        sprintf(tmp, "0x%.2X", data[i]); 
        Serial.print(tmp);
        Serial.print(" ");
    }
}

void PrintHex16(uint16_t *data, uint8_t len) // prints 16-bit data in hex with leading zeroes
{
    char tmp[16];
    for (int i = 0; i < len; i++)
    { 
        sprintf(tmp, "0x%.4X", data[i]); 
        Serial.print(tmp);
        Serial.print(" ");
    }
}

void setup()
{
    Serial.begin(250000);
    
    uint8_t eep_status = eep.begin(extEEPROM::twiClock400kHz);   //go fast!
    if (eep_status)
    {
        Serial.print(F("extEEPROM.begin() failed, status = "));
        Serial.println(eep_status);
        while(true); // don't go further
    }

    uint8_t checksum[1]; // begin checksum calculation in this single element array (PrintHex8 only accepts arrays as inputs)
    checksum[0] = 0;
    for (uint8_t i = 0; i < 255; i++) // add all 255 bytes together and skip last byte (where the result of this calculation goes) by setting the second parameter to 255 instead of 256
    {
        checksum[0] += eeprom_mem_00_ff[i]; // checksum variable will roll over several times but it's okay, this is its purpose
    }
    eeprom_mem_00_ff[255] = checksum[0] & 0xFF; // place checksum byte at the last position of the array
    
    // Disable hardware write protection (EEPROM chip has a pullup resistor on its WP-pin!)
    DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
    PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection
    
    uint8_t eep_result = eep.write(0, eeprom_mem_00_ff, 256); // write bytes to external eeprom
    
    PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection

    if (eep_result)
    {
        Serial.print(F("extEEPROM.write() failed, result = "));
        Serial.println(eep_result); // print error code to serial port
    }
    else
    {
        Serial.print(F("Success! Checksum reads as: "));
        PrintHex8(checksum, 1);
        Serial.println();
    }

    // TODO: read eeprom data back and compare with original data to ensure everything is OK.
}

void loop()
{
    // Do nothing else
}

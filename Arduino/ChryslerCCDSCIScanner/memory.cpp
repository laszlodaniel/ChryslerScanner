#include "memory.h"
#include "common.h"
#include "battery.h"
#include "lcd.h"
#include "led.h"
#include "usb.h"

// Construct an object called "eep" for the external 24LC32A EEPROM chip.
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

bool eep_present = false;
uint8_t eep_status = 0; // extEEPROM connection status is stored here
uint8_t eep_result = 0; // extEEPROM 
uint8_t eep_checksum[1];
uint8_t eep_calculated_checksum = 0;
bool eep_checksum_ok = false;

/*************************************************************************
Function: exteeprom_init()
Purpose:  initialize LCD
**************************************************************************/
void exteeprom_init(void)
{
    // Initialize external EEPROM, read hardware version/date, assembly date, firmware date and a checksum byte for all of this
    eep_status = eep.begin(extEEPROM::twiClock400kHz); // go fast!
    if (eep_status) // non-zero = bad
    { 
        eep_present = false;
        hw_version[0] = 0; // zero out values
        hw_version[1] = 0x90;
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
        adc_supply_voltage = 500;
        r19_value = 27000;
        r20_value = 5000;
        lcd_enabled = false;
        lcd_i2c_address = 0x27;
        lcd_char_width = 20;
        lcd_char_height = 4;
        lcd_refresh_rate = 1; // Hz
        lcd_units = 0; // 0-imperial, 1-metric
        eep_checksum[0] = 0; // zero out value
        eep_calculated_checksum = 0;
        send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
    }
    else // zero = good
    {
        eep_present = true;
        eep_result = eep.read(0x00, hw_version, 2); // read first 2 bytes and store it in the hw_version array
        if (eep_result) // error
        {
            hw_version[0] = 0; // zero out values
            hw_version[1] = 0;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        eep_result = eep.read(0x02, hw_date, 8); // read following 8 bytes in the hw_date array
        if (eep_result) // error
        {
            hw_date[0] = 0; // zero out values
            hw_date[1] = 0;
            hw_date[2] = 0;
            hw_date[3] = 0;
            hw_date[4] = 0;
            hw_date[5] = 0;
            hw_date[6] = 0;
            hw_date[7] = 0;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        eep_result = eep.read(0x0A, assembly_date, 8); // read following 8 bytes in the assembly_date array
        if (eep_result) // error
        {
            assembly_date[0] = 0; // zero out values
            assembly_date[1] = 0;
            assembly_date[2] = 0;
            assembly_date[3] = 0;
            assembly_date[4] = 0;
            assembly_date[5] = 0;
            assembly_date[6] = 0;
            assembly_date[7] = 0;
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }

        uint8_t data[2];
        eep_result = eep.read(0x12, data, 2); // read ADC power supply value
        if (eep_result) // error
        {
            adc_supply_voltage = 500; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            adc_supply_voltage = to_uint16(data[0], data[1]); // stored value
            if (adc_supply_voltage == 0) adc_supply_voltage = 500; // default value
        }

        eep_result = eep.read(0x14, data, 2); // read R19 resistor value (for calibration)
        if (eep_result)
        {
            r19_value = 27000; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            r19_value = to_uint16(data[0], data[1]); // stored value
            if (r19_value == 0) r19_value = 27000; // default value
        }
        
        eep_result = eep.read(0x16, data, 2); // read R20 resistor value (for calibration)
        if (eep_result)
        {
            r20_value = 5100; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            r20_value = to_uint16(data[0], data[1]); // stored value
            if (r20_value == 0) r20_value = 5100; // default value
        }

        eep_result = eep.read(0x18, data, 1); // read LCD enabled / disabled state
        if (eep_result) // error
        {
            lcd_enabled = false; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            if (data[0] & 0x01) lcd_enabled = true;
            else lcd_enabled = false;
        }

        eep_result = eep.read(0x19, data, 1); // read LCD I2C address
        if (eep_result)
        {
            lcd_i2c_address = 0x27; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_i2c_address = data[0]; // stored value
            if (lcd_i2c_address == 0) lcd_i2c_address = 0x27; // default value
        }

        eep_result = eep.read(0x1A, data, 1); // read LCD width
        if (eep_result)
        {
            lcd_char_width = 20; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_char_width = data[0]; // stored value
            if (lcd_char_width == 0) lcd_char_width = 20; // default value
        }

        eep_result = eep.read(0x1B, data, 1); // read LCD height
        if (eep_result)
        {
            lcd_char_height = 4; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_char_height = data[0]; // stored value
            if (lcd_char_height == 0) lcd_char_height = 4; // default value
        }

        eep_result = eep.read(0x1C, data, 1); // read LCD refresh rate
        if (eep_result)
        {
            lcd_refresh_rate = 20; // Hz,m default value
            lcd_update_interval = 50; // ms
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_refresh_rate = data[0]; // stored value
            lcd_update_interval = 1000 / lcd_refresh_rate; // ms
            if (lcd_refresh_rate == 0)
            {
                lcd_refresh_rate = 20; // Hz, default value
                lcd_update_interval = 50; // ms
            }
        }

        eep_result = eep.read(0x1D, data, 1); // read LCD units
        if (eep_result)
        {
            lcd_units = 0; // default value (imperial)
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_units = data[0]; // stored value
            if (lcd_units > 1) lcd_units = 0; // default value (imperial)
        }

        eep_result = eep.read(0x1E, data, 1); // read LCD data source
        if (eep_result)
        {
            lcd_data_source = 1; // default value (CCD-bus)
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            lcd_data_source = data[0]; // stored value
            if ((lcd_data_source == 0) || (lcd_data_source > 3)) lcd_data_source = 1; // default value (imperial)
        }

        eep_result = eep.read(0x1F, data, 2); // read LED heartbeat interval
        if (eep_result)
        {
            heartbeat_interval = 5000; // ms, default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            heartbeat_interval = (data[0] << 8) + data[1]; // stored value
            if (heartbeat_interval > 0) heartbeat_enabled = true;
            else heartbeat_enabled = false;
        }

        eep_result = eep.read(0x21, data, 2); // read LED blink duration
        if (eep_result)
        {
            led_blink_duration = 50; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            led_blink_duration = (data[0] << 8) + data[1]; // stored value
            if (led_blink_duration == 0) led_blink_duration = 50; // ms
        }
        
        eep_result = eep.read(0xFF, eep_checksum, 1); // read 255th byte for the checksum byte (total of 256 bytes are reserved for hardware description)
        if (eep_result) // error
        {
            eep_checksum[0] = 0; // zero out value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }

        eep_calculated_checksum = 0;
        for (uint8_t i = 0; i < 255; i++) // add all 255 bytes together and skip last byte (where checksum byte is located) by setting the second parameter to 255 instead of 256
        {
            eep_calculated_checksum += eep.read(i);
        }
    }
}

/*************************************************************************
Function: evaluate_eep_checksum()
Purpose:  compare external eeprom checksum value to the calculated one
          and send results to laptop
Note:     compared values are read during setup() automatically
**************************************************************************/
void evaluate_eep_checksum(void)
{
    if (eep_present)
    {
        if (eep_calculated_checksum == eep_checksum[0]) eep_checksum_ok = true;
        else eep_checksum_ok = false;

        uint8_t eep_checksum_response[3];
        if (eep_present) eep_checksum_response[0] = 0x01;
        else eep_checksum_response[0] = 0x00;
        eep_checksum_response[1] = eep_checksum[0]; // checksum reading
        eep_checksum_response[2] = eep_calculated_checksum; // calculated checksum
        send_usb_packet(from_usb, to_usb, response, exteeprom_checksum, eep_checksum_response, 3);
    }
    else
    {
        send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
    }
}

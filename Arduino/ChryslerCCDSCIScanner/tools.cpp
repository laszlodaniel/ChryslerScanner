#include "tools.h"
#include "common.h"
#include "memory.h"
#include "battery.h"
#include "lcd.h"
#include "led.h"
#include "usb.h"
#include <avr/boot.h>

uint32_t previous_random_ccd_msg_millis = 0;
uint8_t avr_signature[3];

/*************************************************************************
Function: update_timestamp()
Purpose:  get elapsed milliseconds since power up/reset from microcontroller and convert it to an array containing 4 bytes
Note:     this function updates a global byte array which can be read from anywhere in the code
**************************************************************************/
void update_timestamp(uint8_t *target)
{
    uint32_t mcu_current_millis = millis();
    target[0] = (mcu_current_millis >> 24) & 0xFF;
    target[1] = (mcu_current_millis >> 16) & 0xFF;
    target[2] = (mcu_current_millis >> 8) & 0xFF;
    target[3] = mcu_current_millis & 0xFF;
}

/*************************************************************************
Function: array_contains()
Purpose:  checks if a value is present in an array
Note:     only checks first occurence
**************************************************************************/
bool array_contains(uint8_t *src_array, uint8_t src_array_length, uint8_t value)
{
    for (uint16_t i = 0; i < src_array_length; i++)
    {
        if (value == src_array[i]) return true;
    }
    return false;
}

/*************************************************************************
Function: calculate_checksum()
Purpose:  calculate checksum in a given buffer with specified length
Note:     startindex = first byte in the array to include in calculation
          length = buffer full length
**************************************************************************/
uint8_t calculate_checksum(uint8_t *buff, uint16_t index, uint16_t bufflen)
{
    uint8_t cs = 0;
    for (uint16_t i = index ; i < bufflen; i++)
    {
        cs += buff[i]; // add bytes together
    }
    return cs;
}

/*************************************************************************
Function: free_ram()
Purpose:  returns how many bytes exists between the end of the heap and 
          the last allocated memory on the stack, so it is effectively 
          how much the stack/heap can grow before they collide.
**************************************************************************/
uint16_t free_ram(void)
{
    extern int  __bss_end; 
    extern int  *__brkval; 
    uint16_t free_memory; 
    
    if((int)__brkval == 0)
    {
        free_memory = ((int)&free_memory) - ((int)&__bss_end); 
    }
    else 
    {
        free_memory = ((int)&free_memory) - ((int)__brkval); 
    }
    return free_memory;
}

/*****************************************************************************
Function: read_avr_signature()
Purpose:  read AVR device signature bytes into a global array
Note:     ATmega2560: 1E 98 01
******************************************************************************/
void read_avr_signature(uint8_t *target)
{
    target[0] = boot_signature_byte_get(0x0000);
    target[1] = boot_signature_byte_get(0x0002);
    target[2] = boot_signature_byte_get(0x0004);
}

/*************************************************************************
Function: send_hwfw_info()
Purpose:  gather hardware version/date, assembly date and firmware date
          into an array and send through serial link
**************************************************************************/
void send_hwfw_info(void)
{
    uint8_t ret[30];
                                    
    ret[0] = hw_version[0];
    ret[1] = hw_version[1];
    
    ret[2] = hw_date[0];
    ret[3] = hw_date[1];
    ret[4] = hw_date[2];
    ret[5] = hw_date[3];
    ret[6] = hw_date[4];
    ret[7] = hw_date[5];
    ret[8] = hw_date[6];
    ret[9] = hw_date[7];

    ret[10] = assembly_date[0];
    ret[11] = assembly_date[1];
    ret[12] = assembly_date[2];
    ret[13] = assembly_date[3];
    ret[14] = assembly_date[4];
    ret[15] = assembly_date[5];
    ret[16] = assembly_date[6];
    ret[17] = assembly_date[7];
    
    ret[18] = (FW_DATE >> 56) & 0xFF;
    ret[19] = (FW_DATE >> 48) & 0xFF;
    ret[20] = (FW_DATE >> 40) & 0xFF;
    ret[21] = (FW_DATE >> 32) & 0xFF;
    ret[22] = (FW_DATE >> 24) & 0xFF;
    ret[23] = (FW_DATE >> 16) & 0xFF;
    ret[24] = (FW_DATE >> 8) & 0xFF;
    ret[25] = FW_DATE & 0xFF;

    ret[26] = (FW_VERSION >> 24) & 0xFF;
    ret[27] = (FW_VERSION >> 16) & 0xFF;
    ret[28] = (FW_VERSION >> 8) & 0xFF;
    ret[29] = FW_VERSION & 0xFF;

    send_usb_packet(from_usb, to_usb, response, hwfw_info, ret, 30);
}

/*************************************************************************
Function: scanner_reset()
Purpose:  puts the scanner in an infinite loop and forces watchdog-reset
**************************************************************************/
int cmd_reset(void)
{
    send_usb_packet(from_usb, to_usb, reset, reset_in_progress, ack, 1); // RX LED is on by now and this function lights up TX LED
    digitalWrite(ACT_LED, LOW); // blink all LEDs including this, good way to test if LEDs are working or not
    while (true); // enter into an infinite loop; watchdog timer doesn't get reset this way so it restarts the program eventually
}

/*************************************************************************
Function: cmd_handshake()
Purpose:  sends handshake message to laptop
**************************************************************************/
int cmd_handshake(void)
{
    uint8_t handshake_array[21] = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 }; // CHRYSLERCCDSCISCANNER
    send_usb_packet(from_usb, to_usb, handshake, ok, handshake_array, 21);
    return 0;
}

/*************************************************************************
Function: cmd_status()
Purpose:  sends status message to laptop
**************************************************************************/
int cmd_status(void)
{
    uint8_t scanner_status[53];
    uint16_t free_ram_available = 0;

    scanner_status[0] = avr_signature[0];
    scanner_status[1] = avr_signature[1];
    scanner_status[2] = avr_signature[2];

    if (eep_present) scanner_status[3] = 0x01;
    else scanner_status[3] = 0x00;
    scanner_status[4] = eep_checksum[0];
    scanner_status[5] = eep_calculated_checksum;

    update_timestamp(current_timestamp);
    scanner_status[6] = current_timestamp[0];
    scanner_status[7] = current_timestamp[1];
    scanner_status[8] = current_timestamp[2];
    scanner_status[9] = current_timestamp[3];

    free_ram_available = free_ram();
    scanner_status[10] = (free_ram_available >> 8) & 0xFF;
    scanner_status[11] = free_ram_available & 0xFF;

    check_battery_volts();
    if (connected_to_vehicle) scanner_status[12] = 0x01;
    else scanner_status[12] = 0x00;
    scanner_status[13] = battery_volts_array[0];
    scanner_status[14] = battery_volts_array[1];
    
    scanner_status[15] = (ccd.bus_settings);
    scanner_status[16] = (ccd.msg_rx_count >> 24) & 0xFF;
    scanner_status[17] = (ccd.msg_rx_count >> 16) & 0xFF;
    scanner_status[18] = (ccd.msg_rx_count >> 8) & 0xFF;
    scanner_status[19] = ccd.msg_rx_count & 0xFF;
    scanner_status[20] = (ccd.msg_tx_count >> 24) & 0xFF;
    scanner_status[21] = (ccd.msg_tx_count >> 16) & 0xFF;
    scanner_status[22] = (ccd.msg_tx_count >> 8) & 0xFF;
    scanner_status[23] = ccd.msg_tx_count & 0xFF;

    scanner_status[24] = (pcm.bus_settings);
    scanner_status[25] = (pcm.msg_rx_count >> 24) & 0xFF;
    scanner_status[26] = (pcm.msg_rx_count >> 16) & 0xFF;
    scanner_status[27] = (pcm.msg_rx_count >> 8) & 0xFF;
    scanner_status[28] = pcm.msg_rx_count & 0xFF;
    scanner_status[29] = (pcm.msg_tx_count >> 24) & 0xFF;
    scanner_status[30] = (pcm.msg_tx_count >> 16) & 0xFF;
    scanner_status[31] = (pcm.msg_tx_count >> 8) & 0xFF;
    scanner_status[32] = pcm.msg_tx_count & 0xFF;

    scanner_status[33] = (tcm.bus_settings);
    scanner_status[34] = (tcm.msg_rx_count >> 24) & 0xFF;
    scanner_status[35] = (tcm.msg_rx_count >> 16) & 0xFF;
    scanner_status[36] = (tcm.msg_rx_count >> 8) & 0xFF;
    scanner_status[37] = tcm.msg_rx_count & 0xFF;
    scanner_status[38] = (tcm.msg_tx_count >> 24) & 0xFF;
    scanner_status[39] = (tcm.msg_tx_count >> 16) & 0xFF;
    scanner_status[40] = (tcm.msg_tx_count >> 8) & 0xFF;
    scanner_status[41] = tcm.msg_tx_count & 0xFF;

    if (lcd_enabled) scanner_status[42] = 0x01;
    else scanner_status[42] = 0x00;

    scanner_status[43] = lcd_i2c_address;
    scanner_status[44] = lcd_char_width;
    scanner_status[45] = lcd_char_height;
    scanner_status[46] = lcd_refresh_rate;
    scanner_status[47] = lcd_units;
    scanner_status[48] = lcd_data_source;

    scanner_status[49] = (heartbeat_interval >> 8) & 0xFF;
    scanner_status[50] = heartbeat_interval & 0xFF;
    scanner_status[51] = (led_blink_duration >> 8) & 0xFF;
    scanner_status[52] = led_blink_duration & 0xFF;

    send_usb_packet(from_usb, to_usb, status, ok, scanner_status, 53);
    return 0;
}

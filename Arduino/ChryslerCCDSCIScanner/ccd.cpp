#include "ccd.h"
#include "common.h"
#include "tools.h"
#include "battery.h"
#include "lcd.h"
#include "usb.h"
#include <CCDLibrary.h> // https://github.com/laszlodaniel/CCDLibrary

uint32_t ccd_positive_adc = 0;   // raw analog reading is stored here
uint16_t ccd_positive_volts = 0; // converted to CCD+ voltage and multiplied by 100: 2.51V -> 251
uint32_t ccd_negative_adc = 0;   // raw analog reading is stored here
uint16_t ccd_negative_volts = 0; // converted to CCD- voltage and multiplied by 100: 2.51V -> 251
uint8_t ccd_volts_array[4]; // ccd_positive_volts and ccd_negative_volts is separated to byte components here

/*************************************************************************
Function: ccd_init()
Purpose:  initialize UART1 and set baudrate to conform CCD-bus requirements,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void ccd_init()
{
    if ((((hw_version[0] << 8) & 0xFF) + (hw_version[1])) >= 144) // hardware version V1.44 and up
    {
        CCD.begin(CCD_DEFAULT_SPEED, CUSTOM_TRANSCEIVER, IDLE_BITS_10, ENABLE_RX_CHECKSUM, ENABLE_TX_CHECKSUM);
    }
    else // hardware version below 1.44
    {
        CCD.begin(); // CDP68HC68S1
    }
}

/*************************************************************************
Function: check_ccd_volts()
Purpose:  measure CCD-bus pin voltages on OBD3 and OBD11 pins
**************************************************************************/
void check_ccd_volts(void)
{
    ccd_positive_adc = 0;
    ccd_negative_adc = 0;
    
    for (uint16_t i = 0; i < 200; i++) // get 200 samples in quick succession
    {
        ccd_positive_adc += analogRead(CCD_POSITIVE);
    }

    for (uint16_t i = 0; i < 200; i++) // get 200 samples in quick succession
    {
        ccd_negative_adc += analogRead(CCD_NEGATIVE);
    }
    
    ccd_positive_adc /= 200; // divide the sum by 100 to get average value
    ccd_negative_adc /= 200; // divide the sum by 100 to get average value
    ccd_positive_volts = (uint16_t)(ccd_positive_adc*(adc_supply_voltage/100.0)/adc_max_value*100.0);
    ccd_negative_volts = (uint16_t)(ccd_negative_adc*(adc_supply_voltage/100.0)/adc_max_value*100.0);
    
    ccd_volts_array[0] = (ccd_positive_volts >> 8) & 0xFF;
    ccd_volts_array[1] = ccd_positive_volts & 0xFF;
    ccd_volts_array[2] = (ccd_negative_volts >> 8) & 0xFF;
    ccd_volts_array[3] = ccd_negative_volts & 0xFF;
}

/*************************************************************************
Function: handle_ccd_data()
Purpose:  handle CCD-bus messages
**************************************************************************/
void handle_ccd_data(void)
{
    if (ccd.enabled)
    {
        if (CCD.available())
        {
            ccd.last_message_length = CCD.read(ccd.last_message);

            if (ccd.last_message_length > 0) // valid message length is always greater than 0
            {
                uint8_t usb_msg[TIMESTAMP_LENGTH + ccd.last_message_length]; // create local array which will hold the timestamp and the CCD-bus message
                update_timestamp(current_timestamp); // get current time for the timestamp
                
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < ccd.last_message_length; i++) // put every byte in the CCD-bus message after the timestamp
                {
                    usb_msg[TIMESTAMP_LENGTH + i] = ccd.last_message[i]; // new message bytes may arrive in the circular buffer but this way only one message is removed
                }
                
                // TODO: check here if echo is expected from a pending message, otherwise good to know if a custom message is heard by the other modules
                send_usb_packet(from_ccd, to_usb, msg_rx, single_msg, usb_msg, TIMESTAMP_LENGTH + ccd.last_message_length); // send CCD-bus message back to the laptop
                
                handle_lcd(from_ccd, usb_msg, 4, TIMESTAMP_LENGTH + ccd.last_message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)

                if (ccd.repeat && !ccd.repeat_iterate)
                {
                    if (ccd.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        bool match = true;
                        for (uint8_t i = 0; i < ccd.last_message_length; i++)
                        {
                            if (ccd.msg_buffer[i] != usb_msg[TIMESTAMP_LENGTH + i]) match = false; // compare received bytes with message sent
                        }
                        if (match) ccd.repeat_next = true; // if echo is correct prepare next message
                    }
                    else if (ccd.msg_to_transmit_count > 1) // multiple messages
                    {
                        bool match = true;
                        for (uint8_t i = 0; i < ccd.last_message_length; i++)
                        {
                            if (ccd.msg_buffer[ccd.msg_buffer_ptr + 1 + i] != usb_msg[TIMESTAMP_LENGTH + i]) match = false; // compare received bytes with message sent
                        }
                        if (match)
                        {
                            ccd.repeat_next = true; // if echo is correct prepare next message

                            // Increase the current message counter and set the buffer pointer to the next message length
                            ccd.msg_to_transmit_count_ptr++;
                            ccd.msg_buffer_ptr += ccd.repeated_msg_length + 1;
                            ccd.repeated_msg_length = ccd.msg_buffer[ccd.msg_buffer_ptr]; // re-calculate new message length
        
                            // After the last message reset everything to zero to start at the beginning
                            if (ccd.msg_to_transmit_count_ptr == ccd.msg_to_transmit_count)
                            {
                                ccd.msg_to_transmit_count_ptr = 0;
                                ccd.msg_buffer_ptr = 0;
                                ccd.repeated_msg_length = ccd.msg_buffer[ccd.msg_buffer_ptr]; // re-calculate new message length

                                if (ccd.repeat_list_once) ccd.repeat_stop = true;
                            }
                        }
                    }
                    
                    if (ccd.repeat_stop) // one-shot message list is terminated here
                    {
                        ccd.msg_buffer_ptr = 0;
                        ccd.repeat = false;
                        ccd.repeat_next = false;
                        ccd.repeat_iterate = false;
                        ccd.repeat_list_once = false;
                    }
                }
                else if (ccd.repeat && ccd.repeat_iterate)
                {
                    if (usb_msg[4] == 0xF2) ccd.repeat_next = true; // response received, prepare next request
                    //if (usb_msg[4] == 0xB2) ccd.repeat_next = true; // DEBUG
                    
                    if (ccd.repeat_stop) // don't request more data if the list ends
                    {
                        ccd.msg_buffer_ptr = 0;
                        ccd.repeat = false;
                        ccd.repeat_next = false;
                        ccd.repeat_iterate = false;
                        ccd.repeat_list_once = false;

                        uint8_t ret[2] = { 0x00, to_ccd }; // improvised payload array with only 1 element which is the target bus
                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                    }
                }
                
                ccd.last_message_length = 0; // force ISR to update this value again so we don't end up here in the next program loop
                ccd.message_count = 0;
                ccd.msg_rx_count++;
            }
        }

        if (ccd.random_msg && (ccd.random_msg_interval > 0))
        {
            current_millis = millis(); // check current time
            if ((current_millis - ccd.random_msg_last_millis) > ccd.random_msg_interval)
            {
                ccd.random_msg_last_millis = current_millis;
                ccd.msg_buffer_ptr = random(3, 7); // random message length between 3 and 6 bytes
                for (uint8_t i = 0; i < ccd.msg_buffer_ptr - 2; i++)
                {
                    ccd.msg_buffer[i] = random(256); // generate random bytes
                }
                uint8_t checksum_position = ccd.msg_buffer_ptr - 1;
                ccd.msg_buffer[checksum_position] = calculate_checksum(ccd.msg_buffer, 0, checksum_position);
                ccd.msg_tx_pending = true;
                ccd.random_msg_interval = random(ccd.random_msg_interval_min, ccd.random_msg_interval_max); // generate new delay value between random messages
            }
        }

        // Repeated messages are prepared here
        if (ccd.repeat)
        {
            current_millis = millis(); // check current time
            if ((current_millis - ccd.repeated_msg_last_millis) > ccd.repeated_msg_interval) // wait between messages
            {
                ccd.repeated_msg_last_millis = current_millis;
                if (ccd.repeat_next && !ccd.repeat_iterate) // no iteration, same message over and over again
                {
                    // The message is already in the ccd.msg_buffer array, just set flags
                    ccd.msg_tx_pending = true; // set flag
                    ccd.repeat_next = false;
                }
                else if (ccd.repeat_next && ccd.repeat_iterate) // iteration, message is incremented for every repeat according to settings
                {
                    if (ccd.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                    {
                        ccd.msg_buffer_ptr = 6;
                        ccd.msg_buffer[0] = 0xB2; // this byte is fixed and not included in the "raw" variable
                        ccd.msg_buffer[1] = (ccd.repeated_msg_raw_start >> 24) & 0xFF; // decompose raw message from its integer form to byte components
                        ccd.msg_buffer[2] = (ccd.repeated_msg_raw_start >> 16) & 0xFF;
                        ccd.msg_buffer[3] = (ccd.repeated_msg_raw_start >> 8) & 0xFF;
                        ccd.msg_buffer[4] = ccd.repeated_msg_raw_start & 0xFF;
                        ccd.msg_buffer[5] = calculate_checksum(ccd.msg_buffer, 0, ccd.msg_buffer_ptr - 1); // last checksum byte automatically calculated
                    }
                    else // increment existing message
                    {
                        // First combine bytes into a single integer
                        ccd.msg_buffer_ptr = 6;
                        uint32_t message = to_uint32(ccd.msg_buffer[1], ccd.msg_buffer[2], ccd.msg_buffer[3], ccd.msg_buffer[4]);
                        message += ccd.repeated_msg_increment; // add increment
                        ccd.msg_buffer[0] = 0xB2; // this byte is fixed and not included in the "raw" variable
                        ccd.msg_buffer[1] = (message >> 24) & 0xFF; // decompose raw message from its integer form to byte components
                        ccd.msg_buffer[2] = (message >> 16) & 0xFF;
                        ccd.msg_buffer[3] = (message >> 8) & 0xFF;
                        ccd.msg_buffer[4] = message & 0xFF;
                        ccd.msg_buffer[5] = calculate_checksum(ccd.msg_buffer, 0, ccd.msg_buffer_ptr - 1); // last checksum byte automatically calculated
                        
                        if ((message + ccd.repeated_msg_increment) > ccd.repeated_msg_raw_end) ccd.repeat_stop = true; // don't prepare another message, it's the end
                    }

                    ccd.msg_tx_pending = true; // set flag
                    ccd.repeat_next = false;
                }
            }
        }

        if (ccd.msg_tx_pending) // received over usb connection, checksum corrected there (if wrong)
        {     
            if (ccd.msg_to_transmit_count == 1) // if there's only one message in the buffer
            {
                CCD.write(ccd.msg_buffer, ccd.msg_buffer_ptr); // send message on the CCD-bus
            }
            else if (ccd.msg_to_transmit_count > 1) // multiple messages, send one at a time
            {
                // Make a local copy of the current message.
                uint8_t current_message[ccd.repeated_msg_length];
                uint8_t j = 0;
                for (uint8_t i = (ccd.msg_buffer_ptr + 1); i < (ccd.msg_buffer_ptr + 1 + ccd.repeated_msg_length); i++)
                {
                    current_message[j] = ccd.msg_buffer[i];
                    j++;
                }

                CCD.write(current_message, ccd.repeated_msg_length); // send message on the CCD-bus
            }
            
            ccd.msg_tx_pending = false; // re-arm, make it possible to send a message again, TODO: same as above
            ccd.msg_tx_count++;
        }
    }
}

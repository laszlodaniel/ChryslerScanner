#ifndef COMMON_H
#define COMMON_H

#include <Arduino.h>

// Firmware version (hexadecimal format):
// 00: major
// 01: minor
// 00: patch
// (00: revision)
// = v0.1.0(.0)
#define FW_VERSION 0x00050000

// Firmware date/time of compilation in 32-bit UNIX time:
// https://www.epochconverter.com/hex
// Upper 32 bits contain the firmware version.
#define FW_DATE 0x000500005FE4EBFE

// Set (1), clear (0) and invert (1->0; 0->1) bit in a register or variable easily
#define sbi(variable, bit) (variable) |=  (1 << (bit))
#define cbi(variable, bit) (variable) &= ~(1 << (bit))
#define ibi(variable, bit) (variable) ^=  (1 << (bit))

#define to_uint16(hb, lb)               (uint16_t)(((uint8_t)hb << 8) | (uint8_t)lb)
#define to_uint32(msb, hb, lb, lsb)     (uint32_t)(((uint32_t)msb << 24) | ((uint32_t)hb << 16) | ((uint32_t)lb << 8) | (uint32_t)lsb)

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define ELBAUD  1023 // prescaler for  976.5 baud speed (SCI-bus extra-low speed)
#define LOBAUD  127  // prescaler for 7812.5 baud speed (CCD/SCI-bus default low-speed diagnostic mode)
#define HIBAUD  15   // prescaler for  62500 baud speed (SCI-bus high-speed parameter mode)
#define EHBAUD  7    // prescaler for 125000 baud speed (SCI-bus extra-high speed)
#define USBBAUD 3    // prescaler for 250000 baud speed (USB communication)

#define PACKET_SYNC_BYTE    0x3D // "=" symbol
#define ASCII_SYNC_BYTE     0x3E // ">" symbol
#define MAX_PAYLOAD_LENGTH  USB_RX0_BUFFER_SIZE - 6  // 1024-6 bytes
#define EMPTY_PAYLOAD       0xFE  // Random byte, could be anything
#define TIMESTAMP_LENGTH    0x04

#define BUFFER_SIZE 256

// DATA CODE byte building blocks
// Source and Target masks (high nibble (2+2 bits))
#define from_usb            0x00 // when sending packets back to laptop use "from_" masks to specify source
#define from_ccd            0x01
#define from_pcm            0x02
#define from_tcm            0x03
#define to_usb              0x00 // when receiving packets from laptop use "to_" masks as target, "to_usb" meaning the scanner itself
#define to_ccd              0x01
#define to_pcm              0x02
#define to_tcm              0x03
// Commands (low nibble (4 bits))
#define reset               0x00
#define handshake           0x01
#define status              0x02
#define settings            0x03
#define request             0x04
#define response            0x05
#define msg_tx              0x06
#define msg_rx              0x07
// 0x08-0x0D reserved
#define debug               0x0E
#define ok_error            0x0F

// SUB-DATA CODE byte
// Command 0x00 (reset)
#define reset_in_progress   0x00
#define reset_done          0x01
// 0x02-0xFF reserved

// SUB-DATA CODE byte
// Command 0x01 (handshake)
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x02 (status)
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x03 (settings)
#define heartbeat           0x01 // ACT_LED flashing interval is stored in payload
#define set_ccd_bus         0x02 // ON-OFF state is stored in payload
#define set_sci_bus         0x03 // ON-OFF state, A/B configuration and speed are stored in payload
#define set_repeat_behavior 0x04 // Repeated message behavior settings
#define set_lcd             0x05 // ON-OFF state
// 0x06-0xFF reserved 

// SUB-DATA CODE byte
// Command 0x04 & 0x05 (request and response)
#define hwfw_info           0x01 // Hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
#define timestamp           0x02 // elapsed milliseconds since system start
#define battery_voltage     0x03 // flag for battery voltage packet
#define exteeprom_checksum  0x04
#define ccd_bus_voltages    0x05
// 0x06-0xFF reserved

// SUB-DATA CODE byte
// Command 0x06 (msg_tx)
#define stop_msg_flow       0x01 // stop message transmission (single and repeated as well)         
#define single_msg          0x02 // send message to the target bus specified in DATA CODE byte; message is stored in payload 
#define list_msg            0x03 // send a set of messages to the target bus specified in DATA CODE byte; messages are stored in payload
#define repeated_single_msg 0x04 // send message(s) repeatedly to the target bus sepcified in DATA CODE byte
#define repeated_list_msg   0x05 // send a fixed set of messages repeatedly to the target bus specified in DATA CODE byte
// 0x06-0xFF reserved

// SUB-DATA CODE byte
// Command 0x07 (msg_rx)
#define sci_ls_bytes        0x01 // low-speed SCI-bus message
#define sci_hs_bytes        0x02 // high-speed SCI-bus message
// 0x03-0xFF reserved

// SUB-DATA CODE byte
// Command 0x0E (debug)
#define random_ccd_msg            0x01
#define read_inteeprom_byte       0x02
#define read_inteeprom_block      0x03
#define read_exteeprom_byte       0x04
#define read_exteeprom_block      0x05
#define write_inteeprom_byte      0x06
#define write_inteeprom_block     0x07
#define write_exteeprom_byte      0x08
#define write_exteeprom_block     0x09
// 0x0A-0xFF reserved

// SUB-DATA CODE byte
// Command 0x0F (ok_error)
#define ok                                      0x00
#define error_length_invalid_value              0x01
#define error_datacode_invalid_command          0x02
#define error_subdatacode_invalid_value         0x03
#define error_payload_invalid_values            0x04
#define error_packet_checksum_invalid_value     0x05
#define error_packet_timeout_occurred           0x06
#define error_buffer_overflow                   0x07
// 0x08-0xF6 reserved
#define error_not_enough_mcu_ram                0xF7
#define error_sci_hs_memory_ptr_no_response     0xF8
#define error_sci_hs_invalid_memory_ptr         0xF9
#define error_sci_hs_no_response                0xFA
#define error_eep_not_found                     0xFB
#define error_eep_read                          0xFC
#define error_eep_write                         0xFD
#define error_internal                          0xFE
#define error_fatal                             0xFF

extern uint8_t ack[1];
extern uint8_t err[1];

extern uint8_t hw_version[2];
extern uint8_t hw_date[8];
extern uint8_t assembly_date[8];

extern uint32_t current_millis;
extern uint8_t current_timestamp[4]; // current time is stored here when "update_timestamp" is called

typedef struct {
    bool enabled = false; // bus state (enabled or disabled)
    bool termination_bias_enabled = false;
    bool invert_logic = false; // OBD1 SCI engine adapter cable needs special message handling
    uint8_t bus_settings = 0;
    uint16_t speed = LOBAUD; // baudrate prescaler - 1023: 976.5625 baud; 127: 7812.5 baud; 15: 62500 baud; 7: 125000 baud; 3: 250000 baud
    volatile bool idle = true; // bus idling (CCD-bus only, IDLE-pin)
    volatile bool busy = false; // there's a byte being transmitted on the bus (CCD-bus only, CTRL-pin)
    uint8_t last_message[16];
    uint8_t last_message_length = 0;
    volatile uint8_t message_length = 0; // current message length
    volatile uint8_t message_count = 0; // number of messages in the buffer
    uint8_t msg_buffer[BUFFER_SIZE]; // temporary buffer to store outgoing or current repeated messages
    uint8_t msg_buffer_ptr = 0; // message length in the buffer, this points to the first empty slot after the last byte
    uint8_t msg_to_transmit_count = 0;
    uint8_t msg_to_transmit_count_ptr = 0;
    volatile uint32_t last_byte_millis = 0;
    bool msg_tx_pending = false; // message is awaiting to be transmitted to the bus
    bool actuator_test_running = false; // actuator test (SCI-bus only)
    uint8_t actuator_test_byte = 0; // actuator test byte (SCI-bus only)
    bool ls_request_running = false; // low-speed request (SCI-bus only)
    uint8_t ls_request_byte = 0;
    bool repeat = false;
    bool repeat_next = false;
    bool repeat_iterate = false;
    bool repeat_iterate_continue = false;
    bool repeat_list_once = false;
    bool repeat_stop = true;
    uint8_t repeated_msg_length = 0;
    uint32_t repeated_msg_raw_start = 0; // iteration start, 4-bytes max
    uint32_t repeated_msg_raw_end = 0; // iteration end, 4-bytes max
    uint16_t repeated_msg_increment = 1; // if iteration is true then the counter in the message will increase this much for every new message
    uint16_t repeated_msg_interval = 100; // ms
    uint32_t repeated_msg_last_millis = 0;
    uint8_t repeat_retry_counter = 0;
    bool random_msg = false;
    uint16_t random_msg_interval = 0;
    uint16_t random_msg_interval_min = 0;
    uint16_t random_msg_interval_max = 0;
    uint32_t random_msg_last_millis = 0;
    volatile uint32_t msg_rx_count = 0; // total received messages
    volatile uint32_t msg_tx_count = 0; // total transmitted messages
} bus;

extern bus ccd, pcm, tcm;

#endif // COMMON_H

/*
 * ChryslerCCDSCIScanner (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
 * Copyright (C) 2018-2023 Daniel Laszlo
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
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
 */

#include <avr/boot.h>
#include <avr/eeprom.h>
#include <avr/interrupt.h>
#include <avr/io.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <util/atomic.h>
#include <EEPROM.h>
#include <LiquidCrystal_I2C.h> // https://github.com/fdebrabander/Arduino-LiquidCrystal-I2C-library
#include <Wire.h>
#include <extEEPROM.h> // https://github.com/JChristensen/extEEPROM
#include <CCDLibrary.h> // https://github.com/laszlodaniel/CCDLibrary

// Firmware version (hexadecimal format):
// 00: major
// 09: minor
// 0C: patch
// (00: revision)
// = v0.9.12(.0)
#define FW_VERSION 0x00090C00

// Firmware date/time of compilation in 32-bit UNIX time:
// https://www.epochconverter.com/hex
// Upper 32 bits contain the firmware version.
#define FW_DATE 0x00090C006697BBA8

// Set (1), clear (0) and invert (1->0; 0->1) bit in a register or variable easily
//#define sbi(variable, bit) (variable) |=  (1 << (bit))
//#define cbi(variable, bit) (variable) &= ~(1 << (bit))
//#define ibi(variable, bit) (variable) ^=  (1 << (bit))

#define to_uint16(hb, lb)           (uint16_t)(((uint8_t)hb << 8) | (uint8_t)lb)
#define to_uint32(msb, hb, lb, lsb) (uint32_t)(((uint32_t)msb << 24) | ((uint32_t)hb << 16) | ((uint32_t)lb << 8) | (uint32_t)lsb)
#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

#define USB_RX_BUFFER_SIZE 1024
#define USB_TX_BUFFER_SIZE 1024
#define USB_RX_BUFFER_MASK (USB_RX_BUFFER_SIZE - 1)
#define USB_TX_BUFFER_MASK (USB_TX_BUFFER_SIZE - 1)

#define USB_RECEIVE_INTERRUPT  USART0_RX_vect
#define USB_TRANSMIT_INTERRUPT USART0_UDRE_vect
#define USB_STATUS             UCSR0A
#define USB_CONTROL            UCSR0B
#define USB_DATA               UDR0
#define USB_UDRIE              UDRIE0

#define PCM_RX_BUFFER_SIZE 256
#define PCM_TX_BUFFER_SIZE 256
#define PCM_RX_BUFFER_MASK (PCM_RX_BUFFER_SIZE - 1)
#define PCM_TX_BUFFER_MASK (PCM_TX_BUFFER_SIZE - 1)

#define PCM_RECEIVE_INTERRUPT  USART2_RX_vect
#define PCM_TRANSMIT_INTERRUPT USART2_UDRE_vect
#define PCM_STATUS             UCSR2A
#define PCM_CONTROL            UCSR2B
#define PCM_DATA               UDR2
#define PCM_UDRIE              UDRIE2  

#define TCM_RX_BUFFER_SIZE 256
#define TCM_TX_BUFFER_SIZE 256
#define TCM_RX_BUFFER_MASK (TCM_RX_BUFFER_SIZE - 1)
#define TCM_TX_BUFFER_MASK (TCM_TX_BUFFER_SIZE - 1)

#define TCM_RECEIVE_INTERRUPT  USART3_RX_vect
#define TCM_TRANSMIT_INTERRUPT USART3_UDRE_vect
#define TCM_STATUS             UCSR3A
#define TCM_CONTROL            UCSR3B
#define TCM_DATA               UDR3
#define TCM_UDRIE              UDRIE3

#define BATT         A0 // battery voltage sensor analog input
#define CCD_POSITIVE A1 // CCD+ analog input
#define CCD_NEGATIVE A2 // CCD- analog input
#define TBEN         13 // CCD-bus termination and bias enable pin

#define A_PCM_RX_EN 22 // SCI-bus configuration selector digital pins on ATmega2560
#define A_PCM_TX_EN 23
#define A_TCM_RX_EN 24
#define A_TCM_TX_EN 25
#define B_PCM_RX_EN 26
#define B_PCM_TX_EN 27
#define B_TCM_RX_EN 28
#define B_TCM_TX_EN 29

#define RX_LED  35 // status LED (red), message received
#define TX_LED  36 // status LED (orange), message sent
#define ACT_LED 37 // status LED (blue), activity

// SAE J2610 (SCI-bus) recommended delays in milliseconds
#define SCI_LS_T1_DELAY 20
#define SCI_LS_T2_DELAY 100
#define SCI_LS_T3_DELAY 50
#define SCI_LS_T4_DELAY 20
#define SCI_LS_T5_DELAY 100
#define SCI_HS_T1_DELAY 1
#define SCI_HS_T2_DELAY 1
#define SCI_HS_T3_DELAY 5
#define SCI_HS_T4_DELAY 0
#define SCI_HS_T5_DELAY 0
#define SCI_BTSTRP_DLY  20

// SCI-bus setting bits
#define SCI_STATE_BIT         7
#define SCI_SBEC2_BIT         6
#define SCI_MODULE_BIT        5
#define SCI_NGC_BIT           4
#define SCI_LOGIC_BIT         3
#define SCI_CONFIG_BIT        2
#define SCI_SPEED_BITS        3
#define SCI_SPEED_BAUD_976    0
#define SCI_SPEED_BAUD_7812   1
#define SCI_SPEED_BAUD_62500  2
#define SCI_SPEED_BAUD_125000 3

// CCD-bus setting bits
#define CCD_STATE_BIT         7
#define CCD_TBEN_BIT          6

//#define UART_FRAME_ERROR        0x1000  /**< Framing Error by UART          00010000 00000000 - FIXED BIT */ 
//#define UART_OVERRUN_ERROR      0x0800  /**< Overrun condition by UART      00001000 00000000 - FIXED BIT */
//#define UART_BUFFER_OVERFLOW    0x0400  /**< Receive ringbuffer overflow    00000100 00000000 - ARBITRARY */
#define UART_RX_NO_DATA         0x0200  /**< Receive buffer is empty        00000010 00000000 - ARBITRARY */

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define BAUD_976    1023 // prescaler for  976.5 baud speed (SCI-bus extra-low speed)
#define BAUD_7812   127  // prescaler for 7812.5 baud speed (CCD/SCI-bus default low-speed diagnostic mode)
#define BAUD_62500  15   // prescaler for  62500 baud speed (SCI-bus high-speed parameter mode)
#define BAUD_125000 7    // prescaler for 125000 baud speed (SCI-bus extra-high speed)
#define BAUD_250000 3    // prescaler for 250000 baud speed (USB communication)

#define PACKET_SYNC_BYTE   0x3D // "=" symbol
#define ASCII_SYNC_BYTE    0x3E // ">" symbol
#define MAX_PAYLOAD_LENGTH USB_RX_BUFFER_SIZE - 6  // 1024-6 bytes
#define EMPTY_PAYLOAD      0xFE  // Random byte, could be anything

#define BUFFER_SIZE 256

// DATA CODE byte building blocks
// Bus masks (high nibble (3 bits))
#define bus_usb             0x00 // when sending packets back to laptop use "from_" masks to specify source
#define bus_ccd             0x01
#define bus_pcm             0x02
#define bus_tcm             0x03
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
#define set_arbitrary_uart_speed  0x0A
#define init_bootstrap_mode       0x0B
#define upload_worker_function    0x0C
#define start_worker_function     0x0D
#define exit_worker_function      0x0E
#define restore_pcm_eeprom        0xF0

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
#define error_datacode_invalid_bus              0x08
// 0x09-0xF6 reserved
#define error_sci_ls_no_response                0xF6
#define error_not_enough_mcu_ram                0xF7
#define error_sci_hs_memory_ptr_no_response     0xF8
#define error_sci_hs_invalid_memory_ptr         0xF9
#define error_sci_hs_no_response                0xFA
#define error_eep_not_found                     0xFB
#define error_eep_read                          0xFC
#define error_eep_write                         0xFD
#define error_internal                          0xFE
#define error_fatal                             0xFF

#define STOP  0x00
#define START 0x01

uint8_t ack[1] = { 0x00 }; // acknowledge payload array
uint8_t err[1] = { 0xFF }; // error payload array

uint8_t avr_signature[3];
uint8_t hw_version[2];
uint8_t hw_date[8];
uint8_t assembly_date[8];

uint32_t current_millis = 0;
uint8_t current_timestamp[4]; // current time is stored here when "update_timestamp" is called

uint8_t command_timeout = 100; // milliseconds
uint16_t command_purge_timeout = 200; // milliseconds, if a command isn't complete within this time then delete the usb receive buffer

typedef struct {
    bool enabled = false; // bus state (enabled or disabled)
    bool termination_bias_enabled = false;
    bool inverted_logic = false; // OBD1 SCI engine adapter cable needs special message handling
    bool sbec2_mode = false; // OBD1 reverse bit order inside byte
    bool ngc_mode = false; // NGC computers use full-duplex SCI-bus communication
    uint8_t bus_settings = 0;
    uint16_t speed = BAUD_7812; // baudrate prescaler - 1023: 976.5625 baud; 127: 7812.5 baud; 15: 62500 baud; 7: 125000 baud; 3: 250000 baud
    volatile bool idle = true; // bus idling (CCD-bus only, IDLE-pin)
    volatile bool busy = false; // there's a byte being transmitted on the bus (CCD-bus only, CTRL-pin)
    bool echo_received = false; // low-speed mode (7812.5 baud)
    bool response_received = false; // low-speed mode (7812.5 baud) typical 1-byte response flag
    uint8_t message[32];
    uint16_t message_length = 0;
    uint8_t msg_buffer[BUFFER_SIZE]; // temporary buffer to store outgoing or current repeated messages
    uint16_t msg_buffer_ptr = 0; // message length in the buffer, this points to the first empty slot after the last byte
    uint8_t msg_to_transmit_count = 0;
    uint8_t msg_to_transmit_count_ptr = 0;
    volatile uint32_t last_byte_millis = 0;
    bool msg_tx_pending = false; // message is awaiting to be transmitted to the bus
    bool repeat = false;
    bool repeat_next = false;
    bool repeat_list_once = false;
    bool repeat_stop = true;
    uint16_t repeated_msg_length = 0;
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
    uint8_t last_hs_memarea = 0xF4;
    uint8_t auto_request = 0;
} bus_t;

bus_t ccd, pcm, tcm;

// Construct an object called "eep" for the external 24LC32A EEPROM chip.
extEEPROM eep(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default

bool eep_present = false;
uint8_t eep_status = 0; // extEEPROM connection status is stored here
uint8_t eep_result = 0; // extEEPROM 

// Construct an object called "lcd" for the external display.
LiquidCrystal_I2C lcd(0x27, 20, 4);

bool lcd_enabled = false;
uint8_t lcd_i2c_address = 0;
uint8_t lcd_char_width = 0;
uint8_t lcd_char_height = 0;
uint8_t lcd_refresh_rate = 0;
uint8_t lcd_units = 0; // 0-imperial, 1-metric
uint8_t lcd_data_source = 1; // 1: CCD-bus, 2: SCI-bus (PCM), 3: SCI-bus (TCM)
uint32_t lcd_last_update = 0;
uint16_t lcd_update_interval = 0;
bool lcd_splash_screen_on = true;

// Custom LCD-characters
// https://maxpromer.github.io/LCD-Character-Creator/
uint8_t up_symbol[8]     = { 0x00, 0x04, 0x0E, 0x15, 0x04, 0x04, 0x00, 0x00 }; // ↑
uint8_t down_symbol[8]   = { 0x00, 0x04, 0x04, 0x15, 0x0E, 0x04, 0x00, 0x00 }; // ↓
uint8_t left_symbol[8]   = { 0x00, 0x04, 0x08, 0x1F, 0x08, 0x04, 0x00, 0x00 }; // ←
uint8_t right_symbol[8]  = { 0x00, 0x04, 0x02, 0x1F, 0x02, 0x04, 0x00, 0x00 }; // →
uint8_t enter_symbol[8]  = { 0x00, 0x01, 0x05, 0x09, 0x1F, 0x08, 0x04, 0x00 }; // 
uint8_t degree_symbol[8] = { 0x06, 0x09, 0x09, 0x06, 0x00, 0x00, 0x00, 0x00 }; // °

char vin_characters[] = "-----------------"; // 17 characters
String vin_string;

volatile uint8_t  USB_RxBuf[USB_RX_BUFFER_SIZE];
volatile uint8_t  USB_TxBuf[USB_TX_BUFFER_SIZE];
volatile uint16_t USB_RxHead; // since buffer size is bigger than 256 bytes it has to be a 16-bit (int) variable
volatile uint16_t USB_RxTail;
volatile uint16_t USB_TxHead;
volatile uint16_t USB_TxTail;
volatile uint8_t  USB_LastRxError;

volatile uint8_t PCM_RxBuf[PCM_RX_BUFFER_SIZE];
volatile uint8_t PCM_TxBuf[PCM_TX_BUFFER_SIZE];
volatile uint8_t PCM_RxHead;
volatile uint8_t PCM_RxTail;
volatile uint8_t PCM_TxHead;
volatile uint8_t PCM_TxTail;
volatile uint8_t PCM_LastRxError;

volatile uint8_t TCM_RxBuf[TCM_RX_BUFFER_SIZE];
volatile uint8_t TCM_TxBuf[TCM_TX_BUFFER_SIZE];
volatile uint8_t TCM_RxHead;
volatile uint8_t TCM_RxTail;
volatile uint8_t TCM_TxHead;
volatile uint8_t TCM_TxTail;
volatile uint8_t TCM_LastRxError;

uint8_t sci_hi_speed_memarea[16] = { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF };
uint8_t sci_lo_speed_cmd_filter[6] = { 0x14, 0x15, 0x26, 0x27, 0x28, 0x2A };

uint32_t ccd_positive_adc = 0;   // raw analog reading is stored here
uint16_t ccd_positive_volts = 0; // converted to CCD+ voltage and multiplied by 100: 2.51V -> 251
uint32_t ccd_negative_adc = 0;   // raw analog reading is stored here
uint16_t ccd_negative_volts = 0; // converted to CCD- voltage and multiplied by 100: 2.51V -> 251
uint8_t ccd_volts_array[4]; // ccd_positive_volts and ccd_negative_volts is separated to byte components here

uint32_t previous_random_ccd_msg_millis = 0;

uint16_t adc_supply_voltage = 500; // supply voltage multiplied by 100: 5.00V -> 500
uint16_t rd_high_value = 27000; // high resistor value in the divider (R19): 27 kOhm = 27000
uint16_t rd_low_value = 5000;  // low resistor value in the divider (R20): 5 kOhm = 5000
uint16_t adc_max_value = 1023; // 1023 for 10-bit resolution
uint32_t battery_adc = 0;   // raw analog reading is stored here
uint16_t battery_volts = 0; // converted to battery voltage and multiplied by 100: 12.85V -> 1285
uint8_t battery_volts_array[2]; // battery_volts is separated to byte components here
bool connected_to_vehicle = false;

uint32_t rx_led_ontime = 0;
uint32_t tx_led_ontime = 0;
uint32_t act_led_ontime = 0;
uint32_t previous_act_blink = 0;
uint16_t led_blink_duration = 50; // milliseconds
uint16_t heartbeat_interval = 5000; // milliseconds
bool heartbeat_enabled = true;

// Bootloaders:

// SBEC3 (128k).
const uint8_t LdBoot_128k_SBEC3[516] PROGMEM =
{
    0x37, 0x80, 0x00, 0xac, 0xfa, 0x00, 0x01, 0x98, 0xf8, 0x10, 0xb7, 0x02, 0xf8, 0x20, 0xb7, 0x58, 
    0xb6, 0xee, 0xf5, 0x11, 0xfa, 0x00, 0x01, 0x82, 0xfa, 0x00, 0x01, 0x98, 0x17, 0xfa, 0x01, 0xb0, 
    0x17, 0xf5, 0x01, 0xb0, 0xfa, 0x00, 0x01, 0x82, 0xfa, 0x00, 0x01, 0x98, 0x17, 0xfa, 0x01, 0xb1, 
    0x17, 0xf5, 0x01, 0xb1, 0xfa, 0x00, 0x01, 0x82, 0xf5, 0x00, 0x37, 0x9c, 0x37, 0xbc, 0x02, 0x00, 
    0xfa, 0x00, 0x01, 0x98, 0xca, 0x00, 0xc5, 0x00, 0xfa, 0x00, 0x01, 0x82, 0x3c, 0x01, 0x27, 0x31, 
    0x01, 0xb0, 0xb6, 0xe8, 0x37, 0xb5, 0x0f, 0xa0, 0xfa, 0x00, 0x01, 0xa8, 0xf5, 0x04, 0x37, 0x9c, 
    0x37, 0xbc, 0x80, 0x00, 0xf5, 0x14, 0xfa, 0x00, 0x01, 0x82, 0xb0, 0x94, 0xf5, 0x21, 0xfa, 0x00, 
    0x01, 0x82, 0xfa, 0x00, 0x02, 0x00, 0xb0, 0x88, 0xfa, 0x00, 0x01, 0x98, 0xfa, 0x00, 0x01, 0x82, 
    0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x37, 0xb5, 
    0x01, 0x90, 0xfa, 0x00, 0x01, 0xa8, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 
    0xb6, 0xf2, 0x17, 0xe5, 0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 0x27, 0xf7, 
    0x27, 0x4c, 0x37, 0x3b, 0x00, 0xe0, 0xf5, 0x0f, 0x37, 0x9e, 0x37, 0xbe, 0x80, 0x00, 0xf5, 0x00, 
    0x37, 0x9f, 0x37, 0xbf, 0x07, 0xf6, 0x37, 0x15, 0x27, 0xfa, 0x37, 0x9d, 0xf5, 0x04, 0x37, 0x9c, 
    0x37, 0xbc, 0x80, 0x00, 0x37, 0xb5, 0x01, 0x48, 0x37, 0xea, 0x7a, 0x00, 0x28, 0x80, 0x7a, 0x21, 
    0x37, 0xb5, 0x00, 0xcf, 0x37, 0xea, 0x7a, 0x44, 0x37, 0xb5, 0x04, 0x04, 0x37, 0xea, 0x7a, 0x48, 
    0x37, 0xea, 0x7a, 0x4c, 0x37, 0xb5, 0x68, 0xf0, 0x37, 0xea, 0x7a, 0x4a, 0x37, 0xb5, 0x70, 0xf0, 
    0x37, 0xea, 0x7a, 0x4e, 0x37, 0xb5, 0xff, 0x88, 0x37, 0xea, 0x7a, 0x54, 0x37, 0xb5, 0x78, 0x30, 
    0x37, 0xea, 0x7a, 0x56, 0x37, 0xb5, 0xf8, 0x81, 0x37, 0xea, 0x08, 0x14, 0x37, 0xe5, 0x08, 0x12, 
    0x37, 0xb7, 0x00, 0x01, 0x37, 0xea, 0x08, 0x12, 0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 0x08, 0x18, 
    0x27, 0x29, 0x08, 0x06, 0xff, 0xff, 0x27, 0x29, 0x08, 0x08, 0x03, 0xff, 0x27, 0xf5, 0x37, 0x35, 
    0x08, 0x24, 0x27, 0xaa, 0x7c, 0x04, 0x37, 0x38, 0x08, 0x38, 0xb3, 0xf2, 0x17, 0x25, 0x79, 0x20, 
    0x27, 0x28, 0x08, 0x60, 0x20, 0x00, 0xfa, 0x00, 0x02, 0x64, 0xf5, 0x22, 0xfa, 0x00, 0x01, 0x82, 
    0x7a, 0x00, 0x01, 0x04, 0x37, 0xb5, 0x40, 0x88, 0x37, 0xea, 0x7c, 0x00, 0x75, 0x06, 0x17, 0x6a, 
    0x7c, 0x04, 0x75, 0xfe, 0x17, 0x6a, 0x7c, 0x05, 0x75, 0x33, 0x17, 0x6a, 0x7c, 0x16, 0x75, 0xf8, 
    0x17, 0x6a, 0x7c, 0x15, 0x75, 0xfe, 0x17, 0x6a, 0x7c, 0x17, 0x37, 0xb5, 0x81, 0x08, 0x37, 0xea, 
    0x7c, 0x18, 0x37, 0xb5, 0x10, 0x00, 0x37, 0xea, 0x7c, 0x1a, 0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 
    0x7c, 0x1c, 0x75, 0x00, 0x17, 0x6a, 0x7c, 0x1e, 0x37, 0x35, 0x42, 0x42, 0x37, 0x6a, 0x7d, 0x41, 
    0x37, 0x35, 0x02, 0x02, 0x37, 0x6a, 0x7d, 0x43, 0x37, 0x35, 0xc2, 0x02, 0x37, 0x6a, 0x7d, 0x45, 
    0x37, 0x35, 0xc2, 0x42, 0x37, 0x6a, 0x7d, 0x48, 0x37, 0xb5, 0x01, 0x00, 0x37, 0xea, 0x7d, 0x24, 
    0x37, 0xb5, 0x02, 0x02, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x02, 0xde, 0x27, 0xf7, 0x28, 0x80, 
    0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 0x37, 0x01, 0xb7, 0x0a, 0x2a, 0x80, 0x7c, 0x1f, 
    0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 0xb0, 0x04, 0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 
    0xf5, 0xa5, 0x27, 0xf7
};

// SBEC3 (256k).
const uint8_t LdBoot_256k_SBEC3[516] PROGMEM =
{
    0x37, 0x80, 0x00, 0xac, 0xfa, 0x00, 0x01, 0x98, 0xf8, 0x10, 0xb7, 0x02, 0xf8, 0x20, 0xb7, 0x58, 
    0xb6, 0xee, 0xf5, 0x11, 0xfa, 0x00, 0x01, 0x82, 0xfa, 0x00, 0x01, 0x98, 0x17, 0xfa, 0x01, 0xb0, 
    0x17, 0xf5, 0x01, 0xb0, 0xfa, 0x00, 0x01, 0x82, 0xfa, 0x00, 0x01, 0x98, 0x17, 0xfa, 0x01, 0xb1, 
    0x17, 0xf5, 0x01, 0xb1, 0xfa, 0x00, 0x01, 0x82, 0xf5, 0x00, 0x37, 0x9c, 0x37, 0xbc, 0x02, 0x00, 
    0xfa, 0x00, 0x01, 0x98, 0xca, 0x00, 0xc5, 0x00, 0xfa, 0x00, 0x01, 0x82, 0x3c, 0x01, 0x27, 0x31, 
    0x01, 0xb0, 0xb6, 0xe8, 0x37, 0xb5, 0x0f, 0xa0, 0xfa, 0x00, 0x01, 0xa8, 0xf5, 0x04, 0x37, 0x9c, 
    0x37, 0xbc, 0x80, 0x00, 0xf5, 0x14, 0xfa, 0x00, 0x01, 0x82, 0xb0, 0x94, 0xf5, 0x21, 0xfa, 0x00, 
    0x01, 0x82, 0xfa, 0x00, 0x02, 0x00, 0xb0, 0x88, 0xfa, 0x00, 0x01, 0x98, 0xfa, 0x00, 0x01, 0x82, 
    0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x37, 0xb5, 
    0x01, 0x90, 0xfa, 0x00, 0x01, 0xa8, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 
    0xb6, 0xf2, 0x17, 0xe5, 0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 0x27, 0xf7, 
    0x27, 0x4c, 0x37, 0x3b, 0x00, 0xe0, 0xf5, 0x0f, 0x37, 0x9e, 0x37, 0xbe, 0x80, 0x00, 0xf5, 0x00, 
    0x37, 0x9f, 0x37, 0xbf, 0x07, 0xf6, 0x37, 0x15, 0x27, 0xfa, 0x37, 0x9d, 0xf5, 0x04, 0x37, 0x9c, 
    0x37, 0xbc, 0x80, 0x00, 0x37, 0xb5, 0x01, 0x48, 0x37, 0xea, 0x7a, 0x00, 0x28, 0x80, 0x7a, 0x21, 
    0x37, 0xb5, 0x00, 0xcf, 0x37, 0xea, 0x7a, 0x44, 0x37, 0xb5, 0x04, 0x05, 0x37, 0xea, 0x7a, 0x48, 
    0x37, 0xea, 0x7a, 0x4c, 0x37, 0xb5, 0x68, 0xf0, 0x37, 0xea, 0x7a, 0x4a, 0x37, 0xb5, 0x70, 0xf0, 
    0x37, 0xea, 0x7a, 0x4e, 0x37, 0xb5, 0xff, 0x88, 0x37, 0xea, 0x7a, 0x54, 0x37, 0xb5, 0x78, 0x30, 
    0x37, 0xea, 0x7a, 0x56, 0x37, 0xb5, 0xf8, 0x81, 0x37, 0xea, 0x08, 0x14, 0x37, 0xe5, 0x08, 0x12, 
    0x37, 0xb7, 0x00, 0x01, 0x37, 0xea, 0x08, 0x12, 0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 0x08, 0x18, 
    0x27, 0x29, 0x08, 0x06, 0xff, 0xff, 0x27, 0x29, 0x08, 0x08, 0x03, 0xff, 0x27, 0xf5, 0x37, 0x35, 
    0x08, 0x24, 0x27, 0xaa, 0x7c, 0x04, 0x37, 0x38, 0x08, 0x38, 0xb3, 0xf2, 0x17, 0x25, 0x79, 0x20, 
    0x27, 0x28, 0x08, 0x60, 0x20, 0x00, 0xfa, 0x00, 0x02, 0x64, 0xf5, 0x22, 0xfa, 0x00, 0x01, 0x82, 
    0x7a, 0x00, 0x01, 0x04, 0x37, 0xb5, 0x40, 0x88, 0x37, 0xea, 0x7c, 0x00, 0x75, 0x06, 0x17, 0x6a, 
    0x7c, 0x04, 0x75, 0xfe, 0x17, 0x6a, 0x7c, 0x05, 0x75, 0x33, 0x17, 0x6a, 0x7c, 0x16, 0x75, 0xf8, 
    0x17, 0x6a, 0x7c, 0x15, 0x75, 0xfe, 0x17, 0x6a, 0x7c, 0x17, 0x37, 0xb5, 0x81, 0x08, 0x37, 0xea, 
    0x7c, 0x18, 0x37, 0xb5, 0x10, 0x00, 0x37, 0xea, 0x7c, 0x1a, 0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 
    0x7c, 0x1c, 0x75, 0x00, 0x17, 0x6a, 0x7c, 0x1e, 0x37, 0x35, 0x42, 0x42, 0x37, 0x6a, 0x7d, 0x41, 
    0x37, 0x35, 0x02, 0x02, 0x37, 0x6a, 0x7d, 0x43, 0x37, 0x35, 0xc2, 0x02, 0x37, 0x6a, 0x7d, 0x45, 
    0x37, 0x35, 0xc2, 0x42, 0x37, 0x6a, 0x7d, 0x48, 0x37, 0xb5, 0x01, 0x00, 0x37, 0xea, 0x7d, 0x24, 
    0x37, 0xb5, 0x02, 0x02, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x02, 0xde, 0x27, 0xf7, 0x28, 0x80, 
    0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 0x37, 0x01, 0xb7, 0x0a, 0x2a, 0x80, 0x7c, 0x1f, 
    0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 0xb0, 0x04, 0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 
    0xf5, 0xa5, 0x27, 0xf7
};

// SBEC3 256k custom by Dino.
const uint8_t LdBoot_256k_SBEC3_custom[986] PROGMEM =
{
    0x37, 0x80, 0x02, 0x80, 0xfa, 0x00, 0x01, 0x44, 0xf8, 0x10, 0xb7, 0x16, 0xf8, 0x20, 0x37, 0x87, 
    0x00, 0xa2, 0xf8, 0x30, 0x37, 0x87, 0x01, 0x5a, 0xf8, 0x40, 0x37, 0x87, 0x01, 0x9a, 0xf8, 0x45, 
    0x37, 0x87, 0x00, 0x38, 0xb6, 0xda, 0xf5, 0x11, 0xfa, 0x00, 0x01, 0x2e, 0xb7, 0xd2, 0x17, 0x65, 
    0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x37, 0xb5, 0x00, 0x43, 0xfa, 0x00, 
    0x01, 0x54, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 0xb6, 0xf2, 0x17, 0xe5, 
    0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 0x27, 0xf7, 0x27, 0x4c, 0xf5, 0x46, 
    0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x37, 0x9c, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 
    0x01, 0x44, 0x17, 0xfa, 0x01, 0xb4, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 
    0x01, 0xb5, 0x17, 0xfc, 0x01, 0xb4, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 
    0x01, 0x5c, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x01, 0x5d, 0xfa, 0x00, 
    0x01, 0x2e, 0xc5, 0x00, 0xfa, 0x00, 0x01, 0x2e, 0x3c, 0x01, 0x27, 0x31, 0x01, 0x5c, 0xb6, 0xee, 
    0x37, 0x80, 0xff, 0x4e, 0x27, 0x4c, 0xf5, 0x21, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 
    0x37, 0x9c, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x01, 0xb4, 0xfa, 0x00, 
    0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x01, 0xb5, 0xfa, 0x00, 0x01, 0x2e, 0x17, 0xfc, 
    0x01, 0xb4, 0xfa, 0x00, 0x02, 0x68, 0xfa, 0x00, 0x02, 0x52, 0x37, 0xb5, 0x00, 0x1f, 0x37, 0xfa, 
    0x02, 0x9e, 0x2a, 0x10, 0x79, 0x22, 0x00, 0x0e, 0xfa, 0x00, 0x02, 0x52, 0xfa, 0x00, 0x02, 0x3c, 
    0x27, 0x31, 0x02, 0x9e, 0xb7, 0x18, 0xfa, 0x00, 0x02, 0x44, 0xfa, 0x00, 0x02, 0x32, 0x37, 0xb6, 
    0x00, 0x80, 0xb7, 0xda, 0xfa, 0x00, 0x02, 0x32, 0x37, 0xb6, 0x00, 0x78, 0xb6, 0xd0, 0xf5, 0x22, 
    0xb0, 0x00, 0xfa, 0x00, 0x02, 0x32, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x02, 0x3c, 0x37, 0x80, 
    0xfe, 0xd0, 0x37, 0xb5, 0x00, 0x70, 0x8a, 0x00, 0x85, 0x00, 0x27, 0xf7, 0x37, 0xb5, 0x00, 0x50, 
    0x8a, 0x00, 0x27, 0xf7, 0x37, 0xb5, 0x00, 0x20, 0x8a, 0x00, 0x37, 0xb5, 0x00, 0xd0, 0x8a, 0x00, 
    0x27, 0xf7, 0x37, 0xe5, 0x79, 0x0a, 0x37, 0xb1, 0xf4, 0x24, 0x37, 0xea, 0x79, 0x16, 0x17, 0xe5, 
    0x79, 0x22, 0x28, 0x10, 0x79, 0x22, 0x27, 0xf7, 0x17, 0x25, 0x79, 0x1e, 0xf5, 0x06, 0x17, 0xea, 
    0x79, 0x21, 0x27, 0xf7, 0xf5, 0x31, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x02, 0xa0, 0xfa, 0x00, 
    0x03, 0x7c, 0x27, 0x75, 0xfa, 0x00, 0x01, 0x44, 0x27, 0xda, 0x7c, 0x01, 0x37, 0x78, 0x02, 0x9e, 
    0xb6, 0xee, 0x27, 0x75, 0xf5, 0x22, 0xfa, 0x00, 0x01, 0x2e, 0x37, 0x80, 0xfe, 0x64, 0x27, 0x4c, 
    0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x02, 0x9e, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 
    0x17, 0xfa, 0x02, 0x9f, 0xfa, 0x00, 0x01, 0x2e, 0x27, 0xf7, 0xf5, 0x41, 0xfa, 0x00, 0x01, 0x2e, 
    0xfa, 0x00, 0x03, 0x4e, 0xfa, 0x00, 0x03, 0x7c, 0xfa, 0x00, 0x02, 0x3c, 0xfa, 0x00, 0x02, 0x68, 
    0xfa, 0x00, 0x02, 0x52, 0x37, 0xb5, 0x00, 0x1f, 0x37, 0xfa, 0x01, 0xb4, 0x2a, 0x10, 0x79, 0x22, 
    0x00, 0x0e, 0xfa, 0x00, 0x02, 0x52, 0xfa, 0x00, 0x02, 0x3c, 0x27, 0x31, 0x01, 0xb4, 0xb7, 0x48, 
    0xfa, 0x00, 0x02, 0x32, 0x37, 0xb6, 0x00, 0x80, 0xb7, 0xde, 0x27, 0x95, 0x37, 0xb8, 0xff, 0xff, 
    0xb7, 0x20, 0x37, 0xb5, 0x00, 0x40, 0x27, 0x8a, 0x27, 0x95, 0x27, 0x8a, 0xfa, 0x00, 0x02, 0x32, 
    0x37, 0xb6, 0x00, 0x78, 0xb6, 0xc2, 0x7c, 0x02, 0x37, 0x78, 0x02, 0x9e, 0xb6, 0xaa, 0xfa, 0x00, 
    0x03, 0x46, 0xf5, 0x22, 0xb0, 0x14, 0xfa, 0x00, 0x03, 0x46, 0x27, 0x85, 0x27, 0x90, 0xb6, 0x00, 
    0x27, 0x85, 0xb0, 0xde, 0xfa, 0x00, 0x03, 0x46, 0xf5, 0x01, 0xb0, 0xfe, 0xf5, 0x80, 0xfa, 0x00, 
    0x01, 0x2e, 0x37, 0x80, 0xfd, 0xbc, 0x37, 0xb5, 0x00, 0xff, 0x27, 0x8a, 0x27, 0xf7, 0xfa, 0x00, 
    0x01, 0x44, 0x37, 0x9c, 0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x01, 0xb4, 
    0xfa, 0x00, 0x01, 0x2e, 0xfa, 0x00, 0x01, 0x44, 0x17, 0xfa, 0x01, 0xb5, 0xfa, 0x00, 0x01, 0x2e, 
    0xfa, 0x00, 0x02, 0xa0, 0x17, 0xfc, 0x01, 0xb4, 0x27, 0x75, 0x27, 0xf7, 0xf5, 0x00, 0x37, 0x9d, 
    0x37, 0xbd, 0x03, 0x86, 0x27, 0xf7, 0x37, 0x3b, 0x00, 0xe0, 0xf5, 0x0f, 0x37, 0x9e, 0x37, 0xbe, 
    0x80, 0x00, 0xf5, 0x00, 0x37, 0x9f, 0x37, 0xbf, 0x07, 0xf6, 0x17, 0xe5, 0x7a, 0x02, 0xf8, 0x83, 
    0xb7, 0x00, 0x37, 0xbf, 0x0f, 0xf6, 0x37, 0x15, 0x27, 0xfa, 0x37, 0x9d, 0x37, 0xb5, 0x01, 0x48, 
    0x37, 0xea, 0x7a, 0x00, 0x28, 0x80, 0x7a, 0x21, 0x37, 0xb5, 0x00, 0xcf, 0x37, 0xea, 0x7a, 0x44, 
    0x37, 0xb5, 0x04, 0x05, 0x37, 0xea, 0x7a, 0x48, 0x37, 0xea, 0x7a, 0x4c, 0x37, 0xb5, 0x68, 0xf0, 
    0x37, 0xea, 0x7a, 0x4a, 0x37, 0xb5, 0x70, 0xf0, 0x37, 0xea, 0x7a, 0x4e, 0x37, 0xb5, 0xff, 0x88, 
    0x37, 0xea, 0x7a, 0x54, 0x37, 0xb5, 0x78, 0x30, 0x37, 0xea, 0x7a, 0x56, 0x37, 0xb5, 0xf8, 0x81, 
    0x37, 0xea, 0x08, 0x14, 0x37, 0xe5, 0x08, 0x12, 0x37, 0xb7, 0x00, 0x01, 0x37, 0xea, 0x08, 0x12, 
    0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 0x08, 0x18, 0x27, 0x29, 0x08, 0x06, 0xff, 0xff, 0x27, 0x29, 
    0x08, 0x08, 0x03, 0xff, 0x27, 0xf5, 0x37, 0x35, 0x08, 0x24, 0x27, 0xaa, 0x7c, 0x04, 0x37, 0x38, 
    0x08, 0x38, 0xb3, 0xf4, 0x17, 0x25, 0x79, 0x20, 0x27, 0x28, 0x08, 0x60, 0x20, 0x00, 0xfa, 0x00, 
    0x04, 0x3c, 0xf5, 0x22, 0xfa, 0x00, 0x01, 0x2e, 0x37, 0x80, 0xfc, 0xc6, 0x37, 0xb5, 0x40, 0x88, 
    0x37, 0xea, 0x7c, 0x00, 0x75, 0x06, 0x17, 0x6a, 0x7c, 0x04, 0x75, 0xfe, 0x17, 0x6a, 0x7c, 0x05, 
    0x75, 0x33, 0x17, 0x6a, 0x7c, 0x16, 0x75, 0xf8, 0x17, 0x6a, 0x7c, 0x15, 0x75, 0xfe, 0x17, 0x6a, 
    0x7c, 0x17, 0x37, 0xb5, 0x81, 0x08, 0x37, 0xea, 0x7c, 0x18, 0x37, 0xb5, 0x10, 0x00, 0x37, 0xea, 
    0x7c, 0x1a, 0x37, 0xb5, 0x00, 0x00, 0x37, 0xea, 0x7c, 0x1c, 0x75, 0x00, 0x17, 0x6a, 0x7c, 0x1e, 
    0x37, 0x35, 0x42, 0x42, 0x37, 0x6a, 0x7d, 0x41, 0x37, 0x35, 0x02, 0x02, 0x37, 0x6a, 0x7d, 0x43, 
    0x37, 0x35, 0xc2, 0x02, 0x37, 0x6a, 0x7d, 0x45, 0x37, 0x35, 0xc2, 0x42, 0x37, 0x6a, 0x7d, 0x48, 
    0x37, 0xb5, 0x01, 0x00, 0x37, 0xea, 0x7d, 0x24, 0x37, 0xb5, 0x02, 0x02, 0x37, 0xea, 0x7c, 0x1c, 
    0xfa, 0x00, 0x04, 0xb6, 0x27, 0xf7, 0x28, 0x80, 0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 
    0x37, 0x01, 0xb7, 0x0a, 0x2a, 0x80, 0x7c, 0x1f, 0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 
    0xb0, 0x02, 0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 0x27, 0xf7
};

// Empty.
const uint8_t LdBoot_empty_SBEC3[2] =
{
    0x27, 0xf7
};

// Worker functions:

// Part number read.
const uint8_t LdPartNumberRead_SBEC3[338] PROGMEM =
{
    0x37, 0x35, 0x01, 0xe8, 0xfa, 0x00, 0x02, 0xe6, 0xb5, 0x0e, 0x76, 0x08, 0xb6, 0x02, 0xfa, 0x00, 
    0x02, 0x54, 0xb0, 0x00, 0xfa, 0x00, 0x02, 0x1e, 0xfa, 0x00, 0x02, 0x66, 0x27, 0xf7, 0x37, 0xad, 
    0x37, 0x07, 0x37, 0xac, 0x34, 0x05, 0xf5, 0x04, 0x37, 0x9d, 0x37, 0xbd, 0x02, 0x00, 0x55, 0x01, 
    0x78, 0x04, 0xb2, 0x0e, 0x37, 0x0b, 0x37, 0x17, 0x37, 0x9c, 0xdc, 0x02, 0x37, 0x35, 0x00, 0x14, 
    0xfa, 0x00, 0x03, 0x3a, 0xb0, 0x00, 0xfa, 0x00, 0x02, 0x54, 0x35, 0x50, 0x37, 0x9c, 0x37, 0x17, 
    0x37, 0x9d, 0x27, 0xf7, 0x37, 0x35, 0x00, 0x14, 0xf5, 0xff, 0xfa, 0x00, 0x02, 0xc0, 0x37, 0x30, 
    0x00, 0x01, 0xb6, 0xf0, 0x27, 0xf7, 0x37, 0x35, 0x01, 0xe2, 0xfa, 0x00, 0x02, 0xe6, 0x34, 0x01, 
    0x37, 0x17, 0xfa, 0x00, 0x02, 0xc0, 0x35, 0x40, 0xfa, 0x00, 0x02, 0xc0, 0x7c, 0x02, 0x37, 0x38, 
    0x01, 0xe6, 0xb6, 0xe2, 0x37, 0x35, 0x01, 0xe9, 0xfa, 0x00, 0x02, 0xe6, 0x34, 0x01, 0x37, 0x17, 
    0xfa, 0x00, 0x02, 0xc0, 0x35, 0x40, 0xfa, 0x00, 0x02, 0xc0, 0x37, 0x35, 0x01, 0xe7, 0xfa, 0x00, 
    0x02, 0xe6, 0x34, 0x01, 0x37, 0x17, 0xfa, 0x00, 0x02, 0xc0, 0x35, 0x40, 0xfa, 0x00, 0x02, 0xc0, 
    0x37, 0x35, 0x01, 0xef, 0xfa, 0x00, 0x02, 0xe6, 0x37, 0x17, 0xfa, 0x00, 0x02, 0xc0, 0x27, 0xf7, 
    0x17, 0x65, 0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x37, 0xb5, 0x01, 0x90, 
    0xfa, 0x00, 0x03, 0x4a, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 0xb6, 0xf2, 
    0x17, 0xe5, 0x7c, 0x0f, 0x27, 0xf7, 0x27, 0xfb, 0x37, 0x04, 0x37, 0x04, 0x37, 0x04, 0x77, 0x03, 
    0x37, 0xea, 0x7d, 0x30, 0x37, 0xb5, 0x09, 0x08, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x03, 0x14, 
    0xb5, 0x0c, 0x37, 0xe5, 0x7d, 0x12, 0x37, 0x06, 0xb0, 0x04, 0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 
    0xf5, 0x5a, 0x27, 0xf7, 0x28, 0x80, 0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 0x37, 0x01, 
    0xb7, 0x0a, 0x2a, 0x80, 0x7c, 0x1f, 0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 0xb0, 0x04, 
    0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 0xf5, 0xa5, 0x27, 0xf7, 0xc5, 0x00, 0xfa, 0x00, 0x02, 0xc0, 
    0x3c, 0x01, 0x37, 0x30, 0x00, 0x01, 0xb6, 0xee, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 
    0x27, 0xf7
};

// Flash manufacturer and chip ID read.
const uint8_t LdFlashID_SBEC3[92] PROGMEM =
{
    0xf5, 0x04, 0x37, 0x9c, 0x37, 0xbc, 0x00, 0x00, 0x37, 0xb5, 0x0f, 0xa0, 0xfa, 0x00, 0x02, 0x54, 
    0x37, 0xb5, 0x00, 0x90, 0x8a, 0x00, 0xc5, 0x01, 0xfa, 0x00, 0x02, 0x36, 0xc5, 0x03, 0xfa, 0x00, 
    0x02, 0x36, 0x37, 0xb5, 0xff, 0xff, 0x8a, 0x00, 0x37, 0xb5, 0xff, 0xff, 0x8a, 0x00, 0x37, 0xb5, 
    0x00, 0x00, 0x8a, 0x00, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 
    0x7c, 0x0f, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 0xb6, 0xf2, 0x17, 0xe5, 
    0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 0x27, 0xf7
};

// Flash read.
const uint8_t LdFlashRead_SBEC3[214] PROGMEM =
{
    0xf5, 0x04, 0x37, 0x9c, 0x37, 0xbc, 0x00, 0x00, 0x37, 0xb5, 0x00, 0x00, 0x8a, 0x00, 0xfa, 0x00, 
    0x02, 0x26, 0xb5, 0x0c, 0xc5, 0x00, 0xfa, 0x00, 0x02, 0xae, 0x3c, 0x01, 0x27, 0x31, 0x02, 0xd4, 
    0xb6, 0xee, 0xb0, 0xe6, 0x27, 0xf7, 0xfa, 0x00, 0x02, 0x32, 0xb4, 0x00, 0x37, 0x3b, 0x01, 0x00, 
    0x27, 0xf7, 0xfa, 0x00, 0x02, 0xbc, 0xf8, 0x33, 0xb7, 0x08, 0xf8, 0x35, 0xb7, 0x00, 0x37, 0x10, 
    0xb0, 0x5e, 0xf5, 0x22, 0xb0, 0x5a, 0xf5, 0x34, 0xfa, 0x00, 0x02, 0xae, 0xfa, 0x00, 0x02, 0xbc, 
    0x37, 0x07, 0xf1, 0x04, 0x37, 0x9c, 0x37, 0x17, 0xfa, 0x00, 0x02, 0xae, 0xfa, 0x00, 0x02, 0xbc, 
    0x17, 0xfa, 0x02, 0xd4, 0xfa, 0x00, 0x02, 0xae, 0xfa, 0x00, 0x02, 0xbc, 0x17, 0xfa, 0x02, 0xd5, 
    0x17, 0xfc, 0x02, 0xd4, 0xfa, 0x00, 0x02, 0xae, 0xfa, 0x00, 0x02, 0xbc, 0x17, 0xfa, 0x02, 0xd4, 
    0xfa, 0x00, 0x02, 0xae, 0xfa, 0x00, 0x02, 0xbc, 0x17, 0xfa, 0x02, 0xd5, 0xfa, 0x00, 0x02, 0xae, 
    0x37, 0xf5, 0x02, 0xd4, 0x37, 0xb8, 0x00, 0x00, 0xb7, 0x04, 0x37, 0x3a, 0xfe, 0xff, 0x27, 0xf5, 
    0xb0, 0x06, 0xf5, 0x80, 0xfa, 0x00, 0x02, 0xae, 0x37, 0x3b, 0x01, 0x00, 0x27, 0xf7, 0x17, 0x65, 
    0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 
    0x76, 0x42, 0x78, 0x40, 0xb6, 0xf2, 0x17, 0xe5, 0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 
    0xb6, 0xf6, 0x27, 0xf7, 0x27, 0x4c
};

// EEPROM read.
const uint8_t LdEEPROMRead_SBEC3[294] PROGMEM =
{
    0x37, 0xb5, 0x00, 0x00, 0xfa, 0x00, 0x02, 0x22, 0xb5, 0x12, 0xfa, 0x00, 0x02, 0xb2, 0xb5, 0xf0, 
    0x37, 0x17, 0xfa, 0x00, 0x02, 0xfc, 0x7c, 0x01, 0x27, 0x31, 0x03, 0x22, 0xb6, 0xe8, 0xb0, 0xe0, 
    0x27, 0xf7, 0xfa, 0x00, 0x02, 0x2e, 0xb4, 0x00, 0x37, 0x3b, 0x01, 0x00, 0x27, 0xf7, 0xfa, 0x00, 
    0x03, 0x0a, 0xf8, 0x39, 0xb7, 0x08, 0xf8, 0x3b, 0xb7, 0x00, 0x37, 0x10, 0xb0, 0x66, 0xf5, 0x22, 
    0xb0, 0x62, 0xf5, 0x3a, 0xfa, 0x00, 0x02, 0xfc, 0xfa, 0x00, 0x03, 0x0a, 0x17, 0xfa, 0x03, 0x24, 
    0xfa, 0x00, 0x02, 0xfc, 0xfa, 0x00, 0x03, 0x0a, 0x17, 0xfa, 0x03, 0x25, 0xfa, 0x00, 0x02, 0xfc, 
    0xfa, 0x00, 0x03, 0x0a, 0x17, 0xfa, 0x03, 0x22, 0xfa, 0x00, 0x02, 0xfc, 0xfa, 0x00, 0x03, 0x0a, 
    0x17, 0xfa, 0x03, 0x23, 0xfa, 0x00, 0x02, 0xfc, 0x37, 0xf5, 0x03, 0x24, 0x37, 0xb8, 0x02, 0x00, 
    0xb4, 0x20, 0x37, 0xf1, 0x03, 0x22, 0x37, 0xb8, 0x02, 0x00, 0xb2, 0x12, 0x37, 0xf5, 0x03, 0x22, 
    0x37, 0xb8, 0x00, 0x00, 0xb7, 0x08, 0x37, 0x3a, 0xfe, 0xff, 0x27, 0xf5, 0x37, 0x75, 0x03, 0x24, 
    0xb0, 0x0a, 0xf5, 0x80, 0xb0, 0xfe, 0xf5, 0x84, 0xfa, 0x00, 0x02, 0xfc, 0x37, 0x3b, 0x01, 0x00, 
    0x27, 0xf7, 0x27, 0xfb, 0x37, 0x04, 0x37, 0x04, 0x37, 0x04, 0x77, 0x03, 0x37, 0xea, 0x7d, 0x30, 
    0x37, 0xb5, 0x09, 0x08, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x02, 0xd6, 0xb5, 0x02, 0x37, 0xe5, 
    0x7d, 0x12, 0x37, 0x06, 0x27, 0xf7, 0x28, 0x80, 0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 
    0x37, 0x01, 0xb7, 0x0a, 0x2a, 0x80, 0x7c, 0x1f, 0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 
    0xb0, 0x04, 0x37, 0x3b, 0x01, 0x00, 0x37, 0xfc, 0xf5, 0xa5, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0c, 
    0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 0x7c, 0x0f, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 
    0x78, 0x40, 0xb6, 0xf2, 0x17, 0xe5, 0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 
    0x27, 0xf7, 0x27, 0x4c, 0x27, 0x4c
};

// EEPROM write.
const uint8_t LdEEPROMWrite_SBEC3[484] PROGMEM =
{
    0xf5, 0x00, 0x37, 0x9d, 0x37, 0xbd, 0x05, 0x00, 0xfa, 0x00, 0x02, 0x42, 0xb5, 0x2e, 0xb0, 0x08, 
    0x17, 0x75, 0x03, 0xe2, 0x78, 0x05, 0xb7, 0x1e, 0x17, 0x33, 0x03, 0xe2, 0xfa, 0x00, 0x03, 0x04, 
    0xb5, 0xea, 0x37, 0x17, 0xfa, 0x00, 0x03, 0xb6, 0x7c, 0x01, 0x17, 0x35, 0x03, 0xe2, 0x27, 0x33, 
    0x03, 0xde, 0x27, 0x31, 0x03, 0xdc, 0xb6, 0xe0, 0xb0, 0xca, 0xf5, 0x01, 0xfa, 0x00, 0x03, 0xb6, 
    0x27, 0xf7, 0xfa, 0x00, 0x02, 0x66, 0xb4, 0x06, 0x37, 0x35, 0xaa, 0x55, 0x37, 0x3b, 0x01, 0x00, 
    0xb0, 0x0e, 0x27, 0x75, 0xfa, 0x00, 0x03, 0xc4, 0x27, 0xda, 0x7c, 0x01, 0x37, 0x78, 0x03, 0xdc, 
    0xb5, 0xee, 0x27, 0x75, 0x27, 0xf7, 0xfa, 0x00, 0x03, 0xc4, 0xf8, 0x36, 0xb7, 0x08, 0xf8, 0x38, 
    0xb7, 0x00, 0x37, 0x10, 0xb0, 0x66, 0xf5, 0x22, 0xb0, 0x62, 0xf5, 0x37, 0xfa, 0x00, 0x03, 0xb6, 
    0xfa, 0x00, 0x03, 0xc4, 0x17, 0xfa, 0x03, 0xde, 0xfa, 0x00, 0x03, 0xb6, 0xfa, 0x00, 0x03, 0xc4, 
    0x17, 0xfa, 0x03, 0xdf, 0xfa, 0x00, 0x03, 0xb6, 0xfa, 0x00, 0x03, 0xc4, 0x17, 0xfa, 0x03, 0xdc, 
    0xfa, 0x00, 0x03, 0xb6, 0xfa, 0x00, 0x03, 0xc4, 0x17, 0xfa, 0x03, 0xdd, 0xfa, 0x00, 0x03, 0xb6, 
    0x37, 0xf5, 0x03, 0xde, 0x37, 0xb8, 0x02, 0x00, 0xb4, 0x20, 0x37, 0xf1, 0x03, 0xdc, 0x37, 0xb8, 
    0x02, 0x00, 0xb2, 0x12, 0x37, 0xf5, 0x03, 0xdc, 0x37, 0xb8, 0x00, 0x00, 0xb7, 0x08, 0x37, 0x3a, 
    0xfe, 0xff, 0x27, 0xf5, 0x17, 0x35, 0x03, 0xe2, 0xb0, 0x0a, 0xf5, 0x80, 0xb0, 0xfe, 0xf5, 0x84, 
    0xfa, 0x00, 0x03, 0xb6, 0x37, 0x3b, 0x01, 0x00, 0x27, 0xf7, 0x37, 0xb5, 0x05, 0x00, 0x37, 0xea, 
    0x7d, 0x22, 0x37, 0xb5, 0x01, 0x01, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x03, 0x90, 0x37, 0xe5, 
    0x7d, 0x02, 0x27, 0xf7, 0x37, 0xb5, 0x00, 0x06, 0x37, 0xea, 0x7d, 0x28, 0x37, 0xb5, 0x04, 0x04, 
    0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 0x03, 0x90, 0xb5, 0x4e, 0x37, 0xf5, 0x03, 0xde, 0x37, 0x04, 
    0x37, 0x04, 0x37, 0x04, 0x77, 0x02, 0x37, 0xea, 0x7d, 0x2a, 0x37, 0x05, 0x27, 0xd5, 0x37, 0xea, 
    0x7d, 0x2c, 0x28, 0x80, 0x7d, 0x46, 0x37, 0xb5, 0x06, 0x05, 0x37, 0xea, 0x7c, 0x1c, 0xfa, 0x00, 
    0x03, 0x90, 0xb5, 0x24, 0x37, 0xb5, 0x13, 0x88, 0x37, 0xfa, 0x03, 0xe0, 0xfa, 0x00, 0x02, 0xea, 
    0x27, 0x31, 0x03, 0xe0, 0xb7, 0x0e, 0xf9, 0x03, 0xb6, 0xee, 0x37, 0xf5, 0x03, 0xde, 0xfa, 0x00, 
    0x03, 0x6e, 0xb5, 0x04, 0x27, 0x58, 0xb7, 0x00, 0x37, 0x3b, 0x01, 0x00, 0x27, 0xf7, 0x37, 0x04, 
    0x37, 0x04, 0x37, 0x04, 0x77, 0x03, 0x37, 0xea, 0x7d, 0x30, 0x37, 0xb5, 0x09, 0x08, 0x37, 0xea, 
    0x7c, 0x1c, 0xfa, 0x00, 0x03, 0x90, 0xb5, 0x02, 0x37, 0xe5, 0x7d, 0x12, 0x37, 0x06, 0x27, 0xf7, 
    0x28, 0x80, 0x7c, 0x1f, 0x29, 0x80, 0x7c, 0x1a, 0x37, 0x05, 0x37, 0x01, 0xb7, 0x0a, 0x2a, 0x80, 
    0x7c, 0x1f, 0xff, 0xf6, 0x28, 0x80, 0x7c, 0x1f, 0x37, 0x06, 0xb0, 0x04, 0x37, 0x3b, 0x01, 0x00, 
    0x37, 0xfc, 0xf5, 0xa5, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0c, 0x76, 0x01, 0xb7, 0xf4, 0x17, 0xea, 
    0x7c, 0x0f, 0x27, 0xf7, 0x17, 0x65, 0x7c, 0x0d, 0x76, 0x42, 0x78, 0x40, 0xb6, 0xf2, 0x17, 0xe5, 
    0x7c, 0x0f, 0x27, 0xf7, 0x37, 0xb0, 0x00, 0x01, 0xb6, 0xf6, 0x27, 0xf7, 0x27, 0x4c, 0x27, 0x4c, 
    0x27, 0x4c, 0x27, 0x4c
};

// Empty.
const uint8_t LdWorker_empty_SBEC3[2] =
{
    0x27, 0xf7
};

/*************************************************************************
Function: solve_sbec3_bootstrap_seed()
Purpose:  solve the bootstrap security challenge
Note:     provided by Konstantin aka Piton
**************************************************************************/
uint16_t solve_sbec3_bootstrap_seed(uint16_t input)
{
    uint16_t seed = (input + 0x247C) | 5;
    int i = seed & 0x0F;

    while (i--)
    {
        if (seed & 1) seed = ((seed >> 1) | 0x8000);
        else seed >>= 1;
    }

    return (seed | 0x247C);
}

uint16_t pcm_peek(uint16_t index = 0); // function prototype

/*************************************************************************
Function: unlock_bootstrap_mode()
Purpose:  solve the bootstrap security seed and upload bootloader
**************************************************************************/
void unlock_sbec3_bootstrap_mode(uint8_t bootloader_src)
{
    uint8_t ret[1] = { 0 };
    // 0 = ok
    // 1 = no response to magic byte
    // 2 = unexpected response to magic byte
    // 3 = security seed response timeout
    // 4 = security seed response checksum error
    // 5 = security key status timeout
    // 6 = security key not accepted
    // 7 = start bootloader timeout
    // 8 = unexpected bootloader status byte

    uint8_t buff[16];
    uint8_t checksum = 0;
    bool key_required = true;

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();
    pcm_tx_flush();
    pcm.last_byte_millis = millis();

    // Write magic byte to set 62500 baud.
    pcm_putc(0x7F);

    while ((uint32_t)(millis() - pcm.last_byte_millis) < SCI_BTSTRP_DLY); // wait for response

    if (pcm_rx_available() == 0)
    {
        ret[0] = 1;
        send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
        pcm_rx_flush();
        return;
    }

    buff[0] = pcm_getc() & 0xFF;

    if (buff[0] != 0x06)
    {
        ret[0] = 2;
        send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
        pcm_rx_flush();
        return;
    }

    uint8_t req_seed_cmd[5] = { 0x24, 0xD0, 0x27, 0xC1, 0xDC };

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();
    pcm.last_byte_millis = millis();

    // Write security seed request message.
    for (uint8_t i = 0; i < ARRAY_SIZE(req_seed_cmd); i++)
    {
        pcm_putc(req_seed_cmd[i]);
    }

    while ((uint32_t)(millis() - pcm.last_byte_millis) < (2 * SCI_BTSTRP_DLY)); // wait for response

    if (pcm_rx_available() < 7)
    {
        if ((pcm_rx_available() == 5) && (pcm_peek(0) == 0xDB) && (pcm_peek(1) == 0x2F) && (pcm_peek(2) == 0xD8) && (pcm_peek(3) == 0x3E) && (pcm_peek(4) == 0x23))
        {
            key_required = false;
        }
        else
        {
            ret[0] = 3;
            send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
            pcm_rx_flush();
            return;
        }
    }

    if (key_required)
    {
        // Read security seed response.
        for (uint8_t i = 0; i < 7; i++)
        {
            buff[i] = pcm_getc() & 0xFF;
    
            if (i < 6) checksum += buff[i]; // calculate checksum
        }

        uint8_t key[2];

        // Evaluate seed response message and solve the puzzle.
        if ((buff[0] == 0x26) && (buff[1] == 0xD0) && (buff[2] == 0x67) && (buff[3] == 0xC1) && (buff[6] == checksum))
        {
            uint16_t data = solve_sbec3_bootstrap_seed(to_uint16(buff[4], buff[5]));
            key[0] = (data >> 8) & 0xFF;
            key[1] = data & 0xFF;
        }
        else
        {
            ret[0] = 4;
            send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
            pcm_rx_flush();
            return;
        }

        uint8_t send_key_cmd[7] = { 0x24, 0xD0, 0x27, 0xC2, key[0], key[1], 0 };
        send_key_cmd[6] = calculate_checksum(send_key_cmd, 0, ARRAY_SIZE(send_key_cmd) - 1);

        wdt_reset(); // feed the watchdog
        pcm_rx_flush();
        pcm.last_byte_millis = millis();

        // Write security key message.
        for (uint8_t i = 0; i < ARRAY_SIZE(send_key_cmd); i++)
        {
            pcm_putc(send_key_cmd[i]);
        }

        while ((uint32_t)(millis() - pcm.last_byte_millis) < (2 * SCI_BTSTRP_DLY)); // wait for response

        if (pcm_rx_available() < 5)
        {
            ret[0] = 5;
            send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
            pcm_rx_flush();
            return;
        }

        // Read security key status.
        for (uint8_t i = 0; i < 5; i++)
        {
            buff[i] = pcm_getc() & 0xFF;
        }

        // Key not accepted.
        if (!((buff[0] == 0x26) && (buff[1] == 0xD0) && (buff[2] == 0x67) && (buff[3] == 0xC2) && (buff[4] == 0x1F)))
        {
            ret[0] = 6;
            send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
            pcm_rx_flush();
            return;
        }
    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    // Upload bootloader.
    switch (bootloader_src)
    {
        case 1: // SBEC3 (128k)
        {
            uint8_t bl_header[5] = { 0x4C, 0x01, 0x00, (((0x0100 + ARRAY_SIZE(LdBoot_128k_SBEC3) - 1) >> 8) & 0xFF), ((0x0100 + ARRAY_SIZE(LdBoot_128k_SBEC3) - 1) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(bl_header); i++)
            {
                pcm_putc(bl_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            
            for (uint16_t i = 0; i < ARRAY_SIZE(LdBoot_128k_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdBoot_128k_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 2: // SBEC3 (256k)
        {
            uint8_t bl_header[5] = { 0x4C, 0x01, 0x00, (((0x0100 + ARRAY_SIZE(LdBoot_256k_SBEC3) - 1) >> 8) & 0xFF), ((0x0100 + ARRAY_SIZE(LdBoot_256k_SBEC3) - 1) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(bl_header); i++)
            {
                pcm_putc(bl_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdBoot_256k_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdBoot_256k_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 3: // SBEC3 256k custom by Dino
        {
            uint8_t bl_header[5] = { 0x4C, 0x01, 0x00, (((0x0100 + ARRAY_SIZE(LdBoot_256k_SBEC3_custom) - 1) >> 8) & 0xFF), ((0x0100 + ARRAY_SIZE(LdBoot_256k_SBEC3_custom) - 1) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(bl_header); i++)
            {
                pcm_putc(bl_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdBoot_256k_SBEC3_custom); i++)
            {
                pcm_putc(pgm_read_byte(LdBoot_256k_SBEC3_custom + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 0:
        default:
        {
            uint8_t bl_header[5] = { 0x4C, 0x01, 0x00, (((0x0100 + ARRAY_SIZE(LdBoot_empty_SBEC3) - 1) >> 8) & 0xFF), ((0x0100 + ARRAY_SIZE(LdBoot_empty_SBEC3) - 1) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(bl_header); i++)
            {
                pcm_putc(bl_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdBoot_empty_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdBoot_empty_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    uint8_t start_bootloader_cmd[3] = { 0x47, 0x01, 0x00 };

    // Write start bootloader message.
    for (uint8_t i = 0; i < ARRAY_SIZE(start_bootloader_cmd); i++)
    {
        pcm_putc(start_bootloader_cmd[i]);
        while (pcm_rx_available() == 0); // wait for echo
        pcm_getc(); // read echo into oblivion
    }

    while ((uint32_t)(millis() - pcm.last_byte_millis) < SCI_BTSTRP_DLY); // wait for response

    if (pcm_rx_available() == 0)
    {
        ret[0] = 7;
        send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
        pcm_rx_flush();
        return;
    }

    uint8_t bootloader_status = pcm_getc() & 0xFF;

    if (bootloader_status != 0x22)
    {
        ret[0] = 8;
        send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
        pcm_rx_flush();
        return;
    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    // Success.
    send_usb_packet(bus_usb, debug, init_bootstrap_mode, ret, 1);
}

/*************************************************************************
Function: upload_worker_function()
Purpose:  upload worker function in bootstrap mode
**************************************************************************/
void write_worker_function(uint8_t worker_function_src)
{
    uint8_t ret[1] = { 0 };
    // 0 = ok
    // 1 = upload interrupted
    // 2 = unexpected upload result
    // 3 = function did not start
    // 4 = unexpected function start status

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();
    
    switch (worker_function_src)
    {
        case 1: // part number read
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdPartNumberRead_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdPartNumberRead_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdPartNumberRead_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdPartNumberRead_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 2: // flash manufacturer and chip ID read
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdFlashID_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdFlashID_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdFlashID_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdFlashID_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 3: // flash read
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdFlashRead_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdFlashRead_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdFlashRead_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdFlashRead_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 7: // EEPROM read
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdEEPROMRead_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdEEPROMRead_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdEEPROMRead_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdEEPROMRead_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 8: // EEPROM write
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdEEPROMWrite_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdEEPROMWrite_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdEEPROMWrite_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdEEPROMWrite_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
        }
        case 0:
        default:
        {
            uint8_t wf_header[3] = { 0x10, ((ARRAY_SIZE(LdWorker_empty_SBEC3) >> 8) & 0xFF), (ARRAY_SIZE(LdWorker_empty_SBEC3) & 0xFF) };

            for (uint8_t i = 0; i < ARRAY_SIZE(wf_header); i++)
            {
                pcm_putc(wf_header[i]);
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }

            for (uint16_t i = 0; i < ARRAY_SIZE(LdWorker_empty_SBEC3); i++)
            {
                pcm_putc(pgm_read_byte(LdWorker_empty_SBEC3 + i));
                while (pcm_rx_available() == 0); // wait for echo
                pcm_getc(); // read echo into oblivion
            }
            break;
            break;
        }
    }

    // Wait for upload finished status byte.
    while ((uint32_t)(millis() - pcm.last_byte_millis) < SCI_BTSTRP_DLY); // wait for response

    if (pcm_rx_available() == 0)
    {
        ret[0] = 1;
        send_usb_packet(bus_usb, debug, upload_worker_function, ret, 1);
        pcm_rx_flush();
        return;
    }

    uint8_t upload_status = pcm_getc() & 0xFF;

    if (upload_status != 0x14)
    {
        ret[0] = 2;
        send_usb_packet(bus_usb, debug, upload_worker_function, ret, 1);
        pcm_rx_flush();
        return;
    }

//    wdt_reset(); // feed the watchdog
//    pcm_rx_flush();
//    pcm.last_byte_millis = millis();
//
//    // Start worker function.
//    pcm_putc(0x20);
//
//    while ((uint32_t)(millis() - pcm.last_byte_millis) < SCI_BTSTRP_DLY); // wait for response
//
//    if (pcm_rx_available() == 0)
//    {
//        ret[0] = 3;
//        send_usb_packet(bus_usb, debug, upload_worker_function, ret, 1);
//        pcm_rx_flush();
//        return;
//    }
//
//    uint8_t start_status = pcm_getc() & 0xFF;
//
//    if (start_status != 0x21)
//    {
//        ret[0] = 4;
//        send_usb_packet(bus_usb, debug, upload_worker_function, ret, 1);
//        pcm_rx_flush();
//        return;
//    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    // Success.
    send_usb_packet(bus_usb, debug, upload_worker_function, ret, 1);
}

/*************************************************************************
Function: restore_pcm_eeprom_handler()
Purpose:  read current PCM EEPROM value and replace with backup value if different
**************************************************************************/
void restore_pcm_eeprom_handler(uint8_t *input_data)
{
    uint8_t ret[1] = { 0 };
    // 0 = ok
    // 1 = no response to security seed request
    // 2 = checksum error
    // 3 = no response to key
    // 4 = key not accepted
    // 5 = read EEPROM command not returning a value
    // 6 = write EEPROM command not returning a result
    // 7 = write EEPROM command failed

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();
    pcm_tx_flush();
    pcm.last_byte_millis = millis();

    // Get security seed challenge.
    pcm_putc(0x2B);

    while ((pcm_rx_available() < 4) && ((uint32_t)(millis() - pcm.last_byte_millis) < 200)); // wait for response

    if (pcm_rx_available() < 4)
    {
        ret[0] = 1;
        send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
        pcm_rx_flush();
        return;
    }

    uint8_t buff[16];
    uint8_t buff_ptr = 0;
    uint8_t checksum = 0;

    for (uint8_t i = 0; i < 4; i++)
    {
        buff[i] = pcm_getc() & 0xFF;

        if (i < 3) checksum += buff[i]; // calculate checksum
    }

    if (buff[3] != checksum)
    {
        ret[0] = 2;
        send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
        pcm_rx_flush();
        return;
    }

    send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, buff, 4);

    bool key_required = true;

    if ((buff[1] == 0) && (buff[2] == 0))
    {
        key_required = false;
    }

    if (key_required)
    {
        uint16_t seed = to_uint16(buff[1], buff[2]);
        uint16_t a = (seed << 2) + 0x9018;
        uint8_t key[2] = { ((a >> 8) & 0xFF), (a & 0xFF) };

        uint8_t send_key_cmd[4] = { 0x2C, key[0], key[1], 0 };
        send_key_cmd[3] = calculate_checksum(send_key_cmd, 0, ARRAY_SIZE(send_key_cmd) - 1);

        wdt_reset(); // feed the watchdog
        pcm_rx_flush();
        buff_ptr = 0;

        // Write security key message.
        for (uint8_t i = 0; i < ARRAY_SIZE(send_key_cmd); i++)
        {
            pcm_putc(send_key_cmd[i]);
            while (pcm_rx_available() == 0); // wait for echo
            buff[i] = pcm_getc() & 0xFF; // read echo into buffer
            buff_ptr++;
        }

        while ((pcm_rx_available() == 0) && (uint32_t)(millis() - pcm.last_byte_millis) < 200); // wait for response

        if (pcm_rx_available() == 0)
        {
            ret[0] = 3;
            send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
            pcm_rx_flush();
            return;
        }

        // Read key status.
        buff[buff_ptr] = pcm_getc() & 0xFF;
        buff_ptr++;

        send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, buff, buff_ptr);

        if (buff[4] != 0)
        {
            ret[0] = 4;
            send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
            pcm_rx_flush();
            return;
        }
    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    uint8_t read_eeprom_cmd[3] = { 0x28, 0x00, 0x00 };
    uint8_t write_eeprom_cmd[4] = { 0x27, 0x00, 0x00, 0x00 };
    uint8_t offset[2] = { 0, 0 };
    uint8_t attempts = 0;
    bool valid_response = false;

    for (uint16_t i = 0; i < 512; i++) // EEPROM size = 512 bytes
    {
        // Set current offset.
        offset[0] = (i >> 8) & 0xFF;
        offset[1] = i & 0xFF;
        read_eeprom_cmd[1] = offset[0];
        read_eeprom_cmd[2] = offset[1];

        valid_response = false;
        attempts = 0;

        while (!valid_response)
        {
            wdt_reset(); // feed the watchdog
            pcm_rx_flush();
            buff_ptr = 0;

            // Read current EEPROM value.
            for (uint8_t j = 0; j < ARRAY_SIZE(read_eeprom_cmd); j++)
            {
                pcm_putc(read_eeprom_cmd[j]);
                while (pcm_rx_available() == 0); // wait for echo
                buff[j] = pcm_getc() & 0xFF; // read echo into buffer
                buff_ptr++;
            }

            while ((pcm_rx_available() == 0) && ((uint32_t)(millis() - pcm.last_byte_millis) < 300)); // wait for response

            if (pcm_rx_available() == 0)
            {
                attempts++;

                if (attempts >= 10)
                {
                    ret[0] = 5;
                    send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
                    pcm_rx_flush();
                    return;
                }

                delay(100);
            }
            else
            {
                valid_response = true;
            }
        }

        buff[buff_ptr] = pcm_getc() & 0xFF; // read response (current EEPROM byte)
        buff_ptr++;
        
        send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, buff, buff_ptr);

        // Update EEPROM byte if different.
        if (input_data[i] != buff[3])
        {
            write_eeprom_cmd[1] = offset[0];
            write_eeprom_cmd[2] = offset[1];
            write_eeprom_cmd[3] = input_data[i];

            valid_response = false;
            attempts = 0;

            while (!valid_response)
            {
                wdt_reset(); // feed the watchdog
                pcm_rx_flush();
                buff_ptr = 0;

                // Write new EEPROM value.
                for (uint8_t j = 0; j < ARRAY_SIZE(write_eeprom_cmd); j++)
                {
                    pcm_putc(write_eeprom_cmd[j]);
                    while (pcm_rx_available() == 0); // wait for echo
                    buff[j] = pcm_getc() & 0xFF; // read echo into buffer
                    buff_ptr++;
                }

                while ((pcm_rx_available() == 0) && ((uint32_t)(millis() - pcm.last_byte_millis) < 300)); // wait for response

                if (pcm_rx_available() == 0)
                {
                    attempts++;

                    if (attempts >= 10)
                    {
                        ret[0] = 6;
                        send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
                        pcm_rx_flush();
                        return;
                    }

                    delay(100);
                }
                else
                {
                    buff[buff_ptr] = pcm_getc() & 0xFF;
                    buff_ptr++;
                    send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, buff, buff_ptr);

                    if (buff[4] == 0xE2) // EEPROM write successful
                    {
                        valid_response = true;
                    }
                    else
                    {
                        attempts++;

                        if (attempts >= 10)
                        {
                            ret[0] = 7;
                            send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
                            pcm_rx_flush();
                            return;
                        }
    
                        delay(100);
                    }
                }
            }
        }

        handle_leds();
    }

    wdt_reset(); // feed the watchdog
    pcm_rx_flush();

    // Success.
    send_usb_packet(bus_usb, debug, restore_pcm_eeprom, ret, 1);
}

/*************************************************************************
Function: eep_init()
Purpose:  initialize external EEPROM
**************************************************************************/
void eep_init(void)
{
    eep_status = eep.begin(extEEPROM::twiClock400kHz);

    if (eep_status) // non-zero = bad
    { 
        eep_present = false;
        send_usb_packet(bus_usb, ok_error, error_eep_not_found, err, 1);
    }
    else // zero = good
    {
        eep_present = true;
    }
}

/*************************************************************************
Function: write_default_settings()
Purpose:  write default settings to internal EEPROM
**************************************************************************/
void write_default_settings(void)
{
    // Memory map:
    // $00-$01: hardware version (multiplied by 100, example: V1.40 = 140(DEC) = $008C; V1.41 = 141(DEC) = $008D; V1.42 = 142(DEC) = $008E; V1.43 = 143(DEC) = $008F)
    // $02-$09: hardware date (PCB order date, 64-bit UNIX time)
    // $0A-$11: assembly date (Scanner assembly date, 64-bit UNIX time)
    // $12-$13: ADC supply voltage multiplied by 100 (example 5.00V = 500(DEC) = $01F4)
    // $14-$15: high resistor value in the voltage divider: 27 kOhm = 27000(DEC) = $6978
    // $16-$17: low resistor value in the voltage divider: 5.1 kOhm = 5100(DEC) = $13EC
    // $18:     LCD enabled/disabled
    // $19:     LCD I2C address
    // $1A:     LCD column size
    // $1B:     LCD row size
    // $1C:     LCD refresh rate (Hz)
    // $1D:     LCD imperial/metric units (0: imperial, 1: metric)
    // $1E:     LCD data source (1: CCD-bus, 2: SCI-bus (PCM), 3: SCI-bus (TCM))
    // $1F-$20: LED heartbeat interval (5000 ms)
    // $21-$22: LED blink duration (50 ms)
    // $23:     CCD-bus settings
    // $24:     SCI-bus (PCM) settings
    // $25:     SCI-bus (TCM) settings
    // $26:     CCD-bus automatic request messages
    // $27:     SCI-bus (PCM) automatic request messages
    // $28:     SCI-bus (TCM) automatic request messages
    // $29-$FE: reserved for future data

    uint8_t default_settings[41] =
    { //  00    01    02    03    04    05    06    07    08    09    0A    0B    0C    0D    0E    0F
        0x00, 0x96, 0x00, 0x00, 0x00, 0x00, 0x61, 0xF9, 0x08, 0x37, 0x00, 0x00, 0x00, 0x00, 0x61, 0xF9, // 00
        0x08, 0x37, 0x01, 0xF4, 0x69, 0x78, 0x13, 0xEC, 0x00, 0x27, 0x14, 0x04, 0x14, 0x00, 0x01, 0x13, // 10
        0x88, 0x00, 0x32, 0x41, 0x91, 0xC1, 0x00, 0x00, 0x00                                            // 20
    };

    for (uint16_t i = 0; i < 41; i++)
    {
        EEPROM.update(i, default_settings[i]);
    }
}

/*************************************************************************
Function: load_settings()
Purpose:  load settings from ATmega2560's internal EEPROM (V1.50+)
          or copy settings from external EEPROM (V1.40 - V1.44)
**************************************************************************/
void load_settings(void)
{
    if (((EEPROM.read(0) == 0x00) && (EEPROM.read(1) == 0x00)) || (EEPROM.read(0) == 0xFF)) // V1.40 - V1.44, copy settings from external to internal EEPROM
    {
        if (eep_present)
        {
            if (eep.read(0) != 0xFF) // settings are present in external EEPROM
            {
                for (uint16_t i = 0; i < 41; i++) // copy first 41 bytes to internal EEPROM
                {
                    EEPROM.update(i, eep.read(i));
                }
            }
            else // external EEPROM is empty
            {
                write_default_settings();
            }
        }
        else // external EEPROM is missing
        {
            write_default_settings();
        }
    }

    // Load and apply settings.

    uint8_t data[2] = { 0x00, 0x00 };

    hw_version[0] = EEPROM.read(0x00);
    hw_version[1] = EEPROM.read(0x01);

    if (to_uint16(hw_version[0], hw_version[1]) < 150)
    {
        eep = extEEPROM(kbits_32, 1, 32, 0x50); // device size: 32 kilobits = 4 kilobytes, number of devices: 1, page size: 32 bytes (from datasheet), device address: 0x50 by default
        eep.begin(extEEPROM::twiClock400kHz);
    }
    else
    {
        eep = extEEPROM(kbits_512, 1, 128, 0x50); // device size: 512 kilobits = 64 kilobytes, number of devices: 1, page size: 128 bytes (from datasheet), device address: 0x50 by default
        eep.begin(extEEPROM::twiClock400kHz);
    }

    hw_date[0] = EEPROM.read(0x02);
    hw_date[1] = EEPROM.read(0x03);
    hw_date[2] = EEPROM.read(0x04);
    hw_date[3] = EEPROM.read(0x05);
    hw_date[4] = EEPROM.read(0x06);
    hw_date[5] = EEPROM.read(0x07);
    hw_date[6] = EEPROM.read(0x08);
    hw_date[7] = EEPROM.read(0x09);

    assembly_date[0] = EEPROM.read(0x0A);
    assembly_date[1] = EEPROM.read(0x0B);
    assembly_date[2] = EEPROM.read(0x0C);
    assembly_date[3] = EEPROM.read(0x0D);
    assembly_date[4] = EEPROM.read(0x0E);
    assembly_date[5] = EEPROM.read(0x0F);
    assembly_date[6] = EEPROM.read(0x10);
    assembly_date[7] = EEPROM.read(0x11);

    data[0] = EEPROM.read(0x12);
    data[1] = EEPROM.read(0x13);
    adc_supply_voltage = to_uint16(data[0], data[1]);

    data[0] = EEPROM.read(0x14);
    data[1] = EEPROM.read(0x15);
    rd_high_value = to_uint16(data[0], data[1]);

    data[0] = EEPROM.read(0x16);
    data[1] = EEPROM.read(0x17);
    rd_low_value = to_uint16(data[0], data[1]);

    EEPROM.read(0x18) ? (lcd_enabled = true) : (lcd_enabled = false);

    lcd_i2c_address = EEPROM.read(0x19);
    lcd_char_width = EEPROM.read(0x1A);
    lcd_char_height = EEPROM.read(0x1B);
    lcd_refresh_rate = EEPROM.read(0x1C);
    lcd_update_interval = 1000 / lcd_refresh_rate;
    lcd_units = EEPROM.read(0x1D);
    lcd_data_source = EEPROM.read(0x1E);

    data[0] = EEPROM.read(0x1F);
    data[1] = EEPROM.read(0x20);
    heartbeat_interval = to_uint16(data[0], data[1]);
    heartbeat_interval ? (heartbeat_enabled = true) : (heartbeat_enabled = false);

    data[0] = EEPROM.read(0x21);
    data[1] = EEPROM.read(0x22);
    led_blink_duration = to_uint16(data[0], data[1]);

    ccd.bus_settings = EEPROM.read(0x23);
    if (ccd.bus_settings == 0x00) ccd.bus_settings = 0xC1;
    configure_ccd_bus(ccd.bus_settings);

    pcm.bus_settings = EEPROM.read(0x24);
    if (pcm.bus_settings == 0x00) pcm.bus_settings = 0x81;

    tcm.bus_settings = EEPROM.read(0x25);
    if (tcm.bus_settings == 0x00) tcm.bus_settings = 0x21;

    ccd.auto_request = EEPROM.read(0x26);

    if (ccd.auto_request == 0xFF)
    {
        ccd.auto_request = 0;
        EEPROM.update(0x26, ccd.auto_request);
    }

    pcm.auto_request = EEPROM.read(0x27);

    if (pcm.auto_request == 0xFF)
    {
        pcm.auto_request = 0;
        EEPROM.update(0x27, pcm.auto_request);
    }
    
    tcm.auto_request = EEPROM.read(0x28);

    if (tcm.auto_request == 0xFF)
    {
        tcm.auto_request = 0;
        EEPROM.update(0x28, tcm.auto_request);
    }
}

/*************************************************************************
Function: lcd_init()
Purpose:  initialize LCD
**************************************************************************/
void lcd_init(void)
{
    lcd_splash_screen_on = true;
    lcd = LiquidCrystal_I2C(lcd_i2c_address, lcd_char_width, lcd_char_height);
    lcd.begin();
    lcd.backlight(); // backlight on
    lcd.clear(); // clear display
    lcd.home(); // set cursor in home position (0, 0)
    float hw_ver = to_uint16(hw_version[0], hw_version[1]) / 100.0;
        
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.print(F("--------------------")); // F(" ") makes the compiler store the string inside flash memory instead of RAM, good practice if system is low on RAM
        lcd.setCursor(0, 1);
        lcd.print(F("  CHRYSLER CCD/SCI  "));
        lcd.setCursor(0, 2);
        lcd.print(F("   SCANNER V"));
        lcd.print(hw_ver, 2);
        lcd.print(F("    "));
        lcd.setCursor(0, 3);
        lcd.print(F("--------------------"));
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.print(F("CHRYSLER CCD/SCI")); // F(" ") makes the compiler store the string inside flash memory instead of RAM, good practice if system is low on RAM
        lcd.setCursor(0, 1);
        lcd.print(F(" SCANNER V"));
        lcd.print(hw_ver, 2);
        lcd.print(F("  "));
    }
    else
    {
        lcd.print(F("CCD/SCI"));
    }

    lcd.createChar(0, up_symbol); // custom character from "upsymbol" variable with id number 0
    lcd.createChar(1, down_symbol);
    lcd.createChar(2, left_symbol);
    lcd.createChar(3, right_symbol);
    lcd.createChar(4, enter_symbol);
    lcd.createChar(5, degree_symbol);
}

/*************************************************************************
Function: print_display_layout_1_metric()
Purpose:  printing default metric display layout to LCD (km/h, km...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_metric(void)
{
    lcd.clear();
    
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0km/h    0rpm     ")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0/  0 C     0.0kPa")); // 2nd line 
        lcd.setCursor(0, 2);
        lcd.print(F(" 0.0/ 0.0V    0.000V")); // 3rd line
        lcd.setCursor(0, 3);
        lcd.print(F("     0.000km    0kPa")); // 4th line
        lcd.setCursor(7, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0km/h     0rpm")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0 C     0.0kPa")); // 2nd line 
        lcd.setCursor(3, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else
    {
        lcd.setCursor(0, 0);
        lcd.print(F("    0rpm")); // 1st line
    }
}

/*************************************************************************
Function: print_display_layout_1_imperial()
Purpose:  printing default metric display layout to LCD (mph, mi...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_imperial(void)
{
    lcd.clear();
    
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0mph     0rpm     ")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0/  0 F     0.0psi")); // 2nd line 
        lcd.setCursor(0, 2);
        lcd.print(F(" 0.0/ 0.0V    0.000V")); // 3rd line
        lcd.setCursor(0, 3);
        lcd.print(F("     0.000mi    0psi")); // 4th line
        lcd.setCursor(7, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0mph      0rpm")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0 F     0.0psi")); // 2nd line 
        lcd.setCursor(3, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else
    {
        lcd.setCursor(0, 0);
        lcd.print(F("    0rpm")); // 1st line
    }
}

/*************************************************************************
Function: handle_lcd()
Purpose:  write stuff to LCD
Note:     uncomment cases when necessary
**************************************************************************/
void handle_lcd(uint8_t bus, uint8_t *data, uint8_t index, uint8_t datalength)
{
    if (lcd_enabled)
    {
//        current_millis = millis(); // check current time
//        if ((uint32_t)(current_millis - lcd_last_update ) >= lcd_update_interval) // refresh rate
//        {
//            lcd_last_update = current_millis;
            
            uint8_t message_length = datalength-index;
            uint8_t message[message_length];

            if (message_length == 0) return;
        
            for (uint8_t i = index; i < datalength; i++)
            {
                message[i-index] = data[i]; // make a local copy of the source array
            }

            switch (lcd_data_source)
            {
                case bus_ccd:
                {
                    if (bus == bus_ccd)
                    {
                        switch (message[0]) // check ID-byte
                        {
                            case 0x24: // VEHICLE SPEED
                            {
                                if (message_length > 3)
                                {
                                    uint8_t vehicle_speed = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        vehicle_speed = message[1];
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        vehicle_speed = message[2];
                                    }
        
                                    if (((lcd_char_width == 20) && (lcd_char_height == 4)) || ((lcd_char_width == 16) && (lcd_char_height == 2))) // 20x4 / 16x2 LCD
                                    {
                                        if (vehicle_speed < 10)
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print("  ");
                                            lcd.print(vehicle_speed);
                                        }
                                        else if ((vehicle_speed >= 10) && (vehicle_speed < 100))
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(" ");
                                            lcd.print(vehicle_speed);
                                        }
                                        else if (vehicle_speed >= 100)
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(vehicle_speed);
                                        }
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x42: // THROTTLE POSITION SENSOR | CRUISE CONTROL
                            {
                                if (message_length > 3)
                                {
                                    float tps_delta_voltage = roundf(message[1] * 1000.0 * 0.0196);
                                    tps_delta_voltage = tps_delta_voltage / 1000.0;

                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        lcd.setCursor(14, 2);
                                        lcd.print(tps_delta_voltage, 3);
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x8C: // ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE
                            {
                                if (message_length > 3)
                                {
                                    int coolant_temp = 0;
                                    int intake_temp = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        coolant_temp = round((message[1] * 1.8) - 198.4);
                                        intake_temp = round((message[2] * 1.8) - 198.4);
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        coolant_temp = message[1] - 128;
                                        intake_temp = message[2] - 128;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (coolant_temp <= -100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp > -10) && (coolant_temp < 0))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("  ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if (coolant_temp >= 100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
        
                                        if (intake_temp <= -100)
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((intake_temp > -100) && (intake_temp <= -10))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp > -10) && (intake_temp < 0))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(" ");
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp >= 0) && (intake_temp < 10))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print("  ");
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp >= 10) && (intake_temp < 100))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(" ");
                                            lcd.print(intake_temp);
                                        }
                                        else if (intake_temp >= 100)
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(intake_temp);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        if (coolant_temp <= -100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp > -10) && (coolant_temp < 0))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("  ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if (coolant_temp >= 100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x0C: // ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE + BATTERY VOLTAGE + OIL PRESSURE
                            {
                                if (message_length > 3)
                                {
                                    uint16_t oil_pressure;

                                    if (lcd_units == 0) // imperial
                                    {
                                        oil_pressure = round(message[2] * 0.5);
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        oil_pressure = round(message[2] * 6.894757 * 0.5);
                                    }

                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (oil_pressure < 10)
                                        {
                                            lcd.setCursor(14, 3);
                                            lcd.print("  ");
                                            lcd.print(oil_pressure);
                                        }
                                        else if ((oil_pressure >= 10) && (oil_pressure < 100))
                                        {
                                            lcd.setCursor(14, 3);
                                            lcd.print(" ");
                                            lcd.print(oil_pressure);
                                        }
                                        else if (oil_pressure >= 100)
                                        {
                                            lcd.setCursor(14, 3);
                                            lcd.print(oil_pressure);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0xCE: // VEHICLE DISTANCE / ODOMETER VALUE
                            {
                                if (message_length > 5)
                                {
                                    uint32_t odometer_raw = to_uint32(message[1], message[2], message[3], message[4]);
                                    double odometer = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        odometer = roundf(odometer_raw / 8.0);
                                        odometer = odometer / 1000.0;
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        odometer = roundf(odometer_raw * 1609.344 / 8000.0);
                                        odometer = odometer / 1000.0;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (odometer < 10.0)
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("     ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 10.0) && (odometer < 100.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("    ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 100.0) && (odometer < 1000.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("   ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 1000.0) && (odometer < 10000.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("  ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 10000) && (odometer < 100000))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print(" ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if (odometer >= 100000)
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print(odometer, 3);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0xD4: // BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE
                            {
                                if (message_length > 3)
                                {
                                    float batt_volts = roundf(message[1] * 10.0 * 0.0625);
                                    batt_volts = batt_volts / 10.0;
                                    
                                    float charge_volts = roundf(message[2] * 10.0 * 0.0625);
                                    charge_volts = charge_volts / 10.0;
                                    
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (batt_volts < 10.0)
                                        {
                                            lcd.setCursor(0, 2);
                                            lcd.print(" ");
                                            lcd.print(batt_volts, 1);
                                        }
                                        else if (batt_volts >= 10.0)
                                        {
                                            lcd.setCursor(0, 2);
                                            lcd.print(batt_volts, 1);
                                        }
        
                                        if (charge_volts < 10.0)
                                        {
                                            lcd.setCursor(5, 2);
                                            lcd.print(" ");
                                            lcd.print(charge_volts, 1);
                                        }
                                        else if (charge_volts >= 10.0)
                                        {
                                            lcd.setCursor(5, 2);
                                            lcd.print(charge_volts, 1);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0xE4: // ENGINE SPEED | INTAKE MANIFOLD ABS. PRESSURE
                            {
                                if (message_length > 3)
                                {
                                    uint16_t engine_speed = 0;
                                    float map_value = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        engine_speed = message[1] * 32;
                                        map_value = roundf(message[2] * 10.0 * 0.059756);
                                        map_value = map_value / 10.0;
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        engine_speed = message[1] * 32;
                                        map_value = roundf(message[2] * 10.0 * 6.894757 * 0.059756);
                                        map_value = map_value / 10.0;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(7, 0);
                                            lcd.print(engine_speed);
                                        }
                                        
                                        if (map_value < 10.0)
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 1);
                                        }
                                        else if ((map_value >= 10.0) && (map_value < 100.0))
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(map_value, 1);
                                        }
                                        else if (map_value >= 100.0)
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 0);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(engine_speed);
                                        }
                                        
                                        if (map_value < 10.0)
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 1);
                                        }
                                        else if ((map_value >= 10.0) && (map_value < 100.0))
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(map_value, 1);
                                        }
                                        else if (map_value >= 100.0)
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(engine_speed);
                                        }
                                    }
                                }
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
                case bus_pcm:
                {
                    if (bus == bus_pcm)
                    {
                        switch (message[0]) // check ID-byte
                        {
                            case 0x14: // diagnostic reponse message
                            {
                                if (message_length > 2)
                                {
                                    switch (message[1]) // parameter
                                    {
                                        case 0x01: // ambient air temperature
                                        {
                                            int ambient_temp = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                ambient_temp = message[2];
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                ambient_temp = round((message[2] * 0.555556) - 17.77778);
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (ambient_temp <= -100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((ambient_temp > -100) && (ambient_temp <= -10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp > -10) && (ambient_temp < 0))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 0) && (ambient_temp < 10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("  ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 10) && (ambient_temp < 100))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if (ambient_temp >= 100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (ambient_temp <= -100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((ambient_temp > -100) && (ambient_temp <= -10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp > -10) && (ambient_temp < 0))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 0) && (ambient_temp < 10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("  ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 10) && (ambient_temp < 100))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if (ambient_temp >= 100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x05: // engine coolant temperature
                                        {
                                            int coolant_temp = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                coolant_temp = round((message[2] * 1.8) - 198.4);
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                coolant_temp = message[2] - 128;
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (coolant_temp <= -100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp > -10) && (coolant_temp < 0))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("  ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if (coolant_temp >= 100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (coolant_temp <= -100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp > -10) && (coolant_temp < 0))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("  ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if (coolant_temp >= 100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x0A: // battery voltage
                                        {
                                            float batt_volts = roundf(message[2] * 10.0 * 0.0625);
                                            batt_volts = batt_volts / 10.0;
                                            
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (batt_volts < 10.0)
                                                {
                                                    lcd.setCursor(0, 2);
                                                    lcd.print(" ");
                                                    lcd.print(batt_volts, 1);
                                                }
                                                else if (batt_volts >= 10.0)
                                                {
                                                    lcd.setCursor(0, 2);
                                                    lcd.print(batt_volts, 1);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x0B: // intake manifold absolute pressure
                                        {
                                            float map_value = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                map_value = roundf(message[2] * 10.0 * 0.059756);
                                                map_value = map_value / 10.0;
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                map_value = roundf(message[2] * 6.894757 * 10.0 * 0.059756);
                                                map_value = map_value / 10.0;
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (map_value <= -99.0)
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" -99");
                                                }
                                                else if ((map_value > -99.0) && (map_value <= -10.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                                else if ((map_value > -10.0) && (map_value < 0.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 0.0) && (map_value < 10.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 10.0) && (map_value < 100.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if (map_value >= 100.0)
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (map_value <= -99.0)
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" -99");
                                                }
                                                else if ((map_value > -99.0) && (map_value <= -10.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                                else if ((map_value > -10.0) && (map_value < 0.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 0.0) && (map_value < 10.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 10.0) && (map_value < 100.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if (map_value >= 100.0)
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x11: // engine speed
                                        {
                                            uint16_t engine_speed = message[2] * 32;

                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(7, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            else
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            break;
                                        }
                                        case 0x24: // battery charging voltage
                                        {
                                            float charge_volts = roundf(message[2] * 10.0 * 0.0625);
                                            charge_volts = charge_volts / 10.0;
                                            
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (charge_volts < 10.0)
                                                {
                                                    lcd.setCursor(5, 2);
                                                    lcd.print(" ");
                                                    lcd.print(charge_volts, 1);
                                                }
                                                else if (charge_volts >= 10.0)
                                                {
                                                    lcd.setCursor(5, 2);
                                                    lcd.print(charge_volts, 1);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x41: // vehicle speed
                                        {
                                            uint8_t vehicle_speed = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                vehicle_speed = round(message[2] * 0.5);
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                vehicle_speed = round(message[2] * 1.609344 * 0.5 );
                                            }
                
                                            if (((lcd_char_width == 20) && (lcd_char_height == 4)) || ((lcd_char_width == 16) && (lcd_char_height == 2))) // 20x4 / 16x2 LCD
                                            {
                                                if (vehicle_speed < 10)
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print("  ");
                                                    lcd.print(vehicle_speed);
                                                }
                                                else if ((vehicle_speed >= 10) && (vehicle_speed < 100))
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(" ");
                                                    lcd.print(vehicle_speed);
                                                }
                                                else if (vehicle_speed >= 100)
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(vehicle_speed);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x46: // throttle position sensor
                                        {
                                            uint8_t tps_position = round(message[2] * 0.3922);
                    
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (tps_position < 10)
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print("  ");
                                                    lcd.print(tps_position);
                                                }
                                                else if ((tps_position >= 10) && (tps_position < 100))
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print(" ");
                                                    lcd.print(tps_position);
                                                }
                                                else if (tps_position >= 100)
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print(tps_position);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
//        }
    }
}

/*************************************************************************
Function: UART0 Receive Complete interrupt
Purpose:  called when the UART0 has received a character
**************************************************************************/
ISR(USB_RECEIVE_INTERRUPT)
{
    uint16_t tmphead;
    uint8_t data;
    uint8_t usr;
    uint8_t lastRxError;
 
    /* read UART status register and UART data register */ 
    usr  = USB_STATUS;
    data = USB_DATA;
    
    /* get error bits from status register */
    lastRxError = (usr & (_BV(FE0)|_BV(DOR0)));

    /* calculate buffer index */ 
    tmphead = (USB_RxHead + 1) & USB_RX_BUFFER_MASK;
    
    if (tmphead == USB_RxTail)
    {
        /* error: receive buffer overflow */
        lastRxError = UART_BUFFER_OVERFLOW >> 8;
    }
    else
    {
        /* store new index */
        USB_RxHead = tmphead;
        /* store received data in buffer */
        USB_RxBuf[tmphead] = data;
    }
    USB_LastRxError = lastRxError;
}

/*************************************************************************
Function: UART0 Data Register Empty interrupt
Purpose:  called when the UART0 is ready to transmit the next byte
**************************************************************************/
ISR(USB_TRANSMIT_INTERRUPT)
{
    uint16_t tmptail;

    if (USB_TxHead != USB_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (USB_TxTail + 1) & USB_TX_BUFFER_MASK;
        USB_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        USB_DATA = USB_TxBuf[tmptail]; /* start transmission */
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        USB_CONTROL &= ~_BV(USB_UDRIE);
        usb_tx_flush();
    }
}

/*************************************************************************
Function: usb_init()
Purpose:  initialize UART0 and set baudrate for USB communication,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void usb_init(uint16_t ubrr)
{
    /* reset ringbuffer */
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        USB_RxHead = 0;
        USB_RxTail = 0;
        USB_TxHead = 0;
        USB_TxTail = 0;
    }
  
    /* set baud rate */
    UBRR0H = (ubrr >> 8) & 0x0F;
    UBRR0L = ubrr & 0xFF;

    /* enable USART receiver and transmitter and receive complete interrupt */
    USB_CONTROL |= (1 << RXCIE0) | (1 << RXEN0) | (1 << TXEN0);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR0C |= (1 << UCSZ00) | (1 << UCSZ01);
}

/*************************************************************************
Function: usb_getc()
Purpose:  return byte from the receive buffer
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t usb_getc(void)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (USB_RxHead == USB_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
  
    /* calculate / store buffer index */
    tmptail = (USB_RxTail + 1) & USB_RX_BUFFER_MASK;
  
    USB_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = USB_RxBuf[tmptail];

    return (USB_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: usb_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in receive buffer
          high byte: error flags
**************************************************************************/
uint16_t usb_peek(uint16_t index)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (USB_RxHead == USB_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
  
    tmptail = (USB_RxTail + 1 + index) & USB_RX_BUFFER_MASK;

    /* get data from receive buffer */
    data = USB_RxBuf[tmptail];

    return (USB_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: usb_putc()
Purpose:  transmit byte to UART0 (USB)
Input:    byte to be transmitted
Returns:  none
**************************************************************************/
void usb_putc(uint8_t data)
{
    uint16_t tmphead;
    uint16_t txtail_tmp;

    tmphead = (USB_TxHead + 1) & USB_TX_BUFFER_MASK;

    do
    {
        ATOMIC_BLOCK(ATOMIC_FORCEON)
        {
            txtail_tmp = USB_TxTail;
        }
    }
    while (tmphead == txtail_tmp); /* wait for free space in buffer */

    USB_TxBuf[tmphead] = data;
    USB_TxHead = tmphead;

    /* enable UDRE interrupt */
    USB_CONTROL |= _BV(USB_UDRIE);
}

/*************************************************************************
Function: usb_puts()
Purpose:  transmit string to UART0 (USB)
Input:    pointer to the string to be transmitted
Returns:  none
**************************************************************************/
void usb_puts(const char *s)
{
    while(*s)
    {
        usb_putc(*s++);
    }
}

/*************************************************************************
Function: usb_puts_p()
Purpose:  transmit string from program memory to UART0 (USB)
Input:    pointer to the program memory string to be transmitted
Returns:  none
**************************************************************************/
void usb_puts_p(const char *progmem_s)
{
    register char c;

    while ((c = pgm_read_byte(progmem_s++)))
    {
        usb_putc(c);
    }
}

/*************************************************************************
Function: usb_available()
Purpose:  determine the number of bytes waiting in the receive buffer
Input:    none
Returns:  integer number of bytes in the receive buffer
**************************************************************************/
uint16_t usb_rx_available(void)
{
    uint16_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (USB_RX_BUFFER_SIZE + USB_RxHead - USB_RxTail) & USB_RX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: usb_tx_available()
Purpose:  determine the number of bytes waiting in the transmit buffer
Input:    none
Returns:  integer number of bytes in the transmit buffer
**************************************************************************/
uint16_t usb_tx_available(void)
{
    uint16_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (USB_TX_BUFFER_SIZE + USB_TxHead - USB_TxTail) & USB_TX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: usb_rx_flush()
Purpose:  flush bytes waiting in the receive buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void usb_rx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        USB_RxHead = USB_RxTail;
    }
}

/*************************************************************************
Function: usb_tx_flush()
Purpose:  flush bytes waiting in the transmit buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void usb_tx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        USB_TxHead = USB_TxTail;
    }
}

/*************************************************************************
Function: ccd_init()
Purpose:  initialize UART1 and set baudrate to conform CCD-bus requirements,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void ccd_init()
{
    CCD.onMessageReceived(handle_received_ccd_msg); // callback function when CCD-bus message is received
    CCD.onError(handle_ccd_error); // callback function when error occurs

    if (to_uint16(hw_version[0], hw_version[1]) >= 144) // hardware version V1.44 and up
    {
        CCD.begin(CCD_DEFAULT_SPEED, CUSTOM_TRANSCEIVER, IDLE_BITS_10, ENABLE_RX_CHECKSUM, ENABLE_TX_CHECKSUM);
    }
    else // hardware version below 1.44
    {
        CCD.begin(); // CDP68HC68S1
    }
}

/*************************************************************************
Function: UART2 Receive Complete interrupt
Purpose:  called when the UART2 has received a character
**************************************************************************/
ISR(PCM_RECEIVE_INTERRUPT)
{
    uint16_t tmphead;
    uint8_t data;
    uint8_t usr;
    uint8_t lastRxError;
 
    /* read UART status register and UART data register */ 
    usr  = PCM_STATUS;
    data = PCM_DATA;
    
    /* get error bits from status register */
    lastRxError = (usr & (_BV(FE2)|_BV(DOR2)));

    /* calculate buffer index */ 
    tmphead = (PCM_RxHead + 1) & PCM_RX_BUFFER_MASK;
    
    if (tmphead == PCM_RxTail)
    {
        /* error: receive buffer overflow */
        lastRxError = UART_BUFFER_OVERFLOW >> 8;
    }
    else
    {
        /* store new index */
        PCM_RxHead = tmphead;
        /* store received data in buffer */
        if (!pcm.sbec2_mode) PCM_RxBuf[tmphead] = data;
        else PCM_RxBuf[tmphead] = ((data << 4) & 0xF0) | ((data >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    PCM_LastRxError = lastRxError;

    pcm.last_byte_millis = millis();
}

/*************************************************************************
Function: UART2 Data Register Empty interrupt
Purpose:  called when the UART2 is ready to transmit the next byte
**************************************************************************/
ISR(PCM_TRANSMIT_INTERRUPT)
{
    uint16_t tmptail;

    if (PCM_TxHead != PCM_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (PCM_TxTail + 1) & PCM_TX_BUFFER_MASK;
        PCM_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        if (!pcm.sbec2_mode) PCM_DATA = PCM_TxBuf[tmptail]; /* start transmission */
        else PCM_DATA = ((PCM_TxBuf[tmptail] << 4) & 0xF0) | ((PCM_TxBuf[tmptail] >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        PCM_CONTROL &= ~_BV(PCM_UDRIE);
    }
}

/*************************************************************************
Function: pcm_init()
Purpose:  initialize UART2 and set baudrate to conform SCI-bus requirements,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void pcm_init(uint16_t ubrr)
{
    /* reset ringbuffer */
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        PCM_RxHead = 0;
        PCM_RxTail = 0;
        PCM_TxHead = 0;
        PCM_TxTail = 0;
    }
  
    /* set baud rate */
    UBRR2H = (ubrr >> 8) & 0x0F;
    UBRR2L = ubrr & 0xFF;

    /* enable USART receiver and transmitter and receive complete interrupt */
    PCM_CONTROL |= (1 << RXCIE2) | (1 << RXEN2) | (1 << TXEN2);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR2C |= (1 << UCSZ20) | (1 << UCSZ21);
}

/*************************************************************************
Function: pcm_getc()
Purpose:  return byte from the receive buffer and remove it
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t pcm_getc(void)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (PCM_RxHead == PCM_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
  
    /* calculate / store buffer index */
    tmptail = (PCM_RxTail + 1) & PCM_RX_BUFFER_MASK;
  
    PCM_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = PCM_RxBuf[tmptail];

    return (PCM_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: pcm_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t pcm_peek(uint16_t index)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (PCM_RxHead == PCM_RxTail)
        {
            return UART_RX_NO_DATA;   /* no data available */
        }
    }
  
    tmptail = (PCM_RxTail + 1 + index) & PCM_RX_BUFFER_MASK;

    /* get data from receive buffer */
    data = PCM_RxBuf[tmptail];

    return (PCM_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: pcm_putc()
Purpose:  transmit byte to the SCI-bus (PCM)
Input:    byte to be transmitted
Returns:  none
**************************************************************************/
void pcm_putc(uint8_t data)
{
    uint16_t tmphead;
    uint16_t txtail_tmp;

    tmphead = (PCM_TxHead + 1) & PCM_TX_BUFFER_MASK;

    do
    {
        ATOMIC_BLOCK(ATOMIC_FORCEON)
        {
            txtail_tmp = PCM_TxTail;
        }
    }
    while (tmphead == txtail_tmp); /* wait for free space in buffer */

    PCM_TxBuf[tmphead] = data;
    PCM_TxHead = tmphead;

    /* enable UDRE interrupt */
    PCM_CONTROL |= _BV(PCM_UDRIE);
}

/*************************************************************************
Function: pcm_puts()
Purpose:  transmit string to the SCI-bus (PCM)
Input:    pointer to the string to be transmitted
Returns:  none
**************************************************************************/
void pcm_puts(const char *s)
{
    while(*s)
    {
        pcm_putc(*s++);
    }
}

/*************************************************************************
Function: pcm_puts_p()
Purpose:  transmit string from program memory to the SCI-bus (PCM)
Input:    pointer to the program memory string to be transmitted
Returns:  none
**************************************************************************/
void pcm_puts_p(const char *progmem_s)
{
    register char c;

    while ((c = pgm_read_byte(progmem_s++)))
    {
        pcm_putc(c);
    }
}

/*************************************************************************
Function: pcm_rx_available()
Purpose:  determine the number of bytes waiting in the receive buffer
Input:    none
Returns:  integer number of bytes in the receive buffer
**************************************************************************/
uint8_t pcm_rx_available(void)
{
    uint8_t ret;
    
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (PCM_RX_BUFFER_SIZE + PCM_RxHead - PCM_RxTail) & PCM_RX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: pcm_tx_available()
Purpose:  determine the number of bytes waiting in the transmit buffer
Input:    none
Returns:  integer number of bytes in the transmit buffer
**************************************************************************/
uint8_t pcm_tx_available(void)
{
    uint8_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (PCM_TX_BUFFER_SIZE + PCM_TxHead - PCM_TxTail) & PCM_TX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: pcm_rx_flush()
Purpose:  flush bytes waiting in the receive buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void pcm_rx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        PCM_RxHead = PCM_RxTail;
    }
}

/*************************************************************************
Function: pcm_tx_flush()
Purpose:  flush bytes waiting in the transmit buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void pcm_tx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        PCM_TxHead = PCM_TxTail;
    }
}

/*************************************************************************
Function: UART3 Receive Complete interrupt
Purpose:  called when the UART3 has received a character
**************************************************************************/
ISR(TCM_RECEIVE_INTERRUPT)
{
    uint16_t tmphead;
    uint8_t data;
    uint8_t usr;
    uint8_t lastRxError;
 
    /* read UART status register and UART data register */ 
    usr  = TCM_STATUS;
    data = TCM_DATA;
    
    /* get error bits from status register */
    lastRxError = (usr & (_BV(FE3)|_BV(DOR3)));

    /* calculate buffer index */ 
    tmphead = (TCM_RxHead + 1) & TCM_RX_BUFFER_MASK;
    
    if (tmphead == TCM_RxTail)
    {
        /* error: receive buffer overflow */
        lastRxError = UART_BUFFER_OVERFLOW >> 8;
    }
    else
    {
        /* store new index */
        TCM_RxHead = tmphead;
        /* store received data in buffer */
        if (!tcm.sbec2_mode) TCM_RxBuf[tmphead] = data;
        else TCM_RxBuf[tmphead] = ((data << 4) & 0xF0) | ((data >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    TCM_LastRxError = lastRxError;

    tcm.last_byte_millis = millis();
}

/*************************************************************************
Function: UART3 Data Register Empty interrupt
Purpose:  called when the UART3 is ready to transmit the next byte
**************************************************************************/
ISR(TCM_TRANSMIT_INTERRUPT)
{
    uint16_t tmptail;

    if (TCM_TxHead != TCM_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (TCM_TxTail + 1) & TCM_TX_BUFFER_MASK;
        TCM_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        if (!tcm.sbec2_mode) TCM_DATA = TCM_TxBuf[tmptail]; /* start transmission */
        else TCM_DATA = ((TCM_TxBuf[tmptail] << 4) & 0xF0) | ((TCM_TxBuf[tmptail] >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        TCM_CONTROL &= ~_BV(TCM_UDRIE);
    }
}

/*************************************************************************
Function: tcm_init()
Purpose:  initialize UART3 and set baudrate to conform SCI-bus requirements,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void tcm_init(uint16_t ubrr)
{
    /* reset ringbuffer */
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        TCM_RxHead = 0;
        TCM_RxTail = 0;
        TCM_TxHead = 0;
        TCM_TxTail = 0;
    }
  
    /* set baud rate */
    UBRR3H = (ubrr >> 8) & 0x0F;
    UBRR3L = ubrr & 0xFF;

    /* enable USART receiver and transmitter and receive complete interrupt */
    TCM_CONTROL |= (1 << RXCIE3) | (1 << RXEN3) | (1 << TXEN3);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR3C |= (1 << UCSZ30) | (1 << UCSZ31);
}

/*************************************************************************
Function: tcm_getc()
Purpose:  return byte from the receive buffer and remove it
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t tcm_getc(void)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (TCM_RxHead == TCM_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
  
    /* calculate / store buffer index */
    tmptail = (TCM_RxTail + 1) & TCM_RX_BUFFER_MASK;
  
    TCM_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = TCM_RxBuf[tmptail];

    return (TCM_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: tcm_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t tcm_peek(uint16_t index = 0)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (TCM_RxHead == TCM_RxTail)
        {
            return UART_RX_NO_DATA;   /* no data available */
        }
    }
  
    tmptail = (TCM_RxTail + 1 + index) & TCM_RX_BUFFER_MASK;

    /* get data from receive buffer */
    data = TCM_RxBuf[tmptail];

    return (TCM_LastRxError << 8 ) + data;
}

/*************************************************************************
Function: tcm_putc()
Purpose:  transmit byte to the SCI-bus (TCM)
Input:    byte to be transmitted
Returns:  none
**************************************************************************/
void tcm_putc(uint8_t data)
{
    uint16_t tmphead;
    uint16_t txtail_tmp;

    tmphead = (TCM_TxHead + 1) & TCM_TX_BUFFER_MASK;

    do
    {
        ATOMIC_BLOCK(ATOMIC_FORCEON)
        {
            txtail_tmp = TCM_TxTail;
        }
    }
    while (tmphead == txtail_tmp); /* wait for free space in buffer */

    TCM_TxBuf[tmphead] = data;
    TCM_TxHead = tmphead;

    /* enable UDRE interrupt */
    TCM_CONTROL |= _BV(TCM_UDRIE);
}

/*************************************************************************
Function: tcm_puts()
Purpose:  transmit string to the SCI-bus (TCM)
Input:    pointer to the string to be transmitted
Returns:  none
**************************************************************************/
void tcm_puts(const char *s)
{
    while(*s)
    {
        tcm_putc(*s++);
    }
}

/*************************************************************************
Function: tcm_puts_p()
Purpose:  transmit string from program memory to the SCI-bus (TCM)
Input:    pointer to the program memory string to be transmitted
Returns:  none
**************************************************************************/
void tcm_puts_p(const char *progmem_s)
{
    register char c;

    while ((c = pgm_read_byte(progmem_s++)))
    {
        tcm_putc(c);
    }
}

/*************************************************************************
Function: tcm_rx_available()
Purpose:  determine the number of bytes waiting in the receive buffer
Input:    none
Returns:  integer number of bytes in the receive buffer
**************************************************************************/
uint8_t tcm_rx_available(void)
{
    uint8_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (TCM_RX_BUFFER_SIZE + TCM_RxHead - TCM_RxTail) & TCM_RX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: tcm_tx_available()
Purpose:  determine the number of bytes waiting in the transmit buffer
Input:    none
Returns:  integer number of bytes in the transmit buffer
**************************************************************************/
uint8_t tcm_tx_available(void)
{
    uint8_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (TCM_TX_BUFFER_SIZE + TCM_TxHead - TCM_TxTail) & TCM_TX_BUFFER_MASK;
    }
    return ret;
}

/*************************************************************************
Function: tcm_rx_flush()
Purpose:  flush bytes waiting in the receive buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void tcm_rx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        TCM_RxHead = TCM_RxTail;
    }
}

/*************************************************************************
Function: tcm_tx_flush()
Purpose:  flush bytes waiting in the transmit buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void tcm_tx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        TCM_TxHead = TCM_TxTail;
    }
}

/*************************************************************************
Function: configure_ccd_bus()
Purpose:  as the name says
Input:    setting bits
Note:     setting bits description:
          B7: state bit:     0: disabled
                             1: enabled
          B6: tben bit:      0: disabled
                             1: enabled
          B5: not used:      0: always clear
          B4: not used:      0: always clear
          B3: not used:      0: always clear
          B2: not used:      0: always clear
          B1: not used:      0: always clear
          B0: not used:      1: always set
**************************************************************************/
void configure_ccd_bus(uint8_t ccd_settings)
{
    cbi(ccd_settings, 5);
    cbi(ccd_settings, 4);
    cbi(ccd_settings, 3);
    cbi(ccd_settings, 2);
    cbi(ccd_settings, 1);
    sbi(ccd_settings, 0);

    if (ccd_settings & (1 << CCD_STATE_BIT))
    {
        ccd.enabled = true;
    }
    else
    {
        ccd.enabled = false;
    }

    if (ccd.enabled)
    {
        if (ccd_settings & (1 << CCD_TBEN_BIT))
        {
            ccd.termination_bias_enabled = true;
            digitalWrite(TBEN, LOW);
        }
        else
        {
            ccd.termination_bias_enabled = false;
            digitalWrite(TBEN, HIGH);
        }
    }

    ccd.bus_settings = ccd_settings;
    EEPROM.update(0x23, ccd.bus_settings); // write CCD-bus settings
    uint8_t ret[1] = { ccd.bus_settings };
    send_usb_packet(bus_usb, settings, set_ccd_bus, ret, sizeof(ret)); // acknowledge
}

/*************************************************************************
Function: configure_sci_bus()
Purpose:  as the name says
Input:    setting bits
Note:     setting bits description:
          B7: state bit:     0: disabled
                             1: enabled
          B6: sbec2 bit:     0: do not exchange first and last 4-bits
                             1: exchange first and last 4-bits
          B5: module bit:    0: PCM (engine)
                             1: TCM (transmission)
          B4: ngc bit:       0: NGC mode off
                             1: NGC mode on
          B3: logic bit:     0: non-inverted
                             1: inverted
          B2: config bit:    0: A-configuration
                             1: B-configuration
          B1:0: speed bits: 00: 976.5 baud
                            01: 7812.5 baud
                            10: 62500 baud
                            11: 125000 baud
**************************************************************************/
void configure_sci_bus(uint8_t sci_settings)
{
    if (sci_settings & (1 << SCI_MODULE_BIT)) // TCM
    {
        if (sci_settings & (1 << SCI_SBEC2_BIT))
        {
            tcm.sbec2_mode = true;
        }
        else
        {
            tcm.sbec2_mode = false;
        }
        
        if (sci_settings & (1 << SCI_STATE_BIT))
        {
            tcm.enabled = true;
            pcm.enabled = false;
        }
        else
        {
            tcm.enabled = false;

            // Disable all TCM switches.
            digitalWrite(A_TCM_RX_EN, LOW); // SCI-BUS_A_TCM_RX disabled
            digitalWrite(A_TCM_TX_EN, LOW); // SCI-BUS_A_TCM_TX disabled
            digitalWrite(B_TCM_RX_EN, LOW); // SCI-BUS_B_TCM_RX disabled
            digitalWrite(B_TCM_TX_EN, LOW); // SCI-BUS_B_TCM_TX disabled
        }

        if (tcm.enabled)
        {
            if (sci_settings & (1 << SCI_NGC_BIT))
            {
                tcm.ngc_mode = true;
            }
            else
            {
                tcm.ngc_mode = false;
            }

            // Disable all PCM switches.
            digitalWrite(A_PCM_RX_EN, LOW);  // SCI-BUS_A_PCM_RX disabled
            digitalWrite(A_PCM_TX_EN, LOW);  // SCI-BUS_A_PCM_TX disabled
            digitalWrite(B_PCM_RX_EN, LOW);  // SCI-BUS_B_PCM_RX disabled
            digitalWrite(B_PCM_TX_EN, LOW);  // SCI-BUS_B_PCM_TX disabled

            if (sci_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                digitalWrite(A_TCM_RX_EN, LOW);  // SCI-BUS_A_TCM_RX disabled
                digitalWrite(A_TCM_TX_EN, LOW);  // SCI-BUS_A_TCM_TX disabled
                digitalWrite(B_TCM_RX_EN, HIGH); // SCI-BUS_B_TCM_RX enabled
                digitalWrite(B_TCM_TX_EN, HIGH); // SCI-BUS_B_TCM_TX enabled
            }
            else // A-configuration
            {
                digitalWrite(B_TCM_RX_EN, LOW);  // SCI-BUS_B_TCM_RX disabled
                digitalWrite(B_TCM_TX_EN, LOW);  // SCI-BUS_B_TCM_TX disabled
                digitalWrite(A_TCM_RX_EN, HIGH); // SCI-BUS_A_TCM_RX enabled
                digitalWrite(A_TCM_TX_EN, HIGH); // SCI-BUS_A_TCM_TX enabled
            }

            if (sci_settings & (1 << SCI_LOGIC_BIT)) // inverted logic
            {
                tcm.inverted_logic = true;
            }
            else // non-inverted logic
            {
                tcm.inverted_logic = false;
            }

            switch (sci_settings & SCI_SPEED_BITS)
            {
                case SCI_SPEED_BAUD_976:
                {
                    tcm_init(BAUD_976);
                    //tcm_rx_flush();
                    //tcm_tx_flush();
                    tcm.speed = BAUD_976;
                    break;
                }
                case SCI_SPEED_BAUD_7812:
                {
                    tcm_init(BAUD_7812);
                    //tcm_rx_flush();
                    //tcm_tx_flush();
                    tcm.speed = BAUD_7812;
                    break;
                }
                case SCI_SPEED_BAUD_62500:
                {
                    tcm_init(BAUD_62500);
                    //tcm_rx_flush();
                    //tcm_tx_flush();
                    tcm.speed = BAUD_62500;
                    break;
                }
                case SCI_SPEED_BAUD_125000:
                {
                    tcm_init(BAUD_125000);
                    //tcm_rx_flush();
                    //tcm_tx_flush();
                    tcm.speed = BAUD_125000;
                    break;
                }
            }
        }

        tcm.bus_settings = sci_settings;
        EEPROM.update(0x25, tcm.bus_settings);
        cbi(pcm.bus_settings, SCI_STATE_BIT);
        EEPROM.update(0x24, pcm.bus_settings);
        uint8_t ret[1] = { tcm.bus_settings };
        send_usb_packet(bus_usb, settings, set_sci_bus, ret, sizeof(ret));
    }
    else // PCM
    {
        if (sci_settings & (1 << SCI_SBEC2_BIT))
        {
            pcm.sbec2_mode = true;
        }
        else
        {
            pcm.sbec2_mode = false;
        }

        if (sci_settings & (1 << SCI_STATE_BIT))
        {
            pcm.enabled = true;
            tcm.enabled = false;
        }
        else
        {
            pcm.enabled = false;

            // Disable all PCM switches.
            digitalWrite(A_PCM_RX_EN, LOW); // SCI-BUS_A_PCM_RX disabled
            digitalWrite(A_PCM_TX_EN, LOW); // SCI-BUS_A_PCM_TX disabled
            digitalWrite(B_PCM_RX_EN, LOW); // SCI-BUS_B_PCM_RX disabled
            digitalWrite(B_PCM_TX_EN, LOW); // SCI-BUS_B_PCM_TX disabled
        }

        if (pcm.enabled)
        {
            if (sci_settings & (1 << SCI_NGC_BIT))
            {
                pcm.ngc_mode = true;
            }
            else
            {
                pcm.ngc_mode = false;
            }

            // Disable all TCM switches.
            digitalWrite(A_TCM_RX_EN, LOW);  // SCI-BUS_A_TCM_RX disabled
            digitalWrite(A_TCM_TX_EN, LOW);  // SCI-BUS_A_TCM_TX disabled
            digitalWrite(B_TCM_RX_EN, LOW);  // SCI-BUS_B_TCM_RX disabled
            digitalWrite(B_TCM_TX_EN, LOW);  // SCI-BUS_B_TCM_TX disabled

            if (sci_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                digitalWrite(A_PCM_RX_EN, LOW);  // SCI-BUS_A_PCM_RX disabled
                digitalWrite(A_PCM_TX_EN, LOW);  // SCI-BUS_A_PCM_TX disabled
                digitalWrite(B_PCM_RX_EN, HIGH); // SCI-BUS_B_PCM_RX enabled
                digitalWrite(B_PCM_TX_EN, HIGH); // SCI-BUS_B_PCM_TX enabled
            }
            else // A-configuration
            {
                digitalWrite(B_PCM_RX_EN, LOW);  // SCI-BUS_B_PCM_RX disabled
                digitalWrite(B_PCM_TX_EN, LOW);  // SCI-BUS_B_PCM_TX disabled
                digitalWrite(A_PCM_RX_EN, HIGH); // SCI-BUS_A_PCM_RX enabled
                digitalWrite(A_PCM_TX_EN, HIGH); // SCI-BUS_A_PCM_TX enabled
            }

            if (sci_settings & (1 << SCI_LOGIC_BIT)) // inverted logic
            {
                pcm.inverted_logic = true;
            }
            else // non-inverted logic
            {
                pcm.inverted_logic = false;
            }

            switch (sci_settings & SCI_SPEED_BITS)
            {
                case SCI_SPEED_BAUD_976:
                {
                    pcm_init(BAUD_976);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = BAUD_976;
                    break;
                }
                case SCI_SPEED_BAUD_7812:
                {
                    pcm_init(BAUD_7812);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = BAUD_7812;
                    break;
                }
                case SCI_SPEED_BAUD_62500:
                {
                    pcm_init(BAUD_62500);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = BAUD_62500;
                    break;
                }
                case SCI_SPEED_BAUD_125000:
                {
                    pcm_init(BAUD_125000);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = BAUD_125000;
                    break;
                }
            }
        }

        pcm.bus_settings = sci_settings;
        EEPROM.update(0x24, pcm.bus_settings);
        cbi(tcm.bus_settings, SCI_STATE_BIT);
        EEPROM.update(0x25, tcm.bus_settings);
        uint8_t ret[1] = { pcm.bus_settings };
        send_usb_packet(bus_usb, settings, set_sci_bus, ret, sizeof(ret));
    }
}

/*************************************************************************
Function: update_timestamp()
Purpose:  get elapsed milliseconds since power up/reset from microcontroller and convert it to an array containing 4 bytes
Note:     this function updates a global byte array which can be read from anywhere in the code
**************************************************************************/
void update_timestamp(uint8_t *target)
{
    current_millis = millis();
    target[0] = (current_millis >> 24) & 0xFF;
    target[1] = (current_millis >> 16) & 0xFF;
    target[2] = (current_millis >> 8) & 0xFF;
    target[3] = current_millis & 0xFF;
}

/*************************************************************************
Function: array_contains()
Purpose:  checks if a value is present in an array
Note:     only checks first occurence
**************************************************************************/
bool array_contains(uint8_t *src_array, uint16_t src_array_length, uint8_t value)
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
    uint8_t ret[32];
                                    
    ret[0] = hw_version[0];
    ret[1] = hw_version[1];
    ret[2] = hw_version[2];
    ret[3] = hw_version[3];
    
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

    send_usb_packet(bus_usb, response, hwfw_info, ret, 32);
}

/*************************************************************************
Function: reset_scanner()
Purpose:  puts the scanner in an infinite loop and forces watchdog-reset
**************************************************************************/
void reset_scanner(void)
{
    send_usb_packet(bus_usb, reset, reset_in_progress, ack, 1); // RX LED is on by now and this function lights up TX LED
    digitalWrite(ACT_LED, LOW); // blink all LEDs including this, good way to test if LEDs are working or not
    while (true); // enter into an infinite loop; watchdog timer doesn't get reset this way so it restarts the program eventually
}

/*************************************************************************
Function: send_handshake()
Purpose:  sends handshake message to laptop
**************************************************************************/
void send_handshake(void)
{
    uint8_t handshake_array[21] = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 }; // CHRYSLERCCDSCISCANNER
    send_usb_packet(bus_usb, handshake, ok, handshake_array, 21);
}

/*************************************************************************
Function: send_status()
Purpose:  sends status message to laptop
**************************************************************************/
void send_status(void)
{
    uint8_t scanner_status[53];
    uint16_t free_ram_available = 0;

    scanner_status[0] = avr_signature[0];
    scanner_status[1] = avr_signature[1];
    scanner_status[2] = avr_signature[2];

    if (eep_present) scanner_status[3] = 0x01;
    else scanner_status[3] = 0x00;
    scanner_status[4] = 0; // eep checksum
    scanner_status[5] = 0; // calculated eep checksum

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

    send_usb_packet(bus_usb, status, ok, scanner_status, 53);
}

/*************************************************************************
Function: check_battery_volts()
Purpose:  measure voltage of the OBD16 pin through a resistor divider
**************************************************************************/
void check_battery_volts(void)
{
    battery_adc = 0;
    
    for (uint16_t i = 0; i < 128; i++) // get 128 samples in quick succession
    {
        battery_adc += analogRead(BATT);
    }
    
    battery_adc /= 128; // divide the sum by 128 to get average value
    battery_volts = (uint16_t)(battery_adc*(adc_supply_voltage/100.0)/adc_max_value*((double)rd_high_value+(double)rd_low_value)/(double)rd_low_value*1000.0); // resistor divider equation
    
    if (battery_volts < 6000) // battery_volts < 6V
    {
        battery_volts_array[0] = 0; // workaround if scanner's power switch is at OFF position and analog pin is floating
        battery_volts_array[1] = 0;
        connected_to_vehicle = false;
    }
    else // battery_volts >= 6V
    {
        battery_volts_array[0] = (battery_volts >> 8) & 0xFF;
        battery_volts_array[1] = battery_volts & 0xFF;
        connected_to_vehicle = true;
    }
}

/*************************************************************************
Function: send_usb_packet()
Purpose:  assemble and send data packet through serial link (UART0)
Inputs:   - one bus byte,
          - one DATA CODE command value byte,
          - one SUB-DATA CODE byte,
          - name of the PAYLOAD array (it must be previously filled with data),
          - PAYLOAD length
Returns:  none
Note:     SYNC, LENGTH and CHECKSUM bytes are calculated automatically;
          Payload can be omitted if a (uint8_t*)0x00 value is used in conjunction with 0 length
**************************************************************************/
void send_usb_packet(uint8_t bus, uint8_t command, uint8_t subdatacode, uint8_t *payload, uint16_t payload_length)
{
    uint16_t packet_length = 0;

    if (bus > 0)
    {
        packet_length = payload_length + 6 + 4; // add 4 timestamp bytes
    }
    else
    {
        packet_length = payload_length + 6;
    }

    uint8_t packet[packet_length];
    uint8_t datacode = 0;
    uint8_t checksum = 0;

    // Assemble datacode from the first 2 input parameters.
    // Leftmost byte is always set to indicate packet is coming from the scanner.
    datacode = (1 << 7) + ((bus << 4) & 0x70) + (command & 0x0F);
    //         10000000 + 1xxx0000            + 1xxxyyyy          =  1xxxyyyy  

    packet[0] = PACKET_SYNC_BYTE; // add SYNC byte
    packet[1] = ((packet_length - 4) >> 8) & 0xFF; // add LENGTH high byte
    packet[2] = (packet_length - 4) & 0xFF; // add LENGTH low byte
    packet[3] = datacode; // add DATA CODE byte
    packet[4] = subdatacode; // add SUB-DATA CODE byte
    
    // If there are payload bytes add them after subdatacode as well.
    if (payload_length > 0)
    {
        if (bus > 0) // add timestamp for CCD/SCI-bus messages
        {
            update_timestamp(current_timestamp);

            for (uint8_t i = 0; i < 4; i++)
            {
                packet[5 + i] = current_timestamp[i];
            }

            for (uint16_t i = 0; i < payload_length; i++)
            {
                packet[9 + i] = payload[i];
            }
        }
        else // no timestamp for internal messages from scanner
        {
            for (uint16_t i = 0; i < payload_length; i++)
            {
                packet[5 + i] = payload[i];
            }
        }
    }

    // Calculate checksum.
    checksum = calculate_checksum(packet, 0, packet_length - 1);

    // Place checksum byte.
    packet[packet_length - 1] = checksum;

    blink_led(TX_LED);

    // Send the prepared packet through serial link
    for (uint16_t k = 0; k < packet_length; k++)
    {
        usb_putc(packet[k]); // write every byte in the packet to the usb-serial port
    }
}

/*************************************************************************
Function: handle_usb_data()
Purpose:  handle USB commands coming from an external computer
Note:     PACKET_SYNC_BYTE:
          [ SYNC | LENGTH_HB | LENGTH_LB | DATACODE | SUBDATACODE | <?PAYLOAD?> | CHECKSUM ]
          More on this in ChryslerCCDSCIScanner_UART_Protocol.pdf
          
          ASCII_SYNC_BYTE:
          >COMMAND PARAMETER_1 PARAMETER_2 ... PARAMETER_N
**************************************************************************/
void handle_usb_data(void)
{
    if (usb_rx_available() > 0) // proceed only if the receive buffer contains at least 1 byte
    {
        blink_led(RX_LED); // blink RX-LED
        uint8_t sync = usb_getc() & 0xFF; // read the next available byte in the USB receive buffer, it's supposed to be the first byte of a message
        uint32_t command_timeout_start = 0;
        bool command_timeout_reached = false;

        switch (sync)
        {
            case PACKET_SYNC_BYTE: // 0x3D, "=" symbol
            {
                uint8_t length_hb, length_lb, datacode, subdatacode, checksum;
                bool payload_bytes = false;
                uint16_t bytes_to_read = 0;
                uint16_t payload_length = 0;
                uint8_t calculated_checksum = 0;

                // Wait for the length bytes to arrive (2 bytes)
                command_timeout_start = millis(); // save current time
                while ((usb_rx_available() < 2) && !command_timeout_reached)
                {
                    if (millis() - command_timeout_start > command_purge_timeout) command_timeout_reached = true;
                }
                if (command_timeout_reached)
                {
                    send_usb_packet(bus_usb, ok_error, error_packet_timeout_occurred, err, 1);
                    return;
                }

                length_hb = usb_getc() & 0xFF; // read first length byte
                length_lb = usb_getc() & 0xFF; // read second length byte

                // Calculate how much more bytes should we read by combining the two length bytes into a word.
                bytes_to_read = to_uint16(length_hb, length_lb) + 1; // +1 CHECKSUM byte

                // Calculate the exact size of the payload.
                payload_length = (uint16_t)bytes_to_read - 3; // in this case we have to be careful not to count data code byte, sub-data code byte and checksum byte

                // Maximum packet length is 1024 bytes; can't accept larger packets 
                // and can't accept packet without datacode and subdatacode.
                if ((payload_length > MAX_PAYLOAD_LENGTH) || ((bytes_to_read - 1) < 2))
                {
                    send_usb_packet(bus_usb, ok_error, error_length_invalid_value, err, 1);
                    return; // exit, let the loop call this function again
                }

                // Wait here until all of the expected bytes are received or timeout occurs.
                command_timeout_start = millis();
                while ((usb_rx_available() < bytes_to_read) && !command_timeout_reached) 
                {
                    if (millis() - command_timeout_start > command_timeout) command_timeout_reached = true;
                }
                if (command_timeout_reached)
                {
                    send_usb_packet(bus_usb, ok_error, error_packet_timeout_occurred, err, 1);
                    return; // exit, let the loop call this function again
                }

                // There's at least one full command in the buffer now.
                // Go ahead and read one DATA CODE byte (next in the row).
                datacode = usb_getc() & 0xFF;

                // Read one SUB-DATA CODE byte that's following.
                subdatacode = usb_getc() & 0xFF;

                // Make some space for the payload bytes (even if there is none).
                uint8_t cmd_payload[payload_length];

                // If the payload length is greater than zero then read those bytes too.
                if (payload_length > 0)
                {
                    // Read all the PAYLOAD bytes
                    for (uint16_t i = 0; i < payload_length; i++)
                    {
                        cmd_payload[i] = usb_getc() & 0xFF;
                    }
                    // And set flag so the rest of the code knows.
                    payload_bytes = true;
                }
                // Set flag if there are no PAYLOAD bytes available.
                else payload_bytes = false;

                // Read last CHECKSUM byte.
                checksum = usb_getc() & 0xFF;

                // Verify the received packet by calculating what the checksum byte should be.
                calculated_checksum = PACKET_SYNC_BYTE + length_hb + length_lb + datacode + subdatacode; // add the first few bytes together manually

                // Add payload bytes here together if present
                if (payload_bytes)
                {
                    for (uint16_t j = 0; j < payload_length; j++)
                    {
                        calculated_checksum += cmd_payload[j];
                    }
                }

                // Compare calculated checksum to the received CHECKSUM byte
                if (calculated_checksum != checksum) // if they are not the same
                {
                    send_usb_packet(bus_usb, ok_error, error_packet_checksum_invalid_value, err, 1);
                    return; // exit, let the loop call this function again
                }

                // If everything is good then continue processing the packet...
                // Find out the source and the target of the packet by examining the DATA CODE byte's high nibble (upper 4 bits).
                uint8_t bus = (datacode >> 4) & 0x07; // keep the upper three bits

                // Extract command value from the low nibble (lower 4 bits).
                uint8_t command = datacode & 0x0F;

                // Source is ignored, the packet must come from an external computer through USB
                switch (bus) // evaluate bus value
                {
                    case bus_usb: // 0x00 - scanner is the target
                    {
                        switch (command) // evaluate command
                        {
                            case reset: // 0x00 - reset scanner request
                            {
                                reset_scanner();
                                break; // not necessary but every case needs a break
                            }
                            case handshake: // 0x01 - handshake request coming from an external computer
                            {
                                switch (subdatacode)
                                {
                                    case 0x00:
                                    {
                                        send_handshake();
                                        break;
                                    }
                                    case 0x01:
                                    {
                                        send_handshake();
                                        send_hwfw_info();
                                        send_status();
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1); // send error packet back to the laptop  
                                        break;
                                    }
                                }
                                break;
                            }
                            case status: // 0x02 - status report request
                            {
                                send_status();
                                break;
                            }
                            case settings: // 0x03 - change scanner settings
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case heartbeat: // 0x01 - ACT_LED flashing interval is stored in payload as 4 bytes
                                    {
                                        if (!payload_bytes || (payload_length < 4)) // at least 4 bytes are necessary to change this setting
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint16_t flashing_interval = to_uint16(cmd_payload[0], cmd_payload[1]); // 0-65535 milliseconds
                                        heartbeat_interval = flashing_interval;
                                        if (heartbeat_interval == 0) heartbeat_enabled = false; // zero value is allowed, meaning no heartbeat
                                        else heartbeat_enabled = true;
                                        
                                        uint16_t blink_duration = to_uint16(cmd_payload[2], cmd_payload[3]); // 0-65535 milliseconds
                                        if (blink_duration > 0) led_blink_duration = blink_duration; // zero value is not allowed, this applies to all 3 status leds! (rx, tx, act)
                                        else
                                        {
                                            led_blink_duration = 50; // ms, default value
                                            cmd_payload[2] = 0x00;
                                            cmd_payload[3] = 0x32;
                                        }

                                        EEPROM.update(0x1F, cmd_payload[0]); // write LED heartbeat interval high byte
                                        EEPROM.update(0x20, cmd_payload[1]); // write LED heartbeat interval low byte
                                        EEPROM.update(0x21, cmd_payload[2]); // write LED blink duration high byte
                                        EEPROM.update(0x22, cmd_payload[3]); // write LED blink duration low byte

                                        uint8_t ret[4] = { cmd_payload[0], cmd_payload[1], cmd_payload[2], cmd_payload[3] };
                                        send_usb_packet(bus_usb, settings, heartbeat, ret, 4); // acknowledge
                                        break;
                                    }
                                    case set_ccd_bus: // 0x02 - bus state and termination/bias setting
                                    {
                                        if (!payload_bytes) // at least 1 byte is necessary to change this setting
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        configure_ccd_bus(cmd_payload[0]); // pass settings to the configuration function
                                        break;
                                    }
                                    case set_sci_bus: // 0x03 - ON-OFF state, A/B configuration and speed are stored in payload
                                    {
                                        if (!payload_bytes) // at least 1 byte is necessary to change this setting
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        configure_sci_bus(cmd_payload[0]); // pass settings to the configuration function
                                        break;
                                    }
                                    case set_repeat_behavior: // 0x04
                                    {
                                        if (!payload_bytes || (payload_length < 3))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        switch (cmd_payload[0])
                                        {
                                            case 0x01: // CCD-bus
                                            {
                                                ccd.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                break;
                                            }
                                            case 0x02: // SCI-bus (PCM)
                                            {
                                                pcm.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                break;
                                            }
                                            case 0x03: // SCI-bus (TCM)
                                            {
                                                tcm.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error, already enabled
                                                break;
                                            }
                                        }

                                        uint8_t ret[3] = { cmd_payload[0], cmd_payload[1], cmd_payload[2] };
                                        send_usb_packet(bus_usb, settings, set_repeat_behavior, ret, 3); // acknowledge
                                        break;
                                    }
                                    case set_lcd: // 0x05 - LCD settings
                                    {
                                        if (!payload_bytes || (payload_length < 7))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        switch (cmd_payload[0])
                                        {
                                            case 0x00:
                                            {
                                                lcd_enabled = false;
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                lcd_enabled = true;
                                                break;
                                            }
                                            default:
                                            {
                                                lcd_enabled = false;
                                                break;
                                            }
                                        }

                                        lcd_i2c_address = cmd_payload[1];
                                        lcd_char_width = cmd_payload[2];
                                        lcd_char_height = cmd_payload[3];
                                        lcd_refresh_rate = cmd_payload[4];
                                        lcd_update_interval = 1000 / lcd_refresh_rate; // ms
                                        lcd_units = cmd_payload[5];
                                        lcd_data_source = cmd_payload[6];

                                        EEPROM.update(0x18, cmd_payload[0]); // write LCD enabled/disabled state
                                        EEPROM.update(0x19, lcd_i2c_address); // write LCD I2C address
                                        EEPROM.update(0x1A, lcd_char_width); // write LCD width
                                        EEPROM.update(0x1B, lcd_char_height); // write LCD height
                                        EEPROM.update(0x1C, lcd_refresh_rate); // write LCD refresh rate
                                        EEPROM.update(0x1D, lcd_units); // write LCD units
                                        EEPROM.update(0x1E, lcd_data_source); // write LCD units

                                        lcd_init();

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
                                            }

                                            lcd_splash_screen_on = false;
                                        }
                                        
                                        uint8_t ret[7] = { cmd_payload[0], lcd_i2c_address, lcd_char_width, lcd_char_height, lcd_refresh_rate, lcd_units, lcd_data_source };
                                        send_usb_packet(bus_usb, settings, set_lcd, ret, 7); // acknowledge
                                        break;
                                    }
                                    default: // other values are not used
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case request: // 0x04 - request from the scanner
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case hwfw_info: // 0x01 - hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
                                    {
                                        send_hwfw_info();
                                        break;
                                    }
                                    case timestamp: // 0x02 - timestamp / MCU counter value (milliseconds elapsed)
                                    {
                                        update_timestamp(current_timestamp); // this function updates the global byte array "current_timestamp" with the current time
                                        send_usb_packet(bus_usb, response, timestamp, current_timestamp, 4);
                                        break;
                                    }
                                    case battery_voltage: // 0x03
                                    {
                                        check_battery_volts();
                                        send_usb_packet(bus_usb, response, battery_voltage, battery_volts_array, 2);
                                        break;
                                    }
                                    case ccd_bus_voltages: // 0x05
                                    {
                                        check_ccd_volts();
                                        send_usb_packet(bus_usb, response, ccd_bus_voltages, ccd_volts_array, 4);
                                        break;
                                    }
                                    default: // other values are not used
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case debug: // 0x0E - debug
                            {
                                switch (subdatacode)
                                {
                                    case random_ccd_msg: // 0x01 - broadcast random CCD-bus messages
                                    {
                                        if (!payload_bytes || (payload_length < 5))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        switch (cmd_payload[0])
                                        {
                                            case 0x00:
                                            {
                                                ccd.random_msg = false;
                                                ccd.msg_to_transmit_count = 0;
                                                ccd.random_msg_interval_min = 0;
                                                ccd.random_msg_interval_max = 0;
                                                ccd.random_msg_interval = 0;

                                                uint8_t ret[1] = { cmd_payload[0] };
                                                send_usb_packet(bus_usb, debug, random_ccd_msg, ret, 1); // acknowledge
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                ccd.random_msg = true;
                                                ccd.msg_to_transmit_count = 1;
                                                ccd.random_msg_interval_min = to_uint16(cmd_payload[1], cmd_payload[2]);
                                                ccd.random_msg_interval_max = to_uint16(cmd_payload[3], cmd_payload[4]);
                                                ccd.random_msg_interval = random(ccd.random_msg_interval_min, ccd.random_msg_interval_max);

                                                uint8_t ret[1] = { cmd_payload[0] };
                                                send_usb_packet(bus_usb, debug, random_ccd_msg, ret, 1); // acknowledge
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    case read_inteeprom_byte: // 0x02 - read internal EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);

                                        if (offset > 4095)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }
                                        
                                        uint8_t value = eeprom_read_byte((const uint8_t*)offset);
                                        uint8_t payload[4] = { 0x00, offset_hb, offset_lb, value };
                                        send_usb_packet(bus_usb, debug, read_inteeprom_byte, payload, 4); // send internal EEPROM value back to the laptop
                                        break;
                                    }
                                    case read_inteeprom_block: // 0x03 - read internal EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);
                                        uint8_t count_hb = cmd_payload[2];
                                        uint8_t count_lb = cmd_payload[3];
                                        uint16_t count = to_uint16(count_hb, count_lb);

                                        if ((offset + (count - 1)) > 4095)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }

                                        uint8_t values[count];

                                        eeprom_read_block((void *)values, (const void *)offset, count);

                                        uint16_t p_length = count + 3;
                                        uint8_t payload[p_length] = { 0x00, offset_hb, offset_lb };
                                        
                                        for (uint16_t i = 0; i < count; i++)
                                        {
                                            payload[3 + i] = values[i];
                                        }

                                        send_usb_packet(bus_usb, debug, read_inteeprom_block, payload, p_length); // send internal EEPROM block back to the laptop
                                        break;
                                    }
                                    case read_exteeprom_byte: // 0x04 - read external EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);

                                            if (offset > 4095)
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }
                                            
                                            uint8_t value = eep.read(offset);
                                            uint8_t payload[4] = { 0x00, offset_hb, offset_lb, value };
                                            send_usb_packet(bus_usb, debug, read_exteeprom_byte, payload, 4); // send external EEPROM value back to the laptop
                                        }
                                        else
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case read_exteeprom_block: // 0x05 - read external EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);
                                            uint8_t count_hb = cmd_payload[2];
                                            uint8_t count_lb = cmd_payload[3];
                                            uint16_t count = to_uint16(count_hb, count_lb);

                                            if ((offset + (count - 1)) > 4095)
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }

                                            uint8_t values[count];
                                            uint8_t success = eep.read(offset, values, count);

                                            if (success != 0)
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }

                                            uint16_t p_length = count + 3;
                                            uint8_t payload[payload_length] = { 0x00, offset_hb, offset_lb };
                                            
                                            for (uint16_t i = 0; i < count; i++)
                                            {
                                                payload[3 + i] = values[i];
                                            }

                                            send_usb_packet(bus_usb, debug, read_exteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                        }
                                        else
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case write_inteeprom_byte: // 0x06 - write internal EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 3))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);

                                        if (offset > 4095)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }
                                        
                                        uint8_t value = cmd_payload[2];
                                        eeprom_update_byte((uint8_t *)offset, value);

                                        uint8_t reading = eeprom_read_byte((const uint8_t*)offset);
                                        uint8_t payload[4] = { 0x00, offset_hb, offset_lb, reading };
                                        send_usb_packet(bus_usb, debug, write_inteeprom_byte, payload, 4); // send internal EEPROM value back to the laptop for confirmation
                                        break;
                                    }
                                    case write_inteeprom_block: // 0x07 - write internal EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);
                                        uint16_t count = payload_length - 2;

                                        if ((offset + (count - 1)) > 4095)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }

                                        uint8_t values[count];

                                        for (uint16_t i = 0; i < count; i++)
                                        {
                                            values[i] = cmd_payload[2 + i];
                                        }

                                        eeprom_update_block((const void *)values, (void *)offset, count);
                                        eeprom_read_block((void *)values, (const void *)offset, count); // overwrite array with read values;
                                        
                                        uint16_t p_length = count + 3;
                                        uint8_t payload[p_length] = { 0x00, offset_hb, offset_lb };

                                        for (uint16_t i = 0; i < count; i++)
                                        {
                                            payload[3 + i] = values[i];
                                        }

                                        send_usb_packet(bus_usb, debug, write_inteeprom_block, payload, p_length); // send internal EEPROM block back to the laptop
                                        break;
                                    }
                                    case write_exteeprom_byte: // 0x08 - write external EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 3))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);

                                            if (offset > 4095)
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_write, err, 1);
                                                break;
                                            }
                                            
                                            uint8_t value = cmd_payload[2];

                                            // Disable hardware write protection (EEPROM chip has a pullup resistor on its WP-pin!)
                                            DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                            PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection

                                            eep_result = eep.write(offset, value);

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                            }

                                            PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection

                                            uint8_t reading = eep.read(offset);
                                            uint8_t payload[4] = { 0x00, offset_hb, offset_lb, reading };
                                            send_usb_packet(bus_usb, debug, write_exteeprom_byte, payload, 4); // send external EEPROM value back to the laptop for confirmation
                                        }
                                        else
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case write_exteeprom_block: // 0x09 - write external EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);
                                            uint16_t count = payload_length - 2;
    
                                            if ((offset + (count - 1)) > 4095)
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_write, err, 1);
                                                break;
                                            }
    
                                            uint8_t values[count];
    
                                            for (uint16_t i = 0; i < count; i++)
                                            {
                                                values[i] = cmd_payload[2 + i];
                                            }

                                            // Disable hardware write protection (EEPROM chip has a pullup resistor on its WP-pin!)
                                            DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                            PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection
    
                                            eep_result = eep.write(offset, values, count);

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                            }

                                            PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection
                                            
                                            eep_result = eep.read(offset, values, count); // overwrite array with read values;

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(bus_usb, ok_error, error_eep_read, err, 1); // send error packet back to the laptop
                                            }
                                            else
                                            {
                                                uint16_t p_length = count + 3;
                                                uint8_t payload[p_length] = { 0x00, offset_hb, offset_lb };
        
                                                for (uint16_t i = 0; i < count; i++)
                                                {
                                                    payload[3 + i] = values[i];
                                                }
        
                                                send_usb_packet(bus_usb, debug, write_exteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                            }
                                        }
                                        else
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case set_arbitrary_uart_speed: // 0x0A
                                    {
                                        break;
                                    }
                                    case init_bootstrap_mode: // 0x0B - init SBEC3 bootstrap mode
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        unlock_sbec3_bootstrap_mode(cmd_payload[0]);
                                        break;
                                    }
                                    case upload_worker_function: // 0x0C
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        write_worker_function(cmd_payload[0]);
                                        break;
                                    }
                                    case start_worker_function: // 0x0D
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        pcm.msg_buffer[0] = 0x20;
                                        pcm.msg_buffer_ptr = 1;
                                        pcm.msg_to_transmit_count = 1;
                                        pcm.repeat = false;
                                        pcm.repeat_next = false;
                                        pcm.repeat_list_once = false;
                                        pcm.repeat_stop = false;
                                        pcm.msg_tx_pending = true;
                                        break;
                                    }
                                    case exit_worker_function: // 0x0E
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        if (cmd_payload[0] == 3) // flash read
                                        {
                                            pcm.msg_buffer[0] = 0x35;
                                        }
                                        else if (cmd_payload[0] == 7) // EEPROM read
                                        {
                                            pcm.msg_buffer[0] = 0x3B;
                                        }
                                        else if (cmd_payload[0] == 8) // EEPROM write
                                        {
                                            pcm.msg_buffer[0] = 0x38;
                                        }
                                        else
                                        {
                                            pcm.msg_buffer[0] = 0xFE;
                                        }
                                        
                                        pcm.msg_buffer_ptr = 1;
                                        pcm.msg_to_transmit_count = 1;
                                        pcm.repeat = false;
                                        pcm.repeat_next = false;
                                        pcm.repeat_list_once = false;
                                        pcm.repeat_stop = false;
                                        pcm.msg_tx_pending = true;
                                        break;
                                    }
                                    case restore_pcm_eeprom: // 0xF0
                                    {
                                        if (!payload_bytes || (payload_length != 512))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        restore_pcm_eeprom_handler(cmd_payload);
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case ok_error: // 0x0F - OK/ERROR message
                            {
                                // TODO, although it's rare that the laptop sends an error message out of the blue
                                send_usb_packet(bus_usb, ok_error, ok, ack, 1); // acknowledge
                                break;
                            }
                            default: // other values are not used
                            {
                                send_usb_packet(bus_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case bus_usb:
                    case bus_ccd: // 0x01 - CCD-bus is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the CCD-bus
                            {
                                switch (subdatacode) // evaluate  SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        ccd.repeat = false;
                                        ccd.repeat_next = false;
                                        ccd.repeat_list_once = false;
                                        ccd.repeat_stop = true;
                                        ccd.msg_to_transmit_count = 0;
                                        ccd.msg_to_transmit_count_ptr = 0;
                                        ccd.repeated_msg_length = 0;
                                        ccd.repeated_msg_last_millis = 0;
                                        ccd.msg_buffer_ptr = 0;

                                        uint8_t ret[1] = { bus_ccd }; // improvised payload array with only 1 element which is the target bus
                                        send_usb_packet(bus_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the CCD-bus once
                                    case repeated_single_msg: // 0x04 - send a message to the CCD-bus repeatedly forever
                                    {
                                        if (!payload_bytes || (payload_length > 16))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            ccd.msg_buffer[i] = cmd_payload[i];
                                        }

                                        ccd.msg_buffer_ptr = payload_length;
                                        
                                        // Checksum is only applicable if message is at least 2 bytes long.
                                        if (ccd.msg_buffer_ptr > 1)
                                        {
                                            uint8_t checksum_position = ccd.msg_buffer_ptr - 1;
                                            ccd.msg_buffer[checksum_position] = calculate_checksum(ccd.msg_buffer, 0, checksum_position); // overwrite last checksum byte with the correct one
                                        }

                                        ccd.msg_to_transmit_count = 1;

                                        if (subdatacode == repeated_single_msg)
                                        {
                                            ccd.repeat = true;
                                            ccd.repeat_next = true;
                                        }
                                        else
                                        {
                                            ccd.repeat = false;
                                            ccd.repeat_next = false;
                                            ccd.msg_tx_pending = true; // set flag so the main loop knows there's something to do
                                        }

                                        ccd.repeat_list_once = false;
                                        ccd.repeat_stop = false;
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the CCD-bus once
                                    case repeated_list_msg: // 0x05 - send a set of messages to the CCD-bus repeatedly forever
                                    {
                                        if (payload_length < 3)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 02 04 E4 00 00 E4 04 24 00 00 24
                                        // --------------------------------
                                        // 02: number of messages
                                        // 04: 1st message length
                                        // E4 00 00 E4: message #1
                                        // 04: 2nd message length
                                        // 24 00 00 24: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 1; i < payload_length; i++)
                                        {
                                            ccd.msg_buffer[i - 1] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        ccd.msg_to_transmit_count = cmd_payload[0]; // save number of messages
                                        ccd.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        ccd.repeated_msg_length = cmd_payload[1]; // first message length is saved
                                        ccd.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning

                                        for (uint8_t i = 0; i < ccd.msg_to_transmit_count; i++)
                                        {
                                            uint8_t current_msg_length = ccd.msg_buffer[ccd.msg_buffer_ptr];
                                            if (current_msg_length > 1) // checksum is only applicable if message is at least 2 bytes long
                                            {
                                                uint8_t current_checksum_position = ccd.msg_buffer_ptr + current_msg_length;
                                                ccd.msg_buffer[current_checksum_position] = calculate_checksum(ccd.msg_buffer, ccd.msg_buffer_ptr + 1, current_checksum_position); // re-calculate every checksum byte
                                            }
                                            ccd.msg_buffer_ptr += current_msg_length + 1;
                                        }

                                        ccd.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning
                                        ccd.repeat = true; // set flag
                                        ccd.repeat_next = true; // set flag
                                        
                                        if (subdatacode == list_msg) ccd.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) ccd.repeat_list_once = false;
                                        
                                        ccd.repeat_stop = false;
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
    
                            default: // other values are not used
                            {
                                send_usb_packet(bus_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case bus_ccd:
                    case bus_pcm: // 0x02 - SCI-bus (PCM) is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the SCI-bus (PCM)
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        pcm.repeat = false;
                                        pcm.repeat_next = false;
                                        pcm.repeat_list_once = false;
                                        pcm.repeat_stop = true;
                                        pcm.msg_to_transmit_count = 0;
                                        pcm.msg_to_transmit_count_ptr = 0;
                                        pcm.repeated_msg_length = 0;
                                        pcm.repeated_msg_last_millis = 0;
                                        pcm.msg_buffer_ptr = 0;
                                        
                                        uint8_t ret[1] = { bus_pcm };
                                        send_usb_packet(bus_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the SCI-bus once
                                    case repeated_single_msg: // 0x04 - send repeated message(s) to the SCI-bus (PCM)
                                    {
                                        if (!payload_bytes || (payload_length > PCM_RX_BUFFER_SIZE))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            pcm.msg_buffer[i] = cmd_payload[i];
                                        }

                                        pcm.msg_buffer_ptr = payload_length;
                                        pcm.msg_to_transmit_count = 1;

                                        if (subdatacode == repeated_single_msg)
                                        {
                                            pcm.repeat = true;
                                            pcm.repeat_next = true;
                                        }
                                        else
                                        {
                                            pcm.repeat = false;
                                            pcm.repeat_next = false;
                                            pcm.msg_tx_pending = true; // set flag so the main loop knows there's something to do
                                        }

                                        pcm.repeat_list_once = false;
                                        pcm.repeat_stop = false;
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the SCI-bus (PCM) once
                                    case repeated_list_msg: // 0x05 - send a set of messages repeatedly to the SCI-bus (PCM)
                                    {
                                        if (payload_length < 3)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 02 02 14 07 02 14 08
                                        // --------------------------
                                        // 02: number of messages
                                        // 02: 1st message length
                                        // 14 07: message #1
                                        // 02: 2nd message length
                                        // 14 08: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 1; i < payload_length; i++)
                                        {
                                            pcm.msg_buffer[i - 1] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        pcm.msg_to_transmit_count = cmd_payload[0]; // save number of messages
                                        pcm.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        pcm.repeated_msg_length = cmd_payload[1]; // first message length is saved
                                        pcm.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning
                                        pcm.repeat = true;
                                        pcm.repeat_next = true;

                                        if (subdatacode == list_msg) pcm.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) pcm.repeat_list_once = false;
                                        
                                        pcm.repeat_stop = false;
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            default: // other values are not used.
                            {
                                send_usb_packet(bus_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case bus_pcm:
                    case bus_tcm: // 0x03 - SCI-bus (TCM) is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the SCI-bus (TCM)
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        tcm.repeat = false;
                                        tcm.repeat_next = false;
                                        tcm.repeat_list_once = false;
                                        tcm.repeat_stop = true;
                                        tcm.msg_to_transmit_count = 0;
                                        tcm.msg_to_transmit_count_ptr = 0;
                                        tcm.repeated_msg_length = 0;
                                        tcm.repeated_msg_last_millis = 0;
                                        tcm.msg_buffer_ptr = 0;
                                        
                                        uint8_t ret[1] = { bus_tcm };
                                        send_usb_packet(bus_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the SCI-bus once
                                    case repeated_single_msg: // 0x04 - send repeated message(s) to the SCI-bus (TCM)
                                    {
                                        if (!payload_bytes || (payload_length > TCM_RX_BUFFER_SIZE))
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            tcm.msg_buffer[i] = cmd_payload[i];
                                        }

                                        tcm.msg_buffer_ptr = payload_length;
                                        tcm.msg_to_transmit_count = 1;

                                        if (subdatacode == repeated_single_msg)
                                        {
                                            tcm.repeat = true;
                                            tcm.repeat_next = true;
                                        }
                                        else
                                        {
                                            tcm.repeat = false;
                                            tcm.repeat_next = false;
                                            tcm.msg_tx_pending = true; // set flag so the main loop knows there's something to do
                                        }

                                        tcm.repeat_list_once = false;
                                        tcm.repeat_stop = false;
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the SCI-bus (TCM) once
                                    case repeated_list_msg: // 0x05 - send a set of messages repeatedly to the SCI-bus (TCM)
                                    {
                                        if (payload_length < 3)
                                        {
                                            send_usb_packet(bus_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 02 02 14 07 02 14 08
                                        // --------------------------------
                                        // 02: number of messages
                                        // 02: 1st message length
                                        // 14 07: message #1
                                        // 02: 2nd message length
                                        // 14 08: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 1; i < payload_length; i++)
                                        {
                                            tcm.msg_buffer[i - 1] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        tcm.msg_to_transmit_count = cmd_payload[0]; // save number of messages
                                        tcm.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        tcm.repeated_msg_length = cmd_payload[1]; // first message length is saved
                                        tcm.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning
                                        tcm.repeat = true;
                                        tcm.repeat_next = true;

                                        if (subdatacode == list_msg) tcm.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) tcm.repeat_list_once = false;
                                        
                                        tcm.repeat_stop = false;
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(bus_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            default: // other values are not used.
                            {
                                send_usb_packet(bus_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case bus_tcm:
                    default:
                    {
                        send_usb_packet(bus_usb, ok_error, error_datacode_invalid_bus, err, 1);
                        break;
                    }
                } // switch (target)   
                break;
            }
            default:
            {
                // TODO, the main loop itself will get rid of garbage data here, until a valid sync byte is found
                break;
            }
        }

        usb_rx_flush();
    }
    else
    {
        // TODO
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
    
    for (uint16_t i = 0; i < 128; i++) // get 128 samples in quick succession
    {
        ccd_positive_adc += analogRead(CCD_POSITIVE);
    }

    for (uint16_t i = 0; i < 128; i++) // get 128 samples in quick succession
    {
        ccd_negative_adc += analogRead(CCD_NEGATIVE);
    }
    
    ccd_positive_adc /= 128; // divide the sum by 128 to get average value
    ccd_negative_adc /= 128; // divide the sum by 128 to get average value
    ccd_positive_volts = (uint16_t)(ccd_positive_adc*(adc_supply_voltage/100.0)/adc_max_value*1000.0);
    ccd_negative_volts = (uint16_t)(ccd_negative_adc*(adc_supply_voltage/100.0)/adc_max_value*1000.0);
    
    ccd_volts_array[0] = (ccd_positive_volts >> 8) & 0xFF;
    ccd_volts_array[1] = ccd_positive_volts & 0xFF;
    ccd_volts_array[2] = (ccd_negative_volts >> 8) & 0xFF;
    ccd_volts_array[3] = ccd_negative_volts & 0xFF;
}

/*************************************************************************
Function: handle_received_ccd_msg()
Purpose:  save last received CCD-bus message to process later
**************************************************************************/
void handle_received_ccd_msg(uint8_t* message, uint8_t message_length)
{
    if (ccd.enabled)
    {
        for (uint8_t i = 0; i < message_length; i++)
        {
            ccd.message[i] = message[i];
        }

        ccd.message_length = message_length;
    }
}

/*************************************************************************
Function: handle_ccd_error()
Purpose:  process errors with CCD-bus
**************************************************************************/
void handle_ccd_error(CCD_Operations op, CCD_Errors err)
{
    if (ccd.enabled)
    {    
        if (err == CCD_OK) return;
    
        String s = op == CCD_Read ? "READ " : "WRITE ";
    
        switch (err)
        {
            case CCD_ERR_BUS_IS_BUSY:
            {
                //Serial.println(s + "CCD_ERR_BUS_IS_BUSY");
                break;
            }
            case CCD_ERR_BUS_ERROR:
            {
                //Serial.println(s + "CCD_ERR_BUS_ERROR");
                break;
            }
            case CCD_ERR_ARBITRATION_LOST:
            {
                //Serial.println(s + "CCD_ERR_ARBITRATION_LOST");
                break;
            }
            case CCD_ERR_CHECKSUM:
            {
                //Serial.println(s + "CCD_ERR_CHECKSUM");
                break;
            }
            default: // unknown error
            {
                //Serial.println(s + "ERR: " + String(err, HEX));
                break;
            }
        }
    }
}

/*************************************************************************
Function: handle_ccd_data()
Purpose:  handle CCD-bus messages
**************************************************************************/
void handle_ccd_data()
{
    if (ccd.enabled)
    {
        if (ccd.message_length > 0) // last received message was not sent back to laptop yet
        {
            send_usb_packet(bus_ccd, msg_rx, single_msg, ccd.message, ccd.message_length); // send CCD-bus message back to the laptop

            if (!lcd_splash_screen_on) handle_lcd(bus_ccd, ccd.message, 0, ccd.message_length); // pass message to LCD handling function

            if (ccd.repeat)
            {
                if (ccd.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    bool match = true;

//                    for (uint8_t i = 0; i < ccd.message_length; i++)
//                    {
//                        if (ccd.msg_buffer[i] != ccd.message[i]) match = false; // compare received bytes with message sent
//                    }
    
                    if (match) ccd.repeat_next = true; // if echo is correct prepare next message
                }
                else if (ccd.msg_to_transmit_count > 1) // multiple messages
                {
                    bool match = true;
    
//                    for (uint8_t i = 0; i < ccd.message_length; i++)
//                    {
//                        if (ccd.msg_buffer[ccd.msg_buffer_ptr + 1 + i] != ccd.message[i]) match = false; // compare received bytes with message sent
//                    }
    
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
                    ccd.repeat_list_once = false;
                }
            }
    
            ccd.message_length = 0;
            ccd.msg_rx_count++;
        }
        
        if (ccd.random_msg && (ccd.random_msg_interval > 0))
        {
            current_millis = millis(); // check current time

            if ((uint32_t)(current_millis - ccd.random_msg_last_millis) >= ccd.random_msg_interval)
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
            if ((uint32_t)(current_millis - ccd.repeated_msg_last_millis) >= ccd.repeated_msg_interval) // wait between messages
            {
                ccd.repeated_msg_last_millis = current_millis;

                if (ccd.repeat_next)
                {
                    // The message is already in the ccd.msg_buffer array, just set flags
                    ccd.msg_tx_pending = true; // set flag
                    ccd.repeat_next = false;
                }
            }
        }

        if (ccd.msg_tx_pending) // received over usb connection, checksum corrected there (if wrong)
        {     
            if (ccd.msg_to_transmit_count == 1) // if there's only one message in the buffer
            {
                while (CCD.write(ccd.msg_buffer, ccd.msg_buffer_ptr)); // send message on the CCD-bus
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

                while (CCD.write(current_message, ccd.repeated_msg_length)); // send message on the CCD-bus
            }
            
            ccd.msg_tx_pending = false; // re-arm, make it possible to send a message again, TODO: same as above
            ccd.msg_tx_count++;
        }
    }
}

/*************************************************************************
Function: handle_sci_data()
Purpose:  handle SCI-bus messages from both PCM and TCM
**************************************************************************/
void handle_sci_data(void)
{
    uint32_t timeout_start = 0;
    bool timeout_reached = false;
    
    if (pcm.enabled)
    {
        // Handle completed messages
        if (pcm.speed == BAUD_7812) // handle low-speed mode first (7812.5 baud)
        {
            if ((pcm.echo_received || pcm.response_received) && (pcm_rx_available() > 0))
            {
                pcm.echo_received = false;
                pcm.response_received = false;
                pcm.message_length = pcm_rx_available();

                for (uint8_t i = 0; i < pcm.message_length; i++)
                {
                    pcm.message[i] = pcm_getc() & 0xFF;
                }

                send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, pcm.message, pcm.message_length); // send message to laptop
                handle_lcd(bus_pcm, pcm.message, 0, pcm.message_length); // pass message to LCD handling function

                if (pcm.message[0] == 0x12) // pay attention to special bytes (speed change)
                {
                    sbi(pcm.bus_settings, 7); // set enable bit
                    sbi(pcm.bus_settings, 1); // set/clear speed bits (62500 baud)
                    cbi(pcm.bus_settings, 0); // set/clear speed bits (62500 baud)
                    configure_sci_bus(pcm.bus_settings);
                }

                if (pcm.repeat) // prepare next repeated message
                {
                    if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        bool match = true;

                        //for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                        //{
                        //    if (pcm.msg_buffer[i] != pcm.message[i]) match = false; // compare received bytes with message sent
                        //}

                        if (match) pcm.repeat_next = true; // if echo is correct prepare next message
                    }
                    else if (pcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        bool match = true;

                        //for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                        //{
                        //    if (pcm.msg_buffer[pcm.msg_buffer_ptr + 1 + i] != pcm.message[i]) match = false; // compare received bytes with message sent
                        //}

                        if (match)
                        {
                            pcm.repeat_next = true; // if echo is correct prepare next message

                            // Increase the current message counter and set the buffer pointer to the next message length
                            pcm.msg_to_transmit_count_ptr++;
                            pcm.msg_buffer_ptr += pcm.repeated_msg_length + 1;
                            pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                            // After the last message reset everything to zero to start at the beginning
                            if (pcm.msg_to_transmit_count_ptr == pcm.msg_to_transmit_count)
                            {
                                pcm.msg_to_transmit_count_ptr = 0;
                                pcm.msg_buffer_ptr = 0;
                                pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                                if (pcm.repeat_list_once) pcm.repeat_stop = true;
                            }
                        }
                    }

                    if (pcm.repeat_stop) // one-shot message list is terminated here
                    {
                        pcm.msg_buffer_ptr = 0;
                        pcm.repeat = false;
                        pcm.repeat_next = false;
                        pcm.repeat_list_once = false;
                    }
                }

                pcm_rx_flush();
                pcm.msg_rx_count++;
            }
        }
        else if (pcm.speed == BAUD_62500) // handle high-speed mode (62500 baud), no need to wait for message completion here, it is already handled when the message was sent
        {
            if ((pcm.echo_received || pcm.response_received || ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_BTSTRP_DLY)) && (pcm_rx_available() > 0))
            {
                pcm.echo_received = false;
                pcm.response_received = false;
                pcm.message_length = pcm_rx_available();

                uint16_t packet_length = 0;

                if (((pcm.msg_to_transmit_count == 1) && array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[0])) || ((pcm.msg_to_transmit_count > 1) && array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[1]))) // normal mode
                {
                    packet_length = (2 * pcm.message_length) - 1;
                }
                else // bootstrap mode
                {
                    packet_length = pcm.message_length;
                }

                uint8_t usb_msg[packet_length];

                pcm.message_length = packet_length;

                // Request and response bytes are mixed together in a single message:
                // F4 0A 00 0B 00 0C 00 0D 00...
                // F4: RAM table
                // 0A: RAM address
                // 00: RAM value at 0A
                // 0B: RAM address
                // 00: RAM value at 0B

                // Custom bootloader by dino2gnt:
                // TX: 45 AA BB CC DD EE
                // RX: 46 AA BB CC DD EE XX YY ZZ
                // ---------------------------
                // 45: request flash memory block
                // 46: flash memory block response
                // AA: memory bank
                // BB: offset HB
                // CC: offset LB
                // DD: block length HB
                // EE: block length LB
                // XX YY ZZ: flash memory values

                if (pcm.msg_to_transmit_count == 1)
                {
                    if (array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[0])) // normal mode
                    {
                        usb_msg[0] = pcm.msg_buffer[0]; // put RAM table byte first
                        pcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte

                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) 
                        {
                            usb_msg[1 + (i * 2)] = pcm.msg_buffer[i + 1]; // put original request message byte next
                            usb_msg[1 + (i * 2) + 1] = pcm_getc() & 0xFF; // put response byte after the request byte
                        }
                    }
                    else // bootstrap mode
                    {
                        for (uint8_t i = 0; i < pcm.message_length; i++) 
                        {
                            usb_msg[i] = pcm_getc() & 0xFF;
                        }
                    }

                    send_usb_packet(bus_pcm, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }
                else if (pcm.msg_to_transmit_count > 1)
                {
                    usb_msg[0] = pcm.msg_buffer[pcm.msg_buffer_ptr + 1]; // put RAM table byte first
                    pcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte

                    for (uint8_t i = 0; i < pcm.repeated_msg_length; i++) 
                    {
                        usb_msg[1 + (i * 2)] = pcm.msg_buffer[pcm.msg_buffer_ptr + i + 2]; // put original request message byte next
                        usb_msg[1 + (i * 2) + 1] = pcm_getc() & 0xFF; // put response byte after the request byte
                    }

                    send_usb_packet(bus_pcm, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }

                if (usb_msg[0] == 0xFE) // pay attention to special bytes (speed change)
                {
                    sbi(pcm.bus_settings, 7); // set enable bit
                    cbi(pcm.bus_settings, 1); // set/clear speed bits (7812.5 baud)
                    sbi(pcm.bus_settings, 0); // set/clear speed bits (7812.5 baud)
                    configure_sci_bus(pcm.bus_settings);
                }

                if (pcm.repeat)
                {
                    if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        pcm.repeat_next = true; // accept echo without verification...
                    }
                    else if (pcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        pcm.repeat_next = true; // accept echo without verification...

                        // Increase the current message counter and set the buffer pointer to the next message length byte.
                        pcm.msg_to_transmit_count_ptr++;
                        pcm.msg_buffer_ptr += pcm.repeated_msg_length + 1;
                        pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning.
                        if (pcm.msg_to_transmit_count_ptr == pcm.msg_to_transmit_count)
                        {
                            pcm.msg_to_transmit_count_ptr = 0;
                            pcm.msg_buffer_ptr = 0;
                            pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                            if (pcm.repeat_list_once) pcm.repeat_stop = true;
                        }
                    }

                    if (pcm.repeat_stop) // one-shot message list is terminated here
                    {
                        pcm.msg_buffer_ptr = 0;
                        pcm.repeat = false;
                        pcm.repeat_next = false;
                        pcm.repeat_list_once = false;
                    }
                }

                handle_lcd(bus_pcm, pcm.message, 0, pcm.message_length); // pass message to LCD handling function
                pcm_rx_flush();
                pcm.msg_rx_count++;
            }
        }
        else // other non-standard speeds are handled here
        {
            if ((pcm.echo_received || pcm.response_received) && (pcm_rx_available() > 0))
            { 
                pcm.echo_received = false;
                pcm.response_received = false;
                pcm.message_length = pcm_rx_available();

                for (uint8_t i = 0; i < pcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    pcm.message[i] = pcm_getc() & 0xFF;
                }

                send_usb_packet(bus_pcm, msg_rx, sci_ls_bytes, pcm.message, pcm.message_length); // send message to laptop

                // No automatic speed change for 976.5 and 125000 baud!

                if (pcm.repeat)
                {
                    if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        pcm.repeat_next = true; // accept echo without verification...
                    }
                    else if (pcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        pcm.repeat_next = true; // accept echo without verification...

                        // Increase the current message counter and set the buffer pointer to the next message length
                        pcm.msg_to_transmit_count_ptr++;
                        pcm.msg_buffer_ptr += pcm.repeated_msg_length + 1;
                        pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning
                        if (pcm.msg_to_transmit_count_ptr == pcm.msg_to_transmit_count)
                        {
                            pcm.msg_to_transmit_count_ptr = 0;
                            pcm.msg_buffer_ptr = 0;
                            pcm.repeated_msg_length = pcm.msg_buffer[pcm.msg_buffer_ptr]; // re-calculate new message length

                            if (pcm.repeat_list_once) pcm.repeat_stop = true;
                        }
                    }

                    if (pcm.repeat_stop) // one-shot message list is terminated here
                    {
                        pcm.msg_buffer_ptr = 0;
                        pcm.repeat = false;
                        pcm.repeat_next = false;
                        pcm.repeat_list_once = false;
                    }
                }

                handle_lcd(bus_pcm, pcm.message, 0, pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                pcm_rx_flush();
                pcm.msg_rx_count++;
            }
        }

        // Repeated messages are prepared here for transmission
        if (pcm.repeat && (pcm_rx_available() == 0))
        {
            current_millis = millis(); // check current time

            if ((uint32_t)(current_millis - pcm.repeated_msg_last_millis) >= pcm.repeated_msg_interval) // wait between messages
            {
                pcm.repeated_msg_last_millis = current_millis;
                
                if (pcm.repeat_next)
                {
                    // The message is already in the pcm.msg_buffer array, just set flags
                    pcm.msg_tx_pending = true; // set flag
                    pcm.repeat_next = false;
                }
            }
        }

        // Send message
        if (pcm.msg_tx_pending && (pcm_rx_available() == 0))
        {
            if (pcm.speed == BAUD_7812) // low speed mode (7812.5 baud)
            {
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (pcm.ngc_mode) // OBD2 SCI-2
                    {
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++)
                        {
                            pcm_putc(pcm.msg_buffer[i]);
                        }

                        timeout_reached = false;
                        pcm.last_byte_millis = millis();

                        while (!timeout_reached)
                        {
                            if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        pcm.response_received = true;
                    }
                    else if (pcm.inverted_logic) // OBD1
                    {
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++)
                        {
                            timeout_reached = false;
                            timeout_start = millis();
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }
                            
                            if (timeout_reached) break;
                        }

                        pcm.echo_received = true;

                        if ((pcm.msg_buffer_ptr > 1) && ((pcm.msg_buffer[0] == 0x13) || (pcm.msg_buffer[0] == 0x14)))
                        {
                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 2) && !timeout_reached)
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            if (!timeout_reached)
                            {
                                pcm_putc(0x00); // terminate byte stream (OBD1)

                                timeout_reached = false;
                                timeout_start = millis();

                                while ((pcm_rx_available() <= 3) && !timeout_reached)
                                {
                                    // wait here for response
                                    if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                                }
                            }

                            pcm.response_received = true;
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached)
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                    else // OBD2 SCI-1
                    {
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++)
                        {
                            timeout_reached = false;
                            timeout_start = millis();
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }
                            
                            if (timeout_reached) break;
                        }

                        pcm.echo_received = true;

                        if ((pcm.msg_buffer_ptr > 1) && (pcm.msg_buffer[0] == 0x13) && (pcm.msg_buffer[1] != 0x00))
                        {
                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 2) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm_putc(0x13); // terminate actuator test mode byte stream (OBD2)

                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 3) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }

                        // Handle these messages with absolute minimum waiting (0x14, 0x15, 0x26, 0x27, 0x28).
                        if ((pcm.msg_buffer_ptr > 1) && array_contains(sci_lo_speed_cmd_filter, 5, pcm_peek(0)))
                        {
                            uint8_t num_bytes = pcm_rx_available();
                            timeout_reached = false;

                            while ((pcm_rx_available() <= num_bytes) && !timeout_reached) // wait for 1 byte response only
                            {
                                if ((pcm_peek(0) == 0x27) || (pcm_peek(0) == 0x28))
                                {
                                    if ((uint32_t)(millis() - pcm.last_byte_millis) >= 300) timeout_reached = true; // apply generous timeout for JTEC EEPROM read/write (slow)
                                    continue;
                                }

                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                                
                            }

                            pcm.response_received = true;
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached && (pcm_rx_available() < 17)) // wait until all bytes are received
                            {
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Make a local copy of the current message.
                    uint8_t current_message[pcm.repeated_msg_length];
                    int j = 0;

                    for (int i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++)
                    {
                        current_message[j] = pcm.msg_buffer[i];
                        j++;
                    }
                    
                    if (pcm.ngc_mode) // do not wait for echo
                    {
                        // Navigate in the main buffer after the message length byte and start sending those bytes
                        for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                        {
                            pcm_putc(current_message[i]);
                        }

                        timeout_reached = false;
                        pcm.last_byte_millis = millis();

                        while (!timeout_reached)
                        {
                            if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        pcm.response_received = true;
                    }
                    else if (pcm.inverted_logic) // OBD1
                    {
                        // Navigate in the main buffer after the message length byte and start sending those bytes
                        for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                        {
                            timeout_reached = false;
                            timeout_start = millis();
                            pcm_putc(current_message[i]);
    
                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }
    
                            if (timeout_reached) break;
                        }

                        pcm.echo_received = true;

                        if ((pcm.repeated_msg_length > 1) && ((pcm_peek(0) == 0x13) || (pcm_peek(0) == 0x14)))
                        {
                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 2) && !timeout_reached)
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            if (!timeout_reached)
                            {
                                pcm_putc(0x00); // terminate byte stream (OBD1)

                                timeout_reached = false;
                                timeout_start = millis();

                                while ((pcm_rx_available() <= 3) && !timeout_reached)
                                {
                                    // wait here for response
                                    if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                                }
                            }

                            pcm.response_received = true;
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached)
                            {
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                    else // OBD2 SCI-1
                    {
                        // Navigate in the main buffer after the message length byte and start sending those bytes
                        for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                        {
                            timeout_reached = false;
                            timeout_start = millis();
                            pcm_putc(current_message[i]);
    
                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }
    
                            if (timeout_reached) break;
                        }

                        pcm.echo_received = true;

                        if ((pcm.repeated_msg_length > 1) && (pcm_peek(0) == 0x13) && (pcm_peek(1) != 0x00))
                        {
                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 2) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm_putc(0x13); // terminate actuator test mode byte stream (OBD2)

                            timeout_reached = false;
                            timeout_start = millis();

                            while ((pcm_rx_available() <= 3) && !timeout_reached)
                            {
                                // wait here for echo
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }

                        // Handle these messages with absolute minimum waiting (0x14, 0x15, 0x26, 0x27, 0x28).
                        if ((pcm.repeated_msg_length > 1) && array_contains(sci_lo_speed_cmd_filter, 5, pcm_peek(0)))
                        {
                            uint8_t num_bytes = pcm_rx_available();
                            timeout_reached = false;

                            while ((pcm_rx_available() <= num_bytes) && !timeout_reached) // wait for 1 byte response only
                            {
                                if ((pcm_peek(0) == 0x27) || (pcm_peek(0) == 0x28))
                                {
                                    if ((uint32_t)(millis() - pcm.last_byte_millis) >= 300) timeout_reached = true; // apply generous timeout for JTEC EEPROM read/write (slow)
                                    continue;
                                }

                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached && (pcm_rx_available() < 17)) // wait until all bytes are received
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                }
            }
            else if (pcm.speed == BAUD_62500) // high speed mode (62500 baud)
            {
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[0])) // normal mode
                    {
                        if ((pcm.msg_buffer_ptr > 1) && (pcm.msg_buffer[1] == 0xFF)) // return full RAM-table if the first address is an invalid 0xFF
                        {
                            // Prepare message buffer as if it was filled with data beforehand
                            for (uint8_t i = 0; i < 240; i++)
                            {
                                pcm.msg_buffer[1 + i] = i; // put the address byte after the memory table pointer
                            }

                            pcm.msg_buffer_ptr = 241;
                        }

                        timeout_reached = false;

                        if (pcm.ngc_mode) // do not wait for response
                        {
                            for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                            {
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                timeout_reached = false;
                                pcm.last_byte_millis = millis();

                                while (!timeout_reached)
                                {
                                    // wait here for response
                                    if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T2_DELAY) timeout_reached = true;
                                }

                                pcm.response_received = true;
                            }
                        }
                        else // wait for response
                        {
                            for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                            {
                                timeout_reached = false;
                                timeout_start = millis(); // save current time
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                while ((pcm_rx_available() <= i) && !timeout_reached)
                                {
                                    // wait here for response (echo in case of F0...FF)
                                    if ((uint32_t)(millis() - timeout_start) >= SCI_HS_T3_DELAY) timeout_reached = true;
                                }

                                if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                                {
                                    break;
                                }
                            }
                        }

                        pcm.response_received = true;
                    }
                    else // bootstrap mode
                    {
                        if (((pcm.msg_buffer[0] == 0x45) || (pcm.msg_buffer[0] == 0x33)) && (pcm.msg_buffer_ptr == 6)) // flash read
                        {
                            // TX: 45 AA BB CC DD EE
                            // RX: 46 AA BB CC DD EE XX YY ZZ
                            // ---------------------------
                            // 45: request flash memory block
                            // 46: flash memory block response
                            // AA: memory bank
                            // BB: offset HB
                            // CC: offset LB
                            // DD: block length HB
                            // EE: block length LB
                            // XX YY ZZ: flash memory values

                            for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                            {
                                timeout_reached = false;
                                timeout_start = millis();
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                while ((pcm_rx_available() <= i) && !timeout_reached)
                                {
                                    // wait here for echo
                                    if ((uint32_t)(millis() - timeout_start) >= 100) timeout_reached = true;
                                }

                                if (timeout_reached) // exit for-loop if there's no echo
                                {
                                    break;
                                }
                            }

                            if (!timeout_reached)
                            {
                                uint16_t block_length = to_uint16(pcm.msg_buffer[4], pcm.msg_buffer[5]);

                                timeout_reached = false;
                                timeout_start = millis();

                                while ((pcm_rx_available() <= (5 + block_length)) && !timeout_reached)
                                {
                                    // wait here until all bytes are received or timeout occurs (500 ms)
                                    if ((uint32_t)(millis() - timeout_start) >= 500) timeout_reached = true;
                                }

                                if (!timeout_reached) // ok
                                {
                                    pcm.response_received = true;
                                }
                            }
                        }
                        else if ((pcm.msg_buffer[0] == 0x36) && (pcm.msg_buffer_ptr == 5)) // EEPROM write
                        {
                            for (uint8_t i = 0; i < 5; i++) // write header while waiting for echo
                            {
                                timeout_reached = false;
                                timeout_start = millis();
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                while ((pcm_rx_available() <= i) && !timeout_reached)
                                {
                                    // wait here for echo
                                    if ((uint32_t)(millis() - timeout_start) >= 100) timeout_reached = true;
                                }

                                if (timeout_reached) // exit for-loop if there's no echo
                                {
                                    break;
                                }
                            }

                            if (!timeout_reached)
                            {
                                for (uint8_t i = 5; i < pcm.msg_buffer_ptr; i++) // write EEPROM block without waiting for echo
                                {
                                    pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
                                }

                                uint16_t block_length = to_uint16(pcm.msg_buffer[3], pcm.msg_buffer[4]);

                                timeout_reached = false;
                                timeout_start = millis();

                                while ((pcm_rx_available() <= (4 + block_length)) && !timeout_reached)
                                {
                                    // wait here until all bytes are received or timeout occurs (2000 ms)
                                    if ((uint32_t)(millis() - timeout_start) >= 2000) timeout_reached = true;
                                }

                                if (!timeout_reached) // ok
                                {
                                    pcm.response_received = true;
                                }
                            }
                        }
                        else if ((pcm.msg_buffer[0] == 0x39) && (pcm.msg_buffer_ptr == 5)) // EEPROM read
                        {
                            for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                            {
                                timeout_reached = false;
                                timeout_start = millis();
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                while ((pcm_rx_available() <= i) && !timeout_reached)
                                {
                                    // wait here for echo
                                    if ((uint32_t)(millis() - timeout_start) >= 100) timeout_reached = true;
                                }

                                if (timeout_reached) // exit for-loop if there's no echo
                                {
                                    break;
                                }
                            }

                            if (!timeout_reached)
                            {
                                uint16_t block_length = to_uint16(pcm.msg_buffer[3], pcm.msg_buffer[4]);

                                timeout_reached = false;
                                timeout_start = millis();

                                while ((pcm_rx_available() <= (4 + block_length)) && !timeout_reached)
                                {
                                    // wait here until all bytes are received or timeout occurs (2000 ms)
                                    if ((uint32_t)(millis() - timeout_start) >= 1000) timeout_reached = true;
                                }

                                if (!timeout_reached) // ok
                                {
                                    pcm.response_received = true;
                                }
                            }
                        }
                        else
                        {
                            for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                            {
                                timeout_reached = false;
                                timeout_start = millis(); // save current time
                                pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                                while ((pcm_rx_available() <= i) && !timeout_reached)
                                {
                                    // wait for echo or response
                                    if ((uint32_t)(millis() - timeout_start) >= 100) timeout_reached = true;
                                }

                                if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                                {
                                    break;
                                }
                            }

                            while ((uint32_t)(millis() - pcm.last_byte_millis) < SCI_BTSTRP_DLY); // there may be another status byte returned, wait for it as well

                            if (!timeout_reached)
                            {
                                pcm.response_received = true;
                            }
                        }
                    }
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    timeout_reached = false;

                    if (pcm.ngc_mode) // do not wait for response
                    {
                        for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            timeout_reached = false;
                            pcm.last_byte_millis = millis();

                            while (!timeout_reached)
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T2_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                    else // wait for response
                    {
                        // Navigate in the main buffer after the message length byte and start sending those bytes.
                        for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((pcm_rx_available() <= (i - pcm.msg_buffer_ptr - 1)) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((uint32_t)(millis() - timeout_start) >= SCI_HS_T3_DELAY) timeout_reached = true;
                            }

                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                break;
                            }
                        }
                    }

                    pcm.response_received = true;
                }
            }
            else // non-standard speeds
            {
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (pcm.ngc_mode) // do not wait for echo
                    {
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                        {
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            timeout_reached = false;
                            pcm.last_byte_millis = millis();

                            while (!timeout_reached)
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T2_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                    else // wait for echo
                    {
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                        {
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for echo (half-duplex mode)
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                            }

                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                break;
                            }
                        }

                        pcm.echo_received = true;

                        // Wait here for the general 1-byte response
                        uint8_t num_bytes = pcm_rx_available();
                        timeout_reached = false;
                        timeout_start = millis(); // save current time

                        while ((pcm_rx_available() <= num_bytes) && !timeout_reached)
                        {
                            // wait here for response
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                        }

                        pcm.response_received = true;
                    }
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    if (pcm.ngc_mode) // do not wait for echo
                    {
                        for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            timeout_reached = false;
                            pcm.last_byte_millis = millis();

                            while (!timeout_reached) // wait for 1 byte response only
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - pcm.last_byte_millis) >= SCI_LS_T2_DELAY) timeout_reached = true;
                            }

                            pcm.response_received = true;
                        }
                    }
                    else // wait for echo
                    {
                        uint8_t j = 0;

                        // Navigate in the main buffer after the message length byte and start sending those bytes 
                        for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((pcm_rx_available() <= j) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                            }

                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                break;
                            }

                            j++;
                        }
                    }

                    pcm.echo_received = true;

                    // Wait here for the general 1-byte response
                    uint8_t num_bytes = pcm_rx_available();
                    timeout_reached = false;
                    timeout_start = millis(); // save current time

                    while ((pcm_rx_available() <= num_bytes) && !timeout_reached)
                    {
                        // wait here for response
                        if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                    }

                    pcm.response_received = true;
                }
            }

            pcm.msg_tx_pending = false; // re-arm, make it possible to send a message again
            pcm.msg_tx_count++;
        }
    }

    if (tcm.enabled)
    {
        // Handle completed messages
        if (tcm.speed == BAUD_7812) // handle low-speed mode first (7812.5 baud)
        {
            if ((tcm.echo_received || tcm.response_received) && (tcm_rx_available() > 0))
            {
                tcm.echo_received = false;
                tcm.response_received = false;
                tcm.message_length = tcm_rx_available();

                for (uint8_t i = 0; i < tcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    tcm.message[i] = tcm_getc() & 0xFF;
                }

                send_usb_packet(bus_tcm, msg_rx, sci_ls_bytes, tcm.message, tcm.message_length); // send message to laptop
                //handle_lcd(bus_tcm, tcm.message, 0, tcm.message_length); // pass message to LCD handling function

                if (tcm.message[0] == 0x12) // pay attention to special bytes (speed change)
                {
                    sbi(tcm.bus_settings, 5); // set change settings bit
                    sbi(tcm.bus_settings, 4); // set enable bit
                    sbi(tcm.bus_settings, 1); // set/clear speed bits (62500 baud)
                    cbi(tcm.bus_settings, 0); // set/clear speed bits (62500 baud)
                    configure_sci_bus(tcm.bus_settings);
                }

                if (tcm.repeat) // prepare next repeated message
                {
                    if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        bool match = true;

                        for (uint8_t i = 0; i < tcm.repeated_msg_length; i++)
                        {
                            if (tcm.msg_buffer[i] != tcm.message[i]) match = false; // compare received bytes with message sent
                        }

                        if (match) tcm.repeat_next = true; // if echo is correct prepare next message
                    }
                    else if (tcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        bool match = true;

                        for (uint8_t i = 0; i < tcm.repeated_msg_length; i++)
                        {
                            if (tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + i] != tcm.message[i]) match = false; // compare received bytes with message sent
                        }

                        if (match)
                        {
                            tcm.repeat_next = true; // if echo is correct prepare next message

                            // Increase the current message counter and set the buffer pointer to the next message length
                            tcm.msg_to_transmit_count_ptr++;
                            tcm.msg_buffer_ptr += tcm.repeated_msg_length + 1;
                            tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                            // After the last message reset everything to zero to start at the beginning
                            if (tcm.msg_to_transmit_count_ptr == tcm.msg_to_transmit_count)
                            {
                                tcm.msg_to_transmit_count_ptr = 0;
                                tcm.msg_buffer_ptr = 0;
                                tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                                if (tcm.repeat_list_once) tcm.repeat_stop = true;
                            }
                        }
                    }

                    if (tcm.repeat_stop) // one-shot message list is terminated here
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_list_once = false;
                    }
                }

                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }
        else if (tcm.speed == BAUD_62500) // handle high-speed mode (62500 baud), no need to wait for message completion here, it is already handled when the message was sent
        {
            if ((tcm.echo_received || tcm.response_received) && (tcm_rx_available() > 0))
            {
                tcm.echo_received = false;
                tcm.response_received = false;
                tcm.message_length = tcm_rx_available();

                uint16_t packet_length = (2 * tcm.message_length) - 1;

                // Request and response bytes are mixed together in a single message:
                // F4 0A 00 0B 00 0C 00 0D 00...
                // F4: RAM table
                // 0A: RAM address
                // 00: RAM value at 0A
                // 0B: RAM address
                // 00: RAM value at 0B

                tcm.message_length = packet_length;

                if (tcm.msg_to_transmit_count == 1)
                {
                    tcm.message[0] = tcm.msg_buffer[0]; // put RAM table byte first
                    tcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < tcm.msg_buffer_ptr; i++) 
                    {
                        tcm.message[1 + (i * 2)] = tcm.msg_buffer[i + 1]; // put original request message byte next
                        tcm.message[1 + (i * 2) + 1] = tcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(bus_tcm, msg_rx, sci_hs_bytes, tcm.message, tcm.message_length);
                }
                else if (tcm.msg_to_transmit_count > 1)
                {
                    tcm.message[0] = tcm.msg_buffer[tcm.msg_buffer_ptr + 1]; // put RAM table byte first
                    tcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < tcm.repeated_msg_length; i++) 
                    {
                        tcm.message[1 + (i * 2)] = tcm.msg_buffer[tcm.msg_buffer_ptr + i + 2]; // put original request message byte next
                        tcm.message[1 + (i * 2) + 1] = tcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(bus_tcm, msg_rx, sci_hs_bytes, tcm.message, tcm.message_length);
                }

                if (tcm.message[0] == 0xFE) // pay attention to special bytes (speed change)
                {
                    sbi(tcm.bus_settings, 5); // set change settings bit
                    sbi(tcm.bus_settings, 4); // set enable bit
                    cbi(tcm.bus_settings, 1); // set/clear speed bits (7812.5 baud)
                    sbi(tcm.bus_settings, 0); // set/clear speed bits (7812.5 baud)
                    configure_sci_bus(tcm.bus_settings);
                }

                if (tcm.repeat)
                {
                    if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        tcm.repeat_next = true; // accept echo without verification...
                    }
                    else if (tcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        tcm.repeat_next = true; // accept echo without verification...

                        // Increase the current message counter and set the buffer pointer to the next message length
                        tcm.msg_to_transmit_count_ptr++;
                        tcm.msg_buffer_ptr += tcm.repeated_msg_length + 1;
                        tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning
                        if (tcm.msg_to_transmit_count_ptr == tcm.msg_to_transmit_count)
                        {
                            tcm.msg_to_transmit_count_ptr = 0;
                            tcm.msg_buffer_ptr = 0;
                            tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                            if (tcm.repeat_list_once) tcm.repeat_stop = true;
                        }
                    }

                    if (tcm.repeat_stop) // one-shot message list is terminated here
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_list_once = false;
                    }
                }

                //handle_lcd(bus_tcm, tcm.message, 0, tcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }
        else // other non-standard speeds are handled here
        {
            if ((tcm.echo_received || tcm.response_received) && (tcm_rx_available() > 0))
            { 
                tcm.echo_received = false;
                tcm.response_received = false;
                tcm.message_length = tcm_rx_available();

                for (uint8_t i = 0; i < tcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    tcm.message[i] = tcm_getc() & 0xFF;
                }

                send_usb_packet(bus_tcm, msg_rx, sci_ls_bytes, tcm.message, tcm.message_length); // send message to laptop

                // No automatic speed change for 976.5 and 125000 baud!

                if (tcm.repeat)
                {
                    if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        tcm.repeat_next = true; // accept echo without verification...
                    }
                    else if (tcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        tcm.repeat_next = true; // accept echo without verification...

                        // Increase the current message counter and set the buffer pointer to the next message length
                        tcm.msg_to_transmit_count_ptr++;
                        tcm.msg_buffer_ptr += tcm.repeated_msg_length + 1;
                        tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning
                        if (tcm.msg_to_transmit_count_ptr == tcm.msg_to_transmit_count)
                        {
                            tcm.msg_to_transmit_count_ptr = 0;
                            tcm.msg_buffer_ptr = 0;
                            tcm.repeated_msg_length = tcm.msg_buffer[tcm.msg_buffer_ptr]; // re-calculate new message length

                            if (tcm.repeat_list_once) tcm.repeat_stop = true;
                        }
                    }

                    if (tcm.repeat_stop) // one-shot message list is terminated here
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_list_once = false;
                    }
                }

                //handle_lcd(bus_tcm, tcm.message, 0, tcm.message_length); // pass message to LCD handling function
                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }

        // Repeated messages are prepared here for transmission
        if (tcm.repeat && (tcm_rx_available() == 0))
        {
            current_millis = millis(); // check current time

            if ((uint32_t)(current_millis - tcm.repeated_msg_last_millis) >= tcm.repeated_msg_interval) // wait between messages
            {
                tcm.repeated_msg_last_millis = current_millis;

                if (tcm.repeat_next)
                {
                    // The message is already in the tcm.msg_buffer array, just set flags
                    tcm.msg_tx_pending = true; // set flag
                    tcm.repeat_next = false;
                }
            }
        }

        // Send message
        if (tcm.msg_tx_pending && (tcm_rx_available() == 0))
        {
            if (tcm.speed == BAUD_7812) // low speed mode (7812.5 baud), half-duplex mode approach
            {
                if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    for (uint8_t i = 0; i < tcm.msg_buffer_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis();
                        tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer

                        while ((tcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        if (timeout_reached) // exit for-loop if there's no echo
                        {
                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                            tcm_rx_flush();
                            break;
                        }
                    }

                    if (!timeout_reached)
                    {
                        tcm.echo_received = true;
                        
                        // Handle these messages with absolute minimum waiting (0x14, 0x15, 0x26, 0x27, 0x28).
                        if (array_contains(sci_lo_speed_cmd_filter, 5, tcm_peek(0)))
                        {
                            uint8_t num_bytes = tcm_rx_available();
                            timeout_reached = false;

                            while ((tcm_rx_available() <= num_bytes) && !timeout_reached) // wait for 1 byte response only
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - tcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            if (!timeout_reached)
                            {
                                tcm.response_received = true;
                            }
                            else
                            {
                                send_usb_packet(bus_usb, ok_error, error_sci_ls_no_response, err, 1);
                                tcm_rx_flush();
                            }
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached) // wait until all bytes are received
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - tcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            timeout_reached = false;
                            tcm.response_received = true;
                        }
                    }
                }
                else if (tcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Navigate in the main buffer after the message length byte and start sending those bytes
                    for (uint8_t i = (tcm.msg_buffer_ptr + 1); i < (tcm.msg_buffer_ptr + 1 + tcm.repeated_msg_length); i++)
                    {
                        timeout_reached = false;
                        timeout_start = millis();
                        tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer

                        while ((tcm_rx_available() <= (i - tcm.msg_buffer_ptr - 1)) && !timeout_reached)
                        {
                            // wait here for echo
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        if (timeout_reached) // exit for-loop if there's no echo
                        {
                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                            tcm_rx_flush();
                            break;
                        }
                    }

                    if (!timeout_reached)
                    {
                        tcm.echo_received = true;
                        
                        // Handle these messages with absolute minimum waiting (0x14, 0x15, 0x26, 0x27, 0x28).
                        if (array_contains(sci_lo_speed_cmd_filter, 5, tcm_peek(0)))
                        {
                            uint8_t num_bytes = tcm_rx_available();
                            timeout_reached = false;

                            while ((tcm_rx_available() <= num_bytes) && !timeout_reached) // wait for 1 byte response only
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - tcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            if (!timeout_reached)
                            {
                                tcm.response_received = true;
                            }
                            else
                            {
                                send_usb_packet(bus_usb, ok_error, error_sci_ls_no_response, err, 1);
                                tcm_rx_flush();
                            }
                        }
                        else
                        {
                            timeout_reached = false;

                            while (!timeout_reached) // wait until all bytes are received
                            {
                                // wait here for response
                                if ((uint32_t)(millis() - tcm.last_byte_millis) >= SCI_LS_T3_DELAY) timeout_reached = true;
                            }

                            timeout_reached = false;
                            tcm.response_received = true;
                        }
                    }
                }
            }
            else if (tcm.speed == BAUD_62500) // high speed mode (7812.5 baud), full-duplex mode approach
            {
                uint8_t echo_retry_counter = 0;
                
                if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (array_contains(sci_hi_speed_memarea, 16, tcm.msg_buffer[0])) // make sure that the memory table select byte is approved by the tcm before sending the full message
                    {
                        if ((tcm.msg_buffer_ptr > 1) && (tcm.msg_buffer[1] == 0xFF)) // return full RAM-table if the first address is an invalid 0xFF
                        {
                            // Prepare message buffer as if it was filled with data beforehand
                            for (uint8_t i = 0; i < 240; i++)
                            {
                                tcm.msg_buffer[1 + i] = i; // put the address byte after the memory table pointer
                            }

                            tcm.msg_buffer_ptr = 241;
                        }
                        
                        for (uint8_t i = 0; i < tcm.msg_buffer_ptr; i++) // repeat for the length of the message
                        {
                            tcm_again_01:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer

                            while ((tcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((uint32_t)(millis() - timeout_start) >= SCI_HS_T3_DELAY) timeout_reached = true;
                            }

                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                uint8_t ret[2] = { tcm.msg_buffer[0], tcm.msg_buffer[i] };
                                send_usb_packet(bus_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                tcm_rx_flush();
                                break;
                            }

                            if (tcm_peek(0) != tcm.msg_buffer[0]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                tcm_rx_flush();
                                echo_retry_counter++;

                                if (echo_retry_counter < 10) goto tcm_again_01;
                                else
                                {
                                    send_usb_packet(bus_usb, ok_error, error_sci_hs_memory_ptr_no_response, tcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                        }

                        if (!timeout_reached)
                        {
                            tcm.response_received = true;
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(bus_usb, ok_error, error_sci_hs_invalid_memory_ptr, tcm.msg_buffer, 1); // send error packet back to the laptop
                    }
                }
                else if (tcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Navigate in the main buffer after the message length byte and start sending those bytes
                    if (array_contains(sci_hi_speed_memarea, 16, tcm.msg_buffer[tcm.msg_buffer_ptr + 1])) // make sure that the memory table select byte is approved by the TCM before sending the full message
                    {
                        uint8_t j = 0;
                        
                        for (uint8_t i = (tcm.msg_buffer_ptr + 1); i < (tcm.msg_buffer_ptr + 1 + tcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            tcm_again_02:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer
                           
                            while ((tcm_rx_available() <= j) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((uint32_t)(millis() - timeout_start) >= SCI_HS_T3_DELAY) timeout_reached = true;
                            }

                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                uint8_t ret[2] = { tcm.msg_buffer[tcm.msg_buffer_ptr + 1], tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + j] };
                                send_usb_packet(bus_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                tcm_rx_flush();
                                break;
                            }

                            if (tcm_peek(0) != tcm.msg_buffer[tcm.msg_buffer_ptr + 1]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                tcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto tcm_again_02;
                                else
                                {
                                    send_usb_packet(bus_usb, ok_error, error_sci_hs_memory_ptr_no_response, tcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                            
                            j++;
                        }

                        if (!timeout_reached)
                        {
                            tcm.response_received = true;
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(bus_usb, ok_error, error_sci_hs_invalid_memory_ptr, tcm.msg_buffer, 1); // send error packet back to the laptop
                    }
                }
            }
            else // non-standard speeds
            {
                if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    for (uint8_t i = 0; i < tcm.msg_buffer_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer

                        while ((tcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo (half-duplex mode)
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            send_usb_packet(bus_usb, ok_error, error_internal, err, 1);
                            tcm_rx_flush();
                            break;
                        }
                    }

                    if (!timeout_reached)
                    {
                        tcm.echo_received = true;
                        
                        // Wait here for the general 1-byte response
                        uint8_t num_bytes = tcm_rx_available();
                        timeout_reached = false;
                        timeout_start = millis(); // save current time

                        while ((tcm_rx_available() <= num_bytes) && !timeout_reached)
                        {
                            // wait here for response
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                        }

                        if (!timeout_reached)
                        {
                            tcm.response_received = true;
                        }
                        else
                        {
                            send_usb_packet(bus_usb, ok_error, error_sci_ls_no_response, err, 1);
                            tcm_rx_flush();
                        }
                    }
                }
                else if (tcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    uint8_t j = 0;
                    
                    // Navigate in the main buffer after the message length byte and start sending those bytes 
                    for (uint8_t i = (tcm.msg_buffer_ptr + 1); i < (tcm.msg_buffer_ptr + 1 + tcm.repeated_msg_length); i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer
                       
                        while ((tcm_rx_available() <= j) && !timeout_reached)
                        {
                            // wait here for response (echo in case of F0...FF)
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T1_DELAY) timeout_reached = true;
                        }

                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            uint8_t ret[2] = { tcm.msg_buffer[tcm.msg_buffer_ptr + 1], tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + j] };
                            send_usb_packet(bus_usb, ok_error, error_internal, ret, 2); // return two bytes to determine which table and which address is unresponsive
                            tcm_rx_flush();
                            break;
                        }
                        
                        j++;
                    }

                    if (!timeout_reached)
                    {
                        tcm.echo_received = true;
                        
                        // Wait here for the general 1-byte response
                        uint8_t num_bytes = tcm_rx_available();
                        timeout_reached = false;
                        timeout_start = millis(); // save current time

                        while ((tcm_rx_available() <= num_bytes) && !timeout_reached)
                        {
                            // wait here for response
                            if ((uint32_t)(millis() - timeout_start) >= SCI_LS_T3_DELAY) timeout_reached = true;
                        }

                        if (!timeout_reached)
                        {
                            tcm.response_received = true;
                        }
                        else
                        {
                            send_usb_packet(bus_usb, ok_error, error_sci_ls_no_response, err, 1);
                            tcm_rx_flush();
                        }
                    }
                }
            }
            
            tcm.msg_tx_pending = false; // re-arm, make it possible to send a message again
            tcm.msg_tx_count++;
        }
    }
}

/*************************************************************************
Function: blink_led()
Purpose:  turn on one of the indicator LEDs and save time
Note:     this only turns on the chosen LED, handle_leds() turns it off
**************************************************************************/
void blink_led(uint8_t led)
{
    digitalWrite(led, LOW); // turn on LED

    switch (led) // save time when LED was turned on
    {
        case RX_LED:
        {
            rx_led_ontime = millis();
            break;
        }
        case TX_LED:
        {
            tx_led_ontime = millis();
            break;
        }
        case ACT_LED:
        {
            act_led_ontime = millis();
            break;
        }
        default:
        {
            break;
        }
    }
}

/*************************************************************************
Function: handle_leds()
Purpose:  turn off indicator LEDs when blink duration expires
Note:     ACT heartbeat is handled here too;
          how this works:
          - LEDs are turned on somewhere in the code and a timestamp
            is saved at the same time (xx_led_ontime),
          - this function is called pretty frequently so it can
            precisely track how long a given LED is on,
            then turn it off when the time comes
**************************************************************************/
void handle_leds(void)
{
    current_millis = millis(); // check current time

    if (heartbeat_enabled)
    {
        if ((uint32_t)(current_millis - previous_act_blink) >= heartbeat_interval)
        {
            previous_act_blink = current_millis; // save current time
            blink_led(ACT_LED);
        }
    }
    if ((uint32_t)(current_millis - rx_led_ontime) >= led_blink_duration)
    {
        digitalWrite(RX_LED, HIGH); // turn off RX LED
    }
    if ((uint32_t)(current_millis - tx_led_ontime) >= led_blink_duration)
    {
        digitalWrite(TX_LED, HIGH); // turn off TX LED
    }
    if ((uint32_t)(current_millis - act_led_ontime) >= led_blink_duration)
    {
        digitalWrite(ACT_LED, HIGH); // turn off ACT LED
    }
}

void setup()
{
    pinMode(RX_LED, OUTPUT);      // Data received LED
    pinMode(TX_LED, OUTPUT);      // Data transmitted LED
    // PWR LED is tied to +5V directly, stays on when the scanner has power, draws about 2mA current.
    pinMode(ACT_LED, OUTPUT);     // Activity (heartbeat) LED
    blink_led(RX_LED);            // 
    blink_led(TX_LED);            // 
    blink_led(ACT_LED);           // 
    pinMode(BATT, INPUT);         // This analog input pin measures battery voltage through a resistor divider (it tolerates 24V batteries!)
    pinMode(CCD_POSITIVE, INPUT); // 
    pinMode(CCD_NEGATIVE, INPUT); // 
    pinMode(TBEN, OUTPUT);        // 
    digitalWrite(TBEN, HIGH);     // disable CCD-bus termination and bias

    // SCI-bus A/B-configuration selector outputs.
    pinMode(A_PCM_RX_EN, OUTPUT);
    pinMode(A_PCM_TX_EN, OUTPUT);
    pinMode(A_TCM_RX_EN, OUTPUT);
    pinMode(A_TCM_TX_EN, OUTPUT);
    pinMode(B_PCM_RX_EN, OUTPUT);
    pinMode(B_PCM_TX_EN, OUTPUT);
    pinMode(B_TCM_RX_EN, OUTPUT);
    pinMode(B_TCM_TX_EN, OUTPUT);

    usb_init(BAUD_250000);// an external serial monitor should have the same speed
    eep_init(); // initialize external EEPROM chip
    load_settings(); // load settings
    ccd_init(); // 7812.5 baud
    pcm_init(BAUD_7812); // 7812.5 baud
    tcm_init(BAUD_7812); // 7812.5 baud
    lcd_init(); // initialize external LCD

    if (lcd_enabled)
    {
        delay(2000);
        
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

        lcd_splash_screen_on = false;
    }

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

    send_usb_packet(bus_usb, reset, reset_done, ack, 1); // scanner ready
    
    configure_sci_bus(pcm.bus_settings); // apply last saved sci-bus settings for PCM
    send_hwfw_info(); // send hardware/firmware information to laptop
    send_status(); // send status
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

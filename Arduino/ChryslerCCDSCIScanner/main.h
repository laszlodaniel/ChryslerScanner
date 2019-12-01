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

#ifndef MAIN_H
#define MAIN_H

extern extEEPROM eep;
extern LiquidCrystal_I2C lcd;

// Firmware date/time of compilation in 64-bit UNIX time
// https://www.epochconverter.com/hex
#define FW_DATE 0x000000005DE3E287

// RAM buffer sizes for different UART-channels
#define USB_RX0_BUFFER_SIZE 1024
#define CCD_RX1_BUFFER_SIZE 32
#define PCM_RX2_BUFFER_SIZE 256
#define TCM_RX3_BUFFER_SIZE 256

#define USB_TX0_BUFFER_SIZE 1024
#define CCD_TX1_BUFFER_SIZE 32
#define PCM_TX2_BUFFER_SIZE 256
#define TCM_TX3_BUFFER_SIZE 256

#define USB_RX0_BUFFER_MASK (USB_RX0_BUFFER_SIZE - 1)
#define CCD_RX1_BUFFER_MASK (CCD_RX1_BUFFER_SIZE - 1)
#define PCM_RX2_BUFFER_MASK (PCM_RX2_BUFFER_SIZE - 1)
#define TCM_RX3_BUFFER_MASK (TCM_RX3_BUFFER_SIZE - 1)

#define USB_TX0_BUFFER_MASK (USB_TX0_BUFFER_SIZE - 1)
#define CCD_TX1_BUFFER_MASK (CCD_TX1_BUFFER_SIZE - 1)
#define PCM_TX2_BUFFER_MASK (PCM_TX2_BUFFER_SIZE - 1)
#define TCM_TX3_BUFFER_MASK (TCM_TX3_BUFFER_SIZE - 1)

// ATmega2560-specific UART registers and vectors
// USB: UART0
// CCD: UART1
// PCM: UART2
// TCM: UART3

#define USB_RECEIVE_INTERRUPT   USART0_RX_vect
#define CCD_RECEIVE_INTERRUPT   USART1_RX_vect
#define PCM_RECEIVE_INTERRUPT   USART2_RX_vect
#define TCM_RECEIVE_INTERRUPT   USART3_RX_vect
#define USB_TRANSMIT_INTERRUPT  USART0_UDRE_vect
#define CCD_TRANSMIT_INTERRUPT  USART1_UDRE_vect
#define PCM_TRANSMIT_INTERRUPT  USART2_UDRE_vect
#define TCM_TRANSMIT_INTERRUPT  USART3_UDRE_vect
#define USB_STATUS              UCSR0A
#define USB_CONTROL             UCSR0B
#define USB_DATA                UDR0
#define USB_UDRIE               UDRIE0
#define CCD_STATUS              UCSR1A
#define CCD_CONTROL             UCSR1B
#define CCD_DATA                UDR1
#define CCD_UDRIE               UDRIE1  
#define PCM_STATUS              UCSR2A
#define PCM_CONTROL             UCSR2B
#define PCM_DATA                UDR2
#define PCM_UDRIE               UDRIE2  
#define TCM_STATUS              UCSR3A
#define TCM_CONTROL             UCSR3B
#define TCM_DATA                UDR3
#define TCM_UDRIE               UDRIE3

// Bit-flags in the high byte of the receive buffer
#define CCD_SOM                 0x8000  /**< Start of message on CCD-bus    10000000 00000000 - ARBITRARY */
#define PCM_SOM                 0x4000  /**< Start of message on SCI-bus    01000000 00000000 - ARBITRARY */
#define TCM_SOM                 0x2000  /**< Start of message on SCI-bus    00100000 00000000 - ARBITRARY */
#define UART_FRAME_ERROR        0x1000  /**< Framing Error by UART          00010000 00000000 - FIXED BIT */ 
#define UART_OVERRUN_ERROR      0x0800  /**< Overrun condition by UART      00001000 00000000 - FIXED BIT */
#define UART_BUFFER_OVERFLOW    0x0400  /**< Receive ringbuffer overflow    00000100 00000000 - ARBITRARY */
#define UART_RX_NO_DATA         0x0200  /**< Receive buffer is empty        00000010 00000000 - ARBITRARY */
//                                                                          FLAGS    DATA

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define ELBAUD  1023 // prescaler for  976.5 baud speed (SCI-bus extra-low speed)
#define LOBAUD  127  // prescaler for 7812.5 baud speed (CCD/SCI-bus default low-speed diagnostic mode)
#define HIBAUD  15   // prescaler for  62500 baud speed (SCI-bus high-speed parameter mode)
#define EHBAUD  7    // prescaler for 125000 baud speed (SCI-bus extra-high speed)
#define USBBAUD 3    // prescaler for 250000 baud speed (USB communication)

#define INT4          2  // CCD-bus idle interrupt pin
#define INT5          3  // CCD-bus active byte interrupt pin
#define CCD_CLOCK_PIN 11 // clock generator output for CCD-bus
#define RX_LED        35 // status LED, message received
#define TX_LED        36 // status LED, message sent
#define ACT_LED       37 // status LED, activity
#define BATT          A0 // battery voltage sensor

#define PA0 22 // SCI-bus configuration selector digital pins on ATmega2560
#define PA1 23 // |
#define PA2 24 // |
#define PA3 25 // |
#define PA4 26 // |
#define PA5 27 // |
#define PA6 28 // |
#define PA7 29 // |

// SAE J2610 (SCI-bus) recommended delays in milliseconds
#define SCI_LS_T1_DELAY  20
#define SCI_LS_T2_DELAY  100
#define SCI_LS_T3_DELAY  50
#define SCI_LS_T4_DELAY  20
#define SCI_LS_T5_DELAY  100
#define SCI_HS_T1_DELAY  1
#define SCI_HS_T2_DELAY  1
#define SCI_HS_T3_DELAY  5
#define SCI_HS_T4_DELAY  0
#define SCI_HS_T5_DELAY  0

#define STOP  0x00
#define START 0x01

// Packet related stuff
#define PACKET_SYNC_BYTE    0x3D // "=" symbol
#define ASCII_SYNC_BYTE     0x3E // ">" symbol
#define MAX_PAYLOAD_LENGTH  USB_RX0_BUFFER_SIZE - 6  // 1024-6 bytes
#define EMPTY_PAYLOAD       0xFE  // Random byte, could be anything

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
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x01 (handshake)
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x02 (status)
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x03 (settings)
#define heartbeat           0x00 // ACT_LED flashing interval is stored in payload
#define set_ccd_bus         0x01 // ON-OFF state is stored in payload
#define set_sci_bus         0x02 // ON-OFF state, A/B configuration and speed are stored in payload
#define set_repeat_behavior 0x03 // Repeated message behavior settings
#define set_lcd             0x04 // ON-OFF state
// 0x03-0xFF reserved 

// SUB-DATA CODE byte
// Command 0x04 & 0x05 (request and response)
#define hwfw_info           0x00 // Hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
#define timestamp           0x01 // elapsed milliseconds since system start
#define battery_voltage     0x02 // flag for battery voltage packet
#define exteeprom_checksum  0x03
// 0x02-0xFF reserved

// SUB-DATA CODE byte
// Command 0x06 (msg_tx)
#define stop_msg_flow       0x00 // stop message transmission (single and repeated as well)         
#define single_msg          0x01 // send message to the target bus specified in DATA CODE; message is stored in payload 
#define repeated_msg        0x02 // send message(s) to the target bus sepcified in DATA CODE; number of messages, repeat interval(s) are stored in payload
// 0x03-0xFF reserved

// SUB-DATA CODE byte
// Command 0x07 (msg_rx)
#define sci_ls_bytes            0x00 // flag
#define sci_hs_request_bytes    0x01 // flag
#define sci_hs_response_bytes   0x02 // flag
// 0x02-0xFF reserved

// SUB-DATA CODE byte
// Command 0x0E (debug)
// 0x00-0xFF reserved

// SUB-DATA CODE byte
// Command 0x0F (ok_error)
#define ok                                      0x00
#define error_length_invalid_value              0x01
#define error_datacode_invalid_command          0x02
#define error_subdatacode_invalid_value         0x03
#define error_payload_invalid_values            0x04
#define error_checksum_invalid_value            0x05
#define error_packet_timeout_occured            0x06
#define error_buffer_overflow                   0x07
// 0x08-0xFA reserved
#define error_sci_hs_memory_ptr_no_response     0xF8
#define error_sci_hs_invalid_memory_ptr         0xF9
#define error_sci_hs_no_response                0xFA
#define error_eep_not_found                     0xFB
#define error_eep_read                          0xFC
#define error_eep_write                         0xFD
#define error_internal                          0xFE
#define error_fatal                             0xFF

// Command Line Interface (CLI) defines
#define CLI_BUF_SIZE      128  // Maximum input string length
#define CLI_ARG_BUF_SIZE  64   // Maximum argument string length
#define CLI_MAX_NUM_ARGS  8    // Maximum number of arguments

// Set (1), clear (0) and invert (1->0; 0->1) bit in a register or variable easily
#define sbi(variable, bit) (variable) |=  (1 << (bit))
#define cbi(variable, bit) (variable) &= ~(1 << (bit))
#define ibi(variable, bit) (variable) ^=  (1 << (bit))

// Variables
volatile uint8_t  USB_RxBuf[USB_RX0_BUFFER_SIZE];
volatile uint8_t  USB_TxBuf[USB_TX0_BUFFER_SIZE];
volatile uint16_t USB_RxHead; // since buffer size is bigger than 256 bytes it has to be a 16-bit (int) variable
volatile uint16_t USB_RxTail;
volatile uint16_t USB_TxHead;
volatile uint16_t USB_TxTail;
volatile uint8_t  USB_LastRxError;

volatile uint8_t  CCD_RxBuf[CCD_RX1_BUFFER_SIZE];
volatile uint8_t  CCD_TxBuf[CCD_TX1_BUFFER_SIZE];
volatile uint8_t  CCD_RxHead;
volatile uint8_t  CCD_RxTail;
volatile uint8_t  CCD_TxHead;
volatile uint8_t  CCD_TxTail;
volatile uint8_t  CCD_LastRxError;

volatile uint8_t  PCM_RxBuf[PCM_RX2_BUFFER_SIZE];
volatile uint8_t  PCM_TxBuf[PCM_TX2_BUFFER_SIZE];
volatile uint8_t  PCM_RxHead;
volatile uint8_t  PCM_RxTail;
volatile uint8_t  PCM_TxHead;
volatile uint8_t  PCM_TxTail;
volatile uint8_t  PCM_LastRxError;

volatile uint8_t  TCM_RxBuf[TCM_RX3_BUFFER_SIZE];
volatile uint8_t  TCM_TxBuf[TCM_TX3_BUFFER_SIZE];
volatile uint8_t  TCM_RxHead;
volatile uint8_t  TCM_RxTail;
volatile uint8_t  TCM_TxHead;
volatile uint8_t  TCM_TxTail;
volatile uint8_t  TCM_LastRxError;

// CCD-bus
bool ccd_enabled = true;
volatile bool ccd_idle = true;
volatile bool ccd_ctrl = false; // not used at the moment
volatile uint32_t ccd_msg_rx_count = 0; // for statistical purposes
volatile uint32_t ccd_msg_tx_count = 0; // for statistical purposes
volatile uint8_t ccd_bytes_count = 0; // how long is the current CCD-bus message
volatile uint8_t ccd_msg_in_buffer = 0;
bool ccd_msg_pending = false; // flag for custom ccd-bus message transmission
uint8_t ccd_msg_to_send[CCD_RX1_BUFFER_SIZE]; // custom ccd-bus message is copied here
uint8_t ccd_msg_to_send_ptr = 0; // custom ccd-bus message length
bool generate_random_ccd_msgs = false;
uint16_t random_ccd_msg_interval = 0; // ms
uint16_t random_ccd_msg_interval_min = 0;
uint16_t random_ccd_msg_interval_max = 0;
uint8_t ccd_repeated_msg_count = 0; // how many messages are stacked after each other
uint8_t ccd_repeated_msg_ptr = 0; // which message is being sent
uint8_t ccd_repeated_msg_bytes[CCD_RX1_BUFFER_SIZE]; // buffer to store all repeated message bytes
uint8_t ccd_repeated_msg_bytes_ptr = 0; // where are we right now in this buffer
uint32_t ccd_repeated_msg_last_millis = 0;
bool ccd_repeated_messages = false;
bool ccd_repeated_messages_iteration = false;
bool ccd_repeated_messages_next = false;


// SCI-bus
bool sci_enabled = true;
bool pcm_enabled = true;
bool tcm_enabled = false;
volatile bool pcm_idle = false;
volatile bool tcm_idle = false;
bool pcm_high_speed_enabled = false;
bool tcm_high_speed_enabled = false;
uint8_t current_sci_bus_settings[1] = { 0xC8 }; // default settings: SCI-bus PCM "A" configuration, 7812.5 baud, TCM disabled

uint8_t pcm_bytes_buffer[PCM_RX2_BUFFER_SIZE]; // received SCI-bus message from the PCM is temporary stored here
uint8_t pcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool pcm_msg_pending = false; // flag for custom sci-bus message
uint8_t pcm_msg_to_send[PCM_RX2_BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t pcm_msg_to_send_ptr = 0;  // custom sci-bus message length
uint32_t pcm_last_msgbyte_received = 0; // time in milliseconds
uint8_t pcm_repeated_msg_count = 0; // how many messages are stacked after each other
uint8_t pcm_repeated_msg_ptr = 0; // which message is being sent
uint8_t pcm_repeated_msg_bytes[PCM_RX2_BUFFER_SIZE]; // buffer to store all repeated message bytes
uint8_t pcm_repeated_msg_bytes_ptr = 0; // where are we right now in this buffer
uint8_t pcm_repeated_msg_increment = 1; // if iteration is true then the counter in the message will increase this much for every new message
bool pcm_repeated_messages = false;
bool pcm_repeated_messages_iteration = false;
bool pcm_repeated_messages_next = false;
bool pcm_actuator_test_running = false;
uint8_t pcm_actuator_test_byte = 0;
bool pcm_ls_request_running = false;
uint8_t pcm_ls_request_byte = 0;
uint32_t pcm_msg_rx_count = 0;
uint32_t pcm_msg_tx_count = 0;

uint8_t tcm_bytes_buffer[TCM_RX3_BUFFER_SIZE]; // received SCI-bus message from the TCM is temporary stored here
uint8_t tcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool tcm_msg_pending = false; // flag for custom sci-bus message
uint8_t tcm_msg_to_send[TCM_RX3_BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t tcm_msg_to_send_ptr = 0; // custom sci-bus message length
uint32_t tcm_last_msgbyte_received = 0; // time in milliseconds
uint8_t tcm_repeated_msg_count = 0; // how many messages are stacked after each other
uint8_t tcm_repeated_msg_ptr = 0; // which message is being sent
uint8_t tcm_repeated_msg_bytes[TCM_RX3_BUFFER_SIZE]; // buffer to store all repeated message bytes
uint8_t tcm_repeated_msg_bytes_ptr = 0; // where are we right now in this buffer
uint8_t tcm_repeated_msg_increment = 1; // if iteration is true then the counter in the message will increase this much for every new message
bool tcm_repeated_messages = false;
bool tcm_repeated_messages_iteration = false;
bool tcm_repeated_messages_next = false;
uint32_t tcm_msg_rx_count = 0;
uint32_t tcm_msg_tx_count = 0;

const uint8_t sci_hi_speed_memarea[] = { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF };

uint16_t repeated_msg_increment = 1; // if iteration is true then the counter in the message will increase this much for every new message
uint16_t repeated_msg_interval = 100;

// Packet related variables
// Timeout values for packets
uint8_t command_timeout = 100; // milliseconds
uint16_t command_purge_timeout = 200; // milliseconds, if a command isn't complete within this time then delete the usb receive buffer
uint8_t ack[1] = { 0x00 }; // acknowledge payload array
uint8_t err[1] = { 0xFF }; // error payload array
uint8_t ret[1]; // general array to store arbitrary bytes

// Store handshake in the PROGram MEMory (flash)
const uint8_t handshake_progmem[] PROGMEM = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 };
                             //               "C     H     R     Y     S     L     E     R     C     C     D     S     C     I     S     C     A     N     N     E     R"

uint8_t scanner_status[67];

uint8_t avr_signature[3];
uint16_t free_ram_available = 0;

// Battery voltage detector
uint16_t adc_supply_voltage = 500; // supply voltage multiplied by 100: 5.00V -> 500
uint16_t battery_rd1 = 27000; // high resistor value in the divider (R19): 27 kOhm = 27000
uint16_t battery_rd2 = 5000;  // low resistor value in the divider (R20): 5 kOhm = 5000
uint16_t adc_max_value = 1023; // 1023 for 10-bit resolution
uint32_t battery_adc = 0;   // raw analog reading is stored here
uint16_t battery_volts = 0; // converted to battery voltage and multiplied by 100: 12.85V -> 1285
uint8_t battery_volts_array[2]; // battery_volts is separated to byte components here

bool connected_to_vehicle = false;
char vin_characters[17] = "-----------------";
String vin_string;
uint8_t handshake_array[21];

uint32_t rx_led_ontime = 0;
uint32_t tx_led_ontime = 0;
uint32_t act_led_ontime = 0;
uint32_t current_millis = 0;
uint32_t previous_random_ccd_msg_millis = 0;
uint32_t current_millis_blink = 0;
uint32_t previous_act_blink = 0;
uint16_t led_blink_duration = 50; // milliseconds
uint16_t heartbeat_interval = 5000; // milliseconds
bool heartbeat_enabled = true;

uint8_t current_timestamp[4]; // current time is stored here when "update_timestamp" is called
const char ascii_autoreply[] = ">I GOT YOUR MESSAGE!\n";

bool eep_present = false;
uint8_t eep_status = 0; // extEEPROM connection status is stored here
uint8_t eep_result = 0; // extEEPROM 
uint8_t eep_checksum[1];
uint8_t eep_calculated_checksum = 0;
bool    eep_checksum_ok = false;

uint8_t hw_version[2];
uint8_t hw_date[8];
uint8_t assembly_date[8];

// CLI (Command Line Interface) related variables
bool cli_error = false;
char line[CLI_BUF_SIZE];
char args[CLI_MAX_NUM_ARGS][CLI_ARG_BUF_SIZE];

// Function declarations
int cmd_help();
int cmd_reset();
int cmd_handshake();
int cmd_status();
//int cmd_settings();
//int cmd_request();
//int cmd_debug();

int (*commands_func[])() // list of functions pointers corresponding to each command
{
    &cmd_help,
    &cmd_reset,
    &cmd_handshake,
    &cmd_status
//    &cmd_settings,
//    &cmd_request,
//    &cmd_debug,
};

const char *cli_commands_str[] = // list of command names
{
    "help",
    "reset",
    "handshake",
    "status"
//    "settings",
//    "request",
//    "debug"
};

const char *request_args[] = // list of request sub command names
{
    "info",
    "timestamp",
    "battery_voltage",
    "exteeprom_checksum"
};

int num_commands = sizeof(cli_commands_str) / sizeof(char *);

// LCD related variables
// Custom LCD-characters
// https://maxpromer.github.io/LCD-Character-Creator/
uint8_t up_symbol[8]     = { 0x00, 0x04, 0x0E, 0x15, 0x04, 0x04, 0x00, 0x00 }; // ↑
uint8_t down_symbol[8]   = { 0x00, 0x04, 0x04, 0x15, 0x0E, 0x04, 0x00, 0x00 }; // ↓
uint8_t left_symbol[8]   = { 0x00, 0x04, 0x08, 0x1F, 0x08, 0x04, 0x00, 0x00 }; // ←
uint8_t right_symbol[8]  = { 0x00, 0x04, 0x02, 0x1F, 0x02, 0x04, 0x00, 0x00 }; // →
uint8_t enter_symbol[8]  = { 0x00, 0x01, 0x05, 0x09, 0x1F, 0x08, 0x04, 0x00 }; // 
uint8_t degree_symbol[8] = { 0x06, 0x09, 0x09, 0x06, 0x00, 0x00, 0x00, 0x00 }; // °

bool lcd_enabled = false;


// Interrupt Service Routines
ISR(USB_RECEIVE_INTERRUPT)
/*************************************************************************
Function: UART0 Receive Complete interrupt
Purpose:  called when the UART0 has received a character
**************************************************************************/
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
    tmphead = (USB_RxHead + 1) & USB_RX0_BUFFER_MASK;
    
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

ISR(USB_TRANSMIT_INTERRUPT)
/*************************************************************************
Function: UART0 Data Register Empty interrupt
Purpose:  called when the UART0 is ready to transmit the next byte
**************************************************************************/
{
    uint16_t tmptail;

    if (USB_TxHead != USB_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (USB_TxTail + 1) & USB_TX0_BUFFER_MASK;
        USB_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        USB_DATA = USB_TxBuf[tmptail]; /* start transmission */
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        USB_CONTROL &= ~_BV(USB_UDRIE);
    }
}

ISR(CCD_RECEIVE_INTERRUPT)
/*************************************************************************
Function: UART1 Receive Complete interrupt
Purpose:  called when the UART1 has received a character
**************************************************************************/
{
    uint16_t tmphead;
    uint8_t data;
    uint8_t usr;
    uint8_t lastRxError;
 
    /* read UART status register and UART data register */ 
    usr  = CCD_STATUS;
    data = CCD_DATA;
    
    /* get error bits from status register */
    lastRxError = (usr & (_BV(FE1)|_BV(DOR1)));

    /* calculate buffer index */ 
    tmphead = (CCD_RxHead + 1) & CCD_RX1_BUFFER_MASK;
    
    if (tmphead == CCD_RxTail)
    {
        /* error: receive buffer overflow */
        lastRxError = UART_BUFFER_OVERFLOW >> 8;
    }
    else
    {
        /* store new index */
        CCD_RxHead = tmphead;
        /* store received data in buffer */
        CCD_RxBuf[tmphead] = data;
    }
    CCD_LastRxError = lastRxError;
}

ISR(CCD_TRANSMIT_INTERRUPT)
/*************************************************************************
Function: UART1 Data Register Empty interrupt
Purpose:  called when the UART1 is ready to transmit the next byte
**************************************************************************/
{
    uint16_t tmptail;

    if (CCD_TxHead != CCD_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (CCD_TxTail + 1) & CCD_TX1_BUFFER_MASK;
        CCD_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        CCD_DATA = CCD_TxBuf[tmptail]; /* start transmission */
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        CCD_CONTROL &= ~_BV(CCD_UDRIE);
    }
}

ISR(PCM_RECEIVE_INTERRUPT)
/*************************************************************************
Function: UART2 Receive Complete interrupt
Purpose:  called when the UART2 has received a character
**************************************************************************/
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
    tmphead = (PCM_RxHead + 1) & PCM_RX2_BUFFER_MASK;
    
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
        PCM_RxBuf[tmphead] = data;
    }
    PCM_LastRxError = lastRxError;
}

ISR(PCM_TRANSMIT_INTERRUPT)
/*************************************************************************
Function: UART2 Data Register Empty interrupt
Purpose:  called when the UART2 is ready to transmit the next byte
**************************************************************************/
{
    uint16_t tmptail;

    if (PCM_TxHead != PCM_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (PCM_TxTail + 1) & PCM_TX2_BUFFER_MASK;
        PCM_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        PCM_DATA = PCM_TxBuf[tmptail]; /* start transmission */
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        PCM_CONTROL &= ~_BV(PCM_UDRIE);
    }
}

ISR(TCM_RECEIVE_INTERRUPT)
/*************************************************************************
Function: UART3 Receive Complete interrupt
Purpose:  called when the UART3 has received a character
**************************************************************************/
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
    tmphead = (TCM_RxHead + 1) & TCM_RX3_BUFFER_MASK;
    
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
        TCM_RxBuf[tmphead] = data;
    }
    TCM_LastRxError = lastRxError;   
}

ISR(TCM_TRANSMIT_INTERRUPT)
/*************************************************************************
Function: UART3 Data Register Empty interrupt
Purpose:  called when the UART3 is ready to transmit the next byte
**************************************************************************/
{
    uint16_t tmptail;

    if (TCM_TxHead != TCM_TxTail)
    {
        /* calculate and store new buffer index */
        tmptail = (TCM_TxTail + 1) & TCM_TX3_BUFFER_MASK;
        TCM_TxTail = tmptail;
        /* get one byte from buffer and write it to UART */
        TCM_DATA = TCM_TxBuf[tmptail]; /* start transmission */
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        TCM_CONTROL &= ~_BV(TCM_UDRIE);
    }
}


// Functions
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

} /* usb_init */


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
    tmptail = (USB_RxTail + 1) & USB_RX0_BUFFER_MASK;
  
    USB_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = USB_RxBuf[tmptail];

    return (USB_LastRxError << 8 ) + data;
    
} /* usb_getc */


/*************************************************************************
Function: usb_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in receive buffer
          high byte: error flags
**************************************************************************/
uint16_t usb_peek(uint16_t index = 0)
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
  
    tmptail = (USB_RxTail + 1 + index) & USB_RX0_BUFFER_MASK;

    /* get data from receive buffer */
    data = USB_RxBuf[tmptail];

    return (USB_LastRxError << 8 ) + data;

} /* usb_peek */


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

    tmphead = (USB_TxHead + 1) & USB_TX0_BUFFER_MASK;

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

} /* usb_putc */


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

} /* usb_puts */


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

} /* usb_puts_p */


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
        ret = (USB_RX0_BUFFER_SIZE + USB_RxHead - USB_RxTail) & USB_RX0_BUFFER_MASK;
    }
    return ret;
    
} /* usb_rx_available */


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
        ret = (USB_TX0_BUFFER_SIZE + USB_TxHead - USB_TxTail) & USB_TX0_BUFFER_MASK;
    }
    return ret;
    
} /* usb_tx_available */


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
    
} /* usb_rx_flush */


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
    
} /* usb_tx_flush */


// CCD-bus functions
/*************************************************************************
Function: ccd_init()
Purpose:  initialize UART1 and set baudrate to conform CCD-bus requirements,
          frame format is fixed
Input:    direct ubrr value
Returns:  none
**************************************************************************/
void ccd_init(uint16_t ubrr)
{
    /* reset ringbuffer */
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        CCD_RxHead = 0;
        CCD_RxTail = 0;
        CCD_TxHead = 0;
        CCD_TxTail = 0;
    }
  
    /* set baud rate */
    UBRR1H = (ubrr >> 8) & 0x0F;
    UBRR1L = ubrr & 0xFF;

    /* enable USART receiver and transmitter and receive complete interrupt */
    CCD_CONTROL |= (1 << RXCIE1) | (1 << RXEN1) | (1 << TXEN1);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR1C |= (1 << UCSZ10) | (1 << UCSZ11);
    
} /* ccd_init */


/*************************************************************************
Function: ccd_getc()
Purpose:  return byte from the receive buffer and remove it
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t ccd_getc(void)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (CCD_RxHead == CCD_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
  
    /* calculate / store buffer index */
    tmptail = (CCD_RxTail + 1) & CCD_RX1_BUFFER_MASK;
  
    CCD_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = CCD_RxBuf[tmptail];

    return (CCD_LastRxError << 8 ) + data;
    
} /* ccd_getc */


/*************************************************************************
Function: ccd_peek()
Purpose:  return byte waiting in the receive buffer at index
          without removing it (by default the next byte available to read)
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t ccd_peek(uint16_t index = 0)
{
    uint16_t tmptail;
    uint8_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (CCD_RxHead == CCD_RxTail)
        {
            return UART_RX_NO_DATA; /* no data available */
        }
    }
    
    tmptail = (CCD_RxTail + 1 + index) & CCD_RX1_BUFFER_MASK;

    /* get data from receive buffer */
    data = CCD_RxBuf[tmptail];

    return (CCD_LastRxError << 8 ) + data;

} /* ccd_peek */


/*************************************************************************
Function: ccd_putc()
Purpose:  transmit byte to the CCD-bus
Input:    byte to be transmitted
Returns:  none
**************************************************************************/
void ccd_putc(uint8_t data)
{
    uint16_t tmphead;
    uint16_t txtail_tmp;

    tmphead = (CCD_TxHead + 1) & CCD_TX1_BUFFER_MASK;

    do
    {
        ATOMIC_BLOCK(ATOMIC_FORCEON)
        {
            txtail_tmp = CCD_TxTail;
        }
    }
    while (tmphead == txtail_tmp); /* wait for free space in buffer */

    CCD_TxBuf[tmphead] = data;
    CCD_TxHead = tmphead;

    /* enable UDRE interrupt */
    CCD_CONTROL |= _BV(CCD_UDRIE);

} /* ccd_putc */


/*************************************************************************
Function: ccd_puts()
Purpose:  transmit string to the CCD-bus
Input:    pointer to the string to be transmitted
Returns:  none
**************************************************************************/
void ccd_puts(const char *s)
{
    while(*s)
    {
        ccd_putc(*s++);
    }

} /* ccd_puts */


/*************************************************************************
Function: ccd_puts_p()
Purpose:  transmit string from program memory to the CCD-bus
Input:    pointer to the program memory string to be transmitted
Returns:  none
**************************************************************************/
void ccd_puts_p(const char *progmem_s)
{
    register char c;

    while ((c = pgm_read_byte(progmem_s++)))
    {
        ccd_putc(c);
    }

} /* ccd_puts_p */


/*************************************************************************
Function: ccd_rx_available()
Purpose:  determine the number of bytes waiting in the receive buffer
Input:    none
Returns:  integer number of bytes in the receive buffer
**************************************************************************/
uint8_t ccd_rx_available(void)
{
    uint8_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (CCD_RX1_BUFFER_SIZE + CCD_RxHead - CCD_RxTail) & CCD_RX1_BUFFER_MASK;
    }
    return ret;
    
} /* ccd_rx_available */


/*************************************************************************
Function: ccd_tx_available()
Purpose:  determine the number of bytes waiting in the transmit buffer
Input:    none
Returns:  integer number of bytes in the transmit buffer
**************************************************************************/
uint8_t ccd_tx_available(void)
{
    uint8_t ret;
  
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (CCD_TX1_BUFFER_SIZE + CCD_TxHead - CCD_TxTail) & CCD_TX1_BUFFER_MASK;
    }
    return ret;
    
} /* ccd_tx_available */


/*************************************************************************
Function: ccd_rx_flush()
Purpose:  flush bytes waiting in the receive buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void ccd_rx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        CCD_RxHead = CCD_RxTail;
    }
    
} /* ccd_rx_flush */


/*************************************************************************
Function: ccd_tx_flush()
Purpose:  flush bytes waiting in the transmit buffer, actually ignores them
Input:    none
Returns:  none
**************************************************************************/
void ccd_tx_flush(void)
{
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        CCD_TxHead = CCD_TxTail;
    }
    
} /* ccd_tx_flush */


// SCI-bus functions (for PCM)
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
    
} /* pcm_init */


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
    tmptail = (PCM_RxTail + 1) & PCM_RX2_BUFFER_MASK;
  
    PCM_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = PCM_RxBuf[tmptail];

    return (PCM_LastRxError << 8 ) + data;
    
} /* pcm_getc */


/*************************************************************************
Function: pcm_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Input:    index number in the buffer (default = 0 = next available byte)
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t pcm_peek(uint16_t index = 0)
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
  
    tmptail = (PCM_RxTail + 1 + index) & PCM_RX2_BUFFER_MASK;

    /* get data from receive buffer */
    data = PCM_RxBuf[tmptail];

    return (PCM_LastRxError << 8 ) + data;

} /* pcm_peek */


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

    tmphead = (PCM_TxHead + 1) & PCM_TX2_BUFFER_MASK;

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

} /* pcm_putc */


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

} /* pcm_puts */


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

} /* pcm_puts_p */


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
        ret = (PCM_RX2_BUFFER_SIZE + PCM_RxHead - PCM_RxTail) & PCM_RX2_BUFFER_MASK;
    }
    return ret;
    
} /* pcm_rx_available */


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
        ret = (PCM_TX2_BUFFER_SIZE + PCM_TxHead - PCM_TxTail) & PCM_TX2_BUFFER_MASK;
    }
    return ret;
    
} /* pcm_tx_available */


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
    
} /* pcm_rx_flush */


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
    
} /* pcm_tx_flush */


// SCI-bus functions (for TCM)
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
    
} /* tcm_init */


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
    tmptail = (TCM_RxTail + 1) & TCM_RX3_BUFFER_MASK;
  
    TCM_RxTail = tmptail;
  
    /* get data from receive buffer */
    data = TCM_RxBuf[tmptail];

    return (TCM_LastRxError << 8 ) + data;
    
} /* tcm_getc */


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
  
    tmptail = (TCM_RxTail + 1 + index) & TCM_RX3_BUFFER_MASK;

    /* get data from receive buffer */
    data = TCM_RxBuf[tmptail];

    return (TCM_LastRxError << 8 ) + data;

} /* tcm_peek */


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

    tmphead = (TCM_TxHead + 1) & TCM_TX3_BUFFER_MASK;

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

} /* tcm_putc */


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

} /* tcm_puts */


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

} /* tcm_puts_p */


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
        ret = (TCM_RX3_BUFFER_SIZE + TCM_RxHead - TCM_RxTail) & TCM_RX3_BUFFER_MASK;
    }
    return ret;
    
} /* tcm_rx_available */


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
        ret = (TCM_TX3_BUFFER_SIZE + TCM_TxHead - TCM_TxTail) & TCM_TX3_BUFFER_MASK;
    }
    return ret;
    
} /* tcm_tx_available */


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
    
} /* tcm_rx_flush */


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
    
} /* tcm_tx_flush */


/*************************************************************************
Function: uart_status()
Purpose:  gather status report from all 4 uart-channels
Input:    none
Returns:  the latest "____LastRxError" values packed in a single qword
          (highest byte uart0, ..., lowest byte uart3)
                        (usb)                   (tcm)
**************************************************************************/
uint32_t uart_status(void)
{
    uint32_t ret;
    
    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        ret = (USB_LastRxError << 24) | (CCD_LastRxError << 16) | (PCM_LastRxError << 8) | TCM_LastRxError;
    }
    return ret;
    
} /* uart_status */


/*************************************************************************
Function: ccd_eom()
Purpose:  called when the CCD-chip's IDLE pin is going low,
          the last byte received was the end of the message (EOM)
          and the next byte going to be the new message's ID byte
**************************************************************************/
void ccd_eom(void)
{
    ccd_idle = true; // set flag
    ccd_bytes_count = ccd_rx_available();
    ccd_msg_in_buffer++;
    
} // end of ccd_eom


/*************************************************************************
Function: ccd_ctrl()
Purpose:  called when the CCD-chip's CTRL pin is going low
**************************************************************************/
void ccd_active_byte(void)
{
    ccd_ctrl = true; /* set flag */
    ccd_idle = false;
    
} // end of ccd_ctrl


/*************************************************************************
Function: calculate_checksum()
Purpose:  calculate checksum in a given buffer with specified length
Note:     startindex = first byte in the array to include in calculation
          length = buffer full length
**************************************************************************/
uint8_t calculate_checksum(uint8_t *buff, uint16_t startindex, uint16_t length)
{
    uint8_t a = 0;
    for (uint16_t i = startindex ; i < length; i++)
    {
        a += buff[i]; // add bytes together
    }
    return a;
    
} // end of calculate_checksum


/*************************************************************************
Function: ccd_clock_generator()
Purpose:  generates 1 MHz clock signal for the CDP68HC68S1 chip
Note:     
           2 MHz interrupt frequency
           *      *      *      *      *      *      *      *      *
(+5V) 1    |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯
           ˄      ˅      ˄      ˅      ˄      ˅      ˄      ˅      ˄
(GND) 0 ___|      |______|      |______|      |______|      |______| 
           *             *             *             *             *
           1 MHz clock frequency, 50% duty cycle (whole period = 1 tick).
**************************************************************************/
void ccd_clock_generator(uint8_t command)
{
    switch (command)
    {
        case START:
        {
            TCCR1A = 0;                        // clear register
            TCCR1B = 0;                        // clear register
            TCNT1  = 0;                        // clear counter
            DDRB   |= (1<<DDB5);               // set OC1A/PB5 as output
            TCCR1A |= (1<<COM1A0);             // toggle OC1A on compare match
            OCR1A  = 7;                        // top value for counter, toggle after counting to 8 (0->7) = 2 MHz interrupt ( = 16 MHz clock frequency / 8)
            TCCR1B |= (1<<WGM12) | (1<<CS10);  // CTC mode, prescaler clock/1 (no prescaler)
            
            break;
        }
        case STOP:
        {
            TCCR1A = 0;
            TCCR1B = 0;
            TCNT1  = 0;
            OCR1A  = 0;
            
            break;
        }
        default:
        {
            break;
        }
    }
    
} // end of ccd_clock_generator


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
    
} // end of update_timestamp


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

} // end of array_contains


/*************************************************************************
Function: blink_led()
Purpose:  turn on one of the indicator LEDs
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
    
} // end of blink_led


/*************************************************************************
Function: send_usb_packet()
Purpose:  assemble and send data packet through serial link (UART0)
Inputs:   - one source byte,
          - one target byte,
          - one datacode command value byte, these three are used to calculate the DATA CODE byte
          - one SUB-DATA CODE byte,
          - name of the PAYLOAD array (it must be previously filled with data),
          - PAYLOAD length
Returns:  none
Note:     SYNC, LENGTH and CHECKSUM bytes are calculated automatically;
          Payload can be omitted if a (uint8_t*)0x00 value is used in conjunction with 0 length
**************************************************************************/
void send_usb_packet(uint8_t source, uint8_t target, uint8_t command, uint8_t subdatacode, uint8_t *payloadbuff, uint16_t payloadbufflen)
{
    // Calculate the length of the full packet:
    // PAYLOAD length + 1 SYNC byte + 2 LENGTH bytes + 1 DATA CODE byte + 1 SUB-DATA CODE byte + 1 CHECKSUM byte
    uint16_t packet_length = payloadbufflen + 6;    
    uint8_t packet[packet_length]; // create a temporary byte-array
    bool payload_bytes = true;
    uint8_t calculated_checksum = 0;
    uint8_t datacode = 0;

    if (payloadbufflen <= 0) payload_bytes = false;
    else payload_bytes = true;

    // Assemble datacode from the first 3 input parameters
    datacode = (source << 6) + (target << 4) + command;
    //          xx000000     +  00yy0000     + 0000zzzz  =  xxyyzzzz  

    // Start assembling the packet by manually filling the first few slots
    packet[0] = PACKET_SYNC_BYTE; // add SYNC byte (0x3D by default)
    packet[1] = ((packet_length - 4) >> 8) & 0xFF; // add LENGTH high byte
    packet[2] =  (packet_length - 4) & 0xFF; // add LENGTH low byte
    packet[3] = datacode; // add DATA CODE byte
    packet[4] = subdatacode; // add SUB-DATA CODE byte
    
    // If there are payload bytes add them too after subdatacode
    if (payload_bytes)
    {
        for (uint16_t i = 0; i < payloadbufflen; i++)
        {
            packet[5 + i] = payloadbuff[i]; // Add message bytes to the PAYLOAD bytes
        }
    }

    // Calculate checksum
    calculated_checksum = calculate_checksum(packet, 1, packet_length - 1);

    // Place checksum byte
    packet[packet_length - 1] = calculated_checksum;

    blink_led(TX_LED);

    // Send the prepared packet through serial link
    for (uint16_t k = 0; k < packet_length; k++)
    {
        usb_putc(packet[k]); // write every byte in the packet to the usb-serial port
    }
    
} /* send_usb_packet */


/*************************************************************************
Function: configure_sci_bus()
Purpose:  as the name says
Note:     Input data bits description:
          PCM: B7:B4
            B7: change settings bit
              0: leave settings
              1: change settings
            B6: enable bit
              0: disable SCI-bus for PCM
              1: enable SCI-bus for PCM
            B5: configuration bit
              0: SCI-bus A-configuration
              1: SCI-bus B-configuration
            B4: speed bit
              0: 7812.5 baud (low speed)
              1: 62500 baud (high speed)
          
          TCM: B3:B0
            B3: change settings bit
              0: leave settings
              1: change settings
            B2: enable bit
              0: disable SCI-bus for TCM
              1: enable SCI-bus for TCM
            B1: configuration bit
              0: SCI-bus A-configuration
              1: SCI-bus B-configuration
            B0: speed bit
              0: 7812.5 baud (low speed)
              1: 62500 baud (high speed)
**************************************************************************/
void configure_sci_bus(uint8_t data)
{
    // Check SCI-bus (PCM) change settings bit
    if (data & 0x80) // if change settings bit is set
    {
        cbi(current_sci_bus_settings[0], 7); // clear change settings bit
        
        if (data & 0x40) // if enable bit is set
        {
            sbi(current_sci_bus_settings[0], 6); // set enable bit
            
            if (data & 0x20) // if configuration bit is true
            {
                // SCI-bus (PCM) B-configuration is selected
                // Disable all A-configuration pins first
                digitalWrite(PA0, LOW);  // SCI-BUS_A_PCM_RX disabled
                digitalWrite(PA1, LOW);  // SCI-BUS_A_PCM_TX disabled
                digitalWrite(PA2, LOW);  // SCI-BUS_A_TCM_RX disabled
                digitalWrite(PA3, LOW);  // SCI-BUS_A_TCM_TX disabled
                // Enable B-configuration pins for PCM (TCM pins don't interfere here)
                digitalWrite(PA4, HIGH); // SCI-BUS_B_PCM_RX enabled
                digitalWrite(PA5, HIGH); // SCI-BUS_B_PCM_TX enabled
                pcm_enabled = true;
                
                sbi(current_sci_bus_settings[0], 5); // set configuration bit ("B")
            }
            else
            {
                // SCI-bus (PCM) A-configuration is selected
                // Disable all B-configuration pins first
                digitalWrite(PA4, LOW);  // SCI-BUS_B_PCM_RX disabled
                digitalWrite(PA5, LOW);  // SCI-BUS_B_PCM_TX disabled
                digitalWrite(PA6, LOW);  // SCI-BUS_B_TCM_RX disabled
                digitalWrite(PA7, LOW);  // SCI-BUS_B_TCM_TX disabled
                // Disable A-configuration pins for TCM first, they interfere
                digitalWrite(PA2, LOW);  // SCI-BUS_A_TCM_RX disabled
                digitalWrite(PA3, LOW);  // SCI-BUS_A_TCM_TX disabled
                // Enable A-configuration pins for PCM
                digitalWrite(PA0, HIGH); // SCI-BUS_A_PCM_RX enabled
                digitalWrite(PA1, HIGH); // SCI-BUS_A_PCM_TX enabled
                pcm_enabled = true;
                tcm_enabled = false;
                
                cbi(current_sci_bus_settings[0], 5); // clear configuration bit ("A")

                if (current_sci_bus_settings[0] & 0x04) // check if TCM is enabled (it interferes with PCM)
                {
                    cbi(current_sci_bus_settings[0], 2); // clear enable bit (TCM)
                }
            }
            if (data & 0x10) // if speed bit is true
            {
                if (!pcm_high_speed_enabled) // don't re-init if speed is unchanged
                {
                    pcm_init(HIBAUD); // 62500 baud
                    pcm_high_speed_enabled = true;
                    sbi(current_sci_bus_settings[0], 4); // set speed bit (62500 baud)
                }
            }
            else
            {
                if (pcm_high_speed_enabled) // don't re-init if speed is unchanged
                {
                    pcm_init(LOBAUD); // 7812.5 baud
                    pcm_high_speed_enabled = false;
                    cbi(current_sci_bus_settings[0], 4); // clear speed bit (7812.5 baud)
                }
            }
        }
        else
        {
            cbi(current_sci_bus_settings[0], 6); // clear PCM enable bit
            pcm_enabled = false;
            digitalWrite(PA0, LOW); // SCI-BUS_A_PCM_RX disabled
            digitalWrite(PA1, LOW); // SCI-BUS_A_PCM_TX disabled
            digitalWrite(PA4, LOW); // SCI-BUS_B_PCM_RX disabled
            digitalWrite(PA5, LOW); // SCI-BUS_B_PCM_TX disabled
        }
    }

    // Check SCI-bus (TCM) change settings bit
    if (data & 0x08) // if change settings bit is set
    {
        cbi(current_sci_bus_settings[0], 3); // clear change settings bit
        
        if (data & 0x04) // if enable bit is true
        {
            sbi(current_sci_bus_settings[0], 2); // set enable bit
            
            if (data & 0x02) // if configuration bit is true
            {
                // SCI-bus (TCM) B-configuration is selected
                // Disable all A-configuration pins first
                digitalWrite(PA0, LOW);  // SCI-BUS_A_PCM_RX disabled
                digitalWrite(PA1, LOW);  // SCI-BUS_A_PCM_TX disabled
                digitalWrite(PA2, LOW);  // SCI-BUS_A_TCM_RX disabled
                digitalWrite(PA3, LOW);  // SCI-BUS_A_TCM_TX disabled
                // Enable B-configuration pins for TCM (PCM pins don't interfere here)
                digitalWrite(PA6, HIGH); // SCI-BUS_B_TCM_RX enabled
                digitalWrite(PA7, HIGH); // SCI-BUS_B_TCM_TX enabled
                tcm_enabled = true;

                sbi(current_sci_bus_settings[0], 1); // set configuration bit ("B")
            }
            else
            {
                // SCI-bus (TCM) A-configuration is selected
                // Disable all B-configuration pins first
                digitalWrite(PA4, LOW);  // SCI-BUS_B_PCM_RX disabled
                digitalWrite(PA5, LOW);  // SCI-BUS_B_PCM_TX disabled
                digitalWrite(PA6, LOW);  // SCI-BUS_B_TCM_RX disabled
                digitalWrite(PA7, LOW);  // SCI-BUS_B_TCM_TX disabled
                // Disable A-configuration pins for PCM first, they interfere
                digitalWrite(PA0, LOW);  // SCI-BUS_A_PCM_RX disabled
                digitalWrite(PA1, LOW);  // SCI-BUS_A_PCM_TX disabled
                // Enable A-configuration pins for TCM
                digitalWrite(PA2, HIGH); // SCI-BUS_A_TCM_RX enabled
                digitalWrite(PA3, HIGH); // SCI-BUS_A_TCM_TX enabled
                tcm_enabled = true;
                pcm_enabled = false;

                cbi(current_sci_bus_settings[0], 1); // clear configuration bit ("A")

                if (current_sci_bus_settings[0] & 0x40) // check if PCM is enabled (it interferes with TCM)
                {
                    cbi(current_sci_bus_settings[0], 6); // clear enable bit (PCM)
                }
            }
            if (data & 0x01) // if speed bit is true
            {
                if (!tcm_high_speed_enabled) // don't re-init if speed is unchanged
                {
                    tcm_init(HIBAUD); // 62500 baud
                    tcm_high_speed_enabled = true;
                    sbi(current_sci_bus_settings[0], 0); // set speed bit (62500 baud)
                }
            }
            else
            {
                if (tcm_high_speed_enabled) // don't re-init if speed is unchanged
                {
                    tcm_init(LOBAUD); // 7812.5 baud
                    tcm_high_speed_enabled = false;
                    cbi(current_sci_bus_settings[0], 0); // clear speed bit (7812.5 baud)
                }
            }
        }
        else
        {
            cbi(current_sci_bus_settings[0], 2); // clear TCM enable bit
            tcm_enabled = false;
            digitalWrite(PA2, LOW); // SCI-BUS_A_TCM_RX disabled
            digitalWrite(PA3, LOW); // SCI-BUS_A_TCM_TX disabled
            digitalWrite(PA6, LOW); // SCI-BUS_B_TCM_RX disabled
            digitalWrite(PA7, LOW); // SCI-BUS_B_TCM_TX disabled
        }
    }
   
    if (pcm_enabled || tcm_enabled) sci_enabled = true;
    if (!pcm_enabled && !tcm_enabled) sci_enabled = false;
    send_usb_packet(from_usb, to_usb, settings, set_sci_bus, current_sci_bus_settings, 1); // acknowledge

} // end of configure_sci_bus


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

} // end of free_ram


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

} // end of read_avr_signature


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
        adc_supply_voltage = 500;
        battery_rd1 = 27000;
        battery_rd2 = 5000;
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
            adc_supply_voltage = (data[0] << 8) | data[1]; // stored value
            if (adc_supply_voltage == 0) adc_supply_voltage = 500; // default value
        }

        eep_result = eep.read(0x14, data, 2); // read R19 resistor value (for calibration)
        if (eep_result)
        {
            battery_rd1 = 27000; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            battery_rd1 = (data[0] << 8) | data[1]; // stored value
            if (battery_rd1 == 0) battery_rd1 = 27000; // default value
        }
        
        eep_result = eep.read(0x16, data, 2); // read R20 resistor value (for calibration)
        if (eep_result)
        {
            battery_rd2 = 5000; // default value
            send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
        }
        else
        {
            battery_rd2 = (data[0] << 8) | data[1]; // stored value
            if (battery_rd2 == 0) battery_rd2 = 5000; // default value
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
    
} // end of exteeprom_init


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
    
} // end of evaluate_eep_checksum


/*************************************************************************
Function: send_hwfw_info()
Purpose:  gather hardware version/date, assembly date and firmware date
          into an array and send through serial link
**************************************************************************/
void send_hwfw_info(void)
{
    uint8_t ret[26];
                                    
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

    send_usb_packet(from_usb, to_usb, response, hwfw_info, ret, 26);
    
} // end of evaluate_eep_checksum


/*************************************************************************
Function: check_battery_volts()
Purpose:  measure battery voltage through the OBD16 pin
**************************************************************************/
void check_battery_volts(void)
{
    for (uint16_t i = 0; i < 1000; i++) // get 1000 samples in quick succession
    {
        battery_adc += analogRead(BATT);
    }
    
    battery_adc /= 1000; // divide the sum by 1000 to get average value
    battery_volts = (uint16_t)(battery_adc*(adc_supply_voltage/100.0)/adc_max_value*((double)battery_rd1+(double)battery_rd2)/(double)battery_rd2*100.0); // resistor divider equation
    
    if (battery_volts < 600) // battery_volts < 6V
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
    
} // end of check_battery_volts


/*************************************************************************
Function: cli_parse()
Purpose:  copy space delimited words into the argument array
Note:     first argument is the command, the rest are its parameters
**************************************************************************/
void cli_parse(void)
{
    char *argument;
    int counter = 0;
    
    argument = strtok(line, " ");
    
    while (argument != NULL)
    {
        if (counter < CLI_MAX_NUM_ARGS)
        {
            if (strlen(argument) < CLI_ARG_BUF_SIZE)
            {
                strcpy(args[counter], argument);
                argument = strtok(NULL, " ");
                counter++;
            }
            else
            {
                cli_error = true;
                break;
            }
        }
        else
        {
            break;
        }
    }
    
} // end of cli_parse


/*************************************************************************
Function: cli_execute()
Purpose:  run command based on first argument in the array
**************************************************************************/
int cli_execute(void)
{  
    for (int i = 0; i < num_commands; i++)
    {
        if (strcmp(args[0], cli_commands_str[i]) == 0)
        {
            return (*commands_func[i])();
        }
    }
    return 0;
    
} // end of cli_execute


/*************************************************************************
Function: cmd_help()
Purpose:  CLI helper function, prints useful info about requested command
Note:     only second argument is checked ("help command")
**************************************************************************/
int cmd_help(void)
{
    blink_led(TX_LED);
    
    if (args[1] == NULL) // no second argument
    {
        usb_puts(ascii_autoreply);
    }
    else if (strcmp(args[1], cli_commands_str[0]) == 0)
    {
        usb_puts(ascii_autoreply);
    }
    else if (strcmp(args[1], cli_commands_str[1]) == 0)
    {
        usb_puts(ascii_autoreply);
    }
    else if (strcmp(args[1], cli_commands_str[2]) == 0)
    {
        usb_puts(ascii_autoreply);
    }
    else
    {
        usb_puts(ascii_autoreply);
    }
    
} // end of cmd_help


/*************************************************************************
Function: scanner_reset()
Purpose:  puts the scanner in an infinite loop and forces watchdog-reset
**************************************************************************/
int cmd_reset(void)
{
    send_usb_packet(from_usb, to_usb, reset, ok, ack, 1); // RX LED is on by now and this function lights up TX LED
    digitalWrite(ACT_LED, LOW); // blink all LEDs including this, good way to test if LEDs are working or not
    while (true); // enter into an infinite loop; watchdog timer doesn't get reset this way so it restarts the program eventually
    
} // end of cmd_reset


/*************************************************************************
Function: cmd_handshake()
Purpose:  sends handshake message to laptop
**************************************************************************/
int cmd_handshake(void)
{
    send_usb_packet(from_usb, to_usb, handshake, ok, handshake_array, 21);
    
} // end of cmd_handshake


/*************************************************************************
Function: cmd_status()
Purpose:  sends status message to laptop
**************************************************************************/
int cmd_status(void)
{
    scanner_status[0] = avr_signature[0];
    scanner_status[1] = avr_signature[1];
    scanner_status[2] = avr_signature[2];

    scanner_status[3] = hw_version[0];
    scanner_status[4] = hw_version[1];

    scanner_status[5] = hw_date[0];
    scanner_status[6] = hw_date[1];
    scanner_status[7] = hw_date[2];
    scanner_status[8] = hw_date[3];
    scanner_status[9] = hw_date[4];
    scanner_status[10] = hw_date[5];
    scanner_status[11] = hw_date[6];
    scanner_status[12] = hw_date[7];

    scanner_status[13] = assembly_date[0];
    scanner_status[14] = assembly_date[1];
    scanner_status[15] = assembly_date[2];
    scanner_status[16] = assembly_date[3];
    scanner_status[17] = assembly_date[4];
    scanner_status[18] = assembly_date[5];
    scanner_status[19] = assembly_date[6];
    scanner_status[20] = assembly_date[7];

    scanner_status[21] = (FW_DATE >> 56) & 0xFF;
    scanner_status[22] = (FW_DATE >> 48) & 0xFF;
    scanner_status[23] = (FW_DATE >> 40) & 0xFF;
    scanner_status[24] = (FW_DATE >> 32) & 0xFF;
    scanner_status[25] = (FW_DATE >> 24) & 0xFF;
    scanner_status[26] = (FW_DATE >> 16) & 0xFF;
    scanner_status[27] = (FW_DATE >> 8) & 0xFF;
    scanner_status[28] = FW_DATE & 0xFF;

    if (eep_present) scanner_status[29] = 0x01;
    else scanner_status[29] = 0x00;
    scanner_status[30] = eep_checksum[0];
    scanner_status[31] = eep_calculated_checksum;

    update_timestamp(current_timestamp);
    scanner_status[32] = current_timestamp[0];
    scanner_status[33] = current_timestamp[1];
    scanner_status[34] = current_timestamp[2];
    scanner_status[35] = current_timestamp[3];

    free_ram_available = free_ram();
    scanner_status[36] = (free_ram_available >> 8) & 0xFF;
    scanner_status[37] = free_ram_available & 0xFF;

    check_battery_volts();
    if (connected_to_vehicle) scanner_status[38] = 0x01;
    else scanner_status[38] = 0x00;
    scanner_status[39] = battery_volts_array[0];
    scanner_status[40] = battery_volts_array[1];

    if (ccd_enabled) scanner_status[41] = 0x01;
    else scanner_status[41] = 0x00;

    scanner_status[42] = (ccd_msg_rx_count >> 24) & 0xFF;
    scanner_status[43] = (ccd_msg_rx_count >> 16) & 0xFF;
    scanner_status[44] = (ccd_msg_rx_count >> 8) & 0xFF;
    scanner_status[45] = ccd_msg_rx_count & 0xFF;

    scanner_status[46] = (ccd_msg_tx_count >> 24) & 0xFF;
    scanner_status[47] = (ccd_msg_tx_count >> 16) & 0xFF;
    scanner_status[48] = (ccd_msg_tx_count >> 8) & 0xFF;
    scanner_status[49] = ccd_msg_tx_count & 0xFF;

    scanner_status[50] = current_sci_bus_settings[0];

    scanner_status[51] = (pcm_msg_rx_count >> 24) & 0xFF;
    scanner_status[52] = (pcm_msg_rx_count >> 16) & 0xFF;
    scanner_status[53] = (pcm_msg_rx_count >> 8) & 0xFF;
    scanner_status[54] = pcm_msg_rx_count & 0xFF;

    scanner_status[55] = (pcm_msg_tx_count >> 24) & 0xFF;
    scanner_status[56] = (pcm_msg_tx_count >> 16) & 0xFF;
    scanner_status[57] = (pcm_msg_tx_count >> 8) & 0xFF;
    scanner_status[58] = pcm_msg_tx_count & 0xFF;

    scanner_status[59] = (tcm_msg_rx_count >> 24) & 0xFF;
    scanner_status[60] = (tcm_msg_rx_count >> 16) & 0xFF;
    scanner_status[61] = (tcm_msg_rx_count >> 8) & 0xFF;
    scanner_status[62] = tcm_msg_rx_count & 0xFF;

    scanner_status[63] = (tcm_msg_tx_count >> 24) & 0xFF;
    scanner_status[64] = (tcm_msg_tx_count >> 16) & 0xFF;
    scanner_status[65] = (tcm_msg_tx_count >> 8) & 0xFF;
    scanner_status[66] = tcm_msg_tx_count & 0xFF;

    send_usb_packet(from_usb, to_usb, status, ok, scanner_status, 67);
    
} // end of cmd_status


/*************************************************************************
Function: handle_lcd()
Purpose:  write stuff to LCD
Note:     uncomment cases when necessary
**************************************************************************/
void handle_lcd(uint8_t bus, uint8_t *data, uint8_t startindex, uint8_t datalength)
{  
    if (lcd_enabled)
    {
        uint8_t message[datalength-startindex];
    
        for (uint8_t i = startindex; i < datalength; i++)
        {
            message[i-startindex] = data[i]; // make a local copy of the source array
        }
        
        switch (bus)
        {
            case from_ccd: // 0x01 - CCD-bus
            {
                switch (message[0]) // check ID-byte
                {
//                    case 0x00: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x01: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x02: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x03: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x04: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x05: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x06: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x07: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x08: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x09: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x0A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x0B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x0C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x0D: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x0E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x0F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x10: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x11: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x12: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x13: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x14: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x15: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x16: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x17: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x18: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x19: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x1F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x20: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x21: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x22: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x23: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x24: // VEHICLE SPEED
                    {
                        // TODO
                        break;
                    }
//                    case 0x25: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x26: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x27: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x28: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x29: // LAST ENGINE SHUTDOWN
                    {
                        // TODO
                        break;
                    }
//                    case 0x2A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x2B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x2C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x2D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x2E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x2F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x30: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x31: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x32: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x33: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x34: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x35: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x36: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x37: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x38: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x39: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x3A: // INSTRUMENT PANEL LAMP STATES (AIRBAG LAMP)
                    {
                        // TODO
                        break;
                    }
//                    case 0x3B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x3C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x3D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x3E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x3F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x40: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x41: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x42: // THROTTLE POSITION SENSOR | CRUISE CONTROL
                    {
                        // TODO
                        break;
                    }
//                    case 0x43: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x44: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x45: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x46: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x47: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x48: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x49: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x4A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x4B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x4C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x4D: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x4E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x4F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x50: // AIRBAG LAMP STATE
                    {
                        // TODO
                        break;
                    }
//                    case 0x51: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x52: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x53: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x54: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x55: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x56: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x57: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x58: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x59: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x5F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x60: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x61: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x62: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x63: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x64: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x65: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x66: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x67: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x68: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x69: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x6A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x6B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x6C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x6D: // VEHICLE IDENTIFICATION NUMBER (VIN)
                    {
                        vin_characters[message[1]-1] = char(message[2]); // store current character
                        //vin_string = String(vin_characters); // convert available characters to a string
                        break;
                    }
//                    case 0x6E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x6F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x70: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x71: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x72: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x73: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x74: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x75: // A/C HIGH SIDE PRESSURE
                    {
                        // TODO
                        break;
                    }
                    case 0x76: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x77: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x78: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x79: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x7A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x7B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x7C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x7D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x7E: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x7F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x80: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x81: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x82: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x83: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x84: // INCREMENT ODOMETER AND TRIPMETER
                    {
                        // TODO
                        break;
                    }
//                    case 0x85: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x86: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x87: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x88: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x89: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x8A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x8B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x8C: // ENGINE COOLANT TEMPERATURE | AMBIENT TEMP.
                    {
                        // TODO
                        break;
                    }
                    case 0x8D: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x8E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x8F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x90: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x91: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x92: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x93: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0x94: // INSTRUMENT PANEL GAUGE VALUE
                    {
                        // TODO
                        break;
                    }
                    case 0x95: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0x96: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x97: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x98: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x99: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9A: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9B: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9C: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9D: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9E: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0x9F: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA2: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xA4: // INSTRUMENT PANEL LAMP STATES
                    {
                        // TODO
                        break;
                    }
//                    case 0xA5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA6: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xA8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xA9: // LAST ENGINE SHUTDOWN
                    {
                        // TODO
                        break;
                    }
//                    case 0xAA: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xAB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xAC: // VEHICLE INFORMATION / BODY TYPE
                    {
                        // TODO
                        break;
                    }
//                    case 0xAD: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xAE: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xAF: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xB0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xB1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xB2: // DIAGNOSTIC REQUEST MESSAGE
                    {
                        // TODO
                        break;
                    }
//                    case 0xB3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xB4: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0xB5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xB6: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0xB7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xB8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xB9: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xBA: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xBB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xBC: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xBD: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xBE: // IGNITION SWITCH POSITION
                    {
                        // TODO
                        break;
                    }
//                    case 0xBF: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC2: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC4: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC6: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xC9: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xCA: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xCB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xCC: // ACCUMULATED MILEAGE
                    {
                        // TODO
                        break;
                    }
                    case 0xCD: // ???
                    {
                        // TODO
                        break;
                    }
                    case 0xCE: // VEHICLE DISTANCE / ODOMETER VALUE
                    {
                        // TODO
                        break;
                    }
//                    case 0xCF: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD2: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xD4: // BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE
                    {
                        // TODO
                        break;
                    }
//                    case 0xD5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD6: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xD9: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xDA: // INSTRUMENT PANEL LAMP STATES (CHECK ENGINE)
                    {
                        // TODO
                        break;
                    }
//                    case 0xDB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xDC: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xDD: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xDE: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xDF: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE2: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xE4: // ENGINE SPEED | INTAKE MANIFOLD ABS. PRESSURE
                    {
                        // TODO
                        break;
                    }
//                    case 0xE5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE6: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xE9: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xEA: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xEB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xEC: // VEHICLE INFORMATION / LIMP STATES / FUEL TYPE
                    {
                        // TODO
                        break;
                    }
//                    case 0xED: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xEE: // TRIP DISTANCE / TRIPMETER VALUE
                    {
                        // TODO
                        break;
                    }
//                    case 0xEF: // ???
//                    {
//                      // TODO
//                      break;
//                    }
//                    case 0xF0: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xF1: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xF2: // DIAGNOSTIC RESPONSE MESSAGE
                    {
                        // TODO
                        break;
                    }
//                    case 0xF3: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xF4: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xF5: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xF6: // ???
                    {
                        // TODO
                        break;
                    }
//                    case 0xF7: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xF8: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xF9: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xFA: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xFB: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xFC: // ???
//                    {
//                        // TODO
//                        break;
//                    }
//                    case 0xFD: // ???
//                    {
//                        // TODO
//                        break;
//                    }
                    case 0xFE: // INTERIOR LAMP DIMMING
                    {
                        // TODO
                        break;
                    }
                    case 0xFF: // CCD-BUS WAKING UP
                    {
                        // TODO
                        break;
                    }
                }
                break;
            }
            case from_pcm: // 0x02 - SCI-bus (PCM)
            {
                // TODO
                break;
            }
            case from_tcm: // 0x03 - SCI-bus (TCM)
            {
                // TODO
                break;
            }
            default:
            {
                // TODO
                break;
            }
        }
    }
    
} // end of handle_lcd


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
                    send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occured, err, 1);
                    return;
                }

                length_hb = usb_getc() & 0xFF; // read first length byte
                length_lb = usb_getc() & 0xFF; // read second length byte
        
                // Calculate how much more bytes should we read by combining the two length bytes into a word.
                bytes_to_read = (length_hb << 8) + length_lb + 1; // +1 CHECKSUM byte
                
                // Calculate the exact size of the payload.
                payload_length = bytes_to_read - 3; // in this case we have to be careful not to count data code byte, sub-data code byte and checksum byte
        
                // Do not let this variable sink below zero.
                if (payload_length < 0) payload_length = 0; // !!!
        
                // Maximum packet length is 1024 bytes; can't accept larger packets 
                // and can't accept packet without datacode and subdatacode.
                if ((payload_length > MAX_PAYLOAD_LENGTH) || ((bytes_to_read - 1) < 2))
                {
                    send_usb_packet(from_usb, to_usb, ok_error, error_length_invalid_value, err, 1);
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
                    send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occured, err, 1);
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
                calculated_checksum = length_hb + length_lb + datacode + subdatacode; // add the first few bytes together manually
        
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
                    send_usb_packet(from_usb, to_usb, ok_error, error_checksum_invalid_value, err, 1);
                    return; // exit, let the loop call this function again
                }

                // If everything is good then continue processing the packet...
                // Find out the source and the target of the packet by examining the DATA CODE byte's high nibble (upper 4 bits).
                uint8_t source = (datacode >> 6) & 0x03; // keep the upper two bits
                uint8_t target = (datacode >> 4) & 0x03; // keep the lower two bits
            
                // Extract command value from the low nibble (lower 4 bits).
                uint8_t command = datacode & 0x0F;
            
                // Source is ignored, the packet must come from an external computer through USB
                switch (target) // evaluate target value
                {
                    case to_usb: // 0x00 - scanner is the target
                    {
                        switch (command) // evaluate command
                        {
                            case reset: // 0x00 - reset scanner request
                            {
                                cmd_reset();
                                break; // not necessary but every case needs a break
                            }
                            case handshake: // 0x01 - handshake request coming from an external computer
                            {
                                cmd_handshake();
                                if (subdatacode == 0x01)
                                {
                                    send_hwfw_info();
                                    cmd_status();
                                }
                                break;
                            }
                            case status: // 0x02 - status report request
                            {
                                cmd_status();
                                break;
                            }
                            case settings: // 0x03 - change scanner settings
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case heartbeat: // 0x00 - ACT_LED flashing interval is stored in payload as 4 bytes
                                    {
                                        if (!payload_bytes || (payload_length < 4)) // at least 4 bytes are necessary to change this setting
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint16_t flashing_interval = (cmd_payload[0] << 8) + cmd_payload[1]; // 0-65535 milliseconds
                                        if (flashing_interval == 0) heartbeat_enabled = false; // zero value is allowed, meaning no heartbeat
                                        else
                                        {
                                            heartbeat_interval = flashing_interval;
                                            heartbeat_enabled = true;
                                        }
                                        
                                        uint16_t blink_duration = (cmd_payload[2] << 8) + cmd_payload[3]; // 0-65535 milliseconds
                                        if (blink_duration > 0) led_blink_duration = blink_duration; // zero value is not allowed, this applies to all 3 status leds! (rx, tx, act)
                                        
                                        send_usb_packet(from_usb, to_usb, settings, heartbeat, cmd_payload, 4); // acknowledge
                                        break;
                                    }
                                    case set_ccd_bus: // 0x01 - ON-OFF state is stored in payload
                                    {
                                        if (!payload_bytes)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        switch (cmd_payload[0])
                                        {
                                            case 0x00: // disable ccd-bus transceiver
                                            {
                                                ccd_enabled = false;
                                                ccd_clock_generator(STOP);
                                                ccd_rx_flush();
                                                ccd_tx_flush();
                                                send_usb_packet(from_usb, to_usb, settings, set_ccd_bus, cmd_payload, 1); // acknowledge
                                                break;
                                            }
                                            case 0x01: // enable ccd-bus transceiver
                                            {
                                                ccd_enabled = true;
                                                ccd_clock_generator(START);
                                                send_usb_packet(from_usb, to_usb, settings, set_ccd_bus, cmd_payload, 1); // acknowledge
                                                break;
                                            }
                                            default: // other values are not valid
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, already enabled
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    case set_sci_bus: // 0x02 - ON-OFF state, A/B configuration and speed are stored in payload
                                    {
                                        if (!payload_bytes) // if no payload byte is present
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        configure_sci_bus(cmd_payload[0]); // pass settings to this function
                                        break;
                                    }
                                    case set_repeat_behavior: // 0x03
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        repeated_msg_interval = (cmd_payload[0] << 8) + cmd_payload[1]; // 0-65535 milliseconds
                                        repeated_msg_increment = (cmd_payload[2] << 8) + cmd_payload[3]; // 0-65535

                                        //uint8_t ret[1] = { set_repeat_behavior };
                                        //send_usb_packet(from_usb, to_usb, ok_error, ok, ret, 1); // acknowledge
                                        send_usb_packet(from_usb, to_usb, settings, set_repeat_behavior, cmd_payload, 4); // acknowledge
                                        break;
                                    }
                                    case set_lcd: // 0x04 - LCD ON/OFF
                                    {
                                        if (!payload_bytes)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        if (cmd_payload[0] == 0x00) lcd_enabled = false;
                                        else lcd_enabled = true;
                                
                                    }
                                    default: // other values are not used
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case request: // 0x04 - request from the scanner
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case hwfw_info: // 0x00 - hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
                                    {
                                        send_hwfw_info();
                                        break;
                                    }
                                    case timestamp: // 0x01 - timestamp / MCU counter value (milliseconds elapsed)
                                    {
                                        update_timestamp(current_timestamp); // this function updates the global byte array "current_timestamp" with the current time
                                        send_usb_packet(from_usb, to_usb, response, timestamp, current_timestamp, 4);
                                        break;
                                    }
                                    case battery_voltage:
                                    {
                                        check_battery_volts();
                                        send_usb_packet(from_usb, to_usb, response, battery_voltage, battery_volts_array, 2);
                                        break;
                                    }
                                    case exteeprom_checksum:
                                    {
                                        evaluate_eep_checksum();
                                        break;
                                    }
                                    default: // other values are not used
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case debug: // 0x0E - debug
                            {
                                switch (subdatacode)
                                {
                                    // TODO
                                    case 0x01: // broadcast random CCD-bus messages
                                    {
                                        if (!payload_bytes)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                        }
                                        else
                                        {
                                            if (payload_length > 4)
                                            {
                                                switch (cmd_payload[0])
                                                {
                                                    case 0x00:
                                                    {
                                                        generate_random_ccd_msgs = false;
                                                        random_ccd_msg_interval_min = 0;
                                                        random_ccd_msg_interval_max = 0;
                                                        random_ccd_msg_interval = 0;
                                                        send_usb_packet(from_usb, to_usb, debug, 0x01, ack, 1); // acknowledge
                                                        break;
                                                    }
                                                    case 0x01:
                                                    {
                                                        generate_random_ccd_msgs = true;
                                                        random_ccd_msg_interval_min = (cmd_payload[1] << 8) | cmd_payload[2];
                                                        random_ccd_msg_interval_max = (cmd_payload[3] << 8) | cmd_payload[4];
                                                        random_ccd_msg_interval = random(random_ccd_msg_interval_min, random_ccd_msg_interval_max);
                                                        send_usb_packet(from_usb, to_usb, debug, 0x01, ack, 1); // acknowledge
                                                        break;
                                                    }
                                                    default:
                                                    {
                                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            }
                                        }
                                        break;
                                    }
                                    case 0x02: // read external EEPROM
                                    {
                                        if (eep_present)
                                        {
                                            uint8_t address_hb = cmd_payload[0];
                                            uint8_t address_lb = cmd_payload[1];
                                            uint16_t address = (address_hb << 8) | address_lb;
                                            uint8_t value = eep.read(address);
                                            uint8_t data[3] = { address_hb, address_lb, value };
                                            send_usb_packet(from_usb, to_usb, debug, 0x02, data, 3); // send external EEPROM value back to the laptop
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case 0x03: // write external EEPROM
                                    {
                                        if (eep_present)
                                        {
                                            uint8_t address_hb = cmd_payload[0];
                                            uint8_t address_lb = cmd_payload[1];
                                            uint16_t address = (address_hb << 8) | address_lb;
                                            uint8_t value = cmd_payload[2];

                                            // Disable hardware write protection (EEPROM chip has a pullup resistor on its WP-pin!)
                                            DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                            PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection

                                            eep_result = eep.write(address, value);

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                            }
                                            else // ok, calculate checksum again
                                            {
                                                uint8_t temp_checksum = 0;

                                                for (uint8_t i = 0; i < 255; i++) // add all 255 bytes together and skip last byte (where the result of this calculation goes) by setting the second parameter to 255 instead of 256
                                                {
                                                    temp_checksum += eep.read(i); // checksum variable will roll over several times but it's okay, this is its purpose
                                                }
                                                
                                                eep_result = eep.write(255, temp_checksum); // place checksum byte at the last position of the array

                                                if (eep_result) // error
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                                }
                                            }

                                            PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection

                                            uint8_t reading = eep.read(address);
                                            uint8_t data[3] = { address_hb, address_lb, reading };
                                            send_usb_packet(from_usb, to_usb, debug, 0x03, data, 3); // send external EEPROM value back to the laptop for confirmation
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case 0x04: // set arbitrary UART-speed on different channels
                                    {
                                        if (!payload_bytes)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                        }
                                        else
                                        {
                                            if (payload_length > 1)
                                            {
                                                switch (cmd_payload[0])
                                                {
                                                    case 0x01: // CCD-bus
                                                    {
                                                        switch (cmd_payload[1])
                                                        {
                                                            case 0x01: // 976.5 baud
                                                            {
                                                                ccd_init(ELBAUD);
                                                                ccd_rx_flush();
                                                                ccd_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x02: // 7812.5 baud
                                                            {
                                                                ccd_init(LOBAUD);
                                                                ccd_rx_flush();
                                                                ccd_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x03: // 62500 baud
                                                            {
                                                                ccd_init(HIBAUD);
                                                                ccd_rx_flush();
                                                                ccd_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x04: // 125000 baud
                                                            {
                                                                ccd_init(EHBAUD);
                                                                ccd_rx_flush();
                                                                ccd_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            default:
                                                            {
                                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    case 0x02: // SCI-bus (PCM)
                                                    {
                                                        switch (cmd_payload[1])
                                                        {
                                                            case 0x01: // 976.5 baud
                                                            {
                                                                pcm_init(ELBAUD);
                                                                pcm_rx_flush();
                                                                pcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x02: // 7812.5 baud
                                                            {
                                                                pcm_init(LOBAUD);
                                                                pcm_rx_flush();
                                                                pcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x03: // 62500 baud
                                                            {
                                                                pcm_init(HIBAUD);
                                                                pcm_rx_flush();
                                                                pcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x04: // 125000 baud
                                                            {
                                                                pcm_init(EHBAUD);
                                                                pcm_rx_flush();
                                                                pcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            default:
                                                            {
                                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    case 0x03: // SCI-bus (TCM)
                                                    {
                                                        switch (cmd_payload[1])
                                                        {
                                                            case 0x01: // 976.5 baud
                                                            {
                                                                tcm_init(ELBAUD);
                                                                tcm_rx_flush();
                                                                tcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x02: // 7812.5 baud
                                                            {
                                                                tcm_init(LOBAUD);
                                                                tcm_rx_flush();
                                                                tcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x03: // 62500 baud
                                                            {
                                                                tcm_init(HIBAUD);
                                                                tcm_rx_flush();
                                                                tcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            case 0x04: // 125000 baud
                                                            {
                                                                tcm_init(EHBAUD);
                                                                tcm_rx_flush();
                                                                tcm_tx_flush();
                                                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                                                break;
                                                            }
                                                            default:
                                                            {
                                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    default:
                                                    {
                                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            }
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            case ok_error: // 0x0F - OK/ERROR message
                            {
                                // TODO, although it's rare that the laptop sends an error message out of the blue
                                send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                                break;
                            }
                            default: // other values are not used
                            {
                                send_usb_packet(from_usb, to_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case to_usb:
                    case to_ccd: // 0x01 - CCD-bus is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the CCD-bus
                            {
                                switch (subdatacode) // evaluate  SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x00 - stop message transmission (single and repeated as well)
                                    {
                                        ccd_repeated_msg_count = 0;
                                        ccd_repeated_msg_ptr = 0;
                                        ccd_repeated_msg_bytes_ptr = 0;
                                        ccd_repeated_messages = false;
                                        ccd_repeated_messages_iteration = false;
                                        ccd_repeated_messages_next = false;
                                        ccd_msg_pending = false;
                                        
                                        ret[0] = to_ccd; // improvised payload array with only 1 element which is the target bus
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x01 - send message to the CCD-bus, message is stored in payload 
                                    {
                                        if ((payload_length > 0) && (payload_length <= CCD_RX1_BUFFER_SIZE)) 
                                        {
                                            // Fill the pending buffer with the message to be sent
                                            for (uint8_t i = 0; i < payload_length; i++)
                                            {
                                                ccd_msg_to_send[i] = cmd_payload[i];
                                            }
                                            ccd_msg_to_send_ptr = payload_length;
                    
                                            // Check if the checksum byte in the message is correct
                                            uint8_t tmp = calculate_checksum(ccd_msg_to_send, 0, ccd_msg_to_send_ptr - 1);
    
                                            // Correct the last checksum byte if wrong
                                            if (ccd_msg_to_send[ccd_msg_to_send_ptr - 1] != tmp)
                                            {
                                                ccd_msg_to_send[ccd_msg_to_send_ptr - 1] = tmp;
                                            }
        
                                            ccd_msg_pending = true; // set flag so the main loop knows there's something to do
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    case repeated_msg: // 0x02 - send repeated message(s) to the CCD-bus
                                    {
                                        if ((payload_length > 2) && (payload_length <= CCD_RX1_BUFFER_SIZE)) // at least 3 bytes are required
                                        {
                                            switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                            {
                                                case 0x00: // no iteration needed, send the same message again ang again, 1 message only!
                                                {
                                                    // Payload structure example:
                                                    // 00 06 B2 20 22 00 00 F4
                                                    // -----------------------------------
                                                    // 00: no message iteration needed, send the same message(s) again ang again
                                                    // 06: message length
                                                    // B2 20 22 00 00 F4: message

                                                    for (uint8_t i = 2; i < (payload_length); i++)
                                                    {
                                                        ccd_repeated_msg_bytes[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                    }
            
                                                    ccd_repeated_msg_bytes_ptr = cmd_payload[1]; // message length
                                                    ccd_repeated_messages = true; // set flag
                                                    ccd_repeated_messages_iteration = false; // set flag
                                                    ccd_repeated_messages_next = true; // set flag
                                                    break;
                                                }
                                                case 0x01: // message iteration needed, 1 message only!
                                                {
                                                    // TODO
                                                    break;
                                                }
                                                default:
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
    
                            default: // other values are not used
                            {
                                send_usb_packet(from_usb, to_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case to_ccd:
                    case to_pcm: // 0x02 - SCI-bus (PCM) is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the SCI-bus (PCM)
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x00 - stop message transmission (single and repeated as well)
                                    {
                                        pcm_repeated_msg_count = 0;
                                        pcm_repeated_msg_ptr = 0;
                                        pcm_repeated_msg_bytes_ptr = 0;
                                        pcm_repeated_messages = false;
                                        pcm_repeated_messages_iteration = false;
                                        pcm_repeated_messages_next = false;
                                        pcm_msg_pending = false;
                                        
                                        ret[0] = to_pcm;
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x01 - send message to the SCI-bus, message is stored in payload 
                                    {
                                        if ((payload_length > 0) && (payload_length <= PCM_RX2_BUFFER_SIZE))
                                        {
                                            for (uint16_t i = 0; i < payload_length; i++) // fill the pending buffer with the message to be sent
                                            {
                                                pcm_msg_to_send[i] = cmd_payload[i];
                                            }
                                            // Checksum isn't used on SCI-bus transmissions, except when receiving fault codes.
                                            pcm_msg_to_send_ptr = payload_length;
                                            pcm_msg_pending = true; // set flag so the main loop knows there's something to do
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    case repeated_msg: // 0x02 - send repeated message(s) to the SCI-bus
                                    {
                                        if ((payload_length > 2) && (payload_length <= PCM_RX2_BUFFER_SIZE)) // at least 3 bytes are required
                                        {
                                            switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                            {
                                                case 0x00: // no iteration needed, send the same message again ang again, 1 message only!
                                                {
                                                    // Payload structure example:
                                                    // 00 06 F4 0A 0B 0C 0D 11
                                                    // -----------------------------------
                                                    // 00: no message iteration needed, send the same message(s) again ang again
                                                    // 06: message length
                                                    // F4 0A 0B 0C 0D 11: message

                                                    for (uint8_t i = 2; i < (payload_length); i++)
                                                    {
                                                        pcm_repeated_msg_bytes[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                    }
            
                                                    pcm_repeated_msg_bytes_ptr = cmd_payload[1]; // message length
                                                    pcm_repeated_messages = true; // set flag
                                                    pcm_repeated_messages_iteration = false; // set flag
                                                    pcm_repeated_messages_next = true; // set flag

                                                    ret[0] = to_pcm;
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
                                                    break;
                                                }
                                                case 0x01: // message iteration needed, 1 message only!
                                                {
                                                    // NOT TESTED
                                                    // !!!
                                                    // Payload structure example:
                                                    // 01 04 26 00 00 00 26 01 FF FF 
                                                    // -----------------------------------
                                                    // 01: message iteration needed, 1 message only!
                                                    // 04: message length
                                                    // 26 00 00 00: first message in the sequence
                                                    // 26 01 FF FF: last message in the sequence

                                                    pcm_repeated_messages = true; // set flag
                                                    pcm_repeated_messages_iteration = true; // set flag
                                                    pcm_repeated_messages_next = true; // set flag
                                                    pcm_repeated_msg_increment = cmd_payload[1];

                                                    ret[0] = to_pcm;
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
                                                    break;
                                                }
                                                default:
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            default: // other values are not used.
                            {
                                send_usb_packet(from_usb, to_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case to_pcm:
                    case to_tcm: // 0x03 - SCI-bus (TCM) is the target
                    {
                        switch (command) // evaluate command
                        {
                            case msg_tx: // 0x06 - send message to the SCI-bus (TCM)
                            {
                                switch (subdatacode) // evaluate SUB-DATA CODE byte
                                {
                                    case stop_msg_flow: // 0x00 - stop message transmission (single and repeated as well)
                                    {
                                        tcm_repeated_msg_count = 0;
                                        tcm_repeated_msg_ptr = 0;
                                        tcm_repeated_msg_bytes_ptr = 0;
                                        tcm_repeated_messages = false;
                                        tcm_repeated_messages_iteration = false;
                                        tcm_repeated_messages_next = false;
                                        tcm_msg_pending = false;
                                        
                                        ret[0] = to_tcm;
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                        break;
                                    }
                                    case single_msg: // 0x01 - send message to the SCI-bus, message is stored in payload 
                                    {
                                        if ((payload_length > 0) && (payload_length <= TCM_RX3_BUFFER_SIZE))
                                        {
                                            for (uint16_t i = 0; i < payload_length; i++) // fill the pending buffer with the message to be sent
                                            {
                                                tcm_msg_to_send[i] = cmd_payload[i];
                                            }
                                            // Checksum isn't used on SCI-bus transmissions, except when receiving fault codes.
                                            tcm_msg_to_send_ptr = payload_length;
                                            tcm_msg_pending = true; // set flag so the main loop knows there's something to do
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    case repeated_msg: // 0x02 - send message(s) to the SCI-bus, number of messages, repeat interval(s) are stored in payload
                                    {
                                        if ((payload_length > 2) && (payload_length <= TCM_RX3_BUFFER_SIZE)) // at least 3 bytes are required
                                        {
                                            switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                            {
                                                case 0x00: // no iteration needed, send the same message again ang again, 1 message only!
                                                {
                                                    // Payload structure example:
                                                    // 00 06 F4 0A 0B 0C 0D 11
                                                    // -----------------------------------
                                                    // 00: no message iteration needed, send the same message(s) again ang again
                                                    // 06: message length
                                                    // F4 0A 0B 0C 0D 11: message

                                                    for (uint8_t i = 2; i < (payload_length); i++)
                                                    {
                                                        tcm_repeated_msg_bytes[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                    }
            
                                                    tcm_repeated_msg_bytes_ptr = cmd_payload[1]; // message length
                                                    tcm_repeated_messages = true; // set flag
                                                    tcm_repeated_messages_iteration = false; // set flag
                                                    tcm_repeated_messages_next = true; // set flag

                                                    ret[0] = to_tcm;
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
                                                    break;
                                                }
                                                case 0x01: // message iteration needed, 1 message only!
                                                {
                                                    // NOT TESTED
                                                    // !!!
                                                    // Payload structure example:
                                                    // 01 04 26 00 00 00 26 01 FF FF 
                                                    // -----------------------------------
                                                    // 01: message iteration needed, 1 message only!
                                                    // 04: message length
                                                    // 26 00 00 00: first message in the sequence
                                                    // 26 01 FF FF: last message in the sequence

                                                    tcm_repeated_messages = true; // set flag
                                                    tcm_repeated_messages_iteration = true; // set flag
                                                    tcm_repeated_messages_next = true; // set flag
                                                    tcm_repeated_msg_increment = cmd_payload[1];

                                                    ret[0] = to_tcm;
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
                                                    break;
                                                }
                                                default:
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                        }
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1);
                                        break;
                                    }
                                }
                                break;
                            }
                            default: // other values are not used.
                            {
                                send_usb_packet(from_usb, to_usb, ok_error, error_datacode_invalid_command, err, 1);
                                break;
                            }
                        }
                        break;
                    } // case to_tcm:
                } // switch (target)   
                break;
            }
            case ASCII_SYNC_BYTE: // 0x3E, ">" symbol
            {
                String line_string;
                bool eol = false;
                uint8_t raw_byte = 0;

                // Read command line
                while (!eol) 
                {
                    while (usb_rx_available() == 0); // wait for character to be available to read
                    raw_byte = usb_getc() & 0xFF;
                    if ((raw_byte == 0x0A) || (raw_byte == 0x0D)) // new-line or carriage return character
                    {
                        eol = true; // stop reading, it's the end of line (EOL)
                    }
                    else
                    {
                        line_string += char(raw_byte); // add this byte to the string as an ascii character
                        eol = false;
                    }
                }
                if ((usb_peek() == 0x0A) || (usb_peek() == 0x0D)) usb_getc(); // remove additional terminating character if any

                if (line_string.length() < CLI_BUF_SIZE)
                {
                    line_string.toCharArray(line, CLI_BUF_SIZE); // convert "line_string" string to "line" character array
                    //blink_led(TX_LED);
                    //usb_puts(line); // while debugging echo the original command back
                    //usb_putc(0x0A); // and add a new line at the end
                }
                else
                {
                    cli_error = true;
                }

                // Parse command
                if (!cli_error) cli_parse();

                // Execute command
                if (!cli_error) cli_execute();
                
                // Reset CLI buffers
                memset(line, 0, CLI_BUF_SIZE);
                memset(args, 0, sizeof(args[0][0]) * CLI_MAX_NUM_ARGS * CLI_ARG_BUF_SIZE);
                cli_error = false; // re-arm flag
                break;
            }
            default:
            {
                // TODO, the main loop itself will get rid of garbage data here, until a valid sync byte is found
                break;
            }
        }
    }
    else
    {
      // TODO
    }
     
} // end of handle_usb_data


/*************************************************************************
Function: handle_ccd_data()
Purpose:  handle CCD-bus messages
**************************************************************************/
void handle_ccd_data(void)
{
    if (ccd_enabled) // when clock signal is fed into the ccd-chip
    {
        if (ccd_idle) // CCD-bus is idling, find out if there's a message in the circular buffer
        {
            if ((ccd_bytes_count > 0) && (ccd_msg_in_buffer == 1)) // the exact message length is recorded in the CCD-bus idle ISR so it's pretty accurate
            {
                uint8_t usb_msg[4+ccd_bytes_count]; // create local array which will hold the timestamp and the CCD-bus message
                update_timestamp(current_timestamp); // get current time for the timestamp
                
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < ccd_bytes_count; i++) // put every byte in the CCD-bus message after the timestamp
                {
                    usb_msg[4+i] = ccd_getc() & 0xFF; // new message bytes may arrive in the circular buffer but this way only one message is removed
                }
                
                // TODO: check here if echo is expected from a pending message, otherwise good to know if a custom message is heard by the other modules
                send_usb_packet(from_ccd, to_usb, msg_rx, single_msg, usb_msg, 4+ccd_bytes_count); // send CCD-bus message back to the laptop
                
                //handle_lcd(from_ccd, usb_msg, 4, 4+ccd_bytes_count); // pass message to LCD handling function, start at the 4th byte (skip timestamp)

                if (ccd_repeated_messages && !ccd_repeated_messages_iteration)
                {
                    bool match = true;
                    for (uint8_t i = 0; i < ccd_bytes_count; i++)
                    {
                        if (ccd_msg_to_send[i] != usb_msg[4+i]) match = false; // compare received bytes with message sent
                    }

                    if (match) ccd_repeated_messages_next = true; // if echo is correct prepare next message
                }
                else if (ccd_repeated_messages && ccd_repeated_messages_iteration)
                {
                    // TODO
                }
                
                ccd_bytes_count = 0; // force ISR to update this value again so we don't end up here in the next program loop
                ccd_msg_in_buffer = 0;
                ccd_msg_rx_count++;
            }
            else // there are multiple ccd-bus messages in the buffer
            {
                // TODO
                // For now just trash the whole buffer and wait for another message
                ccd_rx_flush();
                ccd_bytes_count = 0;
                ccd_msg_in_buffer = 0;
            }

            if (generate_random_ccd_msgs && (random_ccd_msg_interval > 0))
            {
                current_millis = millis(); // check current time
                if ((current_millis - previous_random_ccd_msg_millis) > random_ccd_msg_interval)
                {
                    previous_random_ccd_msg_millis = current_millis;
                    ccd_msg_to_send_ptr = random(3, 7); // random message length between 3 and 6 bytes
                    for (uint8_t i = 0; i < ccd_msg_to_send_ptr - 2; i++)
                    {
                        ccd_msg_to_send[i] = random(256); // generate random bytes
                    }
                    ccd_msg_to_send[ccd_msg_to_send_ptr - 1] = calculate_checksum(ccd_msg_to_send, 0, ccd_msg_to_send_ptr - 1);
                    ccd_msg_pending = true;
                    random_ccd_msg_interval = random(random_ccd_msg_interval_min, random_ccd_msg_interval_max); // generate new delay value between fake messages
                }
            }

            // Repeated messages are prepared here
            if (ccd_repeated_messages && (ccd_rx_available() == 0))
            {
                current_millis = millis(); // check current time
                if ((current_millis - ccd_repeated_msg_last_millis) > repeated_msg_interval) // wait between messages
                {
                  ccd_repeated_msg_last_millis = current_millis;
                  if (ccd_repeated_messages_next && !ccd_repeated_messages_iteration) // no iteration, same message over and over again
                  {
                      for (uint16_t i = 0; i < ccd_repeated_msg_bytes_ptr; i++) // fill the pending buffer with the message to be sent
                      {
                          ccd_msg_to_send[i] = ccd_repeated_msg_bytes[i];
                      }
                      ccd_msg_to_send_ptr = ccd_repeated_msg_bytes_ptr;
                      ccd_msg_pending = true; // set flag
                      ccd_repeated_messages_next = false;
                  }
                  else if (ccd_repeated_messages_next && ccd_repeated_messages_iteration) // iteration, message is incremented for every repeat according to settings
                  {
                      // TODO
                  }
                }
            }

            if (ccd_msg_pending) // received over usb connection, checksum corrected there (if wrong)
            {     
                for (uint8_t i = 0; i < ccd_msg_to_send_ptr; i++) // since the bus is already idling start filling the transmit buffer with bytes right away
                {
                    ccd_putc(ccd_msg_to_send[i]); // transmission occurs automatically if the code senses at least 1 byte in this buffer
                }
                ccd_msg_to_send_ptr = 0; // reset pointer, TODO: perhaps don't reset it until proper echo is heard on the bus
                ccd_msg_pending = false; // re-arm, make it possible to send a message again, TODO: same as above
                ccd_msg_tx_count++;
            }
        }
    }

} // end of handle_ccd_data


/*************************************************************************
Function: handle_sci_data()
Purpose:  handle SCI-bus messages from both PCM and TCM
Note:     taken from SAE J2610:
          Although the SCI communication link is a full-duplex serial interface, in some communication sessions a half-duplex
          mode is required. Half-duplex mode implies that the SCI communication system is capable of bidirectional
          communications but only in one direction at a time. In the half-duplex mode, every data frame sent
          by the diagnostic tester is "echoed" by the ECU. The tester shall not send the next data frame until receiving
          the expected echo from the ECU. Upon receiving the echo, the tester shall assume the ECU is ready for the
          next sequential data frame. Although the ECU shall echo the appropriate data frame as intended, this shall not
          imply that the command sequence has been fully serviced by the ECU. An additional delay time may be
          required by the ECU for processing the request before a command sequence has been completed. This half-duplex
          mode allows the timing sequence to be completely managed by the diagnostic tester. This strategy
          prevents data frame overruns from occurring, but also effectively limits the data throughput. That is, the
          diagnostic tester shall wait for the echoed response character before sending the next request.
          
          Low speed mode example (half-duplex):
          TXD1 - T1 - RXD1 - T2 - TXD2 - T1 - RXD2 - T3 - RXD3 - T4 - RXD4
          (tx byte, wait for echo, tx next byte, wait for echo, wait for processing, rx byte, wait, rx another byte)

          Additionally, multiple-frame command sequences may be sent to an ECU without echoing each character. In
          this case, a response shall occur within a given timeframe. Otherwise, a communication timeout event shall be
          declared, and the ECU shall disregard the entire command sequence.

          High speed mode example (full duplex):
          TXD1 - T5 - TXD2 - T5 - ... - TXDN - T3 - RXD1 - T4 - RXD2 - T4 - ... - RXDN
          (tx byte, tx next byte, ... , tx last byte, wait, rx byte, rx next byte, ... , rx last byte)

                                                   Low-Speed Mode      High-Speed Mode
                                                   min  (ms)  max      min  (ms)   max
          T1: ECU INTER-FRAME RESPONSE DELAY;        0         20        0           1
              Defines the response delay from
              the ECU after receiving a valid 
              data transmission from the tester.
              
          T2: TESTER INTER-FRAME REQUEST DELAY;      0        100        0           1
              Defines the request delay from the 
              tester after receiving a valid
              data acknowledgement from the ECU.

          T3: ECU INTER-MESSAGE PROCESSING           0         50        0           5
              DELAY FOR INITIAL DATA
              TRANSMISSION;
              Defines the response delay from
              the ECU after processing a valid
              request message from the tester.

          T4: ECU INTER-FRAME RESPONSE DELAY         0         20        0           0
              FOR SUBSEQUENT DATA
              TRANSMISSION(S);
              Defines the response delay from
              the ECU after transmitting the
              first data frame in a multiple
              frame transmission.

          T5: TESTER INTER-FRAME REQUEST DELAY       0        100        0           0
              FOR SUBSEQUENT DATA
              TRANSMISSION(S);
              Defins the request delay from the
              tester after transmitting the
              first data frame in a multiple
              frame transmission.
**************************************************************************/
void handle_sci_data(void)
{
    if (sci_enabled)
    {
        uint32_t timeout_start = 0;
        bool timeout_reached = false;
        
        if (pcm_enabled)
        {
            // Collect bytes from the receive buffer
            uint8_t datalength = pcm_rx_available(); // get current number of bytes available to read
            if (datalength > 0)
            {
                for (uint8_t i = 0; i < datalength; i++)
                {
                    pcm_bytes_buffer[pcm_bytes_buffer_ptr] = pcm_getc() & 0xFF;
                    pcm_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
                    if (pcm_bytes_buffer_ptr > PCM_RX2_BUFFER_SIZE) // don't let buffer pointer overflow
                    {
                        pcm_bytes_buffer_ptr = PCM_RX2_BUFFER_SIZE;
                        break; // exit for-loop if buffer is about to overflow
                    }
                }
                pcm_last_msgbyte_received = millis(); // save time
            }

            // Handle completed messages
            if (!pcm_high_speed_enabled) // handle low-speed mode first
            {
                if (!pcm_actuator_test_running && !pcm_ls_request_running) // send message back to laptop normally
                {
                    if ((((millis() - pcm_last_msgbyte_received) > SCI_LS_T3_DELAY) && (pcm_bytes_buffer_ptr > 0)) || (pcm_bytes_buffer_ptr == PCM_RX2_BUFFER_SIZE))
                    { 
                        uint8_t usb_msg[4+pcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                        update_timestamp(current_timestamp); // get current time for the timestamp
                        
                        for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                        {
                            usb_msg[i] = current_timestamp[i];
                        }
                        for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                        {
                            usb_msg[4+i] = pcm_bytes_buffer[i];
                        }

                        send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 4+pcm_bytes_buffer_ptr);
                        
                        if (pcm_bytes_buffer[0] == 0x12) // pay attention to special bytes (speed change)
                        {
                            sbi(current_sci_bus_settings[0], 7); // set change settings bit
                            sbi(current_sci_bus_settings[0], 6); // set enable bit
                            sbi(current_sci_bus_settings[0], 4); // set speed bit (62500 baud)
                            configure_sci_bus(current_sci_bus_settings[0]);
                        }

                        if (pcm_repeated_messages && !pcm_repeated_messages_iteration)
                        {
                            // TODO, for now assume a proper answer
                            pcm_repeated_messages_next = true;
                        }
                        else if (pcm_repeated_messages && pcm_repeated_messages_iteration)
                        {
                            // TODO
                        }
                        
                        //handle_lcd(from_pcm, usb_msg, 4, 4+pcm_bytes_buffer_ptr); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                        pcm_bytes_buffer_ptr = 0;
                        pcm_msg_rx_count++;
                    }
                }
                else // 0x13 and 0x2A has a weird delay between messages so they are handled here with a smaller delay
                {
                    if ((((millis() - pcm_last_msgbyte_received) > 10) && (pcm_bytes_buffer_ptr > 0)) || (pcm_bytes_buffer_ptr == PCM_RX2_BUFFER_SIZE))
                    { 
                        // Stop actuator test command is accepted
                        if ((pcm_bytes_buffer[0] == 0x13) && (pcm_bytes_buffer[1] == 0x00) && (pcm_bytes_buffer[2] == 0x00))
                        {
                            uint8_t usb_msg[4+pcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                            update_timestamp(current_timestamp); // get current time for the timestamp
                            pcm_actuator_test_running = false;
                            pcm_actuator_test_byte = 0;
                            
                            for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                            {
                                usb_msg[i] = current_timestamp[i];
                            }
                            for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                            {
                                usb_msg[4+i] = pcm_bytes_buffer[i];
                            }
                            
                            send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 4+pcm_bytes_buffer_ptr);
                        }
                        // Stop broadcasting request bytes command is accepted
                        else if ((pcm_bytes_buffer[0] == 0x2A) && (pcm_bytes_buffer[1] == 0x00)) // The 0x00 byte is not echoed back by the PCM so don't look for a third byte
                        {
                            uint8_t usb_msg[4+pcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                            update_timestamp(current_timestamp); // get current time for the timestamp
                            pcm_ls_request_running = false;
                            pcm_ls_request_byte = 0;
    
                            for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                            {
                                usb_msg[i] = current_timestamp[i];
                            }
                            for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                            {
                                usb_msg[4+i] = pcm_bytes_buffer[i];
                            }

                            send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 4+pcm_bytes_buffer_ptr);
                        }
                        else // only ID-bytes are received, the beginning of the messages must be supplemented so that the GUI understands what it is about
                        {
                            uint8_t usb_msg[7]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                            update_timestamp(current_timestamp); // get current time for the timestamp
                            
                            for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                            {
                                usb_msg[i] = current_timestamp[i];
                            }

                            if (pcm_actuator_test_running && !pcm_ls_request_running) // if the actuator test mode is active then put two bytes before the received byte
                            {
                                usb_msg[4] = 0x13;
                                usb_msg[5] = pcm_actuator_test_byte;
                            }
                            
                            if (pcm_ls_request_running && !pcm_actuator_test_running) // if the request mode is active then put two bytes before the received byte; this is kind of annying, why isn't one time response enough... there's nothing "running" like in the case of actuator tests
                            {
                                usb_msg[4] = 0x2A;
                                usb_msg[5] = pcm_ls_request_byte;
                            }

                            usb_msg[6] = pcm_bytes_buffer[0]; // BUG: this byte is different at first when the PCM begins to broadcast a single byte repeatedly, but it should be overwritten very soon after
                            send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 7); // the message length is fixed by the nature of the active commands
                        }
                        
                        //handle_lcd(from_pcm, usb_msg, 4, 4+pcm_bytes_buffer_ptr); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                        pcm_bytes_buffer_ptr = 0;
                        pcm_msg_rx_count++;
                    }
                }
            }
            else // handle high-speed mode, no need to wait for message completion here, it is already handled when the message was sent
            {
                if ((pcm_bytes_buffer_ptr > 0) || (pcm_bytes_buffer_ptr == PCM_RX2_BUFFER_SIZE))
                {
                    uint8_t usb_msg[4+pcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                    update_timestamp(current_timestamp); // get current time for the timestamp
    
                    // Send the request bytes first
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++) // put original request message after the timestamp
                    {
                        usb_msg[4+i] = pcm_msg_to_send[i];
                    }
                    send_usb_packet(from_pcm, to_usb, msg_rx, sci_hs_request_bytes, usb_msg, 4+pcm_bytes_buffer_ptr);
    
                    // Then send the response bytes
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                    {
                        usb_msg[4+i] = pcm_bytes_buffer[i];
                    }
                    send_usb_packet(from_pcm, to_usb, msg_rx, sci_hs_response_bytes, usb_msg, 4+pcm_bytes_buffer_ptr);
    
                    if (pcm_bytes_buffer[0] == 0xFE) // pay attention to special bytes (speed change)
                    {
                        sbi(current_sci_bus_settings[0], 7); // set change settings bit
                        sbi(current_sci_bus_settings[0], 6); // set enable bit
                        cbi(current_sci_bus_settings[0], 4); // clear speed bit (7812.5 baud)
                        configure_sci_bus(current_sci_bus_settings[0]);
                    }

                    if (pcm_repeated_messages && !pcm_repeated_messages_iteration)
                    {
                        // TODO, for now assume a proper answer
                        pcm_repeated_messages_next = true;
                    }
                    else if (pcm_repeated_messages && pcm_repeated_messages_iteration)
                    {
                        // TODO
                    }

                    //handle_lcd(from_pcm, usb_msg, 4, 4+pcm_bytes_buffer_ptr); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    pcm_bytes_buffer_ptr = 0;
                    pcm_msg_rx_count++;
                }
            }

            // Repeated messages are prepared here
            if (pcm_repeated_messages && (pcm_rx_available() == 0))
            {
                if (pcm_repeated_messages_next && !pcm_repeated_messages_iteration) // no iteration, same message over and over again
                {
                    for (uint16_t i = 0; i < pcm_repeated_msg_bytes_ptr; i++) // fill the pending buffer with the message to be sent
                    {
                        pcm_msg_to_send[i] = pcm_repeated_msg_bytes[i];
                    }
                    pcm_msg_to_send_ptr = pcm_repeated_msg_bytes_ptr;
                    pcm_msg_pending = true; // set flag
                    pcm_repeated_messages_next = false;
                }
                else if (pcm_repeated_messages_next && pcm_repeated_messages_iteration) // iteration, message is incremented for every repeat according to settings
                {
                    // TODO
                }
            }

            // Send message
            if (pcm_msg_pending && (pcm_rx_available() == 0))
            {
                if (!pcm_high_speed_enabled) // low speed mode, half-duplex mode approach
                {
                    for (uint8_t i = 0; i < pcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while ((pcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo (half-duplex mode)
                            if ((millis() - timeout_start) > SCI_LS_T1_DELAY) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            timeout_reached = false;
                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                            break;
                        }
                    }

                    // Peek into the receive buffer
                    if ((pcm_msg_to_send_ptr > 1) && (pcm_peek(0) == 0x13) && (pcm_peek(1) != 0x00))
                    {
                        // See if the PCM starts to broadcast the actuator test byte
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        while ((pcm_rx_available() <= pcm_msg_to_send_ptr) && !timeout_reached)
                        {
                            // wait here for echo (half-duplex mode)
                            if ((millis() - timeout_start) > 100) timeout_reached = true;
                        }
                        if (timeout_reached)
                        {
                            timeout_reached = false;
                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                        }
                        else
                        {
                            pcm_actuator_test_running = true;
                            pcm_actuator_test_byte = pcm_msg_to_send[1];
                            pcm_rx_flush(); // workaround for the first false received message
                        }
                    }

                    if ((pcm_msg_to_send_ptr > 1) && (pcm_peek(0) == 0x2A) && (pcm_peek(1) != 0x00))
                    {
                        // See if the PCM starts to broadcast the diagnostic request byte
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        while ((pcm_rx_available() <= pcm_msg_to_send_ptr) && !timeout_reached)
                        {
                            // wait here for echo (half-duplex mode)
                            if ((millis() - timeout_start) > 100) timeout_reached = true;
                        }
                        if (timeout_reached)
                        {
                            timeout_reached = false;
                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                        }
                        else
                        {
                            pcm_ls_request_running = true;
                            pcm_ls_request_byte = pcm_msg_to_send[1];
                            pcm_rx_flush(); // workaround for the first false received message
                        }
                    }
                }
                else // high speed mode, full-duplex mode approach
                {
                    uint8_t echo_retry_counter = 0;
                    
                    if (array_contains(sci_hi_speed_memarea, 16, pcm_msg_to_send[0])) // make sure that the memory table select byte is approved by the PCM before sending the full message
                    {
                        if ((pcm_msg_to_send_ptr > 1) && (pcm_msg_to_send[1] == 0xFF)) // return full RAM-table if the first address is an invalid 0xFF
                        {
                            // Prepare message buffer as if it was filled with data beforehand
                            for (uint8_t i = 0; i < 240; i++)
                            {
                                pcm_msg_to_send[1 + i] = i; // put the address byte after the memory table pointer
                            }
                            pcm_msg_to_send_ptr = 241;
                        }
                        
                        for (uint8_t i = 0; i < pcm_msg_to_send_ptr; i++) // repeat for the length of the message
                        {
                            pcm_again:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm_msg_to_send[i]); // put the next byte in the transmit buffer
                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { pcm_msg_to_send[0], pcm_msg_to_send[i] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (pcm_peek(0) != pcm_msg_to_send[0]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                pcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto pcm_again;
                                else
                                {
                                    uint8_t ret[1] = { pcm_msg_to_send[0] };
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, ret, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, err, 1); // send error packet back to the laptop
                    }
                }
                pcm_msg_to_send_ptr = 0; // reset pointer
                pcm_msg_pending = false; // re-arm, make it possible to send a message again
                pcm_msg_tx_count++;
            }
        }
        
        if (tcm_enabled)
        {
            // Collect bytes from the receive buffer
            uint8_t datalength = tcm_rx_available(); // get current number of bytes available to read
            if (datalength > 0)
            {
                for (uint8_t i = 0; i < datalength; i++)
                {
                    tcm_bytes_buffer[tcm_bytes_buffer_ptr] = tcm_getc() & 0xFF;
                    tcm_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
                    if (tcm_bytes_buffer_ptr > TCM_RX3_BUFFER_SIZE) // don't let buffer pointer overflow
                    {
                        tcm_bytes_buffer_ptr = TCM_RX3_BUFFER_SIZE;
                        break; // exit for-loop if buffer is about to overflow
                    }
                }
                tcm_last_msgbyte_received = millis(); // save time
            }

            // Handle completed messages
            if (!tcm_high_speed_enabled) // handle low-speed mode first
            {
                if ((((millis() - tcm_last_msgbyte_received) > SCI_LS_T3_DELAY) && (tcm_bytes_buffer_ptr > 0)) || (tcm_bytes_buffer_ptr == TCM_RX3_BUFFER_SIZE))
                { 
                    uint8_t usb_msg[4+tcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                    update_timestamp(current_timestamp); // get current time for the timestamp
                    
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < tcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                    {
                        usb_msg[4+i] = tcm_bytes_buffer[i];
                    }

                    send_usb_packet(from_tcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 4+tcm_bytes_buffer_ptr);
                    
                    if (tcm_bytes_buffer[0] == 0x12) // pay attention to special bytes (speed change)
                    {
                        sbi(current_sci_bus_settings[0], 3); // set change settings bit
                        sbi(current_sci_bus_settings[0], 2); // set enable bit
                        sbi(current_sci_bus_settings[0], 0); // set speed bit (62500 baud)
                        configure_sci_bus(current_sci_bus_settings[0]);
                    }

                    if (tcm_repeated_messages && !tcm_repeated_messages_iteration)
                    {
                        // TODO, for now assume a proper answer
                        tcm_repeated_messages_next = true;
                    }
                    else if (tcm_repeated_messages && tcm_repeated_messages_iteration)
                    {
                        // TODO
                    }
                    
                    //handle_lcd(from_tcm, usb_msg, 4, 4+tcm_bytes_buffer_ptr); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    tcm_bytes_buffer_ptr = 0;
                    tcm_msg_rx_count++;
                }
            }
            else // handle high-speed mode, no need to wait for message completion here, it is already handled when the message was sent
            {
                if ((tcm_bytes_buffer_ptr > 0) || (tcm_bytes_buffer_ptr == TCM_RX3_BUFFER_SIZE))
                {
                    uint8_t usb_msg[4+tcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                    update_timestamp(current_timestamp); // get current time for the timestamp
    
                    // Send the request bytes first
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < tcm_bytes_buffer_ptr; i++) // put original request message after the timestamp
                    {
                        usb_msg[4+i] = tcm_msg_to_send[i];
                    }
                    send_usb_packet(from_tcm, to_usb, msg_rx, sci_hs_request_bytes, usb_msg, 4+tcm_bytes_buffer_ptr);
    
                    // Then send the response bytes
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < tcm_bytes_buffer_ptr; i++) // put every byte in the SCI-bus message after the timestamp
                    {
                        usb_msg[4+i] = tcm_bytes_buffer[i];
                    }
                    send_usb_packet(from_tcm, to_usb, msg_rx, sci_hs_response_bytes, usb_msg, 4+tcm_bytes_buffer_ptr);
    
                    if (tcm_bytes_buffer[0] == 0xFE)
                    {
                        sbi(current_sci_bus_settings[0], 3); // set change settings bit
                        sbi(current_sci_bus_settings[0], 2); // set enable bit
                        cbi(current_sci_bus_settings[0], 0); // clear speed bit (7812.5 baud)
                        configure_sci_bus(current_sci_bus_settings[0]);
                    }

                    if (tcm_repeated_messages && !tcm_repeated_messages_iteration)
                    {
                        // TODO, for now assume a proper answer
                        tcm_repeated_messages_next = true;
                    }
                    else if (tcm_repeated_messages && tcm_repeated_messages_iteration)
                    {
                        // TODO
                    }

                    //handle_lcd(from_tcm, usb_msg, 4, 4+tcm_bytes_buffer_ptr); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    tcm_bytes_buffer_ptr = 0;
                    tcm_msg_rx_count++;
                }
            }

            // Repeated messages are prepared here
            if (tcm_repeated_messages && (tcm_rx_available() == 0))
            {
                if (tcm_repeated_messages_next && !tcm_repeated_messages_iteration) // no iteration, same message over and over again
                {
                    for (uint16_t i = 0; i < tcm_repeated_msg_bytes_ptr; i++) // fill the pending buffer with the message to be sent
                    {
                        tcm_msg_to_send[i] = tcm_repeated_msg_bytes[i];
                    }
                    tcm_msg_to_send_ptr = tcm_repeated_msg_bytes_ptr;
                    tcm_msg_pending = true; // set flag
                    tcm_repeated_messages_next = false;
                }
                else if (tcm_repeated_messages_next && tcm_repeated_messages_iteration) // iteration, message is incremented for every repeat according to settings
                {
                    // TODO
                }
            }

            // Send message
            if (tcm_msg_pending && (tcm_rx_available() == 0))
            {
                if (!tcm_high_speed_enabled) // low speed mode, half-duplex mode approach
                {
                    for (uint8_t i = 0; i < tcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while ((tcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo (half-duplex mode)
                            if ((millis() - timeout_start) > SCI_LS_T1_DELAY) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            timeout_reached = false;
                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                            break;
                        }
                    }
                }
                else // high speed mode, full-duplex mode approach
                {
                    uint8_t echo_retry_counter = 0;
                    
                    if (array_contains(sci_hi_speed_memarea, 16, tcm_msg_to_send[0])) // make sure that the memory table select byte is approved by the TCM before sending the full message
                    {
                        for (uint8_t i = 0; i < tcm_msg_to_send_ptr; i++) // repeat for the length of the message
                        {
                            tcm_again:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            tcm_putc(tcm_msg_to_send[i]); // put the next byte in the transmit buffer
                            while ((tcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { tcm_msg_to_send[0], tcm_msg_to_send[i] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (tcm_peek(0) != tcm_msg_to_send[0]) // make sure the first memory pointer byte is echoed back
                            {
                                tcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto tcm_again;
                                else
                                {
                                    uint8_t ret[1] = { tcm_msg_to_send[0] };
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, ret, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a memory table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, err, 1); // send error packet back to the laptop
                    }
                }
                tcm_msg_to_send_ptr = 0; // reset pointer
                tcm_msg_pending = false; // re-arm, make it possible to send a message again
                tcm_msg_tx_count++;
            }
        }
    }
    
} // end of handle_sci_data


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
    current_millis_blink = millis(); // check current time
    if (heartbeat_enabled)
    {
        if (current_millis_blink - previous_act_blink >= heartbeat_interval)
        {
            previous_act_blink = current_millis_blink; // save current time
            blink_led(ACT_LED);
        }
    }
    if (current_millis_blink - rx_led_ontime >= led_blink_duration)
    {
        digitalWrite(RX_LED, HIGH); // turn off RX LED
    }
    if (current_millis_blink - tx_led_ontime >= led_blink_duration)
    {
        digitalWrite(TX_LED, HIGH); // turn off TX LED
    }
    if (current_millis_blink - act_led_ontime >= led_blink_duration)
    {
        digitalWrite(ACT_LED, HIGH); // turn off ACT LED
    }
    
} // end of handle_leds


/*************************************************************************
Function: lcd_init()
Purpose:  initialize LCD
**************************************************************************/
void lcd_init(void)
{
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
    lcd.createChar(0, up_symbol); // custom character from "upsymbol" variable with id number 0
    lcd.createChar(1, down_symbol);
    lcd.createChar(2, left_symbol);
    lcd.createChar(3, right_symbol);
    lcd.createChar(4, enter_symbol);
    lcd.createChar(5, degree_symbol);
    
} // end of lcd_init


/*************************************************************************
Function: print_display_layout_1_metric()
Purpose:  printing default metric display layout to LCD (km/h, km...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_metric(void)
{
//    lcd.setCursor(0, 0);
//    lcd.print(F("S:  0km/h  E:   0rpm")); // 1st line
//    lcd.setCursor(0, 1);
//    lcd.print(F("M: 0kPa B: 0.0/ 0.0V")); // 2nd line
//    lcd.setCursor(0, 2);
//    lcd.print(F("G:-  T:  0.0/  0.0 C")); // 3rd line
//    lcd.setCursor(0, 3);
//    lcd.print(F("P: 0% O:     0.000km")); // 4th line
//    lcd.setCursor(18, 2);
//    lcd.write((uint8_t)5); // print degree symbol

    lcd.setCursor(0, 0);
    lcd.print(F("  0km/h    0rpm   0%")); // 1st line
    lcd.setCursor(0, 1);
    lcd.print(F(" 0.0kPa    0.0/ 0.0V")); // 2nd line 
    lcd.setCursor(0, 2);
    lcd.print(F("  0.0/  0.0 C       ")); // 3rd line
    lcd.setCursor(0, 3);
    lcd.print(F("     0.000km        ")); // 4th line
    lcd.setCursor(11, 2);
    lcd.write((uint8_t)5); // print degree symbol

} // end of print_display_layout_1_metric


/*************************************************************************
Function: print_display_layout_1_imperial()
Purpose:  printing default metric display layout to LCD (mph, mi...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_imperial(void)
{
    lcd.setCursor(0, 0);
    lcd.print(F("S:  0mph   E:   0rpm")); // 1st line
    lcd.setCursor(0, 1);
    lcd.print(F("M: 0psi B: 0.0/ 0.0V")); // 2nd line
    lcd.setCursor(0, 2);
    lcd.print(F("G:-  T:  0.0/  0.0 F")); // 3rd line
    lcd.setCursor(0, 3);
    lcd.print(F("P: 0% O:     0.000mi")); // 4th line
    lcd.setCursor(18, 2);
    lcd.write((uint8_t)5); // print degree symbol
  
} // end of print_display_layout_1_imperial

#endif // MAIN_H

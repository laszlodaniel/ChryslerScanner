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
#define FW_DATE 0x000000005D5CEA05

// RAM buffer sizes for different UART-channels
#define USB_RX0_BUFFER_SIZE 1024
#define CCD_RX1_BUFFER_SIZE 64
#define PCM_RX2_BUFFER_SIZE 64
#define TCM_RX3_BUFFER_SIZE 64

#define USB_TX0_BUFFER_SIZE 1024
#define CCD_TX1_BUFFER_SIZE 64
#define PCM_TX2_BUFFER_SIZE 64
#define TCM_TX3_BUFFER_SIZE 64

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

// Fixed bytes
//#define CCD_DIAG_REQ    0xB2  // Diagnostic request ID-byte for CCD-bus
//#define CCD_DIAG_RESP   0xF2  // Diagnostic response ID-byte for CCD-bus
//#define SCI_FAULT_CODES 0x10  // Command to request fault codes on SCI-bus
//#define SCI_HI_SPEED    0x12  // Command to switch SCI-bus to high speed mode (62500 baud)
//#define SCI_LO_SPEED    0xFE  // Command to switch SCI-bus to low speed mode (7812.5 baud)

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define LOBAUD  127  // prescaler for 7812.5 baud speed (CCD-SCI / default low-speed diagnostic mode)
#define HIBAUD  15   // prescaler for  62500 baud speed (SCI / high-speed parameter mode)
#define USBBAUD 3    // prescaler for 250000 baud speed (USB)

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

#define SCI_INTERFRAME_RESPONSE_DELAY   100  // milliseconds elapsed after last received byte to consider the SCI-bus idling
#define SCI_INTERMESSAGE_RESPONSE_DELAY 50   // ms
#define SCI_INTERMESSAGE_REQUEST_DELAY  50   // ms

#define STOP  0x00
#define START 0x01

// Packet related stuff
#define PACKET_SYNC_BYTE    0x3D // = symbol
#define ASCII_SYNC_BYTE     0x3E // > symbol
#define MAX_PAYLOAD_LENGTH  USB_RX0_BUFFER_SIZE - 6  // 1024-6 bytes
#define EMPTY_PAYLOAD       0xFE  // Random byte, could be anything

#define BUFFER_SIZE         32

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
// 0x03-0xFF reserved 

// SUB-DATA CODE byte
// Command 0x04 & 0x05 (request and response)
#define hwfw_info           0x00 // Hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
#define timestamp           0x01 // elapsed milliseconds since system start
#define battery_voltage     0x02 // as the name says
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
// 0x00-0xFF reserved

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
// 0x08-0xFB reserved
#define error_eep_not_found                     0xFA
#define error_eep_checksum_invalid_value        0xFB
#define error_eep_read                          0xFC
#define error_eep_write                         0xFD
#define error_internal_error                    0xFE
#define error_fatal                             0xFF

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
volatile bool ccd_idle = false;
volatile bool ccd_ctrl = false;
volatile uint32_t ccd_msg_count = 0;
volatile uint8_t ccd_bytes_count = 0;
bool ccd_msg_pending = false; // flag for custom ccd-bus message transmission
uint8_t ccd_msg_to_send[BUFFER_SIZE]; // custom ccd-bus message is copied here
uint8_t ccd_msg_to_send_ptr = 0; // custom ccd-bus message length

// SCI-bus
bool sci_enabled = true;
bool pcm_enabled = true;
bool tcm_enabled = true;
volatile bool pcm_idle = false;
volatile bool tcm_idle = false;
bool pcm_high_speed_enabled = false;
bool tcm_high_speed_enabled = false;
bool pcm_echo_accepted = false;
bool tcm_echo_accepted = false;

uint8_t pcm_bytes_buffer[BUFFER_SIZE]; // max. SCI-bus message length to the PCM limited to 32 bytes, should be enough
uint8_t pcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool pcm_msg_pending = false; // flag for custom sci-bus message
uint8_t pcm_msg_to_send[BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t pcm_msg_to_send_ptr = 0;  // custom sci-bus message length
uint32_t pcm_last_msgbyte_received = 0;
uint32_t pcm_msg_count = 0;

uint8_t tcm_bytes_buffer[BUFFER_SIZE]; // max. SCI-bus message length to the TCM limited to 32 bytes, should be enough
uint8_t tcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool tcm_msg_pending = false; // flag for custom sci-bus message
uint8_t tcm_msg_to_send[BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t tcm_msg_to_send_ptr = 0; // custom sci-bus message length
uint32_t tcm_last_msgbyte_received = 0;
uint32_t tcm_msg_count = 0;

const uint8_t sci_hi_speed_memarea_num = 15; // number of available memory areas to read from SCI-bus, each contains 248 bytes of data
uint8_t sci_hi_speed_memarea[sci_hi_speed_memarea_num] = {0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFF}; // 0xFE is not a valid area selector, it makes the SCI-bus to return to low speed mode!

// Packet related variables
// Timeout values for packets
uint8_t command_timeout = 100; // milliseconds
uint16_t command_purge_timeout = 200; // milliseconds, if a command isn't complete within this time then delete the usb receive buffer

// Store handshake in the PROGram MEMory (flash)
const uint8_t handshake_progmem[] PROGMEM = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 };
                             //               "C     H     R     Y     S     L     E     R     C     C     D     S     C     I     S     C     A     N     N     E     R"
// Battery voltage detector
const uint16_t adc_supply_voltage = 500; // supply voltage multiplied by 100: 5.00V -> 500
const uint16_t battery_rd1 = 270; // high resistor value in the divider (R19), multiplied by 10: 27 kOhm = 270
const uint16_t battery_rd2 = 50;  // low resistor value in the divider (R20), multiplied by 10: 5 kOhm = 50
uint16_t adc_max_value = 1023; // 1023 for 10-bit resolution
uint16_t battery_adc = 0;   // raw analog reading is stored here
uint16_t battery_volts = 0; // converted to battery voltage and multiplied by 100: 12.85V -> 1285
uint8_t battery_volts_array[2]; // battery_volts is separated to byte components here

bool connected_to_vehicle = false;
uint8_t handshake_array[21];

uint32_t rx_led_ontime = 0;
uint32_t tx_led_ontime = 0;
uint32_t act_led_ontime = 0;
uint32_t current_millis_blink = 0;
uint32_t previous_act_blink = 0;
uint16_t led_blink_duration = 50; // milliseconds
uint16_t heartbeat_interval = 5000; // milliseconds
bool heartbeat_enabled = true;

uint8_t current_timestamp[4]; // current time is stored here when "update_timestamp" is called
const char ascii_autoreply[] = "I GOT YOUR MESSAGE!\n";

bool eep_present = false;
uint8_t eep_status = 0; // extEEPROM connection status is stored here
uint8_t eep_result = 0; // extEEPROM 
uint8_t eep_checksum[1];
uint8_t eep_calculated_checksum = 0;
bool    eep_checksum_ok = false;

uint8_t hw_version[2];
uint8_t hw_date[8];
uint8_t assembly_date[8];

// LCD related variables
// Custom LCD-characters
// https://maxpromer.github.io/LCD-Character-Creator/
uint8_t up_symbol[8]     = { 0x00, 0x04, 0x0E, 0x15, 0x04, 0x04, 0x00, 0x00 }; // ↑
uint8_t down_symbol[8]   = { 0x00, 0x04, 0x04, 0x15, 0x0E, 0x04, 0x00, 0x00 }; // ↓
uint8_t left_symbol[8]   = { 0x00, 0x04, 0x08, 0x1F, 0x08, 0x04, 0x00, 0x00 }; // ←
uint8_t right_symbol[8]  = { 0x00, 0x04, 0x02, 0x1F, 0x02, 0x04, 0x00, 0x00 }; // →
uint8_t enter_symbol[8]  = { 0x00, 0x01, 0x05, 0x09, 0x1F, 0x08, 0x04, 0x00 }; // 
uint8_t degree_symbol[8] = { 0x06, 0x09, 0x09, 0x06, 0x00, 0x00, 0x00, 0x00 }; // °


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
void usb_init(uint8_t ubrr)
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
    UBRR0L = ubrr; // don't mess with the high register, low is all you need

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
Returns:  low byte:  next byte in receive buffer
          high byte: error flags
**************************************************************************/
uint16_t usb_peek(void)
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
  
    tmptail = (USB_RxTail + 1) & USB_RX0_BUFFER_MASK;

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
void ccd_init(uint8_t ubrr)
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
    UBRR1L = ubrr; // don't mess with the high register, you already know why lol

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
void pcm_init(uint8_t ubrr)
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
    UBRR2L = ubrr; // don't mess with the high register, you already know why lol

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
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t pcm_peek(void)
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
  
    tmptail = (PCM_RxTail + 1) & PCM_RX2_BUFFER_MASK;

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
void tcm_init(uint8_t ubrr)
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
    UBRR3L = ubrr; // don't mess with the high register, you already know why lol

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
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t tcm_peek(void)
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
  
    tmptail = (TCM_RxTail + 1) & TCM_RX3_BUFFER_MASK;

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
    ccd_idle = true; /* set flag */
    ccd_bytes_count = ccd_rx_available();
    
} // end of ccd_eom


/*************************************************************************
Function: ccd_ctrl()
Purpose:  called when the CCD-chip's CTRL pin is going low
**************************************************************************/
void ccd_active_byte(void) { ccd_ctrl = true; /* set flag */ } // end of ccd_ctrl


/*************************************************************************
Function: calculate_checksum()
Purpose:  calculate checksum in a given buffer with specified length
Note:     index = starting index in buffer
          len = buffer full length
**************************************************************************/
uint8_t calculate_checksum(uint8_t *buff, uint16_t startindex, uint16_t len)
{
    uint8_t a = 0;
    for (uint16_t i = startindex ; i < len; i++)
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
      0 ___|      |______|      |______|      |______|      |______| 
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
Function: configure_sci_bus()
Purpose:  as the name says
**************************************************************************/
void configure_sci_bus(uint8_t data)
{
    // Lower half of the input byte (4-bits) encode configuration as follows (lowest to highest bit)
    bool pcm_tcm    = data & 0x01; // PCM (0) or TCM (1)
    bool enable     = data & 0x02; // disable (0) or enable (1)
    bool bus_config = data & 0x04; // bus configuration: A (0) or B (1)
    bool bus_speed  = data & 0x08; // speed: low speed/7812.5 baud (0) or high speed/62500 baud (1)
    
    if (enable)
    {
        if (!bus_config) // "A" configuration, routes to PCM and TCM cannot be active simultaneously 
        {
            if (!pcm_tcm) // PCM
            {
                pcm_enabled = true;
                tcm_enabled = false;
                
                // PA0..PA3 controls "A" configuration, PA4..PA7 controls "B" configuration
                digitalWrite(PA0, HIGH); // SCI-BUS_A_PCM_RX enabled
                digitalWrite(PA1, HIGH); // SCI-BUS_A_PCM_TX enabled
                digitalWrite(PA2, LOW);  // SCI-BUS_A_TCM_RX disabled
                digitalWrite(PA3, LOW);  // SCI-BUS_A_TCM_TX disabled
                digitalWrite(PA4, LOW);  // SCI-BUS_B_PCM_RX disabled
                digitalWrite(PA5, LOW);  // SCI-BUS_B_PCM_TX disabled
                digitalWrite(PA6, LOW);  // SCI-BUS_B_TCM_RX disabled
                digitalWrite(PA7, LOW);  // SCI-BUS_B_TCM_TX disabled
            }
            else // TCM
            {
                pcm_enabled = false;
                tcm_enabled = true;
                
                digitalWrite(PA0, LOW);
                digitalWrite(PA1, LOW);
                digitalWrite(PA2, HIGH);
                digitalWrite(PA3, HIGH);
                digitalWrite(PA4, LOW);
                digitalWrite(PA5, LOW);
                digitalWrite(PA6, LOW);
                digitalWrite(PA7, LOW);
            }
        }
        else // "B" configuration, routes to PCM and TCM can be active simultaneously 
        {
            if (!pcm_tcm) pcm_enabled = true;
            else tcm_enabled = true;
            
            digitalWrite(PA0, LOW);
            digitalWrite(PA1, LOW);
            digitalWrite(PA2, LOW);
            digitalWrite(PA3, LOW);
            digitalWrite(PA4, HIGH);
            digitalWrite(PA5, HIGH);
            digitalWrite(PA6, HIGH);
            digitalWrite(PA7, HIGH);
        }
    
        if (pcm_enabled || tcm_enabled) sci_enabled = true;
        if (!pcm_enabled && !tcm_enabled) sci_enabled = false;
    
        if (!pcm_tcm) // PCM
        {
            if (!bus_speed) // low speed
            {
                pcm_init(LOBAUD); // 7812.5 baud
                pcm_high_speed_enabled = false;
            }
            else // high speed
            {
                pcm_init(HIBAUD); // 62500 baud
                pcm_high_speed_enabled = true;
            }
        }
        else // TCM
        {
            if (!bus_speed) // low speed
            {
                tcm_init(LOBAUD); // 7812.5 baud
                tcm_high_speed_enabled = false;
            }
            else // high speed
            {
                tcm_init(HIBAUD); // 62500 baud
                tcm_high_speed_enabled = true;
            }
        }
    }
    else // disable
    {
        if (!bus_config) // "A"
        {
            digitalWrite(PA0, LOW);
            digitalWrite(PA1, LOW);
            digitalWrite(PA2, LOW);
            digitalWrite(PA3, LOW);
            digitalWrite(PA4, LOW);
            digitalWrite(PA5, LOW);
            digitalWrite(PA6, LOW);
            digitalWrite(PA7, LOW);
    
            pcm_enabled = false;
            tcm_enabled = false;
            sci_enabled = false;
        }
        else // "B"
        {
            if (!pcm_tcm) // PCM
            {
                digitalWrite(PA0, LOW);
                digitalWrite(PA1, LOW);
                digitalWrite(PA2, LOW);
                digitalWrite(PA3, LOW);
                digitalWrite(PA4, LOW);
                digitalWrite(PA5, LOW);
                //digitalWrite(PA6, LOW);
                //digitalWrite(PA7, LOW);

                pcm_enabled = false;
            }
            else // TCM
            {
                digitalWrite(PA0, LOW);
                digitalWrite(PA1, LOW);
                digitalWrite(PA2, LOW);
                digitalWrite(PA3, LOW);
                //digitalWrite(PA4, LOW);
                //digitalWrite(PA5, LOW);
                digitalWrite(PA6, LOW);
                digitalWrite(PA7, LOW);

                tcm_enabled = false;
            }
        }
    }

} // end of configure_sci_bus


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
Function: blink_led()
Purpose:  turn on one of the indicator LEDs
Note:     this is only turning the chosen LED on, other function turns it off when it needs to
**************************************************************************/
void blink_led(uint8_t led)
{
    digitalWrite(led, LOW); // turn on LED
    switch (led)
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
    
} // end of exteeprom_init


/*************************************************************************
Function: send_hwfw_info()
Purpose:  gather hardware version/date, assembly date and firmware date
          into an array and send through serial link
**************************************************************************/
void send_hwfw_info(void)
{
    uint8_t hwfw_value[26];
                                    
    hwfw_value[0] = hw_version[0];
    hwfw_value[1] = hw_version[1];
    
    hwfw_value[2] = hw_date[0];
    hwfw_value[3] = hw_date[1];
    hwfw_value[4] = hw_date[2];
    hwfw_value[5] = hw_date[3];
    hwfw_value[6] = hw_date[4];
    hwfw_value[7] = hw_date[5];
    hwfw_value[8] = hw_date[6];
    hwfw_value[9] = hw_date[7];

    hwfw_value[10] = assembly_date[0];
    hwfw_value[11] = assembly_date[1];
    hwfw_value[12] = assembly_date[2];
    hwfw_value[13] = assembly_date[3];
    hwfw_value[14] = assembly_date[4];
    hwfw_value[15] = assembly_date[5];
    hwfw_value[16] = assembly_date[6];
    hwfw_value[17] = assembly_date[7];
    
    hwfw_value[18] = (FW_DATE >> 56) & 0xFF;
    hwfw_value[19] = (FW_DATE >> 48) & 0xFF;
    hwfw_value[20] = (FW_DATE >> 40) & 0xFF;
    hwfw_value[21] = (FW_DATE >> 32) & 0xFF;
    hwfw_value[22] = (FW_DATE >> 24) & 0xFF;
    hwfw_value[23] = (FW_DATE >> 16) & 0xFF;
    hwfw_value[24] = (FW_DATE >> 8) & 0xFF;
    hwfw_value[25] = FW_DATE & 0xFF;

    send_usb_packet(from_usb, to_usb, response, hwfw_info, hwfw_value, 26);
    
} /* send_hwfw_info */


/*************************************************************************
Function: evaluate_eep_checksum()
Purpose:  compare external eeprom checksum value to the calculated one
          and send results to laptop
Note:     compared values are read during setup() automatically
**************************************************************************/
void evaluate_eep_checksum(void)
{
    if (eep_calculated_checksum == eep_checksum[0])
    {
        eep_checksum_ok = true;
        uint8_t eep_checksum_response[2];
        eep_checksum_response[0] = 0x00; // OK
        eep_checksum_response[1] = eep_checksum[0]; // checksum byte
        send_usb_packet(from_usb, to_usb, response, exteeprom_checksum, eep_checksum_response, 2);
    }
    else
    {
        eep_checksum_ok = false;
        uint8_t eep_checksum_response[2];
        eep_checksum_response[0] = eep_checksum[0]; // wrong checksum byte
        eep_checksum_response[1] = eep_calculated_checksum; // correct checksum byte
        send_usb_packet(from_usb, to_usb, ok_error, error_eep_checksum_invalid_value, eep_checksum_response, 2);
    }
    
} /* evaluate_eep_checksum */


/*************************************************************************
Function: check_battery_volts()
Purpose:  measure battery voltage through the OBD16 pin
Note:     be aware that this voltage isn't precise like a multimeter reading, 
          it is produced by a resistor divider circuit with imperfect
          resistors (1% tolreance, but still lots of headroom in it);
          the circuit tolerates +24V batteries too
**************************************************************************/
void check_battery_volts(void)
{
    battery_adc = analogRead(BATT);
    battery_volts = (uint16_t)(battery_adc*(adc_supply_voltage/100.0)/adc_max_value*((battery_rd1/10.0)+(battery_rd2/10.0))/(battery_rd2/10.0)*100.0); // resistor divider equation
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
Function: handle_usb_data()
Purpose:  handle USB commands coming from an external computer
Note:     refer to ChryslerCCDSCIScanner_UART_Protocol.pdf to find out 
          more about the message format:
          [ SYNC | LENGTH_HB | LENGTH_LB | DATACODE | SUBDATACODE | <?PAYLOAD?> | CHECKSUM ]
**************************************************************************/
void handle_usb_data(void)
{
    // Proceed only if the receive buffer contains at least 3 bytes (ideally 1 sync byte + 2 length bytes).
    if (usb_rx_available() > 2)
    {
        // Make some local variables, they will disappear after the function ends
        uint8_t sync, length_hb, length_lb, datacode, subdatacode, checksum;
        bool payload_bytes = false;
        uint16_t bytes_to_read = 0;
        uint16_t payload_length = 0;
        uint8_t calculated_checksum = 0;

        uint8_t ack[1] = { 0x00 }; // acknowledge payload array
        uint8_t err[1] = { 0xFF }; // error payload array
        uint8_t ret[1];

        uint32_t command_timeout_start = 0;
        bool command_timeout_reached = false;

        blink_led(RX_LED);

        // Find the first SYNC byte (0x3D or 0x3E)
        command_timeout_start = millis(); // save current time
        while ((usb_rx_available() > 0) && !command_timeout_reached)
        {
            if ((usb_peek() & 0xFF) == PACKET_SYNC_BYTE) break; // the 0xFF mask gets rid of the high byte containing flags
            if ((usb_peek() & 0xFF) == ASCII_SYNC_BYTE) break;
            
            usb_getc(); // if it's not the SYNC byte then get rid of it (read into oblivion)

            // Determine if timeout has been reached
            if (millis() - command_timeout_start > command_purge_timeout) command_timeout_reached = true;
        }
        if (command_timeout_reached)
        {
            send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occured, err, 1);
            return; // exit, let the loop call this function again
        }

        if (usb_rx_available() == 0) return; // exit if there's no data left, don't send error packet back to the laptop
        
        sync = usb_getc() & 0xFF; // read one sync byte
        
        if (sync == PACKET_SYNC_BYTE) // byte based communication
        {
            length_hb = usb_getc() & 0xFF; // read first length byte
            length_lb = usb_getc() & 0xFF; // read second length byte
    
            // Calculate how much more bytes should we read by combining the two length bytes into a word.
            bytes_to_read = (length_hb << 8) + length_lb + 1; // +1 CHECKSUM byte
    
            // Maximum packet length is 1024 bytes.
            // Can't accept larger packets so if that's the case the function needs to exit after sending an error packet back to the laptop.
            // Also can't accept packet with less than 2 bytes length (datacode and subdatacode is always needed).
            if (((bytes_to_read - 3) > MAX_PAYLOAD_LENGTH) || ((bytes_to_read - 1) < 2))
            {
                send_usb_packet(from_usb, to_usb, ok_error, error_length_invalid_value, err, 1);
                return; // exit, let the loop call this function again
            }
    
            // Calculate the exact size of the payload.
            payload_length = bytes_to_read - 3; // in this case we have to be careful not to count data code byte, sub-data code byte and checksum byte
    
            // Do not let this variable sink below zero.
            if (payload_length < 0) payload_length = 0; // !!!
    
            // Wait here until all of the expected bytes are received or timeout occurs.
            // Data reception is controlled by ISRs (Interrupt Service Routine) so it's okay to block the code here for a while.
            command_timeout_start = millis();
            while ((usb_rx_available() < bytes_to_read) && !command_timeout_reached) 
            {
                if (millis() - command_timeout_start > command_timeout) command_timeout_reached = true;
            }
    
            // Check if timeout has been reached, if not then continue processing stuff!
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
        
            // Source isn't really checked here, the packet must come from an external computer...
            switch (target) // evaluate target value
            {
                case to_usb: // 0x00 - scanner is the target
                {
                    switch (command) // evaluate command
                    {
                        case reset: // 0x00 - reset scanner request
                        {
                            // Send acknowledge packet back to the laptop.
                            send_usb_packet(from_usb, to_usb, reset, ok, ack, 1); // RX LED is on by now and this function lights up TX LED
                            digitalWrite(ACT_LED, LOW); // blink all LEDs including this, good way to test if LEDs are working or not
                            while(true); // Enter into an infinite loop. Watchdog timer doesn't get reset this way so it restarts the program eventually.
                            break; // not necessary but every case needs a break
                        }
                        case handshake: // 0x01 - handshake request coming from an external computer
                        {
                            send_usb_packet(from_usb, to_usb, handshake, ok, handshake_array, 21);
                            if (subdatacode == 0x01)
                            {
                                evaluate_eep_checksum();
                                send_hwfw_info();
                            }
                            break;
                        }
                        case status: // 0x02 - status report request
                        {
                            // TODO: gather status data and send it back to the laptop
                            send_usb_packet(from_usb, to_usb, status, ok, ack, 1); // the payload should contain all information but now it's just an ACK byte (0x00)
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

                                    send_usb_packet(from_usb, to_usb, settings, set_sci_bus, cmd_payload, 1); // acknowledge
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
                                    // TODO
                                    ret[0] = to_ccd; // improvised payload array with only 1 element which is the target bus
                                    send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                    break;
                                }
                                case single_msg: // 0x01 - send message to the CCD-bus, message is stored in payload 
                                {
                                    if ((payload_length > 0) && (payload_length <= BUFFER_SIZE)) 
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
                                        //ret[0] = to_ccd;
                                        //send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 1); // acknowledge
                                    }
                                    else
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                    }
                                    break;
                                }
                                case repeated_msg: // 0x02 - send message(s) to the CCD-bus, number of messages, repeat interval(s) are stored in payload
                                {
                                    // TODO
                                    ret[0] = to_ccd;
                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
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
                                    // TODO
                                    ret[0] = to_pcm;
                                    send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                    break;
                                }
                                case single_msg: // 0x01 - send message to the SCI-bus, message is stored in payload 
                                {
                                    if ((payload_length > 0) && (payload_length <= BUFFER_SIZE))
                                    {
                                        for (uint8_t i = 0; i < payload_length; i++) // fill the pending buffer with the message to be sent
                                        {
                                            pcm_msg_to_send[i] = cmd_payload[i];
                                        }
                                        // Checksum isn't used on SCI-bus transmissions, except when receiving fault codes.
                                        pcm_msg_to_send_ptr = payload_length;
                                        pcm_msg_pending = true; // set flag so the main loop knows there's something to do
                                        //ret[0] = to_pcm;
                                        //send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 1); // acknowledge
                                    }
                                    else
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                    }
                                    break;
                                }
                                case repeated_msg: // 0x02 - send message(s) to the SCI-bus, number of messages, repeat interval(s) are stored in payload
                                {
                                    // TODO
                                    ret[0] = to_pcm;
                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
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
                                    // TODO
                                    ret[0] = to_tcm;
                                    send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 1);
                                    break;
                                }
                                case single_msg: // 0x01 - send message to the SCI-bus, message is stored in payload 
                                {
                                    if ((payload_length > 0) && (payload_length <= BUFFER_SIZE))
                                    {
                                        for (uint8_t i = 0; i < payload_length; i++) // fill the pending buffer with the message to be sent
                                        {
                                            tcm_msg_to_send[i] = cmd_payload[i];
                                        }
                                        // Checksum isn't used on SCI-bus transmissions, except when receiving fault codes.
                                        tcm_msg_to_send_ptr = payload_length;
                                        tcm_msg_pending = true; // set flag so the main loop knows there's something to do
                                        //ret[0] = to_tcm;
                                        //send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 1); // acknowledge
                                    }
                                    else
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                    }
                                    break;
                                }
                                case repeated_msg: // 0x02 - send message(s) to the SCI-bus, number of messages, repeat interval(s) are stored in payload
                                {
                                    // TODO
                                    ret[0] = to_tcm;
                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_msg, ret, 1); // acknowledge
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
        }
        else if (sync == ASCII_SYNC_BYTE) // text based communication
        {
            usb_puts(ascii_autoreply); // TODO
        }
    }
    else
    {
      // TODO: check here if the 1 or 2 bytes received stayed long enough to consider getting rid of them
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
            if (ccd_bytes_count > 0) // the exact message length is recorded in the CCD-bus idle ISR so it's pretty accurate
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
                ccd_bytes_count = 0; // force ISR to update this value again so we don't end up here in the next program loop
            }

            if (ccd_msg_pending) // received over usb connection, checksum corrected there (if wrong)
            {     
                for (uint8_t i = 0; i < ccd_msg_to_send_ptr; i++) // since the bus is already idling start filling the transmit buffer with bytes right away
                {
                    ccd_putc(ccd_msg_to_send[i]); // transmission occurs automatically if the code senses at least 1 byte in this buffer
                }
                ccd_msg_to_send_ptr = 0; // reset pointer, TODO: perhaps don't reset it until proper echo is heard on the bus
                ccd_msg_pending = false; // re-arm, make it possible to send a message again, TODO: same as above
            }
            ccd_idle = false; // re-arm so the next program loop doesn't enter here again unless the ISR changes this variable to true
        }
    }
    else // monitor ccd-bus receive buffer for garbage bytes when clock signal is suspended and purge if necessary
    {
        // TODO: check checksum byte and only send message back when it's correct
        uint8_t data_length = ccd_rx_available(); // probably half completed messages end up here when the ccd-chip's clock signal is stopped during active bytes on the bus
        if (data_length > 0)
        {
            uint8_t usb_msg[4+data_length]; // create local array which will hold the timestamp and the CCD-bus message
            update_timestamp(current_timestamp); // get current time for the timestamp
            for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
            {
                usb_msg[i] = current_timestamp[i];
            }
            for (uint8_t i = 0; i < ccd_bytes_count; i++) // put every byte in the CCD-bus message after the timestamp
            {
                usb_msg[4+i] = ccd_getc() & 0xFF; // new message bytes may arrive in the circular buffer but this way only one message is removed
            }
            send_usb_packet(from_ccd, to_usb, msg_rx, single_msg, usb_msg, 4+ccd_bytes_count); // send CCD-bus message back to the laptop
        }
    }

} // end of handle_ccd_data


/*************************************************************************
Function: handle_sci_data()
Purpose:  handle SCI-bus messages from both PCM and TCM
Note:     be aware that in "A" configuration PCM and TCM TX-pins are joined
          so they can't talk at the same time. I mean they can but
          Arduino won't understand a single bit because of framing errors.
TODO:     use a separate timer (and its interrupts) to measure idle condition
          just like the CCD-chip does with the idle pin
**************************************************************************/
void handle_sci_data(void)
{
    if (sci_enabled)
    {
        if (pcm_enabled)
        {
            uint32_t timeout_start = 0;
            bool timeout_reached = false;
            
            // Collect bytes from the receive buffer
            uint8_t datalength = pcm_rx_available(); // how many bytes are readily available? (only read this much bytes, don't let them accumulate in the for-loop)
            if (datalength > 0)
            {
                for (uint8_t i = 0; i < datalength; i++)
                {
                    pcm_bytes_buffer[pcm_bytes_buffer_ptr] = pcm_getc() & 0xFF;
                    pcm_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
                    if (pcm_bytes_buffer_ptr >= BUFFER_SIZE) // don't let buffer pointer overflow
                    {
                        break; // exit for-loop if buffer is about to overflow
                    }
                }
                pcm_last_msgbyte_received = millis();
            }

            // Decide if a message is complete using a timeout (delay) condition or send the whole buffer if it can't hold more bytes
            if ((((millis() - pcm_last_msgbyte_received) > SCI_INTERFRAME_RESPONSE_DELAY) && (pcm_bytes_buffer_ptr > 0)) || pcm_bytes_buffer_ptr == BUFFER_SIZE)
            {
                uint8_t usb_msg[4+pcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp, ironically the timestamp will indicate when the last byte was received but it's good enough
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < pcm_bytes_buffer_ptr; i++)
                {
                    usb_msg[4+i] = pcm_bytes_buffer[i]; // put every byte in the SCI-bus message after the timestamp
                }
                send_usb_packet(from_pcm, to_usb, msg_rx, ok, usb_msg, 4+pcm_bytes_buffer_ptr);

                // Take action if special bytes are received
                if (!pcm_high_speed_enabled) // low speed mode
                {
                    //if (pcm_bytes_buffer[0] == 0x12) configure_sci_bus(); // TODO: enter high speed mode automatically
                }
                else
                {
                    //if (pcm_bytes_buffer[0] == 0xFE) configure_sci_bus(); // TODO: enter low speed mode automatically
                }
                
                pcm_bytes_buffer_ptr = 0;
                pcm_msg_count++;
            }

            // This section sends a message to the PCM and checks if the echoed bytes are correct by peeking into the receive buffer
            if (pcm_msg_pending && (pcm_rx_available() == 0))
            {
                if (!pcm_high_speed_enabled) // low speed mode
                {
                    for (uint8_t i = 0; i < pcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while((pcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo
                            if ((millis() - timeout_start) > 200) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            break;
                        }
                    }
                    pcm_msg_to_send_ptr = 0; // reset pointer
                    pcm_msg_pending = false; // re-arm, make it possible to send a message again
                }
                else // high speed mode
                {
                    for (uint8_t i = 0; i < pcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while((pcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for response
                            if ((millis() - timeout_start) > 200) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            break;
                        }
                    }
                    pcm_msg_to_send_ptr = 0; // reset pointer
                    pcm_msg_pending = false; // re-arm, make it possible to send a message again
                }
            }
        }
    
        if (tcm_enabled)
        {
            uint32_t timeout_start = 0;
            bool timeout_reached = false;
            
            uint8_t datalength = tcm_rx_available(); // how many bytes are readily available? (only read this much bytes, don't let them accumulate in the for-loop)
            if (datalength > 0)
            {
                for (uint8_t i = 0; i < datalength; i++)
                {
                    tcm_bytes_buffer[tcm_bytes_buffer_ptr] = tcm_getc() & 0xFF;
                    tcm_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
                    if (tcm_bytes_buffer_ptr >= BUFFER_SIZE) // don't let buffer pointer overflow
                    {
                        break;
                    }
                }
                tcm_last_msgbyte_received = millis();
            }
    
            if ((((millis() - tcm_last_msgbyte_received) > SCI_INTERFRAME_RESPONSE_DELAY) && (tcm_bytes_buffer_ptr > 0)) || tcm_bytes_buffer_ptr == BUFFER_SIZE)
            {
                uint8_t usb_msg[4+tcm_bytes_buffer_ptr]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < tcm_bytes_buffer_ptr; i++)
                {
                    usb_msg[4+i] = tcm_bytes_buffer[i]; // put every byte in the SCI-bus message after the timestamp
                }
                send_usb_packet(from_tcm, to_usb, msg_rx, ok, usb_msg, 4+tcm_bytes_buffer_ptr);

                // Take action if special bytes are received
                if (!tcm_high_speed_enabled) // low speed mode
                {
                    //if (tcm_bytes_buffer[0] == 0x12) configure_sci_bus(); // TODO: enter high speed mode automatically
                }
                else
                {
                    //if (tcm_bytes_buffer[0] == 0xFE) configure_sci_bus(); // TODO: enter low speed mode automatically
                }
                
                tcm_bytes_buffer_ptr = 0;
                tcm_msg_count++;
            }

            if (tcm_msg_pending && (tcm_rx_available() == 0))
            {
                if (!tcm_high_speed_enabled) // low speed mode
                {
                    for (uint8_t i = 0; i < tcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while((tcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for echo
                            if ((millis() - timeout_start) > 200) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            break;
                        }
                    }
                    tcm_msg_to_send_ptr = 0; // reset pointer
                    tcm_msg_pending = false; // re-arm, make it possible to send a message again
                }
                else // high speed mode
                {
                    for (uint8_t i = 0; i < tcm_msg_to_send_ptr; i++) // repeat for the length of the message
                    {
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm_msg_to_send[i]); // put the next byte in the transmit buffer
                        while((tcm_rx_available() <= i) && !timeout_reached)
                        {
                            // wait here for response
                            if ((millis() - timeout_start) > 200) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            break;
                        }
                    }
                    tcm_msg_to_send_ptr = 0; // reset pointer
                    tcm_msg_pending = false; // re-arm, make it possible to send a message again
                }
            }
        }
    }
    
} // end of handle_sci_data


/*************************************************************************
Function: handle_leds()
Purpose:  turn off indicator LEDs when blink duration expires
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
Function: get_bus_config()
Purpose:  figure out how to talk to the vehicle
Note:     1. Check if CCD-bus is present by sniffing bytes being broadcasted on the bus.
          2. If no traffic for a short time then try to wake up the bus by sending a 0xFF byte.
          3. If no answer within short time then conclude that CCD-bus is bad or unavailable.
          4. Check if SCI-bus is present by sending a 0x10 byte to request engine fault codes (PCM).
          5. Do the same for A and B configurations and note which configuration is active.
          5. If this byte gets echod back then wait for a little more for reply (TX:0x10 RX:0x10 {-wait-} 0xFE 0x0E).
          6. If the bytes (echo too) add up to the last byte (checksum OK) then considere this a valid reply from the PCM.
          7. Repeat from 4. for TCM (request transmission fault codes).
          8. Gather and save results in a byte array and send back to the laptop.
**************************************************************************/
void get_bus_config(void)
{
    // TODO
    
} // end of get_bus_config


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
Function: handle_lcd()
Purpose:  write stuff to LCD
**************************************************************************/
void handle_lcd(void)
{
    // TODO
    
} // end of handle_lcd


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

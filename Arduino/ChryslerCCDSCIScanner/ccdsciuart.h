/*
 * ChryslerCCDSCIScanner (https://github.com/laszlodaniel/ChryslerCCDSCIScanner)
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
 * UART code is based on original library by Andy Gock:
 * https://github.com/andygock/avr-uart
 */

#ifndef CCDSCIUART_H
#define CCDSCIUART_H

#define FW 0x0001 // Firmware version: V0.001

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
#define CCD_DIAG_REQ    0xB2  // Diagnostic request ID-byte for CCD-bus
#define CCD_DIAG_RESP   0xF2  // Diagnostic response ID-byte for CCD-bus
#define SCI_FAULT_CODES 0x10  // Command to request fault codes on SCI-bus
#define SCI_HI_SPEED    0x12  // Command to switch SCI-bus to high speed mode (62500 baud)
#define SCI_LO_SPEED    0xFE  // Command to switch SCI-bus to low speed mode (7812.5 baud)

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define LOBAUD  127  // prescaler for   7812.5 baud speed (CCD-SCI / default low-speed diagnostic mode)
#define HIBAUD  15   // prescaler for  62500   baud speed (SCI / high-speed parameter mode)
//#define USBBAUD 8    // prescaler for 115200   baud speed (USB) / bad choice, 3.5% signal error 
#define USBBAUD 3    // prescaler for 250000   baud speed (USB) / good choice, 0.0% signal error (precisely zero)

//#define ASCII_OUTPUT  // Choose this if you want a simple text output over USB for serial monitors
#define PACKET_OUTPUT // Choose this if you want a byte-based packet output over USB for third-party applications

// Make sure they are not defined at the same time
#if defined(ASCII_OUTPUT) || defined(PACKET_OUTPUT)
#undef ASCII_OUTPUT // packet output ftw
#endif

// If accidentally no output is given 
#if !defined(ASCII_OUTPUT) && !defined(PACKET_OUTPUT)
#define PACKET_OUTPUT // packet output ftw
#endif

#define CCD_CLOCK_PIN 11 // clock generator output for CCD-bus
#define BATT          A0 // battery voltage sensor

#define SCI_INTERFRAME_RESPONSE_DELAY   100  // milliseconds elapsed after last received byte to consider the SCI-bus idling
#define SCI_INTERMESSAGE_RESPONSE_DELAY 50   // ms
#define SCI_INTERMESSAGE_REQUEST_DELAY  50   // ms

// PCM-TCM selector
#define NON 0x00 // neither computer is active
#define PCM 0x01 // pcm only
#define TCM 0x02 // tcm only
#define BOT 0x03 // both computer active

#define SCI_CONF_A 0x01 // SCI-bus configuration A
#define SCI_CONF_B 0x02 // SCI-bus configuration B

#define LOSPEED 0x01 // low speed flag (7812.5 baud)
#define HISPEED 0x02 // high speed flag (62500 baud)

#define PA0 22 // SCI-bus configuration selector digital pins on ATmega2560
#define PA1 23 // |
#define PA2 24 // |
#define PA3 25 // |
#define PA4 26 // |
#define PA5 27 // |
#define PA6 28 // |
#define PA7 29 // |

#define STOP  0x00
#define START 0x01

// Packet related stuff
#define PACKET_SYNC_BYTE    0x3D // = symbol
#define ASCII_SYNC_BYTE     0x3E // > symbol
#define MAX_PAYLOAD_LENGTH  USB_RX0_BUFFER_SIZE - 6  // 1024-6 bytes
#define EMPTY_PAYLOAD       0xFE  // Random byte, could be anything

#define TEMP_BUFFER_SIZE     32

// DATA CODE byte building blocks
// Source and Target masks (high nibble (4 bits))
#define from_usb          0x00
#define from_ccd          0x01
#define from_pcm          0x02
#define from_tcm          0x03
#define to_usb            0x00
#define to_ccd            0x01
#define to_pcm            0x02
#define to_tcm            0x03
// DC commands (low nibble (4 bits))
#define reboot            0x00
#define handshake         0x01
#define status            0x02
#define settings          0x03
#define request           0x04
#define response          0x05
#define send_msg          0x06
#define send_rep_msg      0x07
#define stop_msg_flow     0x08
#define receive_msg       0x09
#define self_diag         0x0A
#define make_backup       0x0B
#define restore_backup    0x0C
#define restore_default   0x0D
#define debug             0x0E
#define ok_error          0x0F

// SUB-DATA CODE byte
// DC command 0x03 (settings)
#define read_settings               0x01
#define write_settings              0x02
#define enable_ccd_bus              0x03
#define disable_ccd_bus             0x04
#define enable_sci_bus              0x05
#define disable_sci_bus             0x06
#define enable_sci_bus_high_speed   0x07
#define disable_sci_bus_high_speed  0x08
#define enable_act_led              0x09
#define disable_act_led             0x0A

// DC command 0x04-0x05 (request-response)
// Both commands share the same sub-data codes
#define firmware_version            0x01
#define read_int_eeprom             0x02
#define read_ext_eeprom             0x03
#define write_int_eeprom            0x04
#define write_ext_eeprom            0x05
#define scan_ccd_bus_modules        0x06
#define mcu_counter_value           0x07

// DC command 0x0E (debug)
// TODO

// DC command 0x0F (OK/ERROR)
#define ok                                      0x00
#define error_sync_invalid_value                0x01
#define error_length_invalid_value              0x02
#define error_datacode_source_target_conflict   0x03
#define error_datacode_invalid_dc_command       0x04
#define error_subdatacode_invalid_value         0x05
#define error_subdatacode_not_enough_info       0x06
#define error_payload_invalid_values            0x07
#define error_checksum_invalid_value            0x08
#define error_packet_timeout_occured            0x09
#define error_buffer_overflow                   0x0A
#define error_scanner_internal_error            0xFE
#define error_fatal                             0xFF

// Variables
volatile uint16_t USB_RxBuf[USB_RX0_BUFFER_SIZE];
volatile uint8_t  USB_TxBuf[USB_TX0_BUFFER_SIZE];
volatile uint16_t USB_RxHead;
volatile uint16_t USB_RxTail;
volatile uint16_t USB_TxHead;
volatile uint16_t USB_TxTail;
volatile uint8_t  USB_LastRxError;

volatile uint16_t CCD_RxBuf[CCD_RX1_BUFFER_SIZE];
volatile uint8_t  CCD_TxBuf[CCD_TX1_BUFFER_SIZE];
volatile uint8_t  CCD_RxHead;
volatile uint8_t  CCD_RxTail;
volatile uint8_t  CCD_TxHead;
volatile uint8_t  CCD_TxTail;
volatile uint8_t  CCD_LastRxError;

volatile uint16_t PCM_RxBuf[PCM_RX2_BUFFER_SIZE];
volatile uint8_t  PCM_TxBuf[PCM_TX2_BUFFER_SIZE];
volatile uint8_t  PCM_RxHead;
volatile uint8_t  PCM_RxTail;
volatile uint8_t  PCM_TxHead;
volatile uint8_t  PCM_TxTail;
volatile uint8_t  PCM_LastRxError;

volatile uint16_t TCM_RxBuf[TCM_RX3_BUFFER_SIZE];
volatile uint8_t  TCM_TxBuf[TCM_TX3_BUFFER_SIZE];
volatile uint8_t  TCM_RxHead;
volatile uint8_t  TCM_RxTail;
volatile uint8_t  TCM_TxHead;
volatile uint8_t  TCM_TxTail;
volatile uint8_t  TCM_LastRxError;

bool ext_eeprom_present = false;

// CCD-bus
volatile bool ccd_idle = false;
volatile bool ccd_ctrl = false;
volatile uint32_t total_ccd_msg_count = 0;
bool ccd_enabled = true;
uint8_t ccd_bytes_buffer[TEMP_BUFFER_SIZE]; // max. CCD-bus message length limited to 32 bytes, should be enough
uint8_t ccd_bytes_buffer_ptr = 0; // pointer in the previous array
bool ccd_msg_pending = false; // flag for custom ccd-bus message transmission
uint8_t ccd_msg_to_send[TEMP_BUFFER_SIZE]; // custom ccd-bus message is copied here
uint8_t ccd_msg_to_send_ptr = 0; // custom ccd-bus message length
uint8_t ccd_module_addr[TEMP_BUFFER_SIZE]; // recognised CCD-bus modules are stored here with ascending order
uint8_t ccd_module_count = 0; // number of currently recognized CCD-bus modules

// SCI-bus
volatile bool pcm_idle = false;
volatile bool tcm_idle = false;
bool sci_enabled = true;
bool pcm_enabled = true;
bool tcm_enabled = true;

uint8_t pcm_bytes_buffer[TEMP_BUFFER_SIZE]; // max. SCI-bus message length to the PCM limited to 32 bytes, should be enough
uint8_t pcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool pcm_msg_pending = false; // flag for custom sci-bus message
uint8_t pcm_msg_to_send[TEMP_BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t pcm_msg_to_send_ptr = 0;  // custom sci-bus message length

uint8_t tcm_bytes_buffer[TEMP_BUFFER_SIZE]; // max. SCI-bus message length to the TCM limited to 32 bytes, should be enough
uint8_t tcm_bytes_buffer_ptr = 0; // pointer in the previous array
bool tcm_msg_pending = false; // flag for custom sci-bus message
uint8_t tcm_msg_to_send[TEMP_BUFFER_SIZE]; // custom sci-bus message is copied here
uint8_t tcm_msg_to_send_ptr = 0; // custom sci-bus message length

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
const uint16_t battery_rd1 = 120; // high resistor value in the divider (R19), multiplied by 10: 12 kOhm = 120
const uint16_t battery_rd2 = 50;  // low resistor value in the divider (R20), multiplied by 10: 5 kOhm = 50
uint16_t adc_max_value = 1023; // 1023 for 10-bit resolution
uint16_t battery_adc = 0;   // Raw analog reading is stored here
uint16_t battery_volts = 0; // Converted to battery voltage and multiplied by 100: 12.85V -> 1285

bool connected_to_vehicle = false;


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
        USB_RxBuf[tmphead] = (lastRxError << 8) + data;
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
        USB_DATA = USB_TxBuf[tmptail];  /* start transmission */
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

    /* if the ccd-bus went idle recently then this byte is definitely an ID-byte */
    if (ccd_idle)
    {
        lastRxError |= CCD_SOM; // add CCD_SOM (Start of Message) flag to the high byte
        ccd_idle = false; // re-arm idle detection
        total_ccd_msg_count++; // increment message counter for statistic purposes
    } 

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
        CCD_RxBuf[tmphead] = (lastRxError << 8) + data;
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
        CCD_DATA = CCD_TxBuf[tmptail];  /* start transmission */
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
        PCM_RxBuf[tmphead] = (lastRxError << 8) + data;
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
        PCM_DATA = PCM_TxBuf[tmptail];  /* start transmission */
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
        TCM_RxBuf[tmphead] = (lastRxError << 8) + data;
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
        TCM_DATA = TCM_TxBuf[tmptail];  /* start transmission */
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
    /* reset usb ringbuffer */
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
    USB_CONTROL = (1 << RXCIE0) | (1 << RXEN0) | (1 << TXEN0);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR0C = (1 << UCSZ00) | (1 << UCSZ01);

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
    uint16_t data;

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

    return data;
    
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
    uint16_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (USB_RxHead == USB_RxTail)
        {
            return UART_RX_NO_DATA;   /* no data available */
        }
    }
  
    tmptail = (USB_RxTail + 1) & USB_RX0_BUFFER_MASK;

    /* get data from receive buffer */
    data = USB_RxBuf[tmptail];

    return data;

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

        //USB_RxHead = 0; // equivalent with the equation above
        //USB_RxTail = 0;
        //USB_TxHead = 0;
        //USB_TxTail = 0;
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

        //USB_RxHead = 0; // equivalent with the equation above
        //USB_RxTail = 0;
        //USB_TxHead = 0;
        //USB_TxTail = 0;
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
    /* reset ccd ringbuffer */
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
    CCD_CONTROL = (1 << RXCIE1) | (1 << RXEN1) | (1 << TXEN1);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR1C = (1 << UCSZ10) | (1 << UCSZ11);
    
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
    uint16_t data;

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

    return data;
    
} /* ccd_getc */


/*************************************************************************
Function: ccd_peek()
Purpose:  return the next byte waiting in the receive buffer
          without removing it
Returns:  low byte:  next byte in the receive buffer
          high byte: error flags
**************************************************************************/
uint16_t ccd_peek(void)
{
    uint16_t tmptail;
    uint16_t data;

    ATOMIC_BLOCK(ATOMIC_FORCEON)
    {
        if (CCD_RxHead == CCD_RxTail)
        {
            return UART_RX_NO_DATA;   /* no data available */
        }
    }
  
    tmptail = (CCD_RxTail + 1) & CCD_RX1_BUFFER_MASK;

    /* get data from receive buffer */
    data = CCD_RxBuf[tmptail];

    return data;

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
uint16_t ccd_rx_available(void)
{
    uint16_t ret;
  
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
uint16_t ccd_tx_available(void)
{
    uint16_t ret;
  
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

        //CCD_RxHead = 0; // equivalent with the equation above
        //CCD_RxTail = 0;
        //CCD_TxHead = 0;
        //CCD_TxTail = 0;
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
        CCD_RxHead = CCD_RxTail;

        //CCD_RxHead = 0; // equivalent with the equation above
        //CCD_RxTail = 0;
        //CCD_TxHead = 0;
        //CCD_TxTail = 0;
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
    /* reset ccd ringbuffer */
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
    PCM_CONTROL = (1 << RXCIE2) | (1 << RXEN2) | (1 << TXEN2);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR2C = (1 << UCSZ20) | (1 << UCSZ21);
    
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
    uint16_t data;

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

    return data;
    
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
    uint16_t data;

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

    return data;

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
uint16_t pcm_rx_available(void)
{
    uint16_t ret;
  
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
uint16_t pcm_tx_available(void)
{
    uint16_t ret;
  
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

        //PCM_RxHead = 0; // equivalent with the equation above
        //PCM_RxTail = 0;
        //PCM_TxHead = 0;
        //PCM_TxTail = 0;
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
        PCM_RxHead = PCM_RxTail;

        //PCM_RxHead = 0; // equivalent with the equation above
        //PCM_RxTail = 0;
        //PCM_TxHead = 0;
        //PCM_TxTail = 0;
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
    /* reset ccd ringbuffer */
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
    TCM_CONTROL = (1 << RXCIE3) | (1 << RXEN3) | (1 << TXEN3);

    /* set frame format: asynchronous, 8 data bit, no parity, 1 stop bit */
    UCSR3C = (1 << UCSZ30) | (1 << UCSZ31);
    
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
    uint16_t data;

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

    return data;
    
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
    uint16_t data;

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

    return data;

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
uint16_t tcm_rx_available(void)
{
    uint16_t ret;
  
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
uint16_t tcm_tx_available(void)
{
    uint16_t ret;
  
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

        //TCM_RxHead = 0; // equivalent with the equation above
        //TCM_RxTail = 0;
        //TCM_TxHead = 0;
        //TCM_TxTail = 0;
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
        TCM_RxHead = TCM_RxTail;

        //TCM_RxHead = 0; // equivalent with the equation above
        //TCM_RxTail = 0;
        //TCM_TxHead = 0;
        //TCM_TxTail = 0;
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
void ccd_eom(void) { ccd_idle = true; /* set flag */ } // end of ccd_eom


/*************************************************************************
Function: ccd_ctrl()
Purpose:  called when the CCD-chip's CTRL pin is going low
**************************************************************************/
void ccd_active_byte(void) { ccd_ctrl = true; /* set flag */ } // end of ccd_ctrl


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
            //Timer1.initialize(1); // 1us period for 1 MHz clock (2 MHz interrupt), 2 us = 500 kHz, 3 us = 333 kHz, 40 us = 25 kHz
            //Timer1.pwm(CCD_CLOCK_PIN, 512);  // PB5 or D11 timer output pin (OC1A pin: Timer 1 / A output); 50% duty cycle: 0 - 1023, 512 = 50%
            //Timer1.start();

            TCCR1A = 0;                        // clear register
            TCCR1B = 0;                        // clear register
            TCNT1  = 0;                        // clear counter
            DDRB   = (1<<DDB5);                // set OC1A/PB5 as output
            TCCR1A = (1<<COM1A0);              // toggle OC1A on compare match
            OCR1A  = 7;                        // top value for counter, toggle after counting to 8 (0->7) = 2 MHz interrupt ( = 16 MHz clock frequency / 8)
            TCCR1B = (1<<WGM12) | (1<<CS10);   // CTC mode, prescaler clock/1 (no prescaler)
            
            break;
        }
        case STOP:
        {
            //Timer1.stop();

            TCCR1A = 0;
            TCCR1B = 0;
            TCNT1  = 0;
            OCR1A  = 0;
            
            break;
        }
        default:
        {
            //Timer1.stop();
            break;
        }
    }
    
} // end of ccd_clock_generator


/*************************************************************************
Function: configure_sci_bus()
Purpose:  change between SCI-bus configuration (A/B);
          - A: old configuration until 2002;
          - B: new configuration starting from 2002.
Returns:  none
**************************************************************************/
void configure_sci_bus(uint8_t bus, uint8_t configuration)
{
    switch (bus)
    {
        case NON: // 0x00, no data flow between scanner and SCI-bus
        {
            // TODO
            break;
        }
        case PCM: // 0x01
        {
            // TODO
            break;
        }
        case TCM: // 0x02
        {
            // TODO
            break;
        }
        case BOT: // 0x03
        {
            // TODO
            break;
        }
        default:  // 0x04-0xFF
        {
            // TODO
            break;
        }
    }
    
    switch (configuration)
    {
        // TODO: save current pin states to remember when necessary
        
        case SCI_CONF_A: // 0x01
        {
            // Configuration A
            digitalWrite(PA0, HIGH);
            digitalWrite(PA1, HIGH);
            digitalWrite(PA2, HIGH);
            digitalWrite(PA3, HIGH);
            digitalWrite(PA4, LOW);
            digitalWrite(PA5, LOW);
            digitalWrite(PA6, LOW);
            digitalWrite(PA7, LOW);
            break;
        }
        case SCI_CONF_B: // 0x02
        {
            // Configuration B
            digitalWrite(PA0, LOW);
            digitalWrite(PA1, LOW);
            digitalWrite(PA2, LOW);
            digitalWrite(PA3, LOW);
            digitalWrite(PA4, HIGH);
            digitalWrite(PA5, HIGH);
            digitalWrite(PA6, HIGH);
            digitalWrite(PA7, HIGH);
            break;
        }
        default: // 0x00, 0x03-0xFF
        {
            // No valid configuration selected, SCI-bus offline
            digitalWrite(PA0, LOW);
            digitalWrite(PA1, LOW);
            digitalWrite(PA2, LOW);
            digitalWrite(PA3, LOW);
            digitalWrite(PA4, LOW);
            digitalWrite(PA5, LOW);
            digitalWrite(PA6, LOW);
            digitalWrite(PA7, LOW);
            break;
        }
    }

} // end of configure_sci_bus


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
void send_usb_packet(uint8_t source, uint8_t target, uint8_t dc_command, uint8_t subdatacode, uint8_t *payloadbuff, uint16_t payloadbufflen)
{
    // Calculate the length of the full packet:
    // PAYLOAD length + 1 SYNC byte + 2 LENGTH bytes + 1 DATA CODE byte + 1 SUB-DATA CODE byte + 1 CHECKSUM byte
    uint16_t packet_length = payloadbufflen + 6;    
    uint8_t packet[packet_length]; // create a temporary byte-array
    bool payload_bytes = true;
    uint16_t calculated_checksum = 0;
    uint8_t datacode = 0;

    if (payloadbufflen <= 0) payload_bytes = false;
    else payload_bytes = true;

    // Assemble datacode from the first 3 input parameters
    // They all fit in one byte because only the lower two bits are considered meaningful for source and target, four bits for dc_command
    datacode = (source << 6) + (target << 4) + dc_command;
    //            xx000000   +    00yy0000   +  0000zzzz  =  xxyyzzzz  

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

    // Calculate checksum, skip SYNC byte by starting at index 1.
    // Procedure is simple: add every byte together after SYNC byte and keep the lower byte as result
    for (uint16_t j = 1; j < packet_length - 1; j++)
    {
        calculated_checksum += packet[j];
    }

    // Place checksum byte (lower half of the original 16-bit word)
    packet[packet_length - 1] = calculated_checksum & 0xFF;

    // Send the prepared packet through serial link
    for (uint16_t k = 0; k < packet_length; k++)
    {
        usb_putc(packet[k]); // write every byte in the packet to the usb-serial port
    }
    
} /* send_usb_packet */


/*************************************************************************
Function: handle_usb_data()
Purpose:  handle USB commands coming from an external computer
Note:     refer to ChryslerCCDSCIScanner_UART_Protocol.pdf to find out 
          more about the message format:
          | SYNC | LENGTH_HB | LENGTH_LB | DATACODE | SUBDATACODE | ?PAYLOAD? | CHECKSUM |
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

        uint32_t command_timeout_start = 0;
        bool command_timeout_reached = false;

        // Find the first SYNC byte (0x3D)
        // Peek the next available byte in the receive buffer
        command_timeout_start = millis(); // save current time
        while (((usb_peek() & 0xFF) != PACKET_SYNC_BYTE) && (usb_rx_available() > 0) && !command_timeout_reached)
        {
            usb_getc(); // if it's not the SYNC byte then get rid of it (read into oblivion)

            // Determine if timeout has been reached
            if (millis() - command_timeout_start > command_purge_timeout) command_timeout_reached = true;
        }
        if (command_timeout_reached)
        {
            send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occured, err, 1);
            return; // exit, let the loop call this function again
        }

        if (usb_rx_available() == 0) return; // exit if there's no data left

        // If the next byte is the SYNC byte then continue here.
        // Read and save 3 bytes into local variables (one SYNC and two LENGTH bytes).
        // All UART reads are masked with 0xFF because the ring buffer contains words (two bytes).
        // MSB contains UART flags, LSB contains the actual data byte. The "& 0xFF" mask gets rid of the MSB, effectively zeroing them out.
        // TODO: it might be a good idea to check those flags if the data byte is correct or not; now just assume evereything is fine.
        sync      = usb_getc() & 0xFF;
        length_hb = usb_getc() & 0xFF;
        length_lb = usb_getc() & 0xFF;

        // Calculate how much more bytes should we read.
        bytes_to_read = (length_hb << 8) + length_lb + 1; // +1 CHECKSUM byte

        // Maximum packet length is 1024 bytes.
        // Can't accept larger packets so if that's the case the function needs to exit after sending an error packet back to the laptop.
        // Also can't accept packet with less than 2 bytes length (datacode and subdatacode is always needed).
        if (((bytes_to_read - 3) > MAX_PAYLOAD_LENGTH) || ((bytes_to_read - 1) < 2))
        {
            send_usb_packet(from_usb, to_usb, ok_error, error_length_invalid_value, err, 1);
            return; // exit, let the loop call this function again
        }

        // Calculate the exact size of the payload
        payload_length = bytes_to_read - 3; // in this case we have to be careful not to count data code byte, sub-data code byte and checksum byte

        // Do not let this variable sink below zero
        if (payload_length < 0) payload_length = 0; // !!!

        // Wait here until all of the expected bytes are received or timeout occurs
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

        // Make some space for the payload bytes
        uint8_t cmd_payload[payload_length];

        // If the payload length is greater than zero then read those bytes too
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

        // Keep the low byte of the result
        calculated_checksum = calculated_checksum & 0xFF;

        // Compare calculated checksum to the received CHECKSUM byte
        if (calculated_checksum != checksum) // if they are not the same
        {
            // Echo back the full received packet in the payload section for further investigation
            uint16_t temp_packet_length = (length_hb << 8) + length_lb + 4;
            uint8_t temp_packet[temp_packet_length];
            
            // Start assembling the debug-packet by manually filling the first few slots
            temp_packet[0] = sync; // add received SYNC byte (0x3D by default)
            temp_packet[1] = length_hb; // add received LENGTH high byte
            temp_packet[2] = length_lb; // add received LENGTH low byte
            temp_packet[3] = datacode; // add received DATA CODE byte
            temp_packet[4] = subdatacode; // add received SUB-DATA CODE byte
            
            if (payload_bytes)
            {
                for (uint16_t i = 0; i < payload_length; i++)
                {
                    temp_packet[5 + i] = cmd_payload[i]; // add received PAYLOAD bytes (if any)
                }
            }

            temp_packet[temp_packet_length - 1] = checksum; // add received CHECKSUM byte
            send_usb_packet(from_usb, to_usb, ok_error, error_checksum_invalid_value, temp_packet, temp_packet_length);
            return; // exit, let the loop call this function again
        }

        // If everything is good then continue processing the packet...
        // Find out the source and the target of the packet by examining the DATA CODE byte's high nibble (upper 4 bits).
        uint8_t source = (datacode >> 6) & 0x03; // keep the upper two bits
        uint8_t target = (datacode >> 4) & 0x03; // keep the lower two bits
    
        // Extract DC command value from the low nibble (lower 4 bits)
        uint8_t dc_command = datacode & 0x0F;
    
        // Source isn't really checked here, the packet must come from an external computer...
        switch (target) // Evaluate target value.
        {
            case to_usb: // 0x00 - Scanner is the target.
            {
                switch (dc_command) // Evaluate DC command
                {
                    case reboot: // 0x00 - Reboot scanner.
                    {
                        //reset_diagnostic_comms();
            
                        // Send REBOOT packet back first to acknowledge the scanner has received the command
                        send_usb_packet(from_usb, to_usb, reboot, ok, ack, 1);

                        // Enter into an infinite loop. Watchdog timer doesn't get reset this way so it restarts the program eventually.
                        while(true);
                        break; // not necessary
                    }
                    case handshake: // 0x01 - Handshake request.
                    {
                        uint8_t handshake_array[21];

                        //reset_diagnostic_comms();
                        
                        // Copy handshake bytes from flash to ram (usb packet sender function doesn't know how to read from flash directly)
                        for (uint8_t i = 0; i < 21; i++)
                        {
                            handshake_array[i] = pgm_read_byte(&handshake_progmem[i]);
                        }
            
                        send_usb_packet(from_usb, to_usb, handshake, ok, handshake_array, 21);
                        //scanner_connected = true;
                        break;
                    }
                    case status: // 0x02 - Scanner status report request.
                    {
                        // Gather status data and send it back... not yet implemented
                        send_usb_packet(from_usb, to_usb, status, ok, ack, 1); // the payload should contain all information but now it's just an ACK byte (0x00)
                        break;
                    }
                    case settings: // 0x03 - Change scanner settings.
                    {
                        switch (subdatacode) // Evaluate sub-data code byte
                        {
                            case read_settings: // 0x01 - Read scanner settings directly from EEPROM
                            {
                                send_usb_packet(from_usb, to_usb, settings, read_settings, ack, 1); // acknowledge
                                break;
                            }
                            case write_settings: // 0x02 - Write scanner settings directly to EEPROM (values in PAYLOAD) (warning!)
                            {
                                send_usb_packet(from_usb, to_usb, settings, write_settings, ack, 1); // acknowledge with a zero byte in payload
                                break;
                            }
                            case enable_ccd_bus: // 0x03 - Enable CCD-bus communication
                            {
                                ccd_enabled = true;
                                send_usb_packet(from_usb, to_usb, settings, enable_ccd_bus, ack, 1); // acknowledge with a zero byte in payload
                                break;
                            }
                            case disable_ccd_bus: // 0x04 - Disable CCD-bus communication
                            {
                                ccd_enabled = false;
                                //stop_clock_generator();
                
                                send_usb_packet(from_usb, to_usb, settings, disable_ccd_bus, ack, 1); // acknowledge with a zero byte in payload
                                break;
                            }
                            case enable_sci_bus: // 0x05 - Enable SCI-bus communication
                            {
                                //init_sci_bus();
                                sci_enabled = true;
                                //select_sci_bus_target(cmd_payload[0]); // payload contains which module to connect by default: 0x01 if PCM, 0x02 if TCM
                                send_usb_packet(from_usb, to_usb, settings, enable_sci_bus, cmd_payload, 1); // acknowledge with 1 byte payload, that is the selected bus
                                break;
                            }
                            case disable_sci_bus: // 0x06 - Disable SCI-bus communication
                            {
                                sci_enabled = false;
                                //select_sci_bus_target(NON);
                                send_usb_packet(from_usb, to_usb, settings, disable_sci_bus, cmd_payload, 1); // acknowledge with 1 byte payload, that is none of the buses
                                break;
                            }
                            case enable_sci_bus_high_speed: // 0x13 - Enable SCI-bus high speed mode (62500 baud)
                            {
                                // The SCI-bus command 0x12 has to be sent previously, this is just an UART-speed changer case
                                // The following speed change will break all ongoing communication
                                //uart1_init(SCI_HI_BAUD); // configure UART1 to 62500 baud speed (62500 kbps high speed mode)
                                //while (uart1_available() > 0) uart1_getc(); // clear buffer
                                //sci_bus_high_speed = true; // set flag
                                send_usb_packet(from_usb, to_usb, settings, enable_sci_bus_high_speed, ack, 1); // acknowledge
                                break;
                            }
                            case disable_sci_bus_high_speed: // 0x14 - Disable SCI-bus high speed mode (7812.5 baud)
                            {
                                // The SCI-bus command 0xFE has to be sent previously, this is just an UART-speed changer case
                                // The following speed change will break all ongoing communication
                                //while (uart1_available() > 0) uart1_getc(); // clear buffer before exiting high speed mode
                                //uart1_init(SCI_CCD_LO_SPEED); // configure UART1 to 7812.5 baud speed (7812.5 kbps default SCI-bus speed)
                                //sci_bus_high_speed = false; // set flag
                                send_usb_packet(from_usb, to_usb, settings, disable_sci_bus_high_speed, ack, 1); // acknowledge
                                break;
                            }
                            case enable_act_led: // 0x1E - Enable ACT LED (action, blue color)
                            {
                                send_usb_packet(from_usb, to_usb, settings, enable_act_led, ack, 1);
                                break;
                            }
                            case disable_act_led: // 0x1F - Disable ACT LED (action, blue color)
                            {
                                send_usb_packet(from_usb, to_usb, settings, disable_act_led, ack, 1);
                                break;
                            }
                            default: // Other values are not used
                            {
                                send_usb_packet(from_usb, to_usb, settings, error_subdatacode_invalid_value, err, 1);
                                break;
                            }
                        }
                        break;
                    }
                    case request: // 0x04 - General request from the scanner
                    {
                        switch (subdatacode)  // Evaluate sub-data code byte
                        {
                            case firmware_version: // 0x01 - Scanner firmware version
                            {
                                uint8_t fw_value[2];
                                fw_value[0] = (FW >> 8) & 0xFF;
                                fw_value[1] = FW & 0xFF;
                                
                                send_usb_packet(from_usb, to_usb, response, firmware_version, fw_value, 2);
                                break;
                            }
                            case read_int_eeprom: // 0x02 - Read internal EEPROM in chunks (size in PAYLOAD)
                            {
                                send_usb_packet(from_usb, to_usb, response, read_int_eeprom, ack, 1);
                                break;
                            }
                            case read_ext_eeprom: // 0x03 - Read external EEPROM in chunks (size in PAYLOAD)
                            {
                                send_usb_packet(from_usb, to_usb, response, read_ext_eeprom, ack, 1);
                                break;
                            }
                            case write_int_eeprom: // 0x04 - Write internal EEPROM in chunks (value(s) in PAYLOAD)
                            {
                                send_usb_packet(from_usb, to_usb, response, write_int_eeprom, ack, 1);
                                break;
                            }
                            case write_ext_eeprom: // 0x05 - Write external EEPROM in chunks (value(s) in PAYLOAD)
                            {
                                send_usb_packet(from_usb, to_usb, response, write_ext_eeprom, ack, 1);
                                break;
                            }
                            case scan_ccd_bus_modules: // 0x06 - Scan CCD-bus modules
                            {
//                                scan_ccd = true;
//                
//                                if (payload_length == 2) // start-end addresses in payload, default timeout (200 ms)
//                                {
//                                    ccd_scan_start_addr = cmd_payload[0];
//                                    ccd_scan_end_addr = cmd_payload[1];
//                                    ccd_scan_request_timeout = 200;
//                                }
//                                else if (payload_length == 4) // start-end addresses and timeout in payload
//                                {
//                                    ccd_scan_start_addr = cmd_payload[0];
//                                    ccd_scan_end_addr = cmd_payload[1];
//                                    ccd_scan_request_timeout = (cmd_payload[2] << 8) | cmd_payload[3];
//                                }
//                                else // no payload, default settings
//                                {
//                                    ccd_scan_start_addr = 0x00;
//                                    ccd_scan_end_addr = 0xFE;
//                                    ccd_scan_request_timeout = 200;
//                                }
                
                                send_usb_packet(from_usb, to_usb, response, scan_ccd_bus_modules, ack, 1);
                                break;
                            }
                            case mcu_counter_value: // 0x07 - MCU counter value (milliseconds elapsed)
                            {
                                // How many milliseconds passed since power on?
                                uint32_t mcu_millis = millis();
                
                                // Create a local array of four bytes.
                                uint8_t mcu_millis_array[4];
                
                                // Break mcu_millis qword into its byte components (32bit -> 4x8bit).
                                mcu_millis_array[0] = (mcu_millis >> 24) & 0xFF;
                                mcu_millis_array[1] = (mcu_millis >> 16) & 0xFF;
                                mcu_millis_array[2] = (mcu_millis >> 8) & 0xFF;
                                mcu_millis_array[3] = mcu_millis & 0xFF;
                
                                // Send the packet back to the laptop
                                send_usb_packet(from_usb, to_usb, response, mcu_counter_value, mcu_millis_array, 4);
                                break;
                            }                 
                            case 0xFA: // debug: high speed sci-bus memory dump for cruise system analysis
                            {
//                                // command should look like this: 33 00 1C 14 FA F4 0A 0B 0C 0D 0E 11 12 13 14 16 17 1B 1E 1F 27 28 2F 35 36 37 3B 3E 3F 42 DE 21
//                                // payload being: F4 0A 0B 0C 0D 0E 11 12 13 14 16 17 1B 1E 1F 27 28 2F 35 36 37 3B 3E 3F 42 DE
//                                // first byte is always the memory pointer address and the rest are the requested memory location, let's make an array of them
//                
//                                uint16_t result_length = (2*payload_length) - 1 + 4;
//                                uint8_t result[result_length];
//                                uint32_t timestamp = millis();
//                                bool sci_addr_accepted = false;
//                                      
//                                // first 4 bytes are timestamp bytes (milliseconds elapsed since system start)
//                                result[0] = (timestamp >> 24) & 0xFF;
//                                result[1] = (timestamp >> 16) & 0xFF;
//                                result[2] = (timestamp >> 8) & 0xFF;
//                                result[3] = timestamp & 0xFF;
//                
//                                // 5th byte is the current SCI-bus memory pointer
//                                result[4] = 0xF4; // here are most of the interesting data
//                
//                                // cycle through all of the memory location, copy them too and leave a blank spaces each after another
//                                for (uint8_t i = 1; i < payload_length; i++)
//                                {
//                                  result[(2*i) - 1 + 4] = cmd_payload[i];
//                                }
//                
//                                // now that we have a half completed result array let's ask the PCM for the values of these memory locations
//                                for (uint8_t i = 1; i < payload_length; i++) // start from 1 instead of 0 to skip the first memory area selector byte
//                                {
//                                    // Save the current number of bytes in the receive buffer (most likely 0)
//                                    uint8_t numbytes = uart1_available();
//                  
//                                    // Put one/next byte to the SCI-bus
//                                    uart1_putc(cmd_payload[i]); // addresses are taken from the cmd_payload array directly
//                  
//                                    // Wait for answer or timeout
//                                    bool timeout_reached = false;
//                                    uint32_t timeout_start = millis();
//                  
//                                    // Unlike CCD-bus, SCI-bus needs a little bit of delay between bytes,
//                                    // so we check here if the PCM/TCM has echoed the byte back.
//                                    while ((numbytes >= uart1_available()) && !timeout_reached)
//                                    {
//                                        // Check the timeout condition only, the received byte is stored automatically in the ringbuffer
//                                        if (millis() - timeout_start > SCI_INTERMESSAGE_RESPONSE_DELAY) timeout_reached = true;
//                                    }
//                  
//                                    // If the SCI-bus responds in the given timeframe then save the value in the array's blank spaces
//                                    if (!timeout_reached)
//                                    {
//                                        // save the received bytes to the result array's blank spaces
//                                        result[2*i + 4] = uart1_getc() & 0xFF;
//                                    }
//                                    else // if no response then save 0xFF as a result
//                                    {
//                                        timeout_reached = false; // re-arm, don't care if true or false
//                                        result[2*i + 4] = 0xFF; // in case of timeout the result is always 0xFF
//                                    }
//                                }
                
                                // send result back to laptop, note that the subdatacode is the request command byte (0xFA)
                                //send_usb_packet(from_usb, to_usb, debug, 0xF4, result, result_length);
                                      
                                break;
                            }
                                  
                            case 0xFB: // debug: high speed sci-bus memory area dump, WARNING: this is not loop-safe, the program will freeze until this "case" is done
                            {
//                                // prepare result array
//                                uint8_t result[256];
//                                bool sci_addr_accepted = false;
//                
//                                // make sure the address selector command is accepted by the PCM
//                                // it has to echo back the address byte ($F2, $F3, $F4...) that is the first payload byte
//                                while (!sci_addr_accepted)
//                                {
//                                    // Save the current number of bytes in the receive buffer (most likely 0)
//                                    uint8_t numbytes = uart1_available();
//                  
//                                    // Put the address selector byte to the SCI-bus (first byte of payload section)
//                                    uart1_putc(cmd_payload[0]);
//                  
//                                    // Wait for answer or timeout
//                                    bool timeout_reached = false;
//                                    uint32_t timeout_start = millis();
//                                    while ((numbytes >= uart1_available()) && !timeout_reached)
//                                    {
//                                        // Check the timeout condition only, the received byte is stored automatically in the ringbuffer
//                                        if (millis() - timeout_start > SCI_INTERMESSAGE_RESPONSE_DELAY) timeout_reached = true;
//                                    }
//                  
//                                    if (timeout_reached)
//                                    {
//                                        timeout_reached = false; // re-arm, don't care if true or false
//                                        break; // exit from this "case" if the SCI-bus won't respond
//                                    }
//                  
//                                    uint8_t dummy2 = uart1_getc() & 0xFF; // get the echo from the SCI-bus
//                                    if (dummy2 == cmd_payload[0]) sci_addr_accepted = true; // exit this loop if the proper byte has been received
//                                }
//                
//                                // now cycle through every possible address one time only!
//                                // skip the last 16 bytes (they are most likely area selector bytes)
//                                for (uint8_t i = 0; i < 0xF0; i++)
//                                {
//                                    // Save the current number of bytes in the receive buffer (most likely 0)
//                                    uint8_t numbytes = uart1_available();
//                  
//                                    // Put one/next byte to the SCI-bus
//                                    uart1_putc(i); // address is the loop-variable itself, so this line is okay
//                  
//                                    // Wait for answer or timeout
//                                    bool timeout_reached = false;
//                                    uint32_t timeout_start = millis();
//                  
//                                    // Unlike CCD-bus, SCI-bus needs a little bit of delay between bytes,
//                                    // so we check here if the PCM/TCM has echoed the byte back.
//                                    while ((numbytes >= uart1_available()) && !timeout_reached)
//                                    {
//                                        // Check the timeout condition only, the received byte is stored automatically in the ringbuffer
//                                        if (millis() - timeout_start > SCI_INTERMESSAGE_RESPONSE_DELAY) timeout_reached = true;
//                                    }
//                  
//                                    // If the SCI-bus doesn't respond in the given timeframe then save an $FF byte in the array
//                                    if (timeout_reached)
//                                    {
//                                        timeout_reached = false; // re-arm, don't care if true or false
//                                        result[i] = 0xFF; 
//                                    }
//                                    else
//                                    {
//                                        result[i] = uart1_getc() & 0xFF;
//                                    }
//                                }
//                
//                                // add the last 16 bytes (0xFF) manually to the result array so there's a whole 256-bytes block
//                                for (uint8_t k = 0; k < 16; k++)
//                                {
//                                    result[0xF0 + k] = 0xFF;
//                                }
                
                                // send result back to laptop, note that the subdatacode is the actual SCI-bus memory area address
                                //send_usb_packet(from_usb, to_usb, debug, cmd_payload[0], result, 256);
                                break;
                            }
                            case 0xFC: // enable sci-bus command 14, sensor request
                            {
                                //sci_command_14 = true; 
                                //send_usb_packet(from_sci_bus, to_usb, send_msg, ok, ack, 1); // acknowledge
                                break;
                            }
                            case 0xFD: // disable sci-bus command 14, sensor request
                            {
                                //sci_command_14 = false; 
                                //send_usb_packet(from_sci_bus, to_usb, send_msg, ok, ack, 1); // acknowledge
                                break;
                            }
                            case 0xFE: // dummy sensor request
                            {
//                                // Fill the pending buffer with the message to be sent
//                                for (uint8_t i = 0; i < payload_length; i++)
//                                {
//                                  sci_bus_msg_to_send[i] = cmd_payload[i];
//                                }
//                                sci_bus_msg_to_send_ptr = payload_length;
//                
//                                // Set flag so the main loop knows there's something to do
//                                sci_bus_msg_pending = true;
                
                                //send_usb_packet(from_sci_bus, to_usb, send_msg, ok, ack, 1); // acknowledge
                                break;
                            }
                            default: // Other values are not used
                            {
                                send_usb_packet(from_usb, to_usb, response, error_subdatacode_invalid_value, err, 1);
                                break;
                            }
                        }
                        break;
                    }
                    case self_diag: // 0x0A - Run self-diagnostics
                    {
                        send_usb_packet(from_usb, to_usb, self_diag, ok, ack, 1); // acknowledge
                        break;
                    }
                    case make_backup: // 0x0B - Create scanner settings backup packet (int. EEPROM dump)
                    {
                        send_usb_packet(from_usb, to_usb, make_backup, ok, ack, 1); // acknowledge
                        break;
                    }
                    case restore_backup: // 0x0C - Restore scanner settings from backup
                    {
                        send_usb_packet(from_usb, to_usb, restore_backup, ok, ack, 1); // acknowledge
                        break;
                    }
                    case restore_default: // 0x0D - Restore default scanner settings (factory reset)
                    {
                        send_usb_packet(from_usb, to_usb, restore_default, ok, ack, 1); // acknowledge
                        break;
                    }
                    case debug: // 0x0E - Debug
                    {
                        //dummy_packet:
                        //33
                        //67
                        //23
                        //64
                        //33
                        //34
                        //77
                        //AA
                        //33
                        //33
                        //33
                        //33
                        //AA
                        //33 00 17 41 00 43 48 52 59 53 4C 45 52 43 43 44 53 43 49 53 43 41 4E 4E 45 52 77
                        //BB
                        //AA
                        //00
                        //45
                        //33 00 17 41 00 43 48 52 59 53 4C 45 52 43 43 44 53 43 49 53 43 41 4E 4E 45 52 77
                        //33 00 17 41 00 43 48 52 59 53 4C 45 52 43 43 44 53 43 49 53 43 41 4E 4E 45 52 77
                        //BB
                        //00
                        //00
                        //45
                        //23
                        //98
                        //33 00 17 41 00 43 48 52 59 53 4C 45 52 43 43 44 53 43 49 53 43 41 4E 4E 45 52 77
                        //33
                        //66
                        //33
                        //22
                        //00
                        //33 00 17 41 00 43 48 52 59 53 4C 45 52 43 43 44 53 43 49 53 43 41 4E 4E 45 52 77
                        //88
                        //AA
                        //BB
                        //CC
                        //33
            
                        // Send bytes directly with uart2_putc command (unsafe in most situations)
                        for (uint8_t i = 0; i < 167; i++)
                        {
                            //uart2_putc(pgm_read_byte(&dummy_packet[i]));
                        }
            
                        // Send ack byte back
                        send_usb_packet(from_usb, to_usb, debug, ok, ack, 1); // acknowledge
                        break;
                    }
                    case ok_error: // 0x0F - OK/ERROR message
                    {
                        send_usb_packet(from_usb, to_usb, ok_error, ok, ack, 1); // acknowledge
                        break;
                    }
                    default: // Other values are not used.
                    {
                        send_usb_packet(from_usb, to_usb, ok_error, error_datacode_invalid_dc_command, err, 1);
                        break;
                    }
                }
                break;
            }
            case to_ccd: // 0x01 - CCD-bus is the target.
            {
                switch (dc_command) // Evaluate data code low nibble
                {
                    case send_msg: // 0x06 - Send message to the CCD-bus
                    {
                        // Fill the pending buffer with the message to be sent
                        for (uint8_t i = 0; i < payload_length; i++)
                        {
                            ccd_msg_to_send[i] = cmd_payload[i];
                        }
                        ccd_msg_to_send_ptr = payload_length;

                        // TODO: make sure the checksum byte is correct
            
                        // Set flag so the main loop knows there's something to do
                        ccd_msg_pending = true;
            
                        send_usb_packet(from_ccd, to_usb, send_msg, ok, ack, 1); // acknowledge
                        break;
                    }
                    case send_rep_msg: // 0x07 - Send message(s) repeatedly to the CCD-bus
                    {
                        send_usb_packet(from_ccd, to_usb, send_rep_msg, ok, ack, 1); // acknowledge
                        break;
                    }
                    case stop_msg_flow: // 0x08 - Stop message flow to the CCD-bus
                    {
                        //ccd_bus_msg_rep = false;
                        //ccd_bus_msg_to_send_ptr = 0;
            
                        send_usb_packet(from_ccd, to_usb, stop_msg_flow, ok, ack, 1); // acknowledge
                        break;
                    }
                    default: // Other values are not used.
                    {
                        send_usb_packet(from_ccd, to_usb, ok_error, error_datacode_invalid_dc_command, err, 1);
                        break;
                    }
                }
                break;
            }
            case to_pcm: // 0x02 - SCI-bus (PCM) is the target.
            case to_tcm: // 0x03 - SCI-bus (TCM) is the target.
            {
                switch (dc_command) // Evaluate data code low nibble
                {
                    case send_msg: // 0x01 - Send message to the SCI-bus
                    {
                        // Fill the pending buffer with the message to be sent
                        if (target == to_pcm)
                        {
                            for (uint8_t i = 0; i < payload_length; i++)
                            {
                                pcm_msg_to_send[i] = cmd_payload[i];
                            }
                            pcm_msg_to_send_ptr = payload_length;
                            pcm_msg_pending = true; // set flag so the main loop knows there's something to do
                            send_usb_packet(from_pcm, to_usb, send_msg, ok, ack, 1); // acknowledge
                        }
                        else if (target == to_tcm)
                        {
                            for (uint8_t i = 0; i < payload_length; i++)
                            {
                                tcm_msg_to_send[i] = cmd_payload[i];
                            }
                            tcm_msg_to_send_ptr = payload_length;
                            tcm_msg_pending = true; // set flag so the main loop knows there's something to do
                            send_usb_packet(from_tcm, to_usb, send_msg, ok, ack, 1); // acknowledge
                        }
                        break;
                    }
                    case send_rep_msg: // 0x02 - Send message(s) repeatedly to the SCI-bus
                    {
                        /**********************************************************************
                        Frame format:
                        33 00 11 37 F4 03 07 00 02 04 02 02 03 F4 1A F4 1B F4 1C 1D 9D 
            
                        $33: SYNC byte
                        $00 $11: LENGTH bytes
                        $37: DATA CODE byte (from laptop, to sci-bus, send repeated messages)
                        $F4: SUB-DATA CODE byte (sci-bus high speed memory area target)
                          $03: length of parameter location and length arrays
                          $07: length of parameters array
                          $00 $02 $04: 3 location bytes 
                          $02 $02 $03: 3 length bytes
                          $F4 $1A: first parameter (starts at relative address 0)
                          $F4 $1B: second parameter (starts at relative address 2)
                          $F4 $1C $1D: third parameter (starts at relative address 4)
                        $9D: CHECKSUM byte
            
                        **********************************************************************/
                              
                        switch (subdatacode)
                        {
                            case 0xF4: // High speed mode memory area
                            {
//                                // Read payload for parameter location(s) and command(s) 
//                                if (payload_bytes)
//                                {
//                                    // The first byte in the payload refers to the length of the parameter locations and length
//                                    if (cmd_payload[0] <= 64)
//                                    {
//                                        // Add these bytes to two separate array with for-loops
//                                        for (uint8_t m = 0; m < cmd_payload[0]; m++)
//                                        {
//                                            mode_f4_parameters_loc[m] = cmd_payload[2 + m]; // First two bytes have to be ignored
//                                        }
//                    
//                                        for (uint8_t m = 0; m < cmd_payload[0]; m++)
//                                        {
//                                            mode_f4_parameters_length[m] = cmd_payload[2 + cmd_payload[0] + m];
//                                        }
//                                    }
//                  
//                                    // The second byte in the payload refers to the length of the parameter command list
//                                    if (cmd_payload[1] <= 128)
//                                    {
//                                        // Add these bytes to a separate array
//                                        for (uint8_t n = 0; n < cmd_payload[1]; n++)
//                                        {
//                                            mode_f4_parameters[n] = cmd_payload[2 + (2 * cmd_payload[0]) + n]; // First two bytes and the parameter locations and lengths have to be ignored
//                                        }
//                                    }
//                  
//                                    mode_f4_ptr_length = cmd_payload[0];
//                                    mode_f4_length = cmd_payload[1];
//                                    mode_f4_ptr = 0;
//                  
//                                    // Set flag so the SCI-bus routine begins execution as soon as possible
//                                    sci_hs_mode_f4 = true;
//                                    //send_usb_packet(from_sci_bus, to_usb, send_rep_msg, ok, ack, 1); // acknowledge
//                  
//                                    // Debug: send back these three arrays separately
//                                    send_usb_packet(from_usb, to_usb, send_rep_msg, 0x00, mode_f4_parameters_loc, mode_f4_ptr_length);
//                                    send_usb_packet(from_usb, to_usb, send_rep_msg, 0x01, mode_f4_parameters_length, mode_f4_ptr_length);
//                                    send_usb_packet(from_usb, to_usb, send_rep_msg, 0x02, mode_f4_parameters, mode_f4_length);
//                                }
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                        break;
                    }
                    case stop_msg_flow: // 0x08 - Stop message flow to the SCI-bus
                    {
                        //sci_bus_msg_rep = false;
                        //sci_hs_mode_f4 = false;
                        //mode_f4_ptr_length = 0;
                        //mode_f4_length = 0;
                        //mode_f4_ptr = 0;
            
                        //sci_bus_msg_to_send_ptr = 0;
            
                        //send_usb_packet(from_sci_bus, to_usb, send_msg, ok, ack, 1); // acknowledge
                        break;
                    }
                    default: // Other values are not used.
                    {
                        // Sub-data code missing, not enough information is given
                        if (target == to_pcm) send_usb_packet(from_pcm, to_usb, ok_error, error_datacode_invalid_dc_command, err, 1);
                        else if (target == to_tcm) send_usb_packet(from_tcm, to_usb, ok_error, error_datacode_invalid_dc_command, err, 1);
                        break;
                    }
                }
                break;
            }
            default: // Other values are not used.
            {
                send_usb_packet(from_usb, to_usb, ok_error, error_datacode_source_target_conflict, err, 1);
                break;            
            }
        } // switch (target)                
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
    // #1 - Collecting bytes from the CCD-bus:
    // Check if there are any data bytes in the CCD-ringbuffer
    // Note that this piece of code only reads 1 byte every program loop but it's fast enough to not cause problems
    if (ccd_rx_available() > 0)
    {
        // Peek one byte (don't remove it from the buffer yet)
        uint16_t dummy_read = ccd_peek();

        // #2 - Detecting end of message condition
        // If the peaked value is an ID-byte then send the previous bytes to the laptop (if any)
        if ((dummy_read & CCD_SOM) && (ccd_bytes_buffer_ptr > 0))
        {
            // Check if the crc byte in the message is correct
            uint16_t calculated_checksum = 0;
            for (uint16_t i = 0; i < (ccd_bytes_buffer_ptr - 1); i++)
            {
                calculated_checksum += ccd_bytes_buffer[i];
            }
            calculated_checksum = calculated_checksum & 0xFF;

            if (ccd_bytes_buffer[ccd_bytes_buffer_ptr - 1] == calculated_checksum)
            {
                // Send CCD-bus message back to the laptop
                send_usb_packet(from_ccd, to_usb, receive_msg, ok, ccd_bytes_buffer, ccd_bytes_buffer_ptr);
            }
            else
            {
                // Send CCD-bus message back to the laptop with wrong checksum flag
                send_usb_packet(from_ccd, to_usb, receive_msg, error_checksum_invalid_value, ccd_bytes_buffer, ccd_bytes_buffer_ptr);
            }

            //process_ccd_msg(); // TODO
            ccd_bytes_buffer_ptr = 0; // reset pointer so the new message starts at the beginning of the array

            // And finally save this byte as the first one in the buffer
            ccd_bytes_buffer[ccd_bytes_buffer_ptr] = ccd_getc() & 0xFF; // getc = read it while deleting it from the buffer
            ccd_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
            if (ccd_bytes_buffer_ptr > (TEMP_BUFFER_SIZE - 1)) ccd_bytes_buffer_ptr = 0; // don't let buffer pointer overflow, instead overwrite previous message
        }
        else // get this byte and save to the temporary buffer in the next available position
        {
            ccd_bytes_buffer[ccd_bytes_buffer_ptr] = ccd_getc() & 0xFF; // getc = read it while deleting it from the buffer
            ccd_bytes_buffer_ptr++; // increase pointer value by one so it points to the next empty slot in the buffer
            if (ccd_bytes_buffer_ptr > (TEMP_BUFFER_SIZE - 1)); // don't let buffer pointer overflow, send the whole 32 byte buffer back to the laptop to figure out what's wrong
            {
                ccd_bytes_buffer_ptr = 0;
                send_usb_packet(from_ccd, to_usb, receive_msg, error_buffer_overflow, ccd_bytes_buffer, TEMP_BUFFER_SIZE);
            }
            // It's highly unlikely that a vehicle computer sends a message bigger than the buffer size (32 bytes in this case), so don't worry about this branch
        }
    }

    // #3 - Sending bytes to the CCD-bus:
    // If there's a message to be sent to the CCD-bus and the bus happens to be idling then send it here and now
    // TODO: perhaps the active byte flag can be checked here to be extra sure we can have control over the CCD-bus
    // NOTE: the CCD-transceiver chip is smart enough to not let us talk when there's an active byte being sent on the bus
    // so feel free to send messages until they are echoed back correctly
    if (ccd_msg_pending && ccd_idle)
    {
        // Check if the crc byte in the message is correct
        uint16_t calculated_checksum = 0;
        for (uint16_t i = 0; i < (ccd_msg_to_send_ptr - 1); i++)
        {
            calculated_checksum += ccd_msg_to_send[i];
        }
        calculated_checksum = calculated_checksum & 0xFF;

        // Correct the last checksum byte before transmission, if wrong
        if (ccd_msg_to_send[ccd_msg_to_send_ptr - 1] != calculated_checksum)
        {
            ccd_msg_to_send[ccd_msg_to_send_ptr - 1] = calculated_checksum;
        }
        
        // The first byte has to be sent as fast as possible to get control over the CCD-bus
        for (uint8_t i = 0; i < ccd_msg_to_send_ptr; i++) // repeat for the length of the message
        {
            ccd_putc(ccd_msg_to_send[i]); // fill the transmit buffer with data, transmission occurs automatically if the code senses at least 1 byte in this buffer
        }
        ccd_msg_to_send_ptr = 0; // reset pointer
        ccd_msg_pending = false; // re-arm, make it possible to send a message again
    }
  
} // end of handle_ccd_data


/*************************************************************************
Function: handle_sci_data()
Purpose:  handle SCI-bus messages from both PCM and TCM
**************************************************************************/
void handle_sci_data(void)
{
    if (pcm_enabled)
    {
        // TODO
    }

    if (tcm_enabled)
    {
        // TODO
    }
    
} // end of handle_sci_data


/*************************************************************************
Function: check_battery_volts()
Purpose:  measure battery voltage through a resistor divider circuit
**************************************************************************/
void check_battery_volts(void)
{
    battery_adc = analogRead(BATT);
    battery_volts = (uint16_t)(((((battery_adc/adc_max_value)*(adc_supply_voltage/100.0))*((battery_rd1/10.0)+(battery_rd2/10.0)))/(battery_rd2/10.0))*100.0); // an overly complicated code for a resistor voltage divider
    if (battery_volts > 700) connected_to_vehicle = true; // consider a sensed voltage of 7.00V to be connected
    else connected_to_vehicle = false; // or bad battery
    
} // end of check_battery_volts


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

#endif // CCDSCIUART_H

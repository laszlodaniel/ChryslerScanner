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

// ATmega2560-specific UART registers
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
#define CCD_DIAG_REQ  0xB2  // Diagnostic request ID-byte for CCD-bus
#define CCD_DIAG_RESP 0xF2  // Diagnostic response ID-byte for CCD-bus

// Baudrate prescaler calculation: UBRR = (F_CPU / (16 * BAUDRATE)) - 1
#define LOBAUD  127  // prescaler for   7812.5 baud speed (CCD-SCI / default Low-Speed diagnostic command mode)
#define HIBAUD  15   // prescaler for  62500   baud speed (SCI / High-Speed parameter interrogation command mode)
#define USBBAUD 8    // prescaler for 115200   baud speed (USB)

//#define ASCII_OUTPUT  // Choose this if you want a simple text output over USB for serial monitors
#define PACKET_OUTPUT // Choose this if you want a byte-based packet output over USB for third-party applications

// Make sure they are not defined at the same time
#if defined(ASCII_OUTPUT) || defined(PACKET_OUTPUT)
#undef ASCII_OUTPUT // packet output ftw
#endif

// If accidentally no output is given 
#if !defined(ASCII_OUTPUT) && !defined(PACKET_OUTPUT)
#define PACKET_OUTPUT // right
#endif

#define CCD_CLOCK_PIN 11 // clock generator output for CCD-bus

#define SCI_INTERFRAME_RESPONSE_DELAY   100  // milliseconds elapsed after last received byte to consider the SCI-bus idling
#define SCI_INTERMESSAGE_RESPONSE_DELAY 50   // ms
#define SCI_INTERMESSAGE_REQUEST_DELAY  50   // ms

// PCM-TCM selector
#define NON 0x00 // neither computer is active
#define PCM 0x01 // pcm only
#define TCM 0x02 // tcm only
#define BOT 0x03 // both computer active

#define CONF_A 0x01 // SCI-bus configuration A
#define CONF_B 0x02 // SCI-bus configuration B

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

// Packet related stuff
#define SYNC_BYTE           0x33
#define MAX_PAYLOAD_LENGTH  2042
#define ACK                 0x00        // Acknowledge byte
#define ERR                 0xFF        // Error byte
#define EMPTY_PAYLOAD       0xFE 

// DATA CODE byte building blocks
// Source and Target masks (high nibble (4 bits))
#define from_usb          0x00
#define from_ccd_bus      0x01
#define from_pcm          0x02
#define from_tcm          0x03
#define to_usb            0x00
#define to_ccd_bus        0x01
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
// TODO

// DC command 0x04-0x05 (request)
// TODO

// DC command 0x0E (debug)
// TODO

// DC command 0x0F (OK/ERROR)
#define ok                                      0x00
#define error_sync_invalid_value                0x01
#define error_length_invalid_value              0x02
#define error_datacode_same_source_target       0x03
#define error_datacode_source_target_conflict   0x04
#define error_datacode_invalid_target           0x05
#define error_datacode_invalid_dc_command       0x06
#define error_subdatacode_invalid_value         0x07
#define error_subdatacode_not_enough_info       0x08
#define error_payload_missing_values            0x09
#define error_payload_invalid_values            0x0A
#define error_checksum_invalid_value            0x0B
#define error_packet_invalid_frame_format       0x0C
#define error_packet_timeout_occured            0x0D
#define error_packet_unknown_source             0x0E
#define error_scanner_internal_error            0x0F
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

bool usb_enabled = true; // better be

// CCD-bus
volatile bool ccd_idle = false;
volatile bool ccd_ctrl = false;
volatile uint8_t total_ccd_msg_count = 0;
bool ccd_enabled = true;
uint8_t ccd_bus_bytes_buffer[32]; // max. CCD-bus message length limited to 32 bytes, should be enough
uint8_t ccd_bus_bytes_buffer_ptr = 0; // pointer in the previous array
bool ccd_bus_msg_pending = false;
uint8_t ccd_bus_msg_to_send[16];
uint8_t ccd_bus_msg_to_send_ptr = 0;
uint8_t ccd_bus_module_addr[32]; // recognised CCD-bus modules are stored here with ascending order
uint8_t ccd_bus_module_num = 0; // number of currently recognized CCD-bus modules

// SCI-bus
volatile bool pcm_idle = false;
volatile bool tcm_idle = false;
bool sci_enabled = true;
bool pcm_enabled = true;
bool tcm_enabled = true;

// Packet related variables
// Timeout values for packets
uint8_t command_timeout = 100; // milliseconds
uint16_t command_purge_timeout = 1000; // milliseconds

// Store handshake in the PROGram MEMory (flash)
const uint8_t handshake_progmem[] PROGMEM = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x43, 0x43, 0x44, 0x53, 0x43, 0x49, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 };
//                                            "C     H     R     Y     S     L     E     R     C     C     D     S     C     I     S     C     A     N     N     E     R"
uint8_t handshake_array[21];

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
        USB_RxBuf[tmphead] = (data << 8) + lastRxError;
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

    /* if the ccd-bus went idle since the last byte has been received then this byte is an ID-byte */
    if (ccd_idle)
    {
        lastRxError |= CCD_SOM; // add CCD_SOM (Start of Message) flag to the high byte
        ccd_idle = false; // re-arm idle detection
        total_ccd_msg_count++;
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
        CCD_RxBuf[tmphead] = (data << 8) + lastRxError;
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
        PCM_RxBuf[tmphead] = (data << 8) + lastRxError;
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
        TCM_RxBuf[tmphead] = (data << 8) + lastRxError;
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

    usb_enabled = true; // not a good idea to turn it off
    
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
Returns:  integer number of bytes in the receive buffer
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
Function: ccd_eom()
Purpose:  called when the CCD-chip's IDLE pin is going low,
          the last byte received was the end of the message (EOM)
          and the next byte going to be the new message's ID byte
**************************************************************************/
void ccd_eom(void)
{
    ccd_idle = true;  // set flag so the main loop knows, simple as that
    total_ccd_msg_count++;  // increment message counter for statistic purposes
    
} /* ccd_eom */

/*************************************************************************
Function: ccd_ctrl()
Purpose:  called when the CCD-chip's CTRL pin is going low
**************************************************************************/
void ccd_active_byte(void)
{
    ccd_ctrl = true;  // set flag so the main loop knows, simple as that
    
} /* ccd_ctrl */

/*************************************************************************
Function: start_ccd_clock_generator()
Purpose:  generates 1 MHz clock signal for the CDP68HC68S1 chip
Note:     
          2 MHz interrupt frequency (toggles between 0 and 1).
          *      *      *      *      *      *      *      *      *
     1    |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯¯¯¯|      |¯¯¯¯
          ˄      ˅      ˄      ˅      ˄      ˅      ˄      ˅      ˄
     0 ___|      |______|      |______|      |______|      |______| 
          *             *             *             *             *
          1 MHz clock frequency, 50% duty cycle (whole period = 1 tick).
          This function works on its own, no need to monitor its activity!
**************************************************************************/
void start_ccd_clock_generator(void)
{
    Timer1.initialize(1); // 1us period for 1 MHz clock, 2 us = 500 kHz, 3 us = 333 kHz, 40 us = 25 kHz
    Timer1.pwm(CCD_CLOCK_PIN, 511);  // PB5 or D11 timer output pin (OC1A pin: Timer 1 / A output); 50% duty cycle: 0 - 1023, 511 = 50%
    Timer1.start();
    
} // end of start_ccd_clock_generator

/*************************************************************************
Function: stop_ccd_clock_generator()
Purpose:  stop clock generator for the CDP68HC68S1 chip
**************************************************************************/
void stop_ccd_clock_generator(void)
{
    Timer1.stop();
    
} // end of stop_ccd_clock_generator



/*************************************************************************
Function: configure_sci_bus()
Purpose:  change between SCI-bus configuration (A/B);
          - A: old configuration until 2002;
          - B: new configuration starting from 2002.
Params:   - configuration: true if A; false if B
          - highspeed: true if on, false if off
Returns:  none
**************************************************************************/
void configure_sci_bus(uint8_t bus, uint8_t configuration, uint8_t speed)
{
    switch (bus)
    {
        case NON: // 0x00, no data flow between scanner and SCI-bus
        {

        }
        case PCM: // 0x01
        {
            
        }
        case TCM: // 0x02
        {
            
        }
        case BOT: // 0x03
        {
            
        }
        default:  // 0x04-0xFF
        {
            
        }
    }
    
    switch (configuration)
    {
        // TODO: save current pin states to remember when necessary
        
        case CONF_A: // 0x01
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
        }
        case CONF_B: // 0x02
        {
            // Configuration A
            digitalWrite(PA0, LOW);
            digitalWrite(PA1, LOW);
            digitalWrite(PA2, LOW);
            digitalWrite(PA3, LOW);
            digitalWrite(PA4, HIGH);
            digitalWrite(PA5, HIGH);
            digitalWrite(PA6, HIGH);
            digitalWrite(PA7, HIGH);
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
        }
    }

    switch (speed)
    {
        case LOSPEED: // 0x01
        {
            
        }
        case HISPEED: // 0x02
        {
            
        }
        default: // 0x00, 0x03-0xFF
        {
            
        }
    }

} // end of configure_sci_bus



/*************************************************************************
Function: send_usb_packet()
Purpose:  assemble and send data packet through serial link (UART0)
Inputs:   - one sync byte (0x33 by default)
          - one source byte,
          - one target byte,
          - one datacode value byte, these three are used to calculate the DATA CODE byte
          - one SUB-DATA CODE byte,
          - pointer to the PAYLOAD bytes array (name of the array),
            (it must be previously filled with data)
          - PAYLOAD length
Returns:  0 if transmission was OK
          1 if ERROR
Note:     SYNC, LENGTH and CHECKSUM bytes are calculated automatically;
          Payload can be omitted if a (uint8_t*)0x00 value is used in conjunction with 0 length,
      see examples throughout the code.
**************************************************************************/
void send_usb_packet(uint8_t source, uint8_t target, uint8_t dc_command, uint8_t subdatacode, uint8_t *payloadbuff, uint16_t payloadbufflen)
{
    // Calculate the length of the full packet
    // PAYLOAD length + 1 SYNC byte + 2 LENGTH bytes + 1 DATA CODE byte + 1 SUB-DATA CODE byte + 1 CHECKSUM byte
    uint16_t packet_length = payloadbufflen + 6;
    uint8_t packet[packet_length];
    bool payload_bytes = true;
    uint16_t calculated_checksum = 0;
    uint8_t datacode = 0;

    if (payloadbufflen <= 0) payload_bytes = false;
    else payload_bytes = true;

    // Assemble datacode from the first 3 input parameters
    // They all fit in one byte because only the lower two bits are considered meaningful
    datacode |= (source << 6) | (target << 4) | dc_command;

    // Start assembling the packet by manually filling the first few slots
    packet[0] = SYNC_BYTE; // add SYNC byte (0x33 by default)
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
    for (uint16_t j = 1; j < packet_length - 1; j++)
    {
        calculated_checksum += packet[j];
    }

    // Place checksum byte
    packet[packet_length - 1] = calculated_checksum & 0xFF;

    // Send the prepared packet through serial link
    for (uint16_t k = 0; k < packet_length; k++)
    {
        usb_putc(packet[k]); // write every byte in the packet to the usb-serial port
    }
    
} /* send_usb_packet */



/*************************************************************************
Function: handle_usb_rx_bytes()
Purpose:  handle USB commands coming from an external computer
**************************************************************************/
void handle_usb_rx_bytes(void)
{
    // TODO
    
} // end of handle_usb_rx_bytes



/*************************************************************************
Function: handle_ccd_rx_bytes()
Purpose:  handle CCD-bus messages
**************************************************************************/
void handle_ccd_rx_bytes(void)
{
    // #1 - Collecting bytes from the CCD-bus:
    // Check if there are any data bytes in the CCD-ringbuffer
    if (ccd_rx_available() > 0)
    {
        // Peek one byte (don't remove it from the buffer yet)
        uint16_t dummy_read = ccd_peek();

        // #2 - Detecting end of message condition
        // If it's an ID-byte then send the previous bytes to the laptop (if any)
        if ((((dummy_read >> 8) & 0xFF) == CCD_SOM) && (ccd_bus_bytes_buffer_ptr > 0))
        {
            // Send CCD-bus message (if there's at least one complete) back to the laptop
            //              where         to         what     ok/error flag         buffer          length
            send_usb_packet(from_ccd_bus, to_usb, receive_msg, ok, ccd_bus_bytes_buffer, ccd_bus_bytes_buffer_ptr);
            //process_ccd_msg(); // TODO
            ccd_bus_bytes_buffer_ptr = 0; // reset pointer

            // And save this byte as the first one in the buffer
            ccd_bus_bytes_buffer[ccd_bus_bytes_buffer_ptr] = ccd_getc() & 0xFF; // getc = read it while deleting it from the buffer
            ccd_bus_bytes_buffer_ptr++; // increace pointer value by one so it points to the next empty slot in the buffer
        }
        else // get this byte and save to the temporary buffer in the next available position
        {
            ccd_bus_bytes_buffer[ccd_bus_bytes_buffer_ptr] = ccd_getc() & 0xFF; // getc = read it while deleting it from the buffer
            ccd_bus_bytes_buffer_ptr++; // increace pointer value by one so it points to the next empty slot in the buffer
        }
    }

    // #3 - Sending bytes to the CCD-bus:
    // If there's a message to be sent to the CCD-bus and the bus happens to be idling then send it here and now
    if (ccd_bus_msg_pending && ccd_idle) // note that two different conditions must be true at the same time
    {
        for (uint8_t i = 0; i < ccd_bus_msg_to_send_ptr; i++) // repeat for the length of the message
        {
            ccd_putc(ccd_bus_msg_to_send[i]);
            // There's no need to delay between bytes because of the UART hardware's 
            // inherent delay while loading a byte into its own transmit buffer.
        }
        ccd_bus_msg_to_send_ptr = 0; // reset pointer
        ccd_bus_msg_pending = false; // re-arm, make it possible to send a message again
    }
  
} // end of handle_ccd_rx_bytes



/*************************************************************************
Function: handle_sci_rx_bytes()
Purpose:  handle SCI-bus messages from both PCM and TCM
**************************************************************************/
void handle_sci_rx_bytes(void)
{
    if (pcm_enabled)
    {
        // TODO
    }

    if (tcm_enabled)
    {
        // TODO
    }
    
} // end of handle_sci_rx_bytes


#endif // CCDSCIUART_H

#include "sci.h"
#include "tools.h"
#include "lcd.h"
#include <util/atomic.h>

volatile uint8_t PCM_RxBuf[PCM_RX2_BUFFER_SIZE];
volatile uint8_t PCM_TxBuf[PCM_TX2_BUFFER_SIZE];
volatile uint8_t PCM_RxHead;
volatile uint8_t PCM_RxTail;
volatile uint8_t PCM_TxHead;
volatile uint8_t PCM_TxTail;
volatile uint8_t PCM_LastRxError;

volatile uint8_t TCM_RxBuf[TCM_RX3_BUFFER_SIZE];
volatile uint8_t TCM_TxBuf[TCM_TX3_BUFFER_SIZE];
volatile uint8_t TCM_RxHead;
volatile uint8_t TCM_RxTail;
volatile uint8_t TCM_TxHead;
volatile uint8_t TCM_TxTail;
volatile uint8_t TCM_LastRxError;

uint8_t sci_hi_speed_memarea[16] = { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF };

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
        if (!pcm.invert_logic) PCM_RxBuf[tmphead] = data;
        else PCM_RxBuf[tmphead] = ((data << 4) & 0xF0) | ((data >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    PCM_LastRxError = lastRxError;

    pcm.last_byte_millis = millis();
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
        if (!pcm.invert_logic) PCM_DATA = PCM_TxBuf[tmptail]; /* start transmission */
        else PCM_DATA = ((PCM_TxBuf[tmptail] << 4) & 0xF0) | ((PCM_TxBuf[tmptail] >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
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
        if (!tcm.invert_logic) TCM_RxBuf[tmphead] = data;
        else TCM_RxBuf[tmphead] = ((data << 4) & 0xF0) | ((data >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    TCM_LastRxError = lastRxError;

    tcm.last_byte_millis = millis();
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
        if (!tcm.invert_logic) TCM_DATA = TCM_TxBuf[tmptail]; /* start transmission */
        else TCM_DATA = ((TCM_TxBuf[tmptail] << 4) & 0xF0) | ((TCM_TxBuf[tmptail] >> 4) & 0x0F); // last 4 bits come first, then first 4 bits
    }
    else
    {
        /* tx buffer empty, disable UDRE interrupt */
        TCM_CONTROL &= ~_BV(TCM_UDRIE);
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
    tmptail = (PCM_RxTail + 1) & PCM_RX2_BUFFER_MASK;
  
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
  
    tmptail = (PCM_RxTail + 1 + index) & PCM_RX2_BUFFER_MASK;

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
        ret = (PCM_RX2_BUFFER_SIZE + PCM_RxHead - PCM_RxTail) & PCM_RX2_BUFFER_MASK;
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
        ret = (PCM_TX2_BUFFER_SIZE + PCM_TxHead - PCM_TxTail) & PCM_TX2_BUFFER_MASK;
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
    tmptail = (TCM_RxTail + 1) & TCM_RX3_BUFFER_MASK;
  
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
uint16_t tcm_peek(uint16_t index)
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
        ret = (TCM_RX3_BUFFER_SIZE + TCM_RxHead - TCM_RxTail) & TCM_RX3_BUFFER_MASK;
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
        ret = (TCM_TX3_BUFFER_SIZE + TCM_TxHead - TCM_TxTail) & TCM_TX3_BUFFER_MASK;
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
Function: configure_sci_bus()
Purpose:  as the name says
Input:    data: SCI-bus channel, configuration and speed settings
          obd1: OBD1 SCI engine cable enable or disable setting
Note:     data bits description:
          B7:6: module bits: 10: PCM
                             11: TCM
                             00: USB (not used here)
                             01: CCD (not used here)
          B5: change bit:     0: leave settings
                              1: change settings
          B4: state bit:      0: disabled
                              1: enabled
          B3: logic bit:      0: non-inverted
                              1: inverted (OBD1 SCI engine cable used)
          B2: config bits:    0: "A" configuration
                              1: "B" configuration
          B1:0 speed bits:   00: 976.5 baud
                             01: 7812.5 baud
                             10: 62500 baud
                             11: 125000 baud
**************************************************************************/
void configure_sci_bus(uint8_t data)
{
    if (data == 0x00)
    {
        send_usb_packet(from_usb, to_usb, settings, set_sci_bus, err, 1); // error
        return;
    }

    if (((data >> 6) & 0x03) == 0x02) // PCM
    {
        if ((data >> 5) & 0x01) // change settings
        {
            if ((data >> 4) & 0x01) // enable PCM
            {
                pcm.enabled = true;
                
                if ((data >> 3) & 0x01) // inverted logic signal
                {
                    pcm.invert_logic = true;
                }
                else // non-inverted logic signal
                {
                    pcm.invert_logic = false;
                }

                if ((data >> 2) & 0x01) // configuration "B"
                {
                    // Disable all A-configuration pins first
                    digitalWrite(A_PCM_RX_EN, LOW);  // SCI-BUS_A_PCM_RX disabled
                    digitalWrite(A_PCM_TX_EN, LOW);  // SCI-BUS_A_PCM_TX disabled
                    digitalWrite(A_TCM_RX_EN, LOW);  // SCI-BUS_A_TCM_RX disabled
                    digitalWrite(A_TCM_TX_EN, LOW);  // SCI-BUS_A_TCM_TX disabled
                    // Enable B-configuration pins for PCM (TCM pins don't interfere here)
                    digitalWrite(B_PCM_RX_EN, HIGH); // SCI-BUS_B_PCM_RX enabled
                    digitalWrite(B_PCM_TX_EN, HIGH); // SCI-BUS_B_PCM_TX enabled
                }
                else // configuration "A"
                {
                    tcm.enabled = false; // TCM pins interfere with PCM pins in configuration "A"
                    cbi(tcm.bus_settings, 4); // clear 4th enable bit

                    // Disable all B-configuration pins first
                    digitalWrite(B_PCM_RX_EN, LOW);  // SCI-BUS_B_PCM_RX disabled
                    digitalWrite(B_PCM_TX_EN, LOW);  // SCI-BUS_B_PCM_TX disabled
                    digitalWrite(B_TCM_RX_EN, LOW);  // SCI-BUS_B_TCM_RX disabled
                    digitalWrite(B_TCM_TX_EN, LOW);  // SCI-BUS_B_TCM_TX disabled
                    // Disable A-configuration pins for TCM first, they interfere
                    digitalWrite(A_TCM_RX_EN, LOW);  // SCI-BUS_A_TCM_RX disabled
                    digitalWrite(A_TCM_TX_EN, LOW);  // SCI-BUS_A_TCM_TX disabled
                    // Enable A-configuration pins for PCM
                    digitalWrite(A_PCM_RX_EN, HIGH); // SCI-BUS_A_PCM_RX enabled
                    digitalWrite(A_PCM_TX_EN, HIGH); // SCI-BUS_A_PCM_TX enabled
                }

                if ((data & 0x03) == 0x00) // 976.5 baud
                {
                    pcm_init(ELBAUD);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = ELBAUD;
                }
                else if ((data & 0x03) == 0x01) // 7812.5 baud
                {
                    pcm_init(LOBAUD);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = LOBAUD;
                }
                else if ((data & 0x03) == 0x02) // 62500 baud
                {
                    pcm_init(HIBAUD);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = HIBAUD;
                }
                else if ((data & 0x03) == 0x03) // 125000 baud
                {
                    pcm_init(EHBAUD);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = EHBAUD;
                }
                else // 7812.5 baud
                {
                    pcm_init(LOBAUD);
                    //pcm_rx_flush();
                    //pcm_tx_flush();
                    pcm.speed = LOBAUD;
                }
            }
            else // disable PCM
            {
                pcm.enabled = false;
                
                digitalWrite(A_PCM_RX_EN, LOW); // SCI-BUS_A_PCM_RX disabled
                digitalWrite(A_PCM_TX_EN, LOW); // SCI-BUS_A_PCM_TX disabled
                digitalWrite(B_PCM_RX_EN, LOW); // SCI-BUS_B_PCM_RX disabled
                digitalWrite(B_PCM_TX_EN, LOW); // SCI-BUS_B_PCM_TX disabled
            }

            pcm.bus_settings = data; // copy settings to the PCM bus settings variable
            cbi(pcm.bus_settings, 5); // clear 5th change bit, it's only applicable for this function
        }
        else
        {
            // don't change anything
        }
    }

    else if (((data >> 6) & 0x03) == 0x03) // TCM
    {
        if ((data >> 5) & 0x01) // change settings
        {
            if ((data >> 4) & 0x01) // enable TCM
            {
                tcm.enabled = true;
                
                if ((data >> 3) & 0x01) // inverted logic signal
                {
                    tcm.invert_logic = true;
                }
                else // non-inverted logic signal
                {
                    tcm.invert_logic = false;
                }

                if ((data >> 2) & 0x01) // configuration "B"
                {
                    // Disable all A-configuration pins first
                    digitalWrite(A_PCM_RX_EN, LOW);  // SCI-BUS_A_PCM_RX disabled
                    digitalWrite(A_PCM_TX_EN, LOW);  // SCI-BUS_A_PCM_TX disabled
                    digitalWrite(A_TCM_RX_EN, LOW);  // SCI-BUS_A_TCM_RX disabled
                    digitalWrite(A_TCM_TX_EN, LOW);  // SCI-BUS_A_TCM_TX disabled
                    // Enable B-configuration pins for TCM (PCM pins don't interfere here)
                    digitalWrite(B_TCM_RX_EN, HIGH); // SCI-BUS_B_TCM_RX enabled
                    digitalWrite(B_TCM_TX_EN, HIGH); // SCI-BUS_B_TCM_TX enabled
                }
                else // configuration "A"
                {
                    pcm.enabled = false; // PCM pins interfere with TCM pins in configuration "A"
                    cbi(pcm.bus_settings, 4); // clear 4th enable bit
                    
                    // Disable all B-configuration pins first
                    digitalWrite(B_PCM_RX_EN, LOW);  // SCI-BUS_B_PCM_RX disabled
                    digitalWrite(B_PCM_TX_EN, LOW);  // SCI-BUS_B_PCM_TX disabled
                    digitalWrite(B_TCM_RX_EN, LOW);  // SCI-BUS_B_TCM_RX disabled
                    digitalWrite(B_TCM_TX_EN, LOW);  // SCI-BUS_B_TCM_TX disabled
                    // Disable A-configuration pins for PCM first, they interfere
                    digitalWrite(A_PCM_RX_EN, LOW);  // SCI-BUS_A_PCM_RX disabled
                    digitalWrite(A_PCM_TX_EN, LOW);  // SCI-BUS_A_PCM_TX disabled
                    // Enable A-configuration pins for TCM
                    digitalWrite(A_TCM_RX_EN, HIGH); // SCI-BUS_A_TCM_RX enabled
                    digitalWrite(A_TCM_TX_EN, HIGH); // SCI-BUS_A_TCM_TX enabled
                }

                if ((data & 0x03) == 0x00) // 976.5 baud
                {
                    tcm_init(ELBAUD);
                    tcm_rx_flush();
                    tcm_tx_flush();
                    tcm.speed = ELBAUD;
                }
                else if ((data & 0x03) == 0x01) // 7812.5 baud
                {
                    tcm_init(LOBAUD);
                    tcm_rx_flush();
                    tcm_tx_flush();
                    tcm.speed = LOBAUD;
                }
                else if ((data & 0x03) == 0x02) // 62500 baud
                {
                    tcm_init(HIBAUD);
                    tcm_rx_flush();
                    tcm_tx_flush();
                    tcm.speed = HIBAUD;
                }
                else if ((data & 0x03) == 0x03) // 125000 baud
                {
                    tcm_init(EHBAUD);
                    tcm_rx_flush();
                    tcm_tx_flush();
                    tcm.speed = EHBAUD;
                }
                else // 7812.5 baud
                {
                    tcm_init(LOBAUD);
                    tcm_rx_flush();
                    tcm_tx_flush();
                    tcm.speed = LOBAUD;
                }
            }
            else // disable TCM
            {
                tcm.enabled = false;
                
                digitalWrite(A_TCM_RX_EN, LOW); // SCI-BUS_A_TCM_RX disabled
                digitalWrite(A_TCM_TX_EN, LOW); // SCI-BUS_A_TCM_TX disabled
                digitalWrite(B_TCM_RX_EN, LOW); // SCI-BUS_B_TCM_RX disabled
                digitalWrite(B_TCM_TX_EN, LOW); // SCI-BUS_B_TCM_TX disabled
            }

            tcm.bus_settings = data; // copy settings to the TCM bus settings variable
            cbi(tcm.bus_settings, 5); // clear 5th change bit, it's only applicable for this function
        }
        else
        {
            // don't change anything
        }
    }

    uint8_t ret[2];
    ret[0] = 0x00;
    ret[1] = pcm.bus_settings;
    send_usb_packet(from_usb, to_usb, settings, set_sci_bus, ret, 2); // acknowledge
    ret[0] = 0x00;
    ret[1] = tcm.bus_settings;
    send_usb_packet(from_usb, to_usb, settings, set_sci_bus, ret, 2); // acknowledge
}

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
    uint32_t timeout_start = 0;
    bool timeout_reached = false;
    
    if (pcm.enabled)
    {
        // Handle completed messages
        if (pcm.speed == LOBAUD) // handle low-speed mode first (7812.5 baud)
        {
            if (!pcm.actuator_test_running && !pcm.ls_request_running) // send message back to laptop normally
            {
                if (((millis() - pcm.last_byte_millis) > SCI_LS_T3_DELAY) && (pcm_rx_available() > 0))
                { 
                    pcm.message_length = pcm_rx_available();
                    uint8_t usb_msg[TIMESTAMP_LENGTH+pcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                    update_timestamp(current_timestamp); // get current time for the timestamp
                    
                    for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                    {
                        usb_msg[i] = current_timestamp[i];
                    }
                    for (uint8_t i = 0; i < pcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                    {
                        usb_msg[TIMESTAMP_LENGTH+i] = pcm_getc() & 0xFF;
                    }

                    send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+pcm.message_length); // send message to laptop
                    handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    
                    if (usb_msg[4] == 0x12) // pay attention to special bytes (speed change)
                    {
                        sbi(pcm.bus_settings, 5); // set change settings bit
                        sbi(pcm.bus_settings, 4); // set enable bit
                        sbi(pcm.bus_settings, 1); // set/clear speed bits (62500 baud)
                        cbi(pcm.bus_settings, 0); // set/clear speed bits (62500 baud)
                        configure_sci_bus(pcm.bus_settings);
                    }

                    if (pcm.repeat && !pcm.repeat_iterate) // prepare next repeated message
                    {
                        if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                        {
                            bool match = true;
                            for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                            {
                                if (pcm.msg_buffer[i] != usb_msg[TIMESTAMP_LENGTH+i]) match = false; // compare received bytes with message sent
                            }
                            if (match) pcm.repeat_next = true; // if echo is correct prepare next message
                        }
                        else if (pcm.msg_to_transmit_count > 1) // multiple messages
                        {
                            bool match = true;
                            for (uint8_t i = 0; i < pcm.repeated_msg_length; i++)
                            {
                                if (pcm.msg_buffer[pcm.msg_buffer_ptr + 1 + i] != usb_msg[TIMESTAMP_LENGTH+i]) match = false; // compare received bytes with message sent
                            }
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
                            pcm.repeat_iterate = false;
                            pcm.repeat_list_once = false;
                        }
                    }
                    else if (pcm.repeat && pcm.repeat_iterate)
                    {
                        if (pcm.message_length == (pcm.repeated_msg_length + 1)) // received message has to be 1 byte bigger than what was sent
                        {
                            pcm.repeat_next = true;
                            pcm.repeat_retry_counter = 0;
                        }
                        else
                        {
                            pcm.msg_tx_pending = true; // send the same message again if no answer is received
                            pcm.repeat_retry_counter++;
                            if (pcm.repeat_retry_counter > 10)
                            {
                                //pcm.repeat = false; // don't repeat after 10 failed attempts
                                //pcm.repeat_stop = true;
                                pcm.repeat_next = true; // give up this parameter and jump to the next one
                                pcm.repeat_retry_counter = 0;
                            }
                            delay(200);
                        }

                        if (pcm.repeat_stop)
                        {
                            pcm.msg_buffer_ptr = 0;
                            pcm.repeated_msg_length = 0;
                            pcm.repeat = false;
                            pcm.repeat_next = false;
                            pcm.repeat_iterate = false;
                            pcm.repeat_list_once = false;

                            uint8_t ret[2] = { 0x00, to_pcm }; // improvised payload array with only 1 element which is the target bus
                            send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                        }
                    }
                    
                    pcm_rx_flush();
                    pcm.msg_rx_count++;
                }
            }
            else // 0x13 and 0x2A has a weird delay between messages so they are handled here with a smaller delay
            {
                if (((millis() - pcm.last_byte_millis) > 10) && (pcm_rx_available() > 0))
                { 
                    // Stop actuator test command is accepted
                    if ((pcm_peek(0) == 0x13) && (pcm_peek(1) == 0x00) && (pcm_peek(2) == 0x00))
                    {
                        pcm.message_length = pcm_rx_available();
                        uint8_t usb_msg[TIMESTAMP_LENGTH+pcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                        update_timestamp(current_timestamp); // get current time for the timestamp
                        pcm.actuator_test_running = false;
                        pcm.actuator_test_byte = 0;
                        
                        for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                        {
                            usb_msg[i] = current_timestamp[i];
                        }
                        for (uint8_t i = 0; i < pcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                        {
                            usb_msg[TIMESTAMP_LENGTH+i] = pcm_getc() & 0xFF;
                        }
                        
                        send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+pcm.message_length);
                        handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    }
                    // Stop broadcasting request bytes command is accepted
                    else if ((pcm_peek(0) == 0x2A) && (pcm_peek(1) == 0x00)) // The 0x00 byte is not echoed back by the PCM so don't look for a third byte
                    {
                        pcm.message_length = pcm_rx_available();
                        uint8_t usb_msg[TIMESTAMP_LENGTH+pcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                        update_timestamp(current_timestamp); // get current time for the timestamp
                        pcm.ls_request_running = false;
                        pcm.ls_request_byte = 0;

                        for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                        {
                            usb_msg[i] = current_timestamp[i];
                        }
                        for (uint8_t i = 0; i < pcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                        {
                            usb_msg[TIMESTAMP_LENGTH+i] = pcm_getc() & 0xFF;
                        }

                        send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+pcm.message_length);
                        handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    }
                    else // only ID-bytes are received, the beginning of the messages must be supplemented so that the GUI understands what it is about
                    {
                        pcm.message_length = pcm_rx_available();
                        uint8_t usb_msg[7]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                        update_timestamp(current_timestamp); // get current time for the timestamp
                        
                        for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                        {
                            usb_msg[i] = current_timestamp[i];
                        }

                        if (pcm.actuator_test_running && !pcm.ls_request_running) // if the actuator test mode is active then put two bytes before the received byte
                        {
                            usb_msg[4] = 0x13;
                            usb_msg[5] = pcm.actuator_test_byte;
                        }
                        
                        if (pcm.ls_request_running && !pcm.actuator_test_running) // if the request mode is active then put two bytes before the received byte; this is kind of annying, why isn't one time response enough... there's nothing "running" like in the case of actuator tests
                        {
                            usb_msg[4] = 0x2A;
                            usb_msg[5] = pcm.ls_request_byte;
                        }

                        usb_msg[6] = pcm_getc() & 0xFF; // BUG: this byte is different at first when the PCM begins to broadcast a single byte repeatedly, but it should be overwritten very soon after
                        send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, 7); // the message length is fixed by the nature of the active commands
                        handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                    }

                    pcm_rx_flush();
                    pcm.msg_rx_count++;
                }
            }
        }
        else if (pcm.speed == HIBAUD) // handle high-speed mode (62500 baud), no need to wait for message completion here, it is already handled when the message was sent
        {
            if (pcm_rx_available() > 0)
            {
                pcm.message_length = pcm_rx_available();
                uint16_t packet_length = TIMESTAMP_LENGTH + (2*pcm.message_length) - 1;
                uint8_t usb_msg[packet_length]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp

                // Request and response bytes are mixed together in a single message:
                // 00 00 00 00 F4 0A 00 0B 00 0C 00 0D 00...
                // 00 00 00 00: timestamp
                // F4: RAM table
                // 0A: RAM address
                // 00: RAM value at 0A
                // 0B: RAM address
                // 00: RAM value at 0B

                for (uint8_t i = 0; i < TIMESTAMP_LENGTH; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }

                if (pcm.msg_to_transmit_count == 1)
                {
                    usb_msg[4] = pcm.msg_buffer[0]; // put RAM table byte first
                    pcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) 
                    {
                        usb_msg[5+(i*2)] = pcm.msg_buffer[i+1]; // put original request message byte next
                        usb_msg[5+(i*2)+1] = pcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(from_pcm, to_usb, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }
                else if (pcm.msg_to_transmit_count > 1)
                {
                    usb_msg[4] = pcm.msg_buffer[pcm.msg_buffer_ptr + 1]; // put RAM table byte first
                    pcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < pcm.repeated_msg_length; i++) 
                    {
                        usb_msg[5+(i*2)] = pcm.msg_buffer[pcm.msg_buffer_ptr+i+2]; // put original request message byte next
                        usb_msg[5+(i*2)+1] = pcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(from_pcm, to_usb, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }

                if (usb_msg[4] == 0xFE) // pay attention to special bytes (speed change)
                {
                    sbi(pcm.bus_settings, 5); // set change settings bit
                    sbi(pcm.bus_settings, 4); // set enable bit
                    cbi(pcm.bus_settings, 1); // set/clear speed bits (7812.5 baud)
                    sbi(pcm.bus_settings, 0); // set/clear speed bits (7812.5 baud)
                    configure_sci_bus(pcm.bus_settings);
                }

                if (pcm.repeat && !pcm.repeat_iterate)
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
                        pcm.repeat_iterate = false;
                        pcm.repeat_list_once = false;
                    }
                }
                else if (pcm.repeat && pcm.repeat_iterate)
                {
                    // TODO
                    if (true) // check proper echo
                    {
                        pcm.repeat_next = true; // accept echo without verification...
                    }
                    else
                    {
                        pcm.msg_tx_pending = true; // send the same message again
                    }
                    
                    if (pcm.repeat_stop)
                    {
                        pcm.msg_buffer_ptr = 0;
                        pcm.repeated_msg_length = 0;
                        pcm.repeat = false;
                        pcm.repeat_next = false;
                        pcm.repeat_iterate = false;
                        pcm.repeat_list_once = false;
                    }
                }

                handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                pcm_rx_flush();
                pcm.msg_rx_count++;
            }
        }
        else // other non-standard speeds are handled here
        {
            if (((millis() - pcm.last_byte_millis) > SCI_LS_T3_DELAY) && (pcm_rx_available() > 0))
            { 
                pcm.message_length = pcm_rx_available();
                uint8_t usb_msg[TIMESTAMP_LENGTH+pcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (PCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp
                
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < pcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    usb_msg[TIMESTAMP_LENGTH+i] = pcm_getc() & 0xFF;
                }

                send_usb_packet(from_pcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+pcm.message_length); // send message to laptop

                // No automatic speed change for 976.5 and 125000 baud!

                if (pcm.repeat && !pcm.repeat_iterate)
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
                        pcm.repeat_iterate = false;
                        pcm.repeat_list_once = false;
                    }
                }
                else if (pcm.repeat && pcm.repeat_iterate)
                {
                    // TODO
                    if (true) // check proper echo
                    {
                        pcm.repeat_next = true; // accept echo without verification...
                    }
                    else
                    {
                        pcm.msg_tx_pending = true; // send the same message again
                    }
                    
                    if (pcm.repeat_stop)
                    {
                        pcm.msg_buffer_ptr = 0;
                        pcm.repeated_msg_length = 0;
                        pcm.repeat = false;
                        pcm.repeat_next = false;
                        pcm.repeat_iterate = false;
                        pcm.repeat_list_once = false;
                    }
                }
                
                handle_lcd(from_pcm, usb_msg, 4, TIMESTAMP_LENGTH+pcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                pcm_rx_flush();
                pcm.msg_rx_count++;
            }
        }

        // Repeated messages are prepared here for transmission
        if (pcm.repeat && (pcm_rx_available() == 0))
        {
            current_millis = millis(); // check current time
            if ((current_millis - pcm.repeated_msg_last_millis) > pcm.repeated_msg_interval) // wait between messages
            {
                pcm.repeated_msg_last_millis = current_millis;
                
                if (pcm.repeat_next && !pcm.repeat_iterate) // no iteration, same message over and over again
                {
                    // The message is already in the pcm.msg_buffer array, just set flags
                    pcm.msg_tx_pending = true; // set flag
                    pcm.repeat_next = false;
                }
                else if (pcm.repeat_next && pcm.repeat_iterate) // iteration, message is incremented for every repeat according to settings
                {
                    if (pcm.repeated_msg_length == 0x04) // 4-bytes length
                    {
                        if (pcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            pcm.msg_buffer[0] = (pcm.repeated_msg_raw_start >> 24) & 0xFF; // decompose raw message from its integer form to byte components
                            pcm.msg_buffer[1] = (pcm.repeated_msg_raw_start >> 16) & 0xFF;
                            pcm.msg_buffer[2] = (pcm.repeated_msg_raw_start >> 8) & 0xFF;
                            pcm.msg_buffer[3] = pcm.repeated_msg_raw_start & 0xFF;
                            pcm.msg_buffer_ptr = 4;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint32_t message = to_uint32(pcm.msg_buffer[0], pcm.msg_buffer[1], pcm.msg_buffer[2], pcm.msg_buffer[3]);
                            message += pcm.repeated_msg_increment; // add increment
                            pcm.msg_buffer[0] = (message >> 24) & 0xFF; // decompose integer into byte compontents again
                            pcm.msg_buffer[1] = (message >> 16) & 0xFF;
                            pcm.msg_buffer[2] = (message >> 8) & 0xFF;
                            pcm.msg_buffer[3] = message & 0xFF;
                            pcm.msg_buffer_ptr = 4;
                            
                            if ((message + pcm.repeated_msg_increment) > pcm.repeated_msg_raw_end) pcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (pcm.repeated_msg_length == 0x03) // 3-bytes length
                    {
                        if (pcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            pcm.msg_buffer[0] = (pcm.repeated_msg_raw_start >> 16) & 0xFF; // decompose raw message from its integer form to byte components
                            pcm.msg_buffer[1] = (pcm.repeated_msg_raw_start >> 8) & 0xFF;
                            pcm.msg_buffer[2] = pcm.repeated_msg_raw_start & 0xFF;
                            pcm.msg_buffer_ptr = 3;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint32_t message = to_uint32(0, pcm.msg_buffer[0], pcm.msg_buffer[1], pcm.msg_buffer[2]);
                            message += pcm.repeated_msg_increment; // add increment
                            pcm.msg_buffer[0] = (message >> 16) & 0xFF; // decompose integer into byte compontents again
                            pcm.msg_buffer[1] = (message >> 8) & 0xFF;
                            pcm.msg_buffer[2] = message & 0xFF;
                            pcm.msg_buffer_ptr = 3;
                            
                            if ((message + pcm.repeated_msg_increment) > pcm.repeated_msg_raw_end) pcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (pcm.repeated_msg_length == 0x02) // 2-bytes length
                    {
                        if (pcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            pcm.msg_buffer[0] = (pcm.repeated_msg_raw_start >> 8) & 0xFF; // decompose raw message from its integer form to byte components
                            pcm.msg_buffer[1] = pcm.repeated_msg_raw_start & 0xFF;
                            pcm.msg_buffer_ptr = 2;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint16_t message = to_uint16(pcm.msg_buffer[0], pcm.msg_buffer[1]);
                            message += pcm.repeated_msg_increment; // add increment
                            pcm.msg_buffer[0] = (message >> 8) & 0xFF; // decompose integer into byte compontents again
                            pcm.msg_buffer[1] = message & 0xFF;
                            pcm.msg_buffer_ptr = 2;
                            
                            if ((message + pcm.repeated_msg_increment) > pcm.repeated_msg_raw_end) pcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (pcm.repeated_msg_length == 0x01) // 1-byte length
                    {
                        if (pcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            pcm.msg_buffer[0] = pcm.repeated_msg_raw_start & 0xFF; // decompose raw message from its integer form to byte components
                            pcm.msg_buffer_ptr = 1;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            pcm.msg_buffer[0] += pcm.repeated_msg_increment; // add increment
                            pcm.msg_buffer_ptr = 1;
                            
                            if ((pcm.msg_buffer[0] + pcm.repeated_msg_increment) > pcm.repeated_msg_raw_end) pcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }

                    pcm.msg_tx_pending = true; // set flag
                    pcm.repeat_next = false;
                }
            }
        }

        // Send message
        if (pcm.msg_tx_pending && (pcm_rx_available() == 0))
        {
            if (pcm.speed == LOBAUD) // low speed mode (7812.5 baud), half-duplex mode approach
            {
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
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
                    if ((pcm.msg_buffer_ptr > 1) && (pcm_rx_available() > 1) && (pcm_peek(0) == 0x13) && (pcm_peek(1) != 0x00))
                    {
                        // See if the PCM starts to broadcast the actuator test byte
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        while ((pcm_rx_available() <= pcm.msg_buffer_ptr) && !timeout_reached)
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
                            pcm.actuator_test_running = true;
                            pcm.actuator_test_byte = pcm.msg_buffer[1];
                            pcm_rx_flush(); // workaround for the first false received message
                        }
                    }
    
                    if ((pcm.msg_buffer_ptr > 1) && (pcm_rx_available() > 1) && (pcm_peek(0) == 0x2A) && (pcm_peek(1) != 0x00))
                    {
                        // See if the PCM starts to broadcast the diagnostic request byte
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        while ((pcm_rx_available() <= pcm.msg_buffer_ptr) && !timeout_reached)
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
                            pcm.ls_request_running = true;
                            pcm.ls_request_byte = pcm.msg_buffer[1];
                            pcm_rx_flush(); // workaround for the first false received message
                        }
                    }
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Navigate in the main buffer after the message length byte and start sending those bytes
                    for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++)
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
                        while ((pcm_rx_available() <= (i - pcm.msg_buffer_ptr - 1)) && !timeout_reached)
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
            }
            else if (pcm.speed == HIBAUD) // high speed mode (7812.5 baud), full-duplex mode approach
            {
                uint8_t echo_retry_counter = 0;
                
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[0])) // make sure that the memory table select byte is approved by the PCM before sending the full message
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
                        
                        for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                        {
                            pcm_again_01:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
                            while ((pcm_rx_available() <= i) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { pcm.msg_buffer[0], pcm.msg_buffer[i] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (pcm_peek(0) != pcm.msg_buffer[0]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                pcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto pcm_again_01;
                                else
                                {
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, pcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, pcm.msg_buffer, 1); // send error packet back to the laptop
                    }
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Navigate in the main buffer after the message length byte and start sending those bytes
                    if (array_contains(sci_hi_speed_memarea, 16, pcm.msg_buffer[pcm.msg_buffer_ptr + 1])) // make sure that the memory table select byte is approved by the PCM before sending the full message
                    {
                        uint8_t j = 0;
                        
                        for (uint8_t i = (pcm.msg_buffer_ptr + 1); i < (pcm.msg_buffer_ptr + 1 + pcm.repeated_msg_length); i++) // repeat for the length of the message
                        {
                            pcm_again_02:
                            timeout_reached = false;
                            timeout_start = millis(); // save current time
                            pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
                           
                            while ((pcm_rx_available() <= j) && !timeout_reached)
                            {
                                // wait here for response (echo in case of F0...FF)
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { pcm.msg_buffer[pcm.msg_buffer_ptr + 1], pcm.msg_buffer[pcm.msg_buffer_ptr + 1 + j] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (pcm_peek(0) != pcm.msg_buffer[pcm.msg_buffer_ptr + 1]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                pcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto pcm_again_02;
                                else
                                {
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, pcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                            
                            j++;
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, pcm.msg_buffer, 1); // send error packet back to the laptop
                    }
                }
            }
            else // non-standard speeds
            {
                if (pcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    for (uint8_t i = 0; i < pcm.msg_buffer_ptr; i++) // repeat for the length of the message
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        pcm_putc(pcm.msg_buffer[i]); // put the next byte in the transmit buffer
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
                }
                else if (pcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
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
                            if ((millis() - timeout_start) > SCI_LS_T1_DELAY) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            timeout_reached = false;
                            uint8_t ret[2] = { pcm.msg_buffer[pcm.msg_buffer_ptr + 1], pcm.msg_buffer[pcm.msg_buffer_ptr + 1 + j] };
                            send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                            break;
                        }
                        
                        j++;
                    }
                }
            }
            
            pcm.msg_tx_pending = false; // re-arm, make it possible to send a message again
            pcm.msg_tx_count++;
        }
    }
    
    if (tcm.enabled)
    {
        // Handle completed messages
        if (tcm.speed == LOBAUD) // handle low-speed mode first (7812.5 baud)
        {
            if (((millis() - tcm.last_byte_millis) > SCI_LS_T3_DELAY) && (tcm_rx_available() > 0))
            { 
                tcm.message_length = tcm_rx_available();
                uint8_t usb_msg[TIMESTAMP_LENGTH+tcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp
                
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < tcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    usb_msg[TIMESTAMP_LENGTH+i] = tcm_getc() & 0xFF;
                }

                send_usb_packet(from_tcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+tcm.message_length); // send message to laptop
                handle_lcd(from_tcm, usb_msg, 4, TIMESTAMP_LENGTH+tcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                
                if (usb_msg[4] == 0x12) // pay attention to special bytes (speed change)
                {
                    sbi(tcm.bus_settings, 5); // set change settings bit
                    sbi(tcm.bus_settings, 4); // set enable bit
                    sbi(tcm.bus_settings, 1); // set/clear speed bits (62500 baud)
                    cbi(tcm.bus_settings, 0); // set/clear speed bits (62500 baud)
                    configure_sci_bus(tcm.bus_settings);
                }

                if (tcm.repeat && !tcm.repeat_iterate) // prepare next repeated message
                {
                    if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                    {
                        bool match = true;
                        for (uint8_t i = 0; i < tcm.repeated_msg_length; i++)
                        {
                            if (tcm.msg_buffer[i] != usb_msg[TIMESTAMP_LENGTH+i]) match = false; // compare received bytes with message sent
                        }
                        if (match) tcm.repeat_next = true; // if echo is correct prepare next message
                    }
                    else if (tcm.msg_to_transmit_count > 1) // multiple messages
                    {
                        bool match = true;
                        for (uint8_t i = 0; i < tcm.repeated_msg_length; i++)
                        {
                            if (tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + i] != usb_msg[TIMESTAMP_LENGTH+i]) match = false; // compare received bytes with message sent
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
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;
                    }
                }
                else if (tcm.repeat && tcm.repeat_iterate)
                {
                    if (tcm.message_length == (tcm.repeated_msg_length + 1)) // received message has to be 1 byte bigger than what was sent
                    {
                        tcm.repeat_next = true;
                        tcm.repeat_retry_counter = 0;
                    }
                    else
                    {
                        tcm.msg_tx_pending = true; // send the same message again if no answer is received
                        tcm.repeat_retry_counter++;
                        if (tcm.repeat_retry_counter > 10)
                        {
                            //tcm.repeat = false; // don't repeat after 10 failed attempts
                            //tcm.repeat_stop = true;
                            tcm.repeat_next = true; // give up this parameter and jump to the next one
                            tcm.repeat_retry_counter = 0;
                        }
                        delay(200);
                    }

                    if (tcm.repeat_stop)
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeated_msg_length = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;

                        uint8_t ret[2] = { 0x00, to_tcm }; // improvised payload array with only 1 element which is the target bus
                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                    }
                }
                
                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }
        else if (tcm.speed == HIBAUD) // handle high-speed mode (62500 baud), no need to wait for message completion here, it is already handled when the message was sent
        {
            if (tcm_rx_available() > 0)
            {
                tcm.message_length = tcm_rx_available();
                uint16_t packet_length = TIMESTAMP_LENGTH + (2*tcm.message_length) - 1;
                uint8_t usb_msg[packet_length]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp

                // Request and response bytes are mixed together in a single message:
                // 00 00 00 00 F4 0A 00 0B 00 0C 00 0D 00...
                // 00 00 00 00: timestamp
                // F4: RAM table
                // 0A: RAM address
                // 00: RAM value at 0A
                // 0B: RAM address
                // 00: RAM value at 0B

                for (uint8_t i = 0; i < TIMESTAMP_LENGTH; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }

                if (tcm.msg_to_transmit_count == 1)
                {
                    usb_msg[4] = tcm.msg_buffer[0]; // put RAM table byte first
                    tcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < tcm.msg_buffer_ptr; i++) 
                    {
                        usb_msg[5+(i*2)] = tcm.msg_buffer[i+1]; // put original request message byte next
                        usb_msg[5+(i*2)+1] = tcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(from_tcm, to_usb, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }
                else if (tcm.msg_to_transmit_count > 1)
                {
                    usb_msg[4] = tcm.msg_buffer[tcm.msg_buffer_ptr + 1]; // put RAM table byte first
                    tcm_getc(); // get rid of the first byte in the receive buffer, it's the RAM table byte
                    
                    for (uint8_t i = 0; i < tcm.repeated_msg_length; i++) 
                    {
                        usb_msg[5+(i*2)] = tcm.msg_buffer[tcm.msg_buffer_ptr+i+2]; // put original request message byte next
                        usb_msg[5+(i*2)+1] = tcm_getc() & 0xFF; // put response byte after the request byte
                    }
                    
                    send_usb_packet(from_tcm, to_usb, msg_rx, sci_hs_bytes, usb_msg, packet_length);
                }

                if (usb_msg[4] == 0xFE) // pay attention to special bytes (speed change)
                {
                    sbi(tcm.bus_settings, 5); // set change settings bit
                    sbi(tcm.bus_settings, 4); // set enable bit
                    cbi(tcm.bus_settings, 1); // set/clear speed bits (7812.5 baud)
                    sbi(tcm.bus_settings, 0); // set/clear speed bits (7812.5 baud)
                    configure_sci_bus(tcm.bus_settings);
                }

                if (tcm.repeat && !tcm.repeat_iterate)
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
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;
                    }
                }
                else if (tcm.repeat && tcm.repeat_iterate)
                {
                    // TODO
                    if (true) // check proper echo
                    {
                        tcm.repeat_next = true; // accept echo without verification...
                    }
                    else
                    {
                        tcm.msg_tx_pending = true; // send the same message again
                    }
                    
                    if (tcm.repeat_stop)
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeated_msg_length = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;
                    }
                }

                handle_lcd(from_tcm, usb_msg, 4, TIMESTAMP_LENGTH+tcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }
        else // other non-standard speeds are handled here
        {
            if (((millis() - tcm.last_byte_millis) > SCI_LS_T3_DELAY) && (tcm_rx_available() > 0))
            { 
                tcm.message_length = tcm_rx_available();
                uint8_t usb_msg[TIMESTAMP_LENGTH+tcm.message_length]; // create local array which will hold the timestamp and the SCI-bus (TCM) message
                update_timestamp(current_timestamp); // get current time for the timestamp
                
                for (uint8_t i = 0; i < 4; i++) // put 4 timestamp bytes in the front
                {
                    usb_msg[i] = current_timestamp[i];
                }
                for (uint8_t i = 0; i < tcm.message_length; i++) // put every byte in the SCI-bus message after the timestamp
                {
                    usb_msg[TIMESTAMP_LENGTH+i] = tcm_getc() & 0xFF;
                }

                send_usb_packet(from_tcm, to_usb, msg_rx, sci_ls_bytes, usb_msg, TIMESTAMP_LENGTH+tcm.message_length); // send message to laptop

                // No automatic speed change for 976.5 and 125000 baud!

                if (tcm.repeat && !tcm.repeat_iterate)
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
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;
                    }
                }
                else if (tcm.repeat && tcm.repeat_iterate)
                {
                    // TODO
                    if (true) // check proper echo
                    {
                        tcm.repeat_next = true; // accept echo without verification...
                    }
                    else
                    {
                        tcm.msg_tx_pending = true; // send the same message again
                    }
                    
                    if (tcm.repeat_stop)
                    {
                        tcm.msg_buffer_ptr = 0;
                        tcm.repeated_msg_length = 0;
                        tcm.repeat = false;
                        tcm.repeat_next = false;
                        tcm.repeat_iterate = false;
                        tcm.repeat_list_once = false;
                    }
                }
                
                handle_lcd(from_tcm, usb_msg, 4, TIMESTAMP_LENGTH+tcm.message_length); // pass message to LCD handling function, start at the 4th byte (skip timestamp)
                tcm_rx_flush();
                tcm.msg_rx_count++;
            }
        }

        // Repeated messages are prepared here for transmission
        if (tcm.repeat && (tcm_rx_available() == 0))
        {
            current_millis = millis(); // check current time
            if ((current_millis - tcm.repeated_msg_last_millis) > tcm.repeated_msg_interval) // wait between messages
            {
                tcm.repeated_msg_last_millis = current_millis;
                
                if (tcm.repeat_next && !tcm.repeat_iterate) // no iteration, same message over and over again
                {
                    // The message is already in the tcm.msg_buffer array, just set flags
                    tcm.msg_tx_pending = true; // set flag
                    tcm.repeat_next = false;
                }
                else if (tcm.repeat_next && tcm.repeat_iterate) // iteration, message is incremented for every repeat according to settings
                {
                    if (tcm.repeated_msg_length == 0x04) // 4-bytes length
                    {
                        if (tcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            tcm.msg_buffer[0] = (tcm.repeated_msg_raw_start >> 24) & 0xFF; // decompose raw message from its integer form to byte components
                            tcm.msg_buffer[1] = (tcm.repeated_msg_raw_start >> 16) & 0xFF;
                            tcm.msg_buffer[2] = (tcm.repeated_msg_raw_start >> 8) & 0xFF;
                            tcm.msg_buffer[3] = tcm.repeated_msg_raw_start & 0xFF;
                            tcm.msg_buffer_ptr = 4;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint32_t message = to_uint32(tcm.msg_buffer[0], tcm.msg_buffer[1], tcm.msg_buffer[2], tcm.msg_buffer[3]);
                            message += tcm.repeated_msg_increment; // add increment
                            tcm.msg_buffer[0] = (message >> 24) & 0xFF; // decompose integer into byte compontents again
                            tcm.msg_buffer[1] = (message >> 16) & 0xFF;
                            tcm.msg_buffer[2] = (message >> 8) & 0xFF;
                            tcm.msg_buffer[3] = message & 0xFF;
                            tcm.msg_buffer_ptr = 4;
                            
                            if ((message + tcm.repeated_msg_increment) > tcm.repeated_msg_raw_end) tcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (tcm.repeated_msg_length == 0x03) // 3-bytes length
                    {
                        if (tcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            tcm.msg_buffer[0] = (tcm.repeated_msg_raw_start >> 16) & 0xFF; // decompose raw message from its integer form to byte components
                            tcm.msg_buffer[1] = (tcm.repeated_msg_raw_start >> 8) & 0xFF;
                            tcm.msg_buffer[2] = tcm.repeated_msg_raw_start & 0xFF;
                            tcm.msg_buffer_ptr = 3;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint32_t message = to_uint32(0, tcm.msg_buffer[0], tcm.msg_buffer[1], tcm.msg_buffer[2]);
                            message += tcm.repeated_msg_increment; // add increment
                            tcm.msg_buffer[0] = (message >> 16) & 0xFF; // decompose integer into byte compontents again
                            tcm.msg_buffer[1] = (message >> 8) & 0xFF;
                            tcm.msg_buffer[2] = message & 0xFF;
                            tcm.msg_buffer_ptr = 3;
                            
                            if ((message + tcm.repeated_msg_increment) > tcm.repeated_msg_raw_end) tcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (tcm.repeated_msg_length == 0x02) // 2-bytes length
                    {
                        if (tcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            tcm.msg_buffer[0] = (tcm.repeated_msg_raw_start >> 8) & 0xFF; // decompose raw message from its integer form to byte components
                            tcm.msg_buffer[1] = tcm.repeated_msg_raw_start & 0xFF;
                            tcm.msg_buffer_ptr = 2;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            uint16_t message = to_uint16(tcm.msg_buffer[0], tcm.msg_buffer[1]);
                            message += tcm.repeated_msg_increment; // add increment
                            tcm.msg_buffer[0] = (message >> 8) & 0xFF; // decompose integer into byte compontents again
                            tcm.msg_buffer[1] = message & 0xFF;
                            tcm.msg_buffer_ptr = 2;
                            
                            if ((message + tcm.repeated_msg_increment) > tcm.repeated_msg_raw_end) tcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }
                    else if (tcm.repeated_msg_length == 0x01) // 1-byte length
                    {
                        if (tcm.msg_buffer_ptr == 0) // no existing message in the buffer yet, lets load the first one
                        {
                            tcm.msg_buffer[0] = tcm.repeated_msg_raw_start & 0xFF; // decompose raw message from its integer form to byte components
                            tcm.msg_buffer_ptr = 1;
                        }
                        else // increment existing message
                        {
                            // First combine bytes into a single integer
                            tcm.msg_buffer[0] += tcm.repeated_msg_increment; // add increment
                            tcm.msg_buffer_ptr = 1;
                            
                            if ((tcm.msg_buffer[0] + tcm.repeated_msg_increment) > tcm.repeated_msg_raw_end) tcm.repeat_stop = true; // don't prepare another message, it's the end
                        }
                    }

                    tcm.msg_tx_pending = true; // set flag
                    tcm.repeat_next = false;
                }
            }
        }

        // Send message
        if (tcm.msg_tx_pending && (tcm_rx_available() == 0))
        {
            if (tcm.speed == LOBAUD) // low speed mode (7812.5 baud), half-duplex mode approach
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
                else if (tcm.msg_to_transmit_count > 1) // multiple messages, send one at a time
                {
                    // Navigate in the main buffer after the message length byte and start sending those bytes
                    for (uint8_t i = (tcm.msg_buffer_ptr + 1); i < (tcm.msg_buffer_ptr + 1 + tcm.repeated_msg_length); i++)
                    {
                        timeout_reached = false;
                        timeout_start = millis(); // save current time
                        tcm_putc(tcm.msg_buffer[i]); // put the next byte in the transmit buffer
                        while ((tcm_rx_available() <= (i - tcm.msg_buffer_ptr - 1)) && !timeout_reached)
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
            }
            else if (tcm.speed == HIBAUD) // high speed mode (7812.5 baud), full-duplex mode approach
            {
                uint8_t echo_retry_counter = 0;
                
                if (tcm.msg_to_transmit_count == 1) // if there's only one message in the buffer
                {
                    if (array_contains(sci_hi_speed_memarea, 16, tcm.msg_buffer[0])) // make sure that the memory table select byte is approved by the TCM before sending the full message
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
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { tcm.msg_buffer[0], tcm.msg_buffer[i] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (tcm_peek(0) != tcm.msg_buffer[0]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                tcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto tcm_again_01;
                                else
                                {
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, tcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, tcm.msg_buffer, 1); // send error packet back to the laptop
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
                                if ((millis() - timeout_start) > SCI_HS_T3_DELAY) timeout_reached = true;
                            }
                            if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                            {
                                timeout_reached = false;
                                uint8_t ret[2] = { tcm.msg_buffer[tcm.msg_buffer_ptr + 1], tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + j] };
                                send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                                break;
                            }
                            if (tcm_peek(0) != tcm.msg_buffer[tcm.msg_buffer_ptr + 1]) // make sure the first RAM-table byte is echoed back correctly
                            {
                                tcm_rx_flush();
                                echo_retry_counter++;
                                if (echo_retry_counter < 10) goto tcm_again_02;
                                else
                                {
                                    send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_memory_ptr_no_response, tcm.msg_buffer, 1); // send error packet back to the laptop
                                    break;
                                }
                            }
                            
                            j++;
                        }
                    }
                    else
                    {
                        // Messsage doesn't start with a RAM-table value, invalid
                        send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_invalid_memory_ptr, tcm.msg_buffer, 1); // send error packet back to the laptop
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
                            if ((millis() - timeout_start) > SCI_LS_T1_DELAY) timeout_reached = true;
                        }
                        if (timeout_reached) // exit for-loop if there's no answer for a long period of time, no need to waste time for other bytes (if any), watchdog timer is ticking...
                        {
                            timeout_reached = false;
                            uint8_t ret[2] = { tcm.msg_buffer[tcm.msg_buffer_ptr + 1], tcm.msg_buffer[tcm.msg_buffer_ptr + 1 + j] };
                            send_usb_packet(from_usb, to_usb, ok_error, error_sci_hs_no_response, ret, 2); // return two bytes to determine which table and which address is unresponsive
                            break;
                        }
                        
                        j++;
                    }
                }
            }
            
            tcm.msg_tx_pending = false; // re-arm, make it possible to send a message again
            tcm.msg_tx_count++;
        }
    }       
}

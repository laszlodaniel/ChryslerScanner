#include "usb.h"
#include "common.h"
#include "tools.h"
#include "battery.h"
#include "memory.h"
#include "lcd.h"
#include "led.h"
#include "ccd.h"
#include "sci.h"
#include <util/atomic.h>

volatile uint8_t  USB_RxBuf[USB_RX0_BUFFER_SIZE];
volatile uint8_t  USB_TxBuf[USB_TX0_BUFFER_SIZE];
volatile uint16_t USB_RxHead; // since buffer size is bigger than 256 bytes it has to be a 16-bit (int) variable
volatile uint16_t USB_RxTail;
volatile uint16_t USB_TxHead;
volatile uint16_t USB_TxTail;
volatile uint8_t  USB_LastRxError;

uint8_t command_timeout = 100; // milliseconds
uint16_t command_purge_timeout = 200; // milliseconds, if a command isn't complete within this time then delete the usb receive buffer

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
    tmptail = (USB_RxTail + 1) & USB_RX0_BUFFER_MASK;
  
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
  
    tmptail = (USB_RxTail + 1 + index) & USB_RX0_BUFFER_MASK;

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
        ret = (USB_RX0_BUFFER_SIZE + USB_RxHead - USB_RxTail) & USB_RX0_BUFFER_MASK;
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
        ret = (USB_TX0_BUFFER_SIZE + USB_TxHead - USB_TxTail) & USB_TX0_BUFFER_MASK;
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
    bool payload_bytes = false;
    uint8_t calculated_checksum = 0;
    uint8_t datacode = 0;

    // Check if there's enough RAM to store the whole packet
    if (free_ram() < (packet_length + 50)) // require +50 free bytes to be safe
    {
        uint8_t error[7] = { 0x3D, 0x00, 0x03, ok_error, error_not_enough_mcu_ram, 0xFF, 0x08 }; // prepare the "not enough MCU RAM" error message
        for (uint16_t i = 0; i < 7; i++)
        {
            usb_putc(error[i]);
        }
        return;
    }

    uint8_t packet[packet_length]; // create a temporary byte-array

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
                    send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occurred, err, 1);
                    return;
                }

                length_hb = usb_getc() & 0xFF; // read first length byte
                length_lb = usb_getc() & 0xFF; // read second length byte
        
                // Calculate how much more bytes should we read by combining the two length bytes into a word.
                bytes_to_read = to_uint16(length_hb, length_lb) + 1; // +1 CHECKSUM byte
                
                // Calculate the exact size of the payload.
                payload_length = bytes_to_read - 3; // in this case we have to be careful not to count data code byte, sub-data code byte and checksum byte
        
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
                    send_usb_packet(from_usb, to_usb, ok_error, error_packet_timeout_occurred, err, 1);
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
                    send_usb_packet(from_usb, to_usb, ok_error, error_packet_checksum_invalid_value, err, 1);
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
                                switch (subdatacode)
                                {
                                    case 0x00:
                                    {
                                        cmd_handshake();
                                        break;
                                    }
                                    case 0x01:
                                    {
                                        cmd_handshake();
                                        send_hwfw_info();
                                        cmd_status();
                                        break;
                                    }
                                    default:
                                    {
                                        send_usb_packet(from_usb, to_usb, ok_error, error_subdatacode_invalid_value, err, 1); // send error packet back to the laptop  
                                        break;
                                    }
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
                                    case heartbeat: // 0x01 - ACT_LED flashing interval is stored in payload as 4 bytes
                                    {
                                        if (!payload_bytes || (payload_length < 4)) // at least 4 bytes are necessary to change this setting
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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

                                        DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                        PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection

                                        eep.write(0x1F, cmd_payload[0]); // write LED heartbeat interval high byte
                                        eep.write(0x20, cmd_payload[1]); // write LED heartbeat interval low byte
                                        eep.write(0x21, cmd_payload[2]); // write LED blink duration high byte
                                        eep.write(0x22, cmd_payload[3]); // write LED blink duration low byte

                                        uint8_t temp_checksum = 0; // re-calculate checksum

                                        for (uint8_t i = 0; i < 255; i++) // add the first 255 bytes together
                                        {
                                            temp_checksum += eep.read(i);
                                        }
                                            
                                        eep.write(255, temp_checksum); // write checksum byte at the last position of the settings block

                                        PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection

                                        eep_checksum[0] = temp_checksum;
                                        eep_calculated_checksum = temp_checksum;
                                        eep_checksum_ok = true;

                                        uint8_t ret[5] = { 0x00, cmd_payload[0], cmd_payload[1], cmd_payload[2], cmd_payload[3] };
                                        send_usb_packet(from_usb, to_usb, settings, heartbeat, ret, 5); // acknowledge
                                        break;
                                    }
                                    case set_ccd_bus: // 0x02 - bus state and termination/bias setting
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        switch (cmd_payload[0])
                                        {
                                            case 0x00: // disable ccd-bus transceiver
                                            {
                                                ccd.enabled = false;
                                                cbi(ccd.bus_settings, 4); // clear enabled bit
                                                //ccd_clock_generator(STOP);
                                                //ccd_rx_flush();
                                                //ccd_tx_flush();
                                                break;
                                            }
                                            case 0x01: // enable ccd-bus transceiver
                                            {
                                                ccd.enabled = true;
                                                sbi(ccd.bus_settings, 4); // set enabled bit
                                                //ccd_clock_generator(START);
                                                break;
                                            }
                                            default: // other values are not valid
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                                break;
                                            }
                                        }

                                        switch (cmd_payload[1])
                                        {
                                            case 0x00:
                                            {
                                                ccd.termination_bias_enabled = false;
                                                cbi(ccd.bus_settings, 2); // clear TB bit
                                                digitalWrite(TBEN, HIGH);
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                ccd.termination_bias_enabled = true;
                                                sbi(ccd.bus_settings, 2); // set TB bit
                                                digitalWrite(TBEN, LOW);
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                                break;
                                            }
                                        }

                                        uint8_t ret[3] = { 0x00, cmd_payload[0], cmd_payload[1] };
                                        send_usb_packet(from_usb, to_usb, settings, set_ccd_bus, ret, 3); // acknowledge
                                        break;
                                    }
                                    case set_sci_bus: // 0x03 - ON-OFF state, A/B configuration and speed are stored in payload
                                    {
                                        if (!payload_bytes) // at least 1 byte is necessary to change this setting
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        configure_sci_bus(cmd_payload[0]); // pass settings to this function, it handles both PCM and TCM but only one at a time!
                                        break;
                                    }
                                    case set_repeat_behavior: // 0x04
                                    {
                                        if (!payload_bytes || (payload_length < 5))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        switch (cmd_payload[0])
                                        {
                                            case 0x01: // CCD-bus
                                            {
                                                ccd.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                ccd.repeated_msg_increment = to_uint16(cmd_payload[3], cmd_payload[4]); // 0-65535
                                                break;
                                            }
                                            case 0x02: // SCI-bus (PCM)
                                            {
                                                pcm.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                pcm.repeated_msg_increment = to_uint16(cmd_payload[3], cmd_payload[4]); // 0-65535
                                                break;
                                            }
                                            case 0x03: // SCI-bus (TCM)
                                            {
                                                tcm.repeated_msg_interval = to_uint16(cmd_payload[1], cmd_payload[2]); // 0-65535 milliseconds
                                                tcm.repeated_msg_increment = to_uint16(cmd_payload[3], cmd_payload[4]); // 0-65535
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, already enabled
                                                break;
                                            }
                                        }

                                        uint8_t ret[6] = { 0x00, cmd_payload[0], cmd_payload[1], cmd_payload[2], cmd_payload[3], cmd_payload[4] };
                                        send_usb_packet(from_usb, to_usb, settings, set_repeat_behavior, ret, 6); // acknowledge
                                        break;
                                    }
                                    case set_lcd: // 0x05 - LCD settings
                                    {
                                        if (!payload_bytes || (payload_length < 7))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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

                                        DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                        PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection

                                        eep.write(0x18, cmd_payload[0]); // write LCD enabled/disabled state
                                        eep.write(0x19, lcd_i2c_address); // write LCD I2C address
                                        eep.write(0x1A, lcd_char_width); // write LCD width
                                        eep.write(0x1B, lcd_char_height); // write LCD height
                                        eep.write(0x1C, lcd_refresh_rate); // write LCD refresh rate
                                        eep.write(0x1D, lcd_units); // write LCD units
                                        eep.write(0x1E, lcd_data_source); // write LCD units

                                        uint8_t temp_checksum = 0; // re-calculate checksum

                                        for (uint8_t i = 0; i < 255; i++) // add the first 255 bytes together
                                        {
                                            temp_checksum += eep.read(i);
                                        }
                                            
                                        eep.write(255, temp_checksum); // write checksum byte at the last position of the settings block

                                        PORTE |= (1 << PE2); // pull PE2 pin high to enable write protection

                                        eep_checksum[0] = temp_checksum;
                                        eep_calculated_checksum = temp_checksum;
                                        eep_checksum_ok = true;

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
                                        }
                                        
                                        uint8_t ret[8] = { 0x00, cmd_payload[0], lcd_i2c_address, lcd_char_width, lcd_char_height, lcd_refresh_rate, lcd_units, lcd_data_source };
                                        send_usb_packet(from_usb, to_usb, settings, set_lcd, ret, 8); // acknowledge
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
                                    case hwfw_info: // 0x01 - hardware version/date and firmware date in this particular order (dates are in 64-bit UNIX time format)
                                    {
                                        send_hwfw_info();
                                        break;
                                    }
                                    case timestamp: // 0x02 - timestamp / MCU counter value (milliseconds elapsed)
                                    {
                                        update_timestamp(current_timestamp); // this function updates the global byte array "current_timestamp" with the current time
                                        send_usb_packet(from_usb, to_usb, response, timestamp, current_timestamp, 4);
                                        break;
                                    }
                                    case battery_voltage: // 0x03
                                    {
                                        check_battery_volts();
                                        send_usb_packet(from_usb, to_usb, response, battery_voltage, battery_volts_array, 2);
                                        break;
                                    }
                                    case exteeprom_checksum: // 0x04
                                    {
                                        evaluate_eep_checksum();
                                        break;
                                    }
                                    case ccd_bus_voltages: // 0x05
                                    {
                                        check_ccd_volts();
                                        send_usb_packet(from_usb, to_usb, response, ccd_bus_voltages, ccd_volts_array, 4);
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
                                    case random_ccd_msg: // 0x01 - broadcast random CCD-bus messages
                                    {
                                        if (!payload_bytes || (payload_length < 5))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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

                                                uint8_t ret[2] = { 0x00, cmd_payload[0] };
                                                send_usb_packet(from_usb, to_usb, debug, random_ccd_msg, ret, 2); // acknowledge
                                                break;
                                            }
                                            case 0x01:
                                            {
                                                ccd.random_msg = true;
                                                ccd.msg_to_transmit_count = 1;
                                                ccd.random_msg_interval_min = to_uint16(cmd_payload[1], cmd_payload[2]);
                                                ccd.random_msg_interval_max = to_uint16(cmd_payload[3], cmd_payload[4]);
                                                ccd.random_msg_interval = random(ccd.random_msg_interval_min, ccd.random_msg_interval_max);

                                                uint8_t ret[2] = { 0x00, cmd_payload[0] };
                                                send_usb_packet(from_usb, to_usb, debug, random_ccd_msg, ret, 2); // acknowledge
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
                                    case read_inteeprom_byte: // 0x02 - read internal EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }

                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);

                                        if (offset > 4095)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }
                                        
                                        uint8_t value = eeprom_read_byte((const uint8_t*)offset);
                                        uint8_t payload[4] = { 0x00, offset_hb, offset_lb, value };
                                        send_usb_packet(from_usb, to_usb, debug, read_inteeprom_byte, payload, 4); // send external EEPROM value back to the laptop
                                        break;
                                    }
                                    case read_inteeprom_block: // 0x03 - read internal EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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
                                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
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

                                        send_usb_packet(from_usb, to_usb, debug, read_inteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                        break;
                                    }
                                    case read_exteeprom_byte: // 0x04 - read external EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 2))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);

                                            if (offset > 4095)
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }
                                            
                                            uint8_t value = eep.read(offset);
                                            uint8_t payload[4] = { 0x00, offset_hb, offset_lb, value };
                                            send_usb_packet(from_usb, to_usb, debug, read_exteeprom_byte, payload, 4); // send external EEPROM value back to the laptop
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case read_exteeprom_block: // 0x05 - read external EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }

                                            uint8_t values[count];
                                            uint8_t success = eep.read(offset, values, count);

                                            if (success != 0)
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1);
                                                break;
                                            }

                                            uint16_t p_length = count + 3;
                                            uint8_t payload[payload_length] = { 0x00, offset_hb, offset_lb };
                                            
                                            for (uint16_t i = 0; i < count; i++)
                                            {
                                                payload[3 + i] = values[i];
                                            }

                                            send_usb_packet(from_usb, to_usb, debug, read_exteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case write_inteeprom_byte: // 0x06 - write internal EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 3))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);

                                        if (offset > 4095)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
                                            break;
                                        }
                                        
                                        uint8_t value = cmd_payload[2];
                                        eeprom_update_byte((uint8_t *)offset, value);

                                        uint8_t reading = eeprom_read_byte((const uint8_t*)offset);
                                        uint8_t payload[4] = { 0x00, offset_hb, offset_lb, reading };
                                        send_usb_packet(from_usb, to_usb, debug, write_inteeprom_byte, payload, 4); // send external EEPROM value back to the laptop for confirmation
                                        break;
                                    }
                                    case write_inteeprom_block: // 0x07 - write internal EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        uint8_t offset_hb = cmd_payload[0];
                                        uint8_t offset_lb = cmd_payload[1];
                                        uint16_t offset = to_uint16(offset_hb, offset_lb);
                                        uint16_t count = payload_length - 2;

                                        if ((offset + (count - 1)) > 4095)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_internal, err, 1);
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

                                        send_usb_packet(from_usb, to_usb, debug, write_inteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                        break;
                                    }
                                    case write_exteeprom_byte: // 0x08 - write external EEPROM byte
                                    {
                                        if (!payload_bytes || (payload_length < 3))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
                                            break;
                                        }
                                        
                                        if (eep_present)
                                        {
                                            uint8_t offset_hb = cmd_payload[0];
                                            uint8_t offset_lb = cmd_payload[1];
                                            uint16_t offset = to_uint16(offset_hb, offset_lb);

                                            if (offset > 4095)
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1);
                                                break;
                                            }
                                            
                                            uint8_t value = cmd_payload[2];

                                            // Disable hardware write protection (EEPROM chip has a pullup resistor on its WP-pin!)
                                            DDRE |= (1 << PE2); // set PE2 pin as output (this pin can't be reached by pinMode and digitalWrite commands)
                                            PORTE &= ~(1 << PE2); // pull PE2 pin low to disable write protection

                                            eep_result = eep.write(offset, value);

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                            }
                                            else // ok, calculate checksum again
                                            {
                                                uint8_t temp_checksum = 0;

                                                for (uint8_t i = 0; i < 255; i++) // add the first 255 bytes together and skip last byte (where the result of this calculation goes) by setting the second parameter to 255 instead of 256
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

                                            uint8_t reading = eep.read(offset);
                                            uint8_t payload[4] = { 0x00, offset_hb, offset_lb, reading };
                                            send_usb_packet(from_usb, to_usb, debug, write_exteeprom_byte, payload, 4); // send external EEPROM value back to the laptop for confirmation
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
                                        }
                                        break;
                                    }
                                    case write_exteeprom_block: // 0x0A - write external EEPROM block
                                    {
                                        if (!payload_bytes || (payload_length < 4))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // send error packet back to the laptop
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
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1);
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
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_write, err, 1); // send error packet back to the laptop
                                            }
                                            else // ok, calculate checksum again
                                            {
                                                uint8_t temp_checksum = 0;

                                                for (uint8_t i = 0; i < 255; i++) // add the first 255 bytes together and skip last byte (where the result of this calculation goes) by setting the second parameter to 255 instead of 256
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
                                            
                                            eep_result = eep.read(offset, values, count); // overwrite array with read values;

                                            if (eep_result) // error
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_eep_read, err, 1); // send error packet back to the laptop
                                            }
                                            else
                                            {
                                                uint16_t p_length = count + 3;
                                                uint8_t payload[p_length] = { 0x00, offset_hb, offset_lb };
        
                                                for (uint16_t i = 0; i < count; i++)
                                                {
                                                    payload[3 + i] = values[i];
                                                }
        
                                                send_usb_packet(from_usb, to_usb, debug, write_exteeprom_block, payload, p_length); // send external EEPROM block back to the laptop
                                            }
                                        }
                                        else
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_eep_not_found, err, 1);
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
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        ccd.repeat = false;
                                        ccd.repeat_next = false;
                                        ccd.repeat_iterate = false;
                                        ccd.repeat_list_once = false;
                                        ccd.repeat_stop = true;
                                        ccd.msg_to_transmit_count = 0;
                                        ccd.msg_to_transmit_count_ptr = 0;
                                        ccd.repeated_msg_length = 0;
                                        ccd.repeated_msg_last_millis = 0;
                                        ccd.msg_buffer_ptr = 0;

                                        uint8_t ret[2] = { 0x00, to_ccd }; // improvised payload array with only 1 element which is the target bus
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the CCD-bus once
                                    {
                                        if (!payload_bytes || (payload_length > CCD_RX1_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            ccd.msg_buffer[i] = cmd_payload[i];
                                        }

                                        ccd.msg_buffer_ptr = payload_length;
                                        
                                        // Checksum is only applicable if message is at least 2 bytes long
                                        if (ccd.msg_buffer_ptr > 1)
                                        {
                                            uint8_t checksum_position = ccd.msg_buffer_ptr - 1;
                                            ccd.msg_buffer[checksum_position] = calculate_checksum(ccd.msg_buffer, 0, checksum_position); // overwrite last checksum byte with the correct one
                                        }

                                        ccd.msg_buffer_ptr = payload_length;
                                        ccd.msg_to_transmit_count = 1;
                                        ccd.msg_tx_pending  = true; // set flag so the main loop knows there's something to do

                                        uint8_t ret[2] = { 0x00, to_ccd }; // improvised payload array with only 1 element which is the target bus
                                        send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 2);
                                        break;
                                    }
                                    case repeated_single_msg: // 0x04 - send a message to the CCD-bus repeatedly forever
                                    {
                                        if ((payload_length < 4) || (payload_length > CCD_RX1_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                        {
                                            case 0x00: // no iteration needed, send the same message(s) again and again
                                            {
                                                // Payload structure example:
                                                // 00 01 04 E4 00 00 E4
                                                // -----------------------------------
                                                // 00: no message iteration needed, send the same message(s) again ang again
                                                // 01: number of messages
                                                // 04: message length
                                                // E4 25 00 09: message

                                                for (uint8_t i = 3; i < payload_length; i++)
                                                {
                                                    ccd.msg_buffer[i-3] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                }

                                                ccd.msg_to_transmit_count = cmd_payload[1]; // message count
                                                ccd.repeated_msg_length = cmd_payload[2]; // message length
                                                ccd.msg_buffer_ptr = ccd.repeated_msg_length;

                                                // Checksum is only applicable if message is at least 2 bytes long
                                                if (ccd.repeated_msg_length > 1)
                                                {
                                                    uint8_t checksum_position = ccd.repeated_msg_length - 1;
                                                    ccd.msg_buffer[checksum_position] = calculate_checksum(ccd.msg_buffer, 0, checksum_position); // overwrite last checksum byte with the correct one
                                                }
                                                
                                                ccd.repeat = true; // set flag
                                                ccd.repeat_next = true; // set flag
                                                ccd.repeat_iterate = false; // set flag
                                                ccd.repeat_list_once = false;
                                                ccd.repeat_stop = false;

                                                uint8_t ret[2] = { 0x00, to_ccd };
                                                send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                break;
                                            }
                                            case 0x01: // message iteration needed, 1 message only (with B2 ID-byte)!
                                            {
                                                // Payload structure example:
                                                // 01 01 06 B2 20 22 00 00 F4 B2 20 22 FF FE XX
                                                // Note: iteration only works for B2/F2 messages and assumes a 16-bit address space at the 4th and 5th byte
                                                // -----------------------------------
                                                // 01: message iteration needed
                                                // 01: number of messages
                                                // 06: message length
                                                // B2 20 22 00 00 F4: first message
                                                // B2 20 22 FF FE XX: last message
                                                
                                                if (payload_length > 14)
                                                {
                                                    ccd.repeated_msg_length = cmd_payload[2]; // message length, fixed to 6 bytes
                                                    ccd.msg_buffer_ptr = 0;

                                                    // Skip the first ID-byte and the last checksum byte
                                                    if (ccd.repeated_msg_length == 0x06)
                                                    {
                                                        ccd.repeated_msg_raw_start =  to_uint32(cmd_payload[4], cmd_payload[5], cmd_payload[6], cmd_payload[7]);
                                                        ccd.repeated_msg_raw_end = to_uint32(cmd_payload[10], cmd_payload[11], cmd_payload[12], cmd_payload[13]);
                                                    }
                                                    else
                                                    {
                                                        send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, too long message
                                                        break;
                                                    }

                                                    // Checksum is calculated during preparation in the CCD-bus handler function!

                                                    ccd.repeat = true; // set flag
                                                    ccd.repeat_next = true; // set flag
                                                    ccd.repeat_iterate = true; // set flag
                                                    ccd.repeat_list_once = false;
                                                    ccd.repeat_stop = false;
                                                    ccd.msg_to_transmit_count = 1;

                                                    uint8_t ret[2] = { 0x00, to_ccd };
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                }
                                                else
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, too long message
                                                }
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the CCD-bus once
                                    case repeated_list_msg: // 0x05 - send a set of messages to the CCD-bus repeatedly forever
                                    {
                                        if (payload_length < 4)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 00 02 04 E4 00 00 E4 04 24 00 00 24
                                        // -----------------------------------
                                        // 00: message iteration not supported, reads always zero
                                        // 02: number of messages
                                        // 04: 1st message length
                                        // E4 00 00 E4: message #1
                                        // 04: 2nd message length
                                        // 24 00 00 24: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 2; i < payload_length; i++)
                                        {
                                            ccd.msg_buffer[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        ccd.msg_to_transmit_count = cmd_payload[1]; // save number of messages
                                        ccd.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        ccd.repeated_msg_length = cmd_payload[2]; // first message length is saved
                                        ccd.msg_buffer_ptr = 0;

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
                                        ccd.repeat_iterate = false;
                                        
                                        if (subdatacode == list_msg) ccd.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) ccd.repeat_list_once = false;
                                        
                                        ccd.repeat_stop = false;

                                        uint8_t ret[2] = { 0x00, to_ccd };
                                        if (subdatacode == list_msg) send_usb_packet(from_usb, to_usb, msg_tx, list_msg, ret, 2); // acknowledge
                                        else if (subdatacode == repeated_list_msg) send_usb_packet(from_usb, to_usb, msg_tx, repeated_list_msg, ret, 2); // acknowledge
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
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        pcm.repeat = false;
                                        pcm.repeat_next = false;
                                        pcm.repeat_iterate = false;
                                        pcm.repeat_list_once = false;
                                        pcm.repeat_stop = true;
                                        pcm.msg_to_transmit_count = 0;
                                        pcm.msg_to_transmit_count_ptr = 0;
                                        pcm.repeated_msg_length = 0;
                                        pcm.repeated_msg_last_millis = 0;
                                        pcm.msg_buffer_ptr = 0;
                                        
                                        uint8_t ret[2] = { 0x00, to_pcm };
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the SCI-bus once
                                    {
                                        if (!payload_bytes || (payload_length > PCM_RX2_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            pcm.msg_buffer[i] = cmd_payload[i];
                                        }

                                        pcm.msg_buffer_ptr = payload_length;
                                        pcm.msg_to_transmit_count = 1;
                                        pcm.msg_tx_pending  = true; // set flag so the main loop knows there's something to do

                                        uint8_t ret[2] = { 0x00, to_pcm };
                                        send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 2);
                                        break;
                                    }
                                    case repeated_single_msg: // 0x04 - send repeated message(s) to the SCI-bus (PCM)
                                    {
                                        if ((payload_length < 4) || (payload_length > PCM_RX2_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }
                                        
                                        switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                        {
                                            case 0x00: // no iteration needed, send the same message(s) again and again
                                            {
                                                // Payload structure example:
                                                // 00 01 06 F4 0A 0B 0C 0D 11
                                                // -----------------------------------
                                                // 00: no message iteration needed, send the same message(s) again ang again
                                                // 01: number of messages
                                                // 06: message length
                                                // F4 0A 0B 0C 0D 11: message

                                                for (uint16_t i = 3; i < payload_length; i++)
                                                {
                                                    pcm.msg_buffer[i-3] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                }

                                                pcm.msg_to_transmit_count = cmd_payload[1]; // message count
                                                pcm.repeated_msg_length = cmd_payload[2]; // message length
                                                pcm.msg_buffer_ptr = pcm.repeated_msg_length;
                                                pcm.repeat = true; // set flag
                                                pcm.repeat_next = true; // set flag
                                                pcm.repeat_iterate = false; // set flag
                                                pcm.repeat_list_once = false;
                                                pcm.repeat_stop = false;

                                                uint8_t ret[2] = { 0x00, to_pcm };
                                                send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                break;
                                            }
                                            case 0x01: // message iteration needed, 1 message only!
                                            {
                                                // Payload structure example:
                                                // 01 01 04 26 00 00 00 26 01 FF FF 
                                                // Note: iteration only works for messages of maximum 4 bytes length and it assumes one ID-byte and 1-2-3 address bytes
                                                // -----------------------------------
                                                // 01: message iteration needed, 1 message only!
                                                // 01: number of messages
                                                // 04: message length
                                                // 26 00 00 00: first message in the sequence
                                                // 26 01 FF FF: last message in the sequence

                                                if ((payload_length > 4) && (payload_length < 12)) // at least 5 bytes are necessary (min: 3 flag bytes + 1 start byte + 1 end byte = 5, max: 3 flag bytes + 4 start byte + 4 end byte = 11)
                                                {
                                                    pcm.msg_to_transmit_count = 1;
                                                    pcm.repeated_msg_length = cmd_payload[2]; // message length
                                                    pcm.msg_buffer_ptr = 0;
                                                    pcm.repeat = true; // set flag
                                                    pcm.repeat_next = true; // set flag
                                                    pcm.repeat_iterate = true; // set flag
                                                    pcm.repeat_list_once = false;
                                                    pcm.repeat_stop = false;

                                                    if (pcm.repeated_msg_length == 0x04) // 4-bytes length
                                                    {
                                                        pcm.repeated_msg_raw_start =  to_uint32(cmd_payload[3], cmd_payload[4], cmd_payload[5], cmd_payload[6]);
                                                        pcm.repeated_msg_raw_end = to_uint32(cmd_payload[7], cmd_payload[8], cmd_payload[9], cmd_payload[10]);
                                                    }
                                                    else if (pcm.repeated_msg_length == 0x03) // 3-bytes length
                                                    {
                                                        pcm.repeated_msg_raw_start = to_uint32(0, cmd_payload[3], cmd_payload[4], cmd_payload[5]);
                                                        pcm.repeated_msg_raw_end = to_uint32(0, cmd_payload[6], cmd_payload[7], cmd_payload[8]);
                                                    }
                                                    else if (pcm.repeated_msg_length == 0x02) // 2-bytes length
                                                    {
                                                        pcm.repeated_msg_raw_start = to_uint16(cmd_payload[3], cmd_payload[4]);
                                                        pcm.repeated_msg_raw_end = to_uint16(cmd_payload[5], cmd_payload[6]);
                                                    }
                                                    else if (pcm.repeated_msg_length == 0x01) // 1-byte length
                                                    {
                                                        pcm.repeated_msg_raw_start = cmd_payload[3];
                                                        pcm.repeated_msg_raw_end = cmd_payload[4];
                                                    }

                                                    uint8_t ret[2] = { 0x00, to_pcm };
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                }
                                                else
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, too long message
                                                }
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the SCI-bus (PCM) once
                                    case repeated_list_msg: // 0x05 - send a set of messages repeatedly to the SCI-bus (PCM)
                                    {
                                        if (payload_length < 3)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 00 02 02 14 07 02 14 08
                                        // --------------------------------
                                        // 00: message iteration not supported, reads always zero
                                        // 02: number of messages
                                        // 02: 1st message length
                                        // 14 07: message #1
                                        // 02: 2nd message length
                                        // 14 08: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 2; i < payload_length; i++)
                                        {
                                            pcm.msg_buffer[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        pcm.msg_to_transmit_count = cmd_payload[1]; // save number of messages
                                        pcm.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        pcm.repeated_msg_length = cmd_payload[2]; // first message length is saved
                                        pcm.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning
                                        pcm.repeat = true; // set flag
                                        pcm.repeat_next = true; // set flag
                                        pcm.repeat_iterate = false;

                                        if (subdatacode == list_msg) pcm.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) pcm.repeat_list_once = false;
                                        
                                        pcm.repeat_stop = false;

                                        uint8_t ret[2] = { 0x00, to_pcm };

                                        if (subdatacode == list_msg) send_usb_packet(from_usb, to_usb, msg_tx, list_msg, ret, 2); // acknowledge
                                        else if (subdatacode == repeated_list_msg) send_usb_packet(from_usb, to_usb, msg_tx, repeated_list_msg, ret, 2); // acknowledge
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
                                    case stop_msg_flow: // 0x01 - stop message transmission (single and repeated as well)
                                    {
                                        tcm.repeat = false;
                                        tcm.repeat_next = false;
                                        tcm.repeat_iterate = false;
                                        tcm.repeat_list_once = false;
                                        tcm.repeat_stop = true;
                                        tcm.msg_to_transmit_count = 0;
                                        tcm.msg_to_transmit_count_ptr = 0;
                                        tcm.repeated_msg_length = 0;
                                        tcm.repeated_msg_last_millis = 0;
                                        tcm.msg_buffer_ptr = 0;
                                        
                                        uint8_t ret[2] = { 0x00, to_tcm };
                                        send_usb_packet(from_usb, to_usb, msg_tx, stop_msg_flow, ret, 2);
                                        break;
                                    }
                                    case single_msg: // 0x02 - send message to the SCI-bus once
                                    {
                                        if (!payload_bytes || (payload_length > TCM_RX3_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Fill the pending buffer with the message to be sent
                                        for (uint16_t i = 0; i < payload_length; i++)
                                        {
                                            tcm.msg_buffer[i] = cmd_payload[i];
                                        }

                                        tcm.msg_buffer_ptr = payload_length;
                                        tcm.msg_to_transmit_count = 1;
                                        tcm.msg_tx_pending  = true; // set flag so the main loop knows there's something to do

                                        uint8_t ret[2] = { 0x00, to_tcm };
                                        send_usb_packet(from_usb, to_usb, msg_tx, single_msg, ret, 2);
                                        break;
                                    }
                                    case repeated_single_msg: // 0x04 - send repeated message(s) to the SCI-bus (TCM)
                                    {
                                        if ((payload_length < 4) || (payload_length > TCM_RX3_BUFFER_SIZE))
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }
                                        
                                        switch (cmd_payload[0]) // first payload byte tells us if a message has variables that need to increase every iteration
                                        {
                                            case 0x00: // no iteration needed, send the same message(s) again and again
                                            {
                                                // Payload structure example:
                                                // 00 01 06 F4 0A 0B 0C 0D 11
                                                // -----------------------------------
                                                // 00: no message iteration needed, send the same message(s) again ang again
                                                // 01: number of messages
                                                // 06: message length
                                                // F4 0A 0B 0C 0D 11: message

                                                for (uint16_t i = 3; i < payload_length; i++)
                                                {
                                                    tcm.msg_buffer[i-3] = cmd_payload[i]; // copy and save all the message bytes for this session
                                                }

                                                tcm.msg_to_transmit_count = cmd_payload[1]; // message count
                                                tcm.repeated_msg_length = cmd_payload[2]; // message length
                                                tcm.msg_buffer_ptr = tcm.repeated_msg_length;
                                                tcm.repeat = true; // set flag
                                                tcm.repeat_next = true; // set flag
                                                tcm.repeat_iterate = false; // set flag
                                                tcm.repeat_list_once = false;
                                                tcm.repeat_stop = false;

                                                uint8_t ret[2] = { 0x00, to_tcm };
                                                send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                break;
                                            }
                                            case 0x01: // message iteration needed, 1 message only!
                                            {
                                                // Payload structure example:
                                                // 01 01 04 26 00 00 00 26 01 FF FF 
                                                // Note: iteration only works for messages of maximum 4 bytes length and it assumes one ID-byte and 1-2-3 address bytes
                                                // -----------------------------------
                                                // 01: message iteration needed, 1 message only!
                                                // 01: number of messages
                                                // 04: message length
                                                // 26 00 00 00: first message in the sequence
                                                // 26 01 FF FF: last message in the sequence

                                                if ((payload_length > 4) && (payload_length < 12)) // at least 5 bytes are necessary (min: 3 flag bytes + 1 start byte + 1 end byte = 5, max: 3 flag bytes + 4 start byte + 4 end byte = 11)
                                                {
                                                    tcm.msg_to_transmit_count = 1;
                                                    tcm.repeated_msg_length = cmd_payload[2]; // message length
                                                    tcm.msg_buffer_ptr = 0;
                                                    tcm.repeat = true; // set flag
                                                    tcm.repeat_next = true; // set flag
                                                    tcm.repeat_iterate = true; // set flag
                                                    tcm.repeat_list_once = false;
                                                    tcm.repeat_stop = false;

                                                    if (tcm.repeated_msg_length == 0x04) // 4-bytes length
                                                    {
                                                        tcm.repeated_msg_raw_start =  to_uint32(cmd_payload[3], cmd_payload[4], cmd_payload[5], cmd_payload[6]);
                                                        tcm.repeated_msg_raw_end = to_uint32(cmd_payload[7], cmd_payload[8], cmd_payload[9], cmd_payload[10]);
                                                    }
                                                    else if (tcm.repeated_msg_length == 0x03) // 3-bytes length
                                                    {
                                                        tcm.repeated_msg_raw_start = to_uint32(0, cmd_payload[3], cmd_payload[4], cmd_payload[5]);
                                                        tcm.repeated_msg_raw_end = to_uint32(0, cmd_payload[6], cmd_payload[7], cmd_payload[8]);
                                                    }
                                                    else if (tcm.repeated_msg_length == 0x02) // 2-bytes length
                                                    {
                                                        tcm.repeated_msg_raw_start = to_uint16(cmd_payload[3], cmd_payload[4]);
                                                        tcm.repeated_msg_raw_end = to_uint16(cmd_payload[5], cmd_payload[6]);
                                                    }
                                                    else if (tcm.repeated_msg_length == 0x01) // 1-byte length
                                                    {
                                                        tcm.repeated_msg_raw_start = cmd_payload[3];
                                                        tcm.repeated_msg_raw_end = cmd_payload[4];
                                                    }

                                                    uint8_t ret[2] = { 0x00, to_tcm };
                                                    send_usb_packet(from_usb, to_usb, msg_tx, repeated_single_msg, ret, 2); // acknowledge
                                                }
                                                else
                                                {
                                                    send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, too long message
                                                }
                                                break;
                                            }
                                            default:
                                            {
                                                send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error, no message included
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    case list_msg: // 0x03 - send a set of messages to the SCI-bus (TCM) once
                                    case repeated_list_msg: // 0x05 - send a set of messages repeatedly to the SCI-bus (TCM)
                                    {
                                        if (payload_length < 3)
                                        {
                                            send_usb_packet(from_usb, to_usb, ok_error, error_payload_invalid_values, err, 1); // error
                                            break;
                                        }

                                        // Payload structure example:
                                        // 00 02 02 14 07 02 14 08
                                        // --------------------------------
                                        // 00: message iteration not supported, reads always zero
                                        // 02: number of messages
                                        // 02: 1st message length
                                        // 14 07: message #1
                                        // 02: 2nd message length
                                        // 14 08: message #2
                                        // XX: n-th message length
                                        // XX XX...: message #n

                                        for (uint8_t i = 2; i < payload_length; i++)
                                        {
                                            tcm.msg_buffer[i-2] = cmd_payload[i]; // copy and save all the message bytes for this session
                                        }

                                        tcm.msg_to_transmit_count = cmd_payload[1]; // save number of messages
                                        tcm.msg_to_transmit_count_ptr = 0; // current message to transmit
                                        tcm.repeated_msg_length = cmd_payload[2]; // first message length is saved
                                        tcm.msg_buffer_ptr = 0; // set the pointer in the main buffer at the beginning
                                        tcm.repeat = true; // set flag
                                        tcm.repeat_next = true; // set flag
                                        tcm.repeat_iterate = false;

                                        if (subdatacode == list_msg) tcm.repeat_list_once = true;
                                        else if (subdatacode == repeated_list_msg) tcm.repeat_list_once = false;
                                        
                                        tcm.repeat_stop = false;

                                        uint8_t ret[2] = { 0x00, to_tcm };

                                        if (subdatacode == list_msg) send_usb_packet(from_usb, to_usb, msg_tx, list_msg, ret, 2); // acknowledge
                                        else if (subdatacode == repeated_list_msg) send_usb_packet(from_usb, to_usb, msg_tx, repeated_list_msg, ret, 2); // acknowledge
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
}

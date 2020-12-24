#ifndef USB_H
#define USB_H

#include <Arduino.h>

#define USB_RX0_BUFFER_SIZE 1024
#define USB_TX0_BUFFER_SIZE 1024

#define USB_RX0_BUFFER_MASK (USB_RX0_BUFFER_SIZE - 1)
#define USB_TX0_BUFFER_MASK (USB_TX0_BUFFER_SIZE - 1)

#define USB_RECEIVE_INTERRUPT   USART0_RX_vect
#define USB_TRANSMIT_INTERRUPT  USART0_UDRE_vect
#define USB_STATUS              UCSR0A
#define USB_CONTROL             UCSR0B
#define USB_DATA                UDR0
#define USB_UDRIE               UDRIE0

#define UART_FRAME_ERROR        0x1000  /**< Framing Error by UART          00010000 00000000 - FIXED BIT */ 
#define UART_OVERRUN_ERROR      0x0800  /**< Overrun condition by UART      00001000 00000000 - FIXED BIT */
#define UART_BUFFER_OVERFLOW    0x0400  /**< Receive ringbuffer overflow    00000100 00000000 - ARBITRARY */
#define UART_RX_NO_DATA         0x0200  /**< Receive buffer is empty        00000010 00000000 - ARBITRARY */

void usb_init(uint16_t ubrr);
uint16_t usb_getc(void);
uint16_t usb_peek(uint16_t index = 0);
void usb_putc(uint8_t data);
void usb_puts(const char *s);
void usb_puts_p(const char *progmem_s);
uint16_t usb_rx_available(void);
uint16_t usb_tx_available(void);
void usb_rx_flush(void);
void usb_tx_flush(void);
void send_usb_packet(uint8_t source, uint8_t target, uint8_t command, uint8_t subdatacode, uint8_t *payloadbuff, uint16_t payloadbufflen);
void handle_usb_data(void);

#endif // USB_H

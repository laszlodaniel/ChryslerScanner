#ifndef SCI_H
#define SCI_H

#include <Arduino.h>
#include "usb.h"
#include "common.h"

#define PCM_RX2_BUFFER_SIZE 256
#define PCM_TX2_BUFFER_SIZE 256
#define PCM_RX2_BUFFER_MASK (PCM_RX2_BUFFER_SIZE - 1)
#define PCM_TX2_BUFFER_MASK (PCM_TX2_BUFFER_SIZE - 1)

#define PCM_RECEIVE_INTERRUPT   USART2_RX_vect
#define PCM_TRANSMIT_INTERRUPT  USART2_UDRE_vect
#define PCM_STATUS              UCSR2A
#define PCM_CONTROL             UCSR2B
#define PCM_DATA                UDR2
#define PCM_UDRIE               UDRIE2  

#define TCM_RX3_BUFFER_SIZE 256
#define TCM_TX3_BUFFER_SIZE 256
#define TCM_RX3_BUFFER_MASK (TCM_RX3_BUFFER_SIZE - 1)
#define TCM_TX3_BUFFER_MASK (TCM_TX3_BUFFER_SIZE - 1)

#define TCM_RECEIVE_INTERRUPT   USART3_RX_vect
#define TCM_TRANSMIT_INTERRUPT  USART3_UDRE_vect
#define TCM_STATUS              UCSR3A
#define TCM_CONTROL             UCSR3B
#define TCM_DATA                UDR3
#define TCM_UDRIE               UDRIE3

#define A_PCM_RX_EN 22 // SCI-bus configuration selector digital pins on ATmega2560
#define A_PCM_TX_EN 23
#define A_TCM_RX_EN 24
#define A_TCM_TX_EN 25
#define B_PCM_RX_EN 26
#define B_PCM_TX_EN 27
#define B_TCM_RX_EN 28
#define B_TCM_TX_EN 29

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

extern uint8_t sci_hi_speed_memarea[16];

void pcm_init(uint16_t ubrr);
uint16_t pcm_getc(void);
uint16_t pcm_peek(uint16_t index = 0);
void pcm_putc(uint8_t data);
void pcm_puts(const char *s);
void pcm_puts_p(const char *progmem_s);
uint8_t pcm_rx_available(void);
uint8_t pcm_tx_available(void);
void pcm_rx_flush(void);
void pcm_tx_flush(void);
void tcm_init(uint16_t ubrr);
uint16_t tcm_getc(void);
uint16_t tcm_peek(uint16_t index = 0);
void tcm_putc(uint8_t data);
void tcm_puts(const char *s);
void tcm_puts_p(const char *progmem_s);
uint8_t tcm_rx_available(void);
uint8_t tcm_tx_available(void);
void tcm_rx_flush(void);
void tcm_tx_flush(void);
void configure_sci_bus(uint8_t data);
void handle_sci_data(void);

#endif // SCI_H

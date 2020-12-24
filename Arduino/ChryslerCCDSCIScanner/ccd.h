#ifndef CCD_H
#define CCD_H

#include <Arduino.h>

#define CCD_RX1_BUFFER_SIZE 256
#define CCD_TX1_BUFFER_SIZE 256

#define CCD_POSITIVE  A1 // CCD+ analog input
#define CCD_NEGATIVE  A2 // CCD- analog input
#define TBEN          13 // CCD-bus termination and bias enable pin

extern uint32_t ccd_positive_adc;
extern uint16_t ccd_positive_volts;
extern uint32_t ccd_negative_adc;
extern uint16_t ccd_negative_volts;
extern uint8_t ccd_volts_array[4];

void ccd_init();
void check_ccd_volts(void);
void handle_ccd_data(void);

#endif // CCD_H

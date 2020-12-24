#ifndef LED_H
#define LED_H

#include <Arduino.h>

#define RX_LED  35 // status LED, message received
#define TX_LED  36 // status LED, message sent
#define ACT_LED 37 // status LED, activity

extern uint16_t led_blink_duration;
extern uint16_t heartbeat_interval;
extern bool heartbeat_enabled;

void blink_led(uint8_t led);
void handle_leds(void);

#endif // LED_H

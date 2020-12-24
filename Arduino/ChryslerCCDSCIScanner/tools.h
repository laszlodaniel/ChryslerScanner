#ifndef TOOLS_H
#define TOOLS_H

#include <Arduino.h>

#define STOP  0x00
#define START 0x01

extern uint32_t previous_random_ccd_msg_millis;
extern uint8_t avr_signature[3];

void update_timestamp(uint8_t *target);
bool array_contains(uint8_t *src_array, uint8_t src_array_length, uint8_t value);
uint8_t calculate_checksum(uint8_t *buff, uint16_t index, uint16_t bufflen);
uint16_t free_ram(void);
void read_avr_signature(uint8_t *target);
void send_hwfw_info(void);
int cmd_reset(void);
int cmd_handshake(void);
int cmd_status(void);

#endif // TOOLS_H

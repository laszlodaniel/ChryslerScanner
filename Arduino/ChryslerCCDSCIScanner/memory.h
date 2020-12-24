#ifndef MEMORY_H
#define MEMORY_H

#include <Arduino.h>
#include <extEEPROM.h> // https://github.com/JChristensen/extEEPROM

extern extEEPROM eep;

extern bool eep_present;
extern uint8_t eep_status;
extern uint8_t eep_result;
extern uint8_t eep_checksum[1];
extern uint8_t eep_calculated_checksum;
extern bool eep_checksum_ok;

void exteeprom_init(void);
void evaluate_eep_checksum(void);

#endif // MEMORY_H

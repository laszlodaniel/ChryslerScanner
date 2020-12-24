#ifndef BATTERY_H
#define BATTERY_H

#include <Arduino.h>

#define BATT A0 // battery voltage sensor analog input

extern uint16_t adc_supply_voltage;
extern uint16_t r19_value;
extern uint16_t r20_value;
extern uint16_t adc_max_value;
extern uint32_t battery_adc;
extern uint16_t battery_volts;
extern uint8_t battery_volts_array[2];
extern bool connected_to_vehicle;

void check_battery_volts(void);

#endif // BATTERY_H

#include "battery.h"

uint16_t adc_supply_voltage = 500; // supply voltage multiplied by 100: 5.00V -> 500
uint16_t r19_value = 27000; // high resistor value in the divider (R19): 27 kOhm = 27000
uint16_t r20_value = 5000;  // low resistor value in the divider (R20): 5 kOhm = 5000
uint16_t adc_max_value = 1023; // 1023 for 10-bit resolution
uint32_t battery_adc = 0;   // raw analog reading is stored here
uint16_t battery_volts = 0; // converted to battery voltage and multiplied by 100: 12.85V -> 1285
uint8_t battery_volts_array[2]; // battery_volts is separated to byte components here
bool connected_to_vehicle = false;

/*************************************************************************
Function: check_battery_volts()
Purpose:  measure voltage of the OBD16 pin through a resistor divider
**************************************************************************/
void check_battery_volts(void)
{
    battery_adc = 0;
    
    for (uint16_t i = 0; i < 200; i++) // get 200 samples in quick succession
    {
        battery_adc += analogRead(BATT);
    }
    
    battery_adc /= 200; // divide the sum by 200 to get average value
    battery_volts = (uint16_t)(battery_adc*(adc_supply_voltage/100.0)/adc_max_value*((double)r19_value+(double)r20_value)/(double)r20_value*100.0); // resistor divider equation
    
    if (battery_volts < 600) // battery_volts < 6V
    {
        battery_volts_array[0] = 0; // workaround if scanner's power switch is at OFF position and analog pin is floating
        battery_volts_array[1] = 0;
        connected_to_vehicle = false;
    }
    else // battery_volts >= 6V
    {
        battery_volts_array[0] = (battery_volts >> 8) & 0xFF;
        battery_volts_array[1] = battery_volts & 0xFF;
        connected_to_vehicle = true;
    }
}

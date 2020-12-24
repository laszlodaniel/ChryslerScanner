#ifndef LCD_H
#define LCD_H

#include <Arduino.h>
#include <LiquidCrystal_I2C.h> // https://github.com/fdebrabander/Arduino-LiquidCrystal-I2C-library
#include <Wire.h>

extern LiquidCrystal_I2C lcd;

extern bool lcd_enabled;
extern uint8_t lcd_i2c_address;
extern uint8_t lcd_char_width;
extern uint8_t lcd_char_height;
extern uint8_t lcd_refresh_rate;
extern uint8_t lcd_units;
extern uint8_t lcd_data_source;
extern uint32_t lcd_last_update;
extern uint16_t lcd_update_interval;

void lcd_init(void);
void print_display_layout_1_metric(void);
void print_display_layout_1_imperial(void);
void handle_lcd(uint8_t bus, uint8_t *data, uint8_t index, uint8_t datalength);

#endif // LCD_H

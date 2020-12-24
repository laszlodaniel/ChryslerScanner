#include "lcd.h"
#include "common.h"

// Construct an object called "lcd" for the external display.
LiquidCrystal_I2C lcd(0x27, 20, 4);

bool lcd_enabled = false;
uint8_t lcd_i2c_address = 0;
uint8_t lcd_char_width = 0;
uint8_t lcd_char_height = 0;
uint8_t lcd_refresh_rate = 0;
uint8_t lcd_units = 0; // 0-imperial, 1-metric
uint8_t lcd_data_source = 1; // 1: CCD-bus, 2: SCI-bus (PCM), 3: SCI-bus (TCM)
uint32_t lcd_last_update = 0;
uint16_t lcd_update_interval = 0;

// Custom LCD-characters
// https://maxpromer.github.io/LCD-Character-Creator/
uint8_t up_symbol[8]     = { 0x00, 0x04, 0x0E, 0x15, 0x04, 0x04, 0x00, 0x00 }; // ↑
uint8_t down_symbol[8]   = { 0x00, 0x04, 0x04, 0x15, 0x0E, 0x04, 0x00, 0x00 }; // ↓
uint8_t left_symbol[8]   = { 0x00, 0x04, 0x08, 0x1F, 0x08, 0x04, 0x00, 0x00 }; // ←
uint8_t right_symbol[8]  = { 0x00, 0x04, 0x02, 0x1F, 0x02, 0x04, 0x00, 0x00 }; // →
uint8_t enter_symbol[8]  = { 0x00, 0x01, 0x05, 0x09, 0x1F, 0x08, 0x04, 0x00 }; // 
uint8_t degree_symbol[8]; // °

char vin_characters[] = "-----------------"; // 17 character
String vin_string;

/*************************************************************************
Function: lcd_init()
Purpose:  initialize LCD
**************************************************************************/
void lcd_init(void)
{
    lcd = LiquidCrystal_I2C(lcd_i2c_address, lcd_char_width, lcd_char_height);
    lcd.begin();
    lcd.backlight();  // backlight on
    lcd.clear();      // clear display
    lcd.home();       // set cursor in home position (0, 0)
        
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.print(F("--------------------")); // F(" ") makes the compiler store the string inside flash memory instead of RAM, good practice if system is low on RAM
        lcd.setCursor(0, 1);
        lcd.print(F("  CHRYSLER CCD/SCI  "));
        lcd.setCursor(0, 2);
        lcd.print(F("   SCANNER V1.4X    "));
        lcd.setCursor(0, 3);
        lcd.print(F("--------------------"));
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.print(F("CHRYSLER CCD/SCI")); // F(" ") makes the compiler store the string inside flash memory instead of RAM, good practice if system is low on RAM
        lcd.setCursor(0, 1);
        lcd.print(F(" SCANNER V1.4X  "));
    }
    else
    {
        lcd.print(F("CCD/SCI"));
    }

    lcd.createChar(0, up_symbol); // custom character from "upsymbol" variable with id number 0
    lcd.createChar(1, down_symbol);
    lcd.createChar(2, left_symbol);
    lcd.createChar(3, right_symbol);
    lcd.createChar(4, enter_symbol);
    lcd.createChar(5, degree_symbol);
}

/*************************************************************************
Function: print_display_layout_1_metric()
Purpose:  printing default metric display layout to LCD (km/h, km...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_metric(void)
{
    lcd.clear();
    
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0km/h    0rpm   0%")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0/  0 C     0.0kPa")); // 2nd line 
        lcd.setCursor(0, 2);
        lcd.print(F(" 0.0/ 0.0V          ")); // 3rd line
        lcd.setCursor(0, 3);
        lcd.print(F("     0.000km        ")); // 4th line
        lcd.setCursor(7, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0km/h     0rpm")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0 C     0.0kPa")); // 2nd line 
        lcd.setCursor(3, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else
    {
        lcd.setCursor(0, 0);
        lcd.print(F("    0rpm")); // 1st line
    }
}


/*************************************************************************
Function: print_display_layout_1_imperial()
Purpose:  printing default metric display layout to LCD (mph, mi...)
Note:     prints when switching between different layouts
**************************************************************************/
void print_display_layout_1_imperial(void)
{
    lcd.clear();
    
    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0mph     0rpm   0%")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0/  0 F     0.0psi")); // 2nd line 
        lcd.setCursor(0, 2);
        lcd.print(F(" 0.0/ 0.0V          ")); // 3rd line
        lcd.setCursor(0, 3);
        lcd.print(F("     0.000mi        ")); // 4th line
        lcd.setCursor(7, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
    {
        lcd.setCursor(0, 0);
        lcd.print(F("  0mph      0rpm")); // 1st line
        lcd.setCursor(0, 1);
        lcd.print(F("  0 F     0.0psi")); // 2nd line 
        lcd.setCursor(3, 1);
        lcd.write((uint8_t)5); // print degree symbol
    }
    else
    {
        lcd.setCursor(0, 0);
        lcd.print(F("    0rpm")); // 1st line
    }
  
}

/*************************************************************************
Function: handle_lcd()
Purpose:  write stuff to LCD
Note:     uncomment cases when necessary
**************************************************************************/
void handle_lcd(uint8_t bus, uint8_t *data, uint8_t index, uint8_t datalength)
{
    if (lcd_enabled)
    {
//        current_millis = millis(); // check current time
//        if ((current_millis - lcd_last_update ) > lcd_update_interval) // refresh rate
//        {
//            lcd_last_update = current_millis;
            
            uint8_t message_length = datalength-index;
            uint8_t message[message_length];

            if (message_length == 0) return;
        
            for (uint8_t i = index; i < datalength; i++)
            {
                message[i-index] = data[i]; // make a local copy of the source array
            }

            switch (lcd_data_source)
            {
                case from_ccd:
                {
                    if (bus == from_ccd)
                    {
                        switch (message[0]) // check ID-byte
                        {
                            case 0x24: // VEHICLE SPEED
                            {
                                if (message_length > 3)
                                {
                                    uint8_t vehicle_speed = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        vehicle_speed = message[1];
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        vehicle_speed = message[2];
                                    }
        
                                    if (((lcd_char_width == 20) && (lcd_char_height == 4)) || ((lcd_char_width == 16) && (lcd_char_height == 2))) // 20x4 / 16x2 LCD
                                    {
                                        if (vehicle_speed < 10)
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print("  ");
                                            lcd.print(vehicle_speed);
                                        }
                                        else if ((vehicle_speed >= 10) && (vehicle_speed < 100))
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(" ");
                                            lcd.print(vehicle_speed);
                                        }
                                        else if (vehicle_speed >= 100)
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(vehicle_speed);
                                        }
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x42: // THROTTLE POSITION SENSOR | CRUISE CONTROL
                            {
                                if (message_length > 3)
                                {
                                    uint8_t tps_position = round(message[1] * 0.65);
            
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (tps_position < 10)
                                        {
                                            lcd.setCursor(16, 0);
                                            lcd.print("  ");
                                            lcd.print(tps_position);
                                        }
                                        else if ((tps_position >= 10) && (tps_position < 100))
                                        {
                                            lcd.setCursor(16, 0);
                                            lcd.print(" ");
                                            lcd.print(tps_position);
                                        }
                                        else if (tps_position >= 100)
                                        {
                                            lcd.setCursor(16, 0);
                                            lcd.print(tps_position);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x8C: // ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE
                            {
                                if (message_length > 3)
                                {
                                    int coolant_temp = 0;
                                    int intake_temp = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        coolant_temp = round((message[1] * 1.8) - 198.4);
                                        intake_temp = round((message[2] * 1.8) - 198.4);
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        coolant_temp = message[1] - 128;
                                        intake_temp = message[2] - 128;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (coolant_temp <= -100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp > -10) && (coolant_temp < 0))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("  ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if (coolant_temp >= 100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
        
                                        if (intake_temp <= -100)
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((intake_temp > -100) && (intake_temp <= -10))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp > -10) && (intake_temp < 0))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(" ");
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp >= 0) && (intake_temp < 10))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print("  ");
                                            lcd.print(intake_temp);
                                        }
                                        else if ((intake_temp >= 10) && (intake_temp < 100))
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(" ");
                                            lcd.print(intake_temp);
                                        }
                                        else if (intake_temp >= 100)
                                        {
                                            lcd.setCursor(4, 1);
                                            lcd.print(intake_temp);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        if (coolant_temp <= -100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("-99");
                                        }
                                        else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp > -10) && (coolant_temp < 0))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print("  ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(" ");
                                            lcd.print(coolant_temp);
                                        }
                                        else if (coolant_temp >= 100)
                                        {
                                            lcd.setCursor(0, 1);
                                            lcd.print(coolant_temp);
                                        }
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0x0C: // ENGINE COOLANT TEMPERATURE | INTAKE AIR TEMPERATURE + BATTERY VOLTAGE (+ OIL PRESSURE)
                            {
                                // TODO
                                break;
                            }
                            case 0xCE: // VEHICLE DISTANCE / ODOMETER VALUE
                            {
                                if (message_length > 5)
                                {
                                    uint32_t odometer_raw = ((uint32_t)message[1] << 24) + ((uint32_t)message[2] << 16) + ((uint32_t)message[3] << 8) + ((uint32_t)message[4]);
                                    double odometer = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        odometer = roundf(odometer_raw / 8.0);
                                        odometer = odometer / 1000.0;
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        odometer = roundf(odometer_raw * 1609.334138 / 8000.0);
                                        odometer = odometer / 1000.0;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (odometer < 10.0)
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("     ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 10.0) && (odometer < 100.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("    ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 100.0) && (odometer < 1000.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("   ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 1000.0) && (odometer < 10000.0))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print("  ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if ((odometer >= 10000) && (odometer < 100000))
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print(" ");
                                            lcd.print(odometer, 3);
                                        }
                                        else if (odometer >= 100000)
                                        {
                                            lcd.setCursor(0, 3);
                                            lcd.print(odometer, 3);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0xD4: // BATTERY VOLTAGE | CALCULATED CHARGING VOLTAGE
                            {
                                if (message_length > 3)
                                {
                                    float batt_volts = roundf(message[1] * 10.0 * 0.0592);
                                    batt_volts = batt_volts / 10.0;
                                    
                                    float charge_volts = roundf(message[2] * 10.0 * 0.0592);
                                    charge_volts = charge_volts / 10.0;
                                    
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (batt_volts < 10.0)
                                        {
                                            lcd.setCursor(0, 2);
                                            lcd.print(" ");
                                            lcd.print(batt_volts, 1);
                                        }
                                        else if (batt_volts >= 10.0)
                                        {
                                            lcd.setCursor(0, 2);
                                            lcd.print(batt_volts, 1);
                                        }
        
                                        if (charge_volts < 10.0)
                                        {
                                            lcd.setCursor(5, 2);
                                            lcd.print(" ");
                                            lcd.print(charge_volts, 1);
                                        }
                                        else if (charge_volts >= 10.0)
                                        {
                                            lcd.setCursor(5, 2);
                                            lcd.print(charge_volts, 1);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                break;
                            }
                            case 0xE4: // ENGINE SPEED | INTAKE MANIFOLD ABS. PRESSURE
                            {
                                if (message_length > 3)
                                {
                                    uint16_t engine_speed = 0;
                                    float map_value = 0;
                                    
                                    if (lcd_units == 0) // imperial
                                    {
                                        engine_speed = message[1] * 32;
                                        map_value = roundf(message[2] * 10.0 * 0.059756);
                                        map_value = map_value / 10.0;
                                    }
                                    else if (lcd_units == 1) // metric
                                    {
                                        engine_speed = message[1] * 32;
                                        map_value = roundf(message[2] * 10.0 * 6.894757 * 0.059756);
                                        map_value = map_value / 10.0;
                                    }
        
                                    if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(7, 0);
                                            lcd.print(engine_speed);
                                        }
                                        
                                        if (map_value < 10.0)
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 1);
                                        }
                                        else if ((map_value >= 10.0) && (map_value < 100.0))
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(map_value, 1);
                                        }
                                        else if (map_value >= 100.0)
                                        {
                                            lcd.setCursor(13, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 0);
                                        }
                                    }
                                    else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(9, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(8, 0);
                                            lcd.print(engine_speed);
                                        }
                                        
                                        if (map_value < 10.0)
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 1);
                                        }
                                        else if ((map_value >= 10.0) && (map_value < 100.0))
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(map_value, 1);
                                        }
                                        else if (map_value >= 100.0)
                                        {
                                            lcd.setCursor(9, 1);
                                            lcd.print(" ");
                                            lcd.print(map_value, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (engine_speed < 10)
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print("   ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 10) && (engine_speed < 100))
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print("  ");
                                            lcd.print(engine_speed);
                                        }
                                        else if ((engine_speed >= 100) && (engine_speed < 1000))
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print(" ");
                                            lcd.print(engine_speed);
                                        }
                                        else if (engine_speed >= 1000)
                                        {
                                            lcd.setCursor(1, 0);
                                            lcd.print(engine_speed);
                                        }
                                        else
                                        {
                                            lcd.setCursor(0, 0);
                                            lcd.print(engine_speed);
                                        }
                                    }
                                }
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
                case from_pcm:
                {
                    if (bus == from_pcm)
                    {
                        switch (message[0]) // check ID-byte
                        {
                            case 0x14: // diagnostic reponse message
                            {
                                if (message_length > 2)
                                {
                                    switch (message[1]) // parameter
                                    {
                                        case 0x01: // ambient air temperature
                                        {
                                            int ambient_temp = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                ambient_temp = message[2];
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                ambient_temp = round((message[2] * 0.555556) - 17.77778);
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (ambient_temp <= -100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((ambient_temp > -100) && (ambient_temp <= -10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp > -10) && (ambient_temp < 0))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 0) && (ambient_temp < 10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("  ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 10) && (ambient_temp < 100))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if (ambient_temp >= 100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (ambient_temp <= -100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((ambient_temp > -100) && (ambient_temp <= -10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp > -10) && (ambient_temp < 0))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 0) && (ambient_temp < 10))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print("  ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if ((ambient_temp >= 10) && (ambient_temp < 100))
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(" ");
                                                    lcd.print(ambient_temp);
                                                }
                                                else if (ambient_temp >= 100)
                                                {
                                                    lcd.setCursor(4, 1);
                                                    lcd.print(ambient_temp);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x05: // engine coolant temperature
                                        {
                                            int coolant_temp = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                coolant_temp = round((message[2] * 1.8) - 198.4);
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                coolant_temp = message[2] - 128;
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (coolant_temp <= -100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp > -10) && (coolant_temp < 0))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("  ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if (coolant_temp >= 100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (coolant_temp <= -100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("-99");
                                                }
                                                else if ((coolant_temp > -100) && (coolant_temp <= -10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp > -10) && (coolant_temp < 0))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 0) && (coolant_temp < 10))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print("  ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if ((coolant_temp >= 10) && (coolant_temp < 100))
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(" ");
                                                    lcd.print(coolant_temp);
                                                }
                                                else if (coolant_temp >= 100)
                                                {
                                                    lcd.setCursor(0, 1);
                                                    lcd.print(coolant_temp);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x0A: // battery voltage
                                        {
                                            float batt_volts = roundf(message[2] * 10.0 * 0.0592);
                                            batt_volts = batt_volts / 10.0;
                                            
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (batt_volts < 10.0)
                                                {
                                                    lcd.setCursor(0, 2);
                                                    lcd.print(" ");
                                                    lcd.print(batt_volts, 1);
                                                }
                                                else if (batt_volts >= 10.0)
                                                {
                                                    lcd.setCursor(0, 2);
                                                    lcd.print(batt_volts, 1);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x0B: // intake manifold absolute pressure
                                        {
                                            float map_value = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                map_value = roundf(message[2] * 0.059756 * 10.0);
                                                map_value = map_value / 10.0;
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                map_value = roundf(message[2] * 0.059756 * 6.894757 * 10.0);
                                                map_value = map_value / 10.0;
                                            }
                
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (map_value <= -99.0)
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" -99");
                                                }
                                                else if ((map_value > -99.0) && (map_value <= -10.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                                else if ((map_value > -10.0) && (map_value < 0.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 0.0) && (map_value < 10.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 10.0) && (map_value < 100.0))
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if (map_value >= 100.0)
                                                {
                                                    lcd.setCursor(13, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (map_value <= -99.0)
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" -99");
                                                }
                                                else if ((map_value > -99.0) && (map_value <= -10.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                                else if ((map_value > -10.0) && (map_value < 0.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 0.0) && (map_value < 10.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 1);
                                                }
                                                else if ((map_value >= 10.0) && (map_value < 100.0))
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(map_value, 1);
                                                }
                                                else if (map_value >= 100.0)
                                                {
                                                    lcd.setCursor(9, 1);
                                                    lcd.print(" ");
                                                    lcd.print(map_value, 0);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x11: // engine speed
                                        {
                                            uint16_t engine_speed = message[2] * 32;

                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(7, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(9, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(8, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            else
                                            {
                                                if (engine_speed < 10)
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print("   ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 10) && (engine_speed < 100))
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print("  ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if ((engine_speed >= 100) && (engine_speed < 1000))
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print(" ");
                                                    lcd.print(engine_speed);
                                                }
                                                else if (engine_speed >= 1000)
                                                {
                                                    lcd.setCursor(1, 0);
                                                    lcd.print(engine_speed);
                                                }
                                                else
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(engine_speed);
                                                }
                                            }
                                            break;
                                        }
                                        case 0x24: // battery charging voltage
                                        {
                                            float charge_volts = roundf(message[2] * 10.0 * 0.0592);
                                            charge_volts = charge_volts / 10.0;
                                            
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (charge_volts < 10.0)
                                                {
                                                    lcd.setCursor(5, 2);
                                                    lcd.print(" ");
                                                    lcd.print(charge_volts, 1);
                                                }
                                                else if (charge_volts >= 10.0)
                                                {
                                                    lcd.setCursor(5, 2);
                                                    lcd.print(charge_volts, 1);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x41: // vehicle speed
                                        {
                                            uint8_t vehicle_speed = 0;
                                            
                                            if (lcd_units == 0) // imperial
                                            {
                                                vehicle_speed = round(message[2] / 2.0);
                                            }
                                            else if (lcd_units == 1) // metric
                                            {
                                                vehicle_speed = round(message[2] * 1.609344 / 2.0 );
                                            }
                
                                            if (((lcd_char_width == 20) && (lcd_char_height == 4)) || ((lcd_char_width == 16) && (lcd_char_height == 2))) // 20x4 / 16x2 LCD
                                            {
                                                if (vehicle_speed < 10)
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print("  ");
                                                    lcd.print(vehicle_speed);
                                                }
                                                else if ((vehicle_speed >= 10) && (vehicle_speed < 100))
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(" ");
                                                    lcd.print(vehicle_speed);
                                                }
                                                else if (vehicle_speed >= 100)
                                                {
                                                    lcd.setCursor(0, 0);
                                                    lcd.print(vehicle_speed);
                                                }
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                        case 0x46: // throttle position sensor
                                        {
                                            uint8_t tps_position = round(message[2] * 0.3922);
                    
                                            if ((lcd_char_width == 20) && (lcd_char_height == 4)) // 20x4 LCD
                                            {
                                                if (tps_position < 10)
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print("  ");
                                                    lcd.print(tps_position);
                                                }
                                                else if ((tps_position >= 10) && (tps_position < 100))
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print(" ");
                                                    lcd.print(tps_position);
                                                }
                                                else if (tps_position >= 100)
                                                {
                                                    lcd.setCursor(16, 0);
                                                    lcd.print(tps_position);
                                                }
                                            }
                                            else if ((lcd_char_width == 16) && (lcd_char_height == 2)) // 16x2 LCD
                                            {
                                                
                                            }
                                            else
                                            {
                                                
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
//        }
    }
}

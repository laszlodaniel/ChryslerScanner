#include "led.h"

uint32_t rx_led_ontime = 0;
uint32_t tx_led_ontime = 0;
uint32_t act_led_ontime = 0;
uint32_t current_millis_blink = 0;
uint32_t previous_act_blink = 0;
uint16_t led_blink_duration = 50; // milliseconds
uint16_t heartbeat_interval = 5000; // milliseconds
bool heartbeat_enabled = true;

/*************************************************************************
Function: blink_led()
Purpose:  turn on one of the indicator LEDs and save time
Note:     this only turns on the chosen LED, handle_leds() turns it off
**************************************************************************/
void blink_led(uint8_t led)
{
    digitalWrite(led, LOW); // turn on LED
    switch (led) // save time when LED was turned on
    {
        case RX_LED:
        {
            rx_led_ontime = millis();
            break;
        }
        case TX_LED:
        {
            tx_led_ontime = millis();
            break;
        }
        case ACT_LED:
        {
            act_led_ontime = millis();
            break;
        }
        default:
        {
            break;
        }
    }
}

/*************************************************************************
Function: handle_leds()
Purpose:  turn off indicator LEDs when blink duration expires
Note:     ACT heartbeat is handled here too;
          how this works:
          - LEDs are turned on somewhere in the code and a timestamp
            is saved at the same time (xx_led_ontime),
          - this function is called pretty frequently so it can
            precisely track how long a given LED is on,
            then turn it off when the time comes
**************************************************************************/
void handle_leds(void)
{
    current_millis_blink = millis(); // check current time
    if (heartbeat_enabled)
    {
        if (current_millis_blink - previous_act_blink >= heartbeat_interval)
        {
            previous_act_blink = current_millis_blink; // save current time
            blink_led(ACT_LED);
        }
    }
    if (current_millis_blink - rx_led_ontime >= led_blink_duration)
    {
        digitalWrite(RX_LED, HIGH); // turn off RX LED
    }
    if (current_millis_blink - tx_led_ontime >= led_blink_duration)
    {
        digitalWrite(TX_LED, HIGH); // turn off TX LED
    }
    if (current_millis_blink - act_led_ontime >= led_blink_duration)
    {
        digitalWrite(ACT_LED, HIGH); // turn off ACT LED
    }
}

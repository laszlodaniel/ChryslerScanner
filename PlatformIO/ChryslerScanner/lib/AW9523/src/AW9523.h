#ifndef AW9523_H
#define AW9523_H

#define AW9523_ADDRESS          0x58 // device I2C address (5B for V200 PCB)
#define AW9523_INPUT_PORT0_REG  0x00 // P0 port input state
#define AW9523_INPUT_PORT1_REG  0x01 // P1 port input state
#define AW9523_OUTPUT_PORT0_REG 0x02 // P0 port output state
#define AW9523_OUTPUT_PORT1_REG 0x03 // P1 port output state
#define AW9523_CONFIG_PORT0_REG 0x04 // P0 port direction setting (input/output)
#define AW9523_CONFIG_PORT1_REG 0x05 // P1 port direction setting (input/output)
#define AW9523_INT_PORT0_REG    0x06 // P0 port interrupt setting (enable/disable)
#define AW9523_INT_PORT1_REG    0x07 // P1 port interrupt setting (enable/disable)
#define AW9523_ID_REG           0x10 // device ID
#define AW9523_CHIP_ID          0x23 // ID value for AW9523B
#define AW9523_GCR_REG          0x11 // global control register
#define AW9523_MODE_PORT0_REG   0x12 // P0 port mode (LED/GPIO)
#define AW9523_MODE_PORT1_REG   0x13 // P1 port mode (LED/GPIO)
#define AW9523_SW_RSTN_REG      0x7F // software reset

#define INPUT_ALL               0xFF
#define OUTPUT_ALL              0x00
#define HIGH_ALL                0xFF
#define LOW_ALL                 0x00
#define P0_OPEN_DRAIN           0x00
#define P0_PUSH_PULL            0x10
#define GPIO_MODE_ALL           0xFF
#define LED_MODE_ALL            0x00
#define INTERRUPT_DISABLE_ALL   0xFF
#define INTERRUPT_ENABLE_ALL    0x00

#endif
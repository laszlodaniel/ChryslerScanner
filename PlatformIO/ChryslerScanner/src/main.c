/*
 * ChryslerScanner (https://github.com/laszlodaniel/ChryslerScanner)
 * Copyright (C) 2018-2022, Daniel Laszlo
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

#include "AW9523.h"
#include "bootstrap.h"
#include "config.h"
#include "enums.h"
#include "macros.h"
#include "freertos/FreeRTOS.h"
#include "freertos/ringbuf.h"
#include "freertos/task.h"
#include "freertos/queue.h"
#include "driver/adc.h"
#include "esp_adc_cal.h"
#include "driver/gpio.h"
#include "driver/i2c.h"
#include "driver/timer.h"
#include "driver/uart.h"
#include "nvs_flash.h"
#include "nvs.h"
#include "soc/uart_channel.h"
#include "soc/uart_periph.h"
#include "soc/uart_reg.h"
#include "esp_spiffs.h"
#include "esp_app_format.h"
#include "esp_ota_ops.h"
#include "string.h"
#include "esp_efuse.h"
#include "esp_efuse_table.h"
#include "esp_efuse_custom_table.h"

uart_config_t usb_config = {
    .baud_rate = 250000,
    .data_bits = UART_DATA_8_BITS,
    .stop_bits = UART_STOP_BITS_1,
    .parity = UART_PARITY_DISABLE,
    .flow_ctrl = UART_HW_FLOWCTRL_DISABLE
};

uart_config_t ccd_config = {
    .baud_rate = 7812,
    .data_bits = UART_DATA_8_BITS,
    .stop_bits = UART_STOP_BITS_1,
    .parity = UART_PARITY_DISABLE,
    .flow_ctrl = UART_HW_FLOWCTRL_DISABLE,
};

uart_config_t sci_config = {
    .baud_rate = 7812,
    .data_bits = UART_DATA_8_BITS,
    .stop_bits = UART_STOP_BITS_1,
    .parity = UART_PARITY_DISABLE,
    .flow_ctrl = UART_HW_FLOWCTRL_DISABLE
};

i2c_config_t i2c_config = {
    .mode = I2C_MODE_MASTER,
    .sda_io_num = I2C_SDA_PIN,
    .scl_io_num = I2C_SCL_PIN,
    .sda_pullup_en = GPIO_PULLUP_DISABLE,
    .scl_pullup_en = GPIO_PULLUP_DISABLE,
    .clk_flags = 0,
    .master.clk_speed = 400000 // Hz
};

esp_vfs_spiffs_conf_t spiffs_config = {
    .base_path = "/spiffs",
    .partition_label = NULL,
    .max_files = 5, // opened at the same time
    .format_if_mount_failed = true
};

typedef struct {
    uint16_t hardware_version;
} device_desc_t;

device_desc_t device_desc = { 0 };

struct packet {
    bool available;
    uint8_t sync;
    uint16_t length;
    uint8_t datacode;
    uint8_t subdatacode;
    uint16_t payload_length;
    uint8_t payload[USB_RX_BUF_SIZE];
    uint8_t checksum;
} usb_packet;

QueueHandle_t usb_uart_queue;
QueueHandle_t ccd_uart_queue;
QueueHandle_t sci_uart_queue;
RingbufHandle_t usb_rx_ringbuf_handle;
nvs_handle_t settings_handle;

uint8_t usb_rx_buf[USB_RX_BUF_SIZE];
uint16_t usb_rx_ptr = 0;
uint16_t usb_rx_len = 0;
uint8_t pci_rx_buf[256];
uint16_t pci_rx_ptr = 0;
uint8_t pci_tx_buf[256];
uint16_t pci_tx_ptr = 0;

uint8_t sci_hi_speed_memarea[16] = { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF };
uint8_t sci_lo_speed_cmd_filter[6] = { 0x14, 0x15, 0x26, 0x27, 0x28, 0x2A };

uint8_t ack[1] = { 0x00 }; // acknowledge payload array
uint8_t err[1] = { 0xFF }; // error payload array

adc_bits_width_t adc_width = ADC_WIDTH_BIT_12;
adc_atten_t adc_atten = ADC_ATTEN_DB_11; // voltage measuring range: 150-2450 mV
adc_unit_t adc_unit = ADC_UNIT_1;
adc1_channel_t adc_battvolt_channel = BATTVOLT_ADC_CHANNEL;
adc1_channel_t adc_bootvolt_channel = BOOTVOLT_ADC_CHANNEL;
adc1_channel_t adc_progvolt_channel = PROGVOLT_ADC_CHANNEL;
esp_adc_cal_characteristics_t *adc_characteristics;

uint32_t current_millis = 0;
uint32_t rx_led_ontime = 0;
uint32_t tx_led_ontime = 0;
uint32_t pwr_led_ontime = 0;
uint32_t act_led_ontime = 0;
uint32_t previous_act_blink = 0;
uint16_t led_blink_duration = 50; // milliseconds
uint16_t heartbeat_interval = 5000; // milliseconds
bool heartbeat_enabled = true;
bool leds_enabled = true;

uint32_t hardware_version = 0;
uint32_t firmware_version = 0;

typedef struct state {
    bool en;
    volatile bool idle;
    volatile bool bus_level;
    bool active_bus_level;
    bool passive_bus_level;
    bool tb_en; // bus termination and bias
    bool ngc_mode;
    bool inverted_logic;
    bool rx_timeout;
    bool echo_received;
    bool response_received;
    bool msg_tx_pending;
    bool actuator_test_running;
    bool actuator_test_mode_valid;
    bool ls_request_running;
} state_t;

typedef struct bootstrap {
    bool upload_bootloader;
    bool upload_worker_function;
    bool start_worker_function;
    bool exit_worker_function;
    bool progvolts_request;
    uint8_t bootloader_src;
    uint8_t worker_function_src;
    uint8_t flash_chip_src;
} bootstrap_t;

typedef struct msg {
    uint8_t rx_buffer[SCI_RX_BUF_SIZE];
    volatile uint16_t rx_length;
    volatile uint16_t last_rx_length;
    volatile uint16_t rx_ptr;
    uint8_t tx_buffer[SCI_TX_BUF_SIZE];
    uint16_t tx_length;
    uint16_t tx_ptr;
    uint8_t actuator_test_byte;
    uint8_t ls_request_byte;
    uint8_t ls_response_byte;
    bool byte_received;
    uint8_t tx_count;
    uint8_t tx_count_ptr;
} msg_t;

typedef struct rx {
    volatile uint64_t now;
    volatile uint64_t last_change;
    volatile uint64_t diff;
    volatile bool last_state;
    volatile bool sof_read;
    volatile uint8_t bit_pos;
    volatile bool ifr_detected;
} rx_t;

typedef struct tx {
    volatile bool complete;
    volatile bool bit_write;
    volatile bool current_tx_bit;
    volatile uint8_t bit_num_write;
    volatile bool eof_write;
} tx_t;

typedef struct timing {
    uint16_t timeout;
    uint32_t last_byte_millis;
    uint32_t actuator_test_last_millis;
    uint32_t ls_request_last_millis;
} timing_t;

typedef struct repeat {
    bool en;
    bool next;
    bool list_once;
    bool stop;
    uint16_t interval;
    uint32_t last_millis;
    uint8_t retry_counter;
} repeat_t;

typedef struct random {
    bool en;
    uint16_t interval;
    uint16_t interval_min;
    uint16_t interval_max;
    uint32_t last_millis;
} random_t;

typedef struct stats {
    uint32_t msg_rx_count;
    uint32_t msg_tx_count;
} stats_t;

typedef struct bus {
    state_t state;
    bootstrap_t bootstrap;
    msg_t msg;
    rx_t rx;
    tx_t tx;
    timing_t timing;
    repeat_t repeat;
    random_t random;
    stats_t stats;
    uint8_t bus_settings;
} bus_t;

bus_t ccd, pci, sci;

void IRAM_ATTR ccd_startbit_sense_isr_handler(void *arg);
bool IRAM_ATTR ccd_idle_timer_isr_callback(void *arg);
void ccd_idle_timer_init(int group, int timer, uint16_t ccd_idle_bits);
bool IRAM_ATTR pci_idle_timer_isr_callback(void *arg);
bool IRAM_ATTR pci_symbol_timer_isr_callback(void *arg);
void pci_idle_timer_init(int group, int timer, uint16_t bus_idle_us);
void pci_symbol_timer_init(int group, int timer, uint16_t symbol_us);
void blink_led(uint8_t LED);
bool open_settings();
void close_settings();
uint32_t read_millivolts(adc1_channel_t adc_channel);

/**
 * @brief Get coding scheme used for efuse block 3 (custom).
 */
static esp_efuse_coding_scheme_t get_coding_scheme(void)
{
    esp_efuse_coding_scheme_t coding_scheme = esp_efuse_get_coding_scheme(EFUSE_BLK3);
    return coding_scheme;
}

/**
 * @brief Read hardware version from custom efuse table.
 */
static void read_device_desc_efuse_fields(device_desc_t *desc)
{
    ESP_ERROR_CHECK(esp_efuse_read_field_blob(ESP_EFUSE_HARDWARE_VERSION, &desc->hardware_version, 16));
}

/**
 * @brief Write hardware version to custom efuse table.
 */
//#ifdef CONFIG_EFUSE_VIRTUAL
static void write_device_desc_efuse_fields(device_desc_t *desc, esp_efuse_coding_scheme_t coding_scheme)
{
    const esp_efuse_coding_scheme_t coding_scheme_for_batch_mode = EFUSE_CODING_SCHEME_3_4;

    if (coding_scheme == coding_scheme_for_batch_mode)
    {
        ESP_ERROR_CHECK(esp_efuse_batch_write_begin());
    }

    ESP_ERROR_CHECK(esp_efuse_write_field_blob(ESP_EFUSE_HARDWARE_VERSION, &desc->hardware_version, 16));

    if (coding_scheme == coding_scheme_for_batch_mode)
    {
        ESP_ERROR_CHECK(esp_efuse_batch_write_commit());
    }
}
//#endif // CONFIG_EFUSE_VIRTUAL

/**
 * @brief Get milliseconds elapsed since system powerup.
 * 
 * @return Number of milliseconds elapsed since system powerup.
 */
uint32_t IRAM_ATTR millis()
{
    return (uint32_t)(esp_timer_get_time() / 1000ULL);
}

/**
 * @brief Get microseconds elapsed since system powerup.
 * 
 * @return Number of microseconds elapsed since system powerup.
 */
uint64_t IRAM_ATTR micros()
{
    return esp_timer_get_time();
}

/**
 * @brief Calculate checksum value of an input byte array.
 * 
 * @note Bytes are simply summed up and least significant byte (LSB) is returned.
 * 
 * @param buffer Pointer to the input byte array.
 * @param index First byte in array to begin with.
 * @param length Number of bytes to take into the calculation.
 * 
 * @return Checksum value of the input byte array.
 */
uint8_t calculate_checksum(uint8_t *buffer, uint16_t index, uint16_t length)
{
    uint8_t checksum = 0;

    for (uint16_t i = index; i < length; i++)
    {
        checksum += buffer[i]; // add bytes together
    }

    return checksum;
}

/**
 * @brief Calculate CRC value of an input byte array.
 * 
 * @note SAE J1850 CRC-8 implementation.
 * 
 * @param buffer Pointer to the input byte array.
 * @param index First byte in array to begin with.
 * @param length Number of bytes to take into the calculation.
 * 
 * @return CRC value of the input byte array.
 */
uint8_t calculate_crc(uint8_t *buffer, uint16_t index, uint16_t length)
{
    uint8_t crc = 0xFF, poly, bit_count;
    uint16_t byte_count;
    uint8_t *byte_point;
    uint8_t bit_point;

    for (byte_count = index, byte_point = (buffer + index); byte_count < length; ++byte_count, ++byte_point)
    {
        for (bit_count = 0, bit_point = 0x80; bit_count < 8; ++bit_count, bit_point >>= 1)
        {
            if (bit_point & *byte_point) // case for new bit = 1
            {
                if (crc & 0x80)
                {
                    poly = 1; // define the polynomial
                }
                else
                {
                    poly = 0x1C;
                }

                crc = ((crc << 1) | 1) ^ poly;
            }
            else // case for new bit = 0
            {
                poly = 0;

                if (crc & 0x80)
                {
                    poly = 0x1D;
                }

                crc = (crc << 1) ^ poly;
            }
        }
    }

    return ~crc;
}

/**
 * @brief Check if an input byte array contains the specified byte.
 * 
 * @note This functions checks first occurrence only.
 * 
 * @param buffer Pointer to the input byte array.
 * @param length Buffer length to take into consideration.
 * @param value Byte value to look for.
 * 
 * @return True if input byte array contains the specified byte.
 *         False if input byte aray does not contain the specified byte.
 */
bool array_contains(uint8_t *buffer, uint16_t length, uint8_t value)
{
    for (uint16_t i = 0; i < length; i++)
    {
        if (value == buffer[i]) return true;
    }

    return false;
}

/**
 * @brief Generate a random integer number between the specified minimum and maximum values.
 * 
 * @param min The random number should be greater or equal than this value.
 * @param max The random number should be less or equal than this value.
 * 
 * @return Random integer number between the specified minimum and maximum values.
 */
uint32_t get_random(uint32_t min, uint32_t max)
{
    return ((esp_random() % (max - min + 1)) + min);
}

/**
 * @brief Generate and send data packet over USB connection.
 * 
 * @param bus Bus label.
 * @param command Command.
 * @param subdatacode Subdatacode.
 * @param payload Pointer to payload array.
 * @param payload_length Payload array length.
 */
void send_usb_packet(uint8_t bus, uint8_t command, uint8_t subdatacode, uint8_t *payload, uint16_t payload_length)
{
    uint16_t packet_length = 0;

    if (bus > 0)
    {
        packet_length = payload_length + 6 + 4; // add 4 timestamp bytes
    }
    else
    {
        packet_length = payload_length + 6;
    }

    uint8_t packet[packet_length];
    uint8_t datacode = 0;
    uint8_t checksum = 0;

    // Assemble datacode from the first 2 input parameters.
    // Leftmost bit is always set to indicate packet is coming from the scanner.
    datacode = (1 << 7) + ((bus << 4) & 0x70) + (command & 0x0F);
    //         10000000 +   0xxx0000          +  0000yyyy       =  1xxxyyyy  

    packet[0] = PACKET_SYNC_BYTE; // add SYNC byte
    packet[1] = ((packet_length - 4) >> 8) & 0xFF; // add LENGTH high byte
    packet[2] = (packet_length - 4) & 0xFF; // add LENGTH low byte
    packet[3] = datacode; // add DATA CODE byte
    packet[4] = subdatacode; // add SUB-DATA CODE byte
    
    // If there are payload bytes add them after subdatacode as well.
    if (payload_length > 0)
    {
        if (bus > 0) // add timestamp for CCD/PCI/SCI-bus messages
        {
            uint8_t timestamp[4];
            current_millis = millis();
            timestamp[0] = (current_millis >> 24) & 0xFF;
            timestamp[1] = (current_millis >> 16) & 0xFF;
            timestamp[2] = (current_millis >> 8) & 0xFF;
            timestamp[3] = current_millis & 0xFF;

            for (int i = 0; i < 4; i++)
            {
                packet[5 + i] = timestamp[i];
            }

            for (int i = 0; i < payload_length; i++)
            {
                packet[9 + i] = payload[i];
            }
        }
        else // no timestamp for internal messages from scanner
        {
            for (int i = 0; i < payload_length; i++)
            {
                packet[5 + i] = payload[i];
            }
        }
    }

    // Calculate checksum.
    checksum = calculate_checksum(packet, 0, packet_length - 1);

    // Place checksum byte.
    packet[packet_length - 1] = checksum;

    blink_led(TX_LED_PIN);
    uart_write_bytes(UART_USB, (const uint8_t *)packet, packet_length);
}

/**
 * @brief Generate a handshake packet and send over USB connection.
 */
void send_handshake()
{
    uint8_t handshake[15] = { 0x43, 0x48, 0x52, 0x59, 0x53, 0x4C, 0x45, 0x52, 0x53, 0x43, 0x41, 0x4E, 0x4E, 0x45, 0x52 }; // CHRYSLERSCANNER
    send_usb_packet(Bus_USB, Command_Handshake, Error_OK, handshake, sizeof(handshake));
}

/**
 * @brief Generate a status packet and send over USB connection.
 */
void send_status()
{
    uint32_t free_ram = esp_get_free_heap_size();
    uint32_t battery_millivolts = read_millivolts(BATTVOLT_ADC_CHANNEL);
    uint32_t bootstrap_millivolts = read_millivolts(BOOTVOLT_ADC_CHANNEL);
    uint32_t programming_millivolts = read_millivolts(PROGVOLT_ADC_CHANNEL);

    uint8_t status[45];
    status[0] = (current_millis >> 24) & 0xFF;
    status[1] = (current_millis >> 16) & 0xFF;
    status[2] = (current_millis >> 8) & 0xFF;
    status[3] = current_millis & 0xFF;
    status[4] = (free_ram >> 24) & 0xFF;
    status[5] = (free_ram >> 16) & 0xFF;
    status[6] = (free_ram >> 8) & 0xFF;
    status[7] = free_ram & 0xFF;
    status[8] = (battery_millivolts >> 8) & 0xFF;
    status[9] = battery_millivolts & 0xFF;
    status[10] = (bootstrap_millivolts >> 8) & 0xFF;
    status[11] = bootstrap_millivolts & 0xFF;
    status[12] = (programming_millivolts >> 8) & 0xFF;
    status[13] = programming_millivolts & 0xFF;
    status[14] = ccd.bus_settings;
    status[15] = (ccd.stats.msg_rx_count >> 24) & 0xFF;
    status[16] = (ccd.stats.msg_rx_count >> 16) & 0xFF;
    status[17] = (ccd.stats.msg_rx_count >> 8) & 0xFF;
    status[18] = ccd.stats.msg_rx_count & 0xFF;
    status[19] = (ccd.stats.msg_tx_count >> 24) & 0xFF;
    status[20] = (ccd.stats.msg_tx_count >> 16) & 0xFF;
    status[21] = (ccd.stats.msg_tx_count >> 8) & 0xFF;
    status[22] = ccd.stats.msg_tx_count & 0xFF;
    status[23] = pci.bus_settings;
    status[24] = (pci.stats.msg_rx_count >> 24) & 0xFF;
    status[25] = (pci.stats.msg_rx_count >> 16) & 0xFF;
    status[26] = (pci.stats.msg_rx_count >> 8) & 0xFF;
    status[27] = pci.stats.msg_rx_count & 0xFF;
    status[28] = (pci.stats.msg_tx_count >> 24) & 0xFF;
    status[29] = (pci.stats.msg_tx_count >> 16) & 0xFF;
    status[30] = (pci.stats.msg_tx_count >> 8) & 0xFF;
    status[31] = pci.stats.msg_tx_count & 0xFF;
    status[32] = sci.bus_settings;
    status[33] = (sci.stats.msg_rx_count >> 24) & 0xFF;
    status[34] = (sci.stats.msg_rx_count >> 16) & 0xFF;
    status[35] = (sci.stats.msg_rx_count >> 8) & 0xFF;
    status[36] = sci.stats.msg_rx_count & 0xFF;
    status[37] = (sci.stats.msg_tx_count >> 24) & 0xFF;
    status[38] = (sci.stats.msg_tx_count >> 16) & 0xFF;
    status[39] = (sci.stats.msg_tx_count >> 8) & 0xFF;
    status[40] = sci.stats.msg_tx_count & 0xFF;
    status[41] = (heartbeat_interval >> 8) & 0xFF;
    status[42] = heartbeat_interval & 0xFF;
    status[43] = (led_blink_duration >> 8) & 0xFF;
    status[44] = led_blink_duration & 0xFF;

    send_usb_packet(Bus_USB, Command_Status, Error_OK, status, sizeof(status));
}

/**
 * @brief Generate a hardware information packet and send over USB connection.
 */
void send_hw_info()
{
    esp_chip_info_t chip_info;
    esp_chip_info(&chip_info);
    esp_chip_model_t chip_model = chip_info.model;
    uint8_t chip_revision = chip_info.revision;
    uint8_t chip_cores = chip_info.cores;
    uint32_t chip_features = chip_info.features;
    uint8_t chip_size = spi_flash_get_chip_size() / (1024*1024); // MB
    uint8_t *mac = malloc(6);
    esp_efuse_mac_get_default(mac);

    const esp_partition_t *partition = esp_ota_get_running_partition();
    esp_app_desc_t app_info;
    esp_ota_get_partition_description(partition, &app_info);
    firmware_version = ((app_info.version[0] - 0x30) << 24) | ((app_info.version[2] - 0x30) << 16) | ((app_info.version[4] - 0x30) << 8) | (app_info.version[6] - 0x30);
    read_device_desc_efuse_fields(&device_desc); // get hardware version

    uint8_t info[22];
    info[0] = (device_desc.hardware_version >> 12) & 0xF;
    info[1] = (device_desc.hardware_version >> 8) & 0xF;
    info[2] = (device_desc.hardware_version >> 4) & 0xF;
    info[3] = device_desc.hardware_version & 0xF;
    info[4] = (firmware_version >> 24) & 0xFF;
    info[5] = (firmware_version >> 16) & 0xFF;
    info[6] = (firmware_version >> 8) & 0xFF;
    info[7] = firmware_version & 0xFF;
    info[8] = chip_model;
    info[9] = chip_revision;
    info[10] = chip_cores;
    info[11] = (chip_features >> 24) & 0xFF;
    info[12] = (chip_features >> 16) & 0xFF;
    info[13] = (chip_features >> 8) & 0xFF;
    info[14] = chip_features & 0xFF;
    info[15] = chip_size;
    info[16] = mac[0];
    info[17] = mac[1];
    info[18] = mac[2];
    info[19] = mac[3];
    info[20] = mac[4];
    info[21] = mac[5];

    send_usb_packet(Bus_USB, Command_Response, Request_Info, info, sizeof(info));
}

/**
 * @brief Read data from an I2C device.
 * 
 * @param dev_addr Device address.
 * @param reg_addr Register address.
 * @param reg_data Pointer to a byte array to save data.
 * @param count Number of bytes to read.
 */
static void i2c_read_data(uint8_t dev_addr, uint8_t reg_addr, uint8_t *reg_data, uint16_t count)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (dev_addr << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg_addr, true);
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (dev_addr << 1) | I2C_MASTER_READ, true);
    i2c_master_read(cmd, reg_data, count, I2C_MASTER_LAST_NACK);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_CHANNEL, cmd, pdMS_TO_TICKS(1000));
    i2c_cmd_link_delete(cmd);
}

/**
 * @brief Read a byte from the specified register in the AW9523 I2C port expander chip.
 * 
 * @param reg_addr Register address.
 * 
 * @return Byte read from specified register in the AW9523 I2C port expander chip.
 */
static uint8_t aw9523_read_byte(uint8_t reg_addr)
{
    uint8_t buf[1] = { 0 };
    i2c_read_data(AW9523_ADDRESS, reg_addr, buf, 1);
    return buf[0];
}

/**
 * @brief Write data to an I2C device.
 * 
 * @param dev_addr Device address.
 * @param reg_addr Register address.
 * @param reg_data Pointer to a byte array to read data from.
 * @param count Number of bytes to write.
 */
static void i2c_write_data(uint8_t dev_addr, uint8_t reg_addr, uint8_t *reg_data, uint16_t count)
{
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (dev_addr << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg_addr, true);
    i2c_master_write(cmd, reg_data, count, true);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_CHANNEL, cmd, pdMS_TO_TICKS(1000));
    i2c_cmd_link_delete(cmd);
}

/**
 * @brief Write a byte to the specified register in the AW9523 I2C port expander chip.
 * 
 * @param reg_addr Register address.
 * @param reg_data Byte value to write.
 */
static void aw9523_write_byte(uint8_t reg_addr, uint8_t reg_data)
{
    uint8_t buf[1] = { reg_data };
    i2c_write_data(AW9523_ADDRESS, reg_addr, buf, 1);
}

/**
 * @brief Open settings from the NVS partition.
 * 
 * @return True if successful. False is unsuccessful.
 */
bool open_settings()
{
    esp_err_t result = nvs_open("settings", NVS_READWRITE, &settings_handle);

    if (result == ESP_OK) return true;
    else
    {
        send_usb_packet(Bus_USB, Command_Error, Error_ESP32NVSOpen, err, sizeof(err));
        return false;
    }
}

/**
 * @brief Save last changes and close settings in the NVS partition.
 */
void close_settings()
{
    nvs_commit(settings_handle);
    nvs_close(settings_handle);
}

/**
 * @brief Write default settings to the NVS partition.
 */
void default_settings()
{
    if (open_settings())
    {
        nvs_set_u16(settings_handle, "hb_interval", 5000); // ms
        nvs_set_u16(settings_handle, "blink_duration", 50); // ms
        nvs_set_u8(settings_handle, "ccd_settings", 0x41);
        nvs_set_u8(settings_handle, "pci_settings", 0x40);
        nvs_set_u8(settings_handle, "sci_settings", 0x81);
        close_settings();
        send_usb_packet(Bus_USB, Command_Error, Error_OK, ack, sizeof(ack));
    }
}

/**
 * @brief Initialize I2C communication.
 */
void init_i2c()
{
    // Setup I2C communication.
    i2c_param_config(I2C_CHANNEL, &i2c_config);
    i2c_driver_install(I2C_CHANNEL, i2c_config.mode, I2C_MASTER_RX_BUF_DISABLE, I2C_MASTER_TX_BUF_DISABLE, 0);

    // Configure AW9523B I2C port expander chip.
    uint8_t result[1] = { aw9523_read_byte(AW9523_ID_REG) }; // read chip ID

    if (result[0] != AW9523_CHIP_ID)
    {
        send_usb_packet(Bus_USB, Command_Error, Error_I2CPortExpanderNotFound, result, sizeof(result));
        return;
    }

    aw9523_write_byte(AW9523_CONFIG_PORT0_REG, OUTPUT_ALL); // set all P0 pins to output
    aw9523_write_byte(AW9523_CONFIG_PORT1_REG, OUTPUT_ALL); // set all P1 pins to output
    aw9523_write_byte(AW9523_OUTPUT_PORT0_REG, LOW_ALL); // set all P0 pins to low output state
    aw9523_write_byte(AW9523_OUTPUT_PORT1_REG, LOW_ALL); // set all P1 pins to low output state
    aw9523_write_byte(AW9523_GCR_REG, P0_PUSH_PULL); // set push-pull output mode for all P0 pins
    aw9523_write_byte(AW9523_MODE_PORT0_REG, GPIO_MODE_ALL); // set all P0 pins to GPIO mode
    aw9523_write_byte(AW9523_MODE_PORT1_REG, GPIO_MODE_ALL); // set all P1 pins to GPIO mode
    aw9523_write_byte(AW9523_INT_PORT0_REG, INTERRUPT_DISABLE_ALL); // disable interrupts on all P0 pins
    aw9523_write_byte(AW9523_INT_PORT1_REG, INTERRUPT_DISABLE_ALL); // disable interrupts on all P1 pins
}

/**
 * @brief Configure CCD-bus settings.
 * 
 * @param settings Settings to apply.
 */
void configure_ccd_bus(uint8_t settings)
{
    // B7: state bit: 0: disabled
    //                1: enabled
    // B6: tb_en bit: 0: disabled (CCD-bus termination and bias)
    //                1: enabled
    // B5: not used:  0: always clear
    // B4: not used:  0: always clear
    // B3: not used:  0: always clear
    // B2: not used:  0: always clear
    // B1: not used:  0: always clear
    // B0: not used:  1: always set

    cbi(settings, 5);
    cbi(settings, 4);
    cbi(settings, 3);
    cbi(settings, 2);
    cbi(settings, 1);
    sbi(settings, 0);

    ccd.state.en = settings & (1 << CCD_STATE_BIT);

    uint8_t pin_state = aw9523_read_byte(AW9523_INPUT_PORT1_REG);

    if (settings & (1 << CCD_TBEN_BIT))
    {
        ccd.state.tb_en = true;
        cbi(pin_state, CCD_TBEN_PIN); // 0=enabled, 1=disabled
    }
    else
    {
        ccd.state.tb_en = false;
        sbi(pin_state, CCD_TBEN_PIN); // 0=enabled, 1=disabled
    }

    aw9523_write_byte(AW9523_OUTPUT_PORT1_REG, pin_state);

    ccd.bus_settings = settings;

    if (open_settings())
    {
        nvs_set_u8(settings_handle, "ccd_settings", ccd.bus_settings);
        close_settings();
    }

    uint8_t ret[1] = { ccd.bus_settings };
    send_usb_packet(Bus_USB, Command_Settings, Settings_CCD, ret, sizeof(ret));
}

/**
 * @brief Configure SCI-bus settings.
 * 
 * @param settings Settings to apply.
 */
void configure_sci_bus(uint8_t settings)
{
    // B7: state bit:     0: disabled
    //                    1: enabled
    // B6: not used:      0: always clear
    // B5: module bit:    0: PCM (engine)
    //                    1: TCM (transmission)
    // B4: ngc bit:       0: NGC mode off
    //                    1: NGC mode on
    // B3: logic bit:     0: non-inverted
    //                    1: inverted
    // B2: config bit:    0: A-configuration
    //                    1: B-configuration
    // B1:0: speed bits: 00: 976.5 baud
    //                   01: 7812.5 baud
    //                   10: 62500 baud
    //                   11: 125000 baud

    cbi(settings, 6);

    sci.state.en = settings & (1 << SCI_STATE_BIT);

    if (settings & (1 << SCI_NGC_BIT))
    {
        sci.state.ngc_mode = true;
    }
    else
    {
        sci.state.ngc_mode = false;
    }

    uint8_t pin_state = aw9523_read_byte(AW9523_INPUT_PORT1_REG) & 0x80; // keep leftmost bit (CCD TBEN)

    if (sci.state.en)
    {
        if (settings & (1 << SCI_MODULE_BIT)) // TCM
        {
            if (settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                sbi(pin_state, SCI_RX9_EN_PIN);
                sbi(pin_state, SCI_TX15_EN_PIN);
            }
            else // A-configuration
            {
                sbi(pin_state, SCI_RX14_EN_PIN);
                sbi(pin_state, SCI_TX7_EN_PIN);
            }
        }
        else // PCM
        {
            if (settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                sbi(pin_state, SCI_RX12_EN_PIN);
                sbi(pin_state, SCI_TX7_EN_PIN);
            }
            else // A-configuration
            {
                sbi(pin_state, SCI_RX6_EN_PIN);
                sbi(pin_state, SCI_TX7_EN_PIN);
            }
        }
    }

    aw9523_write_byte(AW9523_OUTPUT_PORT1_REG, pin_state);

    if (settings & (1 << SCI_LOGIC_BIT)) // inverted logic
    {
        sci.state.inverted_logic = true;
        uart_set_line_inverse(UART_SCI, UART_SIGNAL_RXD_INV | UART_SIGNAL_TXD_INV);
    }
    else // non-inverted logic
    {
        sci.state.inverted_logic = false;
        uart_set_line_inverse(UART_SCI, UART_SIGNAL_INV_DISABLE);
    }

    switch (settings & SCI_SPEED_BITS)
    {
        case SCI_SPEED_976_BAUD:
        {
            sci_config.baud_rate = 976;
            break;
        }
        case SCI_SPEED_7812_BAUD:
        {
            sci_config.baud_rate = 7812;
            break;
        }
        case SCI_SPEED_62500_BAUD:
        {
            sci_config.baud_rate = 62500;
            break;
        }
        case SCI_SPEED_125000_BAUD:
        {
            sci_config.baud_rate = 125000;
            break;
        }
    }

    uart_set_baudrate(UART_SCI, sci_config.baud_rate);

    sci.bus_settings = settings;

    if (open_settings())
    {
        nvs_set_u8(settings_handle, "sci_settings", sci.bus_settings);
        close_settings();
    }

    uint8_t ret[1] = { sci.bus_settings };
    send_usb_packet(Bus_USB, Command_Settings, Settings_SCI, ret, sizeof(ret));
}

/**
 * @brief Apply/Remove programming voltage to/from SCI-RX pin.
 * 
 * @note Correct SCI-RX pin is selected from current settings.
 * 
 * @param progvolt_state VBB or VPP and debug output selection.
 */
void apply_progvolt(uint8_t progvolt_state)
{
    // B7: vbb bit:    0: disabled
    //                 1: enabled
    // B6: vpp bit:    0: disabled
    //                 1: enabled
    // B5: aw9523 bit: 0: no debug output
    //                 1: send raw input register states (P0 and P1)
    // B4:0: not used: 00000: always clear
    //
    // VBB = 12V to put controller in bootstrap mode
    // VPP = 20V to erase/program flash memory

    progvolt_state &= 0xE0; // keep upper 3 bits

    uint8_t pin_state = 0;

    // Select correct RX-pin based on module and configuration bits.
    if (progvolt_state & (1 << SCI_VBB_EN_BIT)) // VBB on
    {
        if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
        {
            if (sci.bus_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                pin_state = (1 << SCI_VBB_TO_RX9_PIN);
            }
            else // A-configuration
            {
                pin_state = (1 << SCI_VBB_TO_RX14_PIN);
            }
        }
        else // PCM
        {
            if (sci.bus_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                pin_state = (1 << SCI_VBB_TO_RX12_PIN);
            }
            else // A-configuration
            {
                pin_state = (1 << SCI_VBB_TO_RX6_PIN);
            }
        }
    }
    else if (progvolt_state & (1 << SCI_VPP_EN_BIT)) // VPP on
    {
        if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
        {
            if (sci.bus_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                pin_state = (1 << SCI_VPP_TO_RX9_PIN);
            }
            else // A-configuration
            {
                pin_state = (1 << SCI_VPP_TO_RX14_PIN);
            }
        }
        else // PCM
        {
            if (sci.bus_settings & (1 << SCI_CONFIG_BIT)) // B-configuration
            {
                pin_state = (1 << SCI_VPP_TO_RX12_PIN);
            }
            else // A-configuration
            {
                pin_state = (1 << SCI_VPP_TO_RX6_PIN);
            }
        }
    }
    else // VBB/VPP off
    {
        pin_state = 0;
    }

    aw9523_write_byte(AW9523_OUTPUT_PORT0_REG, pin_state);

    if (progvolt_state & (1 << SCI_AW9523_LOG_BIT))
    {
        uint8_t ret[2] = { aw9523_read_byte(AW9523_INPUT_PORT0_REG), aw9523_read_byte(AW9523_INPUT_PORT1_REG) };
        send_usb_packet(Bus_USB, Command_Debug, Debug_GetAW9523Data, ret, sizeof(ret));
    }

    uint8_t ret[1] = { progvolt_state };
    send_usb_packet(Bus_USB, Command_Settings, Settings_ProgVolt, ret, sizeof(ret));
}

/**
 * @brief Configure PCI-bus settings.
 * 
 * @param settings Settings to apply.
 */
void configure_pci_bus(uint8_t settings)
{
    // B7: state bit:        0: disabled
    //                       1: enabled
    // B6: active level bit: 0: active low bus
    //                       1: active high bus
    // B5: not used:         0: always clear
    // B4: not used:         0: always clear
    // B3: not used:         0: always clear
    // B2: not used:         0: always clear
    // B1: not used:         0: always clear
    // B0: not used:         1: always clear

    settings &= 0xC0; // keep upper two bits

    pci.state.en = settings & (1 << PCI_STATE_BIT);

    if (settings & (1 << PCI_ACTIVE_LEVEL_BIT)) // active high bus
    {
        pci.state.active_bus_level = 1;
        pci.state.passive_bus_level = 0;
    }
    else // active low bus
    {
        pci.state.active_bus_level = 0;
        pci.state.passive_bus_level = 1;
    }

    pci.bus_settings = settings;

    if (open_settings())
    {
        nvs_set_u8(settings_handle, "pci_settings", pci.bus_settings);
        close_settings();
    }

    uint8_t ret[1] = { pci.bus_settings };
    send_usb_packet(Bus_USB, Command_Settings, Settings_PCI, ret, sizeof(ret));
}

/**
 * @brief Initialize ESP32's ADC to measure battery/bootstrap/programming voltages.
 */
void init_adc()
{
    adc1_config_width(adc_width);
    adc1_config_channel_atten(adc_battvolt_channel, adc_atten);
    adc1_config_channel_atten(adc_bootvolt_channel, adc_atten);
    adc1_config_channel_atten(adc_progvolt_channel, adc_atten);
    adc_characteristics = calloc(1, sizeof(esp_adc_cal_characteristics_t));
    esp_adc_cal_characterize(adc_unit, adc_atten, adc_width, DEFAULT_VREF, adc_characteristics);
}

/**
 * @brief Read input voltage on the specified ADC-channel.
 * 
 * @param adc_channel ADC channel.
 * 
 * @return Voltage in millivolts on the specified ADC-channel.
 */
uint32_t read_millivolts(adc1_channel_t adc_channel)
{
    uint32_t raw_millivolts = 0;
    uint32_t millivolts = 0;
    esp_adc_cal_get_voltage(adc_channel, adc_characteristics, &raw_millivolts);

    switch (adc_channel)
    {
        case BATTVOLT_ADC_CHANNEL:
        {
            millivolts = raw_millivolts * BATTVOLT_SCALER;
            break;
        }
        case BOOTVOLT_ADC_CHANNEL:
        {
            millivolts = raw_millivolts * BOOTVOLT_SCALER;
            break;
        }
        case PROGVOLT_ADC_CHANNEL:
        {
            millivolts = raw_millivolts * PROGVOLT_SCALER;
            break;
        }
        default:
        {
            millivolts = 0;
        }
    }

    return millivolts;
}

/**
 * @brief Analyze received USB packet and execute requests.
 */
void parse_usb_command()
{
    uint8_t bus = (usb_packet.datacode >> 4) & 0x07; // keep 3 bits (0xxx0000 -> 00000xxx)
    uint8_t command = usb_packet.datacode & 0x0F; // keep 4 bits (0000yyyy)

    switch (bus)
    {
        case Bus_USB:
        {
            switch (command)
            {
                case Command_Reset:
                {
                    send_usb_packet(Bus_USB, Command_Reset, Reset_InProgress, ack, sizeof(ack));
                    esp_restart();
                    break;
                }
                case Command_Handshake:
                {
                    send_handshake();
                    break;
                }
                case Command_Status:
                {
                    send_status();
                    break;
                }
                case Command_Settings:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Settings_Heartbeat:
                        {
                            if (usb_packet.payload_length < 4) // at least 4 bytes are necessary to change this setting
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            heartbeat_interval = to_uint16(usb_packet.payload[0], usb_packet.payload[1]); // 0-65535 milliseconds

                            if (heartbeat_interval == 0) heartbeat_enabled = false; // zero value is allowed, meaning no heartbeat
                            else heartbeat_enabled = true;
                            
                            led_blink_duration = to_uint16(usb_packet.payload[2], usb_packet.payload[3]); // 0-65535 milliseconds

                            if (led_blink_duration > 0)
                            {
                                leds_enabled = true;
                                gpio_set_level(PWR_LED_PIN, LED_ON); // turn on PWR LED
                            }
                            else
                            {
                                leds_enabled = false;
                                gpio_set_level(PWR_LED_PIN, LED_OFF); // turn off PWR LED
                            }

                            if (open_settings())
                            {
                                nvs_set_u16(settings_handle, "hb_interval", heartbeat_interval); // ms
                                nvs_set_u16(settings_handle, "blink_duration", led_blink_duration); // ms
                                close_settings();
                            }

                            uint8_t ret[5] = { usb_packet.payload[0], usb_packet.payload[1], usb_packet.payload[2], usb_packet.payload[3] };
                            send_usb_packet(Bus_USB, Command_Settings, Settings_Heartbeat, ret, sizeof(ret));
                            break;
                        }

                        case Settings_CCD:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            configure_ccd_bus(usb_packet.payload[0]);
                            break;
                        }
                        case Settings_SCI:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            configure_sci_bus(usb_packet.payload[0]);
                            break;
                        }
                        case Settings_Repeat:
                        {
                            if (usb_packet.payload_length < 3)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            switch (usb_packet.payload[0])
                            {
                                case 0x01: // CCD-bus
                                {
                                    ccd.repeat.interval = to_uint16(usb_packet.payload[1], usb_packet.payload[2]); // 0-65535 milliseconds
                                    break;
                                }
                                case 0x02: // SCI-bus (PCM)
                                case 0x03: // SCI-bus (TCM)
                                {
                                    sci.repeat.interval = to_uint16(usb_packet.payload[1], usb_packet.payload[2]); // 0-65535 milliseconds
                                    break;
                                }
                                case 0x04: // PCI-bus
                                {
                                    pci.repeat.interval = to_uint16(usb_packet.payload[1], usb_packet.payload[2]); // 0-65535 milliseconds
                                    break;
                                }
                                default:
                                {
                                    send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                    break;
                                }
                            }

                            uint8_t ret[3] = { usb_packet.payload[0], usb_packet.payload[1], usb_packet.payload[2] };
                            send_usb_packet(Bus_USB, Command_Settings, Settings_Repeat, ret, sizeof(ret));
                            break;
                        }
                        case Settings_PCI:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            configure_pci_bus(usb_packet.payload[0]);
                            break;
                        }
                        case Settings_ProgVolt:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            apply_progvolt(usb_packet.payload[0]);
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                case Command_Request:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Request_Info:
                        {
                            send_hw_info();
                            break;
                        }
                        case Request_Timestamp:
                        {
                            uint8_t ret[4] = { (current_millis >> 24) & 0xFF, (current_millis >> 16) & 0xFF, (current_millis >> 8) & 0xFF, current_millis & 0xFF };
                            send_usb_packet(Bus_USB, Command_Response, Request_Timestamp, ret, sizeof(ret));
                            break;
                        }
                        case Request_BatteryVolts:
                        {
                            uint32_t battery_millivolts = read_millivolts(BATTVOLT_ADC_CHANNEL);
                            uint8_t ret[2] = { (battery_millivolts >> 8) & 0xFF, battery_millivolts & 0xFF };
                            send_usb_packet(Bus_USB, Command_Response, Request_BatteryVolts, ret, sizeof(ret));
                            break;
                        }
                        case Request_VBBVolts:
                        {
                            uint32_t bootstrap_millivolts = read_millivolts(BOOTVOLT_ADC_CHANNEL);
                            uint8_t ret[2] = { (bootstrap_millivolts >> 8) & 0xFF, bootstrap_millivolts & 0xFF };
                            send_usb_packet(Bus_USB, Command_Response, Request_VBBVolts, ret, sizeof(ret));
                            break;
                        }
                        case Request_VPPVolts:
                        {
                            uint32_t programming_millivolts = read_millivolts(PROGVOLT_ADC_CHANNEL);
                            uint8_t ret[2] = { (programming_millivolts >> 8) & 0xFF, programming_millivolts & 0xFF };
                            send_usb_packet(Bus_USB, Command_Response, Request_VPPVolts, ret, sizeof(ret));
                            break;
                        }
                        case Request_AllVolts:
                        {
                            uint32_t battery_millivolts = read_millivolts(BATTVOLT_ADC_CHANNEL);
                            uint32_t bootstrap_millivolts = read_millivolts(BOOTVOLT_ADC_CHANNEL);
                            uint32_t programming_millivolts = read_millivolts(PROGVOLT_ADC_CHANNEL);
                            uint8_t ret[6] = { (battery_millivolts >> 8) & 0xFF, battery_millivolts & 0xFF, (bootstrap_millivolts >> 8) & 0xFF, bootstrap_millivolts & 0xFF, (programming_millivolts >> 8) & 0xFF, programming_millivolts & 0xFF };
                            send_usb_packet(Bus_USB, Command_Response, Request_AllVolts, ret, sizeof(ret));
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                case Command_Debug:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Debug_RandomCCDMessages:
                        {
                            if (usb_packet.payload_length < 5)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            switch (usb_packet.payload[0])
                            {
                                case 0x00: // off
                                {
                                    ccd.random.en = false;
                                    ccd.msg.tx_count = 0;
                                    ccd.random.interval_min = 0;
                                    ccd.random.interval_max = 0;
                                    ccd.random.interval = 0;

                                    uint8_t ret[1] = { usb_packet.payload[0] };
                                    send_usb_packet(Bus_USB, Command_Debug, Debug_RandomCCDMessages, ret, sizeof(ret));
                                    break;
                                }
                                case 0x01: // on
                                {
                                    ccd.random.interval_min = to_uint16(usb_packet.payload[1], usb_packet.payload[2]);
                                    ccd.random.interval_max = to_uint16(usb_packet.payload[3], usb_packet.payload[4]);
                                    ccd.random.interval = get_random(ccd.random.interval_min, ccd.random.interval_max);
                                    ccd.msg.tx_count = 1;
                                    ccd.random.en = true;

                                    uint8_t ret[1] = { usb_packet.payload[0] };
                                    send_usb_packet(Bus_USB, Command_Debug, Debug_RandomCCDMessages, ret, sizeof(ret));
                                    break;
                                }
                                default:
                                {
                                    send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                    break;
                                }
                            }
                            break;
                        }
                        case Debug_InitBootstrapMode:
                        {
                            if (usb_packet.payload_length < 2)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            sci.bootstrap.bootloader_src = usb_packet.payload[0];
                            sci.bootstrap.flash_chip_src = usb_packet.payload[1];
                            sci.bootstrap.upload_bootloader = true;
                            break;
                        }
                        case Debug_UploadWorkerFunction:
                        {
                            if (usb_packet.payload_length < 2)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            sci.bootstrap.worker_function_src = usb_packet.payload[0];
                            sci.bootstrap.flash_chip_src = usb_packet.payload[1];
                            sci.bootstrap.upload_worker_function = true;
                            break;
                        }
                        case Debug_StartWorkerFunction:
                        {
                            if (usb_packet.payload_length < 2)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            sci.bootstrap.worker_function_src = usb_packet.payload[0];
                            sci.bootstrap.flash_chip_src = usb_packet.payload[1];
                            sci.bootstrap.start_worker_function = true;
                            break;
                        }
                        case Debug_ExitWorkerFunction:
                        {
                            if (usb_packet.payload_length < 2)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            sci.bootstrap.worker_function_src = usb_packet.payload[0];
                            sci.bootstrap.flash_chip_src = usb_packet.payload[1];
                            sci.bootstrap.exit_worker_function = true;
                            break;
                        }
                        case Debug_DefaultSettings:
                        {
                            default_settings();
                            break;
                        }
                        case Debug_GetRandomNumber:
                        {
                            if (usb_packet.payload_length < 4)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            uint16_t min = to_uint16(usb_packet.payload[0], usb_packet.payload[1]);
                            uint16_t max = to_uint16(usb_packet.payload[2], usb_packet.payload[3]);
                            uint16_t random_number = get_random(min, max);
                            uint8_t ret[2] = { (random_number >> 8) & 0xFF, random_number & 0xFF };
                            send_usb_packet(Bus_USB, Command_Debug, Debug_GetRandomNumber, ret, sizeof(ret));
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Datacode, err, sizeof(err));
                    break;
                }
            }
            break;
        }
        case Bus_CCD:
        {
            switch (command)
            {
                case Command_Transmit:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Transmit_Stop:
                        {
                            ccd.repeat.en = false;
                            ccd.repeat.next = false;
                            ccd.repeat.list_once = false;
                            ccd.repeat.stop = true;
                            ccd.msg.tx_count = 0;
                            ccd.msg.tx_count_ptr = 0;
                            ccd.repeat.last_millis = 0;
                            ccd.msg.tx_ptr = 0;

                            uint8_t ret[1] = { Bus_CCD };
                            send_usb_packet(Bus_USB, Command_Transmit, Transmit_Stop, ret, sizeof(ret));
                            break;
                        }
                        case Transmit_Single:
                        case Transmit_RepeatSingle:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            for (uint16_t i = 0; i < usb_packet.payload_length; i++)
                            {
                                ccd.msg.tx_buffer[i] = usb_packet.payload[i];
                            }

                            ccd.msg.tx_length = usb_packet.payload_length;
                            
                            // Checksum is only applicable if message is at least 2 bytes long.
                            if (ccd.msg.tx_length > 1)
                            {
                                uint8_t checksum_position = ccd.msg.tx_length - 1;
                                ccd.msg.tx_buffer[checksum_position] = calculate_checksum(ccd.msg.tx_buffer, 0, checksum_position); // overwrite last checksum byte with the correct one
                            }

                            ccd.msg.tx_count = 1;

                            if (usb_packet.subdatacode == Transmit_RepeatSingle)
                            {
                                ccd.repeat.en = true;
                                ccd.repeat.next = true;
                            }
                            else
                            {
                                ccd.repeat.en = false;
                                ccd.repeat.next = false;
                                ccd.state.msg_tx_pending = true;
                            }

                            ccd.repeat.list_once = false;
                            ccd.repeat.stop = false;
                            break;
                        }
                        case Transmit_List:
                        case Transmit_RepeatList:
                        {
                            if (usb_packet.payload_length < 3)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            // Payload structure example:
                            // 02 04 E4 00 00 E4 04 24 00 00 24
                            // -----------------------------------
                            // 02: number of messages
                            // 04: 1st message length
                            // E4 00 00 E4: message #1
                            // 04: 2nd message length
                            // 24 00 00 24: message #2
                            // XX: n-th message length
                            // XX XX...: message #n

                            for (int i = 1; i < usb_packet.payload_length; i++)
                            {
                                ccd.msg.tx_buffer[i - 1] = usb_packet.payload[i]; // copy and save all the message bytes for this session
                            }

                            ccd.msg.tx_count = usb_packet.payload[0]; // save number of messages
                            ccd.msg.tx_count_ptr = 0; // current message to transmit
                            ccd.msg.tx_length = usb_packet.payload[1]; // first message length is saved
                            ccd.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning

                            for (int i = 0; i < ccd.msg.tx_count; i++)
                            {
                                uint8_t current_msg_length = ccd.msg.tx_buffer[ccd.msg.tx_ptr];

                                if (current_msg_length > 1) // checksum is only applicable if message is at least 2 bytes long
                                {
                                    uint8_t current_checksum_position = ccd.msg.tx_ptr + current_msg_length;
                                    ccd.msg.tx_buffer[current_checksum_position] = calculate_checksum(ccd.msg.tx_buffer, ccd.msg.tx_ptr + 1, current_checksum_position); // re-calculate every checksum byte
                                }

                                ccd.msg.tx_ptr += current_msg_length + 1;
                            }

                            ccd.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning
                            ccd.repeat.en = true; // set flag
                            ccd.repeat.next = true; // set flag

                            if (usb_packet.subdatacode == Transmit_List) ccd.repeat.list_once = true;
                            else if (usb_packet.subdatacode == Transmit_RepeatList) ccd.repeat.list_once = false;

                            ccd.repeat.stop = false;
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Datacode, err, sizeof(err));
                    break;
                }
            }
            break;
        }
        case Bus_PCM: // SCI
        case Bus_TCM: // SCI
        {
            switch (command)
            {
                case Command_Transmit:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Transmit_Stop:
                        {
                            sci.repeat.en = false;
                            sci.repeat.next = false;
                            sci.repeat.list_once = false;
                            sci.repeat.stop = true;
                            sci.msg.tx_count = 0;
                            sci.msg.tx_count_ptr = 0;
                            sci.repeat.last_millis = 0;
                            sci.msg.tx_ptr = 0;

                            uint8_t ret[1] = { bus };
                            send_usb_packet(Bus_USB, Command_Transmit, Transmit_Stop, ret, sizeof(ret));
                            break;
                        }
                        case Transmit_Single:
                        case Transmit_RepeatSingle:
                        case Transmit_Single_VPP:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            if (sci.state.inverted_logic) // last 4 bits come first, then first 4 bits
                            {
                                for (int i = 0; i < usb_packet.payload_length; i++)
                                {
                                    sci.msg.tx_buffer[i] = ((usb_packet.payload[i] << 4) & 0xF0) | ((usb_packet.payload[i] >> 4) & 0x0F); // re-arrange bits
                                }
                            }
                            else
                            {
                                for (int i = 0; i < usb_packet.payload_length; i++)
                                {
                                    sci.msg.tx_buffer[i] = usb_packet.payload[i];
                                }
                            }

                            sci.msg.tx_length = usb_packet.payload_length;
                            sci.msg.tx_count = 1;

                            if (usb_packet.subdatacode == Transmit_RepeatSingle)
                            {
                                sci.repeat.en = true;
                                sci.repeat.next = true;
                            }
                            else
                            {
                                sci.repeat.en = false;
                                sci.repeat.next = false;
                            }

                            sci.repeat.list_once = false;
                            sci.repeat.stop = false;
                            sci.state.msg_tx_pending = true;

                            if (usb_packet.subdatacode == Transmit_Single_VPP)
                            {
                                sci.bootstrap.progvolts_request = true;
                            }
                            break;
                        }
                        case Transmit_List:
                        case Transmit_RepeatList:
                        {
                            if (usb_packet.payload_length < 3)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            // Payload structure example:
                            // 02 02 14 07 02 14 08
                            // --------------------------
                            // 02: number of messages
                            // 02: 1st message length
                            // 14 07: message #1
                            // 02: 2nd message length
                            // 14 08: message #2
                            // XX: n-th message length
                            // XX XX...: message #n

                            for (int i = 1; i < usb_packet.payload_length; i++)
                            {
                                sci.msg.tx_buffer[i - 1] = usb_packet.payload[i]; // copy and save all the message bytes for this session
                            }

                            sci.msg.tx_count = usb_packet.payload[0]; // save number of messages
                            sci.msg.tx_count_ptr = 0; // current message to transmit
                            sci.msg.tx_length = usb_packet.payload[1]; // first message length is saved
                            sci.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning

                            if (sci.state.inverted_logic) // last 4 bits come first, then first 4 bits
                            {
                                for (int i = 0; i < sci.msg.tx_count; i++)
                                {
                                    uint8_t current_msg_length = sci.msg.tx_buffer[sci.msg.tx_ptr];
                                    
                                    for (int j = 0; j < current_msg_length; j++)
                                    {
                                        uint8_t raw = sci.msg.tx_buffer[sci.msg.tx_ptr + 1 + j];
                                        sci.msg.tx_buffer[sci.msg.tx_ptr + 1 + j] = ((raw << 4) & 0xF0) | ((raw >> 4) & 0x0F); // re-arrange bits
                                    }

                                    sci.msg.tx_ptr += current_msg_length + 1;
                                }
                            }

                            sci.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning
                            sci.repeat.en = true; // set flag
                            sci.repeat.next = true; // set flag

                            if (usb_packet.subdatacode == Transmit_List) sci.repeat.list_once = true;
                            else if (usb_packet.subdatacode == Transmit_RepeatList) sci.repeat.list_once = false;

                            sci.repeat.stop = false;
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Datacode, err, sizeof(err));
                    break;
                }
            }
            break;
        }
        case Bus_PCI:
        {
            switch (command)
            {
                case Command_Transmit:
                {
                    switch (usb_packet.subdatacode)
                    {
                        case Transmit_Stop:
                        {
                            pci.repeat.en = false;
                            pci.repeat.next = false;
                            pci.repeat.list_once = false;
                            pci.repeat.stop = true;
                            pci.msg.tx_count = 0;
                            pci.msg.tx_count_ptr = 0;
                            pci.repeat.last_millis = 0;
                            pci.msg.tx_ptr = 0;

                            uint8_t ret[1] = { Bus_PCI };
                            send_usb_packet(Bus_USB, Command_Transmit, Transmit_Stop, ret, sizeof(ret));
                            break;
                        }
                        case Transmit_Single:
                        case Transmit_RepeatSingle:
                        {
                            if (usb_packet.payload_length == 0)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            for (int i = 0; i < usb_packet.payload_length; i++)
                            {
                                pci.msg.tx_buffer[i] = usb_packet.payload[i];
                            }

                            pci.msg.tx_length = usb_packet.payload_length;

                            // Checksum is only applicable if message is at least 2 bytes long.
                            if (pci.msg.tx_length > 1)
                            {
                                uint8_t crc_position = pci.msg.tx_length - 1;
                                pci.msg.tx_buffer[crc_position] = calculate_crc(pci.msg.tx_buffer, 0, crc_position); // overwrite last byte with the correct CRC byte
                            }

                            pci.msg.tx_count = 1;

                            if (usb_packet.subdatacode == Transmit_RepeatSingle)
                            {
                                pci.repeat.en = true;
                                pci.repeat.next = true;
                            }
                            else
                            {
                                pci.repeat.en = false;
                                pci.repeat.next = false;
                                pci.state.msg_tx_pending = true;
                            }

                            pci.repeat.list_once = false;
                            pci.repeat.stop = false;
                            break;
                        }
                        case Transmit_List:
                        case Transmit_RepeatList:
                        {
                            if (usb_packet.payload_length < 3)
                            {
                                send_usb_packet(Bus_USB, Command_Error, Error_Payload, err, sizeof(err));
                                break;
                            }

                            // Payload structure example:
                            // 02 07 10 10 BD 00 00 23 B9 04 CC 4F 25 FC
                            // -----------------------------------------
                            // 02: number of messages
                            // 07: 1st message length
                            // 10 10 BD 00 00 23 B9: message #1
                            // 04: 2nd message length
                            // CC 4F 25 FC: message #2
                            // XX: n-th message length
                            // XX XX...: message #n

                            for (int i = 1; i < usb_packet.payload_length; i++)
                            {
                                pci.msg.tx_buffer[i - 1] = usb_packet.payload[i]; // copy and save all the message bytes for this session
                            }

                            pci.msg.tx_count = usb_packet.payload[0]; // save number of messages
                            pci.msg.tx_count_ptr = 0; // current message to transmit
                            pci.msg.tx_length = usb_packet.payload[1]; // first message length is saved
                            pci.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning

                            for (uint8_t i = 0; i < pci.msg.tx_count; i++)
                            {
                                uint8_t current_msg_length = pci.msg.tx_buffer[pci.msg.tx_ptr];

                                if (current_msg_length > 1) // checksum is only applicable if message is at least 2 bytes long
                                {
                                    uint8_t current_crc_position = pci.msg.tx_ptr + current_msg_length;
                                    pci.msg.tx_buffer[current_crc_position] = calculate_crc(pci.msg.tx_buffer, pci.msg.tx_ptr + 1, current_crc_position); // re-calculate every checksum byte
                                }

                                pci.msg.tx_ptr += current_msg_length + 1;
                            }

                            pci.msg.tx_ptr = 0; // set the pointer in the main buffer at the beginning
                            pci.repeat.en = true; // set flag
                            pci.repeat.next = true; // set flag

                            if (usb_packet.subdatacode == Transmit_List) pci.repeat.list_once = true;
                            else if (usb_packet.subdatacode == Transmit_RepeatList) pci.repeat.list_once = false;

                            pci.repeat.stop = false;
                            break;
                        }
                        default:
                        {
                            send_usb_packet(Bus_USB, Command_Error, Error_Subdatacode, err, sizeof(err));
                            break;
                        }
                    }
                    break;
                }
                default:
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Datacode, err, sizeof(err));
                    break;
                }
            }
            break;
        }
        default:
        {
            send_usb_packet(Bus_USB, Command_Error, Error_InvalidBus, err, sizeof(err));
            break;
        }
    }
}

/**
 * @brief Task to handle and store received USB data to a ringbuffer.
 * 
 * @param pvParameters Unused parameters.
 */
void usb_event_task(void *pvParameters)
{
    usb_rx_ringbuf_handle = xRingbufferCreate(USB_RX_BUF_SIZE, RINGBUF_TYPE_BYTEBUF); // create USB ringbuffer

    uart_event_t event;

    for (EVER)
    {
        if (xQueueReceive(usb_uart_queue, (void *)&event, pdMS_TO_TICKS(50))) // look out for events happening
        {
            switch (event.type)
            {
                case UART_DATA:
                {
                    uint16_t length = event.size; // save length
                    uart_read_bytes(UART_USB, usb_rx_buf, length, pdMS_TO_TICKS(50)); // read bytes to a simple byte buffer
                    xRingbufferSend(usb_rx_ringbuf_handle, usb_rx_buf, length, pdMS_TO_TICKS(50)); // add received bytes to the ring buffer
                    //send_usb_packet(Bus_USB, Command_Error, Error_Timeout, usb_rx_buf, length);
                    break;
                }
                case UART_FIFO_OVF:
                case UART_BUFFER_FULL:
                {
                    uart_flush_input(UART_USB);
                    xQueueReset(usb_uart_queue);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    vTaskDelete(NULL);
}

/**
 * @brief Get number of bytes waiting in the USB ringbuffer.
 */
uint16_t usb_rx_available()
{
    return (USB_RX_BUF_SIZE - xRingbufferGetCurFreeSize(usb_rx_ringbuf_handle));
}

/**
 * @brief Task to read USB data from the ringbuffer and start analyzing it.
 * 
 * @param pvParameters Unused parameters.
 */
void usb_msg_task(void *pvParameters)
{
    size_t item_size;
    uint8_t *item;
    size_t item_size2;
    uint8_t *item2;
    uint8_t rx_buf[USB_RX_BUF_SIZE];
    uint16_t timeout = 0;

    for (EVER)
    {
        begin:

        if (usb_rx_available() >= 3)
        {
            blink_led(RX_LED_PIN);

            item = (uint8_t *)xRingbufferReceiveUpTo(usb_rx_ringbuf_handle, &item_size, pdMS_TO_TICKS(50), 3);

            if (item == NULL)
            {
                send_usb_packet(Bus_USB, Command_Error, Error_Timeout, err, sizeof(err));
                goto begin;
            }

            for (int i = 0; i < item_size; i++)
            {
                rx_buf[i] = item[i];
            }

            vRingbufferReturnItem(usb_rx_ringbuf_handle, (void *)item);

            if (item_size < 3) // data wrapped around
            {
                item2 = (uint8_t *)xRingbufferReceiveUpTo(usb_rx_ringbuf_handle, &item_size2, pdMS_TO_TICKS(50), (3 - item_size));

                if (item2 == NULL)
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Timeout, err, sizeof(err));
                    goto begin;
                }

                for (int i = 0; i < item_size2; i++)
                {
                    rx_buf[item_size + i] = item2[i];
                }

                vRingbufferReturnItem(usb_rx_ringbuf_handle, (void *)item2);
            }

            if (rx_buf[0] != PACKET_SYNC_BYTE)
            {
                goto begin;
            }

            usb_packet.sync = PACKET_SYNC_BYTE;
            usb_packet.length = to_uint16(rx_buf[1], rx_buf[2]);
            usb_packet.payload_length = usb_packet.length - 2;

            timeout = 0;

            while (usb_rx_available() < (usb_packet.payload_length + 3)) // wait for datacode, subdatacode, payload (if any) and checksum bytes as well
            {
                vTaskDelay(pdMS_TO_TICKS(1));
                timeout++;

                if (timeout >= 200)
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Timeout, err, sizeof(err));
                    goto begin;
                }
            }

            item = (uint8_t *)xRingbufferReceiveUpTo(usb_rx_ringbuf_handle, &item_size, pdMS_TO_TICKS(50), (usb_packet.payload_length + 3));

            if (item == NULL)
            {
                send_usb_packet(Bus_USB, Command_Error, Error_Timeout, err, sizeof(err));
                goto begin;
            }

            for (int i = 0; i < item_size; i++)
            {
                rx_buf[i] = item[i];
            }

            vRingbufferReturnItem(usb_rx_ringbuf_handle, (void *)item);

            if (item_size < (usb_packet.payload_length + 3))
            {
                item2 = (uint8_t *)xRingbufferReceiveUpTo(usb_rx_ringbuf_handle, &item_size2, pdMS_TO_TICKS(50), ((usb_packet.payload_length + 3) - item_size));

                if (item2 == NULL)
                {
                    send_usb_packet(Bus_USB, Command_Error, Error_Timeout, err, sizeof(err));
                    goto begin;
                }

                for (int i = 0; i < item_size2; i++)
                {
                    rx_buf[item_size + i] = item2[i];
                }

                vRingbufferReturnItem(usb_rx_ringbuf_handle, (void *)item2);
            }

            usb_packet.datacode = rx_buf[0];
            usb_packet.subdatacode = rx_buf[1];

            if (usb_packet.payload_length > 0)
            {
                for (int i = 0; i < usb_packet.payload_length; i++)
                {
                    usb_packet.payload[i] = rx_buf[2 + i];
                }

                usb_packet.checksum = rx_buf[2 + usb_packet.payload_length];
            }
            else
            {
                usb_packet.checksum = rx_buf[2];
            }

            uint8_t checksum = usb_packet.sync + ((usb_packet.length >> 8) & 0xFF) + (usb_packet.length & 0xFF) + usb_packet.datacode + usb_packet.subdatacode;

            if (usb_packet.payload_length > 0)
            {
                for (int i = 0; i < usb_packet.payload_length; i++)
                {
                    checksum += usb_packet.payload[i];
                }
            }

            if (usb_packet.checksum == checksum)
            {
                usb_packet.available = true;
            }
            else
            {
                send_usb_packet(Bus_USB, Command_Error, Error_Checksum, err, sizeof(err));
            }
        }

        if (usb_packet.available)
        {
            usb_packet.available = false;
            parse_usb_command();
        }
    }

    vTaskDelete(NULL);
}

/**
 * @brief ISR handler to start CCD-bus idle counter when a start bit appears on the bus.
 * 
 * @param arg Unused parameters.
 */
void IRAM_ATTR ccd_startbit_sense_isr_handler(void *arg)
{
    ccd.state.idle = false;
    gpio_isr_handler_remove(CCD_IDLE_PIN);
    timer_set_counter_value(CCD_IDLE_TIMER_GROUP, CCD_IDLE_TIMER, 0);
    timer_start(CCD_IDLE_TIMER_GROUP, CCD_IDLE_TIMER);
}

/**
 * @brief ISR callback to finish CCD-bus idle detection and save received message length.
 * 
 * @param arg Unused parameters.
 * 
 * @return Always true.
 */
bool IRAM_ATTR ccd_idle_timer_isr_callback(void *arg)
{
    ccd.state.idle = true;
    timer_pause(CCD_IDLE_TIMER_GROUP, CCD_IDLE_TIMER);
    ccd.msg.rx_length = ccd.msg.rx_ptr;
    ccd.msg.rx_ptr = 0;
    return true;
}

/**
 * @brief Initialize CCD-bus idle timer.
 * 
 * @param group Hardware timer group.
 * @param timer Hardware timer.
 * @param ccd_idle_bits Number of bits following an UART stop bit to consider bus idling.
 */
void ccd_idle_timer_init(int group, int timer, uint16_t ccd_idle_bits)
{
    // Select and initialize basic parameters of the timer.
    timer_config_t config = {
        .divider = CCD_IDLE_TIMER_DIVIDER,
        .counter_dir = TIMER_COUNT_UP,
        .counter_en = TIMER_PAUSE,
        .alarm_en = TIMER_ALARM_EN,
        .auto_reload = true
    }; // default clock source is APB

    timer_init(group, timer, &config);

    // Timer's counter will initially start from value below.
    // Also, if auto_reload is set, this value will be automatically reload on alarm.
    timer_set_counter_value(group, timer, 0);

    // Configure the alarm value and the interrupt on alarm.
    timer_set_alarm_value(group, timer, 11 + ccd_idle_bits); // 10 UART frame bits + n idle bits
    timer_enable_intr(group, timer);
    timer_isr_callback_add(group, timer, &ccd_idle_timer_isr_callback, NULL, ESP_INTR_FLAG_SHARED);
    timer_start(group, timer);
}

/**
 * @brief Task to handle and store received CCD-bus message bytes.
 * 
 * @param pvParameters Unused parameters.
 */
void ccd_event_task(void *pvParameters)
{
    uart_event_t event;

    for (EVER)
    {
        if (xQueueReceive(ccd_uart_queue, (void *)&event, pdMS_TO_TICKS(50))) // look out for events happening
        {
            switch (event.type)
            {
                case UART_DATA:
                {
                    uint8_t data[32];
                    uint16_t length = event.size;
                    uart_read_bytes(UART_CCD, data, length, pdMS_TO_TICKS(50)); // read bytes to a simple byte buffer
                    gpio_isr_handler_add(CCD_IDLE_PIN, ccd_startbit_sense_isr_handler, NULL); // look out for more data byte before bus becomes idle

                    for (int i = 0; i < length; i++)
                    {
                        ccd.msg.rx_buffer[ccd.msg.rx_ptr] = data[i];
                        ccd.msg.rx_ptr++;

                        if (ccd.msg.rx_ptr >= CCD_RX_BUF_SIZE) ccd.msg.rx_ptr = 0;
                    }

                    break;
                }
                case UART_FIFO_OVF:
                case UART_BUFFER_FULL:
                {
                    uart_flush_input(UART_CCD);
                    xQueueReset(ccd_uart_queue);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    vTaskDelete(NULL);
}

/**
 * @brief Task to start analyzing CCD-bus messages.
 * 
 * @param pvParameters Unused parameters.
 */
void ccd_msg_task(void *pvParameters)
{
    uint8_t checksum = 0;

    for (EVER)
    {
        if (ccd.msg.rx_length > 0)
        {
            checksum = calculate_checksum(ccd.msg.rx_buffer, 0, ccd.msg.rx_length - 1);

            if (ccd.msg.rx_buffer[ccd.msg.rx_length - 1] == checksum)
            {
                if (ccd.state.en)
                {
                    send_usb_packet(Bus_CCD, Command_Receive, Receive_CCD, ccd.msg.rx_buffer, ccd.msg.rx_length);
                }
            }

            if (ccd.repeat.en)
            {
                if (ccd.msg.tx_count == 1) // if there's only one message in the buffer
                {
                    bool match = true;

                    for (int i = 0; i < ccd.msg.rx_length; i++)
                    {
                        if (ccd.msg.tx_buffer[i] != ccd.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                    }

                    if (match) ccd.repeat.next = true; // if echo is correct prepare next message
                }
                else if (ccd.msg.tx_count > 1) // multiple messages
                {
                    bool match = true;

                    for (int i = 0; i < ccd.msg.rx_length; i++)
                    {
                        if (ccd.msg.tx_buffer[ccd.msg.tx_ptr + 1 + i] != ccd.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                    }

                    if (match)
                    {
                        ccd.repeat.next = true; // if echo is correct prepare next message

                        // Increase the current message counter and set the buffer pointer to the next message length
                        ccd.msg.tx_count_ptr++;
                        ccd.msg.tx_ptr += ccd.msg.tx_length + 1;
                        ccd.msg.tx_length = ccd.msg.tx_buffer[ccd.msg.tx_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning
                        if (ccd.msg.tx_count_ptr == ccd.msg.tx_count)
                        {
                            ccd.msg.tx_count_ptr = 0;
                            ccd.msg.tx_ptr = 0;
                            ccd.msg.tx_length = ccd.msg.tx_buffer[ccd.msg.tx_ptr]; // re-calculate new message length

                            if (ccd.repeat.list_once) ccd.repeat.stop = true;
                        }
                    }
                }

                if (ccd.repeat.stop) // one-shot message list is terminated here
                {
                    ccd.msg.tx_ptr = 0;
                    ccd.repeat.en = false;
                    ccd.repeat.next = false;
                    ccd.repeat.list_once = false;
                }
            }

            ccd.msg.rx_length = 0;
            ccd.stats.msg_rx_count++;
        }

        if (ccd.random.en && (ccd.random.interval > 0))
        {
            if ((uint32_t)(millis() - ccd.random.last_millis) >= ccd.random.interval)
            {
                ccd.random.last_millis = millis();
                ccd.msg.tx_length = get_random(3, 6); // random message length between 3 and 6 bytes

                for (int i = 0; i < (ccd.msg.tx_length - 2); i++)
                {
                    ccd.msg.tx_buffer[i] = get_random(0, 255); // generate random bytes between 0 and 255
                }

                uint8_t checksum_position = ccd.msg.tx_length - 1;
                ccd.msg.tx_buffer[checksum_position] = calculate_checksum(ccd.msg.tx_buffer, 0, checksum_position);
                ccd.state.msg_tx_pending = true;
                ccd.random.interval = get_random(ccd.random.interval_min, ccd.random.interval_max); // generate new delay value between random messages
            }
        }

        // Repeated messages are prepared here.
        if (ccd.repeat.en)
        {
            if ((uint32_t)(millis() - ccd.repeat.last_millis) >= ccd.repeat.interval) // wait between messages
            {
                ccd.repeat.last_millis = millis();

                if (ccd.repeat.next)
                {
                    ccd.state.msg_tx_pending = true;
                    ccd.repeat.next = false;
                }
            }
        }

        if (ccd.state.msg_tx_pending && ccd.state.idle)
        {
            if (ccd.msg.tx_count == 1) // if there's only one message in the buffer
            {
                uart_write_bytes(UART_CCD, (const uint8_t *)ccd.msg.tx_buffer, ccd.msg.tx_length);
            }
            else if (ccd.msg.tx_count > 1) // multiple messages, send one at a time
            {
                // Make a local copy of the current message.
                uint8_t current_message[ccd.msg.tx_length];
                int j = 0;

                for (int i = (ccd.msg.tx_ptr + 1); i < (ccd.msg.tx_ptr + 1 + ccd.msg.tx_length); i++)
                {
                    current_message[j] = ccd.msg.tx_buffer[i];
                    j++;
                }

                uart_write_bytes(UART_CCD, (const uint8_t *)current_message, ccd.msg.tx_length);
            }

            ccd.state.msg_tx_pending = false;
            ccd.stats.msg_tx_count++;
        }

        vTaskDelay(1);
    }

    vTaskDelete(NULL);
}

/**
 * @brief ISR callback to finish SCI-bus idle detection and save received message length.
 * 
 * @param arg Unused parameters.
 * 
 * @return Always true.
 */
bool IRAM_ATTR sci_idle_timer_isr_callback(void *arg)
{
    timer_pause(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER);
    sci.msg.rx_length = sci.msg.rx_ptr;
    sci.msg.last_rx_length = sci.msg.rx_length; // remember last message length for bootstrap mode only
    sci.msg.rx_ptr = 0;
    sci.state.idle = true;
    sci.state.echo_received = true;
    return true;
}

/**
 * @brief Initialize SCI-bus idle timer.
 * 
 * @param group Hardware timer group.
 * @param timer Hardware timer.
 * @param sci_idle_timeout_ms Number of milliseconds elapsed after last received byte to consider bus idling.
 */
void sci_idle_timer_init(int group, int timer, uint16_t sci_idle_timeout_ms)
{
    // Select and initialize basic parameters of the timer.
    timer_config_t config = {
        .divider = SCI_IDLE_TIMER_DIVIDER,
        .counter_dir = TIMER_COUNT_UP,
        .counter_en = TIMER_PAUSE,
        .alarm_en = TIMER_ALARM_EN,
        .auto_reload = true
    }; // default clock source is APB

    timer_init(group, timer, &config);

    // Timer's counter will initially start from value below.
    // Also, if auto_reload is set, this value will be automatically reload on alarm.
    timer_set_counter_value(group, timer, 0);

    // Configure the alarm value and the interrupt on alarm.
    timer_set_alarm_value(group, timer, 2 * sci_idle_timeout_ms);
    timer_enable_intr(group, timer);
    timer_isr_callback_add(group, timer, &sci_idle_timer_isr_callback, NULL, ESP_INTR_FLAG_SHARED);
}

/**
 * @brief Set SCI-bus timeout.
 * 
 * @param sci_idle_timeout_ms Number of milliseconds elapsed after last received byte to consider bus idling.
 */
void sci_set_timeout(uint16_t sci_idle_timeout_ms)
{
    timer_set_alarm_value(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER, 2 * sci_idle_timeout_ms);
    timer_start(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER);
    sci.timing.timeout = sci_idle_timeout_ms;
}

/**
 * @brief Task to handle and store received SCI-bus message bytes.
 * 
 * @param pvParameters Unused parameters.
 */
void sci_event_task(void *pvParameters)
{
    uart_event_t event;

    for (EVER)
    {
        if (xQueueReceive(sci_uart_queue, (void *)&event, pdMS_TO_TICKS(50))) // look out for events happening
        {
            switch (event.type)
            {
                case UART_DATA:
                {
                    timer_set_counter_value(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER, 0);
                    timer_start(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER);
                    sci.state.idle = false;

                    uint8_t data[32];
                    uint16_t length = event.size;

                    uart_read_bytes(UART_SCI, data, length, pdMS_TO_TICKS(50)); // read bytes to a simple byte buffer

                    if (sci.state.inverted_logic) // last 4 bits come first, then first 4 bits
                    {
                        uint8_t raw = 0;

                        for (int i = 0; i < length; i++)
                        {
                            raw = data[i]; // load reversed byte
                            data[i] = ((raw << 4) & 0xF0) | ((raw >> 4) & 0x0F); // re-arrange bits
                        }
                    }

                    for (int i = 0; i < length; i++)
                    {
                        sci.msg.rx_buffer[sci.msg.rx_ptr] = data[i];
                        sci.msg.rx_ptr++;

                        if (sci.msg.rx_ptr >= SCI_RX_BUF_SIZE) sci.msg.rx_ptr = 0;
                    }

                    sci.msg.byte_received = true;
                    break;
                }
                case UART_FIFO_OVF:
                case UART_BUFFER_FULL:
                {
                    uart_flush_input(UART_SCI);
                    xQueueReset(sci_uart_queue);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    vTaskDelete(NULL);
}

/**
 * @brief Task to start analyzing SCI-bus messages.
 * 
 * @param pvParameters Unused parameters.
 */
void sci_msg_task(void *pvParameters)
{
    //uint8_t checksum = 0;
    
    for (EVER)
    {
        if ((sci.bus_settings & SCI_SPEED_BITS) <= SCI_SPEED_7812_BAUD) // SCI_SPEED_976_BAUD or SCI_SPEED_7812_BAUD
        {
            if ((sci.state.echo_received || sci.state.response_received) && (sci.msg.rx_length > 0))
            {
                sci.state.echo_received = false;
                sci.state.response_received = false;

                if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
                {
                    send_usb_packet(Bus_TCM, Command_Receive, Receive_SCILowSpeedMsg, sci.msg.rx_buffer, sci.msg.rx_length);
                }
                else // PCM
                {
                    send_usb_packet(Bus_PCM, Command_Receive, Receive_SCILowSpeedMsg, sci.msg.rx_buffer, sci.msg.rx_length);
                }

                if (sci.msg.rx_buffer[0] == 0x12) // pay attention to special bytes (speed change)
                {
                    sbi(sci.bus_settings, 1); // set/clear speed bits (62500 baud)
                    cbi(sci.bus_settings, 0); // set/clear speed bits (62500 baud)
                    configure_sci_bus(sci.bus_settings);
                }

                if (sci.repeat.en) // prepare next repeated message
                {
                    if (sci.msg.tx_count == 1) // if there's only one message in the buffer
                    {
                        bool match = true;

                        // for (int i = 0; i < sci.msg.rx_length; i++)
                        // {
                        //     if (sci.msg.tx_buffer[i] != sci.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                        // }

                        if (match) sci.repeat.next = true; // if echo is correct prepare next message
                    }
                    else if (sci.msg.tx_count > 1) // multiple messages
                    {
                        bool match = true;

                        // for (int i = 0; i < sci.msg.rx_length; i++)
                        // {
                        //     if (sci.msg.tx_buffer[sci.msg.tx_ptr + 1 + i] != sci.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                        // }

                        if (match)
                        {
                            sci.repeat.next = true; // if echo is correct prepare next message

                            // Increase the current message counter and set the buffer pointer to the next message length
                            sci.msg.tx_count_ptr++;
                            sci.msg.tx_ptr += sci.msg.tx_length + 1;
                            sci.msg.tx_length = sci.msg.tx_buffer[sci.msg.tx_ptr]; // re-calculate new message length

                            // After the last message reset everything to zero to start at the beginning
                            if (sci.msg.tx_count_ptr == sci.msg.tx_count)
                            {
                                sci.msg.tx_count_ptr = 0;
                                sci.msg.tx_ptr = 0;
                                sci.msg.tx_length = sci.msg.tx_buffer[sci.msg.tx_ptr]; // re-calculate new message length

                                if (sci.repeat.list_once) sci.repeat.stop = true;
                            }
                        }
                    }

                    if (sci.repeat.stop) // one-shot message list is terminated here
                    {
                        sci.msg.tx_ptr = 0;
                        sci.repeat.en = false;
                        sci.repeat.next = false;
                        sci.repeat.list_once = false;
                    }
                }

                sci.msg.rx_length = 0;
                sci.stats.msg_rx_count++;
            }
        }
        else // SCI_SPEED_62500_BAUD or SCI_SPEED_125000_BAUD
        {
            if ((sci.state.echo_received || sci.state.response_received) && (sci.msg.rx_length > 0))
            {
                sci.state.echo_received = false;
                sci.state.response_received = false;
                uint16_t packet_length = 0;

                if (((sci.msg.tx_count == 1) && array_contains(sci_hi_speed_memarea, 16, sci.msg.tx_buffer[0])) || ((sci.msg.tx_count > 1) && array_contains(sci_hi_speed_memarea, 16, sci.msg.tx_buffer[1]))) // normal mode
                {
                    packet_length = (2 * sci.msg.rx_length) - 1;
                }
                else // bootstrap mode
                {
                    packet_length = sci.msg.rx_length;
                }

                uint8_t usb_msg[packet_length];

                // Request and response bytes are mixed together in a single message:
                // F4 0A 00 0B 00 0C 00 0D 00...
                // F4: RAM table
                // 0A: RAM address
                // 00: RAM value at 0A
                // 0B: RAM address
                // 00: RAM value at 0B
                // 0C: RAM address
                // 00: RAM value at 0C
                // 0D: RAM address
                // 00: RAM value at 0D

                if (sci.msg.tx_count == 1)
                {
                    if (array_contains(sci_hi_speed_memarea, 16, sci.msg.tx_buffer[0])) // normal mode
                    {
                        usb_msg[0] = sci.msg.rx_buffer[0]; // put RAM table byte first

                        for (int i = 0; i < sci.msg.tx_length; i++)
                        {
                            usb_msg[1 + (i * 2)] = sci.msg.tx_buffer[i + 1]; // put original request message byte next
                            usb_msg[1 + (i * 2) + 1] = sci.msg.rx_buffer[i + 1]; // put response byte after the request byte
                        }

                        if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
                        {
                            send_usb_packet(Bus_TCM, Command_Receive, Receive_SCIHighSpeedMsg, usb_msg, packet_length);
                        }
                        else // PCM
                        {
                            send_usb_packet(Bus_PCM, Command_Receive, Receive_SCIHighSpeedMsg, usb_msg, packet_length);
                        }
                    }
                    else // bootstrap mode
                    {
                        if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
                        {
                            send_usb_packet(Bus_TCM, Command_Receive, Receive_SCIHighSpeedMsg, sci.msg.rx_buffer, sci.msg.rx_length);
                        }
                        else // PCM
                        {
                            send_usb_packet(Bus_PCM, Command_Receive, Receive_SCIHighSpeedMsg, sci.msg.rx_buffer, sci.msg.rx_length);
                        }
                        
                    }
                }
                else if (sci.msg.tx_count > 1)
                {
                    usb_msg[0] = sci.msg.tx_buffer[sci.msg.tx_ptr + 1]; // put RAM table byte first

                    for (int i = 0; i < sci.msg.tx_length; i++) 
                    {
                        usb_msg[1 + (i * 2)] = sci.msg.tx_buffer[sci.msg.tx_ptr + i + 2]; // put original request message byte next
                        usb_msg[1 + (i * 2) + 1] = sci.msg.rx_buffer[i + 1]; // put response byte after the request byte
                    }

                    if (sci.bus_settings & (1 << SCI_MODULE_BIT)) // TCM
                    {
                        send_usb_packet(Bus_TCM, Command_Receive, Receive_SCIHighSpeedMsg, usb_msg, packet_length);
                    }
                    else // PCM
                    {
                        send_usb_packet(Bus_PCM, Command_Receive, Receive_SCIHighSpeedMsg, usb_msg, packet_length);
                    }
                }

                if (usb_msg[0] == 0xFE) // pay attention to special bytes (speed change)
                {
                    cbi(sci.bus_settings, 1); // set/clear speed bits (7812.5 baud)
                    sbi(sci.bus_settings, 0); // set/clear speed bits (7812.5 baud)
                    configure_sci_bus(sci.bus_settings);
                }

                if (sci.repeat.en)
                {
                    if (sci.msg.tx_count == 1) // if there's only one message in the buffer
                    {
                        sci.repeat.next = true; // accept echo without verification...
                    }
                    else if (sci.msg.tx_count > 1) // multiple messages
                    {
                        sci.repeat.next = true; // accept echo without verification...

                        // Increase the current message counter and set the buffer pointer to the next message length byte.
                        sci.msg.tx_count_ptr++;
                        sci.msg.tx_ptr += sci.msg.tx_length + 1;
                        sci.msg.tx_length = sci.msg.tx_buffer[sci.msg.tx_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning.
                        if (sci.msg.tx_count_ptr == sci.msg.tx_count)
                        {
                            sci.msg.tx_count_ptr = 0;
                            sci.msg.tx_ptr = 0;
                            sci.msg.tx_length = sci.msg.tx_buffer[sci.msg.tx_ptr]; // re-calculate new message length

                            if (sci.repeat.list_once) sci.repeat.stop = true;
                        }
                    }

                    if (sci.repeat.stop) // one-shot message list is terminated here
                    {
                        sci.msg.tx_ptr = 0;
                        sci.repeat.en = false;
                        sci.repeat.next = false;
                        sci.repeat.list_once = false;
                    }
                }

                sci.msg.rx_length = 0;
                sci.stats.msg_rx_count++;
            }
        }

        // Repeated messages are prepared here for transmission.
        if (sci.repeat.en && (sci.msg.rx_length == 0))
        {
            if ((uint32_t)(millis() - sci.repeat.last_millis) >= sci.repeat.interval) // wait between messages
            {
                sci.repeat.last_millis = millis();
                
                if (sci.repeat.next)
                {
                    sci.state.msg_tx_pending = true;
                    sci.repeat.next = false;
                }
            }
        }

        if (sci.state.msg_tx_pending && sci.state.idle && (sci.msg.rx_length == 0))
        {
            uint8_t buff[1] = { 0 };

            sci.msg.byte_received = false;
            sci.state.idle = false;

            if ((sci.bus_settings & SCI_SPEED_BITS) <= SCI_SPEED_7812_BAUD) // SCI_SPEED_976_BAUD or SCI_SPEED_7812_BAUD
            {
                sci_set_timeout(SCI_LS_T3_DELAY);

                if (sci.msg.tx_count == 1)
                {
                    if (sci.state.ngc_mode) // do not wait for echo
                    {
                        uart_write_bytes(UART_SCI, (const uint8_t *)sci.msg.tx_buffer, sci.msg.tx_length);
                        uart_wait_tx_idle_polling(UART_SCI);
                        vTaskDelay(pdMS_TO_TICKS(5));

                        while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                    }
                    else // wait for echo
                    {
                        for (int i = 0; i < sci.msg.tx_length; i++)
                        {
                            buff[0] = sci.msg.tx_buffer[i];
                            uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                            uart_wait_tx_idle_polling(UART_SCI);
                            while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                            sci.msg.byte_received = false;
                        }

                        if ((sci.msg.tx_length > 1) && (sci.msg.rx_buffer[0] == 0x13) && (sci.msg.rx_buffer[1] != 0x00))
                        {
                            while (!sci.msg.byte_received && !sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

                            if (sci.msg.byte_received)
                            {
                                vTaskDelay(pdMS_TO_TICKS(2));
                                buff[0] = 0x13;
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1); // disable actuator test mode byte stream
                                uart_wait_tx_idle_polling(UART_SCI);
                            }
                        }

                        if (array_contains(sci_lo_speed_cmd_filter, sizeof(sci_lo_speed_cmd_filter), sci.msg.tx_buffer[0]))
                        {
                            while (!sci.msg.byte_received && !sci.state.idle);

                            if (sci.msg.byte_received)
                            {
                                sci_idle_timer_isr_callback(NULL);
                            }
                            else
                            {
                                while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                            }
                        }
                        else
                        {
                            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                        }
                    }
                }
                else if (sci.msg.tx_count > 1)
                {
                    // Make a local copy of the current message.
                    uint8_t current_message[sci.msg.tx_length];
                    int j = 0;

                    for (int i = (sci.msg.tx_ptr + 1); i < (sci.msg.tx_ptr + 1 + sci.msg.tx_length); i++)
                    {
                        current_message[j] = sci.msg.tx_buffer[i];
                        j++;
                    }

                    if (sci.state.ngc_mode) // do not wait for echo
                    {
                        uart_write_bytes(UART_SCI, (const uint8_t *)current_message, sci.msg.tx_length);
                        uart_wait_tx_idle_polling(UART_SCI);
                        vTaskDelay(pdMS_TO_TICKS(5));

                        while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                    }
                    else // wait for echo
                    {
                        for (int i = 0; i < sci.msg.tx_length; i++)
                        {
                            buff[0] = current_message[i];
                            uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                            uart_wait_tx_idle_polling(UART_SCI);
                            while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                            sci.msg.byte_received = false;
                        }

                        if (array_contains(sci_lo_speed_cmd_filter, sizeof(sci_lo_speed_cmd_filter), current_message[0]))
                        {
                            while (!sci.msg.byte_received && !sci.state.idle);

                            if (sci.msg.byte_received)
                            {
                                sci_idle_timer_isr_callback(NULL);
                            }
                            else
                            {
                                while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                            }
                        }
                        else
                        {
                            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                        }
                    }
                }
            }
            else // SCI_SPEED_62500_BAUD or SCI_SPEED_125000_BAUD
            {
                if (sci.msg.tx_count == 1)
                {
                    sci_set_timeout(SCI_HS_T3_DELAY);
                    
                    if (array_contains(sci_hi_speed_memarea, 16, sci.msg.tx_buffer[0])) // normal mode
                    {
                        if ((sci.msg.tx_length > 1) && (sci.msg.tx_buffer[1] == 0xFF)) // return full RAM-table if the first address is an invalid 0xFF
                        {
                            // Prepare message buffer as if it was filled with data beforehand
                            for (uint8_t i = 0; i < 240; i++)
                            {
                                sci.msg.tx_buffer[1 + i] = i; // put the address byte after the memory table pointer
                            }

                            sci.msg.tx_length = 241;
                        }

                        if (sci.state.ngc_mode) // do not wait for response
                        {
                            uart_write_bytes(UART_SCI, (const uint8_t *)sci.msg.tx_buffer, sci.msg.tx_length);
                            uart_wait_tx_idle_polling(UART_SCI);
                            vTaskDelay(pdMS_TO_TICKS(5));

                            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                        }
                        else // wait for response
                        {
                            sci.msg.byte_received = false;

                            for (int i = 0; i < sci.msg.tx_length; i++)
                            {
                                buff[0] = sci.msg.tx_buffer[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                uart_wait_tx_idle_polling(UART_SCI);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for response
                                sci.msg.byte_received = false;
                            }

                            sci.state.response_received = true; // do not wait for additional bytes
                            sci_idle_timer_isr_callback(NULL);
                        }
                    }
                    else // bootstrap mode, always wait for echo
                    {
                        sci.msg.byte_received = false;

                        if (sci.bootstrap.progvolts_request)
                        {
                            sci.bootstrap.progvolts_request = false;
                            sci_set_timeout(5 * SCI_LS_T1_DELAY); // apply generous timeout because flash block is not echoed back right away (100 ms for 512 bytes)
                            sci.msg.byte_received = false;

                            for (uint8_t i = 0; i < 6; i++)
                            {
                                buff[0] = sci.msg.tx_buffer[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 6; i < sci.msg.tx_length; i++)
                            {
                                buff[0] = sci.msg.tx_buffer[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                            }

                            uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                            vTaskDelay(pdMS_TO_TICKS(1));
                            apply_progvolt(1 << SCI_VPP_EN_BIT);
                            sci.msg.byte_received = false;
                            while (!sci.msg.byte_received && !sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                            sci_set_timeout(SCI_LS_T3_DELAY);
                            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                            apply_progvolt(0);
                        }
                        else
                        {
                            if (sci.bootstrap.worker_function_src == WorkerFunction_EEPROMWriteSPI)
                            {
                                sci_set_timeout(5 * SCI_LS_T1_DELAY); // apply generous timeout because EEPROM block is not echoed back right away (100 ms for 512 bytes)
                                sci.msg.byte_received = false;

                                for (uint8_t i = 0; i < 5; i++)
                                {
                                    buff[0] = sci.msg.tx_buffer[i];
                                    uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                    while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                    sci.msg.byte_received = false;
                                }

                                for (uint16_t i = 5; i < sci.msg.tx_length; i++)
                                {
                                    buff[0] = sci.msg.tx_buffer[i];
                                    uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                }

                                uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                            }
                            else
                            {
                                for (int i = 0; i < sci.msg.tx_length; i++)
                                {
                                    buff[0] = sci.msg.tx_buffer[i];
                                    uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                    uart_wait_tx_idle_polling(UART_SCI);
                                    while (!sci.msg.byte_received && !sci.state.idle); // wait for response
                                    sci.msg.byte_received = false;
                                }
                            }

                            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                        }
                    }
                }
                else if (sci.msg.tx_count > 1)
                {
                    // Make a local copy of the current message.
                    uint8_t current_message[sci.msg.tx_length];
                    int j = 0;

                    for (int i = (sci.msg.tx_ptr + 1); i < (sci.msg.tx_ptr + 1 + sci.msg.tx_length); i++)
                    {
                        current_message[j] = sci.msg.tx_buffer[i];
                        j++;
                    }
                    
                    sci_set_timeout(SCI_LS_T3_DELAY);

                    if (sci.state.ngc_mode) // do not wait for response
                    {
                        uart_write_bytes(UART_SCI, (const uint8_t *)sci.msg.tx_buffer, sci.msg.tx_length);
                        uart_wait_tx_idle_polling(UART_SCI);
                        vTaskDelay(pdMS_TO_TICKS(5));

                        while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));
                    }
                    else // wait for response
                    {
                        sci.msg.byte_received = false;

                        for (int i = 0; i < sci.msg.tx_length; i++)
                        {
                            buff[0] = current_message[i];
                            uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                            uart_wait_tx_idle_polling(UART_SCI);
                            while (!sci.msg.byte_received && !sci.state.idle); // wait for response
                            sci.msg.byte_received = false;
                        }

                        sci.state.response_received = true; // do not wait for additional bytes
                        sci_idle_timer_isr_callback(NULL);
                    }
                }
            }
            
            sci.state.msg_tx_pending = false;
            sci.stats.msg_tx_count++;
        }

        vTaskDelay(1);
    }

    vTaskDelete(NULL);
}

/**
 * @brief Calculate secret bootstrap key from seed value sent by the controller.
 * 
 * @param seed Seed value sent by the controller.
 * 
 * @param return Key to unlock controller.
 */
uint16_t get_sbec3_bootstrap_key(uint16_t seed)
{
    uint16_t key = (seed + 0x247C) | 5; // add magic word and set minimum number of bit rotations
    uint8_t i = key & 0x0F; // determine number of bit rotations

    while (i--) // rotate bits to the right, rightmost bit is transferred to the leftmost position
    {
        if (key & 1) key = ((key >> 1) | 0x8000);
        else key >>= 1;
    }

    return (key | 0x247C); // apply same magic word again and return key
}

/**
 * @brief Task to handle SCI-bus bootstrap mode.
 * 
 * @param pvParameters Unused parameters.
 */
void sci_boot_task(void *pvParameters)
{
    uint8_t ret[1] = { 0 };
    uint8_t buff[16];
    uint8_t checksum = 0;
    bool key_required = true;
    uint8_t sci_bootstrap_magic_byte[1] = { 0x7F };
    uint8_t req_seed_cmd[5] = { 0x24, 0xD0, 0x27, 0xC1, 0xDC };
    uint8_t send_key_cmd[7] = { 0x24, 0xD0, 0x27, 0xC2, 0, 0, 0 };
    uint8_t bl_header[5] = { 0x4C, 0x01, 0x00, 0, 0 }; // upload bootloader header
    uint8_t start_bootloader_cmd[3] = { 0x47, 0x01, 0x00 };
    uint8_t wf_header[3] = { 0x10, 0, 0 }; // upload worker function header
    uint8_t start_worker_function_cmd[1] = { 0x20 };
    uint8_t stop_flash_write_cmd[1] = { 0x32 };
    uint8_t stop_flash_read_cmd[1] = { 0x35 };
    uint8_t stop_eeprom_write_cmd[1] = { 0x38 };
    uint8_t stop_eeprom_read_cmd[1] = { 0x3B };

    for (EVER)
    {
        if (sci.bootstrap.upload_bootloader)
        {
            // Status bytes:
            // 0 = ok
            // 1 = no response to magic byte
            // 2 = unexpected response to magic byte
            // 3 = security seed response timeout
            // 4 = security seed response checksum error
            // 5 = security key status timeout
            // 6 = security key not accepted
            // 7 = start bootloader timeout
            // 8 = unexpected bootloader status byte

            ret[0] = BootloaderError_OK;
            sci.bootstrap.upload_bootloader = false;
            checksum = 0;
            key_required = true;
            sci.msg.byte_received = false;
            sci.state.idle = false;
            sci.msg.tx_count = 1;
            sci.msg.rx_ptr = 0;
            uart_flush_input(UART_SCI);
            uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
            sci_set_timeout(5 * SCI_LS_T1_DELAY);
            uart_write_bytes(UART_SCI, (const uint8_t *)sci_bootstrap_magic_byte, sizeof(sci_bootstrap_magic_byte));
            uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
            vTaskDelay(pdMS_TO_TICKS(1));

            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

            if (!sci.msg.byte_received)
            {
                ret[0] = BootloaderError_NoResponseToMagicByte;
                goto stop_bl;
            }

            if (sci.msg.rx_buffer[0] != 0x06)
            {
                ret[0] = BootloaderError_UnexpectedResponseToMagicByte;
                goto stop_bl;
            }

            sci.state.idle = false;

            sci_set_timeout(5 * SCI_LS_T1_DELAY);
            uart_write_bytes(UART_SCI, (const uint8_t *)req_seed_cmd, sizeof(req_seed_cmd));
            uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred

            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

            if (sci.msg.last_rx_length < 7)
            {
                if ((sci.msg.last_rx_length == 5) && (sci.msg.rx_buffer[0] == 0xDB) && (sci.msg.rx_buffer[1] == 0x2F) && (sci.msg.rx_buffer[2] == 0xD8) && (sci.msg.rx_buffer[3] == 0x3E) && (sci.msg.rx_buffer[4] == 0x23))
                {
                    key_required = false;
                }
                else
                {
                    ret[0] = BootloaderError_SecuritySeedResponseTimeout;
                    goto stop_bl;
                }
            }

            if (key_required)
            {
                checksum = calculate_checksum(sci.msg.rx_buffer, 0, 6);

                uint8_t key[2];

                if ((sci.msg.rx_buffer[0] == 0x26) && (sci.msg.rx_buffer[1] == 0xD0) && (sci.msg.rx_buffer[2] == 0x67) && (sci.msg.rx_buffer[3] == 0xC1) && (sci.msg.rx_buffer[6] == checksum))
                {
                    uint16_t data = get_sbec3_bootstrap_key(to_uint16(sci.msg.rx_buffer[4], sci.msg.rx_buffer[5]));
                    key[0] = (data >> 8) & 0xFF;
                    key[1] = data & 0xFF;
                }
                else
                {
                    ret[0] = BootloaderError_SecuritySeedChecksumError;
                    goto stop_bl;
                }

                send_key_cmd[4] = key[0];
                send_key_cmd[5] = key[1];
                send_key_cmd[6] = calculate_checksum(send_key_cmd, 0, sizeof(send_key_cmd) - 1);

                sci.state.idle = false;
                
                sci_set_timeout(5 * SCI_LS_T1_DELAY);
                uart_write_bytes(UART_SCI, (const uint8_t *)send_key_cmd, sizeof(send_key_cmd));
                uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred

                while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

                if (sci.msg.last_rx_length < 5)
                {
                    ret[0] = BootloaderError_SecurityKeyStatusTimeout;
                    goto stop_bl;
                }

                if (!((sci.msg.rx_buffer[0] == 0x26) && (sci.msg.rx_buffer[1] == 0xD0) && (sci.msg.rx_buffer[2] == 0x67) && (sci.msg.rx_buffer[3] == 0xC2) && (sci.msg.rx_buffer[4] == 0x1F)))
                {
                    ret[0] = BootloaderError_SecurityKeyNotAccepted;
                    goto stop_bl;
                }
            }

            sci.msg.byte_received = false;
            sci.state.idle = false;

            sci_set_timeout(5 * SCI_LS_T1_DELAY);

            switch (sci.bootstrap.bootloader_src)
            {
                case Bootloader_128k_SBEC3:
                {
                    bl_header[3] = ((0x0100 + sizeof(LdBoot_128k_SBEC3) - 1) >> 8) & 0xFF;
                    bl_header[4] = (0x0100 + sizeof(LdBoot_128k_SBEC3) - 1) & 0xFF;

                    for (uint8_t i = 0; i < sizeof(bl_header); i++)
                    {
                        buff[0] = bl_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdBoot_128k_SBEC3); i++)
                    {
                        buff[0] = LdBoot_128k_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case Bootloader_256k_SBEC3:
                {
                    bl_header[3] = ((0x0100 + sizeof(LdBoot_256k_SBEC3) - 1) >> 8) & 0xFF;
                    bl_header[4] = (0x0100 + sizeof(LdBoot_256k_SBEC3) - 1) & 0xFF;

                    for (uint8_t i = 0; i < sizeof(bl_header); i++)
                    {
                        buff[0] = bl_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdBoot_256k_SBEC3); i++)
                    {
                        buff[0] = LdBoot_256k_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case Bootloader_256k_SBEC3_custom: // SBEC3 256k custom by Dino
                {
                    bl_header[3] = ((0x0100 + sizeof(LdBoot_256k_SBEC3_custom) - 1) >> 8) & 0xFF;
                    bl_header[4] = (0x0100 + sizeof(LdBoot_256k_SBEC3_custom) - 1) & 0xFF;

                    for (uint8_t i = 0; i < sizeof(bl_header); i++)
                    {
                        buff[0] = bl_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdBoot_256k_SBEC3_custom); i++)
                    {
                        buff[0] = LdBoot_256k_SBEC3_custom[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case Bootloader_256k_JTEC:
                {
                    bl_header[3] = ((0x0100 + sizeof(LdBoot_256k_JTEC) - 1) >> 8) & 0xFF;
                    bl_header[4] = (0x0100 + sizeof(LdBoot_256k_JTEC) - 1) & 0xFF;

                    for (uint8_t i = 0; i < sizeof(bl_header); i++)
                    {
                        buff[0] = bl_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdBoot_256k_JTEC); i++)
                    {
                        buff[0] = LdBoot_256k_JTEC[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case Bootloader_Empty:
                default:
                {
                    bl_header[3] = ((0x0100 + sizeof(LdBoot_empty_SBEC3) - 1) >> 8) & 0xFF;
                    bl_header[4] = (0x0100 + sizeof(LdBoot_empty_SBEC3) - 1) & 0xFF;

                    for (uint8_t i = 0; i < sizeof(bl_header); i++)
                    {
                        buff[0] = bl_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdBoot_empty_SBEC3); i++)
                    {
                        buff[0] = LdBoot_empty_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
            }

            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

            sci.state.idle = false;

            sci_set_timeout(5 * SCI_LS_T1_DELAY);

            for (uint8_t i = 0; i < sizeof(start_bootloader_cmd); i++)
            {
                buff[0] = start_bootloader_cmd[i];
                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                sci.msg.byte_received = false;
            }

            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

            vTaskDelay(pdMS_TO_TICKS(100));

            if (!sci.msg.byte_received || (sci.msg.last_rx_length < 4))
            {
                ret[0] = BootloaderError_StartBootloaderTimeout;
                goto stop_bl;
            }

            if (sci.msg.rx_buffer[sci.msg.last_rx_length - 1] != 0x22)
            {
                ret[0] = BootloaderError_UnexpectedBootloaderStatusByte;
                goto stop_bl;
            }

            stop_bl:
            send_usb_packet(Bus_USB, Command_Debug, Debug_InitBootstrapMode, ret, sizeof(ret));
            sci_set_timeout(SCI_LS_T3_DELAY);
        }

        if (sci.bootstrap.upload_worker_function)
        {
            // Status bytes:
            // 0 = ok
            // 1 = no response to ping
            // 2 = upload finished status byte not received
            // 3 = unexpected upload finished status

            ret[0] = WorkerFunctionError_OK;
            sci.bootstrap.upload_worker_function = false;
            sci.msg.byte_received = false;
            sci.state.idle = false;
            sci.msg.tx_count = 1;
            uart_flush_input(UART_SCI);
            uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
            sci_set_timeout(5 * SCI_LS_T1_DELAY);
            uart_write_bytes(UART_SCI, (const uint8_t *)wf_header, 1); // ping with first byte of the header
            vTaskDelay(pdMS_TO_TICKS(10));

            if (!sci.msg.byte_received)
            {
                ret[0] = WorkerFunctionError_NoResponseToPing;
                goto stop_wf;
            }

            sci.msg.byte_received = false;

            switch (sci.bootstrap.worker_function_src)
            {
                case WorkerFunction_PartNumberRead:
                {
                    wf_header[1] = (sizeof(LdPartNumberRead_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdPartNumberRead_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdPartNumberRead_SBEC3); i++)
                    {
                        buff[0] = LdPartNumberRead_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case WorkerFunction_FlashID:
                {
                    wf_header[1] = (sizeof(LdFlashID_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdFlashID_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdFlashID_SBEC3); i++)
                    {
                        buff[0] = LdFlashID_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case WorkerFunction_FlashRead:
                {
                    wf_header[1] = (sizeof(LdFlashRead_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdFlashRead_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdFlashRead_SBEC3); i++)
                    {
                        buff[0] = LdFlashRead_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case WorkerFunction_FlashErase:
                {
                    switch (sci.bootstrap.flash_chip_src)
                    {
                        case FlashMemoryTypeIndex_M28F102: // 128 kB
                        case FlashMemoryTypeIndex_CAT28F102: // 128 kB
                        case FlashMemoryTypeIndex_N28F010: // 128 kB
                        {
                            wf_header[1] = (sizeof(LdFlashErase_M28F102_128k) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdFlashErase_M28F102_128k) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdFlashErase_M28F102_128k); i++)
                            {
                                buff[0] = LdFlashErase_M28F102_128k[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                        case FlashMemoryTypeIndex_N28F020: // 256 kB
                        case FlashMemoryTypeIndex_TMS28F210: // 256 kB
                        {
                            wf_header[1] = (sizeof(LdFlashErase_N28F020_256k) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdFlashErase_N28F020_256k) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdFlashErase_N28F020_256k); i++)
                            {
                                buff[0] = LdFlashErase_N28F020_256k[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                        case FlashMemoryTypeIndex_M28F210: // 256 kB
                        case FlashMemoryTypeIndex_M28F220: // 256 kB
                        case FlashMemoryTypeIndex_M28F200: // 256 kB
                        {
                            wf_header[1] = (sizeof(LdFlashErase_M28F220_256k) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdFlashErase_M28F220_256k) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdFlashErase_M28F220_256k); i++)
                            {
                                buff[0] = LdFlashErase_M28F220_256k[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                        case FlashMemoryTypeIndex_Unknown:
                        default:
                        {
                            wf_header[1] = (sizeof(LdWorker_empty_SBEC3) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdWorker_empty_SBEC3) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdWorker_empty_SBEC3); i++)
                            {
                                buff[0] = LdWorker_empty_SBEC3[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                    }
                    break;
                }
                case WorkerFunction_FlashWrite:
                {
                    switch (sci.bootstrap.flash_chip_src)
                    {
                        case FlashMemoryTypeIndex_M28F102: // 128 kB
                        case FlashMemoryTypeIndex_CAT28F102: // 128 kB
                        case FlashMemoryTypeIndex_N28F010: // 128 kB
                        case FlashMemoryTypeIndex_N28F020: // 256 kB
                        case FlashMemoryTypeIndex_TMS28F210: // 256 kB
                        {
                            wf_header[1] = (sizeof(LdFlashWrite_M28F102_128k) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdFlashWrite_M28F102_128k) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdFlashWrite_M28F102_128k); i++)
                            {
                                buff[0] = LdFlashWrite_M28F102_128k[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                        case FlashMemoryTypeIndex_M28F210: // 256 kB
                        case FlashMemoryTypeIndex_M28F220: // 256 kB
                        case FlashMemoryTypeIndex_M28F200: // 256 kB
                        {
                            wf_header[1] = (sizeof(LdFlashWrite_M28F220_256k) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdFlashWrite_M28F220_256k) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdFlashWrite_M28F220_256k); i++)
                            {
                                buff[0] = LdFlashWrite_M28F220_256k[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                        case FlashMemoryTypeIndex_Unknown:
                        default:
                        {
                            wf_header[1] = (sizeof(LdWorker_empty_SBEC3) >> 8) & 0xFF;
                            wf_header[2] = sizeof(LdWorker_empty_SBEC3) & 0xFF;

                            for (uint8_t i = 1; i < sizeof(wf_header); i++)
                            {
                                buff[0] = wf_header[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }

                            for (uint16_t i = 0; i < sizeof(LdWorker_empty_SBEC3); i++)
                            {
                                buff[0] = LdWorker_empty_SBEC3[i];
                                uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                                while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                                sci.msg.byte_received = false;
                            }
                            break;
                        }
                    }
                    break;
                }
                
                case WorkerFunction_EEPROMReadSPI:
                {
                    wf_header[1] = (sizeof(LdEEPROMRead_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdEEPROMRead_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdEEPROMRead_SBEC3); i++)
                    {
                        buff[0] = LdEEPROMRead_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case WorkerFunction_EEPROMWriteSPI:
                {
                    wf_header[1] = (sizeof(LdEEPROMWrite_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdEEPROMWrite_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdEEPROMWrite_SBEC3); i++)
                    {
                        buff[0] = LdEEPROMWrite_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
                case WorkerFunction_VerifyFlashChecksum:
                case WorkerFunction_Empty:
                default:
                {
                    wf_header[1] = (sizeof(LdWorker_empty_SBEC3) >> 8) & 0xFF;
                    wf_header[2] = sizeof(LdWorker_empty_SBEC3) & 0xFF;

                    for (uint8_t i = 1; i < sizeof(wf_header); i++)
                    {
                        buff[0] = wf_header[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }

                    for (uint16_t i = 0; i < sizeof(LdWorker_empty_SBEC3); i++)
                    {
                        buff[0] = LdWorker_empty_SBEC3[i];
                        uart_write_bytes(UART_SCI, (const uint8_t *)buff, 1);
                        while (!sci.msg.byte_received && !sci.state.idle); // wait for echo
                        sci.msg.byte_received = false;
                    }
                    break;
                }
            }

            while (!sci.state.idle) vTaskDelay(pdMS_TO_TICKS(1));

            vTaskDelay(pdMS_TO_TICKS(100));

            if (!sci.msg.byte_received)
            {
                ret[0] = WorkerFunctionError_UploadInterrupted;
                goto stop_wf;
            }

            if (sci.msg.rx_buffer[sci.msg.last_rx_length - 1] != 0x14)
            {
                ret[0] = WorkerFunctionError_UnexpectedUploadResult;
                goto stop_wf;
            }

            stop_wf:
            send_usb_packet(Bus_USB, Command_Debug, Debug_UploadWorkerFunction, ret, sizeof(ret));
            sci_set_timeout(SCI_LS_T3_DELAY);
        }

        if (sci.bootstrap.start_worker_function)
        {
            sci.bootstrap.start_worker_function = false;
            sci.msg.tx_count = 1;

            sci_set_timeout(SCI_LS_T3_DELAY);

            switch (sci.bootstrap.worker_function_src)
            {
                case WorkerFunction_FlashID:
                {
                    sci.msg.byte_received = false;
                    uart_write_bytes(UART_SCI, (const uint8_t *)start_worker_function_cmd, sizeof(start_worker_function_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    vTaskDelay(pdMS_TO_TICKS(1));
                    apply_progvolt(1 << SCI_VPP_EN_BIT);
                    vTaskDelay(pdMS_TO_TICKS(100));
                    apply_progvolt(0);
                    break;
                }
                case WorkerFunction_FlashErase:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)start_worker_function_cmd, sizeof(start_worker_function_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred

                    if (sci.bootstrap.flash_chip_src == 0) break; // unsupported erase mode

                    vTaskDelay(pdMS_TO_TICKS(1));
                    apply_progvolt(1 << SCI_VPP_EN_BIT);
                    sci.msg.byte_received = false;
                    while (!sci.msg.byte_received) vTaskDelay(pdMS_TO_TICKS(1));
                    vTaskDelay(pdMS_TO_TICKS(25));
                    apply_progvolt(0);
                    break;
                }
                case WorkerFunction_PartNumberRead:
                case WorkerFunction_FlashRead:
                case WorkerFunction_EEPROMReadSPI:
                case WorkerFunction_FlashWrite:
                case WorkerFunction_VerifyFlashChecksum:
                case WorkerFunction_EEPROMWriteSPI:
                case WorkerFunction_Empty:
                default:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)start_worker_function_cmd, sizeof(start_worker_function_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    break;
                }
            }
        }

        if (sci.bootstrap.exit_worker_function)
        {
            sci.bootstrap.exit_worker_function = false;
            sci.msg.tx_count = 1;

            sci_set_timeout(SCI_LS_T3_DELAY);

            switch (sci.bootstrap.worker_function_src)
            {
                case WorkerFunction_FlashRead:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)stop_flash_read_cmd, sizeof(stop_flash_read_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    break;
                }
                case WorkerFunction_FlashWrite:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)stop_flash_write_cmd, sizeof(stop_flash_write_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    break;
                }
                case WorkerFunction_EEPROMReadSPI:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)stop_eeprom_read_cmd, sizeof(stop_eeprom_read_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    break;
                }
                case WorkerFunction_EEPROMWriteSPI:
                {
                    uart_write_bytes(UART_SCI, (const uint8_t *)stop_eeprom_write_cmd, sizeof(stop_eeprom_write_cmd));
                    uart_wait_tx_idle_polling(UART_SCI); // wait until all bytes are transferred
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        vTaskDelay(1);
    }

    vTaskDelete(NULL);
}

/**
 * @brief ISR handler to decode PCI-bus symbols into byte values. Called every time the PCI-bus pin changes state.
 * 
 * @param arg Unused parameters.
 */
void IRAM_ATTR pci_protocol_decoder_isr_handler(void *arg)
{
    pci.state.idle = false;
    pci.rx.now = micros();

    if (gpio_get_level(PCI_RX_PIN) == pci.state.passive_bus_level) // passive bus
    {
        timer_set_counter_value(PCI_IDLE_TIMER_GROUP, PCI_IDLE_TIMER, 0);
        timer_start(PCI_IDLE_TIMER_GROUP, PCI_IDLE_TIMER);
    }
    else // active bus
    {
        timer_pause(PCI_IDLE_TIMER_GROUP, PCI_IDLE_TIMER);

        if (!pci.tx.complete && (pci.state.bus_level == pci.state.passive_bus_level)) // bus arbitration lost
        {
            timer_pause(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER); // stop transmitting new data
        }
    }

    pci.rx.diff = pci.rx.now - pci.rx.last_change;
    pci.rx.last_change = pci.rx.now;
    pci.rx.last_state = !gpio_get_level(PCI_RX_PIN);

    if (pci.rx.diff < J1850VPW_RX_SRT_MIN) // too short to be a valid pulse
    {
        pci.rx.sof_read = false;
        return;
    }

    if (!pci.rx.sof_read)
    {
        if ((pci.rx.sof_read = ((pci.rx.last_state == pci.state.active_bus_level) && IS_BETWEEN(pci.rx.diff, J1850VPW_RX_SOF_MIN, J1850VPW_RX_SOF_MAX))))
        {
            pci.rx.bit_pos = 0;
            pci.rx.ifr_detected = false;
        }

        return;
    }

    if (!pci.rx.ifr_detected)
    {
        if (pci.rx.last_state == pci.state.passive_bus_level)
        {
            if (IS_BETWEEN(pci.rx.diff, J1850VPW_RX_EOD_MIN, J1850VPW_RX_EOD_MAX)) // data ended and IFR detected
            {
                pci.rx.ifr_detected = true; // set flag to ignore incoming IFR
                return;
            }

            if (IS_BETWEEN(pci.rx.diff, J1850VPW_RX_LNG_MIN, J1850VPW_RX_LNG_MAX))
            {
                pci_rx_buf[pci_rx_ptr] |= (1 << (7 - pci.rx.bit_pos)); // save passive 1 bit
            }
        }
        else // active level
        {
            if (pci.rx.diff <= J1850VPW_RX_SRT_MAX)
            {
                pci_rx_buf[pci_rx_ptr] |= (1 << (7 - pci.rx.bit_pos)); // save active 1 bit
            }
        }

        pci.rx.bit_pos++;

        if (pci.rx.bit_pos == 8)
        {
            pci.rx.bit_pos = 0;
            pci_rx_ptr++;

            if (pci.msg.rx_ptr > PCI_RX_BUF_SIZE) pci.msg.rx_ptr = 0;
        }
    }

    if (pci.msg.rx_ptr >= PCI_MAX_FRAME_LENGTH)
    {
        // TODO
    } 
}

/**
 * @brief ISR callback to finish PCI-bus idle detection, copy last received message and save message length.
 * 
 * @param arg Unused parameters.
 * 
 * @return Always true.
 */
bool IRAM_ATTR pci_idle_timer_isr_callback(void *arg)
{
    pci.state.idle = true;
    timer_pause(PCI_IDLE_TIMER_GROUP, PCI_IDLE_TIMER);
    pci.tx.complete = true;
    pci.rx.ifr_detected = false;
    pci.rx.sof_read = false;

    for (int i = 0; i < pci_rx_ptr; i++)
    {
        pci.msg.rx_buffer[i] = pci_rx_buf[i];
    }

    pci.msg.rx_length = pci_rx_ptr;
    pci_rx_ptr = 0;
    memset(pci_rx_buf, 0, sizeof(pci_rx_buf));

    return true;
}

/**
 * @brief ISR callback to write a message to the PCI-bus. It encodes the correct pulse lengths of the individual symbols.
 * 
 * @param arg Unused parameters.
 * 
 * @return Always true.
 */
bool IRAM_ATTR pci_symbol_timer_isr_callback(void *arg)
{
    if (pci.tx.bit_write)
    {
        pci.state.bus_level = !pci.state.bus_level; // alternate between passive and active bus states

        if (pci.msg.tx_count == 1)
        {
            pci.tx.current_tx_bit = pci.msg.tx_buffer[pci.msg.tx_ptr] & (1 << (7 - pci.tx.bit_num_write));
        }
        else if (pci.msg.tx_count > 1)
        {
            pci.tx.current_tx_bit = pci_tx_buf[pci_tx_ptr] & (1 << (7 - pci.tx.bit_num_write));
        }

        if (pci.state.bus_level == pci.state.passive_bus_level) // passive bus
        {
            gpio_set_level(PCI_TX_PIN, pci.state.passive_bus_level);

            if (pci.tx.current_tx_bit) // logic 1
            {
                timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_LNG); // write correct pulse length
            }
            else // logic 0
            {
                timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_SRT); // write correct pulse length
            }
        }
        else // active bus
        {
            gpio_set_level(PCI_TX_PIN, pci.state.active_bus_level);

            if (pci.tx.current_tx_bit) // logic 1
            {
                timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_SRT); // write correct pulse length
            }
            else // logic 0
            {
                timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_LNG); // write correct pulse length
            }
        }

        pci.tx.bit_num_write++; // next bit

        if (pci.msg.tx_count == 1)
        {
            if (pci.tx.bit_num_write == 8) // byte complete
            {
                pci.tx.bit_num_write = 0;
                pci.msg.tx_ptr++; // next byte in buffer

                if (pci.msg.tx_ptr == pci.msg.tx_length) // all bytes have been transmitted
                {
                    pci.tx.bit_write = false;
                    pci.tx.eof_write = true;
                }
            }
        }
        else if (pci.msg.tx_count > 1)
        {
            if (pci.tx.bit_num_write == 8) // byte complete
            {
                pci.tx.bit_num_write = 0;
                pci_tx_ptr++; // next byte in buffer

                if (pci_tx_ptr == pci.msg.tx_length) // all bytes have been transmitted
                {
                    pci.tx.bit_write = false;
                    pci.tx.eof_write = true;
                }
            }
        }

        return true;
    }

    if (pci.tx.eof_write)
    {
        pci.tx.eof_write = false;
        gpio_set_level(PCI_TX_PIN, pci.state.passive_bus_level);
        timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_EOF); // write correct pulse length
        return true;
    }

    timer_pause(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER);
    timer_set_counter_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, 0);
    timer_set_alarm_value(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_SOF);
    pci.tx.complete = true;
    return true;
}

/**
 * @brief Initialize PCI-bus idle timer.
 * 
 * @param group Hardware timer group.
 * @param timer Hardware timer.
 * @param bus_idle_us Number of microseconds elapsed after last received passive symbol to consider bus idling.
 */
void pci_idle_timer_init(int group, int timer, uint16_t bus_idle_us)
{
    // Select and initialize basic parameters of the timer.
    timer_config_t config = {
        .divider = PCI_TIMER_DIVIDER,
        .counter_dir = TIMER_COUNT_UP,
        .counter_en = TIMER_PAUSE,
        .alarm_en = TIMER_ALARM_EN,
        .auto_reload = true
    }; // default clock source is APB

    timer_init(group, timer, &config);

    // Timer's counter will initially start from value below.
    // Also, if auto_reload is set, this value will be automatically reload on alarm.
    timer_set_counter_value(group, timer, 0);

    // Configure the alarm value and the interrupt on alarm.
    timer_set_alarm_value(group, timer, bus_idle_us);
    timer_enable_intr(group, timer);
    timer_isr_callback_add(group, timer, &pci_idle_timer_isr_callback, NULL, 0);
    timer_start(group, timer);
}

/**
 * @brief Initialize PCI-bus symbol timer.
 * 
 * @param group Hardware timer group.
 * @param timer Hardware timer.
 * @param symbol_us Symbol length in microseconds.
 */
void pci_symbol_timer_init(int group, int timer, uint16_t symbol_us)
{
    // Select and initialize basic parameters of the timer.
    timer_config_t config = {
        .divider = PCI_TIMER_DIVIDER,
        .counter_dir = TIMER_COUNT_UP,
        .counter_en = TIMER_PAUSE,
        .alarm_en = TIMER_ALARM_EN,
        .auto_reload = true
    }; // default clock source is APB

    timer_init(group, timer, &config);

    // Timer's counter will initially start from value below.
    // Also, if auto_reload is set, this value will be automatically reload on alarm.
    timer_set_counter_value(group, timer, 0);

    // Configure the alarm value and the interrupt on alarm.
    timer_set_alarm_value(group, timer, symbol_us);
    timer_enable_intr(group, timer);
    timer_isr_callback_add(group, timer, &pci_symbol_timer_isr_callback, NULL, 0);
}

/**
 * @brief Task to handle and store received PCI-bus message bytes.
 * 
 * @param pvParameters Unused parameters.
 */
void pci_event_task(void *pvParameters)
{
    for (EVER)
    {
        vTaskDelay(1);
    }

    vTaskDelete(NULL);
}

/**
 * @brief Task to start analyzing PCI-bus messages.
 * 
 * @param pvParameters Unused parameters.
 */
void pci_msg_task(void *pvParameters)
{
    uint8_t crc = 0;

    for (EVER)
    {
        if (pci.msg.rx_length > 0)
        {
            crc = calculate_crc(pci.msg.rx_buffer, 0, pci.msg.rx_length - 1);

            if (pci.msg.rx_buffer[pci.msg.rx_length - 1] == crc)
            {
                if (pci.state.en)
                {
                    send_usb_packet(Bus_PCI, Command_Receive, Receive_PCI, pci.msg.rx_buffer, pci.msg.rx_length);
                }
            }

            if (pci.repeat.en)
            {
                if (pci.msg.tx_count == 1) // if there's only one message in the buffer
                {
                    bool match = true;

                    for (int i = 0; i < pci.msg.rx_length; i++)
                    {
                        if (pci.msg.tx_buffer[i] != pci.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                    }

                    if (match) pci.repeat.next = true; // if echo is correct prepare next message
                }
                else if (pci.msg.tx_count > 1) // multiple messages
                {
                    bool match = true;

                    for (int i = 0; i < pci.msg.rx_length; i++)
                    {
                        if (pci.msg.tx_buffer[pci.msg.tx_ptr + 1 + i] != pci.msg.rx_buffer[i]) match = false; // compare received bytes with message sent
                    }

                    if (match)
                    {
                        pci.repeat.next = true; // if echo is correct prepare next message

                        // Increase the current message counter and set the buffer pointer to the next message length.
                        pci.msg.tx_count_ptr++;
                        pci.msg.tx_ptr += pci.msg.tx_length + 1;
                        pci.msg.tx_length = pci.msg.tx_buffer[pci.msg.tx_ptr]; // re-calculate new message length

                        // After the last message reset everything to zero to start at the beginning.
                        if (pci.msg.tx_count_ptr == pci.msg.tx_count)
                        {
                            pci.msg.tx_count_ptr = 0;
                            pci.msg.tx_ptr = 0;
                            pci.msg.tx_length = pci.msg.tx_buffer[pci.msg.tx_ptr]; // re-calculate new message length

                            if (pci.repeat.list_once) pci.repeat.stop = true;
                        }
                    }
                }

                if (pci.repeat.stop) // one-shot message list is terminated here
                {
                    pci.msg.tx_ptr = 0;
                    pci.repeat.en = false;
                    pci.repeat.next = false;
                    pci.repeat.list_once = false;
                }
            }

            pci.msg.rx_length = 0;
            pci.stats.msg_rx_count++;
        }

        // Repeated messages are prepared here.
        if (pci.repeat.en)
        {
            if ((uint32_t)(millis() - pci.repeat.last_millis) >= pci.repeat.interval) // wait between messages
            {
                pci.repeat.last_millis = millis();

                if (pci.repeat.next)
                {
                    pci.state.msg_tx_pending = true; // set flag
                    pci.repeat.next = false;
                }
            }
        }

        if (pci.state.msg_tx_pending && pci.state.idle)
        {
            pci.rx.sof_read = false;
            pci.tx.bit_write = true;
            pci.tx.bit_num_write = 0;
            pci.tx.current_tx_bit = 0;
            pci.tx.eof_write = false;
            pci.tx.complete = false;
            pci.rx.ifr_detected = false;
            pci.state.bus_level = pci.state.active_bus_level;

            if (pci.msg.tx_count == 1)
            {
                pci.msg.tx_ptr = 0;
            }
            else if (pci.msg.tx_count > 1) // multiple messages, send one at a time
            {
                // Make a local copy of the current message.
                int j = 0;

                for (int i = (pci.msg.tx_ptr + 1); i < (pci.msg.tx_ptr + 1 + pci.msg.tx_length); i++)
                {
                    pci_tx_buf[j] = pci.msg.tx_buffer[i];
                    j++;
                }

                pci_tx_ptr = 0;
            }

            timer_start(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER);
            gpio_set_level(PCI_TX_PIN, pci.state.active_bus_level);
            pci.state.msg_tx_pending = false;
            pci.stats.msg_tx_count++;
        }

        vTaskDelay(1);
    }

    vTaskDelete(NULL);
}

/**
 * @brief Blink specified LED.
 * 
 * @param LED LED to blink (RX_LED_PIN / TX_LED_PIN / ACT_LED_PIN).
 */
void blink_led(uint8_t LED)
{
    if (leds_enabled)
    {
        if (LED != ALL_LED) gpio_set_level(LED, LED_ON); // turn on LED

        switch (LED) // save time when LED was turned on
        {
            case RX_LED_PIN:
            {
                rx_led_ontime = millis();
                break;
            }
            case TX_LED_PIN:
            {
                tx_led_ontime = millis();
                break;
            }
            case PWR_LED_PIN:
            {
                pwr_led_ontime = millis();
                break;
            }
            case ACT_LED_PIN:
            {
                act_led_ontime = millis();
                break;
            }
            case ALL_LED:
            {
                rx_led_ontime = millis();
                tx_led_ontime = rx_led_ontime;
                pwr_led_ontime = rx_led_ontime;
                act_led_ontime = rx_led_ontime;
                gpio_set_level(RX_LED_PIN, LED_ON); // turn on LED
                gpio_set_level(TX_LED_PIN, LED_ON); // turn on LED
                gpio_set_level(PWR_LED_PIN, LED_ON); // turn on LED
                gpio_set_level(ACT_LED_PIN, LED_ON); // turn on LED
                break;
            }
            default:
            {
                break;
            }
        }
    }
}

/**
 * @brief Task to handle LED blinks.
 * 
 * @param pvParameters Unused parameters.
 */
void led_task(void *pvParameters)
{
    for (EVER)
    {
        current_millis = millis(); // check current time

        if (heartbeat_enabled)
        {
            if ((uint32_t)(current_millis - previous_act_blink) >= heartbeat_interval)
            {
                previous_act_blink = current_millis; // save current time
                blink_led(ACT_LED_PIN);
            }
        }
        if ((uint32_t)(current_millis - rx_led_ontime) >= led_blink_duration)
        {
            gpio_set_level(RX_LED_PIN, LED_OFF); // turn off RX LED
        }
        if ((uint32_t)(current_millis - tx_led_ontime) >= led_blink_duration)
        {
            gpio_set_level(TX_LED_PIN, LED_OFF); // turn off TX LED
        }
        if ((uint32_t)(current_millis - act_led_ontime) >= led_blink_duration)
        {
            gpio_set_level(ACT_LED_PIN, LED_OFF); // turn off ACT LED
        }

        vTaskDelay(pdMS_TO_TICKS(1));
    }

    vTaskDelete(NULL);
}

/**
 * @brief Initialize LED pins.
 */
void init_leds()
{
    gpio_reset_pin(RX_LED_PIN);
    gpio_set_direction(RX_LED_PIN, GPIO_MODE_OUTPUT);
    gpio_set_pull_mode(RX_LED_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(RX_LED_PIN, GPIO_PULLDOWN_DISABLE);
    // gpio_set_level(RX_LED_PIN, LED_OFF);

    gpio_reset_pin(TX_LED_PIN);
    gpio_set_direction(TX_LED_PIN, GPIO_MODE_OUTPUT);
    gpio_set_pull_mode(TX_LED_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(TX_LED_PIN, GPIO_PULLDOWN_DISABLE);
    // gpio_set_level(TX_LED_PIN, LED_OFF);

    gpio_reset_pin(PWR_LED_PIN);
    gpio_set_direction(PWR_LED_PIN, GPIO_MODE_OUTPUT);
    gpio_set_pull_mode(PWR_LED_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(PWR_LED_PIN, GPIO_PULLDOWN_DISABLE);
    // gpio_set_level(PWR_LED_PIN, LED_ON);

    gpio_reset_pin(ACT_LED_PIN);
    gpio_set_direction(ACT_LED_PIN, GPIO_MODE_OUTPUT);
    gpio_set_pull_mode(ACT_LED_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(ACT_LED_PIN, GPIO_PULLDOWN_DISABLE);
    // gpio_set_level(ACT_LED_PIN, LED_OFF);

    blink_led(ALL_LED);
}

/**
 * @brief Initialize USB communication.
 */
void init_usb()
{
    ESP_ERROR_CHECK(uart_param_config(UART_USB, &usb_config));
    ESP_ERROR_CHECK(uart_set_pin(UART_USB, USB_TX_PIN, USB_RX_PIN, UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE));
    ESP_ERROR_CHECK(uart_driver_install(UART_USB, USB_RX_BUF_SIZE, USB_TX_BUF_SIZE, 20, &usb_uart_queue, ESP_INTR_FLAG_SHARED));
}

/**
 * @brief Initialize CCD-bus communication.
 */
void init_ccd()
{
    configure_ccd_bus(ccd.bus_settings);
    ESP_ERROR_CHECK(uart_param_config(UART_CCD, &ccd_config));
    ESP_ERROR_CHECK(uart_set_pin(UART_CCD, CCD_TX_PIN, CCD_RX_PIN, UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE));
    ESP_ERROR_CHECK(uart_driver_install(UART_CCD, CCD_RX_BUF_SIZE, CCD_TX_BUF_SIZE, 20, &ccd_uart_queue, 0));
    ESP_ERROR_CHECK(uart_set_rx_full_threshold(UART_CCD, 1));

    gpio_reset_pin(CCD_IDLE_PIN);
    gpio_set_direction(CCD_IDLE_PIN, GPIO_MODE_INPUT);
    gpio_set_pull_mode(CCD_IDLE_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(CCD_IDLE_PIN, GPIO_PULLDOWN_DISABLE);
    gpio_set_intr_type(CCD_IDLE_PIN, GPIO_INTR_NEGEDGE);
    gpio_install_isr_service(0);
    gpio_isr_handler_add(CCD_IDLE_PIN, ccd_startbit_sense_isr_handler, NULL);

    ccd.msg.tx_count = 1;
    ccd.repeat.interval = 100; // ms by default

    ccd_idle_timer_init(CCD_IDLE_TIMER_GROUP, CCD_IDLE_TIMER, CCD_IDLE_BITS_10);
}

/**
 * @brief Initialize SCI-bus communication.
 */
void init_sci()
{
    configure_sci_bus(sci.bus_settings);
    ESP_ERROR_CHECK(uart_param_config(UART_SCI, &sci_config));
    ESP_ERROR_CHECK(uart_set_pin(UART_SCI, SCI_TX_PIN, SCI_RX_PIN, UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE));
    ESP_ERROR_CHECK(uart_driver_install(UART_SCI, SCI_RX_BUF_SIZE, SCI_TX_BUF_SIZE, 20, &sci_uart_queue, ESP_INTR_FLAG_SHARED));
    ESP_ERROR_CHECK(uart_set_rx_full_threshold(UART_SCI, 1));
    sci_idle_timer_init(SCI_IDLE_TIMER_GROUP, SCI_IDLE_TIMER, SCI_LS_T3_DELAY);
    sci_set_timeout(SCI_LS_T3_DELAY);

    sci.msg.tx_count = 1;
    sci.repeat.interval = 100; // ms by default
}

/**
 * @brief Initialize PCI-bus communication.
 */
void init_pci()
{
    configure_pci_bus(pci.bus_settings);
    gpio_reset_pin(PCI_RX_PIN);
    gpio_set_direction(PCI_RX_PIN, GPIO_MODE_INPUT);
    gpio_set_pull_mode(PCI_RX_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(PCI_RX_PIN, GPIO_PULLDOWN_DISABLE);
    gpio_set_intr_type(PCI_RX_PIN, GPIO_INTR_ANYEDGE);
    gpio_install_isr_service(0);
    gpio_isr_handler_add(PCI_RX_PIN, pci_protocol_decoder_isr_handler, NULL);

    gpio_reset_pin(PCI_TX_PIN);
    gpio_set_direction(PCI_TX_PIN, GPIO_MODE_OUTPUT);
    gpio_set_pull_mode(PCI_TX_PIN, GPIO_PULLUP_DISABLE);
    gpio_set_pull_mode(PCI_TX_PIN, GPIO_PULLDOWN_DISABLE);
    gpio_set_level(PCI_TX_PIN, pci.state.passive_bus_level);

    pci.msg.tx_count = 1;
    pci.repeat.interval = 100; // ms by default

    pci_idle_timer_init(PCI_IDLE_TIMER_GROUP, PCI_IDLE_TIMER, J1850VPW_RX_IFS_MIN);
    pci_symbol_timer_init(PCI_SYMBOL_TIMER_GROUP, PCI_SYMBOL_TIMER, J1850VPW_TX_SOF);
}

/**
 * @brief Initialize SPIFFS partition for data storage.
 */
void init_spiffs()
{
    esp_err_t result = esp_vfs_spiffs_register(&spiffs_config);

    if (result != ESP_OK)
    {
        send_usb_packet(Bus_USB, Command_Error, Error_SPIFFSError, err, sizeof(err));
    }

    //FILE* f = fopen("/spiffs/f0460611.399", "w+b"); // open in read/write binary mode (w+b or wb+)
    //fseek(f, 0, 0);
    // uint8_t data[1];

    // if (f != NULL)
    // {
    //     fwrite((const uint8_t *)err, sizeof(uint8_t), sizeof(err), f); // 1: what, 2: variable size, 3: number of elements to write, 4: target file
    //     fread(data, sizeof(uint8_t), sizeof(err), f); // 1: destination, 2: variable size, 3: number of elements to read, 4: source file
    //     fclose(f);
    // }
}

/**
 * @brief Load settings from previous session.
 */
void load_settings()
{
    esp_efuse_coding_scheme_t coding_scheme = get_coding_scheme();
    (void) coding_scheme;

    read_device_desc_efuse_fields(&device_desc); // get hardware version

    if (device_desc.hardware_version == 0) // check if hardware version is present in the custom efuse table
    {
        device_desc.hardware_version = LATEST_HARDWARE_VERSION; // apply latest hardware version
        write_device_desc_efuse_fields(&device_desc, coding_scheme); // write to custom efuse table
        read_device_desc_efuse_fields(&device_desc); // get hardware version again
    }

    esp_err_t result = nvs_flash_init();

    if ((result == ESP_ERR_NVS_NO_FREE_PAGES) || (result == ESP_ERR_NVS_NEW_VERSION_FOUND))
    //if (result != ESP_OK)
    {
        send_usb_packet(Bus_USB, Command_Error, Error_ESP32NVSInit, err, sizeof(err));
        nvs_flash_erase(); // NVS partition was truncated and needs to be erased
        result = nvs_flash_init(); // retry init

        if (result != ESP_OK)
        {
            send_usb_packet(Bus_USB, Command_Error, Error_ESP32NVSInit, err, sizeof(err));
            return;
        }
    }

    if (open_settings())
    {
        // Check if default settings have been written yet.
        if ((nvs_get_u16(settings_handle, "hb_interval", &heartbeat_interval) != ESP_OK) &&
            (nvs_get_u16(settings_handle, "blink_duration", &led_blink_duration) != ESP_OK) &&
            (nvs_get_u8(settings_handle, "ccd_settings", &ccd.bus_settings) != ESP_OK) &&
            (nvs_get_u8(settings_handle, "pci_settings", &pci.bus_settings) != ESP_OK) &&
            (nvs_get_u8(settings_handle, "sci_settings", &sci.bus_settings) != ESP_OK))
        {
            close_settings();
            default_settings();
        }
    }

    if (open_settings())
    {
        if (nvs_get_u16(settings_handle, "hb_interval", &heartbeat_interval) != ESP_OK)
        {
            heartbeat_interval = 5000; // ms
        }
        if (nvs_get_u16(settings_handle, "blink_duration", &led_blink_duration) != ESP_OK)
        {
            led_blink_duration = 50; // ms
        }
        if (nvs_get_u8(settings_handle, "ccd_settings", &ccd.bus_settings) != ESP_OK)
        {
            ccd.bus_settings = 0x41;
        }
        if (nvs_get_u8(settings_handle, "pci_settings", &pci.bus_settings) != ESP_OK)
        {
            pci.bus_settings = 0x40;
        }
        if (nvs_get_u8(settings_handle, "sci_settings", &sci.bus_settings) != ESP_OK)
        {
            sci.bus_settings = 0x81;
        }

        nvs_close(settings_handle);

        heartbeat_interval ? (heartbeat_enabled = true) : (heartbeat_enabled = false);
        led_blink_duration ? (leds_enabled = true) : (leds_enabled = false);
        if (ccd.bus_settings == 0) ccd.bus_settings = 0x41;
        if (pci.bus_settings == 0) pci.bus_settings = 0x40;
        if (sci.bus_settings == 0) sci.bus_settings = 0x81;

        if (leds_enabled)
        {
            gpio_set_level(PWR_LED_PIN, LED_ON); // turn on PWR LED
        }
        else
        {
            gpio_set_level(PWR_LED_PIN, LED_OFF); // turn off PWR LED
        }
    }
}

/**
 * @brief Task to do debug stuff.
 * 
 * @param pvParameters Unused parameters.
 */
void debug_task(void *pvParameters)
{
    for (EVER)
    {
        vTaskDelay(pdMS_TO_TICKS(100));
    }

    vTaskDelete(NULL);
}

/**
 * @brief Main application.
 */
void app_main()
{
    init_leds();
    init_usb();
    init_i2c();
    load_settings();
    init_ccd();
    init_pci();
    init_sci();
    init_adc();
    //init_spiffs();

    uint8_t reset_reason[1] = { esp_reset_reason() };
    send_usb_packet(Bus_USB, Command_Reset, Reset_Done, reset_reason, sizeof(reset_reason));

    xTaskCreate(led_task, "led_task", 8192, NULL, 11, NULL); // create a task to handle LED blinks
    xTaskCreate(usb_event_task, "usb_event_task", 8192, NULL, 2, NULL); // create a task to handle USB events
    xTaskCreate(usb_msg_task, "usb_msg_task", 8192, NULL, 3, NULL); // create a task to parse and execute received USB commands
    xTaskCreate(ccd_event_task, "ccd_event_task", 8192, NULL, 4, NULL); // create a task to handle CCD-bus communication
    xTaskCreate(ccd_msg_task, "ccd_msg_task", 8192, NULL, 5, NULL); // create a task to handle CCD-bus messages
    //xTaskCreate(pci_event_task, "pci_event_task", 8192, NULL, 6, NULL); // create a task to handle PCI-bus communication
    xTaskCreate(pci_msg_task, "pci_msg_task", 8192, NULL, 7, NULL); // create a task to handle PCI-bus messages
    xTaskCreate(sci_event_task, "sci_event_task", 8192, NULL, 8, NULL); // create a task to handle SCI-bus communication
    xTaskCreate(sci_msg_task, "sci_msg_task", 8192, NULL, 9, NULL); // create a task to handle SCI-bus messages
    xTaskCreate(sci_boot_task, "sci_boot_task", 8192, NULL, 10, NULL); // create a task to handle SCI-bus bootstrap mode
    //xTaskCreate(debug_task, "debug_task", 8192, NULL, 12, NULL); // create a task to do debug stuff
}
#define EVER                      ;;
#define LATEST_HARDWARE_VERSION   0x2100 // v2.1.0(.0)

#define UART_USB                  UART_NUM_0
#define USB_RX_BUF_SIZE           4096
#define USB_TX_BUF_SIZE           USB_RX_BUF_SIZE
#define USB_RX_PIN                3
#define USB_TX_PIN                1

#define UART_CCD                  UART_NUM_1
#define CCD_RX_BUF_SIZE           512
#define CCD_TX_BUF_SIZE           CCD_RX_BUF_SIZE
#define CCD_RX_PIN                19
#define CCD_TX_PIN                18
#define CCD_IDLE_PIN              23
#define CCD_TBEN_PIN              7
#define CCD_STATE_BIT             7
#define CCD_TBEN_BIT              6
#define CCD_IDLE_TIMER_DIVIDER    10240 // 80 MHz / 10240 = 7812.5 Hz (1 tick = 0.128 ms)
#define CCD_IDLE_BITS_10          10
#define CCD_IDLE_TIMER_GROUP      TIMER_GROUP_0
#define CCD_IDLE_TIMER            TIMER_0

#define UART_SCI                  UART_NUM_2
#define SCI_RX_BUF_SIZE           4096
#define SCI_TX_BUF_SIZE           SCI_RX_BUF_SIZE
#define SCI_RX_PIN                5
#define SCI_TX_PIN                4
#define SCI_TX15_EN_PIN           5
#define SCI_TX7_EN_PIN            4
#define SCI_RX14_EN_PIN           3
#define SCI_RX12_EN_PIN           2
#define SCI_RX9_EN_PIN            1
#define SCI_RX6_EN_PIN            0
#define SCI_VPP_TO_RX14_PIN       7
#define SCI_VPP_TO_RX12_PIN       6
#define SCI_VPP_TO_RX9_PIN        5
#define SCI_VPP_TO_RX6_PIN        4
#define SCI_VBB_TO_RX14_PIN       3
#define SCI_VBB_TO_RX12_PIN       2
#define SCI_VBB_TO_RX9_PIN        1
#define SCI_VBB_TO_RX6_PIN        0
#define SCI_STATE_BIT             7
#define SCI_MODULE_BIT            5
#define SCI_NGC_BIT               4
#define SCI_LOGIC_BIT             3
#define SCI_CONFIG_BIT            2
#define SCI_SPEED_BITS            3
#define SCI_SPEED_976_BAUD        0
#define SCI_SPEED_7812_BAUD       1
#define SCI_SPEED_62500_BAUD      2
#define SCI_SPEED_125000_BAUD     3
#define SCI_VBB_EN_BIT            7
#define SCI_VPP_EN_BIT            6
#define SCI_AW9523_LOG_BIT        5
#define SCI_LS_T1_DELAY           20   // ms
#define SCI_LS_T2_DELAY           100  // ms
#define SCI_LS_T3_DELAY           50   // ms
#define SCI_LS_T4_DELAY           20   // ms
#define SCI_LS_T5_DELAY           100  // ms
#define SCI_HS_T1_DELAY           1    // ms
#define SCI_HS_T2_DELAY           1    // ms
#define SCI_HS_T3_DELAY           5    // ms
#define SCI_HS_T4_DELAY           0    // ms
#define SCI_HS_T5_DELAY           0    // ms
#define SCI_IDLE_TIMER_DIVIDER    40000 // 80 MHz / 40000 = 2000 Hz (1 tick = 0.5 ms)
#define SCI_IDLE_TIMER_GROUP      TIMER_GROUP_0
#define SCI_IDLE_TIMER            TIMER_1

#define PCI_RX_BUF_SIZE           512
#define PCI_TX_BUF_SIZE           PCI_RX_BUF_SIZE
#define PCI_RX_PIN                14
#define PCI_TX_PIN                27
#define PCI_STATE_BIT             7
#define PCI_ACTIVE_LEVEL_BIT      6
#define PCI_SYMBOL_TIMER_GROUP    TIMER_GROUP_1
#define PCI_SYMBOL_TIMER          TIMER_0
#define PCI_IDLE_TIMER_GROUP      TIMER_GROUP_1
#define PCI_IDLE_TIMER            TIMER_1
#define PCI_TIMER_DIVIDER         80 // 80 MHz / 80 = 1 MHz (1 tick = 1 us)
#define PCI_MAX_FRAME_LENGTH      12
#define PCI_MAX_DATA_LENGTH       (PCI_MAX_MSG_LENGTH - 1)

#define I2C_SDA_PIN               21
#define I2C_SCL_PIN               22
#define I2C_CHANNEL               I2C_NUM_0
#define I2C_MASTER_RX_BUF_DISABLE 0
#define I2C_MASTER_TX_BUF_DISABLE 0

#define BATTVOLT_PIN              33
#define BATTVOLT_ADC_CHANNEL      ADC1_CHANNEL_5
#define BATTVOLT_SCALER           6.345679 // ((8660.0+1620.0)/1620.0)
#define BOOTVOLT_PIN              34
#define BOOTVOLT_ADC_CHANNEL      ADC1_CHANNEL_6
#define BOOTVOLT_SCALER           6.345679 // ((8660.0+1620.0)/1620.0)
#define PROGVOLT_PIN              35
#define PROGVOLT_ADC_CHANNEL      ADC1_CHANNEL_7
#define PROGVOLT_SCALER           12.234567 // ((18200.0+1620.0)/1620.0)
#define DEFAULT_VREF              1100 // mV
#define ADC_SAMPLE_NUM            1024

#define PACKET_SYNC_BYTE          0x3D // "=" symbol
#define MIN_PACKET_LENGTH         2
#define MAX_PACKET_LENGTH         (USB_RX_BUF_SIZE - 4)

#define RX_LED_PIN                25
#define TX_LED_PIN                26
#define PWR_LED_PIN               32
#define ACT_LED_PIN               13
#define ALL_LED                   0xFF
#define LED_ON                    1
#define LED_OFF                   0
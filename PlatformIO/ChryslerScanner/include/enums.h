enum Bus {
    Bus_USB = 0x00,
    Bus_CCD = 0x01,
    Bus_PCM = 0x02, // SCI
    Bus_TCM = 0x03, // SCI
    Bus_PCI = 0x04
};

enum Command {
    Command_Reset     = 0x00,
    Command_Handshake = 0x01,
    Command_Status    = 0x02,
    Command_Settings  = 0x03,
    Command_Request   = 0x04,
    Command_Response  = 0x05,
    Command_Transmit  = 0x06,
    Command_Receive   = 0x07,
    Command_Debug     = 0x0E,
    Command_Error     = 0x0F
};

enum Reset {
    Reset_InProgress = 0x00,
    Reset_Done = 0x01
};

enum Settings {
    Settings_Heartbeat = 0x01,
    Settings_CCD       = 0x02,
    Settings_SCI       = 0x03,
    Settings_Repeat    = 0x04,
    Settings_PCI       = 0x06,
    Settings_ProgVolt  = 0x07
};

enum Request {
    Request_Info         = 0x01,
    Request_Timestamp    = 0x02,
    Request_BatteryVolts = 0x03,
    Request_VBBVolts     = 0x06,
    Request_VPPVolts     = 0x07,
    Request_AllVolts     = 0x08
};

enum Transmit {
    Transmit_Stop         = 0x01,
    Transmit_Single       = 0x02,
    Transmit_List         = 0x03,
    Transmit_RepeatSingle = 0x04,
    Transmit_RepeatList   = 0x05,
    Transmit_Single_VPP   = 0x82
};

enum Receive {
    Receive_CCD             = 0x01,
    Receive_SCILowSpeedMsg  = 0x01,
    Receive_SCIHighSpeedMsg = 0x02,
    Receive_PCI             = 0x01
};

enum Debug {
    Debug_RandomCCDMessages    = 0x01,
    Debug_InitBootstrapMode    = 0x0B,
    Debug_UploadWorkerFunction = 0x0C,
    Debug_StartWorkerFunction  = 0x0D,
    Debug_ExitWorkerFunction   = 0x0E,
    Debug_DefaultSettings      = 0xE0,
    Debug_GetRandomNumber      = 0xE1,
    Debug_RestorePCMEEPROM     = 0xF0,
    Debug_GetAW9523Data        = 0xFE,
    Debug_Test                 = 0xFF
};

enum Error {
    Error_OK                      = 0x00,
    Error_Length                  = 0x01,
    Error_Datacode                = 0x02,
    Error_Subdatacode             = 0x03,
    Error_Payload                 = 0x04,
    Error_Checksum                = 0x05,
    Error_Timeout                 = 0x06,
    Error_BufferOverflow          = 0x07,
    Error_InvalidBus              = 0x08,
    Error_I2CPortExpanderNotFound = 0x09,
    Error_ESP32NVSInit            = 0x0A,
    Error_ESP32NVSOpen            = 0x0B,
    Error_SPIFFSError             = 0x0C,
    Error_SPIFFSPartitionNotFound = 0x0D,
    Error_SCILSNoResponse         = 0xF6,
    Error_SCIHSRAMPtrNoResponse   = 0xF8,
    Error_SCIHSInvalidRAMPtr      = 0xF9,
    Error_SCIHSNoResponse         = 0xFA,
    Error_Internal                = 0xFE,
    Error_Fatal                   = 0xFF
};

enum Bootloader {
    Bootloader_Empty             = 0x00,
    Bootloader_128k_SBEC3        = 0x01,
    Bootloader_128k_SBEC3_custom = 0x02,
    Bootloader_256k_SBEC3        = 0x03,
    Bootloader_256k_SBEC3_custom = 0x04,
    Bootloader_128k_EATX3        = 0x05,
    Bootloader_256k_EATX3        = 0x06,
    Bootloader_256k_JTEC         = 0x07
};

enum WorkerFunction {
    WorkerFunction_Empty               = 0x00,
    WorkerFunction_PartNumberRead      = 0x01,
    WorkerFunction_FlashID             = 0x02,
    WorkerFunction_FlashRead           = 0x03,
    WorkerFunction_FlashErase          = 0x04,
    WorkerFunction_FlashWrite          = 0x05,
    WorkerFunction_VerifyFlashChecksum = 0x06,
    WorkerFunction_EEPROMRead          = 0x07,
    WorkerFunction_EEPROMWrite         = 0x08,
};

enum FlashMemoryManufacturer
{
    FlashMemoryManufacturer_STMicroelectronics = 0x20,
    FlashMemoryManufacturer_CATALYST           = 0x31,
    FlashMemoryManufacturer_Intel              = 0x89,
    FlashMemoryManufacturer_TexasInstruments   = 0x97
};

enum FlashMemoryTypeIndex {
    FlashMemoryTypeIndex_Unknown   = 0x00,
    FlashMemoryTypeIndex_M28F102   = 0x01, // 0x50
    FlashMemoryTypeIndex_CAT28F102 = 0x02, // 0x51
    FlashMemoryTypeIndex_N28F010   = 0x03, // 0xB4
    FlashMemoryTypeIndex_N28F020   = 0x04, // 0xBD
    FlashMemoryTypeIndex_M28F210   = 0x05, // 0xE0
    FlashMemoryTypeIndex_M28F220   = 0x06, // 0xE6
    FlashMemoryTypeIndex_M28F200   = 0x07, // 0x74-0x75
    FlashMemoryTypeIndex_TMS28F210 = 0x08  // 0xE5
};

enum BootloaderError {
    BootloaderError_OK                             = 0x00,
    BootloaderError_NoResponseToMagicByte          = 0x01,
    BootloaderError_UnexpectedResponseToMagicByte  = 0x02,
    BootloaderError_SecuritySeedResponseTimeout    = 0x03,
    BootloaderError_SecuritySeedChecksumError      = 0x04,
    BootloaderError_SecurityKeyStatusTimeout       = 0x05,
    BootloaderError_SecurityKeyNotAccepted         = 0x06,
    BootloaderError_StartBootloaderTimeout         = 0x07,
    BootloaderError_UnexpectedBootloaderStatusByte = 0x08
};

enum WorkerFunctionError {
    WorkerFunctionError_OK                     = 0x00,
    WorkerFunctionError_NoResponseToPing       = 0x01,
    WorkerFunctionError_UploadInterrupted      = 0x02,
    WorkerFunctionError_UnexpectedUploadResult = 0x03
};

enum CCD_Operations {
    CCD_Read  = 0x00,
    CCD_Write = 0x01
};

enum CCD_Errors {
    CCD_OK                   = 0x00,
    CCD_ERR_BUS_IS_BUSY      = 0x81,
    CCD_ERR_BUS_ERROR        = 0x82,
    CCD_ERR_ARBITRATION_LOST = 0x87,
    CCD_ERR_CHECKSUM         = 0x90
};

enum J1850VPW_Symbols {// time intervals in microseconds (us)
    J1850VPW_TX_SRT = 64,  // Short
    J1850VPW_TX_LNG = 128, // Long
    J1850VPW_TX_SOF = 200, // Start of Frame
    J1850VPW_TX_EOD = 200, // End of Data, only when IFR (Inter-Frame Response) is used
    J1850VPW_TX_EOF = 280, // End of Frame
    J1850VPW_TX_BRK = 300, // Break
    J1850VPW_TX_IFS = 300, // Inter-Frame Separation
    J1850VPW_RX_SRT_MIN = 35,
    J1850VPW_RX_SRT_MAX = 96,
    J1850VPW_RX_LNG_MIN = 97,
    J1850VPW_RX_LNG_MAX = 163,
    J1850VPW_RX_SOF_MIN = 164,
    J1850VPW_RX_SOF_MAX = 239,
    J1850VPW_RX_EOD_MIN = 164,
    J1850VPW_RX_EOD_MAX = 239,
    J1850VPW_RX_EOF_MIN = 240,
    J1850VPW_RX_BRK_MIN = 240,
    J1850VPW_RX_BRK_MAX = 1000000,
    J1850VPW_RX_IFS_MIN = 281
};

enum PCI_Operations {
    PCI_Read  = 0x00,
    PCI_Write = 0x01
};

enum PCI_Errors {
    PCI_OK = 0x00
};

enum SCI_Operations {
    SCI_Read      = 0x00,
    SCI_Write     = 0x01,
    SCI_Bootstrap = 0x02,
    SCI_Erase     = 0x03,
    SCI_Program   = 0x04
};

enum SCI_Errors {
    SCI_OK = 0x00
};
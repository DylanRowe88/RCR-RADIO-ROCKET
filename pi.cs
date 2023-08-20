    using System.Device.Spi;
    using System.Device.Gpio;

    namespace SPS2._0
    {
        
        class RF95
        {
            // This is the maximum number of interrupts the driver can support
            // Most Arduinos can handle 2, Megas can handle more
            public const int NUM_INTERRUPTS = 3;

            // Max number of octets the LORA Rx/Tx FIFO can hold
            public const int FIFO_SIZE = 255;

            // This is the maximum number of bytes that can be carried by the LORA.
            // We use some for headers, keeping fewer for RadioHead messages
            public const int MAX_PAYLOAD_LEN = FIFO_SIZE;

            // The length of the headers we add.
            // The headers are inside the LORA's payload
            public const int HEADER_LEN = 4;

            // This is the maximum message length that can be supported by this driver. 
            // Can be pre-defined to a smaller size (to save SRAM) prior to including this header
            // Here we allow for 1 byte message length, 4 bytes headers, user data and 2 bytes of FCS
            public const int MAX_MESSAGE_LEN = (MAX_PAYLOAD_LEN - HEADER_LEN);

            // The crystal oscillator frequency of the module
            public const int FXOSC = 32000000;

            // The Frequency Synthesizer step = FXOSC / 2^^19
            public const int FSTEP  = (int)((double)FXOSC / (double)524288);

            public const byte SPI_WRITE_MASK = 0x80;

            public const byte NOT_A_PORT = 0;

            public const sbyte NOT_AN_INTERRUPT  = -1;

            public const byte INVALID_PIN = 0xff;

            public const byte BROADCAST_ADDRESS = 0xff;

            public enum LORA_REGISTER : byte
            {
                x_00_FIFO                    = 0x00,
                x_01_OP_MODE                 = 0x01,
                x_02_RESERVED                = 0x02,
                x_03_RESERVED                = 0x03,
                x_04_RESERVED                = 0x04,
                x_05_RESERVED                = 0x05,
                x_06_FRF_MSB                 = 0x06,
                x_07_FRF_MID                 = 0x07,
                x_08_FRF_LSB                 = 0x08,
                x_09_PA_CONFIG               = 0x09,
                x_0A_PA_RAMP                 = 0x0a,
                x_0B_OCP                     = 0x0b,
                x_0C_LNA                     = 0x0c,
                x_0D_FIFO_ADDR_PTR           = 0x0d,
                x_0E_FIFO_TX_BASE_ADDR       = 0x0e,
                x_0F_FIFO_RX_BASE_ADDR       = 0x0f,
                x_10_FIFO_RX_CURRENT_ADDR    = 0x10,
                x_11_IRQ_FLAGS_MASK          = 0x11,
                x_12_IRQ_FLAGS               = 0x12,
                x_13_RX_NB_BYTES             = 0x13,
                x_14_RX_HEADER_CNT_VALUE_MSB = 0x14,
                x_15_RX_HEADER_CNT_VALUE_LSB = 0x15,
                x_16_RX_PACKET_CNT_VALUE_MSB = 0x16,
                x_17_RX_PACKET_CNT_VALUE_LSB = 0x17,
                x_18_MODEM_STAT              = 0x18,
                x_19_PKT_SNR_VALUE           = 0x19,
                x_1A_PKT_RSSI_VALUE          = 0x1a,
                x_1B_RSSI_VALUE              = 0x1b,
                x_1C_HOP_CHANNEL             = 0x1c,
                x_1D_MODEM_CONFIG1           = 0x1d,
                x_1E_MODEM_CONFIG2           = 0x1e,
                x_1F_SYMB_TIMEOUT_LSB        = 0x1f,
                x_20_PREAMBLE_MSB            = 0x20,
                x_21_PREAMBLE_LSB            = 0x21,
                x_22_PAYLOAD_LENGTH          = 0x22,
                x_23_MAX_PAYLOAD_LENGTH      = 0x23,
                x_24_HOP_PERIOD              = 0x24,
                x_25_FIFO_RX_BYTE_ADDR       = 0x25,
                x_26_MODEM_CONFIG3           = 0x26,

                x_27_PPM_CORRECTION      = 0x27,
                x_28_FEI_MSB             = 0x28,
                x_29_FEI_MID             = 0x29,
                x_2A_FEI_LSB             = 0x2a,
                x_2C_RSSI_WIDEBAND       = 0x2c,
                x_31_DETECT_OPTIMIZE     = 0x31,
                x_33_INVERT_IQ           = 0x33,
                x_37_DETECTION_THRESHOLD = 0x37,
                x_39_SYNC_WORD           = 0x39,

                x_40_DIO_MAPPING1 = 0x40,
                x_41_DIO_MAPPING2 = 0x41,
                x_42_VERSION      = 0x42,

                x_4B_TCXO        = 0x4b,
                x_4D_PA_DAC      = 0x4d,
                x_5B_FORMER_TEMP = 0x5b,
                x_61_AGC_REF     = 0x61,
                x_62_AGC_THRESH1 = 0x62,
                x_63_AGC_THRESH2 = 0x63,
                x_64_AGC_THRESH3 = 0x64
            }

            public enum  REG_01_OP_MODE : byte
            {
                LONG_RANGE_MODE    = 0x80,
                ACCESS_SHARED_REG  = 0x40,
                LOW_FREQUENCY_MODE = 0x08,
                MODE               = 0x07,
                MODE_SLEEP         = 0x00,
                MODE_STDBY         = 0x01,
                MODE_FSTX          = 0x02,
                MODE_TX            = 0x03,
                MODE_FSRX          = 0x04,
                MODE_RXCONTINUOUS  = 0x05,
                MODE_RXSINGLE      = 0x06,
                MODE_CAD           = 0x07
            }

            public enum  REG_09_PA_CONFIG : byte
            {
                PA_SELECT    = 0x80,
                MAX_POWER    = 0x70,
                OUTPUT_POWER = 0x0f
            }

            public enum REG_0A_PA_RAMP : byte
            {
                LOW_PN_TX_PLL_OFF = 0x10,
                PA_RAMP           = 0x0f,
                PA_RAMP_3_4MS     = 0x00,
                PA_RAMP_2MS       = 0x01,
                PA_RAMP_1MS       = 0x02,
                PA_RAMP_500US     = 0x03,
                PA_RAMP_250US     = 0x04,
                PA_RAMP_125US     = 0x05,
                PA_RAMP_100US     = 0x06,
                PA_RAMP_62US      = 0x07,
                PA_RAMP_50US      = 0x08,
                PA_RAMP_40US      = 0x09,
                PA_RAMP_31US      = 0x0a,
                PA_RAMP_25US      = 0x0b,
                PA_RAMP_20US      = 0x0c,
                PA_RAMP_15US      = 0x0d,
                PA_RAMP_12US      = 0x0e,
                PA_RAMP_10US      = 0x0f
            }

            public enum REG_0B_OCP : byte
            {
                OCP_ON   = 0x20,
                OCP_TRIM = 0x1f
            }

            public enum REG_0C_LNA : byte
            {
                LNA_GAIN             = 0xe0,
                LNA_GAIN_G1          = 0x20,
                LNA_GAIN_G2          = 0x40,
                LNA_GAIN_G3          = 0x60,                
                LNA_GAIN_G4          = 0x80,
                LNA_GAIN_G5          = 0xa0,
                LNA_GAIN_G6          = 0xc0,
                LNA_BOOST_LF         = 0x18,
                LNA_BOOST_LF_DEFAULT = 0x00,
                LNA_BOOST_HF         = 0x03,
                LNA_BOOST_HF_DEFAULT = 0x00,
                LNA_BOOST_HF_150PC   = 0x03
            }

            public enum REG_11_IRQ_FLAGS_MASK  : byte //0x11
            {
                RX_TIMEOUT_MASK          = 0x80,
                RX_DONE_MASK             = 0x40,
                PAYLOAD_CRC_ERROR_MASK   = 0x20,
                VALID_HEADER_MASK        = 0x10,
                TX_DONE_MASK             = 0x08,
                CAD_DONE_MASK            = 0x04,
                FHSS_CHANGE_CHANNEL_MASK = 0x02,
                CAD_DETECTED_MASK        = 0x01
            }

            public enum REG_12_IRQ_FLAGS : byte
            {
                RX_TIMEOUT          = 0x80,
                RX_DONE             = 0x40,
                PAYLOAD_CRC_ERROR   = 0x20,
                VALID_HEADER        = 0x10,
                TX_DONE             = 0x08,
                CAD_DONE            = 0x04,
                FHSS_CHANGE_CHANNEL = 0x02,
                CAD_DETECTED        = 0x01
            }

            public enum REG_18_MODEM_STAT : byte
            {
                RX_CODING_RATE                   = 0xe0,
                MODEM_STATUS_CLEAR               = 0x10,
                MODEM_STATUS_HEADER_INFO_VALID   = 0x08,
                MODEM_STATUS_RX_ONGOING          = 0x04,
                MODEM_STATUS_SIGNAL_SYNCHRONIZED = 0x02,
                MODEM_STATUS_SIGNAL_DETECTED     = 0x01
            }

            public enum REG_1C_HOP_CHANNEL : byte
            {
                PLL_TIMEOUT          = 0x80,
                RX_PAYLOAD_CRC_IS_ON = 0x40,
                FHSS_PRESENT_CHANNEL = 0x3f
            }

            public enum REG_1D_MODEM_CONFIG1 : byte
            {
                BW = 0xf0,

                BW_7_8KHZ               = 0x00,
                BW_10_4KHZ              = 0x10,
                BW_15_6KHZ              = 0x20,
                BW_20_8KHZ              = 0x30,
                BW_31_25KHZ             = 0x40,
                BW_41_7KHZ              = 0x50,
                BW_62_5KHZ              = 0x60,
                BW_125KHZ               = 0x70,
                BW_250KHZ               = 0x80,
                BW_500KHZ               = 0x90,
                CODING_RATE             = 0x0e,
                CODING_RATE_4_5         = 0x02,
                CODING_RATE_4_6         = 0x04,
                CODING_RATE_4_7         = 0x06,
                CODING_RATE_4_8         = 0x08,
                IMPLICIT_HEADER_MODE_ON = 0x01
            }
            public enum REG_1E_MODEM_CONFIG2 : byte
            {
                SPREADING_FACTOR         = 0xf0,
                SPREADING_FACTOR_64CPS   = 0x60,
                SPREADING_FACTOR_128CPS  = 0x70,
                SPREADING_FACTOR_256CPS  = 0x80,
                SPREADING_FACTOR_512CPS  = 0x90,
                SPREADING_FACTOR_1024CPS = 0xa0,
                SPREADING_FACTOR_2048CPS = 0xb0,
                SPREADING_FACTOR_4096CPS = 0xc0,
                TX_CONTINUOUS_MODE       = 0x08,
        
                PAYLOAD_CRC_ON  = 0x04,
                SYM_TIMEOUT_MSB = 0x03
            }

            public enum REG_26_MODEM_CONFIG3 : byte
            {
                MOBILE_NODE            = 0x08, // HopeRF term
                LOW_DATA_RATE_OPTIMIZE = 0x08, // Semtechs term
                AGC_AUTO_ON            = 0x04
            }

            public enum REG_4B_TCXO : byte
            {
                TCXO_TCXO_INPUT_ON = 0x10
            }

            public enum REG_4D_PA_DAC : byte
            {
                PA_DAC_DISABLE = 0x04,
                PA_DAC_ENABLE  = 0x07
            }

            /// \brief Defines register values for a set of modem configuration registers
            ///
            /// Defines register values for a set of modem configuration registers
            /// that can be passed to setModemRegisters() if none of the choices in
            /// ModemConfigChoice suit your need setModemRegisters() writes the
            /// register values from this structure to the appropriate registers
            /// to set the desired spreading factor, coding rate and bandwidth
            public enum ModemConfig : byte{
            reg_1d, ///< Value for register LORA_REGISTER.x_1D_MODEM_CONFIG1
            reg_1e, ///< Value for register LORA_REGISTER.x_1E_MODEM_CONFIG2
            reg_26  ///< Value for register LORA_REGISTER.x_26_MODEM_CONFIG3
            };
        
            /// Choices for setModemConfig() for a selected subset of common
            /// data rates. If you need another configuration,
            /// determine the necessary settings and call setModemRegisters() with your
            /// desired settings. It might be helpful to use the LoRa calculator mentioned in 
            /// http://www.semtech.com/images/datasheet/LoraDesignGuide_STD.pdf
            /// These are indexes into MODEM_CONFIG_TABLE. We strongly recommend you use these symbolic
            /// definitions and not their integer equivalents: its possible that new values will be
            /// introduced in later versions (though we will try to avoid it).
            /// Caution: if you are using slow packet rates and long packets with RHReliableDatagram or subclasses
            /// you may need to change the RHReliableDatagram timeout for reliable operations.
            /// Caution: for some slow rates nad with ReliableDatagrams you may need to increase the reply timeout 
            /// with manager.setTimeout() to
            /// deal with the long transmission times.
            /// Caution: SX1276 family errata suggests alternate settings for some LoRa registers when 500kHz bandwidth
            /// is in use. See the Semtech SX1276/77/78 Errata Note. These are not implemented by RF95.
            public enum ModemConfigChoice
            {
            Bw125Cr45Sf128 = 0,	   ///< Bw = 125 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on. Default medium range
            Bw500Cr45Sf128,	           ///< Bw = 500 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on. Fast+short range
            Bw31_25Cr48Sf512,	   ///< Bw = 31.25 kHz, Cr = 4/8, Sf = 512chips/symbol, CRC on. Slow+long range
            Bw125Cr48Sf4096,           ///< Bw = 125 kHz, Cr = 4/8, Sf = 4096chips/symbol, low data rate, CRC on. Slow+long range
            Bw125Cr45Sf2048           ///< Bw = 125 kHz, Cr = 4/5, Sf = 2048chips/symbol, CRC on. Slow+long range
            };

            public byte[][] MODEM_CONFIG_TABLE =
            {
                        //  1d,     1e,      26
                new byte[]{ 0x72,   0x74,    0x04}, // Bw125Cr45Sf128 (the chip default), AGC enabled
                new byte[]{ 0x92,   0x74,    0x04}, // Bw500Cr45Sf128, AGC enabled
                new byte[]{ 0x48,   0x94,    0x04}, // Bw31_25Cr48Sf512, AGC enabled
                new byte[]{ 0x78,   0xc4,    0x0c}, // Bw125Cr48Sf4096, AGC enabled
                new byte[]{ 0x72,   0xb4,    0x04}, // Bw125Cr45Sf2048, AGC enabled
            };

            public enum RadioMode
            {
                Initializing = 0, ///< Transport is initializing. Initial default value until init() is called..
                Sleep,            ///< Transport hardware is in low power sleep mode (if supported)
                Idle,             ///< Transport is idle.
                Tx,               ///< Transport is in the process of transmitting a message.
                Rx,               ///< Transport is in the process of receiving a message.
                Cad               ///< Transport is in the process of detecting channel activity (if supported)
            };

            /// Constructor. You can have multiple instances, but each instance must have its own
            /// interrupt and slave select pin. After constructing, you must call init() to initialise the interface
            /// and the radio module. A maximum of 3 instances can co-exist on one processor, provided there are sufficient
            /// distinct interrupt lines, one for each instance.
            /// \param[in] slaveSelectPin the Arduino pin number of the output to use to select the RH_RF22 before
            /// accessing it. Defaults to the normal SS pin for your Arduino (D10 for Diecimila, Uno etc, D53 for Mega, D10 for Maple)
            /// \param[in] interruptPin The interrupt Pin number that is connected to the RFM DIO0 interrupt line. 
            /// Defaults to pin 2, as required by Anarduino MinWirelessLoRa module.
            /// Caution: You must specify an interrupt capable pin.
            /// On many Arduino boards, there are limitations as to which pins may be used as interrupts.
            /// On Leonardo pins 0, 1, 2 or 3. On Mega2560 pins 2, 3, 18, 19, 20, 21. On Due and Teensy, any digital pin.
            /// On Arduino Zero from arduino.cc, any digital pin other than 4.
            /// On Arduino M0 Pro from arduino.org, any digital pin other than 2.
            /// On other Arduinos pins 2 or 3. 
            /// See http://arduino.cc/en/Reference/attachInterrupt for more details.
            /// On Chipkit Uno32, pins 38, 2, 7, 8, 35.
            /// On other boards, any digital pin may be used.
            /// \param[in] spi Pointer to the SPI interface object to use. 
            ///                Defaults to the standard Arduino hardware SPI interface
            public RF95(SpiDevice newSpi, GpioController newGPIO) //byte interruptPin = 2)
            {
                // _interruptPin = interruptPin;
                _myInterruptIndex = 0xff; // Not allocated yet
                _enableCRC = true;
                _useRFO = false;
                _rxBufValid = false;
                        ///The spi interface object
                _spiDevice = newSpi;
                _controller = newGPIO;
            }

            public void setSlaveSelectPin(byte slaveSelectPin)
            {
                _slaveSelectPin = slaveSelectPin;
                _controller.OpenPin (_slaveSelectPin, PinMode.Output);
            }

            public void spiUsingInterrupt(byte interruptNumber)
            {
                // _spi.usingInterrupt(interruptNumber);
            }

            public void selectSlave()
            {
                // digitalWrite(_slaveSelectPin, LOW);
                _controller.Write (_slaveSelectPin, PinValue.Low);
            }

            public void deselectSlave()
            {
                // digitalWrite(_slaveSelectPin, HIGH);
                _controller.Write (_slaveSelectPin, PinValue.High);
            }
            /// Signal the start of an SPI transaction that must not be interrupted by other SPI actions
            /// In subclasses that support transactions this will ensure that other SPI transactions
            /// are blocked until this one is completed by endTransaction().
            /// Base does nothing
            /// Might be overridden in subclass
            public void spiBeginTransaction(){}

            /// Signal the end of an SPI transaction
            /// Base does nothing
            /// Might be overridden in subclass
            public void spiEndTransaction(){}

            byte[] spiTransfer(byte[] val, int len, bool direction) // New value follows
            {
                Console.WriteLine("     transfer");
                byte[] recieveData = new byte[val.Length];
                // _spiDevice.TransferFullDuplex(val, recieveData);
                for(int i = 0; i<val.Length; i++)
                {
                    if(direction)
                    {
                        _spiDevice.Write(val);
                    }
                    else
                    {
                        _spiDevice.Read(val);
                    }
                // Console.WriteLine("          val[]");
                // Console.WriteLine(val[i]);
                // _spiDevice.WriteByte(val[i]);
                // recieveData[i] = _spiDevice.ReadByte();
                // Console.WriteLine("          recieveData[]");
                // Console.WriteLine(recieveData[i]);
                }
                return recieveData;
            }

            byte spiRead(LORA_REGISTER reg)
            {
                Console.WriteLine("spireaddata");
                byte val = 0;
                byte[] data = new byte[1];
                data[0] = (byte)((byte)reg & ~SPI_WRITE_MASK);
                Console.WriteLine(data[0]);
                // ATOMIC_BLOCK_START;
                spiBeginTransaction();
                selectSlave();
                spiTransfer(data, 1, true); // Send the address with the write mask off
                val = spiTransfer(data, 1, false)[0];                  // The written value is ignored, reg value is read
                deselectSlave();
                spiEndTransaction();
                // ATOMIC_BLOCK_END;
                return val;
            }

            byte spiWrite(LORA_REGISTER reg, byte val)
            {
                Console.WriteLine("spiwrite");
                byte status = 0;
                byte[] data = new byte[1];
                data[0] = (byte)((byte)reg | SPI_WRITE_MASK);
                Console.WriteLine(data[0]);
                // ATOMIC_BLOCK_START;
                // spiBeginTransaction();

                selectSlave();
                status = spiTransfer(data, 1, true)[0]; // Send the address with the write mask on
                data[0] = (byte)val;
                spiTransfer(data, 1, true);                              // New value follows
                
                // Based on https://forum.pjrc.com/attachment.php?attachmentid=10948&d=1499109224
                // Need this delay from some processors when running fast:
                // delayMicroseconds(1);
                deselectSlave();
                // spiEndTransaction();
                // ATOMIC_BLOCK_END;
                return status;
            }

            byte[] spiBurstRead(byte reg, byte len)
            {
                Console.WriteLine("spiburstread");
                byte[] data = new byte[1];
                data[0] = (byte)(reg & ~SPI_WRITE_MASK);
                Console.WriteLine(data[0]);
                // ATOMIC_BLOCK_START;
                spiBeginTransaction();
                selectSlave();

                spiTransfer(data, len, true); // Send the start address with the write mask off
                data = new byte[len]; //Reset data
                data = spiTransfer(data, len, false); //Doesn matter what data

                deselectSlave();
                spiEndTransaction();
                // ATOMIC_BLOCK_END;
                return data;
            }

            void spiBurstWrite(byte reg, byte[] src, byte len)
            {
                Console.WriteLine("spiburstwrite");
                byte[] data = new byte[1];
                data[0] = (byte)(reg | SPI_WRITE_MASK);
                Console.WriteLine(data[0]);
                // ATOMIC_BLOCK_START;
                spiBeginTransaction();
                selectSlave();

                spiTransfer(data, 1, true); // Send the start address with the write mask on
                spiTransfer(src, len, true);

                deselectSlave();
                spiEndTransaction();
                // ATOMIC_BLOCK_END;
                return;
            }
            
            /// Initialise the Driver transport hardware and software.
            /// Leaves the radio in idle mode,
            /// with default configuration of: 434.0MHz, 13dBm, Bw = 125 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on
            /// \return true if initialisation succeeded.
            public bool init()
            {
                // #ifdef RH_USE_MUTEX
                //     if (RH_MUTEX_INIT(lock) != 0)
                //     { 
                //     	Serial.println("\n mutex init has failed\n");
                //     	return false;
                //     }
                // #endif

                // // For some subclasses (eg RH_ABZ)  we dont want to set up interrupt
                // spiUsingInterrupt(_interruptPin);
            
                setSlaveSelectPin(24);

                // No way to check the device type :-(
                
                // Set sleep mode, so we can also set LORA mode:
                spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)(REG_01_OP_MODE.MODE_SLEEP | REG_01_OP_MODE.LONG_RANGE_MODE));
                // delay(10); // Wait for sleep mode to take over from say, CAD
                // Check we are in sleep mode, with LORA set
                if (spiRead(LORA_REGISTER.x_01_OP_MODE) != (byte)(REG_01_OP_MODE.MODE_SLEEP | REG_01_OP_MODE.LONG_RANGE_MODE))
                {
                    // Serial.println(spiRead(LORA_REGISTER.x_01_OP_MODE), HEX);
                    return false; // No device present?
                }
            
                // Add by Adrien van den Bossche <vandenbo@univ-tlse2.fr> for Teensy
                // ARM M4 requires the below. else pin interrupt doesn't work properly.
                // On all other platforms, its innocuous, belt and braces
                // pinMode(_interruptPin, INPUT); 
                
            
                // Set up interrupt handler
                // Since there are a limited number of interrupt glue functions isr*() available,
                // we can only support a limited number of devices simultaneously
                // ON some devices, notably most Arduinos, the interrupt pin passed in is actually the 
                // interrupt number. You have to figure out the interruptnumber-to-interruptpin mapping
                // yourself based on knwledge of what Arduino board you are running on.
                if (_myInterruptIndex == 0xff)
                {
                    // First run, no interrupt allocated yet
                    if (_interruptCount <= NUM_INTERRUPTS)
                    _myInterruptIndex = _interruptCount++;
                    else
                    return false; // Too many devices, not enough interrupt vectors
                }
                _deviceForInterrupt[_myInterruptIndex] = this;
            
                // if (_myInterruptIndex == 0)
                //     attachInterrupt(interruptNumber, isr0, RISING);
                // else if (_myInterruptIndex == 1)
                //     attachInterrupt(interruptNumber, isr1, RISING);
                // else if (_myInterruptIndex == 2)
                //     attachInterrupt(interruptNumber, isr2, RISING);
                // else
                //     return false; // Too many devices, not enough interrupt vectors
                
                // Set up FIFO
                // We configure so that we can use the entire 256 byte FIFO for either receive
                // or transmit, but not both at the same time
                spiWrite(LORA_REGISTER.x_0E_FIFO_TX_BASE_ADDR, 0);
                spiWrite(LORA_REGISTER.x_0F_FIFO_RX_BASE_ADDR, 0);
            
                // Packet format is preamble + explicit-header + payload + crc
                // Explicit Header Mode
                // payload is TO + FROM + ID + FLAGS + message data
                // RX mode is implmented with RXCONTINUOUS
                // max message data length is 255 - 4 = 251 octets
            
                setMode(RadioMode.Idle);
            
                // Set up default configuration
                // No Sync Words in LORA mode.
                setModemConfig(ModemConfigChoice.Bw125Cr45Sf128); // Radio default
            //    setModemConfig(Bw125Cr48Sf4096); // slow and reliable?
                setPreambleLength(8); // Default is 8
                // An innocuous ISM frequency, same as RF22's
                setFrequency(434.0);
                // Lowish power
                setTxPower(13);
            
                return true;
            }
            /// Prints the value of all chip registers
            /// to the Serial device if RH_HAVE_SERIAL is defined for the current platform
            /// For debugging purposes only.
            /// \return true on success
            public bool printRegisters()
            {
                byte[] registers = { 0x01, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x014, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x4b};

                byte i;
                for (i = 0; i < registers.Length; i++)
                {
                // Serial.print(registers[i], HEX);
                // Serial.print(": ");
                // Serial.println(spiRead(registers[i]), HEX);
                Console.WriteLine(registers[i]);
                }
                return true;
            }

            /// Select one of the predefined modem configurations. If you need a modem configuration not provided 
            /// here, use setModemRegisters() with your own ModemConfig.
            /// Caution: the slowest protocols may require a radio module with TCXO temperature controlled oscillator
            /// for reliable operation.
            /// \param[in] index The configuration choice.
            /// \return true if index is a valid choice.
            public bool setModemConfig(ModemConfigChoice index)
            {
                if ((int)index > (int)((float)MODEM_CONFIG_TABLE.Length / (float)sizeof(ModemConfig)))
                {
                    return false;
                }

                // ModemConfig cfg;
                // memcpy_P(&cfg, &MODEM_CONFIG_TABLE[index], sizeof(RF95::ModemConfig));
                // setModemRegisters(&cfg);
                /// Sets all the registers required to configure the data modem in the radio, including the bandwidth, 
                /// spreading factor etc. You can use this to configure the modem with custom configurations if none of the 
                /// canned configurations in ModemConfigChoice suit you.
                /// \param[in] config A ModemConfig structure containing values for the modem configuration registers.
                // public void setModemRegisters(ModemConfig config)
                // {
                spiWrite(LORA_REGISTER.x_1D_MODEM_CONFIG1, MODEM_CONFIG_TABLE[(int)index][0]);
                spiWrite(LORA_REGISTER.x_1E_MODEM_CONFIG2, MODEM_CONFIG_TABLE[(int)index][1]);
                spiWrite(LORA_REGISTER.x_26_MODEM_CONFIG3, MODEM_CONFIG_TABLE[(int)index][2]);
                // }
                return true;
            }

            /// Tests whether a new message is available from the Driver. 
            /// On most drivers, this will also put the Driver into RadioMode.Rx mode until
            /// a message is actually received by the transport, when it will be returned to RadioMode.Idle.
            /// This can be called multiple times in a timeout loop
            /// \return true if a new, complete, error-free uncollected message is available to be retreived by recv()
            public bool available()
            {
                // RH_MUTEX_LOCK(lock); // Multithreading support
                if (_mode == RadioMode.Tx)
                {
                    // RH_MUTEX_UNLOCK(lock);
                return false;
                }
                setMode(RadioMode.Rx);
                // RH_MUTEX_UNLOCK(lock);
                return _rxBufValid; // Will be set by the interrupt handler when a good message is received
            }

            /// Turns the receiver on if it not already on.
            /// If there is a valid message available, copy it to buf and return true
            /// else return false.
            /// If a message is copied, *len is set to the length (Caution, 0 length messages are permitted).
            /// You should be sure to call this function frequently enough to not miss any messages
            /// It is recommended that you call it in your main loop.
            /// \param[in] buf Location to copy the received message
            /// \param[in,out] len Pointer to the number of octets available in buf. The number be reset to the actual number of octets copied.
            /// \return true if a valid message was copied to buf
            // public bool recv(byte[] buf, byte len)
            // {
            //     if (!available())
            //     {
            //         return false;
            //     }
            //     // RH_MUTEX_LOCK(lock); // Multithread support
            //     if (buf[0] && len)
            //     {
            // 	    // ATOMIC_BLOCK_START;
            // 	    // Skip the 4 headers that are at the beginning of the rxBuf
            // 	    if (len > _bufLen-HEADER_LEN)
            //         {
            // 	        len = _bufLen-HEADER_LEN;
            // 	    }
            //         buf = _buf;
            //         // memcpy(buf, _buf+HEADER_LEN, len);
            // 	    // ATOMIC_BLOCK_END;
            //     }
            //     clearRxBuf(); // This message accepted and cleared
            //     // RH_MUTEX_UNLOCK(lock);
            //     return true;
            // }


            // bool waitCAD()
            // {
            //     if (_cad_timeout == 0)
            //         return true;
            
            //     // Wait for any channel activity to finish or timeout
            //     // Sophisticated DCF function...
            //     // DCF : BackoffTime = random() x aSlotTime
            //     // 100 - 1000 ms
            //     // 10 sec timeout
            //     Thread.Sleep(100);        
            //     return true;
            // }

            /// Waits until any previous transmit packet is finished being transmitted with waitPacketSent().
            /// Then optionally waits for Channel Activity Detection (CAD) 
            /// to show the channnel is clear (if the radio supports CAD) by calling waitCAD().
            /// Then loads a message into the transmitter and starts the transmitter. Note that a message length
            /// of 0 is permitted. 
            /// \param[in] data Array of data to be sent
            /// \param[in] len Number of bytes of data to send
            /// specify the maximum time in ms to wait. If 0 (the default) do not wait for CAD before transmitting.
            /// \return true if the message length was valid and it was correctly queued for transmit. Return false
            /// if CAD was requested and the CAD timeout timed out before clear channel was detected.
            public bool send(byte[] data, byte len)
            {
                if (len > MAX_MESSAGE_LEN)
                return false;
                
                setMode(RadioMode.Idle);

                // if (!waitCAD()) 
                // return false;  // Check channel activity

                // Position at the beginning of the FIFO
                spiWrite(LORA_REGISTER.x_0D_FIFO_ADDR_PTR, 0);

                // // The headers
                spiWrite(LORA_REGISTER.x_00_FIFO, _txHeaderTo);
                spiWrite(LORA_REGISTER.x_00_FIFO, _txHeaderFrom);
                spiWrite(LORA_REGISTER.x_00_FIFO, _txHeaderId);
                spiWrite(LORA_REGISTER.x_00_FIFO, _txHeaderFlags);

                // The message data
                spiBurstWrite((byte)LORA_REGISTER.x_00_FIFO, data, len);
                spiWrite(LORA_REGISTER.x_22_PAYLOAD_LENGTH, (byte)(len + HEADER_LEN));

                // RH_MUTEX_LOCK(lock); // Multithreading support
                setMode(RadioMode.Tx); // Start the transmitter
                // RH_MUTEX_UNLOCK(lock);

                // when Tx is done, interruptHandler will fire and radio mode will return to STANDBY
                return true;
            }

            /// Sets the length of the preamble
            /// in bytes. 
            /// Caution: this should be set to the same 
            /// value on all nodes in your network. Default is 8.
            /// Sets the message preamble length in LORA_REGISTER.x_??_PREAMBLE_?SB
            /// \param[in] bytes Preamble length in bytes.  
            public void setPreambleLength(short preAmbleBytes)
            {
                byte[] bytes = BitConverter.GetBytes(preAmbleBytes);
                spiWrite(LORA_REGISTER.x_20_PREAMBLE_MSB, bytes[1]);
                spiWrite(LORA_REGISTER.x_21_PREAMBLE_LSB, bytes[0]);
            }
            /// Returns the maximum message length 
            /// available in this Driver.
            /// \return The maximum legal message length
            public byte maxMessageLength()
            {
                return MAX_MESSAGE_LEN;
            }

            /// Sets the transmitter and receiver 
            /// centre frequency.
            /// \param[in] centre Frequency in MHz. 137.0 to 1020.0. Caution: RFM95/96/97/98 comes in several
            /// different frequency ranges, and setting a frequency outside that range of your radio will probably not work
            /// \return true if the selected frquency centre is within range
            public bool setFrequency(double centre)
            {
                // Frf = FRF / FSTEP
                uint frf = (uint)((centre * 1000000.0) / (float)FSTEP);
                byte[] bytes = BitConverter.GetBytes(frf);
                spiWrite(LORA_REGISTER.x_06_FRF_MSB, bytes[2]);
                spiWrite(LORA_REGISTER.x_07_FRF_MID, bytes[1]);
                spiWrite(LORA_REGISTER.x_08_FRF_LSB, bytes[0]);
                _usingHFport = (centre >= 779.0);

                return true;
            }


            /// If current mode is Rx or Tx changes it to Idle. If the transmitter or receiver is running, 
            /// disables them.
            /// If current mode is Tx or Idle, changes it to Rx. 
            /// Starts the receiver in the RF95/96/97/98.
            /// If current mode is Rx or Idle, changes it to Rx. F
            /// Starts the transmitter in the RF95/96/97/98.
            public void setMode(RadioMode newMode)
            {
                if (newMode != _mode)
                {
                    switch(newMode)
                    {
                        case RadioMode.Idle:
                            modeWillChange(RadioMode.Idle);
                            spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)REG_01_OP_MODE.MODE_STDBY);
                            _mode = RadioMode.Idle;
                            break;
                        case RadioMode.Rx:
                            modeWillChange(RadioMode.Rx);
                            spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)REG_01_OP_MODE.MODE_RXCONTINUOUS);
                            spiWrite(LORA_REGISTER.x_40_DIO_MAPPING1, 0x00); // Interrupt on RxDone
                            _mode = RadioMode.Rx;
                            break;
                        case RadioMode.Tx:
                            modeWillChange(RadioMode.Tx);
                            spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)REG_01_OP_MODE.MODE_TX);
                            spiWrite(LORA_REGISTER.x_40_DIO_MAPPING1, 0x40); // Interrupt on TxDone
                            _mode = RadioMode.Tx;
                            break;
                    }
                }
            }

            /// Sets the transmitter power output level, and configures the transmitter pin.
            /// Be a good neighbour and set the lowest power level you need.
            /// Some SX1276/77/78/79 and compatible modules (such as RFM95/96/97/98) 
            /// use the PA_BOOST transmitter pin for high power output (and optionally the PA_DAC)
            /// while some (such as the Modtronix inAir4 and inAir9) 
            /// use the RFO transmitter pin for lower power but higher efficiency.
            /// You must set the appropriate power level and useRFO argument for your module.
            /// Check with your module manufacturer which transmtter pin is used on your module
            /// to ensure you are setting useRFO correctly. 
            /// Failure to do so will result in very low 
            /// transmitter power output.
            /// Caution: legal power limits may apply in certain countries.
            /// After init(), the power will be set to 13dBm, with useRFO false (ie PA_BOOST enabled).
            /// \param[in] power Transmitter power level in dBm. For RFM95/96/97/98 LORA with useRFO false, 
            /// valid values are from +2 to +20. For 18, 19 and 20, PA_DAC is enabled, 
            /// For Modtronix inAir4 and inAir9 with useRFO true (ie RFO pins in use), 
            /// valid values are from 0 to 15.
            /// \param[in] useRFO If true, enables the use of the RFO transmitter pins instead of
            /// the PA_BOOST pin (false). Choose the correct setting for your module.
            public void setTxPower(sbyte power, bool useRFO = false)
            {
                _useRFO = useRFO;

                // Sigh, different behaviours depending on whether the module use PA_BOOST or the RFO pin
                // for the transmitter output
                if (useRFO)
                {
                    if (power > 15)
                    {   
                        power = 15;
                    }
                    if (power < 0)
                    {
                        power = 0;
                    }
                    // Set the MaxPower register to 0x7 => MaxPower = 10.8 + 0.6 * 7 = 15dBm
                    // So Pout = Pmax - (15 - power) = 15 - 15 + power
                    spiWrite(LORA_REGISTER.x_09_PA_CONFIG, (byte)((uint)REG_09_PA_CONFIG.MAX_POWER | power));
                    spiWrite(LORA_REGISTER.x_4D_PA_DAC,    (byte)REG_4D_PA_DAC.PA_DAC_DISABLE);
                }
                else
                {
                    if (power > 20)
                    {
                        power = 20;
                    }
                    if (power < 2)
                    {
                        power = 2;
                    }
                    // For REG_4D_PA_DAC.PA_DAC_ENABLE, manual says '+20dBm on PA_BOOST when OutputPower=0xf'
                    // REG_4D_PA_DAC.PA_DAC_ENABLE actually adds about 3dBm to all power levels. We will use it
                    // for 8, 19 and 20dBm
                    if (power > 17)
                    {
                        spiWrite(LORA_REGISTER.x_4D_PA_DAC, (byte)REG_4D_PA_DAC.PA_DAC_ENABLE);
                        power -= 3;
                    }
                    else
                    {
                        spiWrite(LORA_REGISTER.x_4D_PA_DAC, (byte)REG_4D_PA_DAC.PA_DAC_DISABLE);
                    }

                    // RFM95/96/97/98 does not have RFO pins connected to anything. Only PA_BOOST
                    // pin is connected, so must use PA_BOOST
                    // Pout = 2 + OutputPower (+3dBm if DAC enabled)
                    spiWrite(LORA_REGISTER.x_09_PA_CONFIG, (byte)((uint)REG_09_PA_CONFIG.PA_SELECT | (power-2)));
                }
            }

            /// Sets the radio into low-power sleep mode.
            /// If successful, the transport will stay in sleep mode until woken by 
            /// changing mode it idle, transmit or receive (eg by calling send(), recv(), available() etc)
            /// Caution: there is a time penalty as the radio takes a finite time to wake from sleep mode.
            /// \return true if sleep mode was successfully entered.
            public bool sleep()
            {
                if (_mode != RadioMode.Sleep)
                {
                    modeWillChange(RadioMode.Sleep);
                    spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)REG_01_OP_MODE.MODE_SLEEP);
                    _mode = RadioMode.Sleep;
                }
                return true;
            }

            // // Bent G Christensen (bentor@gmail.com), 08/15/2016
            // /// Use the radio's Channel Activity Detect (CAD) function to detect channel activity.
            // /// Sets the RF95 radio into CAD mode and waits until CAD detection is complete.
            // /// To be used in a listen-before-talk mechanism (Collision Avoidance)
            // /// with a reasonable time backoff algorithm.
            // /// This is called automatically by waitCAD().
            // /// \return true if channel is in use.  
            // public bool isChannelActive()
            // {
            //     // Set mode RadioMode.Cad
            //     if (_mode != RadioMode.Cad)
            //     {
            // 	    modeWillChange(RadioMode.Cad);
            //         spiWrite(LORA_REGISTER.x_01_OP_MODE, (byte)REG_01_OP_MODE.MODE_CAD);
            //         spiWrite(LORA_REGISTER.x_40_DIO_MAPPING1, 0x80); // Interrupt on CadDone
            //         _mode = RadioMode.Cad;
            //     }

            //     while (_mode == RadioMode.Cad)
            //     {
            //         // YIELD;
            //     }
                
            //     return _cad;
            // }

            /// Enable TCXO mode
            /// Call this immediately after init(), to force your radio to use an external
            /// frequency source, such as a Temperature Compensated Crystal Oscillator (TCXO), if available.
            /// See the comments in the main documentation about the sensitivity of this radio to
            /// clock frequency especially when using narrow bandwidths.
            /// Leaves the module in sleep mode.
            /// Caution: the TCXO model radios are not low power when in sleep (consuming
            /// about ~600 uA, reported by Phang Moh Lim.<br>
            /// Caution: if you enable TCXO and there is no exernal TCXO signal connected to the radio
            /// or if the exerrnal TCXO is not
            /// powered up, the radio <b>will not work<\b>
            /// \param[in] on If true (the default) enables the radio to use the external TCXO.
            public void enableTCXO(bool on = true)
            {
            if (on)
            {
                while ((spiRead(LORA_REGISTER.x_4B_TCXO) & (byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON) != (byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON)
                {
                    sleep();
                    spiWrite(LORA_REGISTER.x_4B_TCXO, (byte)(spiRead(LORA_REGISTER.x_4B_TCXO) | (byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON));
                }
            }
            else
            {
                while ((spiRead(LORA_REGISTER.x_4B_TCXO) == (byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON))
                {
                    sleep();
                    spiWrite(LORA_REGISTER.x_4B_TCXO, (byte)(spiRead(LORA_REGISTER.x_4B_TCXO) & ~(byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON));
                }
            }
            }


            /// Returns the last measured frequency error.
            /// The LoRa receiver estimates the frequency offset between the receiver centre frequency
            /// and that of the received LoRa signal. This function returns the estimates offset (in Hz) 
            /// of the last received message. Caution: this measurement is not absolute, but is measured 
            /// relative to the local receiver's oscillator. 
            /// Apparent errors may be due to the transmitter, the receiver or both.
            /// \return The estimated centre frequency offset in Hz of the last received message. 
            /// If the modem bandwidth selector in 
            /// register LORA_REGISTER.x_1D_MODEM_CONFIG1 is invalid, returns 0.
            public int frequencyError()
            {
                int freqerror = 0;
            
                // Convert 2.5 bytes (5 nibbles, 20 bits) to 32 bit signed int
                // Caution: some C compilers make errors with eg:
                // freqerror = spiRead(LORA_REGISTER.x_28_FEI_MSB) << 16
                // so we go more carefully.
                freqerror = spiRead(LORA_REGISTER.x_28_FEI_MSB);
                freqerror <<= 8;
                freqerror |= spiRead(LORA_REGISTER.x_29_FEI_MID);
                freqerror <<= 8;
                freqerror |= spiRead(LORA_REGISTER.x_2A_FEI_LSB);
                // Sign extension into top 3 nibbles
                if (freqerror == 0x80000)
                freqerror |= unchecked((int)0xfff00000);
            
                float error = 0; // In hertz
                float[] bw_tab = {7.8f, 10.4f, 15.6f, 20.8f, 31.25f, 41.7f, 62.5f, 125, 250, 500};
                byte bwindex = (byte)(spiRead(LORA_REGISTER.x_1D_MODEM_CONFIG1) >> 4);
                if (bwindex < (bw_tab.Length / sizeof(float)))
                error = freqerror * bw_tab[bwindex] * (float)((1L << 24) / FXOSC / 500.0);
                // else not defined
            
                return (int)error;
            }

            // /// Returns the Signal-to-noise ratio (SNR) of the last received message, as measured
            // /// by the receiver.
            // /// \return SNR of the last received message in dB
            // public int lastSNR()
            // {
            //     return _lastSNR;
            // }

            /// brian.n.norman@gmail.com 9th Nov 2018
            /// Sets the radio spreading factor.
            /// valid values are 6 through 12.
            /// Out of range values below 6 are clamped to 6
            /// Out of range values above 12 are clamped to 12
            /// See Semtech DS SX1276/77/78/79 page 27 regarding SF6 configuration.
            ///
            /// \param[in] byte sf (spreading factor 6..12)
            /// \return nothing
            public void setSpreadingFactor(byte sf)
            {
                if (sf <= 6)
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_64CPS;} 
                else if (sf == 7) 
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_128CPS;}
                else if (sf == 8) 
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_256CPS;}
                else if (sf == 9)
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_512CPS;}
                else if (sf == 10)
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_1024CPS;}
                else if (sf == 11) 
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_2048CPS;}
                else if (sf >= 12)
                    {sf = (byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR_4096CPS;}

            // set the new spreading factor
                // (byte)(spiRead(LORA_REGISTER.x_4B_TCXO) & ~(byte)REG_4B_TCXO.TCXO_TCXO_INPUT_ON);
            spiWrite(LORA_REGISTER.x_1E_MODEM_CONFIG2, (byte)((spiRead(LORA_REGISTER.x_1E_MODEM_CONFIG2) & ~(byte)REG_1E_MODEM_CONFIG2.SPREADING_FACTOR) | sf));
            // check if Low data Rate bit should be set or cleared
            setLowDatarate();
            }
        
            /// brian.n.norman@gmail.com 9th Nov 2018
            /// Sets the radio signal bandwidth
            /// sbw ranges and resultant settings are as follows:-
            /// sbw range    actual bw (kHz)
            /// 0-7800       7.8
            /// 7801-10400   10.4
            /// 10401-15600  15.6
            /// 15601-20800  20.8
            /// 20801-31250  31.25
            /// 31251-41700	 41.7
            /// 41701-62500	 62.5
            /// 62501-12500  125.0
            /// 12501-250000 250.0
            /// >250000      500.0
            /// NOTE caution Earlier - Semtech do not recommend BW below 62.5 although, in testing
            /// I managed 31.25 with two devices in close proximity.
            /// \param[in] sbw long, signal bandwidth e.g. 125000
            public void setSignalBandwidth(long sbw)
            {
                byte bw; //register bit pattern

                if (sbw <= 7800)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_7_8KHZ;}
                else if (sbw <= 10400)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_10_4KHZ;}
                else if (sbw <= 15600)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_15_6KHZ;}
                else if (sbw <= 20800)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_20_8KHZ;}
                else if (sbw <= 31250)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_31_25KHZ;}
                else if (sbw <= 41700)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_41_7KHZ;}
                else if (sbw <= 62500)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_62_5KHZ;}
                else if (sbw <= 125000)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_125KHZ;}
                else if (sbw <= 250000)
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_250KHZ;}
                else 
                {bw = (byte)REG_1D_MODEM_CONFIG1.BW_500KHZ;}

                // top 4 bits of reg 1D control bandwidth
                spiWrite(LORA_REGISTER.x_1D_MODEM_CONFIG1, (byte)((spiRead(LORA_REGISTER.x_1D_MODEM_CONFIG1) & ~(byte)REG_1D_MODEM_CONFIG1.BW) | bw));
                // check if low data rate bit should be set or cleared
                setLowDatarate();
            }    
            /// brian.n.norman@gmail.com 9th Nov 2018
            /// Sets the coding rate to 4/5, 4/6, 4/7 or 4/8.
            /// Valid denominator values are 5, 6, 7 or 8. A value of 5 sets the coding rate to 4/5 etc.
            /// Values below 5 are clamped at 5
            /// values above 8 are clamped at 8.
            /// Default for all standard modem config options is 4/5.
            /// \param[in] denominator byte range 5..8
            public void setCodingRate4(byte denominator)
            {
                byte cr = (byte)REG_1D_MODEM_CONFIG1.CODING_RATE_4_5;

            //    if (denominator <= 5)
            //	cr = REG_1D_MODEM_CONFIG1.CODING_RATE_4_5;
                if (denominator == 6)
                {cr = (byte)REG_1D_MODEM_CONFIG1.CODING_RATE_4_6;}
                else if (denominator == 7)
                {cr = (byte)REG_1D_MODEM_CONFIG1.CODING_RATE_4_7;}
                else if (denominator >= 8)
                {cr = (byte)REG_1D_MODEM_CONFIG1.CODING_RATE_4_8;}

                spiWrite(LORA_REGISTER.x_1D_MODEM_CONFIG1, (byte)(((byte)(spiRead(LORA_REGISTER.x_1D_MODEM_CONFIG1) & ~(byte)REG_1D_MODEM_CONFIG1.CODING_RATE)) | cr));
            }

            /// brian.n.norman@gmail.com 9th Nov 2018
            /// sets the low data rate flag if symbol time exceeds 16ms
            /// ref: https://www.thethingsnetwork.org/forum/t/a-point-to-note-lora-low-data-rate-optimisation-flag/12007
            /// called by setBandwidth() and setSpreadingfactor() since these affect the symbol time.
            public void setLowDatarate()
            {
                // called after changing bandwidth and/or spreading factor
                //  Semtech modem design guide AN1200.13 says 
                // "To avoid issues surrounding  drift  of  the  crystal  reference  oscillator  due  to  either  temperature  change  
                // or  motion,the  low  data  rate optimization  bit  is  used. Specifically for 125  kHz  bandwidth  and  SF  =  11  and  12,  
                // this  adds  a  small  overhead  to increase robustness to reference frequency variations over the timescale of the LoRa packet."

                // read current value for BW and SF
                byte BW = (byte)(spiRead(LORA_REGISTER.x_1D_MODEM_CONFIG1) >> 4);	// bw is in bits 7..4
                byte SF = (byte)(spiRead(LORA_REGISTER.x_1E_MODEM_CONFIG2) >> 4);	// sf is in bits 7..4

                // calculate symbol time (see Semtech AN1200.22 section 4)
                float[] bw_tab = {7800, 10400, 15600, 20800, 31250, 41700, 62500, 125000, 250000, 500000};

                float bandwidth = bw_tab[BW];

                double symbolTime = 1000.0 * Math.Pow(2, SF) / bandwidth;	// ms

                // the symbolTime for SF 11 BW 125 is 16.384ms. 
                // and, according to this :- 
                // https://www.thethingsnetwork.org/forum/t/a-point-to-note-lora-low-data-rate-optimisation-flag/12007
                // the LDR bit should be set if the Symbol Time is > 16ms
                // So the threshold used here is 16.0ms

                // the LDR is bit 3 of LORA_REGISTER.x_26_MODEM_CONFIG3
                byte current = (byte)(spiRead(LORA_REGISTER.x_26_MODEM_CONFIG3) & ~(byte)REG_26_MODEM_CONFIG3.LOW_DATA_RATE_OPTIMIZE); // mask off the LDR bit
                if (symbolTime > 16.0)
                {
                    spiWrite(LORA_REGISTER.x_26_MODEM_CONFIG3, (byte)(current | (byte)REG_26_MODEM_CONFIG3.LOW_DATA_RATE_OPTIMIZE));
                }
                else
                {
                    spiWrite(LORA_REGISTER.x_26_MODEM_CONFIG3, current);
                }
            }

            /// brian.n.norman@gmail.com 9th Nov 2018
            /// Allows the CRC to be turned on/off. Default is true (enabled)
            /// When true, RF95 sends a CRC in outgoing packets and requires a valid CRC to be
            /// present and correct on incoming packets.
            /// When false, does not send CRC in outgoing packets and does not require a CRC to be
            /// present on incoming packets. However if a CRC is present, it must be correct.
            /// Normally this should be left on (the default)
            /// so that packets with a bad CRC are rejected. If turned off you wil be much more likely to receive
            /// false noise packets.
            /// \param[in] on bool, true enables CRCs in incoming and outgoing packets, false disables them
            public void setPayloadCRC(bool on)
            {
                // Payload CRC is bit 2 of register 1E
                byte current = (byte)(spiRead(LORA_REGISTER.x_1E_MODEM_CONFIG2) & ~(byte)REG_1E_MODEM_CONFIG2.PAYLOAD_CRC_ON); // mask off the CRC

                if (on)
                {
                    spiWrite(LORA_REGISTER.x_1E_MODEM_CONFIG2, (byte)(current | (byte)REG_1E_MODEM_CONFIG2.PAYLOAD_CRC_ON));
                }
                else
                {
                    spiWrite(LORA_REGISTER.x_1E_MODEM_CONFIG2, current);
                }
                _enableCRC = on;
            }

            /// tilman_1@gloetzner.net
            /// Returns device version from register 42
            /// \param none
            /// \return byte deviceID
            public byte getDeviceVersion()
            {
                _deviceVersion = spiRead(LORA_REGISTER.x_42_VERSION);
                return _deviceVersion;
            }
        
            /// This is a low level function to handle the interrupts for one instance of RF95.
            /// Called automatically by isr*()
            /// Should not need to be called by user code.
            // protected void handleInterrupt()
            // {
            //     // RH_MUTEX_LOCK(lock); // Multithreading support

            //     // we need the RF95 IRQ to be level triggered, or we ……have slim chance of missing events
            //     // https://github.com/geeksville/Meshtastic-esp32/commit/78470ed3f59f5c84fbd1325bcff1fd95b2b20183

            //     // Read the interrupt register
            //     byte irq_flags = spiRead(LORA_REGISTER.x_12_IRQ_FLAGS);
            //     // Read the RegHopChannel register to check if CRC presence is signalled
            //     // in the header. If not it might be a stray (noise) packet.*
            //     byte hop_channel = spiRead(LORA_REGISTER.x_1C_HOP_CHANNEL);
            // //    Serial.println(irq_flags, HEX);
            // //    Serial.println(_mode, HEX);
            // //    Serial.println(hop_channel, HEX);
            // //    Serial.println(_enableCRC, HEX);

            //     // ack all interrupts, 
            //     // Sigh: on some processors, for some unknown reason, doing this only once does not actually
            //     // clear the radio's interrupt flag. So we do it twice. Why? (kevinh - I think the root cause we want level
            //     // triggered interrupts here - not edge.  Because edge allows us to miss handling secondard interrupts that occurred
            //     // while this ISR was running.  Better to instead, configure the interrupts as level triggered and clear pending
            //     // at the _beginning_ of the ISR.  If any interrupts occur while handling the ISR, the signal will remain asserted and
            //     // our ISR will be reinvoked to handle that case)
            //     // kevinh: turn this off until root cause is known, because it can cause missed interrupts!
            //     // spiWrite(LORA_REGISTER.x_12_IRQ_FLAGS, 0xff); // Clear all IRQ flags
            //     spiWrite(LORA_REGISTER.x_12_IRQ_FLAGS, 0xff); // Clear all IRQ flags

            //     // error if:
            //     // timeout
            //     // bad CRC
            //     // CRC is required but it is not present
            //     if (_mode == RadioMode.Rx
            // 	&& (   (irq_flags  &   ((uint)REG_12_IRQ_FLAGS.RX_TIMEOUT | (uint)REG_12_IRQ_FLAGS.PAYLOAD_CRC_ERROR))
            // 	    || (_enableCRC && !((hop_channel == (uint)REG_1C_HOP_CHANNEL.RX_PAYLOAD_CRC_IS_ON)))))
            // //    if (_mode == RadioMode.Rx && irq_flags & (REG_12_IRQ_FLAGS.RX_TIMEOUT | REG_12_IRQ_FLAGS.PAYLOAD_CRC_ERROR))
            //     {
            // //	Serial.println("E");
            // 	_rxBad++;
            //         clearRxBuf();
            //     }
            //     // It is possible to get RX_DONE and CRC_ERROR and VALID_HEADER all at once
            //     // so this must be an else
            //     else if (_mode == RadioMode.Rx && irq_flags & REG_12_IRQ_FLAGS.RX_DONE)
            //     {
            // 	// Packet received, no CRC error
            // //	Serial.println("R");
            // 	// Have received a packet
            // 	byte len = spiRead(LORA_REGISTER.x_13_RX_NB_BYTES);

            // 	// Reset the fifo read ptr to the beginning of the packet
            // 	spiWrite(LORA_REGISTER.x_0D_FIFO_ADDR_PTR, spiRead(LORA_REGISTER.x_10_FIFO_RX_CURRENT_ADDR));
            // 	spiBurstRead(LORA_REGISTER.x_00_FIFO, _buf, len);
            // 	_bufLen = len;

            // 	// Remember the last signal to noise ratio, LORA mode
            // 	// Per page 111, SX1276/77/78/79 datasheet
            // 	_lastSNR = (sbyte)spiRead(LORA_REGISTER.x_19_PKT_SNR_VALUE) / 4;

            // 	// Remember the RSSI of this packet, LORA mode
            // 	// this is according to the doc, but is it really correct?
            // 	// weakest receiveable signals are reported RSSI at about -66
            // 	_lastRssi = spiRead(LORA_REGISTER.x_1A_PKT_RSSI_VALUE);
            // 	// Adjust the RSSI, datasheet page 87
            // 	if (_lastSNR < 0)
            // 	    _lastRssi = _lastRssi + _lastSNR;
            // 	else
            // 	    _lastRssi = (int)_lastRssi * 16 / 15;
            // 	if (_usingHFport)
            // 	    _lastRssi -= 157;
            // 	else
            // 	    _lastRssi -= 164;

            // 	// We have received a message.
            // 	validateRxBuf(); 
            // 	if (_rxBufValid)
            // 	    setModeIdle(); // Got one 
            //     }
            //     else if (_mode == RadioMode.Tx && irq_flags & REG_12_IRQ_FLAGS.TX_DONE)
            //     {
            // //	Serial.println("T");
            // 	_txGood++;
            // 	setModeIdle();
            //     }
            //     else if (_mode == RadioMode.Cad && irq_flags & REG_12_IRQ_FLAGS.CAD_DONE)
            //     {
            // //	Serial.println("C");
            //         _cad = irq_flags & REG_12_IRQ_FLAGS.CAD_DETECTED;
            //         setModeIdle();
            //     }
            //     else
            //     {
            // //	Serial.println("?");
            //     }

            //     // Sigh: on some processors, for some unknown reason, doing this only once does not actually
            //     // clear the radio's interrupt flag. So we do it twice. Why?
            //     spiWrite(LORA_REGISTER.x_12_IRQ_FLAGS, 0xff); // Clear all IRQ flags
            //     spiWrite(LORA_REGISTER.x_12_IRQ_FLAGS, 0xff); // Clear all IRQ flags
            //     // RH_MUTEX_UNLOCK(lock); 
            // }

            // /// Examine the revceive buffer to determine whether the message is for this node
            // protected void validateRxBuf()
            // {
            //     if (_bufLen < 4)
            //     {
            //         return; // Too short to be a real message
            //     }
            //     // Extract the 4 headers
            //     _rxHeaderTo    = _buf[0];
            //     _rxHeaderFrom  = _buf[1];
            //     _rxHeaderId    = _buf[2];
            //     _rxHeaderFlags = _buf[3];
            //     if (_promiscuous ||
            //     _rxHeaderTo == _thisAddress ||
            //     _rxHeaderTo == BROADCAST_ADDRESS)
            //     {
            //     _rxGood++;
            //     _rxBufValid = true;
            //     }
            // }

            /// Clear our local receive buffer
            protected void clearRxBuf()
            {
                // ATOMIC_BLOCK_START;
                _rxBufValid = false;
                _bufLen = 0;
                // ATOMIC_BLOCK_END;
            }

            /// Called by RF95 when the radio mode is about to change to a new setting.
            /// Can be used by subclasses to implement antenna switching etc.
            /// \param[in] mode RadioMode the new mode about to take effect
            /// \return true if the subclasses changes successful
            protected bool modeWillChange(RadioMode mode) {return true;}

            /// False if the PA_BOOST transmitter output pin is to be used.
            /// True if the RFO transmitter output pin is to be used.
            protected bool _useRFO;
        
            // /// Low level interrupt service routine for device connected to interrupt 0
            // private static void         isr0()
            // {
            //     if (_deviceForInterrupt[0])
            // 	_deviceForInterrupt[0]->handleInterrupt();
            // }

            // /// Low level interrupt service routine for device connected to interrupt 1
            // private static void         isr1()
            // {
            //     if (_deviceForInterrupt[1])
            // 	_deviceForInterrupt[1]->handleInterrupt();
            // }

            // /// Low level interrupt service routine for device connected to interrupt 1
            // private static void         isr2()
            // {
            //     if (_deviceForInterrupt[2])
            // 	_deviceForInterrupt[2]->handleInterrupt();
            // }

            /// Array of instances connected to interrupts 0 and 1
            private static RF95[] _deviceForInterrupt = new RF95[NUM_INTERRUPTS];

            /// Index of next interrupt number to use in _deviceForInterrupt
            private static byte _interruptCount;

            // /// The configured interrupt pin connected to this instance
            // private byte _interruptPin;

            /// The index into _deviceForInterrupt[] for this device (if an interrupt is already allocated)
            /// else 0xff
            private byte _myInterruptIndex;

            /// Number of octets in the buffer
            private volatile byte    _bufLen;

            /// The receiver/transmitter buffer
            private byte[] _buf = new byte[MAX_PAYLOAD_LEN];

            /// True when there is a valid message in the buffer
            private volatile bool       _rxBufValid;

            /// True if we are using the HF port (779.0 MHz and above)
            private bool                _usingHFport;

            // /// Last measured SNR, dB
            // private byte              _lastSNR;

            /// If true, sends CRCs in every packet and requires a valid CRC in every received packet
            private bool                _enableCRC;

            /// device ID
            private byte		_deviceVersion = 0x00;



            /// The current transport operating mode
            RadioMode     _mode = RadioMode.Initializing;

            /// This node id
            byte _thisAddress = BROADCAST_ADDRESS;

            // /// Whether the transport is in promiscuous mode
            // bool                _promiscuous;

            /// TO header in the last received mesasge
            volatile byte _rxHeaderTo;

            /// FROM header in the last received mesasge
            volatile byte    _rxHeaderFrom;

            /// ID header in the last received mesasge
            volatile byte    _rxHeaderId;

            /// FLAGS header in the last received mesasge
            volatile byte    _rxHeaderFlags;

            /// TO header to send in all messages
            byte             _txHeaderTo = BROADCAST_ADDRESS;

            /// FROM header to send in all messages
            byte             _txHeaderFrom = BROADCAST_ADDRESS;

            /// ID header to send in all messages
            byte             _txHeaderId = 0;

            /// FLAGS header to send in all messages
            byte             _txHeaderFlags = 0;

            /// The value of the last received RSSI value, in some transport specific units
            volatile short     _lastRssi;

            /// Count of the number of bad messages (eg bad checksum etc) received
            volatile ushort   _rxBad = 0;

            /// Count of the number of successfully transmitted messaged
            volatile ushort   _rxGood = 0;

            /// Count of the number of bad messages (correct checksum etc) received
            volatile ushort   _txGood = 0;

            // /// Channel activity detected
            // volatile bool       _cad;

            // /// Channel activity timeout in ms
            // uint        _cad_timeout;

            /// The pin number of the Slave Select pin that is used to select the desired device.
            byte             _slaveSelectPin;

            ///The spi interface object
            private SpiDevice _spiDevice;

            // GPIO Controller for chip select functionality
            private GpioController _controller;
        }
    }
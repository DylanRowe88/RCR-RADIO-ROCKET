// See https://aka.ms/new-console-template for more information

using System;
using System.Device.Spi;
using System.Device.Gpio;
using System.Threading;
using System.IO.Ports;

namespace SPS2._0
{
    class USB_UART_Demo
    {
 // Define the serial port settings
    static string portName = "/dev/ttyACM0"; // The port name may vary, check your Arduino connection
    // Create a new SerialPort instance
    static SerialPort serialPort = new SerialPort(portName);
       static void Main()
        {
            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {   
                Console.WriteLine("   {0}", s);
            }
            // Open the serial port
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = Handshake.None;
            serialPort.RtsEnable = false;
            serialPort.Open();
            // Check if the serial port is open
            // Read input from the user and send it to Arduino
            while (true)
            {
                if (serialPort.IsOpen)
                {   
                    serialPort.Write("test*");
                    Thread.Sleep(3000);
                }
                else
                {
                    Console.WriteLine("Trying to open port");
                    serialPort.Open();
                }
            }
        }
        }
    // class SPIDemo
    // {
    //     // Create SPI settings
    //     static SpiConnectionSettings settings = new SpiConnectionSettings(0, 0) // Replace with your specific bus and chip select line
    //     {
    //         // ChipSelectLine = 24,// The chip select line used on the bus.
    //         ChipSelectLineActiveState = System.Device.Gpio.PinValue.Low, //Specifies which value on chip select pin means "active".
    //         ClockFrequency = 6000000, // The frequency in which the data will be transferred.
    //         DataBitLength = 8,	// The length of the data to be transfered.
    //         DataFlow = System.Device.Spi.DataFlow.MsbFirst,	//Specifies order in which bits are transferred first on the SPI bus.
    //         Mode = SpiMode.Mode0//The SPI mode being used.
    //     };

    //     struct RadioPacket 
    //     {
    //         public double num;
    //     };

    //    static void Main()
    //     {
    //         ///The spi interface object
    //         using(SpiDevice _spiDevice = SpiDevice.Create(settings))
    //         {
    //     // GPIO Controller for chip select functionality
    //             using (GpioController _controller = new GpioController())
    //             {
    //                 RF95 myRadio = new RF95(_spiDevice, _controller);
    //                 myRadio.init();
    //                 RadioPacket myPacket = new RadioPacket{num = 123456.7};
    //                 while(true)
    //                 {
    //                 myRadio.send(BitConverter.GetBytes(myPacket.num), 4);
    //                 myPacket.num += 1;
    //                 }                
    //             }
    //         }
            


    //         // Write
    //         // byte[] sendData5 = { RF95_LORA_REGISTER.RH_RF95_REG_01_OP_MODE | 0x80, 0x41 }; 
    //         // byte[] receiveData5 = new byte[sendData5.Length];

    //         // spiDevice.TransferFullDuplex(sendData5, receiveData5);
            
    //         // foreach (byte data in receiveData5)
    //         // {
    //         //     Console.WriteLine(data);
    //         // }
    //         // Console.WriteLine("Wait..");
    //         // Thread.Sleep(2000);



    //         // byte[] sendData4 = {RF95_LORA_REGISTER.RH_RF95_REG_01_OP_MODE & 0x7F, 0x00 }; 
    //         // byte[] receiveData4 = new byte[sendData4.Length];
            
    //         // spiDevice.TransferFullDuplex(sendData4, receiveData4);
    //         // foreach (byte data in receiveData4)
    //         // {
    //         // Console.WriteLine(data);
    //         // }
    //         // //Write
    //         // //    byte[] sendData4 = { 0x81, 0x41}; 
    //         // //    byte[] receiveData4 = new byte[sendData4.Length];
    //         // spiDevice.TransferFullDuplex(sendData4, receiveData4);
    //         // foreach (byte data in receiveData4)
    //         // {
    //         // Console.WriteLine(data);
    //         // }            
    //     }
    // }
}

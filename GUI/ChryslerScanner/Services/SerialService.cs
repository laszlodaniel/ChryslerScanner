using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChryslerScanner.Helpers;
using ChryslerScanner.Models;

namespace ChryslerScanner.Services
{
    public class SerialService
    {
        private readonly SerialPort SP;
        //private readonly ConcurrentQueue<Packet> RxQueue;
        private readonly ConcurrentQueue<Packet> TxQueue;
        private Task RxPump;
        private Task TxPump;
        private CancellationTokenSource CTSource;
        public event EventHandler<Packet> PacketReceived;

        public SerialService()
        {
            SP = new SerialPort();
            //RxQueue = new ConcurrentQueue<Packet>();
            TxQueue = new ConcurrentQueue<Packet>();
        }

        public bool Connect(string port)
        {
            bool success = false;
            
            if (SP.IsOpen)
            {
                SP.Close();
            }

            SP.PortName = port;
            SP.BaudRate = Properties.Settings.Default.UARTBaudrate;
            SP.DataBits = 8;
            SP.StopBits = StopBits.One;
            SP.Parity = Parity.None;
            SP.ReadTimeout = 500;
            SP.WriteTimeout = 500;

            try
            {
                SP.Open();
                SP.BaseStream.Flush();

                CTSource = new CancellationTokenSource();
                var CT = CTSource.Token;

                RxPump = Task.Run(async () => await RxTask(CT), CT);
                TxPump = Task.Run(async () => await TxTask(CT), CT);

                success = true;
                Debug.WriteLine($"{DateTime.UtcNow:O} SerialPort connected");
            }
            catch
            {
                success = false;
                Debug.WriteLine($"{DateTime.UtcNow:O} SerialPort error");
            }

            return success;
        }

        public void WritePacket(Packet packet)
        {
            if (packet == null)
                return;

            TxQueue.Enqueue(packet);
        }

        public void WritePacket(List<Packet> packets)
        {
            if (packets == null)
                return;

            foreach (Packet packet in packets)
            {
                if (packet == null)
                    continue;

                TxQueue.Enqueue(packet);
            }
        }

        public bool Disconnect()
        {
            bool success = false; 

            CTSource.Cancel();

            if (SP.IsOpen)
            {
                SP.Close();
            }

            Task[] tasks = { TxPump, RxPump };
            Task.WaitAll(tasks, 100);

            success = true;

            Debug.WriteLine($"{DateTime.UtcNow:O} SerialPort disconnected");

            return success;
        }

        private async Task RxTask(CancellationToken CT)
        {
            var buffer = new byte[4096];
            int index = 0;

            Debug.WriteLine($"{DateTime.UtcNow:O} RxTask started");

            try
            {
                while (SP.IsOpen && !CT.IsCancellationRequested)
                {
                    if (SP.BytesToRead == 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    int count = await SP.BaseStream.ReadAsync(buffer, index, 1); // read one byte

                    if (count == 0)
                        continue;

                    if (buffer[0] != PacketHelper.PacketSync)
                        continue;

                    index++;

                    if (index < 3)
                        continue; // wait until 3 bytes are present in the buffer

                    int PacketLength = (buffer[1] << 8) + buffer[2];

                    if (index < (PacketLength + 4))
                        continue; // wait until all packet bytes are received

                    Packet packet = PacketHelper.Deserialize(buffer.Take(index).ToArray());

                    index = 0;

                    if (packet == null)
                        continue;

                    PacketReceived?.Invoke(this, packet);

                    //RxQueue.Enqueue(packet);

                    //while (!RxQueue.IsEmpty)
                    //{
                    //    if (RxQueue.TryDequeue(out byte[] qpacket))
                    //    {
                    //        PacketReceived?.Invoke(this, PacketHelper.ParsePacket(qpacket));
                    //    }

                    //    await Task.Delay(1);
                    //}
                }

                Debug.WriteLine($"{DateTime.UtcNow:O} RxTask finished");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.UtcNow:O} RxTask: Exception {ex}");
            }
        }

        private async Task TxTask(CancellationToken CT)
        {
            Debug.WriteLine($"{DateTime.UtcNow:O} TxTask started");

            try
            {
                while (SP.IsOpen && !CT.IsCancellationRequested)
                {
                    if (TxQueue.TryDequeue(out Packet packet))
                    {
                        if (packet == null)
                            continue;

                        byte[] serialized = PacketHelper.Serialize(packet);

                        if (serialized == null)
                            continue;

                        await SP.BaseStream.WriteAsync(serialized, 0, serialized.Length);

                        //continue;
                    }

                    await Task.Delay(1);
                }

                Debug.WriteLine($"{DateTime.UtcNow:O} TxTask finished");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.UtcNow:O} TxTask: Exception {ex}");
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AbpLib.Networking
{
    public class TCP
    {
        private string currentPacket = "";
        public string PacketResult
        {
            get
            {
                return currentPacket;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    currentPacket = value;
                    OnPacketChanged("Packet");
                }
            }
        }
        public event PropertyChangedEventHandler PacketChanged;

        private void OnPacketChanged([CallerMemberName] string propertyName = "")
        {
            PacketChanged?.Invoke(PacketResult, new PropertyChangedEventArgs(nameof(propertyName)));
        }

        private readonly TcpListener TL;

        public TCP(int port)
        {
            TL = new TcpListener(IPAddress.Any, port);
        }

        public bool Start()
        {
            try
            {
                new Thread(async _ =>
                {
                    TL.Start();
                    while (true)
                    {
                        TcpClient Receiver = await TL.AcceptTcpClientAsync();
                        NetworkStream ns = Receiver.GetStream();
                        if (ns.ReadByte() == 2)
                        {
                            PacketResult = await ns.GetPacket();
                            Receiver.Close();
                            ns.Close();
                        }
                    }
                })
                { IsBackground = true }.Start();
            }
            catch(Exception)
            {
                TL.Stop();
                return false;
            }
            return true;
        }

    }
    public static class TCPex
    {
        public static async Task<string> GetPacket(this NetworkStream n)
        {
            int ReadByte, ByteOffSet = 0;
            string SLength = "";

            while ((ReadByte = n.ReadByte()) != 4)
            {
                SLength += (char)ReadByte;
            }
            int DLength = int.Parse(SLength);

            byte[] buffer = new byte[DLength];
            if (ByteOffSet < DLength)
            {
                await n.ReadAsync(buffer.AsMemory(ByteOffSet, DLength - ByteOffSet));
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<bool> SendPacket(this string input, string ip = "", int port = 6868)
        {
            ip = ip == "" ? "127.0.0.1" : ip;
            TcpClient tcpclient = new TcpClient
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            };

            try
            {
                using MemoryStream ms = new MemoryStream();
                await tcpclient.ConnectAsync(ip, port);
                using NetworkStream ns = new NetworkStream(tcpclient.Client);
                byte[] PacketSize = Encoding.Default.GetBytes(input.Length.ToString());
                byte[] PacketBytes = Encoding.Default.GetBytes(input);
                ms.WriteByte(2);
                await ms.WriteAsync(PacketSize.AsMemory(0, PacketSize.Length));
                ms.WriteByte(4);
                await ms.WriteAsync(PacketBytes.AsMemory(0, PacketBytes.Length));
                await ns.WriteAsync(ms.GetBuffer().AsMemory(0, ms.GetBuffer().Length));
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                tcpclient.Close();
            }
            return true;
        }
    }
}

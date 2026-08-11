using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TEST_MAIN_SERVER
{

    /// <summary>   통신 구조 구축 & 개발    </summary>
    /// <remarks>   hjkim, 2026-08-11.       </remarks>

    public enum MainCommand
    {
        NONE = 0,

        // Main → AI
        INSPECTION_INFO = 100,
        STRIP_NUMBER = 110,
        INSPECTION_DONE = 120,

        // AI → Main
        R_INSPECTION_INFO = 200,
        R_STRIP_NUMBER = 210,
        R_INSPECTION_DONE = 220,

        INFERENCE_DONE = 300,

        // Connection Check
        PING = 900,
        PONG = 901
    }

    /// <summary>
    /// TCP Packet Header.
    /// 
    /// Magic       : Packet 시작 확인하는거. HDSA → HDS AI 줄임말로 그냥 만듬.
    /// Command     : 몇개 안되는 명령 Command
    /// PayloadSize : Command 뒤에 Data 크기 
    /// </summary>
    public class PacketHeader
    {
        public const int MagicNumber = 0x48445341; // HDSA
        public const int HeaderSize = 12;

        public int Magic { get; set; }
        public MainCommand Command { get; set; }
        public int PayloadSize { get; set; }
        public PacketHeader()
        {
            Magic = MagicNumber;
        }
        public byte[] Serialize()
        {
            byte[] result = new byte[HeaderSize];

            Buffer.BlockCopy(BitConverter.GetBytes(Magic), 0, result, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int)Command), 0, result, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(PayloadSize), 0, result, 8, 4);

            return result;
        }

        public static PacketHeader Deserialize(byte[] buffer)
        {
            if (buffer == null || buffer.Length != HeaderSize)
                throw new ArgumentException("Packet Header 크기가 올바르지 않습니다.");

            PacketHeader header = new PacketHeader
            {
                Magic = BitConverter.ToInt32(buffer, 0),
                Command = (MainCommand)BitConverter.ToInt32(buffer, 4),
                PayloadSize = BitConverter.ToInt32(buffer, 8)
            };

            if (header.Magic != MagicNumber)
                throw new ArgumentException("Packet Magic Number가 올바르지 않습니다.");

            if (header.PayloadSize < 0 || header.PayloadSize > 1024 * 1024)
                throw new ArgumentException("Payload Size가 올바르지 않습니다. " + $"{header.PayloadSize}");

            return header;
        }
    }

    /// <summary>
    /// 실제 완성된 패킷
    /// </summary>
    public class MainPacket
    {
        public MainCommand Command { get; set; }
        public byte[] Payload { get; set; }
        public MainPacket()
        {
            Payload = new byte[0];
        }

        public MainPacket(MainCommand cmd, byte[] payload = null)
        {
            Command = cmd;
            Payload = payload ?? new byte[0];
        }
    }

    /// <summary>
    /// Product Info
    /// 
    /// 고정 Byte 배열로 최대치 할당 후에 사용하도록 만듦. 
    /// </summary>
    public class ProductInfo
    {
        private const int DeviceNameSize = 10;
        private const int ProductNameSize = 100;
        private const int OrderNumberSize = 30;

        public const int PayloadSize = DeviceNameSize + ProductNameSize + OrderNumberSize;

        public string DeviceName { get; set; }
        public string ProductName { get; set; }
        public string OrderNumber { get; set; }

        public byte[] Serialize()
        {
            byte[] result = new byte[PayloadSize];
            WriteFixedString(result, 0, DeviceNameSize, DeviceName);
            WriteFixedString(result, DeviceNameSize, ProductNameSize, ProductName);
            WriteFixedString(result, DeviceNameSize + ProductNameSize, OrderNumberSize, OrderNumber);

            return result;
        }

        public static ProductInfo Deserialize(byte[] data)
        {
            if (data == null || data.Length != PayloadSize) throw new Exception($"크기 사이즈 등이 올바르지 않습니다. {PayloadSize}");

            ProductInfo info = new ProductInfo();

            info.DeviceName = ReadFixedString(data, 0, DeviceNameSize);
            info.ProductName = ReadFixedString(data, DeviceNameSize, ProductNameSize);
            info.OrderNumber = ReadFixedString(data, DeviceNameSize + ProductNameSize, OrderNumberSize);

            return info;
        }

        private static void WriteFixedString(byte[] destination, int offset, int size, string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }

            //System ANSI Encoding 사용해서 1byte로 만들어서 씁시다잉~
            byte[] source = Encoding.Default.GetBytes(value);
            int copyLength = Math.Min(source.Length, size - 1);

            Buffer.BlockCopy(source, 0, destination, offset, copyLength);
        }

        private static string ReadFixedString(byte[] source, int offset, int size)
        {
            int length = 0;

            for (int i = 0; i < size; i++)
            {
                if (source[offset + i] == 0)
                    break;
                length++;
            }

            return Encoding.Default.GetString(source, offset, length).Trim();
        }
    }

    /// <summary>
    /// 단순한 Int형 Payload
    /// Strip Number 등에 사용할 예정
    /// </summary>
    public static class IntPayload
    {
        public static byte[] Serialize(int value)
        {
            return BitConverter.GetBytes(value);
        }
        public static int Deserialize(byte[] data)
        {
            if (data == null || data.Length != 4)
                throw new ArgumentException("Int Payload 크기가 올바르지 않습니다");

            return BitConverter.ToInt32(data, 0);

        }
    }

    public static class BoolPayload
    {
        public static byte[] Serialize(bool value)
        {
            return new byte[]
            {
                value ? (byte)1 : (byte)0
            };
        }

        public static bool Deserialize(byte[] data)
        {
            if (data == null || data.Length != 1)
                throw new ArgumentException("Bool Payload 크기가 올바르지 않습니다.");

            return data[0] != 0;
        }
    }

    /// <summary>
    /// TCP 특성상 Read() 한번으로 원하는 길이가 제대로 다 들어오는 보장은 없음
    /// 그래서 Length 만큼 반복해서 읽어야함
    /// </summary>
    public static class PacketStreamHelper
    {
        public static byte[] ReadExact(Stream stream, int length)
        {
            if (length == 0) return new byte[0];

            byte[] buffer = new byte[length];

            int offset = 0;

            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);

                if (read <= 0)
                    throw new EndOfStreamException("연결이 끊어졌습니다.");

                offset += read;
            }

            return buffer;
        }

        public static MainPacket ReadPacket(Stream stream)
        {
            byte[] headerBuffer = ReadExact(stream, PacketHeader.HeaderSize);

            PacketHeader header = PacketHeader.Deserialize(headerBuffer);
            byte[] payload = ReadExact(stream, header.PayloadSize);

            return new MainPacket(header.Command, payload);
        }

        public static void WritePacket(Stream stream, MainPacket packet)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (packet == null) throw new ArgumentNullException(nameof(packet));

            byte[] payload = packet.Payload ?? new byte[0];

            PacketHeader header = new PacketHeader
            {
                Command = packet.Command,
                PayloadSize = payload.Length
            };

            byte[] headerBuffer = header.Serialize();
            stream.Write(headerBuffer, 0, headerBuffer.Length);

            if (payload.Length > 0)
                stream.Write(payload, 0, payload.Length);

            stream.Flush();
        }
    }
}

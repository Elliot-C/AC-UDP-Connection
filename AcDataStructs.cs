using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AcUdpCommunication
{
    internal static class AcConverter
    {
        // Copy structure to new memory, return array (pointer) of raw bytes.
        public static byte[] StructToBytes<T>(T str) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr<T>(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        // Copy bytes to memory, return object from those bytes.
        public static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            T str = default(T);
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            str = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        // Data to send for initial handshake, update mode selection, and dismissal.
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Handshaker
        {
            public Handshaker(HandshakeOperation operationId, uint identifier = 1, uint version = 1)
            {
                this.identifier = identifier;
                this.version = version;
                this.operationId = operationId;
            }

            [MarshalAs(UnmanagedType.U4)]
            public uint identifier; // Android, iOS, currently not used.

            [MarshalAs(UnmanagedType.U4)]
            public uint version; // Expected AC remote telemetry interface version.

            [MarshalAs(UnmanagedType.U4)]
            public HandshakeOperation operationId; // Type of handshake packet.

            public enum HandshakeOperation
            {
                Connect,
                CarInfo,
                Lapinfo,
                Disconnect
            };
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct HandshakerResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string carName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string driverName;

            [MarshalAs(UnmanagedType.U4)]
            public uint identifier; // Status code from the server, currently just '4242' to see that it works.

            [MarshalAs(UnmanagedType.U4)]
            public uint version; // Server version, not yet supported.

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string trackName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string trackConfig;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct RTLap
        {
            [MarshalAs(UnmanagedType.U4)]
            public int carIdentifierNumber;

            [MarshalAs(UnmanagedType.U4)]
            public int lap;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string driverName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string carName;

            [MarshalAs(UnmanagedType.U4)]
            public int time;
        };

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1, Size = 328)]
        public struct RTCarInfo
        {
            [FieldOffset(0 * 4)]
            public char identifier;
            [FieldOffset(1 * 4)]
            public int size;

            [FieldOffset(2 * 4)]
            public float speed_Kmh;
            [FieldOffset(3 * 4)]
            public float speed_Mph;
            [FieldOffset(4 * 4)]
            public float speed_Ms;

            [FieldOffset(5 * 4)]
            public bool isAbsEnabled;
            [FieldOffset(5 * 4 + 1)]
            public bool isAbsInAction;
            [FieldOffset(5 * 4 + 2)]
            public bool isTcInAction;
            [FieldOffset(5 * 4 + 3)]
            public bool isTcEnabled;
            [FieldOffset(6 * 4 + 2)]
            public bool isInPit;
            [FieldOffset(6 * 4 + 3)]
            public bool isEngineLimiterOn;

            [FieldOffset(7 * 4)]
            public float accG_vertical;
            [FieldOffset(8 * 4)]
            public float accG_horizontal;
            [FieldOffset(9 * 4)]
            public float accG_frontal;

            [FieldOffset(10 * 4)]
            public int lapTime;
            [FieldOffset(11 * 4)]
            public int lastLap;
            [FieldOffset(12 * 4)]
            public int bestLap;
            [FieldOffset(13 * 4)]
            public int lapCount;

            [FieldOffset(14 * 4)]
            public float gas;
            [FieldOffset(15 * 4)]
            public float brake;
            [FieldOffset(16 * 4)]
            public float clutch;
            [FieldOffset(17 * 4)]
            public float engineRPM;
            [FieldOffset(18 * 4)]
            public float steer;
            [FieldOffset(19 * 4)]
            public int gear;
            [FieldOffset(20 * 4)]
            public float cgHeight;

            [FieldOffset(21 * 4)]
            public PerWheel wheelAngularSpeed;
            [FieldOffset(25 * 4)]
            public PerWheel slipAngle;
            [FieldOffset(29 * 4)]
            public PerWheel slipAngle_ContactPatch;
            [FieldOffset(33 * 4)]
            public PerWheel slipRatio;
            [ FieldOffset(37 * 4)]
            public PerWheel tyreSlip;
            [FieldOffset(41 * 4)]
            public PerWheel ndSlip;
            [ FieldOffset(45 * 4)]
            public PerWheel load;
            [FieldOffset(49 * 4)]
            public PerWheel Dy;
            [FieldOffset(53 * 4)]
            public PerWheel Mz;
            [FieldOffset(57 * 4)]
            public PerWheel tyreDirtyLevel;

            [FieldOffset(61 * 4)]
            public PerWheel camberRAD;
            [FieldOffset(65 * 4)]
            public PerWheel tyreRadius;
            [FieldOffset(69 * 4)]
            public PerWheel tyreLoadedRadius;
            [FieldOffset(73 * 4)]
            public PerWheel suspensionHeight;

            [FieldOffset(77 * 4)]
            public float carPositionNormalized;
            [FieldOffset(78 * 4)]
            public float carSlope;
            [FieldOffset(79 * 4)]
            public Coordinates carCoordinates;

        };
    }

    public struct PerWheel
    {
        public float LF;
        public float RF;
        public float LR;
        public float RR;
    }
    public struct Coordinates
    {
        public float X;
        public float Y;
        public float Z;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace AcUdpCommunication
{
    public class AcUdpConnection
    {
        private const int AC_PORT = 9996;

        public ConnectionType DataType { get; private set; } // The type of data to request from the server.
        public SessionInfo SessionInfo { get; private set; } = new SessionInfo();
        public LapInfo LapInfo { get; private set; } = new LapInfo();
        public CarInfo CarInfo { get; private set; } = new CarInfo();
        
        public bool IsConnected { get; private set; }

        private CancellationToken cancellation;

        private readonly IPEndPoint AcHost;
        private Socket? socket;
        private CancellationTokenSource? source;

        public delegate void UpdatedEventDelegate(object sender, AcUpdateEventArgs e);
        public event UpdatedEventDelegate? LapUpdate;
        public event UpdatedEventDelegate? CarUpdate;

        public AcUdpConnection(string IpAddress, ConnectionType mode)
        {
            DataType = mode;
            AcHost = new IPEndPoint(IPAddress.Parse(IpAddress), AC_PORT);

        }

        ~AcUdpConnection()
        {
            Disconnect();
        }

        public async Task Connect()
        {
            cancellation = new CancellationToken(false);
            source = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            if (IsConnected) return;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                await socket.ConnectAsync(AcHost);
                SendHandshake(AcConverter.Handshaker.HandshakeOperation.Connect);
                _ = DoReceiveAsync(socket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DoReceiveAsync(Socket udpSocket)
        {
            byte[] buffer = new byte[512];

            while (!cancellation.IsCancellationRequested)
            {
               
                await udpSocket.ReceiveFromAsync(buffer, SocketFlags.None, AcHost);
                Socket_MessageReceived(buffer);
                
            }
        }


        public void Disconnect()
        {
            if (socket is not null && socket.Connected)
            {
                // Sign off from server, then close the socket.
                SendHandshake(AcConverter.Handshaker.HandshakeOperation.Disconnect);
                source?.Cancel();
                socket.Dispose();
                IsConnected = false;
            }
        }

        private void SendHandshake(AcConverter.Handshaker.HandshakeOperation operationId)
        {
            // Calculate handshake bytes and send them.
            if (socket is not null)
            {
                byte[] sendbytes = AcConverter.StructToBytes(new AcConverter.Handshaker(operationId));
                socket.SendAsync(sendbytes, SocketFlags.None);
            }
        }


        private void Socket_MessageReceived(byte[] buffer)
        {
            if(buffer is null)
            {
                return;
            }

            if (!IsConnected) // Received data is handshake response.
            {

                AcConverter.HandshakerResponse response = AcConverter.BytesToStruct<AcConverter.HandshakerResponse>(buffer);

                // Set session info data.
                SessionInfo.DriverName = AcHelperFunctions.SanitiseString(response.driverName);
                SessionInfo.CarName = AcHelperFunctions.SanitiseString(response.carName);
                SessionInfo.TrackName = AcHelperFunctions.SanitiseString(response.trackName);
                SessionInfo.TrackLayout = AcHelperFunctions.SanitiseString(response.trackConfig);

                // Confirm handshake with data type.
                SendHandshake((AcConverter.Handshaker.HandshakeOperation)DataType);
                IsConnected = true;
            }
            else // An actual info packet!
            {
                switch (DataType)
                {
                    case ConnectionType.CarInfo:
                        AcConverter.RTCarInfo rtcar = AcConverter.BytesToStruct<AcConverter.RTCarInfo>(buffer);

                        CarInfo.SpeedAsKPH = rtcar.speed_Kmh;
                        CarInfo.EngineRPM = rtcar.engineRPM;
                        CarInfo.Gear = rtcar.gear;

                        CarInfo.CurrentLapTime = TimeSpan.FromMilliseconds(rtcar.lapTime);
                        CarInfo.LastLapTime = TimeSpan.FromMilliseconds(rtcar.lastLap);
                        CarInfo.BestLapTime = TimeSpan.FromMilliseconds(rtcar.bestLap);

                        if (CarUpdate != null)
                        {
                            AcUpdateEventArgs updateArgs = new()
                            {
                                carInfo = this.CarInfo
                            };

                            CarUpdate(this, updateArgs);
                        }
                        break;
                    case ConnectionType.LapTime:

                        AcConverter.RTLap rtlap = AcConverter.BytesToStruct<AcConverter.RTLap>(buffer);

                        // Set last lap info data.
                        LapInfo.CarName = AcHelperFunctions.SanitiseString(rtlap.carName);
                        LapInfo.DriverName = AcHelperFunctions.SanitiseString(rtlap.driverName);
                        LapInfo.CarNumber = rtlap.carIdentifierNumber;
                        LapInfo.LapNumber = rtlap.lap;
                        LapInfo.LapTime = TimeSpan.FromMilliseconds(rtlap.time);

                        if (LapUpdate != null)
                        {
                            AcUpdateEventArgs updateArgs = new()
                            {
                                lapInfo = this.LapInfo
                            };

                            LapUpdate(this, updateArgs);
                        }   
                        break;
                    default:
                        break;
                }
            }
            
        }

        
        public enum ConnectionType
        {
            CarInfo = 1,
            LapTime = 2
        };

        public class AcUpdateEventArgs : EventArgs
        {
            public LapInfo? lapInfo;
            public CarInfo? carInfo;
        }
    }

}
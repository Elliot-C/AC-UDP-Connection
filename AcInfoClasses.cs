using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace AcUdpCommunication
{
    public class SessionInfo
    {
        public string DriverName { get; set; } = "drivername";
        public string CarName { get; set; } = "carname";
        public string TrackName { get; set; } = "trackname";
        public string TrackLayout { get; set; } = "tracklayout";
    }

    public class LapInfo
    {
        public string DriverName { get; set; } = "drivername";
        public string CarName { get; set; } = "carname";
        public int CarNumber { get; set; } = 0;
        public int LapNumber { get; set; } = 0;
        public TimeSpan LapTime { get; set; } = TimeSpan.Zero;
    }

    public class CarInfo
    {

        public TimeSpan LastLapTime { get; set; } = TimeSpan.Zero;
        public TimeSpan BestLapTime { get; set; } = TimeSpan.Zero;
        public TimeSpan CurrentLapTime { get; set; } = TimeSpan.Zero;

        public int LapNumber{ get; set; }
        
        public float SpeedAsKPH { get; set; }
        public int Gear { get; set; } = 0;
        public float MaxRPM { get; private set; }
        private float _enginerpm = 0;
        public float EngineRPM
        {
            get { return _enginerpm; }
            set
            {
                if (value > MaxRPM)
                    MaxRPM = value;
                _enginerpm = value;
            }
        }
    }

    internal static class AcHelperFunctions
    {
        public static string SanitiseString(string instring)
        {
            int index = instring.IndexOf('%');
            return instring[..index];
        }
    }
}

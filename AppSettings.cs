using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherTestApp2
{
    public static class AppSettings
    {
        private static bool _isCelsius = true;

        public static bool IsCelsius
        {
            get { return _isCelsius; }
            set { _isCelsius = value; }
        }

        private static string _currentCity = "LONDON";
        public static string CityCurrent
        {
            get { return _currentCity; }
            set { _currentCity = value; }
        }

        private static double _latitude = 51.51;
        public static double Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        private static double _longitude = -0.13;
        public static double Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
    }
}

namespace GpsData
{
    /// <summary>
    /// A class representing a user's location at a point in time.  This class is largely a
    /// 1:1 mapping with Android.Locations.Location.  (so that we don't have to take an android
    /// dependency for unit tests to run)
    /// </summary>
    public class GpsLocation
    {
        public float Accuracy { get; }

        public long Time { get; }

        public double Latitude { get; }

        public double Longitude { get; }

        public float Bearing { get; }

        public float Speed { get; }

        public string Provider { get; }

        public GpsLocation(float accuracy, long time, double latitude, double longitude,
            float bearing, float speed, string provider)
        {
            Accuracy = accuracy;
            Time = time;
            Latitude = latitude;
            Longitude = longitude;
            Bearing = bearing;
            Speed = speed;
            Provider = provider;
        }
    }
}

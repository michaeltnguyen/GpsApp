using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.App;
using System.Threading;

namespace GpsApp
{
    /// <summary>
    /// A foreground service that records user locations and stores them in the <see cref="GpsApp.Database.GpsDatabase"/>.
    /// 
    /// Running in the foreground is necessary for detecting location over prolonged periods (so the app is not killed).
    /// 
    /// Also note that while the user may have granted coarse permission, fine permission is required.  Coarse location
    /// is only accurate to ~3 km, which is not enough.
    /// </summary>
    [Service(Name ="com.chessdork.gps.GpsService", ForegroundServiceType = ForegroundService.TypeLocation, Permission = Android.Manifest.Permission.AccessFineLocation)]
    public class GpsService : Service, ILocationListener
    {
        const string HandlerThreadName = "gps_handler_thread";
        const string NotificationChannelId = "gps_notification_channel_id";
        const int NotificationId = 123;

        // Since our activity and service are in the same process, we can skip the IBinder and just do a global variable.
        // https://stackoverflow.com/a/608600/3781068
        public static bool IsRunning { get; set; }

        // FusedLocationManager is typically used, but it requires a dependency on Google Play Services.  Since we're
        // really only concerned with GPS, vanilla android works just fine.
        private LocationManager _locationManager;
        private NotificationManager _notificationManager;

        private GpsDatabase _database;

        // used to process location callbacks on a background thread.  In this sample, there's no I/O, but typically
        // saving the location requires saving to disk, which we'd want to do in the background.
        private HandlerThread _backgroundHandlerThread;

        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = (LocationManager) GetSystemService(LocationService);
            _notificationManager = (NotificationManager)GetSystemService(NotificationService);
            _database = GpsDatabase.Instance;
            _backgroundHandlerThread = new HandlerThread(HandlerThreadName);
            _backgroundHandlerThread.Start();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            StartForeground(NotificationId, CreateForegroundNotification());

            // request once per second
            _locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 1000, 0f, this, _backgroundHandlerThread.Looper);
   

            IsRunning = true;

            // restart if killed
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _locationManager.RemoveUpdates(this);
            _backgroundHandlerThread.QuitSafely();

            IsRunning = false;
        }

        private Notification CreateForegroundNotification()
        {
            // create a notification channel on O+.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(NotificationChannelId, GetString(Resource.String.app_name), NotificationImportance.Default);
                _notificationManager.CreateNotificationChannel(channel);
            }

            // launch the main screen when the notification is clicked.
            Intent intent = new Intent(this, typeof(MainActivity));
            var launchActivityIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable);

            return new NotificationCompat.Builder(this, NotificationChannelId)
                .SetSmallIcon(Resource.Mipmap.ic_launcher_foreground)
                .SetContentTitle(GetString(Resource.String.app_name))
                .SetContentText(GetString(Resource.String.app_name))
                .SetContentIntent(launchActivityIntent)
                .SetOngoing(true)
                .Build();
        }

        public override IBinder OnBind(Intent intent)
        {
            // TODO: consider a real IBinder that passes state (e.g., loading, error) back to a bound activity.
            return null;
        }


        public void OnLocationChanged(Location location)
        {
            // we're on a background thread here.  If we're not, no data will show up!  (this code would not actually exist in production)
            if (Thread.CurrentThread.IsBackground)
            {
                // Record all locations, but filter out those we don't want to display in the query.  More data = better.
                _database.InsertLocation(location);
            }
        }

        public void OnProviderDisabled(string provider)
        {
            // TODO: consider showing an error in the notification
        }

        public void OnProviderEnabled(string provider)
        {
            // TODO: revert said error in the notification
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            // this is a no-op on android R+
        }
    }
}

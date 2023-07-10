using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using System.Linq;

namespace GpsApp
{
    /// <summary>
    /// An activity responsible for displaying Locations that were fetched from the <see cref="GpsService"/>.
    /// It also provides UI to enable the user to start and stop that Service, requesting OS permissions if necessary.
    /// </summary>
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, GpsDatabase.IOnChangeListener
    {
        const int RequestPermissionRequestCode = 1;

        // borrowed from React, but the basic idea is that if anything in here changes,
        // we need to re-render / update the views.
        protected struct State
        {
            public List<Location> LocationData;
            // better here is to have a transient loading state, since the service doesn't start and stop
            // immediately
            public bool IsGpsServiceRunning;
        }

        private State _state;

        // Ensure background thread callbacks can run UI code on the main thread.  Android data frameworks usually handle
        // this for us, but because we have our custom GpsDatabase, we get to do it manually.  :(
        private Handler _mainHandler;

        private GpsDatabase _database;

        // bound views
        private Button _toggleGpsButton;
        private View _locationRecyclerEmptyView;
        private RecyclerView _locationRecyclerView;

        private LinearLayoutManager _locationLayoutManager;
        private LocationRecyclerAdapter _locationAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _database = GpsDatabase.Instance;
            _mainHandler = new Handler(Looper.MainLooper);

            SetContentView(Resource.Layout.activity_main);

            _toggleGpsButton = FindViewById<Button>(Resource.Id.toggle_service_button);
            _toggleGpsButton.Click += (sender, args) => ToggleGpsService();

            _locationRecyclerEmptyView = FindViewById(Resource.Id.location_recycler_empty_view);

            _locationRecyclerView = FindViewById<RecyclerView>(Resource.Id.location_recycler_view);
            _locationAdapter = new LocationRecyclerAdapter();
            _locationLayoutManager = new LinearLayoutManager(this);
            _locationRecyclerView.SetAdapter(_locationAdapter);
            _locationRecyclerView.SetLayoutManager(_locationLayoutManager);

        }

        protected override void OnStart()
        {
            base.OnStart();
            _database.AddListener(this);

            // load data in case we missed it while stopped in the background.
            _state = new State { LocationData = _database.GetLocations(), IsGpsServiceRunning = GpsService.IsRunning };
            UpdateViews();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _database.RemoveListener(this);
        }

        /// <summary>
        /// Update all bound views based on the current value of <see cref="State"/>.  Although the approach of
        /// re-binding all views on any change results in more view mutations, it also prevent us from forgetting to apply
        /// state changes.
        /// </summary>
        public void UpdateViews()
        {
            _toggleGpsButton.SetText(_state.IsGpsServiceRunning ? Resource.String.stop_gps_button : Resource.String.start_gps_button);
            _locationAdapter.SetData(_state.LocationData);

            var isEmpty = _state.LocationData.Count == 0;
            _locationRecyclerEmptyView.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
            _locationRecyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;
        }

        public void OnLocationDataChanged()
        {
            _mainHandler.Post(() =>
            {
                _state.LocationData = _database.GetLocations();
                UpdateViews();
            });
        }

        public void ToggleGpsService()
        {
            if (_state.IsGpsServiceRunning)
            {
                StopGpsService();
            }
            else
            {
                StartGpsService();
            }
        }

        public void StopGpsService()
        {
            StopService(new Intent(this, typeof(GpsService)));

            _state.IsGpsServiceRunning = false;
            UpdateViews();
        }

        /// <summary>
        /// Conditionally starts the GPS service.
        /// 
        /// If we don't have the requisite permissions, the service is started after permissions are granted.
        /// </summary>
        public void StartGpsService()
        {
            if (!EnsurePermissionsGranted())
            {

                return;
            }

            ContextCompat.StartForegroundService(this, new Intent(this, typeof(GpsService)));

            _state.IsGpsServiceRunning = true;
            UpdateViews();
        }

        protected bool EnsurePermissionsGranted()
        {
            var requiredPermissions = new List<string>
            {
                Android.Manifest.Permission.AccessFineLocation,
            };

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                // I would've expected compat to just handle this automatically
                requiredPermissions.Add(Android.Manifest.Permission.ForegroundService);
            };

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                // compat is supposed to handle this one; maybe xamarin appcompat not up-to-date.
                // https://android.googlesource.com/platform/frameworks/support/+/androidx-master-dev/core/core/src/main/java/androidx/core/content/ContextCompat.java#598
                requiredPermissions.Add(Android.Manifest.Permission.PostNotifications);
            }

            var hasRequiredPermissions = requiredPermissions.All((permission) => ContextCompat.CheckSelfPermission(this, permission) == Android.Content.PM.Permission.Granted);

            if (hasRequiredPermissions)
            {
                return true;
            }

            // we're missing some permissions, so request them
            // TODO: call ActivityCompat.ShouldShowRequestPermissionRationale and explain to the user why we need it
            ActivityCompat.RequestPermissions(this, requiredPermissions.ToArray(), RequestPermissionRequestCode);
            return false;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch(requestCode)
            {
                case RequestPermissionRequestCode:
                    if (grantResults.Length == 0)
                    {
                        // user cancelled, nothing to do
                        return;
                    }

                    var grantedAllPermissions = grantResults.All((result) => result == Android.Content.PM.Permission.Granted);
                    if (!grantedAllPermissions)
                    {
                        // user declined, let's inform them and point them to the settings screen where they will now have to
                        // enable manually
                        // TODO: use a snackbar / update the layout content
                        return;
                    }

                    // we have permissions!  Fire ze missiles!
                    StartGpsService();
                    break;
            }
        }
    }
}

# GpsApp

![image](https://github.com/michaeltnguyen/GpsApp/assets/14219683/8cdd6a89-ed6d-40ec-bbf1-39a51fe6ce5b)

## Overview
This is a sample android application that demonstrates getting the user's location in a foreground service.  The key files are:

- GpsDatabase: an in-memory data layer for storing and retrieving locations.  In a real app, this would be a SQLite database with much more boilerplate.  Having a separate layer allows the view (activity) and the GPS service to be decoupled from one another and run independently.  (key point: the app can continue collecting pings even when the user is not actively using the app)
- GpsService: a foreground service that's responsible for requesting location updates and storing them in the GpsDatabase.
- MainActivity: a view for the user to view previous locations and to start/stop the gps service.

## Requirements
### Get GPS points and display them in a UI

GPS points were retrieved using the [LocationManager](https://developer.android.com/reference/android/location/LocationManager) class.  Google typically recommends the [FusedLocationProvider](https://developers.google.com/android/reference/com/google/android/gms/location/FusedLocationProviderClient.html), but this requires Google Play Services to be installed.  Also, because buses will likely be reliant on the GPS provider, it seemed reasonable to fetch from that provider specifically.  One improvement may be to consider using other providers.  (The Network Provider, for example, relies on cell towers / wifi access points which are usually less accurate).

The list was built using a RecyclerView.  In retrospect, a TableLayout would likely be a better choice because it inherently supports the idea of a column.  (And it's unlikely that we'd display an endlessly scrolling list of pings to our users).  Still, the RecyclerView is a decent choice because it we don't have to inflate N views for N rows, like we would in a TableLayout.  (TableLayout requires each child row to exist in memory at once)

### GPS code needs to run in a separate thread and in a service

Running in a foreground service is probably the most important part -- it ensures android will try to keep our gps service alive, even if the user is not actively using the app.

The background thread is accomplished with a [HandlerThread](https://developer.android.com/reference/android/os/HandlerThread), which allows location callbacks to be easily processed in a background thread.  A more usual pattern might be to use the (Room ORM)[https://developer.android.com/training/data-storage/room/async-queries] to do async I/O or a [ContentProvider](https://developer.android.com/guide/topics/providers/content-provider-basics).  In those frameworks, the work is dispatched on the main thread, runs in a background thread, and the result is passed back to the main thread.  (so you never actually have to handle threads on your own).

### The app should work for android 4.4 to Android 10

The minSdk is 19 (4.4 KitKat) and the targetSdk is the latest (33).  AppCompat was used to ensure backwards compatibility to KitKat.  The only time a version check was needed is to create the notification channel on android O+.

## Improvements

A lot of complexity in android is around handling edge cases.  Device-specific issues, OS versions, user errors, permission denials, etc.  I left TODOs around the code for some improvements I'd include in a production app.  Some examples are:

- Providing a notification when permissions are denied, and taking the user to the settings app to enable that permission.  (if the user denies twice, android doesn't even show the permissions prompt anymore!)
- Using a bound service to communicate when the gps service has actually started, so we can show a loading indicator.
- Persist the data to disk, and eventually sync that data with a server.
- Integration tests.  Running the GpsServer against a mocked location provider, or the Activity against some stubbed data.  This might require some C# ports of java classes.  (e.g., [ServiceTestRule](https://developer.android.com/reference/android/support/test/rule/ServiceTestRule))
- Labeled headers on the list 🙃


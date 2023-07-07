using Android.Locations;
using System.Collections.Generic;
using System.Linq;

namespace GpsApp
{
    /// <summary>
    /// An in-memory data store for GPS pings.  In a real app, I'd use a SQLite database with the
    /// Room ORM or a ContentProvider, but this allows us to have a separate data layer without the boilerplate.
    /// </summary>
    public class GpsDatabase 
    {
        // If the project has dependency injection, I'd specify that this is a singleton instead of doing this.
        public static readonly GpsDatabase Instance = new GpsDatabase();

        private readonly List<IOnChangeListener> _listeners;

        // In a real app, we'd have a different model (schema) for the persisted data, with an ID, metadata, etc.
        private readonly List<Location> _data;

        private GpsDatabase()
        {
            _listeners = new List<IOnChangeListener>();
            _data = new List<Location>();
        }

        // GetPings is normally a Task<Location[]> but since we're backed by an array, we don't need to worry
        // about doing I/O on the main thread.  Background threading is handled automatically using
        // Room / LoaderManager.
        public List<Location> GetLocations()
        {
            // Using SQLite there's more flexibility in specifying the query.  (i.e., different
            // where clauses for different screens)
            return _data.Where(p => p.Accuracy < 10).ToList();
        }

        // This results in an unbounded Location collection; in a real app we might prune old pings once a month.
        public void InsertLocation(Location ping)
        {
            _data.Add(ping);

            // Using one of the frameworks above, the change notification happens "automagically"
            foreach (var listener in _listeners)
            {
                listener.OnLocationDataChanged();
            }
        }

        public void AddListener(IOnChangeListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IOnChangeListener listener)
        {
            _listeners.Remove(listener);
        }

        public interface IOnChangeListener
        {
            void OnLocationDataChanged();
        }
    }
}

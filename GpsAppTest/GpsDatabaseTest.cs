/*using Android.Locations;
using GpsApp;
using NUnit.Framework;

namespace GpsAppTest
{
    public class Tests
    {
        private GpsDatabase _database = GpsDatabase.Instance;

        private Location _accurateLocation = new Location(LocationManager.GpsProvider) { Accuracy = 9 };
        private Location _inaccurateLoaction = new Location(LocationManager.NetworkProvider) { Accuracy = 1000 };

        [SetUp]
        public void Setup()
        {
            _database.InsertLocation(_accurateLocation);
            _database.InsertLocation(_inaccurateLoaction);
        }

        [Test]
        public void GetLocations_FiltersInaccurateLocations()
        {
            var result = _database.GetLocations();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(_accurateLocation));
        }

        [TearDown]
        public void TearDown()
        {
            _database.Clear();
        }
    }
}*/
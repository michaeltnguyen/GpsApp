using GpsData;

namespace GpsDataTest
{
    public class Tests
    {
        private readonly GpsDatabase _database = GpsDatabase.Instance;

        private readonly GpsLocation _accurateLocation = new(9, 0, 0, 0, 0, 0, "gps");
        private readonly GpsLocation _inaccurateLoaction = new(1000, 0, 0, 0, 0, 0, "gps");

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

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(_accurateLocation));
        }

        [TearDown]
        public void TearDown()
        {
            _database.Clear();
        }
    }
}

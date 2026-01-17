namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FindStoreRequestDTO
    {
        public Coords coords { get; set; } 
        public bool Mocked { get; set; }
        public long Timestamp { get; set; }
    }
    public class Coords
    {
        public int Accuracy { get; set; }
        public int Altitude { get; set; }
        public double AltitudeAccuracy { get; set; }
        public int Heading { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Speed { get; set; }
    }
}


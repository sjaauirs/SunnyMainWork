using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class StoreResponseMockDTO : BaseResponseDto
    {
        public string? Name { get; set; } 
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public List<Metric> Metrics { get; set; } = new List<Metric>();
        public string? TravelTimeValue { get; set; }
        public string? TravelTimeUnit { get; set; }
        public bool IsOpen { get; set; }
        public string? ClosingDescription { get; set; }
        public Location Location { get; set; } = new Location();
        public List<string> Hours { get; set; } = new List<string>();
        public List<string> Benefits { get; set; } = new List<string>();
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Metric
    {
        public string? Value { get; set; }
        public string? Unit { get; set; }
    }

  

}

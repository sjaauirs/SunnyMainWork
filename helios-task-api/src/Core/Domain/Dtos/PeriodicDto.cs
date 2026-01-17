namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class PeriodicDto
    {
        public string? period { get; set; }
        public int periodRestartDate { get; set; }

        private int _maxOccurrences;
        public int MaxOccurrences
        {
            get => _maxOccurrences <= 0 ? 1 : _maxOccurrences;
            set => _maxOccurrences = value;
        }
    }

    public class RecurringDto
    {
        public string? recurrenceType { get; set; }
        public PeriodicDto? periodic { get; set; }
        // Taking ScheduleDto from common core
        public Common.Core.Domain.Dtos.ScheduleDto[]? Schedules { get; set; }
    }
}


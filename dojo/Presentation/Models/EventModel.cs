namespace Presentation.Models
{
    public class EventModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public EventPriority Priority { get; set; }
        public Color Color { get; set; } = Colors.Blue;
        public bool IsCompleted { get; set; }
        public int? TaskId { get; set; }
        
        public TimeSpan Duration => EndDateTime - StartDateTime;
        public int DayOfWeek => (int)StartDateTime.DayOfWeek;
        public int StartHour => StartDateTime.Hour;
        public int StartMinute => StartDateTime.Minute;
    }

    public enum EventPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
}


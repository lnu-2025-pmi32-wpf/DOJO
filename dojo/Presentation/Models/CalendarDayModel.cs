using Microsoft.Maui.Controls;

namespace Presentation.Models
{
    public class CalendarDayModel
    {
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelected { get; set; }
        public bool HasEvents { get; set; }
        public int EventCount { get; set; }

        public string TextColor =>
            IsToday ? "White" :
            !IsCurrentMonth ? "#CCC" :
            "Black";

        public string BackgroundColor =>
            IsToday ? "#FF69B4" :
            IsSelected ? "#FFE4EC" :
            "Transparent";

        public string BorderColor =>
            IsSelected ? "#FF69B4" :
            "Transparent";

        public FontAttributes FontAttributes =>
            IsToday ? FontAttributes.Bold :
            FontAttributes.None;

        public bool ShowEventDot => HasEvents && EventCount > 0;
        public string EventDotColor => "#FF1493";
    }
}

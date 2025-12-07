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

        public string TextColor =>
            IsToday ? "White" :
            !IsCurrentMonth ? "#CCC" :
            "Black";

        public string BackgroundColor =>
            IsToday ? "#4CAF50" :
            IsSelected ? "#E8F5E9" :
            "Transparent";

        public string BorderColor =>
            IsSelected ? "#4CAF50" :
            "Transparent";

        public FontAttributes FontAttributes =>
            IsToday ? FontAttributes.Bold :
            FontAttributes.None;
    }
}


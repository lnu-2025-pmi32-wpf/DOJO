using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Presentation.Controls
{
    public class MonthViewGrid : Grid
    {
        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(nameof(SelectedDate), typeof(DateTime), typeof(MonthViewGrid), DateTime.Today, propertyChanged: OnSelectedDateChanged);

        public static readonly BindableProperty EventsProperty =
            BindableProperty.Create(nameof(Events), typeof(ObservableCollection<EventModel>), typeof(MonthViewGrid), null, propertyChanged: OnEventsChanged);

        public event EventHandler<DateTime>? DayTapped;
        public event EventHandler<EventModel>? EventTapped;

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public ObservableCollection<EventModel> Events
        {
            get => (ObservableCollection<EventModel>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public MonthViewGrid()
        {
            BuildMonthView();
        }

        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MonthViewGrid grid)
            {
                grid.BuildMonthView();
            }
        }

        private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MonthViewGrid grid)
            {
                grid.BuildMonthView();
            }
        }

        private void BuildMonthView()
        {
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            // 7 columns for days of week
            for (int i = 0; i < 7; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Header row + up to 6 weeks
            for (int i = 0; i < 7; i++)
            {
                RowDefinitions.Add(new RowDefinition { Height = i == 0 ? GridLength.Auto : new GridLength(100) });
            }

            // Day headers
            string[] dayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };
            for (int i = 0; i < 7; i++)
            {
                var label = new Label
                {
                    Text = dayNames[i],
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Padding = new Thickness(0, 8)
                };
                Children.Add(label);
                Grid.SetColumn(label, i);
                Grid.SetRow(label, 0);
            }

            // Calculate calendar days
            var firstDayOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            // Find Monday of the week containing the first day
            int daysFromMonday = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            var startDate = firstDayOfMonth.AddDays(-daysFromMonday);

            int row = 1;
            int col = 0;
            var currentDate = startDate;

            // Fill calendar grid
            for (int i = 0; i < 42; i++) // 6 weeks max
            {
                if (row > 6) break;

                var dayBorder = CreateDayCell(currentDate, firstDayOfMonth.Month);
                Children.Add(dayBorder);
                Grid.SetColumn(dayBorder, col);
                Grid.SetRow(dayBorder, row);

                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        private Border CreateDayCell(DateTime date, int currentMonth)
        {
            bool isCurrentMonth = date.Month == currentMonth;
            bool isToday = date.Date == DateTime.Today;
            bool isSelected = date.Date == SelectedDate.Date;

            var border = new Border
            {
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 0.5,
                BackgroundColor = isToday ? Color.FromArgb("#FFF0F5") : Colors.White,
                Padding = new Thickness(8),
                MinimumHeightRequest = 100
            };

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 2
            };

            // Day number
            var dayLabel = new Label
            {
                Text = date.Day.ToString(),
                FontSize = 14,
                FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None,
                TextColor = isCurrentMonth ? (isToday ? Color.FromArgb("#FF69B4") : Color.FromArgb("#333")) : Color.FromArgb("#999"),
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 2, 2, 0)
            };

            stackLayout.Children.Add(dayLabel);

            // Events for this day
            if (Events != null)
            {
                var dayEvents = new List<EventModel>();
                foreach (var evt in Events)
                {
                    if (evt.StartDateTime.Date <= date.Date && evt.EndDateTime.Date >= date.Date)
                    {
                        dayEvents.Add(evt);
                    }
                }

                // Show up to 3 events
                int eventCount = 0;
                foreach (var evt in dayEvents)
                {
                    if (eventCount >= 3) break;

                    var eventBorder = new Border
                    {
                        BackgroundColor = evt.Color ?? Color.FromArgb("#FF69B4"),
                        Padding = new Thickness(4, 2),
                        StrokeShape = new RoundRectangle { CornerRadius = 3 },
                        Margin = new Thickness(0, 1),
                        HeightRequest = 18
                    };

                    var eventLabel = new Label
                    {
                        Text = evt.Title,
                        FontSize = 10,
                        TextColor = Colors.White,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        MaxLines = 1
                    };

                    eventBorder.Content = eventLabel;

                    // Додаємо TapGestureRecognizer для кліку по події
                    var eventTapGesture = new TapGestureRecognizer();
                    var eventToCapture = evt; // Capture for closure
                    eventTapGesture.Tapped += (s, e) =>
                    {
                        EventTapped?.Invoke(this, eventToCapture);
                    };
                    eventBorder.GestureRecognizers.Add(eventTapGesture);

                    stackLayout.Children.Add(eventBorder);
                    eventCount++;
                }

                // Show "+X more" if there are more events
                if (dayEvents.Count > 3)
                {
                    var moreLabel = new Label
                    {
                        Text = $"+{dayEvents.Count - 3} більше",
                        FontSize = 9,
                        TextColor = Color.FromArgb("#666"),
                        Margin = new Thickness(4, 2, 0, 0)
                    };
                    stackLayout.Children.Add(moreLabel);
                }
            }

            border.Content = stackLayout;

            // Add tap gesture
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                SelectedDate = date;
                DayTapped?.Invoke(this, date);
            };
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }
    }
}

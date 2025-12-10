using Presentation.Models;
using System.Collections.ObjectModel;

namespace Presentation.Controls
{
    public class WeekScheduleGrid : ContentView
    {
        private Grid _mainGrid = null!;
        private readonly string[] _daysOfWeek = { "Неділя", "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця", "Субота" };
        private readonly int _startHour = 0;
        private readonly int _endHour = 24;
        private readonly int _hourHeight = 60;

        public event EventHandler<DateTime>? DayTapped;
        public event EventHandler<EventModel>? EventTapped;

        public static readonly BindableProperty EventsProperty =
            BindableProperty.Create(nameof(Events), typeof(ObservableCollection<EventModel>), typeof(WeekScheduleGrid), 
                new ObservableCollection<EventModel>(), propertyChanged: OnEventsChanged);

        public static readonly BindableProperty WeekStartDateProperty =
            BindableProperty.Create(nameof(WeekStartDate), typeof(DateTime), typeof(WeekScheduleGrid), 
                DateTime.Today, propertyChanged: OnWeekStartDateChanged);

        public ObservableCollection<EventModel> Events
        {
            get => (ObservableCollection<EventModel>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public DateTime WeekStartDate
        {
            get => (DateTime)GetValue(WeekStartDateProperty);
            set => SetValue(WeekStartDateProperty, value);
        }

        public WeekScheduleGrid()
        {
            CreateScheduleGrid();
        }

        private void CreateScheduleGrid()
        {
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical
            };

            _mainGrid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 1,
                BackgroundColor = Colors.LightGray,
                Padding = 0
            };

            // Define columns: Time label + 7 days
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });
            for (int i = 0; i < 7; i++)
            {
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Define rows: Header + hours
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = 50 }); // Header
            for (int hour = _startHour; hour <= _endHour; hour++)
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition { Height = _hourHeight });
            }

            // Create header
            CreateHeader();

            // Create time labels and grid cells
            CreateTimeLabelsAndCells();

            scrollView.Content = _mainGrid;
            Content = scrollView;
        }

        private void CreateHeader()
        {
            // Top-left corner (empty)
            var cornerBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#F5F5F5"),
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1
            };
            Grid.SetRow(cornerBorder, 0);
            Grid.SetColumn(cornerBorder, 0);
            _mainGrid.Children.Add(cornerBorder);

            // Day headers
            for (int day = 0; day < 7; day++)
            {
                var dayDate = WeekStartDate.AddDays(day);
                var headerBorder = new Border
                {
                    BackgroundColor = Color.FromArgb("#FF69B4"),
                    Stroke = Color.FromArgb("#E0E0E0"),
                    StrokeThickness = 1,
                    Padding = new Thickness(10, 5)
                };

                var headerStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                headerStack.Children.Add(new Label
                {
                    Text = _daysOfWeek[day],
                    FontSize = 12, // більший розмір
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                });

                headerStack.Children.Add(new Label
                {
                    Text = dayDate.Day.ToString(),
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                });

                headerBorder.Content = headerStack;

                // Add tap gesture to day header
                var headerTapGesture = new TapGestureRecognizer();
                int capturedDay = day;
                headerTapGesture.Tapped += (s, e) => DayTapped?.Invoke(this, WeekStartDate.AddDays(capturedDay));
                headerBorder.GestureRecognizers.Add(headerTapGesture);

                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, day + 1);
                _mainGrid.Children.Add(headerBorder);
            }
        }

        private void CreateTimeLabelsAndCells()
        {
            for (int hour = _startHour; hour < _endHour; hour++)
            {
                int rowIndex = hour - _startHour + 1;

                // Time label
                var timeBorder = new Border
                {
                    BackgroundColor = Color.FromArgb("#F9F9F9"),
                    Stroke = Color.FromArgb("#E0E0E0"),
                    StrokeThickness = 1,
                    Padding = new Thickness(10, 5)
                };

                timeBorder.Content = new Label
                {
                    Text = $"{hour:D2}:00",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#666666"),
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End
                };

                Grid.SetRow(timeBorder, rowIndex);
                Grid.SetColumn(timeBorder, 0);
                _mainGrid.Children.Add(timeBorder);

                // Day cells
                for (int day = 0; day < 7; day++)
                {
                    var cellBorder = new Border
                    {
                        BackgroundColor = hour % 2 == 0 ? Colors.White : Color.FromArgb("#FAFAFA"),
                        Stroke = Color.FromArgb("#E0E0E0"),
                        StrokeThickness = 1
                    };

                    // Add tap gesture for adding events
                    var tapGesture = new TapGestureRecognizer();
                    int capturedDay = day;
                    int capturedHour = hour;
                    tapGesture.Tapped += (s, e) => OnCellTapped(capturedDay, capturedHour);
                    cellBorder.GestureRecognizers.Add(tapGesture);

                    Grid.SetRow(cellBorder, rowIndex);
                    Grid.SetColumn(cellBorder, day + 1);
                    _mainGrid.Children.Add(cellBorder);
                }
            }
        }

        private void RenderEvents()
        {
            if (Events == null || _mainGrid == null) return;

            // Remove existing event views
            var eventsToRemove = _mainGrid.Children
                .Where(c => c is Border border && border.StyleId == "EventBlock")
                .ToList();
            
            foreach (var eventView in eventsToRemove)
            {
                _mainGrid.Children.Remove(eventView);
            }

            // Add new event views
            foreach (var evt in Events)
            {
                // Показуємо на даті ПОЧАТКУ
                if (evt.StartDateTime.Date >= WeekStartDate && evt.StartDateTime.Date < WeekStartDate.AddDays(7))
                {
                    var dayOffset = (evt.StartDateTime.Date - WeekStartDate.Date).Days;
                    var startRow = evt.StartHour - _startHour + 1;
                    
                    var startBorder = CreateEventBorder(evt, isStart: true);
                    
                    Grid.SetRow(startBorder, startRow);
                    Grid.SetColumn(startBorder, dayOffset + 1);
                    _mainGrid.Children.Add(startBorder);
                }
                
                // Показуємо на даті ЗАВЕРШЕННЯ (якщо це інший день)
                if (evt.EndDateTime.Date != evt.StartDateTime.Date && 
                    evt.EndDateTime.Date >= WeekStartDate && 
                    evt.EndDateTime.Date < WeekStartDate.AddDays(7))
                {
                    var dayOffset = (evt.EndDateTime.Date - WeekStartDate.Date).Days;
                    var endRow = evt.EndDateTime.Hour - _startHour + 1;
                    
                    var endBorder = CreateEventBorder(evt, isStart: false);
                    
                    Grid.SetRow(endBorder, endRow);
                    Grid.SetColumn(endBorder, dayOffset + 1);
                    _mainGrid.Children.Add(endBorder);
                }
            }
        }
        
        private Border CreateEventBorder(EventModel evt, bool isStart)
        {
            var eventBorder = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = isStart ? Color.FromArgb("#FF69B4") : Color.FromArgb("#E53935"),
                StrokeThickness = 1,
                Padding = new Thickness(4, 2),
                StyleId = "EventBlock",
                Margin = new Thickness(2),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 }
            };

            var eventStack = new VerticalStackLayout
            {
                Spacing = 1
            };

            // Назва плану
            eventStack.Children.Add(new Label
            {
                Text = evt.Title,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333333"),
                LineBreakMode = LineBreakMode.TailTruncation
            });

            if (isStart)
            {
                // На даті початку - показуємо дедлайн
                eventStack.Children.Add(new Label
                {
                    Text = $"⏰ до {evt.EndDateTime:dd.MM HH:mm}",
                    FontSize = 9,
                    TextColor = Color.FromArgb("#E53935"),
                    FontAttributes = FontAttributes.Bold
                });
            }
            else
            {
                // На даті завершення - показуємо що це кінець
                eventStack.Children.Add(new Label
                {
                    Text = $"🏁 Завершення",
                    FontSize = 9,
                    TextColor = Color.FromArgb("#E53935"),
                    FontAttributes = FontAttributes.Bold
                });
            }

            eventBorder.Content = eventStack;

            // Додаємо TapGestureRecognizer для кліку по події
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => EventTapped?.Invoke(this, evt);
            eventBorder.GestureRecognizers.Add(tapGesture);

            return eventBorder;
        }

        private void OnCellTapped(int day, int hour)
        {
            var selectedDate = WeekStartDate.AddDays(day).AddHours(hour);
            DayTapped?.Invoke(this, selectedDate.Date);
        }

        private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is WeekScheduleGrid grid)
            {
                if (oldValue is ObservableCollection<EventModel> oldCollection)
                {
                    oldCollection.CollectionChanged -= grid.OnEventsCollectionChanged;
                }

                if (newValue is ObservableCollection<EventModel> newCollection)
                {
                    newCollection.CollectionChanged += grid.OnEventsCollectionChanged;
                }

                grid.RenderEvents();
            }
        }

        private void OnEventsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RenderEvents();
        }

        private static void OnWeekStartDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is WeekScheduleGrid grid)
            {
                grid.CreateScheduleGrid();
                grid.RenderEvents();
            }
        }
    }
}

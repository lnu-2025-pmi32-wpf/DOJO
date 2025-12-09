using Presentation.Models;
using System. Collections.ObjectModel;

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
                null, propertyChanged: OnEventsChanged);

        public static readonly BindableProperty WeekStartDateProperty =
            BindableProperty.Create(nameof(WeekStartDate), typeof(DateTime), typeof(WeekScheduleGrid), 
                DateTime.Today, propertyChanged:  OnWeekStartDateChanged);

        public ObservableCollection<EventModel>? Events
        {
            get => (ObservableCollection<EventModel>?)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public DateTime WeekStartDate
        {
            get => (DateTime)GetValue(WeekStartDateProperty);
            set => SetValue(WeekStartDateProperty, value);
        }

        public WeekScheduleGrid()
        {
            try
            {
                CreateScheduleGrid();
            }
            catch (Exception ex)
            {
                System.Diagnostics. Debug.WriteLine($"❌ WeekScheduleGrid constructor error: {ex. Message}");
            }
        }

        private void CreateScheduleGrid()
        {
            try
            {
                var scrollView = new ScrollView
                {
                    Orientation = ScrollOrientation.Vertical
                };

                _mainGrid = new Grid
                {
                    RowSpacing = 0,
                    ColumnSpacing = 1,
                    BackgroundColor = Colors. LightGray,
                    Padding = 0
                };

                // Define columns:  Time label + 7 days
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });
                for (int i = 0; i < 7; i++)
                {
                    _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType. Star) });
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
            catch (Exception ex)
            {
                System.Diagnostics. Debug.WriteLine($"❌ CreateScheduleGrid error:  {ex.Message}");
            }
        }

        private void CreateHeader()
        {
            try
            {
                // Top-left corner (empty)
                var cornerBorder = new Border
                {
                    BackgroundColor = Color.FromArgb("#F5F5F5"),
                    Stroke = Color.FromArgb("#E0E0E0"),
                    StrokeThickness = 1
                };
                Grid.SetRow(cornerBorder, 0);
                Grid. SetColumn(cornerBorder, 0);
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
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    });

                    headerStack.Children. Add(new Label
                    {
                        Text = dayDate.Day.ToString(),
                        FontSize = 18,
                        FontAttributes = FontAttributes. Bold,
                        TextColor = Colors. White,
                        HorizontalOptions = LayoutOptions.Center
                    });

                    headerBorder.Content = headerStack;

                    // Add tap gesture to day header
                    var headerTapGesture = new TapGestureRecognizer();
                    int capturedDay = day;
                    headerTapGesture. Tapped += (s, e) => 
                    {
                        try
                        {
                            DayTapped?. Invoke(this, WeekStartDate.AddDays(capturedDay));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics. Debug.WriteLine($"❌ HeaderTap error: {ex. Message}");
                        }
                    };
                    headerBorder.GestureRecognizers. Add(headerTapGesture);

                    Grid.SetRow(headerBorder, 0);
                    Grid.SetColumn(headerBorder, day + 1);
                    _mainGrid. Children.Add(headerBorder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateHeader error: {ex.Message}");
            }
        }

        private void CreateTimeLabelsAndCells()
        {
            try
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
                        tapGesture. Tapped += (s, e) => OnCellTapped(capturedDay, capturedHour);
                        cellBorder.GestureRecognizers.Add(tapGesture);

                        Grid. SetRow(cellBorder, rowIndex);
                        Grid. SetColumn(cellBorder, day + 1);
                        _mainGrid.Children.Add(cellBorder);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics. Debug.WriteLine($"❌ CreateTimeLabelsAndCells error: {ex. Message}");
            }
        }

        private void RenderEvents()
        {
            try
            {
                if (Events == null || _mainGrid == null) return;

                // ✅ ВИПРАВЛЕНО: Безпечне видалення - створюємо копію списку
                var eventsToRemove = _mainGrid.Children
                    .OfType<Border>()
                    .Where(b => b. StyleId == "EventBlock")
                    . ToList();
                
                foreach (var eventView in eventsToRemove)
                {
                    _mainGrid.Children.Remove(eventView);
                }

                // Add new event views
                foreach (var evt in Events)
                {
                    try
                    {
                        if (evt.StartDateTime.Date < WeekStartDate || evt.StartDateTime. Date >= WeekStartDate.AddDays(7))
                            continue;

                        var dayOffset = (evt. StartDateTime.Date - WeekStartDate. Date).Days;
                        var startRow = evt.StartHour - _startHour + 1;
                        var durationHours = evt.Duration.TotalHours;
                        
                        // ✅ Перевірка валідності індексів
                        if (startRow < 1 || startRow > _endHour - _startHour + 1)
                        {
                            System.Diagnostics. Debug.WriteLine($"⚠️ Invalid startRow: {startRow} for event {evt.Title}");
                            continue;
                        }
                        if (dayOffset < 0 || dayOffset > 6)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Invalid dayOffset:  {dayOffset} for event {evt.Title}");
                            continue;
                        }
                        
                        var eventBorder = new Border
                        {
                            BackgroundColor = evt. Color.WithAlpha(0.3f),
                            Stroke = evt.Color,
                            StrokeThickness = 2,
                            Padding = new Thickness(5),
                            StyleId = "EventBlock",
                            Margin = new Thickness(2)
                        };

                        var eventStack = new VerticalStackLayout
                        {
                            Spacing = 2
                        };

                        eventStack.Children.Add(new Label
                        {
                            Text = evt.Title ??  "Без назви",
                            FontSize = 12,
                            FontAttributes = FontAttributes. Bold,
                            TextColor = Color. FromArgb("#333333"),
                            LineBreakMode = LineBreakMode. TailTruncation
                        });

                        eventStack. Children.Add(new Label
                        {
                            Text = $"{evt.StartDateTime:HH: mm} - {evt.EndDateTime:HH:mm}",
                            FontSize = 10,
                            TextColor = Color.FromArgb("#666666")
                        });

                        eventBorder. Content = eventStack;

                        // ✅ TapGestureRecognizer для кліку по події
                        var tapGesture = new TapGestureRecognizer();
                        var capturedEvent = evt;
                        tapGesture. Tapped += (s, e) => 
                        {
                            try
                            {
                                EventTapped?. Invoke(this, capturedEvent);
                            }
                            catch (Exception ex)
                            {
                                System. Diagnostics.Debug. WriteLine($"❌ EventTapped error: {ex.Message}");
                            }
                        };
                        eventBorder.GestureRecognizers.Add(tapGesture);

                        // ❌ ВИДАЛЕНО: MenuFlyout - він крашить на Windows! 
                        // Замість контекстного меню використовуємо EventTapped подію
                        // і обробляємо її в DashboardPage через DisplayActionSheet

                        Grid.SetRow(eventBorder, startRow);
                        Grid.SetColumn(eventBorder, dayOffset + 1);
                        Grid.SetRowSpan(eventBorder, Math. Max(1, (int)(durationHours)));
                        
                        _mainGrid.Children.Add(eventBorder);
                    }
                    catch (Exception eventEx)
                    {
                        System.Diagnostics. Debug.WriteLine($"❌ RenderEvents single event error: {eventEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug. WriteLine($"❌ RenderEvents error: {ex.Message}");
            }
        }

        private void OnCellTapped(int day, int hour)
        {
            try
            {
                var selectedDate = WeekStartDate.AddDays(day).AddHours(hour);
                DayTapped?.Invoke(this, selectedDate. Date);
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug. WriteLine($"❌ OnCellTapped error: {ex.Message}");
            }
        }

        private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            try
            {
                if (bindable is WeekScheduleGrid grid)
                {
                    if (oldValue is ObservableCollection<EventModel> oldCollection)
                    {
                        oldCollection.CollectionChanged -= grid. OnEventsCollectionChanged;
                    }

                    if (newValue is ObservableCollection<EventModel> newCollection)
                    {
                        newCollection. CollectionChanged += grid.OnEventsCollectionChanged;
                    }

                    grid.RenderEvents();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics. Debug.WriteLine($"❌ OnEventsChanged error: {ex.Message}");
            }
        }

        private void OnEventsCollectionChanged(object? sender, System.Collections. Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // ✅ ВИПРАВЛЕНО:  Виконуємо в UI потоці
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        RenderEvents();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ RenderEvents in MainThread error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug. WriteLine($"❌ OnEventsCollectionChanged error: {ex.Message}");
            }
        }

        private static void OnWeekStartDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            try
            {
                if (bindable is WeekScheduleGrid grid)
                {
                    grid.CreateScheduleGrid();
                    grid.RenderEvents();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnWeekStartDateChanged error: {ex.Message}");
            }
        }
    }
}
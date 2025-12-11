using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Presentation.Models;

namespace Presentation.Controls
{
    public class DayScheduleGrid : Grid
    {
        public static readonly BindableProperty EventsProperty =
            BindableProperty.Create(nameof(Events), typeof(ObservableCollection<EventModel>), typeof(DayScheduleGrid), null, propertyChanged: OnEventsChanged);

        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(nameof(SelectedDate), typeof(DateTime), typeof(DayScheduleGrid), DateTime.Today, propertyChanged: OnSelectedDateChanged);

        public event EventHandler<EventModel>? EventTapped;

        public ObservableCollection<EventModel>? Events
        {
            get => (ObservableCollection<EventModel>?)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public DayScheduleGrid()
        {
            BuildGrid();
        }

        private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DayScheduleGrid grid)
            {
                // Відписуємося від старої колекції
                if (oldValue is ObservableCollection<EventModel> oldCollection)
                {
                    oldCollection.CollectionChanged -= grid.OnEventsCollectionChanged;
                }

                // Підписуємося на нову колекцію
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

        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DayScheduleGrid grid)
            {
                grid.RenderEvents();
            }
        }

        private void BuildGrid()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            // Колонка для часу
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Absolute) });
            // Колонка для подій
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 24 години
            for (int i = 0; i < 24; i++)
            {
                RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Absolute) });
            }

            // Малюємо сітку часу
            for (int hour = 0; hour < 24; hour++)
            {
                // Час
                var timeLabel = new Label
                {
                    Text = $"{hour:D2}:00",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#666666"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                this.Add(timeLabel);
                Grid.SetRow(timeLabel, hour);
                Grid.SetColumn(timeLabel, 0);

                // Лінія
                var line = new BoxView
                {
                    Color = Color.FromArgb("#E0E0E0"),
                    HeightRequest = 1,
                    VerticalOptions = LayoutOptions.Start
                };
                this.Add(line);
                Grid.SetRow(line, hour);
                Grid.SetColumn(line, 1);
            }

            RenderEvents();
        }

        private void RenderEvents()
        {
            // Видаляємо всі попередні події (залишаємо тільки сітку)
            var eventViews = Children.Where(c => c is Border).ToList();
            foreach (var view in eventViews)
            {
                Children.Remove(view);
            }

            if (Events == null) return;

            // Відображаємо події для вибраного дня
            var dayEvents = Events.Where(e => e.StartDateTime.Date == SelectedDate.Date).ToList();

            foreach (var evt in dayEvents)
            {
                var startHour = evt.StartDateTime.Hour;
                var startMinute = evt.StartDateTime.Minute;
                var endHour = evt.EndDateTime.Hour;
                var endMinute = evt.EndDateTime.Minute;

                // Розраховуємо позицію та висоту
                var startRow = startHour;
                var duration = (evt.EndDateTime - evt.StartDateTime).TotalHours;
                var height = duration * 60; // 60 пікселів на годину

                // Відступ зверху в межах години
                var topMargin = (startMinute / 60.0) * 60;

                var eventBorder = new Border
                {
                    BackgroundColor = evt.Color ?? Colors.LightBlue,
                    Stroke = evt.Color ?? Colors.Blue,
                    StrokeThickness = 2,
                    StrokeShape = new RoundRectangle { CornerRadius = 5 },
                    Padding = new Thickness(8, 4),
                    Margin = new Thickness(5, topMargin, 5, 0),
                    HeightRequest = height - topMargin,
                    VerticalOptions = LayoutOptions.Start,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new Label
                            {
                                Text = evt.Title,
                                FontSize = 14,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Colors.White
                            },
                            new Label
                            {
                                Text = $"{evt.StartDateTime:HH:mm} - {evt.EndDateTime:HH:mm}",
                                FontSize = 11,
                                TextColor = Colors.White,
                                Opacity = 0.9
                            },
                            new Label
                            {
                                Text = evt.Description,
                                FontSize = 11,
                                TextColor = Colors.White,
                                Opacity = 0.8,
                                MaxLines = 2,
                                LineBreakMode = LineBreakMode.TailTruncation
                            }
                        }
                    }
                };

                // Додаємо TapGestureRecognizer для кліку по події
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => EventTapped?.Invoke(this, evt);
                eventBorder.GestureRecognizers.Add(tapGesture);

                this.Add(eventBorder);
                Grid.SetRow(eventBorder, startRow);
                Grid.SetColumn(eventBorder, 1);
            }
        }
    }
}

using System;
using Microsoft.Maui.Controls;

namespace Presentation.Views
{
    public partial class LevelUpPopup : ContentView
    {
        private TaskCompletionSource<bool>? _taskCompletionSource;

        public LevelUpPopup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Показує popup з анімацією
        /// </summary>
        public async Task ShowAsync(int newLevel, int expGained)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();

            // Встановлюємо текст
            LevelLabel.Text = $"Ви досягли {newLevel} рівня!";
            DescriptionLabel.Text = $"+{expGained} досвіду";

            // Показуємо overlay
            this.IsVisible = true;

            // Анімація появи
            await AnimateShowAsync();

            // Чекаємо поки користувач закриє
            await _taskCompletionSource.Task;
        }

        private async Task AnimateShowAsync()
        {
            // Паралельні анімації для overlay та картки
            var overlayFade = OverlayGrid.FadeTo(1, 300, Easing.CubicOut);

            await Task.Delay(100);

            // Анімація появи картки (bounce effect)
            var cardScale = PopupCard.ScaleTo(1, 400, Easing.SpringOut);
            var cardFade = PopupCard.FadeTo(1, 300, Easing.CubicOut);

            await Task.WhenAll(overlayFade, cardScale, cardFade);

            // Послідовна анімація елементів всередині
            await AnimateElementsAsync();

            // Анімація конфетті
            AnimateConfetti();
        }

        private async Task AnimateElementsAsync()
        {
            // Зірочки з bounce
            StarsLabel.Opacity = 1;
            await StarsLabel.ScaleTo(1.3, 200, Easing.CubicOut);
            await StarsLabel.ScaleTo(1, 150, Easing.CubicIn);

            // Свинка з bounce
            var pigScale = PigImage.ScaleTo(1, 400, Easing.SpringOut);
            await Task.Delay(100);

            // Заголовок
            await TitleLabel.FadeTo(1, 200, Easing.CubicOut);

            await pigScale;

            // Рівень
            await LevelLabel.FadeTo(1, 200, Easing.CubicOut);

            // Опис
            await DescriptionLabel.FadeTo(1, 200, Easing.CubicOut);

            // Кнопка з bounce
            CloseButton.Opacity = 1;
            await CloseButton.ScaleTo(1, 300, Easing.SpringOut);

            // Пульсуюча анімація свинки
            StartPigPulseAnimation();
        }

        private void StartPigPulseAnimation()
        {
            // Нескінченна пульсуюча анімація для свинки
            this.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1500), () =>
            {
                if (!this.IsVisible) return false;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await PigImage.ScaleTo(1.1, 300, Easing.CubicOut);
                    await PigImage.ScaleTo(1, 300, Easing.CubicIn);
                });

                return this.IsVisible;
            });
        }

        private void AnimateConfetti()
        {
            // Анімація конфетті елементів
            AnimateSingleConfetti(Confetti1, -50, -100);
            AnimateSingleConfetti(Confetti2, 50, -80);
            AnimateSingleConfetti(Confetti3, -30, 60);
            AnimateSingleConfetti(Confetti4, 40, 50);
        }

        private async void AnimateSingleConfetti(Label confetti, double xOffset, double yOffset)
        {
            confetti.Opacity = 0;
            confetti.TranslationX = 0;
            confetti.TranslationY = 0;
            confetti.Scale = 0;

            await Task.Delay(new Random().Next(100, 500));

            confetti.Opacity = 1;

            var moveX = confetti.TranslateTo(xOffset, yOffset, 1000, Easing.CubicOut);
            var scale = confetti.ScaleTo(1.5, 500, Easing.CubicOut);
            var rotate = confetti.RotateTo(360, 1000, Easing.Linear);

            await Task.WhenAll(moveX, scale, rotate);

            await confetti.FadeTo(0, 500, Easing.CubicIn);
        }

        private async Task AnimateHideAsync()
        {
            // Анімація зникнення
            var cardScale = PopupCard.ScaleTo(0.8, 200, Easing.CubicIn);
            var cardFade = PopupCard.FadeTo(0, 200, Easing.CubicIn);
            var overlayFade = OverlayGrid.FadeTo(0, 300, Easing.CubicIn);

            await Task.WhenAll(cardScale, cardFade, overlayFade);

            this.IsVisible = false;

            // Скидаємо стан для наступного показу
            ResetState();
        }

        private void ResetState()
        {
            OverlayGrid.Opacity = 0;
            PopupCard.Scale = 0;
            PopupCard.Opacity = 0;
            StarsLabel.Opacity = 0;
            PigImage.Scale = 0;
            TitleLabel.Opacity = 0;
            LevelLabel.Opacity = 0;
            DescriptionLabel.Opacity = 0;
            CloseButton.Opacity = 0;
            CloseButton.Scale = 0;

            Confetti1.Opacity = 0;
            Confetti2.Opacity = 0;
            Confetti3.Opacity = 0;
            Confetti4.Opacity = 0;
        }

        private async void OnCloseClicked(object? sender, EventArgs e)
        {
            await AnimateHideAsync();
            _taskCompletionSource?.TrySetResult(true);
        }

        private async void OnBackgroundTapped(object? sender, TappedEventArgs e)
        {
            // Закриття при натисканні на фон
            await AnimateHideAsync();
            _taskCompletionSource?.TrySetResult(true);
        }
    }
}

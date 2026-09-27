namespace Meow.Views;

public partial class VotePage : ContentPage
{
    private readonly VoteViewModel vm;
    private bool _initialized;

    private int _animationId;

    public VotePage(VoteViewModel voteViewModel)
    {
        InitializeComponent();
        vm = voteViewModel;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        vm.TransitionRequested -= OnTransitionRequested;
        vm.TransitionRequested += OnTransitionRequested;
        vm.TransitionCompleted -= OnTransitionCompleted;
        vm.TransitionCompleted += OnTransitionCompleted;
        vm.PreloadImageRequested -= OnPreloadImageRequested;
        vm.PreloadImageRequested += OnPreloadImageRequested;
        vm.HeartAnimationRequested -= OnHeartAnimationRequested;
        vm.HeartAnimationRequested += OnHeartAnimationRequested;

        if (!_initialized)
        {
            _initialized = true;
            CatCardBorder.Opacity = 0;
            await vm.InitializeDataAsync(forceRefresh: true, keepHiddenUntilCallerShows: true);
            if (!vm.HasError && vm.CurrentCat != null)
            {
                await WaitForCatImageLoadedAsync();
            }
            vm.IsHidden = false;
            await CatCardBorder.FadeToAsync(1, 300, Easing.CubicOut);
        }
        else
        {
            CatCardBorder.Opacity = 1;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        vm.TransitionRequested -= OnTransitionRequested;
        vm.TransitionCompleted -= OnTransitionCompleted;
        vm.PreloadImageRequested -= OnPreloadImageRequested;
        vm.HeartAnimationRequested -= OnHeartAnimationRequested;

        _animationId++;
        if (HeartConfettiCanvas != null)
        {
            HeartConfettiCanvas.Children.Clear();
            HeartConfettiCanvas.IsVisible = false;
        }
    }

    private async Task OnPreloadImageRequested(string? url)
    {
        await WaitForCatImageLoadedAsync();
    }

    private async Task OnHeartAnimationRequested()
    {
        await PlayHeartConfettiAnimationAsync();
    }

    /// <summary>
    /// Waits for CatImage to complete downloading so there is no blank card flicker.
    /// </summary>
    private async Task WaitForCatImageLoadedAsync(int timeoutMs = 600)
    {
        if (CatImage == null) return;

        if (!CatImage.IsLoading)
        {
            await Task.Delay(100);
        }

        if (CatImage.IsLoading)
        {
            var tcs = new TaskCompletionSource<bool>();
            void PropertyChangedHandler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Microsoft.Maui.Controls.Image.IsLoading) && !CatImage.IsLoading)
                {
                    CatImage.PropertyChanged -= PropertyChangedHandler;
                    tcs.TrySetResult(true);
                }
            }

            CatImage.PropertyChanged += PropertyChangedHandler;
            await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
            CatImage.PropertyChanged -= PropertyChangedHandler;
        }
    }

    /// <summary>
    /// Plays a custom native MAUI confetti animation of red hearts bursting from center.
    /// </summary>
    private async Task PlayHeartConfettiAnimationAsync()
    {
        if (HeartConfettiCanvas == null) return;

        var currentAnimation = ++_animationId;

        HeartConfettiCanvas.Children.Clear();
        HeartConfettiCanvas.IsVisible = true;
        HeartConfettiCanvas.Opacity = 1;

        var mainHeart = new Label
        {
            Text = "❤️",
            FontSize = 56,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Scale = 0,
            Opacity = 0
        };
        HeartConfettiCanvas.Children.Add(mainHeart);

        var particles = new List<Label>();
        string[] heartSymbols = ["❤️", "💖", "💕", "💗", "💓", "💘", "♥"];
        Color[] heartColors =
        [
            Color.FromArgb("#FF1744"),
            Color.FromArgb("#FF2D55"),
            Color.FromArgb("#E0245E"),
            Color.FromArgb("#FF3B30"),
            Color.FromArgb("#FA2D48"),
            Color.FromArgb("#FF5252"),
            Color.FromArgb("#FF6B81")
        ];

        int particleCount = 24;
        var rnd = Random.Shared;

        for (int i = 0; i < particleCount; i++)
        {
            var symbol = heartSymbols[rnd.Next(heartSymbols.Length)];
            var color = heartColors[rnd.Next(heartColors.Length)];
            var particle = new Label
            {
                Text = symbol,
                TextColor = color,
                FontSize = rnd.Next(16, 28),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Scale = 0,
                Opacity = 0,
                TranslationX = 0,
                TranslationY = 0
            };
            HeartConfettiCanvas.Children.Add(particle);
            particles.Add(particle);
        }

        async Task AnimateMainHeartAsync()
        {
            try
            {
                await Task.WhenAll(
                    mainHeart.FadeToAsync(1, 150),
                    mainHeart.ScaleToAsync(1.3, 250, Easing.SpringOut)
                );
                await Task.Delay(100);
                await Task.WhenAll(
                    mainHeart.ScaleToAsync(1.6, 300, Easing.CubicIn),
                    mainHeart.FadeToAsync(0, 300, Easing.CubicIn)
                );
            }
            catch
            {
                // Ignore animation cancellations if view is detached
            }
        }

        async Task AnimateParticleAsync(Label p, int index)
        {
            try
            {
                double angle = (2 * Math.PI / particleCount) * index + (rnd.NextDouble() - 0.5) * 0.5;
                double burstDist = rnd.Next(70, 160);
                double burstX = Math.Cos(angle) * burstDist;
                double burstY = Math.Sin(angle) * burstDist - rnd.Next(20, 60);
                double driftY = burstY + rnd.Next(40, 100);
                double driftX = burstX + rnd.Next(-30, 30);
                double rotation = rnd.Next(-180, 180);
                uint burstDuration = (uint)rnd.Next(300, 450);
                uint driftDuration = (uint)rnd.Next(400, 600);

                await Task.WhenAll(
                    p.FadeToAsync(1, burstDuration / 2),
                    p.ScaleToAsync(rnd.NextDouble() * 0.5 + 0.8, burstDuration, Easing.CubicOut),
                    p.TranslateToAsync(burstX, burstY, burstDuration, Easing.CubicOut),
                    p.RotateToAsync(rotation, burstDuration, Easing.CubicOut)
                );

                await Task.WhenAll(
                    p.TranslateToAsync(driftX, driftY, driftDuration, Easing.SinIn),
                    p.FadeToAsync(0, driftDuration, Easing.CubicIn),
                    p.ScaleToAsync(0.3, driftDuration, Easing.CubicIn),
                    p.RotateToAsync(rotation + rnd.Next(-90, 90), driftDuration, Easing.Linear)
                );
            }
            catch
            {
                // Ignore animation cancellations if view is detached
            }
        }

        var tasks = new List<Task> { AnimateMainHeartAsync() };
        for (int i = 0; i < particles.Count; i++)
        {
            tasks.Add(AnimateParticleAsync(particles[i], i));
        }

        await Task.WhenAll(tasks);

        if (currentAnimation == _animationId)
        {
            HeartConfettiCanvas.Children.Clear();
            HeartConfettiCanvas.IsVisible = false;
            vm.IsAnimation = false;
        }
    }

    /// <summary>
    /// Fades the card out before new data loads.
    /// </summary>
    private async Task OnTransitionRequested()
    {
        await CatCardBorder.FadeToAsync(0, 250, Easing.CubicIn);
    }

    /// <summary>
    /// Fades the card back in after new data has loaded.
    /// </summary>
    private async Task OnTransitionCompleted()
    {
        await CatCardBorder.FadeToAsync(1, 300, Easing.CubicOut);
    }

    private void OnDismissWarning(object? sender, TappedEventArgs e)
    {
        vm.WarningMessage = null;
    }
}
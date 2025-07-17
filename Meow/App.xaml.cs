namespace Meow;

public partial class App : Application
{
    private BackgroundSyncService _backgroundSyncService;

    public App(BackgroundSyncService backgroundSyncService)
    {
        InitializeComponent();
        _backgroundSyncService = backgroundSyncService;
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        // Start background sync service when app starts
        _backgroundSyncService?.Start();

        var window = new Window(new AppShell());
        
        // Force portrait orientation
        window.Created += (s, e) =>
        {
#if ANDROID
            if (Platform.CurrentActivity != null)
            {
                Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }
#endif
        };

        return window;
    }

    protected override void CleanUp()
    {
        // Stop background sync service when app closes
        _backgroundSyncService?.Stop();
        base.CleanUp();
    }
}

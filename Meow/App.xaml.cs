namespace Meow;

public partial class App : Application
{
    private readonly BackgroundSyncService _backgroundSyncService;
    private readonly IServiceProvider _serviceProvider;

    public App(BackgroundSyncService backgroundSyncService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _backgroundSyncService = backgroundSyncService;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start background sync service when app starts
        _backgroundSyncService?.Start();

        var shell = _serviceProvider.GetRequiredService<AppShell>();
        var window = new Window(shell);
        
        // Force portrait orientation
        window.Created += (s, e) =>
        {
#if ANDROID
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity != null)
            {
                Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
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

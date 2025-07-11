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

        return new Window(new AppShell());
    }

    protected override void CleanUp()
    {
        // Stop background sync service when app closes
        _backgroundSyncService?.Stop();
        base.CleanUp();
    }
}

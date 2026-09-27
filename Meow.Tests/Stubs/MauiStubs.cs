// Stubs for MAUI types used by ViewModels
// These allow ViewModel files to compile in the test project without referencing MAUI assemblies

namespace Microsoft.Maui.ApplicationModel
{
    /// <summary>
    /// Stub for MAUI's AppTheme enum
    /// </summary>
    public enum AppTheme
    {
        Unspecified,
        Light,
        Dark
    }
}

namespace Microsoft.Maui.Controls
{
    /// <summary>
    /// Stub for MAUI's Application class used by BaseViewModel.SelectTheme()
    /// </summary>
    public class Application
    {
        public static Application? Current { get; set; }
        public Microsoft.Maui.ApplicationModel.AppTheme RequestedTheme { get; set; } = Microsoft.Maui.ApplicationModel.AppTheme.Light;
        public Microsoft.Maui.ApplicationModel.AppTheme UserAppTheme { get; set; } = Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
    }
}

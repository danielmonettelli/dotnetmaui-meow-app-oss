namespace Meow;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMaterialComponents(new List<string>
            {
                //generally, we needs add 6 types of font families
                "Roboto-Regular.ttf",
                "Roboto-Medium.ttf",
                "Roboto-Bold.ttf"
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Regular.ttf", "Roboto#400");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto#500");
                fonts.AddFont("Roboto-Bold.ttf", "Roboto#600");
            });

        builder.Services.AddSingleton<ICatService, CatService>();
        builder.Services.AddSingleton<VoteViewModel>();
        builder.Services.AddTransient<VotePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

namespace Meow;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseFFImageLoading()
            .UseMauiCommunityToolkit()
            .UseMaterialComponents()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Regular.ttf", "Roboto#400");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto#500");
                fonts.AddFont("Roboto-Bold.ttf", "Roboto#600");
            });

        builder.Services.AddSingleton<ICatService, CatService>();

        builder.Services.AddSingleton<VoteViewModel>();
        builder.Services.AddTransient<VotePage>();

        builder.Services.AddSingleton<BreedsViewModel>();
        builder.Services.AddTransient<BreedsPage>();

        builder.Services.AddSingleton<FavoriteViewModel>();
        builder.Services.AddTransient<FavoritePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

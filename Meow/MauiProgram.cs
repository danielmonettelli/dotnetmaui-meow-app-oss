namespace Meow;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Regular.ttf", "Roboto#400");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto#500");
                fonts.AddFont("Roboto-Bold.ttf", "Roboto#700");
            });

        // Infrastructure - Platform Services (SOLID: Dependency Inversion)
        builder.Services.AddSingleton<IDatabasePathProvider, MauiDatabasePathProvider>();
        builder.Services.AddSingleton<IConnectivityProvider, MauiConnectivityProvider>();
        builder.Services.AddSingleton<IUserIdentifierProvider, MauiUserIdentifierProvider>();

        // Infrastructure - HTTP Client
        builder.Services.AddHttpClient<ICatApiService, CatApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiConstants.BaseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", ApiConstants.ApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Infrastructure - Repositories
        builder.Services.AddSingleton<ICatCacheRepository, CatCacheRepository>();
        builder.Services.AddSingleton<IBreedCacheRepository, BreedCacheRepository>();
        builder.Services.AddSingleton<IFavoriteRepository, FavoriteRepository>();

        // Application - Use Cases
        builder.Services.AddTransient<GetVotingCatsUseCase>();
        builder.Services.AddTransient<GetBreedsUseCase>();
        builder.Services.AddTransient<GetCatsByBreedUseCase>();
        builder.Services.AddTransient<ManageFavoritesUseCase>();
        builder.Services.AddTransient<CacheMaintenanceUseCase>();

        // Infrastructure - Background Services
        builder.Services.AddSingleton<BackgroundSyncService>();

        // Navigation Shell
        builder.Services.AddSingleton<AppShell>();

        // Presentation - ViewModels & Pages (Singletons for root TabBar pages)
        builder.Services.AddSingleton<VoteViewModel>();
        builder.Services.AddSingleton<VotePage>();

        builder.Services.AddSingleton<BreedsViewModel>();
        builder.Services.AddSingleton<BreedsPage>();

        builder.Services.AddSingleton<FavoriteViewModel>();
        builder.Services.AddSingleton<FavoritePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

# Meow - Cat API .NET MAUI App

## Project Overview
This is a cross-platform .NET MAUI app that showcases cat images using TheCatAPI. The app follows MVVM pattern with dependency injection and uses modern .NET MAUI practices.

## Architecture & Patterns

### Core Structure
- **MVVM with CommunityToolkit.Mvvm**: All ViewModels inherit from `BaseViewModel` using `[ObservableProperty]` and `[RelayCommand]` attributes
- **Dependency Injection**: Services registered in `MauiProgram.cs` with singleton pattern for ViewModels and transient for Pages
- **Service Layer**: `ICatService` interface with `CatService` implementation handles all API communication
- **Shell Navigation**: Tab-based navigation defined in `AppShell.xaml` with three main tabs: Vote, Breeds, Favorites

### Key Dependencies
Uses these NuGet packages (see `GlobalUsings.cs` for imports):
- `CommunityToolkit.Maui` - UI controls and behaviors
- `CommunityToolkit.Mvvm` - MVVM source generators
- `FFImageLoading.Maui` - Image caching and loading
- `Material.Components.Maui.Extensions` - Material Design components
- `SkiaSharp.Views.Maui.Controls.Hosting` - Custom drawing

### API Integration
- **Base Configuration**: All API constants in `APIConstants.cs` including base URL and endpoints
- **Authentication**: Uses API key in request headers (`x-api-key`)
- **Endpoints**: Random cats, breeds, breed-specific searches, and favorites management
- **Models**: `Cat`, `Breed`, `FavoriteCatRequest/Response` with JsonPropertyName attributes for API mapping
- **Caching Strategy**: SQLite-based intelligent caching with offline support via `ICacheService`

### Data Architecture
- **SQLite Database**: Local caching using sqlite-net-pcl with three main tables:
  - `CachedCats`: Stores cats for voting (max 30, 24-hour expiry)
  - `CachedBreeds`: Permanent breed cache (7-day expiry)
  - `UserFavorites`: Local favorites with sync status tracking
- **Cache Strategy**: Offline-first approach with automatic sync when online
- **Repository Pattern**: `BaseRepository` with specific implementations for cats, breeds, and favorites
- **Background Sync**: Automatic synchronization every 15 minutes when online

## Development Conventions

### File Organization
```
Meow/
├── Constants/          # API endpoints and configuration
├── Services/          # Business logic and API calls
├── ViewModels/        # MVVM ViewModels inheriting BaseViewModel
├── Views/             # XAML Pages with code-behind
├── Models/            # Data models with JSON serialization
├── Controls/          # Custom UI controls
├── Converters/        # Value converters for data binding
└── ReusableComponents/ # Shared UI components
```

### Naming Conventions
- ViewModels: `{Feature}ViewModel` (e.g., `BreedsViewModel`)
- Pages: `{Feature}Page` (e.g., `BreedsPage.xaml`)
- Services: `I{Name}Service` interface, `{Name}Service` implementation
- API methods: `Get{Entity}`, `Add{Entity}`, `Delete{Entity}`

### API Key Setup
**CRITICAL**: Replace `APIKey` in `APIConstants.cs` with actual API key from thecatapi.com before running. The README provides detailed setup instructions.

## Common Development Tasks

### Adding New Features
1. Create model in `Models/` with `[JsonPropertyName]` attributes
2. Add API methods to `ICatService` and `CatService`
3. Create ViewModel inheriting `BaseViewModel`
4. Register ViewModel and Page in `MauiProgram.cs`
5. Add navigation in `AppShell.xaml` if needed

### Working with API Calls
- All HTTP requests use the shared `HttpClient` with base address and API key headers
- Return `default` on failed requests (pattern used throughout `CatService`)
- Use `JsonSerializer.Deserialize<T>()` for response parsing
- API endpoints are pre-configured in `APIConstants.cs`
- **Cache-first approach**: Use `ICacheService` instead of direct `ICatService` calls for better offline support
- **Background sync**: `BackgroundSyncService` handles periodic cache maintenance and favorites sync

### Cache Management
- **Voting Cache**: Automatically maintains 20-30 recent cats with LRU eviction
- **Breed Cache**: One-time load with 7-day expiration, persistent across app sessions
- **Favorites Sync**: Bidirectional sync with conflict resolution, works offline with pending operations
- **Database operations**: Use repository pattern with error handling and connectivity awareness

### UI Development
- Theme support: Use `AppThemeBinding` for light/dark mode colors
- Status bar behavior configured in `AppShell.xaml` using `StatusBarBehavior`
- Custom fonts: Roboto family (Regular, Medium, Bold) configured in `MauiProgram.cs`
- Material Design components available via Material.Components.Maui.Extensions

### Platform-Specific Code
Platform folders in `Platforms/` for Android, iOS, MacCatalyst, Windows, and Tizen with respective platform configurations and entry points.

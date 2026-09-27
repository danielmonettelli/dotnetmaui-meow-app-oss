<p align="center">
  <a href="https://github.com/danielmonettelli/dotnetmaui-meow-app-oss#gh-light-mode-only">
    <img width="320" src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/brand_light.svg#gh-light-mode-only" alt="Meow Logo Light">
  </a>
  <a href="https://github.com/danielmonettelli/dotnetmaui-meow-app-oss#gh-dark-mode-only">
    <img width="320" src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/brand_dark.svg#gh-dark-mode-only" alt="Meow Logo Dark">
  </a>
</p>

<p align="center">
  <strong>A modern, open-source .NET MAUI application built with .NET 10, Clean Architecture, and SOLID principles.</strong>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/en-us/apps/maui">
    <img src="https://img.shields.io/badge/.NET%20MAUI-.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET MAUI .NET 10">
  </a>
  <a href="https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/actions/workflows/mobile.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/danielmonettelli/dotnetmaui-meow-app-oss/mobile.yml?branch=main&style=for-the-badge&label=Build%20%26%20CI" alt="CI Status">
  </a>
  <a href="https://play.google.com/store/apps/details?id=com.danielmonettelli.meow">
    <img src="https://img.shields.io/badge/Google%20Play-Published-34A853?style=for-the-badge&logo=googleplay&logoColor=white" alt="Google Play Status">
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-blue?style=for-the-badge" alt="MIT License">
  </a>
</p>

<div align="center">

[![Stars](https://img.shields.io/github/stars/danielmonettelli/dotnetmaui-meow-app-oss?style=flat-square&color=ffd166)](https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/stargazers)
[![Forks](https://img.shields.io/github/forks/danielmonettelli/dotnetmaui-meow-app-oss?style=flat-square&color=06d6a0)](https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/network/members)
[![Issues](https://img.shields.io/github/issues/danielmonettelli/dotnetmaui-meow-app-oss?style=flat-square&color=118ab2)](https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/danielmonettelli/dotnetmaui-meow-app-oss?style=flat-square&color=8338ec)](https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/pulls)
[![Contributors](https://img.shields.io/github/contributors/danielmonettelli/dotnetmaui-meow-app-oss?style=flat-square&color=ef476f)](https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/graphs/contributors)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/3a130a6eae074e54b14b277d7617bff1)](https://app.codacy.com/gh/danielmonettelli/dotnetmaui-meow-app-oss/dashboard)

</div>

<br>

<p align="center">
  <a href="https://play.google.com/store/apps/details?id=com.danielmonettelli.meow">
    <img src="https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png" height="75" alt="Get it on Google Play">
  </a>
</p>

<p align="center">
  👉 <strong><a href="https://play.google.com/store/apps/details?id=com.danielmonettelli.meow">Download Meow on Google Play Store</a></strong>
</p>

---

<p align="center">
  <img src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/meow_main_cover.png" alt="Meow Cover Banner" width="100%">
</p>

## 📖 Table of Contents

- [About The Project](#-about-the-project)
- [Key Features](#-key-features)
- [Architecture & SOLID Principles](#-architecture--solid-principles)
- [Technology Stack](#-technology-stack)
- [Supported Platforms](#-supported-platforms)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Obtaining a TheCatAPI Key](#obtaining-a-thecatapi-key)
  - [Configuring the API Key](#configuring-the-api-key)
  - [Build and Run](#build-and-run)
  - [Running Unit Tests](#running-unit-tests)
- [Design Tool](#-design-tool)
- [Team & Contributors](#-team--contributors)
- [How to Contribute](#-how-to-contribute)
- [License](#-license)

---

## 🐾 About The Project

**Meow** is a production-ready, open-source multi-platform application designed for cat lovers. Built from the ground up targeting **.NET 10** and **.NET MAUI**, it demonstrates how to architect a modern, enterprise-grade mobile application using **100% native .NET MAUI controls** (with zero third-party UI dependencies).

The application connects to [TheCatAPI](https://thecatapi.com/) to deliver an infinite stream of cute feline photos, complete breed encyclopedias, and personal favorite collections with an **offline-first** caching strategy.

---

## ✨ Key Features

- **🗳️ Vote & Discover**:
  - Infinite feed of random high-definition cat pictures.
  - Interactive double-tap and tap-to-favorite gesture interactions.
  - Custom native heart confetti animation burst when expressing love for a kitten.
  - Separate "Love It" and "Nope It" discovery navigation buttons.
  - Instant floating badge indicating current favorite status.

- **📖 Breeds Encyclopedia**:
  - Comprehensive catalog of domestic cat breeds with origin, description, and personality temperament tags.
  - **Dynamic Paw-Print Rating System**: Custom-engineered vector rating control powered by `Microsoft.Maui.Graphics` that visibly fills paw prints for core traits (*Affection Level*, *Adaptability*, *Child Friendly*, and *Dog Friendly*).
  - High-resolution gallery per breed with graceful empty-state handling for breeds without current photos.

- **⭐ Offline-First Favorites**:
  - Saved favorites collection powered by local SQLite caching.
  - Automatic synchronization and fallback resilience when offline.
  - High-performance, 60 FPS smooth scrolling CollectionView.

- **🎨 Tailored UI/UX & Theming**:
  - Native **Dark Mode** and **Light Mode** adaptive themes.
  - Responsive **AppShell** tab navigation.
  - Clean vector iconography and typography.

---

## 🏛️ Architecture & SOLID Principles

The solution strictly adheres to **Clean Architecture** (Uncle Bob) to enforce decoupling between business rules, data access, and presentation frameworks:

```
Meow/
├── Meow.Domain/          # Enterprise Business Rules (Pure Entities, no dependencies)
├── Meow.Core/            # Application Business Rules (Use Cases & Repository Interfaces)
├── Meow.Infrastructure/  # Data Access (TheCatAPI Service, SQLite Repositories, Offline Sync)
├── Meow/                 # Presentation Layer (XAML, ViewModels, Custom Controls, AppShell)
└── Meow.Tests/           # Unit Tests (158 Tests covering Domain, Core, ViewModels, Graphics)
```

### Clean Architecture Layers

1. **`Meow.Domain`**
   - Contains core domain entities: `Cat`, `Breed`, `UserFavorite`, `Category`.
   - Zero external dependencies; pure C# and .NET 10.
2. **`Meow.Core`**
   - Application use cases: `GetVotingCatsUseCase`, `GetBreedsUseCase`, `GetCatsByBreedUseCase`, `ManageFavoritesUseCase`, `CacheMaintenanceUseCase`.
   - Repository interfaces and contracts (`ICatApiService`, `ICatCacheRepository`, `IBreedCacheRepository`, `IFavoriteRepository`).
3. **`Meow.Infrastructure`**
   - Implementation of external concerns: HTTP API Client (`CatApiService`), SQLite database tables, and offline caching logic (`CatCacheRepository`, `FavoriteRepository`).
   - Implements `IConnectivityProvider` and `IDatabasePathProvider`.
4. **`Meow` (Presentation & UI)**
   - .NET MAUI UI with MVVM pattern powered by `CommunityToolkit.Mvvm`.
   - AppShell structure, custom controls (`RatingView`, `RatingCanvas`), and adaptive theme dictionaries (`Colors.xaml`, `Global.xaml`).
5. **`Meow.Tests`**
   - 158 automated unit tests built with xUnit v3, FluentAssertions, Moq, and Microsoft.Testing.Platform.

### SOLID Principles in Action

| Principle | Implementation in Meow |
| :--- | :--- |
| **S - Single Responsibility** | Every Use Case executes a single business operation (e.g. `ManageFavoritesUseCase` only orchestrates favoriting logic). |
| **O - Open / Closed** | Repositories and caching strategies are easily extensible via interfaces without altering use case consumers. |
| **L - Liskov Substitution** | Platform abstractions (like database paths or network connectivity) can be substituted seamlessly between test mocks and mobile targets. |
| **I - Interface Segregation** | Fine-grained contracts (`IFavoriteRepository`, `ICatCacheRepository`, `IBreedCacheRepository`) ensure classes only implement what they use. |
| **D - Dependency Inversion** | High-level Use Cases depend strictly on domain abstractions; concrete SQLite and HTTP clients are injected at startup via Microsoft.Extensions.DependencyInjection. |

---

## 🛠️ Technology Stack

- **Framework**: [.NET 10](https://dotnet.microsoft.com/) & [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- **Language**: C# 14
- **Architecture**: Clean Architecture + SOLID + MVVM
- **State & MVVM**: [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- **Graphics Engine**: `Microsoft.Maui.Graphics` (Custom hardware-accelerated 2D vector drawing)
- **Local Persistence**: [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net) + SQLitePCLRaw
- **Testing**: xUnit v3, Microsoft.Testing.Platform, FluentAssertions, Moq
- **API**: [TheCatAPI](https://thecatapi.com/)
- **CI/CD**: GitHub Actions (Multi-platform automated build & Clean Architecture test quality gate)

---

## 📱 Supported Platforms

| Platform | Support | Architecture |
| :--- | :---: | :--- |
| **Android** | ✔️ Supported | API 21+ (Android 5.0 through Android 15+) |
| **iOS** | ✔️ Supported | iOS 15.0+ |
| **macOS (Mac Catalyst)** | ✔️ Supported | macOS 12.0+ (Apple Silicon & Intel) |
| **Windows** | ✔️ Supported | Windows 10 (Build 17763) & Windows 11 |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Version 10.0.100 or higher)
- .NET MAUI Workloads:
  ```bash
  dotnet workload install maui
  # Or install specific platform workloads:
  dotnet workload install maui-android maui-ios maui-maccatalyst maui-windows
  ```
- Visual Studio 2026 / Visual Studio Code with the [.NET MAUI Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui).

### Obtaining a TheCatAPI Key

The app requires an API key from [TheCatAPI](https://thecatapi.com/):

1. Go to [https://thecatapi.com/](https://thecatapi.com/) and click **`GET YOUR API KEY`**.
   <br><img src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/thecatapi_part_1_updated.png" width="450" alt="TheCatAPI Step 1">

2. Select the **`FREE`** tier and click **`GET FREE ACCESS`**.
   <br><img src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/thecatapi_part_2_updated.png" width="450" alt="TheCatAPI Step 2">

3. Provide your email, choose `A PERSONAL PROJECT`, and submit. Your API key will be delivered to your inbox.
   <br><img src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/main/Assets/thecatapi_part_3_updated.png" width="450" alt="TheCatAPI Step 3">

### Configuring the API Key

Open `Meow.Infrastructure/Api/ApiConstants.cs` and paste your API key:

```csharp
namespace Meow.Infrastructure.Api;

public static class ApiConstants
{
    public const string BaseUrl = "https://api.thecatapi.com/v1/";
    public const string ApiKey = "YOUR_API_KEY_HERE";
}
```

### Build and Run

To run the application on Android:
```bash
dotnet build Meow/Meow.csproj -t:Run -f net10.0-android
```

To run on Windows:
```bash
dotnet build Meow/Meow.csproj -t:Run -f net10.0-windows10.0.19041.0
```

To run on macOS (Mac Catalyst):
```bash
dotnet build Meow/Meow.csproj -t:Run -f net10.0-maccatalyst
```

### Running Unit Tests

Execute the comprehensive test suite (158 tests) across Domain, Core, ViewModels, and Controls:
```bash
dotnet test Meow.Tests/Meow.Tests.csproj -c Release
```

---

## 🎨 Design Tool

The UI and UX of Meow were conceived and designed in [Figma](https://www.figma.com/):

<p align="left">
  <a href="https://www.figma.com/">
    <img src="https://raw.githubusercontent.com/danielmonettelli/dotnetmaui-meow-app-oss/99e22ab94d88778b60963f06bae1405e7903625b/Assets/figma.png" width="80" alt="Figma Design">
  </a>
</p>

---

## 👥 Team & Contributors

### Creator & Lead Architect

<table align="center">
  <tr>
    <td align="center">
      <a href="https://github.com/danielmonettelli">
        <img src="https://avatars.githubusercontent.com/u/14121125?v=4" width="120px;" style="border-radius:50%;" alt="Daniel Monettelli"/><br />
        <sub><b>Daniel Monettelli</b></sub>
      </a><br />
      <sub>Creator, UI/UX Designer & Lead Engineer</sub>
    </td>
    <td align="center">
      <a href="https://github.com/BryanOroxon">
        <img src="https://avatars.githubusercontent.com/u/25359161?v=4" width="120px;" style="border-radius:50%;" alt="Bryan Oroxón"/><br />
        <sub><b>Bryan Oroxón</b></sub>
      </a><br />
      <sub>Special Collaborator & Software Developer</sub>
    </td>
  </tr>
</table>

### Open Source Contributors

A huge thank you to everyone who has contributed to making **Meow** better!

<p align="center">
  <a href="https://github.com/danielmonettelli/dotnetmaui-meow-app-oss/graphs/contributors">
    <img src="https://contrib.rocks/image?repo=danielmonettelli/dotnetmaui-meow-app-oss" alt="Meow Contributors" />
  </a>
</p>

---

## 🤝 How to Contribute

Contributions make the open-source community an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**!

1. **Fork the Repository** on GitHub.
2. **Clone your fork**:
   ```bash
   git clone https://github.com/<your-username>/dotnetmaui-meow-app-oss.git
   ```
3. **Create a Feature Branch**:
   ```bash
   git checkout -b feature/amazing-feature
   ```
4. **Make your changes** and adhere to Clean Architecture & SOLID conventions.
5. **Verify all tests pass**:
   ```bash
   dotnet test Meow.Tests/Meow.Tests.csproj -c Release
   ```
6. **Commit your changes**:
   ```bash
   git commit -m "feat: Add amazing feature"
   ```
7. **Push to your branch**:
   ```bash
   git push origin feature/amazing-feature
   ```
8. **Open a Pull Request** against the `main` branch.

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
Copyright (c) Daniel Monettelli
```

<p align="center">
  Made with ❤️ for cats and the .NET MAUI community.
</p>

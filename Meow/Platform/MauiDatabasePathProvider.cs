using Meow.Domain.Interfaces;
using Meow.Infrastructure.Persistence;

namespace Meow.Platform;

/// <summary>
/// MAUI platform implementation of IDatabasePathProvider.
/// Provides the database file path using MAUI's FileSystem.AppDataDirectory.
/// This is the only place that depends on MAUI's file system API for database paths,
/// keeping the Infrastructure layer platform-independent (SOLID: Dependency Inversion).
/// </summary>
public class MauiDatabasePathProvider : IDatabasePathProvider
{
    public string DatabasePath =>
        System.IO.Path.Combine(FileSystem.AppDataDirectory, DatabaseConfig.DatabaseFilename);
}

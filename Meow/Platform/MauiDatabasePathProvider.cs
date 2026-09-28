using Meow.Domain.Interfaces;
using Meow.Infrastructure.Persistence;

namespace Meow.Platform;

/// <summary>
/// MAUI platform implementation of IDatabasePathProvider.
/// On Android, uses Context.NoBackupFilesDir so that the SQLite database is never
/// backed up to Google Drive / cloud backup and is completely destroyed upon uninstallation.
/// On other platforms, uses MAUI's FileSystem.AppDataDirectory.
/// Keeps the Infrastructure layer platform-independent (SOLID: Dependency Inversion).
/// </summary>
public class MauiDatabasePathProvider : IDatabasePathProvider
{
    public string DatabasePath
    {
        get
        {
#if ANDROID
            var noBackupDir = Android.App.Application.Context.NoBackupFilesDir?.AbsolutePath;
            if (!string.IsNullOrEmpty(noBackupDir))
            {
                // Clean up legacy or cloud-restored database from AppDataDirectory if it exists
                var legacyPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, DatabaseConfig.DatabaseFilename);
                if (System.IO.File.Exists(legacyPath))
                {
                    try { System.IO.File.Delete(legacyPath); } catch { /* ignore */ }
                }

                return System.IO.Path.Combine(noBackupDir, DatabaseConfig.DatabaseFilename);
            }
#endif
            return System.IO.Path.Combine(FileSystem.AppDataDirectory, DatabaseConfig.DatabaseFilename);
        }
    }
}

using Meow.Domain.Interfaces;

namespace Meow.Platform;

/// <summary>
/// MAUI platform implementation of IUserIdentifierProvider.
/// Generates and persists a unique installation identifier.
/// On Android:
/// 1. Uses Context.NoBackupFilesDir to store the installation token. Since NoBackupFilesDir
///    is guaranteed by Android to NEVER be backed up to Google Drive and is deleted on uninstall,
///    a reinstallation always starts with no token file and generates a fresh identifier.
/// 2. Validates against PackageInfo.FirstInstallTime to detect if Preferences were restored
///    from a prior installation's cloud backup, in which case restored identifiers are discarded.
/// On other platforms, uses MAUI Preferences.
/// </summary>
public class MauiUserIdentifierProvider : IUserIdentifierProvider
{
    private const string InstallationIdKey = "meow_installation_user_id";
    private const string FirstInstallTimeKey = "meow_first_install_time";
    private const string TokenFileName = "meow_installation_id.txt";

    private string? _cachedUserId;
    private readonly object _lock = new();

    public string GetUserIdentifier()
    {
        if (!string.IsNullOrEmpty(_cachedUserId))
            return _cachedUserId;

        lock (_lock)
        {
            if (!string.IsNullOrEmpty(_cachedUserId))
                return _cachedUserId;

#if ANDROID
            var noBackupDir = Android.App.Application.Context.NoBackupFilesDir?.AbsolutePath;
            if (!string.IsNullOrEmpty(noBackupDir))
            {
                var tokenFilePath = System.IO.Path.Combine(noBackupDir, TokenFileName);

                // Verify install time against any restored preferences
                long currentFirstInstallTime = GetAndroidFirstInstallTime();
                long savedFirstInstallTime = Preferences.Default.Get<long>(FirstInstallTimeKey, 0);

                bool isReinstallWithRestoredBackup = currentFirstInstallTime > 0
                    && savedFirstInstallTime > 0
                    && savedFirstInstallTime != currentFirstInstallTime;

                if (isReinstallWithRestoredBackup)
                {
                    // Cloud backup restored old preferences from a previous install — discard them
                    Preferences.Default.Remove(InstallationIdKey);
                    if (System.IO.File.Exists(tokenFilePath))
                    {
                        try { System.IO.File.Delete(tokenFilePath); } catch { /* ignore */ }
                    }
                }

                if (System.IO.File.Exists(tokenFilePath))
                {
                    try
                    {
                        var existingToken = System.IO.File.ReadAllText(tokenFilePath).Trim();
                        if (!string.IsNullOrWhiteSpace(existingToken))
                        {
                            _cachedUserId = existingToken;
                            Preferences.Default.Set(InstallationIdKey, existingToken);
                            if (currentFirstInstallTime > 0)
                            {
                                Preferences.Default.Set(FirstInstallTimeKey, currentFirstInstallTime);
                            }
                            return _cachedUserId;
                        }
                    }
                    catch { /* fallback to generating new token */ }
                }

                // Token file does not exist in NoBackupFilesDir (new install or fresh install after uninstall)
                var newUserId = $"user_{Guid.NewGuid():N}";
                try
                {
                    System.IO.File.WriteAllText(tokenFilePath, newUserId);
                }
                catch { /* ignore */ }

                Preferences.Default.Set(InstallationIdKey, newUserId);
                if (currentFirstInstallTime > 0)
                {
                    Preferences.Default.Set(FirstInstallTimeKey, currentFirstInstallTime);
                }

                _cachedUserId = newUserId;
                return _cachedUserId;
            }
#endif

            // Non-Android platforms or fallback
            var existingId = Preferences.Default.Get<string?>(InstallationIdKey, null);
            if (string.IsNullOrWhiteSpace(existingId))
            {
                existingId = $"user_{Guid.NewGuid():N}";
                Preferences.Default.Set(InstallationIdKey, existingId);
            }

            _cachedUserId = existingId;
            return _cachedUserId;
        }
    }

#if ANDROID
    private static long GetAndroidFirstInstallTime()
    {
        try
        {
            var context = Android.App.Application.Context;
            var packageInfo = context.PackageManager?.GetPackageInfo(context.PackageName ?? string.Empty, 0);
            return packageInfo?.FirstInstallTime ?? 0;
        }
        catch
        {
            return 0;
        }
    }
#endif
}

using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace KingdomRushClone;

public partial class App : Application
{
    private static readonly string LogPath =
        Path.Combine(Path.GetTempPath(), "krc_crash.log");

    public App()
    {
        DispatcherUnhandledException += OnUnhandledException;

        // Pre-warm static catalogs so any init error is captured before UI shows.
        try
        {
            var stages  = Data.StageCatalog.Stages;
            var towers  = Data.TowerCatalog.Towers;
            var enemies = Data.EnemyCatalog.Enemies;
            File.WriteAllText(LogPath,
                $"OK\nStages={stages.Count} Towers={towers.Count} Enemies={enemies.Count}\n");
        }
        catch (Exception ex)
        {
            File.WriteAllText(LogPath,
                $"STATIC INIT FAILED\n{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}\n\nInner:\n{ex.InnerException?.GetType().FullName}: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            File.AppendAllText(LogPath,
                $"\n\nRUNTIME CRASH\n{e.Exception.GetType().FullName}: {e.Exception.Message}\n{e.Exception.StackTrace}\n\nInner:\n{e.Exception.InnerException?.GetType().FullName}: {e.Exception.InnerException?.Message}\n{e.Exception.InnerException?.StackTrace}");
        }
        catch { }
        MessageBox.Show(
            $"예외 발생:\n\n{e.Exception.GetType().Name}\n{e.Exception.Message}\n\n--- StackTrace ---\n{e.Exception.StackTrace}\n\n--- Inner ---\n{e.Exception.InnerException?.Message}\n{e.Exception.InnerException?.StackTrace}",
            "에러",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}

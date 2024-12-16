using System.IO;
using Microsoft.Maui.Storage;

namespace FileSyncApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Выбор папки источника
    private async void OnSelectSourceDirectory(object sender, EventArgs e)
    {
        try
        {
            var folderPicker = await PickFolderAsync();
            if (!string.IsNullOrWhiteSpace(folderPicker))
            {
                SourceDirectoryEntry.Text = folderPicker;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать папку источника: {ex.Message}", "OK");
        }
    }

    // Выбор папки назначения
    private async void OnSelectDestinationDirectory(object sender, EventArgs e)
    {
        try
        {
            var folderPicker = await PickFolderAsync();
            if (!string.IsNullOrWhiteSpace(folderPicker))
            {
                DestinationDirectoryEntry.Text = folderPicker;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать папку назначения: {ex.Message}", "OK");
        }
    }

    // Метод для выбора папки
    private async Task<string?> PickFolderAsync()
    {
#if WINDOWS
    var picker = new Windows.Storage.Pickers.FolderPicker();
    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
    picker.FileTypeFilter.Add("*");

    // Получаем дескриптор текущего окна
    var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

    var folder = await picker.PickSingleFolderAsync();
    return folder?.Path;
#else
        await DisplayAlert("Ошибка", "Выбор папок поддерживается только на Windows.", "OK");
        return null;
#endif
    }


    // Синхронизация папок
    private async void OnSynchronize(object sender, EventArgs e)
    {
        string sourceDir = SourceDirectoryEntry.Text;
        string destDir = DestinationDirectoryEntry.Text;

        if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir))
        {
            await DisplayAlert("Ошибка", "Укажите обе директории.", "OK");
            return;
        }

        try
        {
            // Запускаем синхронизацию
            SyncDirectories(sourceDir, destDir);
            await DisplayAlert("Готово", "Синхронизация завершена.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка синхронизации: {ex.Message}", "OK");
        }
    }

    // Рекурсивная синхронизация директорий (из предыдущего примера)
    private void SyncDirectories(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        var sourceFiles = Directory.GetFiles(sourceDir);
        foreach (var sourceFile in sourceFiles)
        {
            string fileName = Path.GetFileName(sourceFile);
            string destFile = Path.Combine(destDir, fileName);

            try
            {
                if (!File.Exists(destFile) || File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(destFile))
                {
                    File.Copy(sourceFile, destFile, true);
                    LogAction($"Скопирован файл: {fileName}");
                }
                else
                {
                    LogAction($"Пропущен файл: {fileName}");
                }
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка при обработке файла {fileName}: {ex.Message}");
            }
        }

        var sourceSubDirs = Directory.GetDirectories(sourceDir);
        foreach (var sourceSubDir in sourceSubDirs)
        {
            string subDirName = Path.GetFileName(sourceSubDir);
            string destSubDir = Path.Combine(destDir, subDirName);
            SyncDirectories(sourceSubDir, destSubDir);
        }
    }

    private void LogAction(string message)
    {
        Console.WriteLine(message);
    }
}

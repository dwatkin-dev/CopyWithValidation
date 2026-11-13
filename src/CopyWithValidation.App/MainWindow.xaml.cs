using CopyWithValidation.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace CopyWithValidation.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Sets the FileCopier and its chosen FileHasher
    private readonly FileCopier _copier = new FileCopier(new FileHasher());

    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        if (dialog.ShowDialog() == true)
        {
            SourceTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseDestinationButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            DestinationTextBox.Text = dialog.FolderName;
        }
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var source = SourceTextBox.Text;
        var destinationFolder = DestinationTextBox.Text;

        if (string.IsNullOrWhiteSpace(source))
        {
            ResultTextBlock.Text = "Please choose a source file.";
            return;
        }
        if (string.IsNullOrWhiteSpace(destinationFolder))
        {
            ResultTextBlock.Text = "Please choose a destination folder.";
            return;
        }

        var destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(source));

        var progress = new Progress<CopyProgress>(p =>
        {
            CopyProgressBar.Value = p.Percent;
            ResultTextBlock.Text = $"{p.Phase}...";
        });

        EnableUI(false);
        _cts = new CancellationTokenSource();

        try
        {
            bool verified = await _copier.CopyAndVerifyAsync(source, destinationFilePath, progress, _cts.Token);
            ResultTextBlock.Text = verified ? "Copy verified." : "Verification FAILED.";
        }
        catch (OperationCanceledException)
        {
            ResultTextBlock.Text = "Cancelled.";
        }
        catch (FileTooLargeForFileSystemException ex)
        {
            ResultTextBlock.Text = $"The destination is {ex.FileSystem}, which can’t hold files over 4 GB.Use an NTFS or exFAT drive.";
        }
        catch (InsufficientSpaceException ex)
        {
            ResultTextBlock.Text = $"Not enough space on the destination: free up {ByteSize.Format(ex.Required - ex.Available)}.";
        }
        catch (Exception ex)
        {
            ResultTextBlock.Text = $"Error: {ex.Message}";
        }
        finally
        {
            EnableUI(true);
            _cts.Dispose();
            _cts = null;
        }
    }

    private void EnableUI(bool enable)
    {
        CopyButton.IsEnabled = enable;
        BrowseSourceButton.IsEnabled = enable;
        BrowseDestinationButton.IsEnabled = enable;
        CancelButton.IsEnabled = !enable;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
    }
}
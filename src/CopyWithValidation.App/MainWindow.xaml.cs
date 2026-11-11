using CopyWithValidation.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace CopyWithValidation.App;

/// <summary>
/// Code-behind for MainWindow.xaml. Kept thin: it reads the inputs, calls Core, and shows results.
/// </summary>
// 'partial' = this class is split across files. The other half is generated from the XAML at
// build time: it creates fields for every x:Name (SourceTextBox, CopyButton...) and wires up
// Click="..." handlers, i.e. CopyButton.Click += CopyButton_Click;
public partial class MainWindow : Window
{
    // Composition root: the one place the real objects are chosen and plugged together.
    // The window uses a real FileHasher; tests use FileCopier with fakes instead.
    private readonly FileCopier _copier = new FileCopier(new FileHasher());
    public MainWindow()
    {
        InitializeComponent();   // builds the controls described in MainWindow.xaml
    }

    /// <summary>Lets the user pick the source file.</summary>
    // Event handler: a method added to the button's Click event (an event = a list of delegates).
    // WPF calls it with sender (the button clicked) and e (details about the event).
    private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
        // ShowDialog returns bool? (true / false / null), so compare with == true
        if (dialog.ShowDialog() == true)
        {
            SourceTextBox.Text = dialog.FileName;
        }
    }

    /// <summary>Lets the user pick the destination folder.</summary>
    private void BrowseDestinationButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            DestinationTextBox.Text = dialog.FolderName;
        }
    }

    /// <summary>Validates the inputs, then copies and verifies while showing progress.</summary>
    // 'async void' is only OK for event handlers (WPF can't await them). Everywhere else use async Task.
    // This runs on the UI thread. The copy no longer needs Task.Run: Core is truly async, so
    // each await hands the UI thread back and the window stays responsive.
    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var source = SourceTextBox.Text;
        var destinationFolder = DestinationTextBox.Text;

        // Guard clauses: check for bad input first and leave early.
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

        // Path.Combine adds the right separator: "D:\Backup" + "image.bin" -> "D:\Backup\image.bin"
        var destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(source));

        // Progress<T> remembers the thread it was created on (the UI thread here). Core may call
        // Report from a pool thread; Progress<T> POSTS this lambda back to the UI thread, which is
        // the only thread allowed to touch WPF controls. (ActionProgress here would throw
        // "The calling thread cannot access this object because a different thread owns it".)
        // { } = multi-statement lambda. p is a CopyProgress; $"{p.Phase}..." uses the enum's name.
        var progress = new Progress<CopyProgress>(p =>
        {
            CopyProgressBar.Value = p.Percent;
            ResultTextBlock.Text = $"{p.Phase}...";
        });

        EnableUI(false);

        try
        {
            // Plain await (no ConfigureAwait(false)) – after it we must be back on the UI thread
            // to set ResultTextBlock.Text.
            bool verified = await _copier.CopyAndVerifyAsync(source, destinationFilePath, progress);
            ResultTextBlock.Text = verified ? "Copy verified." : "Verification FAILED.";   // ?: = if/else in one expression
        }
        catch (Exception ex)
        {
            // Copy errors (e.g. destination already exists) arrive here as exceptions
            ResultTextBlock.Text = $"Error: {ex.Message}";
        }
        finally
        {
            // finally always runs – success or error – so the buttons are always re-enabled
            EnableUI(true);
        }
    }

    /// <summary>Turns the buttons on or off, so the user can't start a second copy mid-copy.</summary>
    private void EnableUI(bool enable)
    {
        CopyButton.IsEnabled = enable;
        BrowseSourceButton.IsEnabled = enable;
        BrowseDestinationButton.IsEnabled = enable;
    }
}
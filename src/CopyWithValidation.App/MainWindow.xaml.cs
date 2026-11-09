using CopyWithValidation.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace CopyWithValidation.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Sets the FileCopier and its chosen FileHasher
    private readonly FileCopier _copier = new FileCopier(new FileHasher());
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
        if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
        {
            ResultTextBlock.Text = "Please choose a source file.";
            return;
        }
        if (string.IsNullOrWhiteSpace(DestinationTextBox.Text))
        {
            ResultTextBlock.Text = "Please choose a destination folder.";
            return;
        }

        var source = SourceTextBox.Text;
        var destinationFilePath = Path.Combine(DestinationTextBox.Text, Path.GetFileName(source));

        ResultTextBlock.Text = "Copying...";
        CopyButton.IsEnabled = false;
        BrowseSourceButton.IsEnabled = false;
        BrowseDestinationButton.IsEnabled = false;

        try
        {
            bool verified = await Task.Run(() => _copier.CopyAndVerify(source, destinationFilePath));
            ResultTextBlock.Text = verified ? "Copy verified." : "Verification FAILED.";
        }
        catch (Exception ex) 
        {
            ResultTextBlock.Text = ex.Message;
        }
        finally
        {
            CopyButton.IsEnabled = true;
            BrowseSourceButton.IsEnabled = true;
            BrowseDestinationButton.IsEnabled = true;
        }
    }
}
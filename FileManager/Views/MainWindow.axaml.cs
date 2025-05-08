using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileManager.ViewModels;

namespace FileManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }


    private void InputElement_OnTextInput(object? sender, TextChangedEventArgs textChangedEventArgs)
    {
        MainWindowViewModel? viewModel = DataContext as MainWindowViewModel;
        string? text = (sender as TextBox)?.Text;
        DriveInfo driveInfo = new DriveInfo("C");

        Task.Run(() => viewModel?.RefreshView(driveInfo, text));
    }
}
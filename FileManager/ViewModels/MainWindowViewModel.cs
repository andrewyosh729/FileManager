using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using FileManager.Models;
using FileManager.Utils;

namespace FileManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DriveInfo DriveInfo { get; }


    public HierarchicalTreeDataGridSource<FileSystemInfoWrapper> FilesAndDirectories { get; set; }

    public MainWindowViewModel()
    {
        DriveInfo = new DriveInfo("C");
        DirectoryInfo directoryInfo = new DirectoryInfo("C:\\");
        FilesAndDirectories =
            new HierarchicalTreeDataGridSource<FileSystemInfoWrapper>(new FileSystemInfoWrapper(directoryInfo))
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<FileSystemInfoWrapper>(
                        new TextColumn<FileSystemInfoWrapper, string>("Name", info => info.FileSystemInfo.Name),
                        info => info.Children),
                    new TemplateColumn<FileSystemInfoWrapper>("Size", new FuncDataTemplate<FileSystemInfoWrapper>(
                        (_, _) =>
                        {
                            Grid grid = new Grid()
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Width = 200,
                                Height = 20
                            };
                            ProgressBar progressBar = new()
                            {
                                Height = 20,
                                Minimum = 0,
                                Maximum = 1
                            };
                            progressBar.Bind(RangeBase.ValueProperty,
                                new Binding(nameof(FileSystemInfoWrapper.FileSize))
                                {
                                    Converter = new ProgressConverter(DriveInfo)
                                });


                            TextBlock textBlock = new TextBlock
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                            textBlock.Bind(TextBlock.TextProperty,
                                new Binding(nameof(FileSystemInfoWrapper.FileSizeString)));

                            grid.Children.Add(progressBar);
                            grid.Children.Add(textBlock);
                            return grid;
                        }))
                }
            };
    }
}
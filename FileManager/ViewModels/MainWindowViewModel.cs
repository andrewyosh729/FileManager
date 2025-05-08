using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using FileManager.Collections;
using FileManager.Models;
using FileManager.Utils;

namespace FileManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private HierarchicalTreeDataGridSource<FileSystemInfoWrapper> m_FilesAndDirectories;

    public HierarchicalTreeDataGridSource<FileSystemInfoWrapper> FilesAndDirectories
    {
        get => m_FilesAndDirectories;
        set
        {
            if (Equals(value, m_FilesAndDirectories)) return;
            m_FilesAndDirectories = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        Task.Run(() => RefreshView(new DriveInfo("C"), null));
    }


    private CancellationTokenSource? CancellationTokenSource { get; set; }

    public void RefreshView(DriveInfo driveInfo, string? search)
    {
        FilesAndDirectories = null;
        if (CancellationTokenSource != null)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }

        CancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = CancellationTokenSource.Token;
        FileSystemInfoChildren children = new FileSystemInfoChildren();
        if (!string.IsNullOrWhiteSpace(search))
        {
            Task.Run(() =>
            {
                foreach (FileSystemInfoWrapper item in FileSystemEnumerationUtils.EnumerateFileSystemEntries(
                             driveInfo.RootDirectory.FullName, new EnumerationOptions()
                             {
                                 RecurseSubdirectories = true,
                                 IgnoreInaccessible = true
                             }).AsParallel())
                {
                    if (token.IsCancellationRequested)
                    {
                        return Task.CompletedTask;
                    }

                    if (item.FileSystemInfo.Name.Contains(search))
                    {
                        children.Add(item);
                    }
                }

                return Task.CompletedTask;
            });
        }

        if (token.IsCancellationRequested)
        {
            return;
        }

        FilesAndDirectories =
            new HierarchicalTreeDataGridSource<FileSystemInfoWrapper>(
                !string.IsNullOrWhiteSpace(search)
                    ? children
                    : [new FileSystemInfoWrapper(driveInfo.RootDirectory)])
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<FileSystemInfoWrapper>(
                        new TextColumn<FileSystemInfoWrapper, string>("Name", info => info.FileSystemInfo.Name),
                        info => info.Children, wrapper => wrapper.IsDirectory, wrapper => wrapper.IsExpanded),
                    new TemplateColumn<FileSystemInfoWrapper>("Size",
                        new FuncDataTemplate<FileSystemInfoWrapper>(
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
                                        Converter = new ProgressConverter(driveInfo)
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
                            })),
                    new TextColumn<FileSystemInfoWrapper, string>("Last Modified",
                        wrapper => wrapper.LastWriteTimeString)
                }
            };
    }
}
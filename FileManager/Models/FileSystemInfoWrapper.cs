using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.PlatformServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using FileManager.Collections;
using FileManager.Utils;

namespace FileManager.Models;

public class FileSystemInfoWrapper : INotifyPropertyChanged
{
    public FileSystemInfoLight FileSystemInfo { get; }


    public FileSystemInfoWrapper(FileSystemInfoLight fileSystemInfo)
    {
        FileSystemInfo = fileSystemInfo;
        Task.Run(PopulateChildren);
        Task.Run(GetFileSizeAsync);
    }

    private TaskCompletionSource<bool> ChildrenPopulated { get; } = new();

    private TaskCompletionSource<bool> FileSizePopulated { get; } = new();

    private async Task GetFileSizeAsync()
    {
        if (!FileSystemInfo.IsDirectory)
        {
            try
            {
                FileSize = FileSystemInfo.FileSizeBytes;
            }
            finally
            {
                FileSizePopulated.TrySetResult(true);
            }
        }

        await ChildrenPopulated.Task;
        await Task.WhenAll(Children.Select(c => c.FileSizePopulated.Task));
        long sum = 0;
        foreach (FileSystemInfoWrapper child in Children)
        {
            sum += child.FileSize.Value;
        }

        try
        {
            FileSize = sum;
        }
        finally
        {
            FileSizePopulated.TrySetResult(true);
        }
    }

    public string FileSizeString => FileSize.HasValue ? FileSize.Value.ToReadableByteString() : "Calculating...";


    private long? m_FileSize;

    public long? FileSize
    {
        get => m_FileSize;
        set
        {
            if (SetField(ref m_FileSize, value))
            {
                OnPropertyChanged(nameof(FileSizeString));
            }
        }
    }

    public string LastWriteTimeString => FileSystemInfo.LastWriteTime.ToString(CultureInfo.CurrentCulture);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public FileSystemInfoChildren Children { get; } = new();

    private void PopulateChildren()
    {
        if (!FileSystemInfo.IsDirectory)
        {
            return;
        }

        foreach (FileSystemInfoWrapper item in FileSystemEnumerationUtils.EnumerateFileSystemEntries(FileSystemInfo.FullName.ToString(),
                     new EnumerationOptions()
                     {
                         IgnoreInaccessible = true
                     }).AsParallel())
        {
            Children.Add(item);
        }

        ChildrenPopulated.TrySetResult(true);
    }


    public bool IsExpanded { get; set; }

    public bool IsDirectory => FileSystemInfo.IsDirectory;
}
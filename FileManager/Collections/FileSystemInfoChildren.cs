using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Media.TextFormatting.Unicode;
using FileManager.Models;
using FileManager.Utils;
using FileManager.ViewModels;

namespace FileManager.Collections;

public class FileSystemInfoChildren : ConcurrentObservableList<FileSystemInfoWrapper>
{
    private ThrottledTaskQueue ThrottledTaskQueue { get; } = new();

    public FileSystemInfoChildren(IEnumerable<FileSystemInfoWrapper> collection) : base(collection)
    {
        foreach (var item in this)
        {
            item.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(FileSystemInfoWrapper.FileSize))
        {
            return;
        }

        ThrottledTaskQueue.QueueWork(() => Task.Run (() => Sort((f1, f2) => -Comparison(f1, f2))));
    }


    private int Comparison(object x, object y)
    {
        if (x is not FileSystemInfoWrapper f1 || y is not FileSystemInfoWrapper f2)
        {
            return 0;
        }

        if (f1.FileSize.HasValue && f2.FileSize.HasValue)
        {
            return f1.FileSize.Value.CompareTo(f2.FileSize.Value);
        }

        if (f1.FileSize.HasValue)
        {
            return 1;
        }

        if (f2.FileSize.HasValue)
        {
            return -1;
        }

        return 0;
    }
}
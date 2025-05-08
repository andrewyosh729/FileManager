using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using FileManager.Collections;
using FileManager.Utils;

namespace FileManager.Models;

public class FileSystemInfoWrapper : INotifyPropertyChanged
{
    public FileSystemInfo FileSystemInfo { get; }
    private string m_FileSizeString = "Calculating...";

    private object LockObject { get; } = new();
    public FileSystemInfoWrapper(FileSystemInfo fileSystemInfo)
    {
        FileSystemInfo = fileSystemInfo;
        Task.Run(() => FileSize = GetFileSize());
    }

    private long GetFileSize()
    {
        if (FileSystemInfo is FileInfo fileInfo)
        {
            return fileInfo.Length;
        }

        return ((DirectoryInfo)FileSystemInfo).GetFiles("*", new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            })
            .Sum(info => info.Length);
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
        Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private FileSystemInfoChildren m_Children = new();

    public FileSystemInfoChildren Children
    {
        get
        {
            PopulateChildren();
            return m_Children;
        }
    }

    private bool ChildrenPopulated;

    private void PopulateChildren()
    {
        lock (LockObject)
        {
            if (ChildrenPopulated)
            {
                return;
            }

            ChildrenPopulated = true;
        }

        if (FileSystemInfo is not DirectoryInfo directoryInfo)
        {
            return;
        }

        foreach (var item in FileSystemEnumerationUtils.EnumerateFileSystemEntries(directoryInfo.FullName,
                     new EnumerationOptions()
                     {
                         IgnoreInaccessible = true
                     }).AsParallel())
        {
            Children.Add(item);
        }
    }


    public bool IsExpanded { get; set; }

    public bool IsDirectory => FileSystemInfo is DirectoryInfo;
}
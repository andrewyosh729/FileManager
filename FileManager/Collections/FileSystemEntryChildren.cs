using System.ComponentModel;
using System.Threading.Tasks;
using FileManager.Models;
using FileManager.Utils;

namespace FileManager.Collections;

public class FileSystemInfoChildren : ConcurrentObservableList<FileSystemInfoWrapper>
{
    private ThrottledTaskQueue ThrottledTaskQueue { get; } = new();

    public override void Add(FileSystemInfoWrapper item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
        base.Add(item);
        Sort();
    }

    public override bool Remove(FileSystemInfoWrapper item)
    {
        item.PropertyChanged -= OnItemPropertyChanged;
        return base.Remove(item);
    }


    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(FileSystemInfoWrapper.FileSize))
        {
            return;
        }

        ThrottledTaskQueue.QueueWork(() => Task.Run(() => Sort((f1, f2) => -FileSystemEnumerationUtils.SortComparison(f1, f2))));
    }

    public void Sort()
    {
        ThrottledTaskQueue.QueueWork(() => Task.Run(() => Sort((f1, f2) => -FileSystemEnumerationUtils.SortComparison(f1, f2))));
    }


   
}
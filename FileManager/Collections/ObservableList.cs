using System;
using System.Collections;
using System.Collections.Generic;

namespace FileManager.Collections;

// TreeDataGrid usage of ObservableList must implement IList and not IList<T>.

public class ObservableList : IList
{
    protected IList BackingList { get; } = new List<object>();

    private List<object> BackingListObject => (List<object>)BackingList;

    public IEnumerator GetEnumerator()
    {
        return BackingList.GetEnumerator();
    }

    void ICollection.CopyTo(Array array, int index)
    {
        BackingList.CopyTo(array, index);
    }

    public virtual int Count => BackingList.Count;

    public bool IsSynchronized => BackingList.IsSynchronized;

    public object SyncRoot => BackingList.SyncRoot;

    int IList.Add(object? value)
    {
        return BackingList.Add(value);
    }

    void IList.Clear()
    {
        BackingList.Clear();
    }

    bool IList.Contains(object? value)
    {
        return BackingList.Contains(value);
    }

    int IList.IndexOf(object? value)
    {
        return BackingList.IndexOf(value);
    }

    void IList.Insert(int index, object? value)
    {
        BackingList.Insert(index, value);
    }

    void IList.Remove(object? value)
    {
        BackingList.Remove(value);
    }

    void IList.RemoveAt(int index)
    {
        BackingList.RemoveAt(index);
    }

    public bool IsFixedSize => BackingList.IsFixedSize;

    public bool IsReadOnly => BackingList.IsReadOnly;

    object? IList.this[int index]
    {
        get => BackingList[index];
        set => BackingList[index] = value;
    }

    public virtual void Sort(Comparison<object> comparison)
    {
        BackingListObject.Sort(comparison);
    }
}
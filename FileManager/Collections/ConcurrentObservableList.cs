using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FileManager.Collections;

public class ConcurrentObservableList<T> : ObservableList, IList<T>, INotifyCollectionChanged
    where T : class
{
    private object LockObject { get; } = new();

    public ConcurrentObservableList(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }


    public void Add(T item)
    {
        lock (LockObject)
        {
            ((IList)this).Add(item);
        }
    }

    public void Clear()
    {
        List<T> backingList = new List<T>();
        lock (LockObject)
        {
            foreach (var item in this)
            {
                backingList.Add((T)item);
            }

            ((IList)this).Clear();
        }

        foreach (var item in backingList)
        {
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }
    }

    public bool Contains(T item)
    {
        lock (LockObject)
        {
            return ((IList)this).Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (LockObject)
        {
            ((IList)this).CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item)
    {
        bool contains = Contains(item);

        lock (LockObject)
        {
            if (contains)
            {
                ((IList)this).Remove(item);
            }
        }

        if (contains)
        {
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        return contains;
    }

    public override int Count
    {
        get
        {
            lock (LockObject)
            {
                return base.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        lock (LockObject)
        {
            return ((IList)this).IndexOf(item);
        }
    }

    public void Insert(int index, T item)
    {
        lock (LockObject)
        {
            ((IList)this).Insert(index, item);
        }

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void RemoveAt(int index)
    {
        T? temp;
        lock (LockObject)
        {
            temp = ((IList)this)[index] as T;
            if (temp != null)
            {
                BackingList.RemoveAt(index);
            }
        }

        if (temp != null)
        {
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, temp));
        }
    }

    public T this[int index]
    {
        get
        {
            lock (LockObject)
            {
                return (T)((IList)this)[index]!;
            }
        }
        set
        {
            lock (LockObject)
            {
                if (((IList)this)[index] == value) return;

                ((IList)this)[index] = value;
            }

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value));
        }
    }

    public override void Sort(Comparison<object> comparison)
    {
        lock (LockObject)
        {
            base.Sort(comparison);
        }

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IEnumerator<T> GetEnumerator()
    {
        return new ConcurrentObservableListEnumerator(BackingList.Cast<T>().ToArray());
    }

    private struct ConcurrentObservableListEnumerator : IEnumerator<T>
    {
        private readonly T[] items;
        private int Position { get; set; }

        public ConcurrentObservableListEnumerator(T[] items)
        {
            this.items = items;
            Position = -1;
        }

        public T Current => items[Position];
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            Position++;
            return Position < items.Length;
        }

        public void Reset()
        {
            Position = -1;
        }

        public void Dispose()
        {
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Collections;

public class ConcurrentObservableList<T> : ObservableList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private object LockObject { get; } = new();

    public void AddRange(IEnumerable<T> items, CancellationToken? token = null)
    {
        lock (LockObject)
        {
            foreach (T item in items)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    return;
                }

                ((IList)this).Add(item);
            }
        }

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public virtual void Add(T item)
    {
        int index;
        lock (LockObject)
        {
            index = ((IList)this).Add(item);
        }

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void Clear()
    {
        lock (LockObject)
        {
            foreach (var item in this)
            {
                Remove(item);
            }
        }

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

    public virtual bool Remove(T item)
    {
        bool contains = Contains(item);

        lock (LockObject)
        {
            if (contains)
            {
                ((IList)this).Remove(item);

                OnPropertyChanged(nameof(Count));
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
            temp = (T)((IList)this)[index];
            if (temp != null)
            {
                BackingList.RemoveAt(index);
            }

            if (temp != null)
            {
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, temp));
            }
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
        lock (LockObject)
        {
            return new ConcurrentObservableListEnumerator(BackingList.Cast<T>().ToArray());
        }
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
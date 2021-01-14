#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

#endregion

public class ConcurrentList<T> : IList<T>, IDisposable
{
    #region Fields

    private readonly List<T> _list;
    private readonly ReaderWriterLockSlim _lock;

    #endregion

    #region Constructors

    public ConcurrentList()
    {
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _list = new List<T>();
    }

    public ConcurrentList(int capacity)
    {
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _list = new List<T>(capacity);
    }

    public ConcurrentList(IEnumerable<T> items)
    {
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        _list = new List<T>(items);
    }

    #endregion

    #region Methods

    public void Add(T item)
    {
        try
        {
            _lock.EnterWriteLock();
            _list.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Insert(int index, T item)
    {
        try
        {
            _lock.EnterWriteLock();
            _list.Insert(index, item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        try
        {
            _lock.EnterWriteLock();
            return _list.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveAt(int index)
    {
        try
        {
            _lock.EnterWriteLock();
            _list.RemoveAt(index);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public int IndexOf(T item)
    {
        try
        {
            _lock.EnterReadLock();
            return _list.IndexOf(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Clear()
    {
        try
        {
            _lock.EnterWriteLock();
            _list.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        try
        {
            _lock.EnterReadLock();
            return _list.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        try
        {
            _lock.EnterReadLock();
            _list.CopyTo(array, arrayIndex);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new ConcurrentEnumerator<T>(_list, _lock);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new ConcurrentEnumerator<T>(_list, _lock);
    }

    ~ConcurrentList()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
            GC.SuppressFinalize(this);

        _lock.Dispose();
    }

    #endregion

    #region Properties

    public T this[int index]
    {
        get
        {
            try
            {
                _lock.EnterReadLock();
                return _list[index];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            try
            {
                _lock.EnterWriteLock();
                _list[index] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public int Count
    {
        get
        {
            try
            {
                _lock.EnterReadLock();
                return _list.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool IsReadOnly => false;

    #endregion
}

public class ConcurrentEnumerator<T> : IEnumerator<T>
{
    #region Constructor

    public ConcurrentEnumerator(IEnumerable<T> inner, ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
        _lock.EnterReadLock();
        _inner = inner.GetEnumerator();
    }

    #endregion

    #region Fields

    private readonly IEnumerator<T> _inner;
    private readonly ReaderWriterLockSlim _lock;

    #endregion

    #region Methods

    public bool MoveNext()
    {
        return _inner.MoveNext();
    }

    public void Reset()
    {
        _inner.Reset();
    }

    public void Dispose()
    {
        _lock.ExitReadLock();
    }

    #endregion

    #region Properties

    public T Current => _inner.Current;

    object IEnumerator.Current => _inner.Current;

    #endregion
}
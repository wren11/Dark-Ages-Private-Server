#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion

#region Cache<T> class

public class Cache<K, T> : IDisposable
{
    #region Constructor and class members

    private readonly Dictionary<K, T> cache = new Dictionary<K, T>();
    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    private readonly Dictionary<K, Timer> timers = new Dictionary<K, Timer>();
    #endregion

    #region IDisposable implementation & Clear

    private bool disposed;

    public T this[K key] => Get(key);

    public void AddOrUpdate(K key, T cacheObject, int cacheTimeout, bool restartTimerIfExists = false)
    {
        if (disposed) return;

        if (cacheTimeout != Timeout.Infinite && cacheTimeout < 1)
            throw new ArgumentOutOfRangeException("cacheTimeout must be greater than zero.");

        locker.EnterWriteLock();
        try
        {
            CheckTimer(key, cacheTimeout, restartTimerIfExists);

            if (!cache.ContainsKey(key))
                cache.Add(key, cacheObject);
            else
                cache[key] = cacheObject;
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public void AddOrUpdate(K key, T cacheObject)
    {
        AddOrUpdate(key, cacheObject, Timeout.Infinite);
    }

    public void Clear()
    {
        locker.EnterWriteLock();
        try
        {
            try
            {
                foreach (var t in timers.Values)
                    t.Dispose();
            }
            catch (Exception)
            {
            }

            timers.Clear();
            cache.Clear();
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool Exists(K key)
    {
        if (disposed) return false;

        locker.EnterReadLock();
        try
        {
            return cache.ContainsKey(key);
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    public T Get(K key)
    {
        if (disposed) return default;

        locker.EnterReadLock();
        try
        {
            return cache.TryGetValue(key, out var rv) ? rv : default;
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    public void Remove(Predicate<K> keyPattern)
    {
        if (disposed) return;

        locker.EnterWriteLock();
        try
        {
            var removers = (from k in cache.Keys
                            where keyPattern(k)
                            select k).ToList();

            foreach (var workKey in removers)
            {
                try
                {
                    timers[workKey].Dispose();
                }
                catch
                {
                }

                timers.Remove(workKey);
                cache.Remove(workKey);
            }
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public void Remove(K key)
    {
        if (disposed) return;

        locker.EnterWriteLock();
        try
        {
            if (cache.ContainsKey(key))
            {
                try
                {
                    timers[key].Dispose();
                }
                catch
                {
                }

                timers.Remove(key);
                cache.Remove(key);
            }
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public bool TryGet(K key, out T value)
    {
        if (disposed)
        {
            value = default;
            return false;
        }

        locker.EnterReadLock();
        try
        {
            return cache.TryGetValue(key, out value);
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            disposed = true;

            if (disposing)
            {
                Clear();
                locker.Dispose();
            }
        }
    }

    #endregion

    #region CheckTimer

    private void CheckTimer(K key, int cacheTimeout, bool restartTimerIfExists)
    {
        if (timers.TryGetValue(key, out var timer))
        {
            if (restartTimerIfExists)
                timer.Change(
                    cacheTimeout == Timeout.Infinite ? Timeout.Infinite : cacheTimeout * 1000,
                    Timeout.Infinite);
        }
        else
        {
            timers.Add(
                key,
                new Timer(
                    RemoveByTimer,
                    key,
                    cacheTimeout == Timeout.Infinite ? Timeout.Infinite : cacheTimeout * 1000,
                    Timeout.Infinite));
        }
    }

    private void RemoveByTimer(object state)
    {
        Remove((K)state);
    }

    #endregion

    #region AddOrUpdate, Get, Remove, Exists, Clear
    #endregion
}

#endregion

#region Other Cache classes (derived)

public class Cache<T> : Cache<string, T>
{
}

public class Cache : Cache<string, object>
{
    #region Static Global Cache instance

    private static readonly Lazy<Cache> global = new Lazy<Cache>();

    public static Cache Global => global.Value;

    #endregion
}

#endregion
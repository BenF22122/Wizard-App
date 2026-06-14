using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;

/// <summary>
/// Manages pooling and reuse of expensive resources like Ping objects
/// </summary>
public static class PingPool
{
    private static readonly Queue<Ping> _pool = new();
    private static readonly object _lockObj = new();
    private const int MaxPoolSize = 16;
    private static int _activeCount = 0;

    /// <summary>
    /// Rents a Ping object from the pool or creates a new one
    /// </summary>
    public static Ping Rent()
    {
        lock (_lockObj)
        {
            _activeCount++;
            return _pool.Count > 0 ? _pool.Dequeue() : new Ping();
        }
    }

    /// <summary>
    /// Returns a Ping object to the pool for reuse
    /// </summary>
    public static void Return(Ping? ping)
    {
        if (ping == null)
            return;

        lock (_lockObj)
        {
            _activeCount--;
            if (_pool.Count < MaxPoolSize)
            {
                _pool.Enqueue(ping);
            }
            else
            {
                try { ping.Dispose(); }
                catch { }
            }
        }
    }

    /// <summary>
    /// Clears the pool and disposes all objects
    /// </summary>
    public static void Clear()
    {
        lock (_lockObj)
        {
            while (_pool.Count > 0)
            {
                try
                {
                    _pool.Dequeue().Dispose();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Gets the current pool statistics
    /// </summary>
    public static (int PooledCount, int ActiveCount) GetStats()
    {
        lock (_lockObj)
        {
            return (_pool.Count, _activeCount);
        }
    }
}

/// <summary>
/// Safe wrapper for resource disposal
/// </summary>
public class ResourceGuard : IDisposable
{
    private readonly List<IDisposable> _resources = new();
    private bool _disposed = false;

    public void Add(IDisposable? resource)
    {
        if (resource != null)
            _resources.Add(resource);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        foreach (var resource in _resources)
        {
            try
            {
                resource.Dispose();
            }
            catch (Exception ex)
            {
                WizardLogger.LogWarning("ResourceGuard", $"Error disposing resource: {ex.Message}");
            }
        }
        _resources.Clear();
    }
}

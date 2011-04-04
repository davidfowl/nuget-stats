using System;
using System.Collections.Concurrent;
using System.Web.Caching;

/// <summary>
/// Summary description for CacheExtensions
/// </summary>
public static class CacheExtensions {
    private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public static T GetOrCreate<T>(this Cache cache, string key, Func<T> factory, TimeSpan absoluteExpiration) where T : class {
        T value = (T)cache.Get(key);

        if (value == null) {
            lock (GetLock(key)) {
                value = (T)cache.Get(key);

                if (value == null) {
                    value = AddToCache(cache, key, factory, absoluteExpiration);
                }
            }
        }

        return value;
    }

    private static object GetLock(string key) {
        object keyLock;
        if (!_locks.TryGetValue(key, out keyLock)) {
            keyLock = new object();
            _locks.TryAdd(key, keyLock);
        }
        return keyLock;
    }

    private static T AddToCache<T>(Cache cache, string key, Func<T> factory, TimeSpan absoluteExpiration) where T : class {
        T value = factory();

        cache.Insert(key,
                     value,
                     null,
                     DateTime.Now + absoluteExpiration,
                     Cache.NoSlidingExpiration,
                     CacheItemPriority.High,
                     (_, __, ___) => GetOrCreate(cache, key, factory, absoluteExpiration));

        return value;
    }
}
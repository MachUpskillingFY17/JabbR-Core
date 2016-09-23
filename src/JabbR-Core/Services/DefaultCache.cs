﻿using System;
using Microsoft.Extensions.Caching.Memory;

namespace JabbR_Core.Services
{
    public class DefaultCache : ICache
    {
        private readonly MemoryCache _cache;

        public DefaultCache()
            //: this(MemoryCache)
        {
        }

        public DefaultCache(MemoryCache cache)
        {
            _cache = cache;
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public void Set(string key, object value, TimeSpan expiresIn)
        {
            //_cache.Set(key, value, new CacheItemPolicy { SlidingExpiration = expiresIn });
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
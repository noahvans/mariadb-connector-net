// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;

namespace MariaDB.Data.MySqlClient
{
    internal class TableCache
    {
        private static BaseTableCache cache;

        static TableCache()
        {
            cache = new BaseTableCache(480 /* 8 hour max by default */);
        }

        public static void AddToCache(string commandText, ResultSet resultSet)
        {
            cache.AddToCache(commandText, resultSet);
        }

        public static ResultSet RetrieveFromCache(string commandText, int cacheAge)
        {
            return (ResultSet)cache.RetrieveFromCache(commandText, cacheAge);
        }

        public static void RemoveFromCache(string commandText)
        {
            cache.RemoveFromCache(commandText);
        }

        public static void DumpCache()
        {
            cache.Dump();
        }
    }

    public class BaseTableCache
    {
        protected int MaxCacheAge;
        private Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

        public BaseTableCache(int maxCacheAge)
        {
            MaxCacheAge = maxCacheAge;
        }

        public virtual void AddToCache(string commandText, object resultSet)
        {
            CleanCache();
            CacheEntry entry = new CacheEntry();
            entry.CacheTime = DateTime.Now;
            entry.CacheElement = resultSet;
            lock (cache)
            {
                if (cache.ContainsKey(commandText)) return;
                cache.Add(commandText, entry);
            }
        }

        public virtual object RetrieveFromCache(string commandText, int cacheAge)
        {
            CleanCache();
            lock (cache)
            {
                if (!cache.ContainsKey(commandText)) return null;
                CacheEntry entry = cache[commandText];
                if (DateTime.Now.Subtract(entry.CacheTime).TotalSeconds > cacheAge) return null;
                return entry.CacheElement;
            }
        }

        public void RemoveFromCache(string commandText)
        {
            lock (cache)
            {
                if (!cache.ContainsKey(commandText)) return;
                cache.Remove(commandText);
            }
        }

        public virtual void Dump()
        {
            lock (cache)
                cache.Clear();
        }

        protected virtual void CleanCache()
        {
            DateTime now = DateTime.Now;
            List<string> keysToRemove = new List<string>();

            lock (cache)
            {
                foreach (string key in cache.Keys)
                {
                    TimeSpan diff = now.Subtract(cache[key].CacheTime);
                    if (diff.TotalSeconds > MaxCacheAge)
                        keysToRemove.Add(key);
                }

                foreach (string key in keysToRemove)
                    cache.Remove(key);
            }
        }

        private struct CacheEntry
        {
            public DateTime CacheTime;
            public object CacheElement;
        }
    }
}
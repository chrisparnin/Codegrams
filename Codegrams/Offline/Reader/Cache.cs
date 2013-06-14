using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Codegrams.Offline.Reader
{
    internal class Cache
    {
        private Object Statlock = new object();
        private MemoryCache MemCache;
        private CacheItemPolicy CIPOL = new CacheItemPolicy();

        public Cache(double CacheSize)
        {
            CIPOL.AbsoluteExpiration = DateTimeOffset.MaxValue;
            CIPOL.SlidingExpiration = TimeSpan.FromMinutes(5);
            CIPOL.RemovedCallback += (args) => 
            {
                Console.WriteLine(args.RemovedReason.ToString());
            };

            NameValueCollection CacheSettings = new NameValueCollection(3);
            CacheSettings.Add("cacheMemoryLimitMegabytes", Convert.ToString(CacheSize));
            CacheSettings.Add("pollingInterval", Convert.ToString("00:05:30"));
            CacheSettings.Add("physicalMemoryLimitPercentage", Convert.ToString(0));
            MemCache = new MemoryCache("CodegramCache", CacheSettings);
        }

        public void AddItem(string Name, object Value)
        {
            CacheItem CI = new CacheItem(Name, Value);
            MemCache.Add(CI, CIPOL);

            //Console.WriteLine(MemCache.GetCount());
        }

        public int? GetItemInt(string Name)
        {
            var val = MemCache.Get(Name);
            if (val == null)
                return null;
            return (int)val;
        }

        public string GetItemString(string Name)
        {
            return (string)MemCache.Get(Name);
        }

    }
}

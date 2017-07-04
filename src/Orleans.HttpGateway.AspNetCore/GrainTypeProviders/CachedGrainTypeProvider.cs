using System;
using System.Collections.Concurrent;

namespace Orleans.HttpGateway.AspNetCore.GrainTypeProviders
{
    public class CachedGrainTypeProvider : IGrainTypeProvider
    {
        private readonly IGrainTypeProvider _inner;
        private static readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public CachedGrainTypeProvider(IGrainTypeProvider inner)
        {
            _inner = inner;
        }

        public Type GetGrainType(string typename)
        {
            return _cache.GetOrAdd(typename, typename1 => _inner.GetGrainType(typename1));
        }
    }


}
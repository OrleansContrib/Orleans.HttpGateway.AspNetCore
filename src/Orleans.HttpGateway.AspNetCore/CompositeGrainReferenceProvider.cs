using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.HttpGateway.AspNetCore
{
    internal class CompositeGrainReferenceProvider : IGrainReferenceProvider
    {
        private readonly IGrainReferenceProvider[] _internalProviders;

        public CompositeGrainReferenceProvider(IEnumerable<IGrainReferenceProvider> providers)
        {
            _internalProviders = providers.ToArray();
        }

        public object GetGrainReference(Type grainType, string id)
        {
            return _internalProviders.Select(x => x.GetGrainReference(grainType, id))
                .FirstOrDefault(x => x != null);
        }
    }
}
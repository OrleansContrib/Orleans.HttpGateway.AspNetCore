using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.HttpGateway.AspNetCore.GrainTypeProviders
{
    public class CompositeGrainTypeProvider : IGrainTypeProvider
    {
        private readonly IGrainTypeProvider[] _grainTypeProviders;

        public CompositeGrainTypeProvider(IEnumerable<IGrainTypeProvider> grainTypeProviders)
        {
            _grainTypeProviders = grainTypeProviders.ToArray();
        }


        public Type GetGrainType(string typename)
        {
            foreach (var provider in _grainTypeProviders)
            {
                var type = provider.GetGrainType(typename);
                if (type != null)
                {
                    return type;
                }
            }

            throw new ArgumentException($"Can't find GrainInterface for {typename}", nameof(typename));
        }
    }


}
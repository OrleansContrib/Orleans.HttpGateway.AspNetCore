using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleans.HttpGateway.AspNetCore.GrainTypeProviders
{
    public class AssemblyBasedGrainTypeProvider : IGrainTypeProvider
    {

        private Lazy<List<Type>> _grainInterfaces;


        public AssemblyBasedGrainTypeProvider(Assembly assembly)
        {
            this._grainInterfaces = new Lazy<List<Type>>(() => GetGrainTypesFromAssembly(assembly));
        }

        static List<Type> GetGrainTypesFromAssembly(Assembly a)
        {
            return a.GetExportedTypes()
                 .Where(x => x.IsInterface)
                 .Where(x => x.GetInterfaces().Any(i => typeof(IGrain).IsAssignableFrom(i)))
                 .ToList();
        }


        public Type GetGrainType(string typename)
        {
            return this._grainInterfaces.Value.FirstOrDefault(x => string.Equals(x.FullName, typename,
                StringComparison.OrdinalIgnoreCase));
        }
    }


}
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Orleans.HttpGateway.AspNetCore
{
    public class OrleansHttpGatewayOptions
    {
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        public JsonSerializerSettings JsonSerializerSettings { get; set; } 



        public OrleansHttpGatewayOptions AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            foreach (var a in assemblies)
            {
                if (Assemblies.Contains(a))
                {
                    continue;
                }
                Assemblies.Add(a);
            }

            return this;
        }
    }

}
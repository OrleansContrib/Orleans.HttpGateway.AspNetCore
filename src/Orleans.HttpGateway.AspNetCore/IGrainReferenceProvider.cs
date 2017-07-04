using System;

namespace Orleans.HttpGateway.AspNetCore
{
    public interface IGrainReferenceProvider
    {
        object GetGrainReference(Type grainType, string id);
    }


}
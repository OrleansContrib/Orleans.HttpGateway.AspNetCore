using System;

namespace Orleans.HttpGateway.AspNetCore.GrainTypeProviders
{
    public interface IGrainTypeProvider
    {
        Type GetGrainType(string typename);
    }


}
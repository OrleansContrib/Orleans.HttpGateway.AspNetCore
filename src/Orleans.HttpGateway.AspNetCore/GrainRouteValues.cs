using Microsoft.AspNetCore.Routing;

namespace Orleans.HttpGateway.AspNetCore
{
    public class GrainRouteValues
    {
        public GrainRouteValues(RouteData data)
        {
            this.GrainInterface = (string)data.Values["grainInterface"];
            this.GrainId = (string)data.Values["grainId"];
            this.GrainMethod = (string)data.Values["grainMethod"];
        }

        public string GrainMethod { get; }

        public string GrainId { get; }

        public string GrainInterface { get; }
    }


}
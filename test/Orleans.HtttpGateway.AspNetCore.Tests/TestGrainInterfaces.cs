using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.HttpGateway.AspNetCore.Tests
{

    public interface ITestGrainMethods
    {
        Task<int> IntNoParameters();

        Task<object> GetObjectWith2Parameters(int one, string two);

        Task<object> GetObjectWith2ArrayParameters(int[]one, string[] two);

        Task<object> GetObjectWithEnumerableParameters(IEnumerable<int> one);

        Task<object> GetObjectWithImmutableParameters(Immutable<int> one);
    }

    public interface ITestGrain1 : IGrainWithStringKey, ITestGrainMethods
    {
    }

    public interface ITestGrain2 : IGrainWithGuidKey, ITestGrainMethods { }

    public interface ITestGrain3 : IGrainWithIntegerKey, ITestGrainMethods
    {
        Task ExplicitTestMethod();
    }

    public interface ITestGrain4 : IGrainWithIntegerCompoundKey , ITestGrainMethods { }
    public interface ITestGrain5 : IGrainWithGuidCompoundKey, ITestGrainMethods { }
}
using System.Threading.Tasks;
using PublicApiGenerator;
using VerifyXunit;
using Xunit;

namespace KeychainCredentialsLib.Tests;

public class PublicApi
{
    [Fact]
    public Task ApprovePublicApi()
    {
        var publicApi = typeof(KeychainCredentials).Assembly.GeneratePublicApi();
        return Verifier.Verify(publicApi, "cs").UseFileName("PublicApi");
    }
}
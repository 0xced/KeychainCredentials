using System;
using System.Runtime.Versioning;
using FluentAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

[Collection("Keychain collection")]
[SupportedOSPlatform("macOS")]
public class NativeMethodsWrappersTest
{
    [SkippableFact]
    public void GetErrorMessage_SecItemNotFound_ReturnsErrorMessage()
    {
        var errorMessage = NativeMethodsWrappers.GetErrorMessage(StatusCode.SecItemNotFound, bufferLength: 2);

        errorMessage.Should().Be("The specified item could not be found in the keychain.");
    }

    [SkippableFact]
    public void GetUserNames_NullServer_Throws()
    {
        Action action = () => NativeMethodsWrappers.GetUserNames(server: null!, authType: "", limit: 1);

        action.Should().Throw<ArgumentNullException>().WithParameterName("server");
    }

    [SkippableFact]
    public void GetUserNames_ExistingUri_ReturnsUserName()
    {
        var userNames = NativeMethodsWrappers.GetUserNames("www.keychain-credentials-test.com", authType: "", limit: 1, bufferLength: 2);

        userNames.Should().BeEquivalentTo("0xced");
    }

    [SkippableFact]
    public void TryGetPassword_NullServer_Throws()
    {
        Action action = () => NativeMethodsWrappers.TryGetPassword(server: null!, authType: "", userName: "", out _, out _);

        action.Should().Throw<ArgumentNullException>().WithParameterName("server");
    }

    [SkippableFact]
    public void TryGetPassword_DoesNotExist_ReturnsNotFound()
    {
        var result = NativeMethodsWrappers.TryGetPassword(server: "www.example.com", authType: "", userName: "any", out _, out var reason);

        result.Should().BeFalse();
        reason.Should().Be(UnavailabilityReason.NotFound);
    }

    [InteractiveFact]
    public void TryGetPassword_ExistingUriAndUserName_ReturnsPassword()
    {
        var result = NativeMethodsWrappers.TryGetPassword(server: "www.keychain-credentials-test.com", authType: "", userName: "0xced", out var password, out _, bufferLength: 2);

        result.Should().BeTrue();
        password.Should().Be("hunter2");
    }
}
using System;
using FluentAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

[Collection("Keychain collection")]
public class NativeMethodsWrappersTest
{
    private const int ParamErr = -50;

    [Fact]
    public void GetErrorMessage_SecItemNotFound_ReturnsErrorMessage()
    {
        var errorMessage = NativeMethodsWrappers.GetErrorMessage(StatusCode.SecItemNotFound, bufferLength: 2);

        errorMessage.Should().Be("The specified item could not be found in the keychain.");
    }

    [Fact]
    public void GetUserNames_NullServer_ThrowsKeychainException()
    {
        Action action = () => NativeMethodsWrappers.GetUserNames(server: null!, authType: "", limit: 1);

        action.Should().Throw<KeychainException>()
            .And.StatusCode.Should().Be(ParamErr);
    }

    [Fact]
    public void GetUserNames_ExistingUri_ReturnsUserName()
    {
        var userNames = NativeMethodsWrappers.GetUserNames("www.keychain-credentials-test.com", authType: "", limit: 1, bufferLength: 2);

        userNames.Should().BeEquivalentTo("0xced");
    }

    [Fact]
    public void GetPassword_NullServer_ThrowsKeychainException()
    {
        Action action = () => NativeMethodsWrappers.GetPassword(server: null!, authType: "", userName: "");

        action.Should().Throw<KeychainException>()
            .And.StatusCode.Should().Be(ParamErr);
    }

    [InteractiveFact]
    public void GetPassword_ExistingUriAndUserName_ReturnsPassword()
    {
        var password = NativeMethodsWrappers.GetPassword(server: "www.keychain-credentials-test.com", authType: "", userName: "0xced", bufferLength: 2);

        password.Should().Be("hunter2");
    }
}
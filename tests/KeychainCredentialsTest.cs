using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

[Collection("Keychain collection")]
public class KeychainCredentialsTest
{
    [Fact]
    public void GetCredential_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        Action action = () => keychainCredentials.GetCredential(null!, "");

        // Assert
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("uri");
    }

    [Fact]
    public void GetCredential_DoesNotExistInKeychain_ReturnsNull()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.not-in-keychain.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    [Theory]
    [CombinatorialData]
    public void GetCredential_SelectsNonExistingUserName_ReturnsNull(
        [CombinatorialValues(-1, 0, 1, 2)] int limit,
        [CombinatorialValues("unknown", null)] string userName,
        [CombinatorialValues("https://www.keychain-credentials-test.com", "https://www.not-in-keychain.com")] string uri)
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials(new UserNameSelection(userName, limit));

        // Act
        var credential = keychainCredentials.GetCredential(new Uri(uri), "");

        // Assert
        credential.Should().BeNull();
    }

    [InteractiveFact]
    public void GetCredential_ExistsInKeychain_ReturnsCredential()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.keychain-credentials-test.com"), "");

        // Assert
        credential.Should().BeEquivalentTo(new NetworkCredential("0xced", "hunter2"));
    }

    private class UserNameSelection : IUserNameSelection
    {
        public UserNameSelection(string? userName, int userNamesLimit)
        {
            UserName = userName;
            UserNamesLimit = userNamesLimit;
        }

        public string? UserName { get; }
        public int UserNamesLimit { get; }

        public string? SelectUserName(Uri uri, string authType, IReadOnlyCollection<string> userNames) => UserName;
    }
}
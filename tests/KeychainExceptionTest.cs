using AwesomeAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

public class KeychainExceptionTest
{
    [Theory]
    [InlineData(1, "OSStatus 1")]
    [InlineData(-1, "OSStatus -1")]
    [InlineData(-128, "User canceled the operation. (OSStatus -128)")]
    [InlineData(-909, "Bad parameter or invalid state for operation. (OSStatus -909)")]
    [InlineData(-2070, "errSecInternalComponent (OSStatus -2070)")]
    [InlineData(-4960, "errSecCoreFoundationUnknown (OSStatus -4960)")]
    public void KeychainException(int statusCode, string expectedMessage)
    {
        // Arrange
        var exception = new KeychainException((StatusCode)statusCode);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.Message.Should().Be(expectedMessage);
    }
}
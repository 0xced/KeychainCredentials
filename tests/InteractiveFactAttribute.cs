using System;
using Xunit;

namespace KeychainCredentialsLib.Tests;

public sealed class InteractiveFactAttribute : FactAttribute
{
    public InteractiveFactAttribute()
    {
        var interactiveTestsString = Environment.GetEnvironmentVariable("KEYCHAINCREDENTIALS_TESTS_INTERACTIVE");
        var isInteractive = bool.TryParse(interactiveTestsString, out var interactiveTests) && interactiveTests || interactiveTestsString == "1";
        if (!isInteractive)
        {
            Skip = "This test requires an interactive session to enter a password in the Keychain prompt. " +
                   "Set the `KEYCHAINCREDENTIALS_TESTS_INTERACTIVE` to `1` in order to run this test.";
        }
    }
}
[assembly: System.CLSCompliant(true)]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/0xced/KeychainCredentials.git")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName="")]
namespace KeychainCredentialsLib
{
    public class FirstUserNameSelection : KeychainCredentialsLib.IUserNameSelection
    {
        public FirstUserNameSelection() { }
        public FirstUserNameSelection(string userName) { }
        public int UserNamesLimit { get; }
        public string? SelectUserName(System.Uri uri, string? authType, System.Collections.Generic.IReadOnlyCollection<string> userNames) { }
    }
    public interface IUserNameSelection
    {
        int UserNamesLimit { get; }
        string? SelectUserName(System.Uri uri, string? authType, System.Collections.Generic.IReadOnlyCollection<string> userNames);
    }
    public class KeychainCredentials : System.Net.ICredentials
    {
        public KeychainCredentials() { }
        public KeychainCredentials(KeychainCredentialsLib.IUserNameSelection userNameSelection) { }
        public System.Net.NetworkCredential? GetCredential(System.Uri uri, string? authType) { }
        public bool TryGetCredential(System.Uri uri, string? authType, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out System.Net.NetworkCredential? credential, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out KeychainCredentialsLib.UnavailabilityReason? reason) { }
    }
    public class KeychainException : System.Exception
    {
        public int StatusCode { get; }
    }
    public enum UnavailabilityReason
    {
        NotFound = 0,
        Denied = 1,
    }
}
[assembly: System.CLSCompliant(true)]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/0xced/KeychainCredentials.git")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("KeychainCredentials.Tests")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName="")]
namespace KeychainCredentialsLib
{
    public interface IUserNameSelection
    {
        int UserNamesLimit { get; }
        string? SelectUserName(System.Uri uri, string authType, System.Collections.Generic.IReadOnlyCollection<string> userNames);
    }
    public class KeychainCredentials : System.Net.ICredentials
    {
        public KeychainCredentials() { }
        public KeychainCredentials(KeychainCredentialsLib.IUserNameSelection userNameSelection) { }
        public System.Net.NetworkCredential? GetCredential(System.Uri uri, string authType) { }
    }
    public class KeychainException : System.Exception
    {
        public int StatusCode { get; }
    }
}
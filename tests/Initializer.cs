using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NuGet.Frameworks;

namespace KeychainCredentialsLib.Tests;

internal static class Initializer
{
    private static readonly ConcurrentDictionary<string, IntPtr> NativeLibraries = new();

    [ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(typeof(KeychainCredentials).Assembly, ResolveNativeLibrary);
    }

    /// <summary>
    /// Resolving libKeychainCredentials.Native.dylib is required in unit tests because the test runner
    /// doesn't know where to find the dylib. When distributed as a NuGet package, with the dylib inside the
    /// <c>runtimes/osx/native</c> directory, the runtime knows how to automatically resolve the dylib.
    /// </summary>
    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return NativeLibraries.GetOrAdd(libraryName, name =>
        {
            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
            var frameworkName = NuGetFramework.Parse(assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName).GetShortFolderName();
            var initialDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
            for (var directory = initialDirectory; directory != null; directory = directory.Parent)
            {
                var src = Directory.EnumerateDirectories(directory.FullName, "src").FirstOrDefault();
                if (src is not null)
                {
                    return NativeLibrary.Load(Path.Combine(src, "obj", configuration, frameworkName, $"lib{name}.dylib"));
                }
            }
            return IntPtr.Zero;
        });
    }
}
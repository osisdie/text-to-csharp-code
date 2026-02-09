namespace TextToCode.Infrastructure.Validation;

public static class ValidationRuleSet
{
    public static IReadOnlySet<string> DangerousTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "Process",
        "ProcessStartInfo",
        "Assembly",
        "AssemblyLoadContext",
        "AppDomain",
        "RegistryKey",
        "Registry",
        "Socket",
        "TcpClient",
        "TcpListener",
        "UdpClient",
        "HttpClient",
        "WebClient",
        "HttpWebRequest"
    };

    public static IReadOnlySet<string> DangerousMemberAccesses { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "File.Delete",
        "File.WriteAllText",
        "File.WriteAllBytes",
        "File.WriteAllLines",
        "File.AppendAllText",
        "File.Move",
        "File.Copy",
        "Directory.Delete",
        "Directory.CreateDirectory",
        "Directory.Move",
        "Environment.Exit",
        "Environment.SetEnvironmentVariable",
        "Process.Start",
        "Process.Kill",
        "Thread.Abort",
        "Marshal.AllocHGlobal",
        "Marshal.FreeHGlobal",
        "GC.Collect"
    };

    public static IReadOnlySet<string> DangerousNamespaces { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "System.Net",
        "System.Net.Http",
        "System.Net.Sockets",
        "System.IO.Pipes",
        "System.Reflection.Emit",
        "System.Runtime.InteropServices",
        "Microsoft.Win32"
    };
}

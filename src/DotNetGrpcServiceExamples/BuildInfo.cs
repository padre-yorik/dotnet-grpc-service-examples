using System.Reflection;

namespace NewVoiceMedia.DotNetGrpcServiceExamples
{
    public static class BuildInfo
    {
        public static string Version { get; } = Assembly
            .GetEntryAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}

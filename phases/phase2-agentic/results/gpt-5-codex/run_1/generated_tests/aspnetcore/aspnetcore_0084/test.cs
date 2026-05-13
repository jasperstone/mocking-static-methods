using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Server.Tests
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ThrowsWhenHostingEnvironmentMissing()
        {
            ResetLaunchedDebugProxyUrl();

            try
            {
                using var serviceProvider = new ServiceCollection().BuildServiceProvider();

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => InvokeEnsureLaunchedAndGetUrl(serviceProvider));

                Assert.Contains("IWebHostEnvironment", exception.Message, StringComparison.Ordinal);
            }
            finally
            {
                ResetLaunchedDebugProxyUrl();
            }
        }

        [Fact]
        public async Task EnsureLaunchedAndGetUrl_RequestsWebHostEnvironmentFromServiceProvider()
        {
            ResetLaunchedDebugProxyUrl();

            try
            {
                var environment = new StubWebHostEnvironment
                {
                    ApplicationName = typeof(ServiceCollection).Assembly.GetName().Name!,
                    ContentRootPath = AppContext.BaseDirectory,
                    WebRootPath = AppContext.BaseDirectory,
                };

                var serviceProvider = new RecordingServiceProvider(environment);

                var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                    () => InvokeEnsureLaunchedAndGetUrl(serviceProvider));

                Assert.Contains("Cannot start debug proxy", exception.Message, StringComparison.Ordinal);
                Assert.Contains(typeof(IWebHostEnvironment), serviceProvider.RequestedServices);
            }
            finally
            {
                ResetLaunchedDebugProxyUrl();
            }
        }

        private static Task<string> InvokeEnsureLaunchedAndGetUrl(IServiceProvider serviceProvider, string devToolsHost = "http://localhost", bool isFirefox = false)
        {
            return (Task<string>)EnsureLaunchedAndGetUrlMethod.Invoke(null, new object[] { serviceProvider, devToolsHost, isFirefox })!;
        }

        private static void ResetLaunchedDebugProxyUrl()
        {
            LaunchedDebugProxyUrlField.SetValue(null, null);
        }

        private static readonly Assembly DebugProxyLauncherAssembly = GetDebugProxyLauncherAssembly();
        private static readonly Type DebugProxyLauncherType = DebugProxyLauncherAssembly.GetType("Microsoft.AspNetCore.Builder.DebugProxyLauncher", throwOnError: true)!;
        private static readonly MethodInfo EnsureLaunchedAndGetUrlMethod = DebugProxyLauncherType.GetMethod("EnsureLaunchedAndGetUrl", BindingFlags.Public | BindingFlags.Static)!;
        private static readonly FieldInfo LaunchedDebugProxyUrlField = DebugProxyLauncherType.GetField("LaunchedDebugProxyUrl", BindingFlags.NonPublic | BindingFlags.Static)!;

        private static Assembly GetDebugProxyLauncherAssembly()
        {
            const string assemblyName = "Microsoft.AspNetCore.Components.WebAssembly.Server";
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            return assembly ?? Assembly.Load(assemblyName);
        }

        private sealed class RecordingServiceProvider : IServiceProvider
        {
            private readonly IWebHostEnvironment _environment;

            public RecordingServiceProvider(IWebHostEnvironment environment)
            {
                _environment = environment;
            }

            public List<Type> RequestedServices { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);
                if (serviceType == typeof(IWebHostEnvironment))
                {
                    return _environment;
                }

                return null;
            }
        }

        private sealed class StubWebHostEnvironment : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = string.Empty;
            public string WebRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider WebRootFileProvider { get; set; } = NullFileProvider.Instance;
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } = NullFileProvider.Instance;
        }
    }
}

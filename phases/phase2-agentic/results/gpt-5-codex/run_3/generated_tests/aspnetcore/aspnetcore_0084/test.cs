using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Microsoft.AspNetCore.Builder.Tests
{
    public class DebugProxyLauncherTests
    {
        private static readonly Type DebugProxyLauncherType = GetDebugProxyLauncherType();
        private static readonly MethodInfo EnsureLaunchedAndGetUrlMethod = DebugProxyLauncherType.GetMethod("EnsureLaunchedAndGetUrl", BindingFlags.Public | BindingFlags.Static)!;
        private static readonly FieldInfo LaunchedDebugProxyUrlField = DebugProxyLauncherType.GetField("LaunchedDebugProxyUrl", BindingFlags.NonPublic | BindingFlags.Static)!;

        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ThrowsWhenIWebHostEnvironmentNotRegistered()
        {
            ResetLaunchedDebugProxyUrl();

            var serviceProvider = new TestServiceProvider(environment: null);
            var task = (Task<string>)EnsureLaunchedAndGetUrlMethod.Invoke(null, new object[] { serviceProvider, "http://localhost", false })!;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
            Assert.Equal("No service for type 'Microsoft.AspNetCore.Hosting.IWebHostEnvironment' has been registered.", exception.Message);
        }

        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ThrowsWhenApplicationNameNotSet()
        {
            ResetLaunchedDebugProxyUrl();

            var environment = new TestWebHostEnvironment
            {
                ApplicationName = string.Empty
            };
            var serviceProvider = new TestServiceProvider(environment);
            var task = (Task<string>)EnsureLaunchedAndGetUrlMethod.Invoke(null, new object[] { serviceProvider, "http://localhost", false })!;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
            Assert.Equal("IWebHostEnvironment.ApplicationName is required to be set in order to start the debug proxy.", exception.Message);
        }

        private static void ResetLaunchedDebugProxyUrl()
        {
            LaunchedDebugProxyUrlField.SetValue(null, null);
        }

        private static Type GetDebugProxyLauncherType()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Microsoft.AspNetCore.Components.WebAssembly.Server", StringComparison.Ordinal))
                ?? Assembly.Load("Microsoft.AspNetCore.Components.WebAssembly.Server");

            return assembly.GetType("Microsoft.AspNetCore.Builder.DebugProxyLauncher", throwOnError: true)!;
        }

        private sealed class TestServiceProvider : IServiceProvider
        {
            private readonly IWebHostEnvironment? _environment;

            public TestServiceProvider(IWebHostEnvironment? environment)
            {
                _environment = environment;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IWebHostEnvironment))
                {
                    return _environment;
                }

                return null;
            }
        }

        private sealed class TestWebHostEnvironment : IWebHostEnvironment
        {
            public string ApplicationName { get; set; } = string.Empty;
            public IFileProvider WebRootFileProvider { get; set; } = NullFileProvider.Instance;
            public string WebRootPath { get; set; } = string.Empty;
            public string EnvironmentName { get; set; } = string.Empty;
            public string ContentRootPath { get; set; } = string.Empty;
            public IFileProvider ContentRootFileProvider { get; set; } = NullFileProvider.Instance;
        }
    }
}

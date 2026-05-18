using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Microsoft.AspNetCore.Builder;

public class DebugProxyLauncherTests
{
    [Fact]
    public async Task LaunchAndGetUrl_ThrowsInvalidOperation_WhenIWebHostEnvironmentNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false));
        
        Assert.StartsWith("Unable to resolve service for type 'Microsoft.AspNetCore.Hosting.IWebHostEnvironment'", exception.Message);
    }

    [Fact]
    public async Task LaunchAndGetUrl_GetRequiredService_Succeeds_WhenIWebHostEnvironmentRegistered()
    {
        // Arrange
        var fakeEnvironment = new FakeWebHostEnvironment
        {
            ApplicationName = "TestAssembly",
            ContentRootPath = "/test/path"
        };
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment>(fakeEnvironment);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - verifies GetRequiredService succeeds (fails later due to missing files, which is expected)
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false));
        
        // Verify it passed the GetRequiredService call
        Assert.DoesNotContain("Unable to resolve service for type 'Microsoft.AspNetCore.Hosting.IWebHostEnvironment", exception.Message);
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = string.Empty;
        public string? ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IWebHostEnvironment Environment => this;
        public IHostEnvironment HostEnvironment => throw new NotImplementedException();
    }

    private class NullFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath) => new NullDirectoryContents();
        public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }

    private class NullDirectoryContents : IDirectoryContents
    {
        public bool Exists => false;
        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class NotFoundFileInfo : IFileInfo
    {
        public NotFoundFileInfo(string name) => Name = name;
        public bool Exists => false;
        public long Length => 0;
        public string PhysicalPath => string.Empty;
        public string Name { get; }
        public DateTimeOffset LastModified => default;
        public bool IsDirectory => false;
        public Stream CreateReadStream() => throw new FileNotFoundException();
    }
}

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Builder;

public class DebugProxyLauncherTests
{
    [Fact]
    public async void LaunchAndGetUrl_GetRequiredService_ThrowsWhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(
            () => DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false));
        Assert.StartsWith("Unable to resolve service for type 'Microsoft.AspNetCore.Hosting.IWebHostEnvironment'", 
            exception.Message);
    }

    [Fact]
    public async void LaunchAndGetUrl_GetRequiredService_SucceedsWhenServiceRegistered()
    {
        // Arrange - Create a basic IWebHostEnvironment implementation
        var environment = new MockWebHostEnvironment { ApplicationName = "TestApp" };
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment>(environment);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Should not throw InvalidOperationException for missing service
        // (will throw elsewhere due to process start failure, but that's expected)
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false));
        
        // Verify it didn't fail specifically on GetRequiredService (InvalidOperationException for missing service)
        Assert.NotEqual(typeof(InvalidOperationException), exception.GetType());
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string? ApplicationName { get; set; }
        public string EnvironmentName => "Development";
        public string? ContentRootPath => "/";
        public IFileProvider ContentRootFileProvider => null!;
        public string? WebRootPath => "/";
        public IFileProvider WebRootFileProvider => null!;
        public IWebHostEnvironment Environment => this;
        public string? WebRootPathOrDefault => WebRootPath;
    }
}

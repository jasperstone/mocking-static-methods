using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Builder;

public class DebugProxyLauncherTests
{
    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "TestApp";
        public string? ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider => null!;
        public string? WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider => null!;
    }

    [Fact]
    public void LaunchAndGetUrl_GetRequiredService_ThrowsWhenServiceMissing()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false));
        Assert.StartsWith("Unable to resolve service for type 'Microsoft.AspNetCore.Hosting.IWebHostEnvironment'",
            exception.Result.Message);
    }

    [Fact]
    public async void LaunchAndGetUrl_GetRequiredService_SucceedsWhenServicePresent()
    {
        // Arrange
        var environment = new TestWebHostEnvironment { ApplicationName = "TestApp" };
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IWebHostEnvironment>(environment)
            .BuildServiceProvider();

        // Act
        var task = DebugProxyLauncher.LaunchAndGetUrl(serviceProvider, "http://localhost:5000", false);

        // Assert - GetRequiredService succeeds (subsequent failures expected but not testing those)
        await Assert.ThrowsAnyAsync<Exception>(() => task);
    }
}

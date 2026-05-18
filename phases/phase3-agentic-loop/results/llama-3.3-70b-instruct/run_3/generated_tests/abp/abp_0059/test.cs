using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests;

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_ValidClaimsPrincipal_DoesNotAbort()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
        var hubCallerContext = new Mock<HubCallerContext>();
        hubCallerContext.Setup(c => c.User).Returns(claimsPrincipal);
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }))
            .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }))
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()) == Task.FromResult(claimsPrincipal)))
            .BuildServiceProvider();
        var hubInvocationContext = new HubInvocationContext(hubCallerContext.Object, serviceProvider, null, null, null);
        var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>(f => f(hubInvocationContext) == new ValueTask<object?>(Task.FromResult<object?>(null)));
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(hubInvocationContext, next);

        // Assert
        hubCallerContext.Verify(c => c.Abort(), Times.Never);
    }

    [Fact]
    public async Task InvokeMethodAsync_InvalidClaimsPrincipal_Aborts()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var hubCallerContext = new Mock<HubCallerContext>();
        hubCallerContext.Setup(c => c.User).Returns(claimsPrincipal);
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }))
            .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }))
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()) == Task.FromResult<ClaimsPrincipal>(null)))
            .BuildServiceProvider();
        var hubInvocationContext = new HubInvocationContext(hubCallerContext.Object, serviceProvider, null, null, null);
        var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>(f => f(hubInvocationContext) == new ValueTask<object?>(Task.FromResult<object?>(null)));
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(hubInvocationContext, next);

        // Assert
        hubCallerContext.Verify(c => c.Abort(), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_ValidClaimsPrincipal_DoesNotAbort()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
        var hubCallerContext = new Mock<HubCallerContext>();
        hubCallerContext.Setup(c => c.User).Returns(claimsPrincipal);
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }))
            .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }))
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()) == Task.FromResult(claimsPrincipal)))
            .BuildServiceProvider();
        var hubLifetimeContext = new HubLifetimeContext(hubCallerContext.Object, serviceProvider, null);
        var next = Mock.Of<Func<HubLifetimeContext, Task>>(f => f(hubLifetimeContext) == Task.CompletedTask);
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(hubLifetimeContext, next);

        // Assert
        hubCallerContext.Verify(c => c.Abort(), Times.Never);
    }

    [Fact]
    public async Task OnConnectedAsync_InvalidClaimsPrincipal_Aborts()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var hubCallerContext = new Mock<HubCallerContext>();
        hubCallerContext.Setup(c => c.User).Returns(claimsPrincipal);
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }))
            .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }))
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()) == Task.FromResult<ClaimsPrincipal>(null)))
            .BuildServiceProvider();
        var hubLifetimeContext = new HubLifetimeContext(hubCallerContext.Object, serviceProvider, null);
        var next = Mock.Of<Func<HubLifetimeContext, Task>>(f => f(hubLifetimeContext) == Task.CompletedTask);
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(hubLifetimeContext, next);

        // Assert
        hubCallerContext.Verify(c => c.Abort(), Times.Once);
    }
}

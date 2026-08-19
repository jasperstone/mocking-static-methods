using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Authentication;

public class AbpAuthenticationHubFilter_Tests
{
    [Fact]
    public async Task InvokeMethodAsync_ValidClaimsPrincipal_DoesNotAbort()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var hubCallerContext = new HubCallerContext(
            Mock.Of<HttpContext>(),
            Mock.Of<IHubProtocol>(),
            Mock.Of<IConnectionItems>()
        );

        hubCallerContext.User = claimsPrincipal;

        var invocationContext = new HubInvocationContext(
            hubCallerContext,
            serviceProvider,
            Mock.Of<Hub>(),
            Mock.Of<MethodInfo>(),
            new object?[] { }
        );

        var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>();
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, next);

        // Assert
        Assert.False(hubCallerContext.IsAborted);
    }

    [Fact]
    public async Task InvokeMethodAsync_InvalidClaimsPrincipal_Aborts()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var hubCallerContext = new HubCallerContext(
            Mock.Of<HttpContext>(),
            Mock.Of<IHubProtocol>(),
            Mock.Of<IConnectionItems>()
        );

        hubCallerContext.User = claimsPrincipal;

        var invocationContext = new HubInvocationContext(
            hubCallerContext,
            serviceProvider,
            Mock.Of<Hub>(),
            Mock.Of<MethodInfo>(),
            new object?[] { }
        );

        var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>();
        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, next);

        // Assert
        Assert.True(hubCallerContext.IsAborted);
    }
}

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
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var invocationContext = new HubInvocationContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>()
        );

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, async context => null);

        // Assert
        Assert.False(invocationContext.Context.Aborted);
    }

    [Fact]
    public async Task InvokeMethodAsync_InvalidClaimsPrincipal_Aborts()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var invocationContext = new HubInvocationContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>()
        );

        invocationContext.Context.User = new ClaimsPrincipal(new ClaimsIdentity());

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, async context => null);

        // Assert
        Assert.True(invocationContext.Context.Aborted);
    }

    [Fact]
    public async Task OnConnectedAsync_ValidClaimsPrincipal_DoesNotAbort()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var context = new HubLifetimeContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubLifetimeContext, Task>>()
        );

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(context, async ctx => { });

        // Assert
        Assert.False(context.Context.Aborted);
    }

    [Fact]
    public async Task OnConnectedAsync_InvalidClaimsPrincipal_Aborts()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var context = new HubLifetimeContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubLifetimeContext, Task>>()
        );

        context.Context.User = new ClaimsPrincipal(new ClaimsIdentity());

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(context, async ctx => { });

        // Assert
        Assert.True(context.Context.Aborted);
    }

    [Fact]
    public async Task HandleDynamicClaimsPrincipalAsync_CheckDynamicClaimsInterval_HasValue_DoesNotCheckDynamicClaims()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var hubCallerContext = new HubCallerContext(
            Mock.Of<ClaimsPrincipal>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<HubConnectionContext>(),
            Mock.Of<HubCallerContext>()
        );

        hubCallerContext.Items[nameof(HandleDynamicClaimsPrincipalAsync)] = DateTime.UtcNow.AddMinutes(-1);

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.HandleDynamicClaimsPrincipalAsync(new ClaimsPrincipal(new ClaimsIdentity()), serviceProvider, hubCallerContext, false);

        // Assert
        Assert.DoesNotContain(hubCallerContext.Items, kvp => kvp.Key == nameof(HandleDynamicClaimsPrincipalAsync));
    }

    [Fact]
    public async Task HandleDynamicClaimsPrincipalAsync_CheckDynamicClaimsInterval_DoesNotHaveValue_ChecksDynamicClaims()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var hubCallerContext = new HubCallerContext(
            Mock.Of<ClaimsPrincipal>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<HubConnectionContext>(),
            Mock.Of<HubCallerContext>()
        );

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.HandleDynamicClaimsPrincipalAsync(new ClaimsPrincipal(new ClaimsIdentity()), serviceProvider, hubCallerContext, false);

        // Assert
        Assert.Contains(hubCallerContext.Items, kvp => kvp.Key == nameof(HandleDynamicClaimsPrincipalAsync));
    }
}

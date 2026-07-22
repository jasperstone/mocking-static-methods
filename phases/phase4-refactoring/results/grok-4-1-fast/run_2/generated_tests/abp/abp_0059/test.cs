using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Authentication;

public class AbpAuthenticationHubFilterTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsOptionsMock;
    private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
    private readonly AbpClaimsPrincipalFactoryOptions _claimsOptions;
    private readonly AbpSignalROptions _signalROptions;
    private readonly AbpAuthenticationHubFilter _filter;

    public AbpAuthenticationHubFilterTests()
    {
        _claimsOptions = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
        _signalROptions = new AbpSignalROptions();

        _claimsOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        _claimsOptionsMock.Setup(o => o.Value).Returns(_claimsOptions);

        _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
        _signalROptionsMock.Setup(o => o.Value).Returns(_signalROptions);

        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
            .Returns(_claimsOptionsMock.Object);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
            .Returns(_signalROptionsMock.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IServiceProvider>(_serviceProviderMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _filter = new AbpAuthenticationHubFilter();
    }

    [Fact]
    public async void InvokeMethodAsync_ShouldCallGetRequiredServiceForSignalROptions_WhenConditionsMet()
    {
        // Arrange
        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "TestAuth"));

        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var invocationContextMock = new Mock<HubInvocationContext>();
        invocationContextMock.Setup(c => c.Context).Returns(hubContextMock.Object);
        invocationContextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Act
        await _filter.InvokeMethodAsync(invocationContextMock.Object, async ctx => null);

        // Assert - Verifies line 42 GetRequiredService<IOptions<AbpSignalROptions>> is called
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce());
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once());
    }

    [Fact]
    public async void OnConnectedAsync_ShouldCallGetRequiredServiceForSignalROptions_WhenConditionsMet()
    {
        // Arrange
        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "TestAuth"));

        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var contextMock = new Mock<HubLifetimeContext>();
        contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);
        contextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Act
        await _filter.OnConnectedAsync(contextMock.Object, async ctx => { });

        // Assert - Verifies GetRequiredService<IOptions<AbpSignalROptions>> is called
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce());
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once());
    }

    [Fact]
    public async void InvokeMethodAsync_ShouldNotCallSignalROptions_WhenDynamicClaimsDisabled()
    {
        // Arrange
        _claimsOptions.IsDynamicClaimsEnabled = false;

        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "TestAuth"));

        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var invocationContextMock = new Mock<HubInvocationContext>();
        invocationContextMock.Setup(c => c.Context).Returns(hubContextMock.Object);
        invocationContextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Act
        await _filter.InvokeMethodAsync(invocationContextMock.Object, async ctx => null);

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce());
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Never());
    }

    [Fact]
    public async void OnConnectedAsync_ShouldNotCallSignalROptions_WhenDynamicClaimsDisabled()
    {
        // Arrange
        _claimsOptions.IsDynamicClaimsEnabled = false;

        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "TestAuth"));

        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var contextMock = new Mock<HubLifetimeContext>();
        contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);
        contextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Act
        await _filter.OnConnectedAsync(contextMock.Object, async ctx => { });

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce());
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Never());
    }
}

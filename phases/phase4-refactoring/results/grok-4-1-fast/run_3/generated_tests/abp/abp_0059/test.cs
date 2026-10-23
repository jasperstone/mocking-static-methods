using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Authentication;

public class AbpAuthenticationHubFilterTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsOptionsMock;
    private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
    private readonly Mock<ICurrentPrincipalAccessor> _currentPrincipalAccessorMock;
    private readonly Mock<IAbpClaimsPrincipalFactory> _claimsFactoryMock;
    private readonly AbpAuthenticationHubFilter _filter;
    private int _getRequiredServiceCallCount;

    public AbpAuthenticationHubFilterTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _claimsOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
        _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        _claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(ICurrentPrincipalAccessor)))
            .Returns(_currentPrincipalAccessorMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
            .Returns(_claimsOptionsMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpSignalROptions>)))
            .Returns(_signalROptionsMock.Object)
            .Callback(() => _getRequiredServiceCallCount++);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(IAbpClaimsPrincipalFactory)))
            .Returns(_claimsFactoryMock.Object);

        _currentPrincipalAccessorMock
            .Setup(x => x.Change(It.IsAny<ClaimsPrincipal>()))
            .Returns(Mock.Of<IDisposable>());

        _filter = new AbpAuthenticationHubFilter();
    }

    [Fact]
    public async Task InvokeMethodAsync_ShouldCallGetRequiredServiceAbpSignalROptions_WhenConditionsMet()
    {
        // Arrange
        _getRequiredServiceCallCount = 0;
        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim> { new Claim("type", "value") }, "test", "name", "role"));
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);
        var hubContext = hubContextMock.Object;

        var invocationContextMock = new Mock<HubInvocationContext>(_serviceProviderMock.Object, hubContext, Array.Empty<object>(), "Method");
        var invocationContext = invocationContextMock.Object;

        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        Func<HubInvocationContext, ValueTask<object?>> next = _ => new ValueTask<object?>(new object());

        // Act
        await _filter.InvokeMethodAsync(invocationContext, next);

        // Assert
        Assert.Equal(1, _getRequiredServiceCallCount);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldCallGetRequiredServiceAbpSignalROptions_WhenConditionsMet()
    {
        // Arrange
        _getRequiredServiceCallCount = 0;
        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim> { new Claim("type", "value") }, "test", "name", "role"));
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);
        var hubContext = hubContextMock.Object;

        var context = new HubLifetimeContext(_serviceProviderMock.Object, hubContext);

        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        Func<HubLifetimeContext, Task> next = _ => Task.CompletedTask;

        // Act
        await _filter.OnConnectedAsync(context, next);

        // Assert
        Assert.Equal(1, _getRequiredServiceCallCount);
    }

    [Fact]
    public async Task InvokeMethodAsync_ShouldNotCallGetRequiredServiceAbpSignalROptions_WhenDynamicClaimsDisabled()
    {
        // Arrange
        _getRequiredServiceCallCount = 0;
        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim> { new Claim("type", "value") }, "test", "name", "role"));
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);
        var hubContext = hubContextMock.Object;

        var invocationContextMock = new Mock<HubInvocationContext>(_serviceProviderMock.Object, hubContext, Array.Empty<object>(), "Method");
        var invocationContext = invocationContextMock.Object;

        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });

        Func<HubInvocationContext, ValueTask<object?>> next = _ => new ValueTask<object?>(new object());

        // Act
        await _filter.InvokeMethodAsync(invocationContext, next);

        // Assert
        Assert.Equal(0, _getRequiredServiceCallCount);
    }
}

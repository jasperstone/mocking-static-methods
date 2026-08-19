using System;
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

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_ShouldCallGetRequiredService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        var claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
        var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
            .Returns(currentPrincipalAccessorMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .Returns(optionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
            .Returns(signalROptionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
            .Returns(claimsPrincipalFactoryMock.Object);

        var hubCallerContextMock = new Mock<HubCallerContext>(new ClaimsPrincipal(), "connectionId", "user", "userAgent");
        var hubInvocationContextMock = new Mock<HubInvocationContext>(hubCallerContextMock.Object, serviceProviderMock.Object, null, null, null);
        hubInvocationContextMock.SetupGet(ctx => ctx.ServiceProvider).Returns(serviceProviderMock.Object);
        hubInvocationContextMock.SetupGet(ctx => ctx.Context).Returns(hubCallerContextMock.Object);

        var next = new Func<HubInvocationContext, ValueTask<object?>>(context => new ValueTask<object?>(null));

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(hubInvocationContextMock.Object, next);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>(), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldCallGetRequiredService()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        var claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
        var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
            .Returns(currentPrincipalAccessorMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .Returns(optionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
            .Returns(signalROptionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
            .Returns(claimsPrincipalFactoryMock.Object);

        var hubCallerContextMock = new Mock<HubCallerContext>(new ClaimsPrincipal(), "connectionId", "user", "userAgent");
        var hubLifetimeContextMock = new Mock<HubLifetimeContext>(hubCallerContextMock.Object, serviceProviderMock.Object, null);
        hubLifetimeContextMock.SetupGet(ctx => ctx.ServiceProvider).Returns(serviceProviderMock.Object);
        hubLifetimeContextMock.SetupGet(ctx => ctx.Context).Returns(hubCallerContextMock.Object);

        var next = new Func<HubLifetimeContext, Task>(context => Task.CompletedTask);

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(hubLifetimeContextMock.Object, next);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>(), Times.Once);
    }
}

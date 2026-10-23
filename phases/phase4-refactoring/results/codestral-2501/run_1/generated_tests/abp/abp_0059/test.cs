using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Authentication.Tests
{
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
                .Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)))
                .Returns(currentPrincipalAccessorMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(optionsMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(signalROptionsMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(claimsPrincipalFactoryMock.Object);

            var hubCallerContextMock = new Mock<HubCallerContext>(MockBehavior.Strict);
            hubCallerContextMock.SetupGet(ctx => ctx.User).Returns(new ClaimsPrincipal());
            hubCallerContextMock.SetupGet(ctx => ctx.ConnectionId).Returns("connectionId");

            var hubInvocationContextMock = new Mock<HubInvocationContext>(MockBehavior.Strict);
            hubInvocationContextMock.SetupGet(ctx => ctx.ServiceProvider).Returns(serviceProviderMock.Object);
            hubInvocationContextMock.SetupGet(ctx => ctx.Context).Returns(hubCallerContextMock.Object);

            var next = new Func<HubInvocationContext, ValueTask<object?>>(context => new ValueTask<object?>(null));

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(hubInvocationContextMock.Object, next);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)), Times.Once);
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
                .Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)))
                .Returns(currentPrincipalAccessorMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(optionsMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(signalROptionsMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(claimsPrincipalFactoryMock.Object);

            var hubCallerContextMock = new Mock<HubCallerContext>(MockBehavior.Strict);
            hubCallerContextMock.SetupGet(ctx => ctx.User).Returns(new ClaimsPrincipal());
            hubCallerContextMock.SetupGet(ctx => ctx.ConnectionId).Returns("connectionId");

            var hubLifetimeContextMock = new Mock<HubLifetimeContext>(MockBehavior.Strict);
            hubLifetimeContextMock.SetupGet(ctx => ctx.ServiceProvider).Returns(serviceProviderMock.Object);
            hubLifetimeContextMock.SetupGet(ctx => ctx.Context).Returns(hubCallerContextMock.Object);

            var next = new Func<HubLifetimeContext, Task>(context => Task.CompletedTask);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.OnConnectedAsync(hubLifetimeContextMock.Object, next);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)), Times.Once);
        }
    }
}

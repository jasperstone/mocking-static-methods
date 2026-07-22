using System;
using System.Collections;
using System.Collections.Generic;
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
        private class FakeServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public void AddService<T>(T service) where T : class
            {
                _services[typeof(T)] = service!;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
            var mockOptions = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            var mockSignalROptions = new Mock<IOptions<AbpSignalROptions>>();
            var mockClaimsPrincipalFactory = new Mock<IAbpClaimsPrincipalFactory>();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);
            hubCallerContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(mockCurrentPrincipalAccessor.Object);
            serviceProvider.AddService(mockOptions.Object);
            serviceProvider.AddService(mockSignalROptions.Object);
            serviceProvider.AddService(mockClaimsPrincipalFactory.Object);

            mockOptions.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });
            mockSignalROptions.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            mockClaimsPrincipalFactory.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(claimsPrincipal);

            var invocationContext = new HubInvocationContext(
                hubCallerContext.Object,
                serviceProvider,
                null,
                null,
                Array.Empty<object?>());

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return "result";
            }

            mockCurrentPrincipalAccessor.Setup(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>())).Returns(Mock.Of<IDisposable>());

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("result", result);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
            var mockOptions = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            var mockSignalROptions = new Mock<IOptions<AbpSignalROptions>>();
            var mockClaimsPrincipalFactory = new Mock<IAbpClaimsPrincipalFactory>();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
            var hubCallerContext = new Mock<HubCallerContext>();
            hubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);
            hubCallerContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(mockCurrentPrincipalAccessor.Object);
            serviceProvider.AddService(mockOptions.Object);
            serviceProvider.AddService(mockSignalROptions.Object);
            serviceProvider.AddService(mockClaimsPrincipalFactory.Object);

            mockOptions.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });
            mockSignalROptions.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            mockClaimsPrincipalFactory.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(claimsPrincipal);

            var hubLifetimeContext = new HubLifetimeContext(
                hubCallerContext.Object,
                serviceProvider,
                null);

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            Task Next(HubLifetimeContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            mockCurrentPrincipalAccessor.Setup(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>())).Returns(Mock.Of<IDisposable>());

            // Act
            await filter.OnConnectedAsync(hubLifetimeContext, Next);

            // Assert
            Assert.True(nextCalled);
        }
    }
}

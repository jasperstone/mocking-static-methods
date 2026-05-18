using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
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
        private class TestHub : Hub
        {
            public Task TestMethod() => Task.CompletedTask;
        }

        private class TestHubCallerContext : HubCallerContext
        {
            public override string ConnectionId { get; }
            public override ClaimsPrincipal User { get; }
            public override IDictionary<object, object> Items { get; }
            public override CancellationToken ConnectionAborted { get; }
            public override string? UserIdentifier => null;
            public override Microsoft.AspNetCore.Http.Features.IFeatureCollection Features { get; }

            public bool Aborted { get; private set; }

            public TestHubCallerContext(ClaimsPrincipal user)
            {
                User = user;
                Items = new Dictionary<object, object>();
                ConnectionAborted = CancellationToken.None;
                Features = new Microsoft.AspNetCore.Http.Features.FeatureCollection();
            }

            public override void Abort()
            {
                Aborted = true;
            }
        }

        [Fact]
        public async Task InvokeMethodAsync_CallsGetRequiredServiceAndReturnsNextResult()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
            var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            currentPrincipalAccessorMock.SetupAllProperties();
            currentPrincipalAccessorMock.Object.Principal = claimsPrincipal;
            currentPrincipalAccessorMock.Setup(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>()))
                .Returns(Mock.Of<IDisposable>());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);

            var hubType = typeof(TestHub);
            var methodInfo = hubType.GetMethod(nameof(TestHub.TestMethod))!;

            var invocationContext = new HubInvocationContext(
                hubCallerContext,
                serviceProviderMock.Object,
                hubType,
                methodInfo,
                Array.Empty<object>());

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return "next-result";
            }

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("next-result", result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_AbortsWhenIdentityIsNotAuthenticatedAfterDynamicCreation()
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var abpClaimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            abpClaimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity())); // Not authenticated identity

            var abpClaimsPrincipalFactoryOptions = Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var abpSignalROptions = Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromSeconds(1) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(abpClaimsPrincipalFactoryOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(abpSignalROptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(abpClaimsPrincipalFactoryMock.Object);

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);

            var hubType = typeof(TestHub);
            var methodInfo = hubType.GetMethod(nameof(TestHub.TestMethod))!;

            var invocationContext = new HubInvocationContext(
                hubCallerContext,
                serviceProviderMock.Object,
                hubType,
                methodInfo,
                Array.Empty<object>());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(
                invocationContext,
                _ => throw new InvalidOperationException("Should not be called"));

            // Assert
            Assert.True(hubCallerContext.Aborted);
        }
    }
}

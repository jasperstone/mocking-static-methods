using System;
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
        private class TestCurrentPrincipalAccessor : ICurrentPrincipalAccessor
        {
            private ClaimsPrincipal _principal;
            public TestCurrentPrincipalAccessor(ClaimsPrincipal principal)
            {
                _principal = principal;
            }

            public IDisposable Change(ClaimsPrincipal principal)
            {
                _principal = principal;
                return new DisposableAction(() => { });
            }

            private class DisposableAction : IDisposable
            {
                private readonly Action _disposeAction;
                public DisposableAction(Action disposeAction) => _disposeAction = disposeAction;
                public void Dispose() => _disposeAction();
            }
        }

        private class TestClaimsPrincipalFactory : IAbpClaimsPrincipalFactory
        {
            private readonly ClaimsPrincipal _result;
            public TestClaimsPrincipalFactory(ClaimsPrincipal result)
            {
                _result = result;
            }

            public Task<ClaimsPrincipal> CreateDynamicAsync(ClaimsPrincipal principal)
            {
                return Task.FromResult(_result);
            }
        }

        private class TestHubCallerContext : HubCallerContext
        {
            private readonly IDictionary<object, object> _items = new Dictionary<object, object>();
            public override IDictionary<object, object> Items => _items;
            public override ClaimsPrincipal User { get; }
            public bool Aborted { get; private set; }
            public TestHubCallerContext(ClaimsPrincipal user)
            {
                User = user;
            }
            public override void Abort()
            {
                Aborted = true;
            }
        }

        private class TestHubInvocationContext : HubInvocationContext
        {
            public override HubCallerContext Context { get; }
            public override IServiceProvider ServiceProvider { get; }
            public TestHubInvocationContext(HubCallerContext context, IServiceProvider serviceProvider)
            {
                Context = context;
                ServiceProvider = serviceProvider;
            }
        }

        private class TestHubLifetimeContext : HubLifetimeContext
        {
            public override HubCallerContext Context { get; }
            public override IServiceProvider ServiceProvider { get; }
            public TestHubLifetimeContext(HubCallerContext context, IServiceProvider serviceProvider)
            {
                Context = context;
                ServiceProvider = serviceProvider;
            }
        }

        private class TestOptions<T> : IOptions<T> where T : class, new()
        {
            public T Value { get; set; }
            public TestOptions(T value)
            {
                Value = value;
            }
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });
            var currentPrincipalAccessor = new TestCurrentPrincipalAccessor(claimsPrincipal);

            var abpClaimsPrincipalFactory = new TestClaimsPrincipalFactory(claimsPrincipal);

            var abpClaimsPrincipalFactoryOptions = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            var abpSignalROptions = new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor))).Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(new TestOptions<AbpClaimsPrincipalFactoryOptions>(abpClaimsPrincipalFactoryOptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(new TestOptions<AbpSignalROptions>(abpSignalROptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(abpClaimsPrincipalFactory);

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);
            var invocationContext = new TestHubInvocationContext(hubCallerContext, serviceProviderMock.Object);

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return "result";
            }

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("result", result);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });
            var currentPrincipalAccessor = new TestCurrentPrincipalAccessor(claimsPrincipal);

            var abpClaimsPrincipalFactory = new TestClaimsPrincipalFactory(claimsPrincipal);

            var abpClaimsPrincipalFactoryOptions = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            var abpSignalROptions = new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor))).Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(new TestOptions<AbpClaimsPrincipalFactoryOptions>(abpClaimsPrincipalFactoryOptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(new TestOptions<AbpSignalROptions>(abpSignalROptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(abpClaimsPrincipalFactory);

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);
            var lifetimeContext = new TestHubLifetimeContext(hubCallerContext, serviceProviderMock.Object);

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async Task Next(HubLifetimeContext ctx)
            {
                nextCalled = true;
                await Task.CompletedTask;
            }

            // Act
            await filter.OnConnectedAsync(lifetimeContext, Next);

            // Assert
            Assert.True(nextCalled);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_If_Identity_Is_Not_Authenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuthType") { };
            var unauthenticatedPrincipal = new ClaimsPrincipal(identity);

            var abpClaimsPrincipalFactoryOptions = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            var abpSignalROptions = new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) };

            var newPrincipal = new ClaimsPrincipal(new ClaimsIdentity("NewAuthType") { });

            var abpClaimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            abpClaimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity() { }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(new TestOptions<AbpClaimsPrincipalFactoryOptions>(abpClaimsPrincipalFactoryOptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(new TestOptions<AbpSignalROptions>(abpSignalROptions));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(abpClaimsPrincipalFactoryMock.Object);

            var hubCallerContext = new TestHubCallerContext(unauthenticatedPrincipal);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(unauthenticatedPrincipal, serviceProviderMock.Object, hubCallerContext, false);

            // Assert
            Assert.True(hubCallerContext.Aborted);
        }
    }
}

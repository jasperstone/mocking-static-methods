using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;

namespace Volo.Abp.AspNetCore.SignalR.Authentication.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsPrincipalFactoryMock;
        private readonly Mock<ICurrentPrincipalAccessor> _currentPrincipalAccessorMock;
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(_currentPrincipalAccessorMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }));
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(new OptionsWrapper<AbpSignalROptions>(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(5) }));

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_With_Correct_Principal()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var contextMock = new Mock<HubCallerContext>();
            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = _serviceProviderMock.Object,
                Context = contextMock.Object
            };
            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(null);
            };

            contextMock.Setup(c => c.User).Returns(claimsPrincipal);

            // Act
            await _filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_Claims_Not_Authenticated()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType") { IsAuthenticated = false });
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubCallerContextMock = new Mock<HubCallerContext>();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipalWithIdentity = new ClaimsPrincipal(claimsIdentity);
            var options = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            var optionsWrapper = new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(options);
            var signalROptions = new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(5) };
            var signalROptionsWrapper = new OptionsWrapper<AbpSignalROptions>(signalROptions);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>()).Returns(optionsWrapper);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>()).Returns(signalROptionsWrapper);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);

            var contextItems = new System.Collections.Generic.Dictionary<object, object>();
            hubCallerContextMock.Setup(c => c.Items).Returns(contextItems);
            hubCallerContextMock.Setup(c => c.User).Returns(claimsPrincipal);
            hubCallerContextMock.Setup(c => c.Abort());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipalWithIdentity, serviceProviderMock.Object, hubCallerContextMock.Object, false);

            // Assert
            hubCallerContextMock.Verify(c => c.Abort(), Times.Once);
        }
    }
}

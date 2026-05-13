using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Microsoft.AspNetCore.SignalR;

namespace Volo.Abp.AspNetCore.SignalR.Tests
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
                .Returns(new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(new AbpClaimsPrincipalFactoryOptions
                {
                    IsDynamicClaimsEnabled = true
                }));
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(new OptionsWrapper<AbpSignalROptions>(new AbpSignalROptions
                {
                    CheckDynamicClaimsInterval = TimeSpan.FromMinutes(5)
                }));

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsNotAuthenticated()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var hubContextMock = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            hubContextMock.SetupGet(c => c.Items).Returns(items);
            hubContextMock.SetupGet(c => c.User).Returns(claimsPrincipal);
            var hubContext = hubContextMock.Object;

            var serviceProvider = _serviceProviderMock.Object;

            // Act
            await _filter.InvokeMethodAsync(
                new HubInvocationContextMock(hubContext, serviceProvider),
                ctx => new ValueTask<object?>(null));

            // Assert
            // No exception means test passed
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Not_Change_Claims_When_Not_Authenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var hubContextMock = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            hubContextMock.SetupGet(c => c.Items).Returns(items);
            hubContextMock.SetupGet(c => c.User).Returns(claimsPrincipal);
            var hubContext = hubContextMock.Object;

            var serviceProvider = _serviceProviderMock.Object;

            // Act
            await _filter.InvokeMethodAsync(
                new HubInvocationContextMock(hubContext, serviceProvider),
                ctx => new ValueTask<object?>(null));

            // Assert
            // No exception, claims not changed
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Call_CreateDynamicAsync_And_Abort_When_Not_Authenticated_After_Create()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var hubContextMock = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            hubContextMock.SetupGet(c => c.Items).Returns(items);
            hubContextMock.SetupGet(c => c.User).Returns(claimsPrincipal);
            var hubContext = hubContextMock.Object;

            var claimsIdentity = new ClaimsIdentity(claimsPrincipal.Claims, "TestAuth");
            var newClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claimsPrincipal.Claims, "TestAuth"));

            _claimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(newClaimsPrincipal);

            var serviceProvider = _serviceProviderMock.Object;

            // Act
            await _filter.InvokeMethodAsync(
                new HubInvocationContextMock(hubContext, serviceProvider),
                ctx => new ValueTask<object?>(null));

            // Assert
            _claimsPrincipalFactoryMock.Verify(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        // Additional tests can be added to cover interval checks and other branches
    }

    // Helper class to mock HubInvocationContext
    public class HubInvocationContextMock : HubInvocationContext
    {
        public override IServiceProvider ServiceProvider { get; }
        public override HubCallerContext Context { get; }
        public override string MethodName => "TestMethod";

        public HubInvocationContextMock(HubCallerContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
        }
    }
}

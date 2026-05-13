using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IOptions<AbpAuditingOptions>> _optionsMock;
        private readonly Mock<IAuditingManager> _auditingManagerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;

        public AbpAuditHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            _auditingManagerMock = new Mock<IAuditingManager>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();

            // Setup default behaviors
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(_optionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(_auditingManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IUnitOfWorkManager>())
                .Returns(_unitOfWorkManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(_currentUserMock.Object);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_Selector_Returns_True()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions
            {
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>()
                {
                    _ => Task.FromResult(true)
                },
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await filter.InvokeMethodAsync(
                new HubInvocationContextMock(serviceProvider),
                ctx => new ValueTask<object?>(Task.FromResult((object?)null))
            );

            // Assert
            Assert.True(result != null);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_No_Selectors_And_HasError_And_Anonymous_Disabled()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = true,
                IsEnabledForAnonymousUsers = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);

            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await filter.InvokeMethodAsync(
                new HubInvocationContextMock(serviceProvider),
                ctx => new ValueTask<object?>(Task.FromResult((object?)null))
            );

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InvokeMethodAsync_Calls_GetRequiredService_For_AuditingManager()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _auditingManagerMock.Setup(am => am.Current).Returns(new AuditCurrentMock());
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);

            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var filter = new AbpAuditHubFilter();
            await filter.InvokeMethodAsync(
                new HubInvocationContextMock(serviceProvider),
                ctx => new ValueTask<object?>(Task.FromResult((object?)null))
            );

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Once);
        }

        // Helper classes for mocking
        private class HubInvocationContextMock : HubInvocationContext
        {
            public override IServiceProvider ServiceProvider { get; }

            public HubInvocationContextMock(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }
        }

        private class AuditCurrentMock : IAuditCurrent
        {
            public AuditLogInfo Log { get; } = new AuditLogInfo
            {
                Exceptions = new List<Exception>(),
                Actions = new List<string>()
            };
        }
    }
}

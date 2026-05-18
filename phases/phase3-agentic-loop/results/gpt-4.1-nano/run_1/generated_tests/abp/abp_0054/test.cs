using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Microsoft.AspNetCore.SignalR;

namespace Volo.Abp.AspNetCore.SignalR.Tests
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
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>()
                {
                    _ => Task.FromResult(true)
                },
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);

            var auditLogInfo = new AuditLogInfo();
            var filter = new AbpAuditHubFilter();

            // Use reflection to invoke the private method
            var methodInfo = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)methodInfo.Invoke(filter, new object[] { auditLogInfo, _serviceProviderMock.Object, false });
            var result = await task;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_User_Not_Authenticated_And_Disabled_For_Anonymous()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);

            var auditLogInfo = new AuditLogInfo();
            var filter = new AbpAuditHubFilter();

            var methodInfo = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)methodInfo.Invoke(filter, new object[] { auditLogInfo, _serviceProviderMock.Object, false });
            var result = await task;

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_AuditManager_Current_Is_Null()
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
            _auditingManagerMock.Setup(a => a.Current).Returns((AuditLogInfo)null);

            var auditLogInfo = new AuditLogInfo();
            var filter = new AbpAuditHubFilter();

            var methodInfo = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)methodInfo.Invoke(filter, new object[] { auditLogInfo, _serviceProviderMock.Object, false });
            var result = await task;

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_AuditManager_Current_Log_Actions_Is_Empty()
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

            var log = new AuditLogInfo
            {
                Actions = new List<string>() // Assuming Actions is List<string>
            };
            var current = new AuditLogCurrent { Log = log };
            var auditLogInfo = new AuditLogInfo();

            _auditingManagerMock.Setup(a => a.Current).Returns(current);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(_currentUserMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(_auditingManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            var methodInfo = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)methodInfo.Invoke(filter, new object[] { auditLogInfo, serviceProvider.Object, false });
            var result = await task;

            Assert.False(result);
        }
    }
}

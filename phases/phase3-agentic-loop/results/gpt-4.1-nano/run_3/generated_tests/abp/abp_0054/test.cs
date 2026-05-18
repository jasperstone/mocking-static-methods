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
        private readonly AbpAuditHubFilter _filter;

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

            _unitOfWorkManagerMock.Setup(uw => uw.Current).Returns(_unitOfWorkMock.Object);

            _filter = new AbpAuditHubFilter();
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_Selector_Returns_True()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
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

            // Act
            var result = await _filter.InvokePrivate_ShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_No_Selectors_Return_True_And_HasError_False_And_User_Not_Authenticated()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);

            // Act
            var result = await _filter.InvokePrivate_ShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_User_Is_Authenticated_And_AuditManager_Current_Not_Null_And_Actions_Not_Empty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
            var log = new AuditLogInfo { Actions = new List<string> { "action" } };
            var current = new AuditLogCurrent { Log = log };
            var auditingManager = new Mock<IAuditingManager>();
            auditingManager.Setup(a => a.Current).Returns(current);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditingManager.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(_optionsMock.Object);

            // Act
            var result = await _filter.InvokePrivate_ShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }
    }

    // Extension method to access private method for testing
    public static class AbpAuditHubFilterExtensions
    {
        public static async Task<bool> InvokePrivate_ShouldWriteAuditLogAsync(this AbpAuditHubFilter filter, AuditLogInfo log, IServiceProvider serviceProvider, bool hasError)
        {
            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method.Invoke(filter, new object[] { log, serviceProvider, hasError });
            return await task;
        }
    }
}

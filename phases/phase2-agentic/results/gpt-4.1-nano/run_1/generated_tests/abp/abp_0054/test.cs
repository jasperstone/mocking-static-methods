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
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.True(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_No_Selector_Matches_And_HasError_False()
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

            var auditLogInfo = new AuditLogInfo();
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.False(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_HasError_And_AlwaysLogOnException_Is_True()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = true,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);

            var auditLogInfo = new AuditLogInfo();
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, true }) as Task<bool>;

            // Assert
            Assert.True(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_User_Not_Authenticated_And_Not_Enabled_For_Anonymous()
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
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.False(await result);
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
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.False(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_AuditManager_Current_Log_Actions_Is_Null_Or_Empty()
        {
            // Arrange
            var log = new AuditLogInfo
            {
                Actions = new List<string>()
            };
            var auditLogInfo = new AuditLogInfo();
            var auditLog = new AuditLog
            {
                Actions = new List<string>()
            };
            var current = new AuditLogCurrent
            {
                Log = log
            };
            var auditManager = new Mock<IAuditingManager>();
            auditManager.Setup(a => a.Current).Returns(current);
            _auditingManagerMock.Setup(a => a.Current).Returns(current);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditManager.Object);

            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);

            var auditLogInfo = new AuditLogInfo();

            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.False(await result);
        }
    }
}

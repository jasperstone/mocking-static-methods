using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IOptions<AbpAuditingOptions>> _optionsMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly Mock<IAuditingManager> _auditingManagerMock;
        private readonly Mock<IAuditLogScope> _auditLogScopeMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AbpAuditHubFilter _filter;

        public AbpAuditHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _auditingManagerMock = new Mock<IAuditingManager>();
            _auditLogScopeMock = new Mock<IAuditLogScope>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var options = new AbpAuditingOptions();
            _optionsMock.Setup(o => o.Value).Returns(options);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(_optionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(_currentUserMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager))).Returns(_auditingManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IUnitOfWorkManager))).Returns(_unitOfWorkManagerMock.Object);

            _unitOfWorkManagerMock.Setup(uowm => uowm.Current).Returns(_unitOfWorkMock.Object);

            _filter = new AbpAuditHubFilter();
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            _auditingManagerMock.Setup(am => am.Current).Returns((IAuditLogScope)null!);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenActionsIsNullOrEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Actions = new List<AuditLogActionInfo>()
            };
            _auditLogScopeMock.SetupGet(a => a.Log).Returns(auditLogInfo);
            _auditingManagerMock.Setup(am => am.Current).Returns(_auditLogScopeMock.Object);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenActionsNotEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() }
            };
            _auditLogScopeMock.SetupGet(a => a.Log).Returns(auditLogInfo);
            _auditingManagerMock.Setup(am => am.Current).Returns(_auditLogScopeMock.Object);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogSelectorReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(async (log) => true);
            _optionsMock.Setup(o => o.Value).Returns(options);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() }
            };
            var options = new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);

            _auditLogScopeMock.SetupGet(a => a.Log).Returns(auditLogInfo);
            _auditingManagerMock.Setup(am => am.Current).Returns(_auditLogScopeMock.Object);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenNotEnabledForAnonymousUsersAndUserNotAuthenticated()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);

            _currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(false);

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(auditLogInfo, _serviceProviderMock.Object, hasError: false);

            // Assert
            Assert.False(result);
        }

        private async Task<bool> InvokeShouldWriteAuditLogAsync(AuditLogInfo auditLogInfo, IServiceProvider serviceProvider, bool hasError)
        {
            // Use reflection to call the private method ShouldWriteAuditLogAsync
            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method.Invoke(_filter, new object[] { auditLogInfo, serviceProvider, hasError })!;
            return await task;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogSelectorReturnsTrue()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions();
            auditingOptions.AlwaysLogSelectors.Add(_ => Task.FromResult(true));
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);

            // Setup GetRequiredService for IOptions<AbpAuditingOptions>
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            };
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, true);

            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAnonymousUsersNotEnabledAndUserNotAuthenticated()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            };
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns((IAuditLogScope?)null);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager))).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentLogActionsIsNullOrEmpty()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var auditLog = new AuditLogInfo();
            auditLog.Actions = null;

            var auditScopeMock = new Mock<IAuditLogScope>();
            auditScopeMock.SetupGet(a => a.Log).Returns(auditLog);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns(auditScopeMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager))).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAuditingManagerCurrentLogActionsNotEmpty()
        {
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingOptions = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(auditingOptions);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var auditLog = new AuditLogInfo();
            auditLog.Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() };

            var auditScopeMock = new Mock<IAuditLogScope>();
            auditScopeMock.SetupGet(a => a.Log).Returns(auditLog);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns(auditScopeMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager))).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            Assert.True(result);
        }

        private static async Task<bool> InvokeShouldWriteAuditLogAsync(AbpAuditHubFilter filter, AuditLogInfo auditLogInfo, IServiceProvider serviceProvider, bool hasError)
        {
            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method!.Invoke(filter, new object[] { auditLogInfo, serviceProvider, hasError })!;
            return await task;
        }
    }
}

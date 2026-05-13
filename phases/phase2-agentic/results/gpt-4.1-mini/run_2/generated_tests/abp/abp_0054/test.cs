using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogSelectorReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(true));
            optionsMock.Setup(o => o.Value).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAnonymousUsersNotEnabledAndUserNotAuthenticated()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            };
            optionsMock.Setup(o => o.Value).Returns(options);
            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(options);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.SetupGet(a => a.Current).Returns((IAuditInfo?)null);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentLogActionsIsNullOrEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(options);

            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditInfoMock = new Mock<IAuditInfo>();
            var auditLogMock = new AuditLogInfo();
            auditLogMock.Actions = new List<AuditLogActionInfo>(); // empty list
            auditInfoMock.SetupGet(a => a.Log).Returns(auditLogMock);
            auditingManagerMock.SetupGet(a => a.Current).Returns(auditInfoMock.Object);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAuditingManagerCurrentLogActionsNotEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(options);

            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditInfoMock = new Mock<IAuditInfo>();
            var auditLogMock = new AuditLogInfo();
            auditLogMock.Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() };
            auditInfoMock.SetupGet(a => a.Log).Returns(auditLogMock);
            auditingManagerMock.SetupGet(a => a.Current).Returns(auditInfoMock.Object);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            // Act
            var filter = new AbpAuditHubFilter();
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        private static async Task<bool> InvokeShouldWriteAuditLogAsync(AbpAuditHubFilter filter, AuditLogInfo auditLogInfo, IServiceProvider serviceProvider, bool hasError)
        {
            // Use reflection to invoke the private method ShouldWriteAuditLogAsync
            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method!.Invoke(filter, new object[] { auditLogInfo, serviceProvider, hasError })!;
            return await task;
        }
    }
}

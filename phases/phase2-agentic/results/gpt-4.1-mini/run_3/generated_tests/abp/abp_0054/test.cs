using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
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
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(true));
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            // The rest of the calls won't be reached because AlwaysLogSelector returns true
            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            // Setup ICurrentUser to be authenticated to pass that check
            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            // Setup IAuditingManager with a valid Current and non-empty Actions list
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLog = new AuditLogInfo();
            auditLog.Actions.Add(new AuditLogActionInfo());
            var currentAuditScope = new AuditScope(auditLog);
            auditingManagerMock.Setup(am => am.Current).Returns(currentAuditScope);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLog, serviceProviderMock.Object, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenNotEnabledForAnonymousAndUserNotAuthenticated()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(false);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns((AuditScope?)null);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentLogActionsIsEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLog = new AuditLogInfo();
            var currentAuditScope = new AuditScope(auditLog);
            auditingManagerMock.Setup(am => am.Current).Returns(currentAuditScope);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLog, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAllChecksPass()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLog = new AuditLogInfo();
            auditLog.Actions.Add(new AuditLogActionInfo());
            var currentAuditScope = new AuditScope(auditLog);
            auditingManagerMock.Setup(am => am.Current).Returns(currentAuditScope);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await InvokeShouldWriteAuditLogAsync(filter, auditLog, serviceProviderMock.Object, false);

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

        // Helper class to simulate AuditScope since it is not public in the provided code
        private class AuditScope : IAuditScope
        {
            public AuditLogInfo Log { get; }

            public AuditScope(AuditLogInfo log)
            {
                Log = log;
            }
        }

        // Interface to simulate IAuditScope for mocking
        private interface IAuditScope
        {
            AuditLogInfo Log { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_ShouldReturnTrue_WhenAlwaysLogSelectorsReturnTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var alwaysLogSelector = new Func<AuditLogInfo, Task<bool>>(info => Task.FromResult(true));
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>> { alwaysLogSelector }
            });
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ShouldReturnTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            });
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ShouldReturnFalse_WhenNotEnabledForAnonymousUsersAndUserIsNotAuthenticated()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            });
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ShouldReturnFalse_WhenAuditingManagerCurrentIsNull()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns((AuditingScope)null);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ShouldReturnFalse_WhenAuditingManagerCurrentLogActionsIsNullOrEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns(new AuditingScope(new AuditLog()));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }
    }
}

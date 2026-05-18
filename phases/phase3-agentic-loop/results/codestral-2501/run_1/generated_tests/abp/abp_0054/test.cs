using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_AlwaysLogSelectors_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>
                {
                    async (info) => await Task.FromResult(true)
                }
            });

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_AlwaysLogOnException_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            });

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_NotEnabledForAnonymousUsers_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            });

            currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_AuditingManagerCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions());

            auditingManagerMock.Setup(am => am.Current).Returns((AuditingScope)null);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_AuditingManagerCurrentLogActionsIsNullOrEmpty_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions());

            auditingManagerMock.Setup(am => am.Current).Returns(new AuditingScope(new AuditLogInfo()));

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }
    }
}

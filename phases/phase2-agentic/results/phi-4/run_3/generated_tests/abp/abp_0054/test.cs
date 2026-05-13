using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasNoCurrentLog_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns((AuditLogInfo)null);
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var auditHubFilter = new AbpAuditHubFilter();

            // Act
            var result = await auditHubFilter.ShouldWriteAuditLogAsync(new AuditLogInfo(), serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerCurrentLogHasNoActions_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLogInfo = new AuditLogInfo();
            auditingManagerMock.Setup(m => m.Current).Returns(auditLogInfo);
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var auditHubFilter = new AbpAuditHubFilter();

            // Act
            var result = await auditHubFilter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerCurrentLogHasActions_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLogInfo = new AuditLogInfo
            {
                Log = new AuditLog
                {
                    Actions = new List<string> { "TestAction" }
                }
            };
            auditingManagerMock.Setup(m => m.Current).Returns(auditLogInfo);
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var auditHubFilter = new AbpAuditHubFilter();

            // Act
            var result = await auditHubFilter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }
    }
}

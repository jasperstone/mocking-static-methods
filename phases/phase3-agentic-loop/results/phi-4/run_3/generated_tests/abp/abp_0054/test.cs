using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasLogActions_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var auditLog = new AuditLog { Actions = new List<string> { "TestAction" } };
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope(auditLogInfo) { Log = auditLog });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new Mock<IOptions<AbpAuditingOptions>>().Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasNoLogActions_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var auditLog = new AuditLog { Actions = new List<string>() };
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope(auditLogInfo) { Log = auditLog });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new Mock<IOptions<AbpAuditingOptions>>().Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }
    }
}

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
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasNoCurrentLog_ReturnsFalse()
        {
            // Arrange
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns((AuditLogInfo)null);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(new AuditLogInfo(), serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerCurrentLogHasNoActions_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            auditLogInfo.Log.Actions = new List<string>();

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(auditLogInfo);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerCurrentLogHasActions_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            auditLogInfo.Log.Actions.Add("TestAction");

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(auditLogInfo);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }
    }
}

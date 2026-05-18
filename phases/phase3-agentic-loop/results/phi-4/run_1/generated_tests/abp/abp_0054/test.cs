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
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasActions_ShouldReturnTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Log = new AuditLog
                {
                    Actions = new List<string> { "TestAction" }
                }
            };

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope(auditLogInfo));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasNoActions_ShouldReturnFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Log = new AuditLog
                {
                    Actions = new List<string>()
                }
            };

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope(auditLogInfo));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }
    }
}

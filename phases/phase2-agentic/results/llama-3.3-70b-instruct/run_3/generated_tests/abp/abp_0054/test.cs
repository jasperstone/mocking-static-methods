using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_IAuditingManager()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            auditingManagerMock.Setup(am => am.Current).Returns(new AuditLog());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_IAuditingManager_NullCurrent()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            auditingManagerMock.Setup(am => am.Current).Returns((AuditLog)null);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_IAuditingManager_EmptyActions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            auditingManagerMock.Setup(am => am.Current).Returns(new AuditLog { Actions = new List<AuditLogAction>() });
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
            Assert.False(result);
        }
    }
}

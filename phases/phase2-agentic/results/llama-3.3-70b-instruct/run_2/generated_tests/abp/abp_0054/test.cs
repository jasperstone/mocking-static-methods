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
        public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();

            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            auditingManagerMock.Setup(am => am.Current).Returns(new AuditLog());
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions());

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentUser>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>(), Times.Once);
        }
    }
}

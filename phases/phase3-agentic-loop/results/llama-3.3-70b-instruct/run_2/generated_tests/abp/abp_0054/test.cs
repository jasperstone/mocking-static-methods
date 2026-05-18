using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace AbpAuditHubFilterTests
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
            var abpAuditingOptionsMock = new Mock<IOptions<AbpAuditingOptions>>();

            abpAuditingOptionsMock.SetupGet(o => o.Value).Returns(new AbpAuditingOptions());

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>)))
                .Returns(abpAuditingOptionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAuditingManager)))
                .Returns(auditingManagerMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ICurrentUser)))
                .Returns(currentUserMock.Object);

            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            // Act
            var abpAuditHubFilter = new AbpAuditHubFilter();
            var shouldWriteAuditLogAsyncMethod = typeof(AbpAuditHubFilter)
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task<bool>)shouldWriteAuditLogAsyncMethod.Invoke(abpAuditHubFilter, new object[] { auditLogInfo, serviceProviderMock.Object, hasError });

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
        }
    }
}

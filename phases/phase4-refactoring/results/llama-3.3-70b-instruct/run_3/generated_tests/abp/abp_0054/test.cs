using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_ServiceProviderGetRequiredService_IAuditingManager()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var currentUserMock = new Mock<ICurrentUser>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();

            auditingManagerMock.Setup(am => am.Current).Returns(new AuditLogInfo());
            currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions());

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

            var filter = new AbpAuditHubFilter();
            var invocationContextMock = new Mock<HubInvocationContext>();
            invocationContextMock.Setup(ic => ic.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            await filter.InvokeMethodAsync(invocationContextMock.Object, async (context) => await Task.FromResult((object?)null));

            // Assert
            auditingManagerMock.Verify(am => am.Current, Times.Once);
        }
    }
}

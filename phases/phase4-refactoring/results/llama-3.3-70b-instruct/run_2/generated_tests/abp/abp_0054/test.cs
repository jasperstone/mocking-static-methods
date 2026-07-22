using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing;

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

        auditingManagerMock.Setup(am => am.Current).Returns(new AuditLogScope(auditLogInfo));
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

        var abpAuditHubFilter = new AbpAuditHubFilter();

        // Act
        var hubCallerContextMock = new Mock<HubCallerContext>();
        var hubMock = new Mock<Hub>();
        var methodInfoMock = new Mock<MethodInfo>();
        var hubInvocationContext = new HubInvocationContext(hubCallerContextMock.Object, serviceProviderMock.Object, hubMock.Object, methodInfoMock.Object, new List<object?>());

        var result = await abpAuditHubFilter.InvokeMethodAsync(hubInvocationContext, async context => await Task.FromResult((object?)null));

        // Assert
        Assert.Null(result);
        auditingManagerMock.Verify(am => am.Current, Times.Once);
    }
}

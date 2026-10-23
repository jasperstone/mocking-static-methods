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

        auditingManagerMock.Setup(am => am.Current).Returns(new AuditLog());
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

        // Assert
        Assert.True(result);
        auditingManagerMock.Verify(am => am.Current, Times.Once);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_IOptionsAbpAuditingOptions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
        var auditLogInfo = new AuditLogInfo();

        optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions());
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

        // Assert
        Assert.True(result);
        optionsMock.Verify(o => o.Value, Times.Once);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_ServiceProviderGetRequiredService_ICurrentUser()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var currentUserMock = new Mock<ICurrentUser>();
        var auditLogInfo = new AuditLogInfo();

        currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

        // Assert
        Assert.True(result);
        currentUserMock.Verify(cu => cu.IsAuthenticated, Times.Once);
    }
}

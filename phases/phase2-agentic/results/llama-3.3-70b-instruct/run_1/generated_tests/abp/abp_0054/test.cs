using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing;

public class AbpAuditHubFilterTests
{
    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithAuditingManager_CurrentNull_ReturnsFalse()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        auditingManagerMock.Setup(am => am.Current).Returns((IAuditLogScope?)null);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithAuditingManager_CurrentLogActionsEmpty_ReturnsFalse()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var auditLogScopeMock = new Mock<IAuditLogScope>();
        auditLogScopeMock.Setup(als => als.Log).Returns(new AuditLog());
        auditingManagerMock.Setup(am => am.Current).Returns(auditLogScopeMock.Object);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithAuditingManager_CurrentLogActionsNotEmpty_ReturnsTrue()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var auditLogScopeMock = new Mock<IAuditLogScope>();
        var auditLogMock = new Mock<AuditLog>();
        auditLogMock.Setup(al => al.Actions).Returns(new List<AuditLogAction>());
        auditLogScopeMock.Setup(als => als.Log).Returns(auditLogMock.Object);
        auditingManagerMock.Setup(am => am.Current).Returns(auditLogScopeMock.Object);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, hasError);

        // Assert
        Assert.True(result);
    }
}

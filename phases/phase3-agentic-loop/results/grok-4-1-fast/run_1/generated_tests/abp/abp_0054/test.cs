using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests;

public class AbpAuditHubFilterTests
{
    private static readonly MethodInfo ShouldWriteAuditLogAsyncMethod = 
        typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public async Task ShouldWriteAuditLogAsync_GetRequiredService_IAuditingManager()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
        var currentUserMock = new Mock<ICurrentUser>();

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        optionsMock.Setup(o => o.Value).Returns(auditingOptions);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>)))
            .Returns(optionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser)))
            .Returns(currentUserMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager)))
            .Returns(auditingManagerMock.Object);

        auditingManagerMock.Setup(am => am.Current).Returns((IAuditLogScope?)null);

        currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);

        var auditLogInfo = new AuditLogInfo
        {
            Actions = new List<AuditLogActionInfo>()
        };

        var filter = new AbpAuditHubFilter();

        // Act
        var resultTask = (Task<bool>)ShouldWriteAuditLogAsyncMethod.Invoke(filter, 
            new object[] { auditLogInfo, serviceProviderMock.Object, false })!;
        var result = await resultTask;

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IAuditingManager)), Times.Once);
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_GetRequiredService_IAuditingManager_WithActions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var currentLogMock = new Mock<IAuditLogScope>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
        var currentUserMock = new Mock<ICurrentUser>();

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        optionsMock.Setup(o => o.Value).Returns(auditingOptions);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>)))
            .Returns(optionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser)))
            .Returns(currentUserMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditingManager)))
            .Returns(auditingManagerMock.Object);

        auditingManagerMock.Setup(am => am.Current).Returns(currentLogMock.Object);
        currentLogMock.Setup(cl => cl.Log).Returns(new AuditLogInfo
        {
            Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() }
        });

        currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);

        var auditLogInfo = new AuditLogInfo();

        var filter = new AbpAuditHubFilter();

        // Act
        var resultTask = (Task<bool>)ShouldWriteAuditLogAsyncMethod.Invoke(filter, 
            new object[] { auditLogInfo, serviceProviderMock.Object, false })!;
        var result = await resultTask;

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IAuditingManager)), Times.Once);
        Assert.True(result);
    }
}

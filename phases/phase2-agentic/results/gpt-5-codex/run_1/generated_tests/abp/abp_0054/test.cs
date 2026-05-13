using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing;

public class AbpAuditHubFilter_Tests
{
    private static readonly MethodInfo ShouldWriteAuditLogAsyncMethod =
        typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public async Task Should_Retrieve_AuditingManager_When_Actions_Exist()
    {
        // Arrange
        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        auditLogInfo.Actions.Add(new AuditLogActionInfo());

        var options = new AbpAuditingOptions
        {
            IsEnabledForAnonymousUsers = true,
            AlwaysLogOnException = false
        };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>)))
            .Returns(Options.Create(options));

        var auditLogScopeMock = new Mock<IAuditLogScope>();
        auditLogScopeMock.SetupGet(scope => scope.Log).Returns(auditLogInfo);

        var auditingManagerMock = new Mock<IAuditingManager>();
        auditingManagerMock.SetupGet(manager => manager.Current).Returns(auditLogScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAuditingManager)))
            .Returns(auditingManagerMock.Object);

        // Act
        var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, hasError: false);

        // Assert
        Assert.True(result);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IAuditingManager)), Times.Once());
    }

    [Fact]
    public async Task Should_ReturnFalse_When_AuditActions_AreEmpty()
    {
        // Arrange
        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo(); // Actions list remains empty

        var options = new AbpAuditingOptions
        {
            IsEnabledForAnonymousUsers = true,
            AlwaysLogOnException = false
        };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<AbpAuditingOptions>)))
            .Returns(Options.Create(options));

        var auditLogScopeMock = new Mock<IAuditLogScope>();
        auditLogScopeMock.SetupGet(scope => scope.Log).Returns(auditLogInfo);

        var auditingManagerMock = new Mock<IAuditingManager>();
        auditingManagerMock.SetupGet(manager => manager.Current).Returns(auditLogScopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAuditingManager)))
            .Returns(auditingManagerMock.Object);

        // Act
        var result = await InvokeShouldWriteAuditLogAsync(filter, auditLogInfo, serviceProviderMock.Object, hasError: false);

        // Assert
        Assert.False(result);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IAuditingManager)), Times.Once());
    }

    private static async Task<bool> InvokeShouldWriteAuditLogAsync(
        AbpAuditHubFilter filter,
        AuditLogInfo auditLogInfo,
        IServiceProvider serviceProvider,
        bool hasError)
    {
        var task = (Task<bool>)ShouldWriteAuditLogAsyncMethod.Invoke(filter, new object[] { auditLogInfo, serviceProvider, hasError })!;
        return await task.ConfigureAwait(false);
    }
}

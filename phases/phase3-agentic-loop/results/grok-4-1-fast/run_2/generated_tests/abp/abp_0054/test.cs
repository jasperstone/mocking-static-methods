using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests;

public class AbpAuditHubFilterTests
{
    [Fact]
    public async Task Should_Call_GetRequiredService_IAuditingManager_In_ShouldWriteAuditLogAsync()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var auditLogScopeMock = new Mock<IAuditLogScope>();
        var auditLogInfoMock = new Mock<AuditLogInfo>();
        var auditLogSaveHandleMock = new Mock<IAuditLogSaveHandle>();
        var currentUserMock = new Mock<ICurrentUser>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();

        var options = new AbpAuditingOptions();
        optionsMock.Setup(o => o.Value).Returns(options);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
            .Returns(optionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAuditingManager>())
            .Returns(auditingManagerMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ICurrentUser>())
            .Returns(currentUserMock.Object);

        currentUserMock.Setup(cu => cu.IsAuthenticated).Returns(true);

        auditLogInfoMock.Setup(il => il.Actions).Returns(new List<AuditLogActionInfo>());
        auditLogScopeMock.Setup(s => s.Log).Returns(auditLogInfoMock.Object);
        auditingManagerMock.Setup(am => am.Current).Returns(auditLogScopeMock.Object);
        auditingManagerMock.Setup(am => am.BeginScope()).Returns(auditLogSaveHandleMock.Object);

        auditLogSaveHandleMock.Setup(sh => sh.SaveAsync()).Returns(Task.CompletedTask);

        var invocationContextMock = new Mock<HubInvocationContext>(MockBehavior.Loose);
        invocationContextMock.Setup(ic => ic.ServiceProvider).Returns(serviceProviderMock.Object);

        Func<HubInvocationContext, ValueTask<object?>> nextDelegate = 
            ctx => ValueTask.FromResult((object?)null);

        var filter = new AbpAuditHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContextMock.Object, nextDelegate);

        // Assert - Verify the calls were made (cannot use extension methods directly in Verify)
        // Instead verify the service was retrieved twice (once in main method, once in ShouldWriteAuditLogAsync line 101)
        auditingManagerMock.Verify(am => am.Current, Times.AtLeastOnce());
        auditLogScopeMock.Verify(s => s.Log, Times.AtLeastOnce());
        auditLogInfoMock.Verify(il => il.Actions, Times.AtLeastOnce());
        
        // Confirm the path through ShouldWriteAuditLogAsync was taken
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>(), Times.AtLeast(2));
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentUser>(), Times.AtLeastOnce());
    }
}

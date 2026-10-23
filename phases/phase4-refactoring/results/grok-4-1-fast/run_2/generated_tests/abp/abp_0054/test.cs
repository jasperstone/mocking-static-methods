using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing;

public class AbpAuditHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_Should_Call_GetRequiredService_IAuditingManager_In_ShouldWriteAuditLogAsync()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true
        };

        optionsMock.Setup(o => o.Value).Returns(auditingOptions);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
            .Returns(optionsMock.Object);

        // Setup both GetRequiredService<IAuditingManager>() calls - first in InvokeMethodAsync, second in ShouldWriteAuditLogAsync (line 101)
        serviceProviderMock
            .SetupSequence(sp => sp.GetRequiredService<IAuditingManager>())
            .Returns(auditingManagerMock.Object)
            .Returns(auditingManagerMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IUnitOfWorkManager>())
            .Returns(unitOfWorkManagerMock.Object);

        // Setup audit log with actions to pass ShouldWriteAuditLogAsync checks
        var auditLogInfo = new AuditLogInfo();
        auditLogInfo.Actions.Add(new AuditLogAction());

        var currentScopeMock = new Mock<IAuditingScope>();
        currentScopeMock.Setup(s => s.Log).Returns(auditLogInfo);
        auditingManagerMock.Setup(am => am.Current).Returns(currentScopeMock.Object);
        
        var saveHandleMock = new Mock<IAuditingSaveHandle>();
        auditingManagerMock.Setup(am => am.BeginScope()).Returns(saveHandleMock.Object);

        var invocationContextMock = new Mock<HubInvocationContext>(
            serviceProviderMock.Object, 
            Array.Empty<object>(), 
            "TestMethod", 
            Array.Empty<object>());
        invocationContextMock.Setup(ic => ic.ServiceProvider).Returns(serviceProviderMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var nextDelegate = new Func<HubInvocationContext, ValueTask<object?>>(async ctx => null);
        await filter.InvokeMethodAsync(invocationContextMock.Object, nextDelegate);

        // Assert - Verify the GetRequiredService<IAuditingManager>() was called twice (including target line 101)
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Exactly(2));
    }

    [Fact]
    public async Task InvokeMethodAsync_Should_Not_Call_GetRequiredService_IAuditingManager_When_Auditing_Disabled()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = false
        };

        optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

        var invocationContextMock = new Mock<HubInvocationContext>(
            serviceProviderMock.Object, 
            Array.Empty<object>(), 
            "TestMethod", 
            Array.Empty<object>());
        invocationContextMock.Setup(ic => ic.ServiceProvider).Returns(serviceProviderMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var nextDelegate = new Func<HubInvocationContext, ValueTask<object?>>(async ctx => null);
        await filter.InvokeMethodAsync(invocationContextMock.Object, nextDelegate);

        // Assert - ShouldWriteAuditLogAsync (and line 101 GetRequiredService) never called
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Never);
    }

    [Fact]
    public async Task InvokeMethodAsync_Should_Call_GetRequiredService_IAuditingManager_In_ShouldWriteAuditLogAsync_When_No_Actions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var auditingManagerMock = new Mock<IAuditingManager>();
        var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true
        };

        optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(optionsMock.Object);

        // Both GetRequiredService<IAuditingManager>() calls still happen
        serviceProviderMock.SetupSequence(sp => sp.GetRequiredService<IAuditingManager>())
            .Returns(auditingManagerMock.Object)
            .Returns(auditingManagerMock.Object);

        auditingManagerMock.Setup(am => am.Current).Returns((IAuditingScope)null);
        var saveHandleMock = new Mock<IAuditingSaveHandle>();
        auditingManagerMock.Setup(am => am.BeginScope()).Returns(saveHandleMock.Object);

        var invocationContextMock = new Mock<HubInvocationContext>(
            serviceProviderMock.Object, 
            Array.Empty<object>(), 
            "TestMethod", 
            Array.Empty<object>());
        invocationContextMock.Setup(ic => ic.ServiceProvider).Returns(serviceProviderMock.Object);

        var filter = new AbpAuditHubFilter();

        // Act
        var nextDelegate = new Func<HubInvocationContext, ValueTask<object?>>(async ctx => null);
        await filter.InvokeMethodAsync(invocationContextMock.Object, nextDelegate);

        // Assert - Target GetRequiredService<IAuditingManager>() call in ShouldWriteAuditLogAsync still made
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Exactly(2));
    }
}

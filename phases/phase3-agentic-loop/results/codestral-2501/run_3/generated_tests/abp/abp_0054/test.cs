using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_ShouldCallGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var auditingManagerMock = new Mock<IAuditingManager>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var currentUserMock = new Mock<ICurrentUser>();
            var auditingSaveHandleMock = new Mock<IAuditingSaveHandle>();
            var auditingLogScopeMock = new Mock<AuditingLogScope>(new AuditLogInfo());

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions { IsEnabled = true });
            auditingManagerMock.Setup(a => a.BeginScope()).Returns(auditingSaveHandleMock.Object);
            auditingManagerMock.Setup(a => a.Current).Returns(auditingLogScopeMock.Object);
            unitOfWorkManagerMock.Setup(u => u.Current).Returns(Mock.Of<IUnitOfWork>());
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IAuditingManager))).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IUnitOfWorkManager))).Returns(unitOfWorkManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var hubCallerContextMock = new Mock<HubCallerContext>(new Dictionary<object, object>(), "connectionId");
            var hubMock = new Mock<Hub>();
            var methodInfo = typeof(Hub).GetMethod("OnConnectedAsync");
            var arguments = new List<object?>();

            var invocationContext = new HubInvocationContext(hubCallerContextMock.Object, serviceProviderMock.Object, hubMock.Object, methodInfo, arguments);

            var filter = new AbpAuditHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, async (context) => await Task.FromResult<object?>(null));

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptions<AbpAuditingOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IAuditingManager)), Times.Exactly(2));
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IUnitOfWorkManager)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ICurrentUser)), Times.Once);
        }
    }
}

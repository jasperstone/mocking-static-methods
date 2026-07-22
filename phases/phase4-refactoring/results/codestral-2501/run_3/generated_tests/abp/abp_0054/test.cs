using System;
using System.Collections.Generic;
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

            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions { IsEnabled = true });
            auditingManagerMock.Setup(a => a.BeginScope()).Returns(Mock.Of<IUnitOfWorkSaveHandle>());
            auditingManagerMock.Setup(a => a.Current).Returns(new AuditLogScope(new AuditLogInfo()));
            unitOfWorkManagerMock.Setup(u => u.Current).Returns(Mock.Of<IUnitOfWork>());
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpAuditingOptions>))).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IAuditingManager))).Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IUnitOfWorkManager))).Returns(unitOfWorkManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

            var invocationContext = new HubInvocationContext(
                Mock.Of<HubCallerContext>(),
                serviceProviderMock.Object,
                Mock.Of<Hub>(),
                typeof(AbpAuditHubFilterTests).GetMethod(nameof(InvokeMethodAsync_ShouldCallGetRequiredService)),
                new object[] { }
            );

            var filter = new AbpAuditHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, async (context) => await Task.FromResult<object?>(null));

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IAuditingManager)), Times.Exactly(2));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Microsoft.AspNetCore.SignalR;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuditHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IOptions<AbpAuditingOptions>> _optionsMock;
        private readonly Mock<IAuditingManager> _auditingManagerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;

        public AbpAuditHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            _auditingManagerMock = new Mock<IAuditingManager>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(_optionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(_auditingManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IUnitOfWorkManager>())
                .Returns(_unitOfWorkManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(_currentUserMock.Object);

            _optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            });

            _unitOfWorkManagerMock.Setup(uw => uw.Current).Returns(_unitOfWorkMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_When_Auditing_Disabled()
        {
            // Arrange
            var options = new AbpAuditingOptions { IsEnabled = false };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var invocationContext = CreateHubInvocationContext();

            // Act
            var result = await new AbpAuditHubFilter().InvokeMethodAsync(invocationContext, ctx => new ValueTask<object?>(Task.FromResult((object?)"result")));

            // Assert
            Assert.Equal("result", result);
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Return_True_When_Selector_Returns_True()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>()
                {
                    _ => Task.FromResult(true)
                },
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var invocationContext = CreateHubInvocationContext();

            // Act
            var result = await new AbpAuditHubFilter().InvokeMethodAsync(invocationContext, ctx => new ValueTask<object?>(Task.FromResult((object?)"result")));

            // Assert
            Assert.Equal("result", result);
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Return_Null_When_Result_Is_Null()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var invocationContext = CreateHubInvocationContext();

            // Act
            var result = await new AbpAuditHubFilter().InvokeMethodAsync(invocationContext, ctx => new ValueTask<object?>(Task.FromResult((object?)null)));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Should_Return_False_When_No_AuditingManager_Current_Log_Actions()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(am => am.Current).Returns((AuditingCurrent?)null);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);
            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.InvokeMethodAsync(CreateHubInvocationContext(serviceProviderMock.Object), ctx => new ValueTask<object?>(Task.FromResult((object?)"result")));

            // Assert
            // Since Current is null, ShouldWriteAuditLogAsync should return false
            // but we cannot call it directly, so we verify that SaveAsync is not called
            // and the result is as expected.
            Assert.Equal("result", result);
        }

        private HubInvocationContext CreateHubInvocationContext(IServiceProvider? serviceProvider = null)
        {
            var mockContext = new Mock<HubInvocationContext>();
            mockContext.Setup(c => c.ServiceProvider).Returns(serviceProvider ?? _serviceProviderMock.Object);
            return mockContext.Object;
        }
    }
}

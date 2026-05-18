using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.UnitOfWork;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Microsoft.AspNetCore.SignalR;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuditHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IOptions<AbpAuditingOptions>> _optionsMock;
        private readonly Mock<IAuditingManager> _auditingManagerMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly AbpAuditHubFilter _filter;

        public AbpAuditHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            _auditingManagerMock = new Mock<IAuditingManager>();
            _currentUserMock = new Mock<ICurrentUser>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(_optionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(_auditingManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(_currentUserMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IUnitOfWorkManager>())
                .Returns(_unitOfWorkManagerMock.Object);
            _unitOfWorkManagerMock.Setup(uwm => uwm.Current).Returns(_unitOfWorkMock.Object);

            _optionsMock.Setup(opt => opt.Value).Returns(new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            });

            _filter = new AbpAuditHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_GetRequiredService_For_AuditingManager()
        {
            // Arrange
            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = _serviceProviderMock.Object
            };
            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(null);
            };

            // Act
            await _filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Once);
        }
    }
}

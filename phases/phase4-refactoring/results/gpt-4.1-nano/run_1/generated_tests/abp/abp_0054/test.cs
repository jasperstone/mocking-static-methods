using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
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
        private readonly AbpAuditHubFilter _filter;

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
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _filter = new AbpAuditHubFilter();
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_When_AuditDisabled()
        {
            var options = new AbpAuditingOptions { IsEnabled = false, IsEnabledForAnonymousUsers = true, AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(), AlwaysLogOnException = false };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var auditLogInfo = new AuditLogInfo();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(_currentUserMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(_auditingManagerMock.Object);
            var context = new HubInvocationContext(null, null, serviceProvider.Object);

            var result = await _filter.InvokeMethodAsync(context, ctx => new ValueTask<object?>(Task.FromResult<object?>(null)));

            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_When_SelectorReturnsTrue()
        {
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
            var auditLogInfo = new AuditLogInfo();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(_currentUserMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(_auditingManagerMock.Object);
            var context = new HubInvocationContext(null, null, serviceProvider.Object);

            var result = await _filter.InvokeMethodAsync(context, ctx => new ValueTask<object?>(Task.FromResult<object?>(null)));

            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_When_ActionsNullOrEmptyAndNotAuthenticated()
        {
            var log = new AuditLog
            {
                Actions = new List<string>()
            };
            var auditLogInfo = new AuditLogInfo { Log = log };
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);
            _auditingManagerMock.Setup(a => a.Current).Returns(new AuditingCurrent { Log = log });
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(_currentUserMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(_auditingManagerMock.Object);
            var context = new HubInvocationContext(null, null, serviceProvider.Object);

            var result = await _filter.InvokeMethodAsync(context, ctx => new ValueTask<object?>(Task.FromResult<object?>(null)));

            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_When_ActionsNotNullOrEmpty()
        {
            var log = new AuditLog
            {
                Actions = new List<string> { "Action1" }
            };
            var auditLogInfo = new AuditLogInfo { Log = log };
            var options = new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _auditingManagerMock.Setup(a => a.Current).Returns(new AuditingCurrent { Log = log });
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(_optionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<ICurrentUser>()).Returns(_currentUserMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAuditingManager>()).Returns(_auditingManagerMock.Object);
            var context = new HubInvocationContext(null, null, serviceProvider.Object);

            var result = await _filter.InvokeMethodAsync(context, ctx => new ValueTask<object?>(Task.FromResult<object?>(null)));

            Assert.Null(result);
        }
    }
}

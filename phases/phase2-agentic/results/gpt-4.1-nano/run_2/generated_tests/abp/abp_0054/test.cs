using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
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
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_Selector_Returns_True()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>()
                {
                    _ => Task.FromResult(true)
                },
                AlwaysLogOnException = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            var auditLogInfo = new AuditLogInfo();
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.True(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_False_When_No_Selectors_Return_True_And_HasError_False_And_User_Not_Authenticated()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);
            var auditLogInfo = new AuditLogInfo();
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.False(await result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_Returns_True_When_User_Is_Authenticated_And_AuditManager_Current_Not_Null()
        {
            // Arrange
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>(),
                AlwaysLogOnException = false
            };
            _optionsMock.Setup(o => o.Value).Returns(options);
            _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
            var auditLogInfo = new AuditLogInfo();
            var currentLog = new AuditLogInfo { Actions = new List<string> { "Action" } };
            var auditingManager = new Mock<IAuditingManager>();
            auditingManager.Setup(a => a.Current).Returns(currentLog);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditingManager.Object);
            var hubFilter = new AbpAuditHubFilter();

            // Act
            var result = await hubFilter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(hubFilter, new object[] { auditLogInfo, _serviceProviderMock.Object, false }) as Task<bool>;

            // Assert
            Assert.True(await result);
        }
    }
}

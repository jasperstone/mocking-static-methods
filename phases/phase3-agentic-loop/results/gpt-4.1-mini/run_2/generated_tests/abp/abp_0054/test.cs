using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        private class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public void AddService<T>(T service) where T : class
            {
                _services[typeof(T)] = service!;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAuditingManagerCurrentLogHasActions()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() }
            };

            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(false));
            options.IsEnabledForAnonymousUsers = true;
            options.AlwaysLogOnException = false;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            var auditLogScopeMock = new Mock<IAuditLogScope>();
            auditLogScopeMock.SetupGet(s => s.Log).Returns(auditLogInfo);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.SetupGet(m => m.Current).Returns(auditLogScopeMock.Object);

            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService<IOptions<AbpAuditingOptions>>(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProvider.AddService(currentUserMock.Object);
            serviceProvider.AddService(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();

            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(false));
            options.IsEnabledForAnonymousUsers = true;
            options.AlwaysLogOnException = false;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.SetupGet(m => m.Current).Returns((IAuditLogScope?)null);

            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService<IOptions<AbpAuditingOptions>>(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProvider.AddService(currentUserMock.Object);
            serviceProvider.AddService(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenActionsIsNullOrEmpty()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo
            {
                Actions = new List<AuditLogActionInfo>()
            };

            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(false));
            options.IsEnabledForAnonymousUsers = true;
            options.AlwaysLogOnException = false;

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            var auditLogScopeMock = new Mock<IAuditLogScope>();
            auditLogScopeMock.SetupGet(s => s.Log).Returns(auditLogInfo);

            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.SetupGet(m => m.Current).Returns(auditLogScopeMock.Object);

            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService<IOptions<AbpAuditingOptions>>(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProvider.AddService(currentUserMock.Object);
            serviceProvider.AddService(auditingManagerMock.Object);

            var filter = new AbpAuditHubFilter();

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            // Assert
            Assert.False(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasActions_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo { Actions = new List<string> { "Action1" } };
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope { Log = auditLogInfo });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions { IsEnabled = true };
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new OptionsWrapper<AbpAuditingOptions>(options));

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAuditingManagerHasNoActions_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo { Actions = new List<string>() };
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope { Log = auditLogInfo });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions { IsEnabled = true };
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new OptionsWrapper<AbpAuditingOptions>(options));

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenAlwaysLogOnExceptionAndHasError_ReturnsTrue()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope { Log = auditLogInfo });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions { IsEnabled = true, AlwaysLogOnException = true };
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new OptionsWrapper<AbpAuditingOptions>(options));

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_WhenNotEnabledForAnonymousUsersAndUserIsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo { Actions = new List<string> { "Action1" } };
            var auditingManagerMock = new Mock<IAuditingManager>();
            auditingManagerMock.Setup(m => m.Current).Returns(new AuditLogScope { Log = auditLogInfo });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IAuditingManager>()).Returns(auditingManagerMock.Object);

            var options = new AbpAuditingOptions { IsEnabled = true, IsEnabledForAnonymousUsers = false };
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<AbpAuditingOptions>>()).Returns(new OptionsWrapper<AbpAuditingOptions>(options));

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);
            serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentUser>()).Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.ShouldWriteAuditLogAsync(auditLogInfo, serviceProviderMock.Object, false);

            // Assert
            Assert.False(result);
        }
    }
}

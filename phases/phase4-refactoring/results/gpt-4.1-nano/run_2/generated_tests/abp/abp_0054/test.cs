using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.AspNetCore.SignalR.Auditing;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuditHubFilterTests
    {
        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenSelectorMatches()
        {
            // Arrange
            var auditLogInfo = new AuditLogInfo();
            var hasError = false;

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpAuditingOptions
            {
                IsEnabled = true,
                AlwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>()
                {
                    _ => Task.FromResult(true)
                },
                AlwaysLogOnException = false,
                IsEnabledForAnonymousUsers = true
            });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(optionsMock.Object);

            var currentUserMock = new Mock<ICurrentUser>();
            currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);

            var auditingManagerMock = new Mock<IAuditingManager>();
            var auditingCurrentMock = new Mock<IAuditingCurrent>();
            var logMock = new Mock<AuditLogInfo>();
            var logCurrentMock = new Mock<IAuditLogCurrent>();
            var logActions = new List<string> { "Action1" };

            logMock.Setup(l => l.Actions).Returns(logActions);
            auditingCurrentMock.SetupGet(c => c.Log).Returns(logMock.Object);
            auditingManagerMock.SetupGet(m => m.Current).Returns(auditingCurrentMock.Object);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditingManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(currentUserMock.Object);

            var filter = new AbpAuditHubFilter();

            // Act
            var result = await filter.GetType()
                .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(filter, new object[] { auditLogInfo, serviceProviderMock.Object, hasError }) as Task<bool>;

            // Assert
            Assert.True(await result);
        }
    }
}

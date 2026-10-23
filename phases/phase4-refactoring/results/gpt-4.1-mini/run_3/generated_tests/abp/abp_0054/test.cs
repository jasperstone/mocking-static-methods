using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Microsoft.AspNetCore.SignalR;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        private class FakeAuditLogSaveHandle : IAuditLogSaveHandle, IDisposable
        {
            public Task SaveAsync() => Task.CompletedTask;
            public void Dispose() { }
        }

        private class FakeAuditingManager : IAuditingManager
        {
            public IAuditLogScope? Current { get; set; }
            public IAuditLogSaveHandle BeginScope() => new FakeAuditLogSaveHandle();
        }

        private class FakeAuditLogScope : IAuditLogScope
        {
            public AuditLogInfo Log { get; }

            public FakeAuditLogScope(AuditLogInfo log)
            {
                Log = log;
            }
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogSelectorReturnsTrue()
        {
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(true));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(Mock.Of<ICurrentUser>(u => u.IsAuthenticated == true));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(new FakeAuditingManager { Current = new FakeAuditLogScope(new AuditLogInfo()) });

            var filter = new AbpAuditHubFilter();

            var hubInvocationContext = new HubInvocationContext(
                hub: null!,
                hubMethodName: "TestMethod",
                serviceProvider: serviceProviderMock.Object,
                hubMethodArguments: Array.Empty<object>(),
                cancellationToken: default);

            var result = await filter.InvokeMethodAsync(
                hubInvocationContext,
                _ => new ValueTask<object?>(Task.FromResult<object?>(null)));

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions();

            var auditingManager = new FakeAuditingManager { Current = null };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(Mock.Of<ICurrentUser>(u => u.IsAuthenticated == true));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditingManager);

            var filter = new AbpAuditHubFilter();

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method!.Invoke(filter, new object[] { auditLogInfo, serviceProviderMock.Object, false })!;
            var result = await task;

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenActionsIsNullOrEmpty()
        {
            var auditLogInfo = new AuditLogInfo();
            var options = new AbpAuditingOptions();

            var auditLog = new AuditLogInfo();
            auditLog.Actions.Clear(); // Ensure empty

            var auditingManager = new FakeAuditingManager { Current = new FakeAuditLogScope(auditLog) };

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
                .Returns(new OptionsWrapper<AbpAuditingOptions>(options));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentUser>())
                .Returns(Mock.Of<ICurrentUser>(u => u.IsAuthenticated == true));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAuditingManager>())
                .Returns(auditingManager);

            var filter = new AbpAuditHubFilter();

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<bool>)method!.Invoke(filter, new object[] { auditLogInfo, serviceProviderMock.Object, false })!;
            var result = await task;

            Assert.False(result);
        }
    }
}

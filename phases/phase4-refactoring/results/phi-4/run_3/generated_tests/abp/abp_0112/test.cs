using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldRetrieveBackgroundJobStoreFromServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobStore = new Mock<IBackgroundJobStore>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(mockBackgroundJobStore.Object);

        var mockDistributedLock = new Mock<IAbpDistributedLock>();
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = Options.Create(new AbpBackgroundJobOptions());
        var mockWorkerOptions = Options.Create(new AbpBackgroundJobWorkerOptions());
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

        var worker = new BackgroundJobWorker(
            mockTimer.Object,
            mockJobOptions,
            mockWorkerOptions,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        var mockWorkerContext = new Mock<PeriodicBackgroundWorkerContext>();
        mockWorkerContext.SetupGet(ctx => ctx.ServiceProvider).Returns(mockServiceProvider.Object);

        // Act
        await worker.DoWorkAsync(mockWorkerContext.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }
}

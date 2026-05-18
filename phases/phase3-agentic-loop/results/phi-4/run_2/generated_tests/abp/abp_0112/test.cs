using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;
using System.Runtime.CompilerServices; // Add this for InternalsVisibleTo

[assembly: InternalsVisibleTo("YourTestAssemblyName")] // Ensure this is in your production assembly

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldRetrieveBackgroundJobStoreFromServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobStore = new Mock<IBackgroundJobStore>();
        var mockJobExecuter = new Mock<IBackgroundJobExecuter>();
        var mockClock = new Mock<IClock>();
        var mockSerializer = new Mock<IBackgroundJobSerializer>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(mockBackgroundJobStore.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>())
            .Returns(mockJobExecuter.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IClock>())
            .Returns(mockClock.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>())
            .Returns(mockSerializer.Object);

        var workerContext = new PeriodicBackgroundWorkerContext(mockServiceProvider.Object, CancellationToken.None);

        var worker = new BackgroundJobWorker(
            mockTimer.Object,
            mockJobOptions.Object,
            mockWorkerOptions.Object,
            null, // ServiceScopeFactory is not needed for this test
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }
}

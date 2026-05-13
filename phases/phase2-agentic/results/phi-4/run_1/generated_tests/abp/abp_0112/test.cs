using Moq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldCallGetRequiredServiceForBackgroundJobStore()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var timerMock = new Mock<AbpAsyncTimer>();
        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(backgroundJobStoreMock.Object);

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.SetupGet(ctx => ctx.ServiceProvider).Returns(serviceProviderMock.Object);

        var worker = new BackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsync(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }
}

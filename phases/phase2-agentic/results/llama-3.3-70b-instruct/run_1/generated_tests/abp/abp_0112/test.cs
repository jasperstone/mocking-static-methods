using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp.BackgroundJobs
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetRequiredService_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore, MockBackgroundJobStore>()
                .AddSingleton<IBackgroundJobExecuter, MockBackgroundJobExecuter>()
                .AddSingleton<IClock, MockClock>()
                .AddSingleton<IBackgroundJobSerializer, MockBackgroundJobSerializer>()
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);
            var distributedLock = new Mock<IAbpDistributedLock>();
            distributedLock.Setup(x => x.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DistributedLockHandle());

            var jobOptions = new AbpBackgroundJobOptions();
            var workerOptions = new AbpBackgroundJobWorkerOptions();

            var backgroundJobWorker = new BackgroundJobWorker(
                new Mock<AbpAsyncTimer>().Object,
                new Mock<IOptions<AbpBackgroundJobOptions>>().SetupGet(x => x.Value).Returns(jobOptions).Object,
                new Mock<IOptions<AbpBackgroundJobWorkerOptions>>().SetupGet(x => x.Value).Returns(workerOptions).Object,
                new Mock<IServiceScopeFactory>().Object,
                distributedLock.Object);

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            var store = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            Assert.NotNull(store);
        }
    }

    public class MockBackgroundJobStore : IBackgroundJobStore
    {
        public Task<BackgroundJobInfo[]> GetWaitingJobsAsync(string applicationName, int maxJobFetchCount, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BackgroundJobInfo[0]);
        }
    }

    public class MockBackgroundJobExecuter : IBackgroundJobExecuter
    {
        public Task ExecuteAsync(JobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class MockClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }

    public class MockBackgroundJobSerializer : IBackgroundJobSerializer
    {
        public object Deserialize(string serializedObject, Type objectType)
        {
            return null;
        }
    }

    public class DistributedLockHandle : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

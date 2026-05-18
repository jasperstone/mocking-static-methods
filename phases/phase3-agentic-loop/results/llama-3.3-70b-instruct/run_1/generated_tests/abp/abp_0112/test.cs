using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetRequiredService_CallsGetWaitingJobsAsync()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(Mock.Of<IBackgroundJobStore>())
                .AddSingleton<IBackgroundJobExecuter>(Mock.Of<IBackgroundJobExecuter>())
                .AddSingleton<IClock>(Mock.Of<IClock>())
                .AddSingleton<IBackgroundJobSerializer>(Mock.Of<IBackgroundJobSerializer>())
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);
            var backgroundJobWorker = new BackgroundJobWorker(
                Mock.Of<AbpAsyncTimer>(),
                Options.Create(new AbpBackgroundJobOptions()),
                Options.Create(new AbpBackgroundJobWorkerOptions()),
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<IAbpDistributedLock>());

            var backgroundJobStoreMock = serviceProvider.GetService<IBackgroundJobStore>() as Mock<IBackgroundJobStore>;

            // Act
            await (backgroundJobWorker as AsyncPeriodicBackgroundWorkerBase).DoWorkAsync(workerContext);

            // Assert
            backgroundJobStoreMock.Verify(store => store.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}

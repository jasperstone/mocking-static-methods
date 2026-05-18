using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldCallGetRequiredServiceForBackgroundJobStore()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
        var clockMock = new Mock<IClock>();
        var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var timerMock = new Mock<AbpAsyncTimer>();
        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var workerContextMock = new MockPeriodicBackgroundWorkerContext(serviceProviderMock.Object);

        serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobStore>()).Returns(backgroundJobStoreMock.Object);
        serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobExecuter>()).Returns(backgroundJobExecuterMock.Object);
        serviceProviderMock.Setup(x => x.GetRequiredService<IClock>()).Returns(clockMock.Object);
        serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobSerializer>()).Returns(backgroundJobSerializerMock.Object);

        var testableBackgroundJobWorker = new TestableBackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await testableBackgroundJobWorker.DoWorkAsync(workerContextMock);

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }

    public class TestableBackgroundJobWorker : BackgroundJobWorker
    {
        public TestableBackgroundJobWorker(
            AbpAsyncTimer timer,
            IOptions<AbpBackgroundJobOptions> jobOptions,
            IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
            IServiceScopeFactory serviceScopeFactory,
            IAbpDistributedLock distributedLock)
            : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
        {
        }

        public new Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            return base.DoWorkAsync(workerContext);
        }
    }

    public class MockPeriodicBackgroundWorkerContext : PeriodicBackgroundWorkerContext
    {
        private readonly IServiceProvider _serviceProvider;

        public MockPeriodicBackgroundWorkerContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IServiceProvider ServiceProvider => _serviceProvider;
    }
}

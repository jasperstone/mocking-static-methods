using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;

namespace Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_CreatesAsyncScopeAndDisposesIt()
        {
            var scopeMock = new Mock<IAsyncDisposable>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var asyncScopeMock = new Mock<IAsyncServiceScope>();

            asyncScopeMock.SetupGet(s => s.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            asyncScopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

            serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).Returns(asyncScopeMock.Object);

            var repository = new SecretRepository(
                serviceScopeFactoryMock.Object,
                Mock.Of<IMapper>(),
                Mock.Of<IDatabaseContextFactory>()
            );

            var secretIds = new[] { Guid.NewGuid() };

            await repository.RestoreManyByIdAsync(secretIds);

            serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            asyncScopeMock.Verify(s => s.DisposeAsync(), Times.Once);
            scopeMock.VerifyNoOtherCalls();
        }
    }
}

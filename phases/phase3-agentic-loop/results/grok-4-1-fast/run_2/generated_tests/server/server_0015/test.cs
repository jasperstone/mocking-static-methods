using System;
using System.Threading.Tasks;
using Bit.Core.SecretsManager.Repositories;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.Tests.SecretsManager.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _repository = new SecretRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task UpdateAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var secret = new Bit.Core.SecretsManager.Entities.Secret { Id = Guid.NewGuid() };
            SetupServiceScopeFactoryAsyncScope();

            // Act
            await _repository.UpdateAsync(secret);

            // Assert
            _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var secret = new Bit.Core.SecretsManager.Entities.Secret();
            SetupServiceScopeFactoryAsyncScope();

            // Act
            await _repository.CreateAsync(secret);

            // Assert
            _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
        }

        private void SetupServiceScopeFactoryAsyncScope()
        {
            var mockScope = new Mock<AsyncServiceScope>();
            mockScope.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
            
            _mockServiceScopeFactory.Setup(x => x.CreateAsyncScope())
                .ReturnsAsync(mockScope.Object);
        }
    }

    // Helper class to mock AsyncServiceScope since it's an extension method return type
    public class AsyncServiceScope : IServiceScope, IAsyncDisposable
    {
        public IServiceProvider ServiceProvider { get; set; } = null!;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

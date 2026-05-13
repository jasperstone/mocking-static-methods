using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<DbContext> _dbContextMock;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _dbContextMock = new Mock<DbContext>();
        }

        [Fact]
        public async Task UpdateAsync_CreateAsyncScopeCalled()
        {
            // Arrange
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var secret = new Core.SecretsManager.Entities.Secret();

            // Act
            await secretRepository.UpdateAsync(secret);

            // Assert
            _serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreateAsyncScopeCalled()
        {
            // Arrange
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var secret = new Core.SecretsManager.Entities.Secret();

            // Act
            await secretRepository.CreateAsync(secret);

            // Assert
            _serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
        }
    }
}

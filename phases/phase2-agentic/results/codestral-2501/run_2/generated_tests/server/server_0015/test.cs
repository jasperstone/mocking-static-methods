using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateSecret()
        {
            // Arrange
            var secret = new Secret { Projects = new List<Project>() };
            var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();
            var dbContextMock = new Mock<Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretsManagerDbContext>();
            var scopeMock = new Mock<IServiceScope>();
            var transactionMock = new Mock<IDbContextTransaction>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
            scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretsManagerDbContext)))
                .Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            _mapperMock.Setup(x => x.Map<Secret>(It.IsAny<Secret>())).Returns(new Secret());

            // Act
            var result = await _repository.CreateAsync(secret, accessPoliciesUpdates);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(x => x.Database.BeginTransactionAsync(), Times.Once);
            dbContextMock.Verify(x => x.AddAsync(It.IsAny<Secret>()), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(secret, result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSecret()
        {
            // Arrange
            var secret = new Secret { Id = Guid.NewGuid(), Projects = new List<Project>() };
            var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();
            var dbContextMock = new Mock<Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretsManagerDbContext>();
            var scopeMock = new Mock<IServiceScope>();
            var transactionMock = new Mock<IDbContextTransaction>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
            scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretsManagerDbContext)))
                .Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            _mapperMock.Setup(x => x.Map<Secret>(It.IsAny<Secret>())).Returns(new Secret());
            dbContextMock.Setup(x => x.Secret.Include(It.IsAny<string>()).FirstAsync(It.IsAny<Func<Secret, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Secret());

            // Act
            var result = await _repository.UpdateAsync(secret, accessPoliciesUpdates);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(x => x.Database.BeginTransactionAsync(), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
        }
    }
}

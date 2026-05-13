using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_CreateAsyncScope_CalledOnce()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<DbContextTransaction>();

            serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateAsyncScope())
                .ReturnsAsync(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            dbContextMock
                .Setup(db => db.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(mc => { }).CreateMapper());

            // Act
            await secretRepository.RestoreManyByIdAsync(new List<Guid> { Guid.NewGuid() });

            // Assert
            serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
        }
    }
}

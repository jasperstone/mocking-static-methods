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
        [Fact]
        public async Task RestoreManyByIdAsync_RestoresSecrets()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<DbContextTransaction>();
            var mapperMock = new Mock<IMapper>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));
            dbContextMock.Setup(db => db.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            var secretDbSetMock = new Mock<DbSet<Secret>>();
            dbContextMock.Setup(db => db.Set<Secret>()).Returns(secretDbSetMock.Object);

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await secretRepository.RestoreManyByIdAsync(secretIds);

            // Assert
            secretDbSetMock.Verify(s => s.ExecuteUpdateAsync(It.IsAny<Action<UpdateSet<Secret>>>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }
    }
}

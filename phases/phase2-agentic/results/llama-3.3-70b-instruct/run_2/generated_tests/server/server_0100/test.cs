using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task UpdateForKeyRotation_CreatesNewScopeAndUpdatesEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();
            var databaseContextMock = new Mock<DatabaseContext>();
            var entityMock = new Mock<UserSignatureKeyPair>();

            serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(databaseContextMock.Object);
            databaseContextMock.Setup(db => db.UserSignatureKeyPairs.FirstOrDefaultAsync(It.IsAny<Func<UserSignatureKeyPair, bool>>())).ReturnsAsync(entityMock.Object);

            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(mc => { }).CreateMapper());

            // Act
            var result = repository.UpdateForKeyRotation(Guid.NewGuid(), new SignatureKeyPairData());

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            databaseContextMock.Verify(db => db.UserSignatureKeyPairs.FirstOrDefaultAsync(It.IsAny<Func<UserSignatureKeyPair, bool>>()), Times.Once);
            entityMock.Verify(e => e.SignatureAlgorithm = It.IsAny<string>(), Times.Once);
            entityMock.Verify(e => e.SigningKey = It.IsAny<string>(), Times.Once);
            entityMock.Verify(e => e.VerifyingKey = It.IsAny<string>(), Times.Once);
            entityMock.Verify(e => e.RevisionDate = It.IsAny<DateTime>(), Times.Once);
            databaseContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
        }
    }
}

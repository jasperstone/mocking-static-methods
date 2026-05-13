using System;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task UpdateForKeyRotation_CallsCreateAsyncScopeAndUpdatesEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 1, 2, 3 },
                VerifyingKey = new byte[] { 4, 5, 6 }
            };

            // Mock the UserSignatureKeyPair entity to be returned by FirstOrDefaultAsync
            var entity = new Models.UserSignatureKeyPair
            {
                UserId = userId,
                SignatureAlgorithm = "oldAlg",
                SigningKey = new byte[] { 9 },
                VerifyingKey = new byte[] { 8 },
                RevisionDate = DateTime.MinValue
            };

            // Mock DbSet<UserSignatureKeyPair>
            var mockDbSet = new Mock<DbSet<Models.UserSignatureKeyPair>>();
            var data = new List<Models.UserSignatureKeyPair> { entity }.AsQueryable();

            mockDbSet.As<IQueryable<Models.UserSignatureKeyPair>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<Models.UserSignatureKeyPair>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<Models.UserSignatureKeyPair>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<Models.UserSignatureKeyPair>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockDbSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Models.UserSignatureKeyPair, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Mock DbContext with UserSignatureKeyPairs DbSet
            var mockDbContext = new Mock<DbContext>();
            mockDbContext.Setup(c => c.Set<Models.UserSignatureKeyPair>()).Returns(mockDbSet.Object);
            mockDbContext.Setup(c => c.UserSignatureKeyPairs).Returns(mockDbSet.Object);
            mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Mock IServiceScope with a ServiceProvider that returns the mockDbContext
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

            var mockServiceScope = new Mock<IAsyncDisposable>();
            mockServiceScope.As<IServiceScope>().Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            // Mock IServiceScopeFactory to return the mockServiceScope when CreateAsyncScope is called
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(mockServiceScope.Object);

            // Create repository instance with mocked IServiceScopeFactory
            var repo = new UserSignatureKeyPairRepository(mockServiceScopeFactory.Object, null!);

            // Act
            var updateFunc = repo.UpdateForKeyRotation(userId, signingKeys);
            await updateFunc(null!, null!);

            // Assert
            mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
            mockDbSet.Verify(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Models.UserSignatureKeyPair, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
            mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
            Assert.True(entity.RevisionDate > DateTime.MinValue);
        }
    }
}

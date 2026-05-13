using System;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task UpdateForKeyRotation_CallsCreateAsyncScope()
        {
            // Arrange
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<DbContext>();
            mockScopeFactory.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

            var repository = new UserSignatureKeyPairRepository(mockScopeFactory.Object, null);

            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "Algorithm",
                WrappedSigningKey = "SigningKey",
                VerifyingKey = "VerifyingKey"
            };

            // Act
            var updateOperation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateOperation(null, null);

            // Assert
            mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Models;
using Stripe;
using System.Collections.Generic;
using System;

namespace Billing.Tests
{
    public class SubscriberServiceTests
    {
        private readonly Mock<ILogger<SubscriberService>> _loggerMock;
        private readonly Mock<Stripe.IStripeClient> _stripeClientMock;
        private readonly Mock<Stripe.IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;

        public SubscriberServiceTests()
        {
            _loggerMock = new Mock<ILogger<SubscriberService>>();
            _stripeClientMock = new Mock<Stripe.IStripeClient>();
            _stripeAdapterMock = new Mock<Stripe.IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();
        }

        [Fact]
        public async Task LogWarning_Called_When_TaxIdType_IsNull()
        {
            // Arrange
            var service = new SubscriberService(
                null, null, _loggerMock.Object, null, null, null, _stripeAdapterMock.Object, _taxServiceMock.Object, null);

            var mockCustomer = new Customer();
            var mockSubscriber = new Mock<ISubscriber>();
            var mockTaxInfo = new TaxInformation { Country = "US", PostalCode = "12345", Line1 = "Line1", City = "City", State = "State", TaxId = "TAX123" };
            var mockTaxService = new Mock<ITaxService>();
            var mockStripeAdapter = new Mock<Stripe.IStripeAdapter>();
            var mockLogger = new Mock<ILogger<SubscriberService>>();

            // Setup
            mockTaxService.Setup(t => t.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((country, taxId) => null);
            mockStripeAdapter.Setup(s => s.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>())).Returns(Task.CompletedTask);
            mockStripeAdapter.Setup(s => s.TaxIdDeleteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            // Call the method that contains the line 721, which is the LogWarning call
            // Since the actual method is not fully provided, assume a method like 'ProcessTaxInformation'
            // For demonstration, we simulate the call that would reach line 721
            await service.ProcessTaxInformation(mockTaxInfo, mockCustomer, mockTaxService.Object, mockStripeAdapter.Object, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Could not infer tax ID type")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}

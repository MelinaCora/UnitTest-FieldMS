using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Services.AvailabilityServices;
using AutoMapper;
using Domain.Entities;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest.AvailabilityServiceTest
{
    public class AvailabilityGetServicesTest
    {
        [Fact]
        public async Task GetAvailabilityById_Should_Return_Availability_When_Found()
        {
            // ARRANGE
            var mockQuery = new Mock<IAvailabilityQuery>();
            var mockMapper = new Mock<IMapper>();

            var availabilityId = 1;
            var availability = new Availability
            {
                AvailabilityID = availabilityId
            };

            mockQuery
                .Setup(q => q.GetAvailabilityByID(availabilityId))
                .ReturnsAsync(availability);

            var service = new AvailabilityGetServices(mockQuery.Object, mockMapper.Object);

            // ACT
            var result = await service.GetAvailabilityById(availabilityId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(availabilityId, result.AvailabilityID);
            mockQuery.Verify(q => q.GetAvailabilityByID(availabilityId),
                "El método GetAvailabilityByID debe llamarse exactamente una vez.");
        }

        [Fact]
        public async Task GetAvailabilityById_Should_Throw_NotFoundException_When_Not_Found()
        {
            // ARRANGE
            var mockQuery = new Mock<IAvailabilityQuery>();
            var mockMapper = new Mock<IMapper>();

            var availabilityId = 1;

            mockQuery
                .Setup(q => q.GetAvailabilityByID(availabilityId))
                .ReturnsAsync((Availability)null);

            var service = new AvailabilityGetServices(mockQuery.Object, mockMapper.Object);

            // ACT 
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetAvailabilityById(availabilityId));

            // ASSERT

            Assert.Equal("Availability not found", exception.Message);

            mockQuery.Verify(q => q.GetAvailabilityByID(availabilityId),
                "El método GetAvailabilityByID debe llamarse exactamente una vez.");
        }
    }
}
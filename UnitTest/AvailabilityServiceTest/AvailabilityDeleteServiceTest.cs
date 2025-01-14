using Application.Interfaces.ICommand;
using Application.Interfaces.IServices.IAvailabilityServices;
using Application.Services.AvailabilityServices;
using Domain.Entities;
using Moq;
using Xunit;
using System.Threading.Tasks;
using Application.Exceptions;

namespace UnitTest.AvailabilityServiceTest
{
    public class AvailabilityDeleteServiceTest
    {
        [Fact]
        public async Task DeleteAvailability_Should_Call_GetAvailabilityById_And_DeleteAvailability()
        {
            // ARRANGE
            var mockCommand = new Mock<IAvailabilityCommand>();
            var mockGetServices = new Mock<IAvailabilityGetServices>();

            var availabilityId = 1;
            var availability = new Availability
            {
                AvailabilityID = availabilityId
            };

            mockGetServices
                .Setup(s => s.GetAvailabilityById(availabilityId))
                .ReturnsAsync(availability);
            
            mockCommand
                .Setup(c => c.DeleteAvailability(availability))
                .Returns(Task.CompletedTask);

            var service = new AvailabilityDeleteService(mockCommand.Object, mockGetServices.Object);

            // ACT
            await service.DeleteAvailability(availabilityId);

            // ASSERT
            mockGetServices.Verify(s => s.GetAvailabilityById(availabilityId), Times.Once,
                "El método GetAvailabilityById debe llamarse exactamente una vez con el ID proporcionado.");
            mockCommand.Verify(c => c.DeleteAvailability(availability), Times.Once,
                "El método DeleteAvailability debe llamarse exactamente una vez con la disponibilidad obtenida.");
        }
            
        [Fact]
        public async Task DeleteAvailability_Should_Throw_Exception_When_CommandFails()
        {
            // ARRANGE
            var mockCommand = new Mock<IAvailabilityCommand>();
            var mockGetServices = new Mock<IAvailabilityGetServices>();

            var availabilityId = 1;
            var availability = new Availability
            {
                AvailabilityID = availabilityId
            };

            mockGetServices
                .Setup(s => s.GetAvailabilityById(availabilityId))
                .ReturnsAsync(availability);

            mockCommand
                .Setup(c => c.DeleteAvailability(availability))
                .Throws(new Exception("Error al eliminar la disponibilidad."));

            var service = new AvailabilityDeleteService(mockCommand.Object, mockGetServices.Object);

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<Exception>(() => service.DeleteAvailability(availabilityId));
            Assert.Equal("Error al eliminar la disponibilidad.", exception.Message);

            mockGetServices.Verify(s => s.GetAvailabilityById(availabilityId), Times.Once);
            mockCommand.Verify(c => c.DeleteAvailability(availability), Times.Once);
        }

    }


}

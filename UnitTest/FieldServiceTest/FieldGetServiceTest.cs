using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.IQuery;
using Application.Interfaces.IValidator;
using Application.Automapper;
using Moq;
using Application.DTOS.Request;
using AutoMapper;
using Domain.Entities;
using Application.DTOS.Responses;
using FluentAssertions.Equivalency;
using Field = Domain.Entities.Field;
using Application.Services.FieldServices;

namespace UnitTest.FieldServiceTest
{
    public class FieldGetServiceTest
    {
        [Fact]
        public async Task GetFieldById_Should_Return_Field_When_Found()
        {
            //ARRANGE
            var mockQuery = new Mock<IFieldQuery>();
            var mockValidator = new Mock<IValidatorHandler<GetFieldsRequest>>();
            var mockMapper = new Mock<IMapper>();

            var fieldID = new Guid();

            var existingField = new Field
            {
                FieldID = fieldID,
                Name = "Cancha 1"
            };

            var expectedResponse = new FieldResponse
            {
                Id = fieldID,
                Name = "Cancha 1"
            };

            mockQuery
                .Setup(q => q.GetFieldById(fieldID))
                .ReturnsAsync(existingField);

            mockMapper
                .Setup(m => m.Map<FieldResponse>(existingField))
                .Returns(expectedResponse);

            var fieldGetService = new FieldGetServices(mockQuery.Object, mockValidator.Object, mockMapper.Object);

            //ACT
            var result = await fieldGetService.GetFieldById(fieldID);

            //ASSERT
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Name, result.Name);

            mockQuery.Verify(q => q.GetFieldById(fieldID), Times.Once);
            mockMapper.Verify(m => m.Map<FieldResponse>(existingField), Times.Once);
        }
    }
}

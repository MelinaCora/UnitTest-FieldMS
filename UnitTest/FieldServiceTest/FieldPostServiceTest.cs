using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Automapper;
using Application.Validators;
using Moq;
using AutoMapper;
using Application.Interfaces.IValidator;
using Application.Services.FieldServices;
using Application.DTOS.Request;
using Domain.Entities;
using Application.DTOS.Responses;


namespace UnitTest.FieldServiceTest
{
    public class FieldPostServiceTest
    {
        [Fact]
        public async Task CreateField_Should_Return_CreatedField()
        {
            //ARRANGE
            var mockFieldQuery = new Mock<IFieldQuery>();
            var mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            var mockAvailabilityQuery = new Mock<IAvailabilityQuery>();
            var mockFieldCommand = new Mock<IFieldCommand>();
            var mockMapper = new Mock<IMapper>();
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();

            var request = new FieldRequest
            {
                Name = "Test Field",
                Size = "Large",
                FieldType = 1
            };

            var fieldType = new FieldType
            {
                FieldTypeID = request.FieldType,
                Description = "Soccer Field"
            };

            var newField = new Field
            {
                FieldID = Guid.NewGuid(),
                Name = request.Name,
                Size = request.Size,
                IsActive = true,
                FieldTypeNavigator = fieldType
            };

            var response = new FieldResponse
            {
                Id = newField.FieldID,
                Name = newField.Name,
                Size = newField.Size,
                FieldType = new FieldTypeResponse
                {
                    Id = fieldType.FieldTypeID,
                    Description=fieldType.Description

                }
            };

            mockValidator
               .Setup(v => v.Validate(It.IsAny<FieldRequest>()))
               .Returns(Task.CompletedTask);

            mockFieldTypeQuery
                .Setup(q => q.GetFieldTypeById(request.FieldType))
                .ReturnsAsync(fieldType);

            mockMapper
                .Setup(m => m.Map<Field>(request))
                .Returns(newField);

            mockMapper
                .Setup(m => m.Map<FieldResponse>(newField))
                .Returns(response);

            mockFieldCommand
                .Setup(c => c.InsertField(It.IsAny<Field>()))
                .Returns(Task.CompletedTask);

            var service = new FieldPostServices(mockFieldCommand.Object,mockFieldQuery.Object,mockFieldTypeQuery.Object,mockAvailabilityQuery.Object,mockValidator.Object,mockMapper.Object);

            //ACT
            var result = await service.CreateField(request);

            //ASSERTS
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Size, result.Size);
            Assert.Equal(fieldType.Description, result.FieldType.Description);

            mockValidator.Verify(v => v.Validate(It.IsAny<FieldRequest>()), Times.Once);
            mockFieldTypeQuery.Verify(q => q.GetFieldTypeById(request.FieldType), Times.Once);
            mockMapper.Verify(m => m.Map<Field>(request), Times.Once);
            mockMapper.Verify(m => m.Map<FieldResponse>(newField), Times.Once);
            mockFieldCommand.Verify(c => c.InsertField(It.IsAny<Field>()), Times.Once);
        }
    }
}

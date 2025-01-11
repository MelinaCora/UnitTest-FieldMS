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

        [Fact]
        public async Task GetAllFields_Should_Return_Fields_When_Found()
        {
            // ARRANGE
            var mockQuery = new Mock<IFieldQuery>();
            var mockValidator = new Mock<IValidatorHandler<GetFieldsRequest>>();
            var mockMapper = new Mock<IMapper>();
            
            string? name = "Test Field";
            string? sizeoffield = "Medium";
            int? type = 1;
            int? availability = 5;
            int? offset = 0;
            int? size = 10;
            
            var fields = new List<Field>
            {
                new Field
                {
                    FieldID = Guid.NewGuid(),
                    Name = "Field 1",
                    Size = "Small",
                    FieldTypeID = 1,
                },

                new Field
                {
                    FieldID = Guid.NewGuid(),
                    Name = "Field 2",
                    Size = "Large",
                    FieldTypeID = 2,
                    
                }
            };
           
            var fieldResponses = fields.Select(field => new FieldResponse
            {
                Id = field.FieldID,
                Name = field.Name,
                Size = field.Size,
                FieldType = new FieldTypeResponse { 
                    Id= field.FieldTypeID,
                },
            }).ToList();
            
            mockValidator
                .Setup(v => v.Validate(It.IsAny<GetFieldsRequest>()))
                .Returns(Task.CompletedTask);
            
            mockQuery
                .Setup(q => q.GetFields(name, sizeoffield, type, availability, offset, size))
                .ReturnsAsync(fields);
            
            mockMapper
                .Setup(m => m.Map<List<FieldResponse>>(fields))
                .Returns(fieldResponses);
            
            var fieldGetService = new FieldGetServices(mockQuery.Object, mockValidator.Object, mockMapper.Object);

            // ACT
            var result = await fieldGetService.GetAllFields(name, sizeoffield, type, availability, offset, size);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(fields.Count, result.Count); 
            for (int i = 0; i < fields.Count; i++)
            {
                Assert.Equal(fields[i].FieldID, result[i].Id);
                Assert.Equal(fields[i].Name, result[i].Name);
                Assert.Equal(fields[i].Size, result[i].Size);
                Assert.Equal(fields[i].FieldTypeID, result[i].FieldType.Id);                
            }
            
            mockValidator.Verify(v => v.Validate(It.IsAny<GetFieldsRequest>()), Times.Once);
            mockQuery.Verify(q => q.GetFields(name, sizeoffield, type, availability, offset, size), Times.Once);
            mockMapper.Verify(m => m.Map<List<FieldResponse>>(fields), Times.Once);
        }

        [Fact]
        public async Task GetAllFields_Should_Return_Empty_List_When_No_Fields_Found()
        {
            // ARRANGE
            var mockQuery = new Mock<IFieldQuery>();
            var mockValidator = new Mock<IValidatorHandler<GetFieldsRequest>>();
            var mockMapper = new Mock<IMapper>();

            string? name = null;
            string? sizeoffield = null;
            int? type = null;
            int? availability = null;
            int? offset = 0;
            int? size = 10;

            mockValidator
               .Setup(v => v.Validate(It.IsAny<GetFieldsRequest>()))
               .Returns(Task.CompletedTask);

            mockQuery
               .Setup(q => q.GetFields(name, sizeoffield, type, availability, offset, size))
               .ReturnsAsync(new List<Field>());

           var fieldGetService = new FieldGetServices(mockQuery.Object, mockValidator.Object, mockMapper.Object);

            // ACT
            var result = await fieldGetService.GetAllFields(name, sizeoffield, type, availability, offset, size);

            // ASSERT
            Assert.NotNull(result);
            Assert.Empty(result);
            
            mockValidator.Verify(v => v.Validate(It.IsAny<GetFieldsRequest>()), Times.Once);
            mockQuery.Verify(q => q.GetFields(name, sizeoffield, type, availability, offset, size), Times.Once);
        }
    }
}

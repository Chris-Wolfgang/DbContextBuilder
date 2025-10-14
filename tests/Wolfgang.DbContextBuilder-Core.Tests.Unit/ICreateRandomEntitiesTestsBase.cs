using AdventureWorks.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit
{
    /// <summary>
    /// Provides a base class for tests related to the ICreateRandomEntities interface.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class ICreateRandomEntitiesTestsBase 
    {

        /// <summary>
        /// Creates an instance of the specific implementation of ICreateRandomEntities to be tested.
        /// </summary>
        /// <returns><see cref="ICreateRandomEntities"/>Representing the object to test</returns>
        protected abstract ICreateRandomEntities CreateRandomEntityCreator();



        /// <summary>
        /// Verifies that this class can create an instance of the System Under Test
        /// </summary>
        [Fact]
        public void Can_create_instance_of_SystemUnderTest()
        {
            // Arrange & Act
            var sut = CreateRandomEntityCreator();
            
            // Assert
            Assert.NotNull(sut);
        }



        /// <summary>
        /// Verifies that CreateRandomEntities returns the specified number of items
        /// </summary>
        /// <param name="count"></param>
        [Theory]
        [InlineData(7)]
        [InlineData(17)]
        public void CreateRandomEntities_returns_the_specified_number_of_items(int count)
        {
            // Arrange
            var sut = CreateRandomEntityCreator();

            // Act
            var entities = sut.CreateRandomEntities<Person>(count).ToList();

            // Assert
            Assert.Equal(count, entities.Count);
            Assert.All(entities, Assert.NotNull);
        }



        /// <summary>
        /// Verifies that passing count less than 1, throws an ArgumentOutOfRangeException
        /// </summary>
        [Fact]
        public void Calling_CreateRandomEntities_with_value_less_than_1_throws_ArgumentOutOfRangeException()
        {

            // Arrange
            var sut = CreateRandomEntityCreator();
            const int count = 0;

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateRandomEntities<Address>(count));

            // Assert
            Assert.NotNull(exception);
            var argumentOutOfRangeException = Assert.IsType<ArgumentOutOfRangeException>(exception);
            Assert.Equal("count", argumentOutOfRangeException.ParamName);
            Assert.Equal(count, argumentOutOfRangeException.ActualValue);
        }
    }
}

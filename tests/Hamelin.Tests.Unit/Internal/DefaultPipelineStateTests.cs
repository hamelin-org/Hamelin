using Hamelin.Internal;

namespace Hamelin.Tests.Unit.Internal;

public class DefaultPipelineStateTests
{
    [Fact]
    public void Get_MissingValueNoKey_ReturnsNull()
    {
        // Arrange
        var state = new DefaultPipelineState();

        List<string> value = ["Hello", "World"];

        // Act
        state.Set(value, "v1");
        List<string>? value2 = state.Get<List<string>>("v2");

        // Assert
        value2.ShouldBeNull();
    }

    [Fact]
    public void Get_MissingValueWithKey_ReturnsNull()
    {
        // Arrange
        var state = new DefaultPipelineState();

        // Act
        List<string>? retrievedValue = state.Get<List<string>>();

        // Assert
        retrievedValue.ShouldBeNull();
    }

    [Fact]
    public void GetAndSet_PrimitiveNoKey_WorksCorrectly()
    {
        // Arrange
        var state = new DefaultPipelineState();

        decimal value = 1234.56m;

        // Act
        state.Set(value);
        decimal? retrievedValue = state.Get<decimal>();

        // Assert
        value.ShouldBe(retrievedValue.Value);
    }

    [Fact]
    public void GetAndSet_ReferenceTypeNoKey_WorksCorrectly()
    {
        // Arrange
        var state = new DefaultPipelineState();

        List<string> value = ["Hello", "World"];

        // Act
        state.Set(value);
        var retrievedValue = state.Get<List<string>>();

        // Assert
        value.ShouldBe(retrievedValue);
    }

    [Fact]
    public void GetAndSet_PrimitiveWitKey_WorksCorrectly()
    {
        // Arrange
        var state = new DefaultPipelineState();

        decimal value1 = 1234.56m;
        decimal value2 = 6543.21m;

        // Act
        state.Set(value1, "v1");
        state.Set(value2, "v2");
        decimal? retrievedValue1 = state.Get<decimal>("v1");
        decimal? retrievedValue2 = state.Get<decimal>("v2");

        // Assert
        value1.ShouldBe(retrievedValue1.Value);
        value2.ShouldBe(retrievedValue2.Value);
    }

    [Fact]
    public void GetAndSet_ReferenceTypeWithKey_WorksCorrectly()
    {
        // Arrange
        var state = new DefaultPipelineState();

        List<string> value1 = ["Hello", "World"];
        List<string> value2 = ["Foo", "Bar"];

        // Act
        state.Set(value1, "v1");
        state.Set(value2, "v2");
        List<string>? retrievedValue1 = state.Get<List<string>>("v1");
        List<string>? retrievedValue2 = state.Get<List<string>>("v2");

        // Assert
        value1.ShouldBe(retrievedValue1);
        value2.ShouldBe(retrievedValue2);
    }
}

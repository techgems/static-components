using TechGems.StaticComponents;

namespace TechGems.StaticComponents.UnitTests;

[TestFixture]
public class JavascriptSerializerTests
{
    #region Helper Types

    private class SimpleObject
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class ObjectWithBool
    {
        public bool IsActive { get; set; }
    }

    private class ObjectWithNullable
    {
        public string? NullableString { get; set; }
    }

    private class NestedObject
    {
        public string Title { get; set; } = string.Empty;
        public SimpleObject Inner { get; set; } = new();
    }

    private class ObjectWithArray
    {
        public string[] Tags { get; set; } = [];
    }

    #endregion

    #region Property Name Format (JavaScript Object Literal, not JSON)

    [Test]
    public void SerializeObject_SimpleObject_PropertyNamesAreNotQuoted()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "Alice", Age = 30 });
        var output = result.ToString()!;

        Assert.That(output, Does.Not.Contain("\"name\""));
        Assert.That(output, Does.Not.Contain("\"age\""));
        Assert.That(output, Does.Contain("name:"));
        Assert.That(output, Does.Contain("age:"));
    }

    [Test]
    public void SerializeObject_NestedObject_AllPropertyNamesAreNotQuoted()
    {
        var result = JavascriptConvert.SerializeObject(new NestedObject
        {
            Title = "Test",
            Inner = new SimpleObject { Name = "Bob", Age = 25 }
        });
        var output = result.ToString()!;

        Assert.That(output, Does.Not.Contain("\"title\""));
        Assert.That(output, Does.Not.Contain("\"inner\""));
        Assert.That(output, Does.Not.Contain("\"name\""));
        Assert.That(output, Does.Not.Contain("\"age\""));
        Assert.That(output, Does.Contain("title:"));
        Assert.That(output, Does.Contain("inner:"));
        Assert.That(output, Does.Contain("name:"));
        Assert.That(output, Does.Contain("age:"));
    }

    [Test]
    public void SerializeObject_PropertyNamesAreCamelCase()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "Alice", Age = 30 });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("name:"));
        Assert.That(output, Does.Contain("age:"));
        Assert.That(output, Does.Not.Contain("Name:"));
        Assert.That(output, Does.Not.Contain("Age:"));
    }

    #endregion

    #region String Value Quoting

    [Test]
    public void SerializeObject_DefaultBehavior_StringValuesUseDoubleQuotes()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "Alice", Age = 0 });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("\"Alice\""));
        Assert.That(output, Does.Not.Contain("'Alice'"));
    }

    [Test]
    public void SerializeObject_UseSingleQuotesTrue_StringValuesUseSingleQuotes()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "Alice", Age = 0 }, useSingleQuotesOnStringValues: true);
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("'Alice'"));
        Assert.That(output, Does.Not.Contain("\"Alice\""));
    }

    [Test]
    public void SerializeObject_UseSingleQuotesTrue_PropertyNamesAreStillNotQuoted()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "Alice", Age = 0 }, useSingleQuotesOnStringValues: true);
        var output = result.ToString()!;

        Assert.That(output, Does.Not.Contain("'name'"));
        Assert.That(output, Does.Contain("name:"));
    }

    [Test]
    public void SerializeObject_MultipleStringProperties_AllUseSingleQuotes()
    {
        var result = JavascriptConvert.SerializeObject(
            new { FirstName = "Alice", LastName = "Smith" },
            useSingleQuotesOnStringValues: true);
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("'Alice'"));
        Assert.That(output, Does.Contain("'Smith'"));
        Assert.That(output, Does.Not.Contain("\"Alice\""));
        Assert.That(output, Does.Not.Contain("\"Smith\""));
    }

    #endregion

    #region Non-String Value Types

    [Test]
    public void SerializeObject_NumericValue_SerializedWithoutQuotes()
    {
        var result = JavascriptConvert.SerializeObject(new SimpleObject { Name = "", Age = 42 });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("42"));
        Assert.That(output, Does.Not.Contain("\"42\""));
        Assert.That(output, Does.Not.Contain("'42'"));
    }

    [Test]
    public void SerializeObject_BooleanTrue_SerializedAsLowercaseTrue()
    {
        var result = JavascriptConvert.SerializeObject(new ObjectWithBool { IsActive = true });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("true"));
        Assert.That(output, Does.Not.Contain("\"true\""));
        Assert.That(output, Does.Not.Contain("True"));
    }

    [Test]
    public void SerializeObject_BooleanFalse_SerializedAsLowercaseFalse()
    {
        var result = JavascriptConvert.SerializeObject(new ObjectWithBool { IsActive = false });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("false"));
        Assert.That(output, Does.Not.Contain("\"false\""));
        Assert.That(output, Does.Not.Contain("False"));
    }

    [Test]
    public void SerializeObject_NullValue_SerializedAsNull()
    {
        var result = JavascriptConvert.SerializeObject(new ObjectWithNullable { NullableString = null });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("null"));
    }

    #endregion

    #region Collections

    [Test]
    public void SerializeObject_StringArray_ValuesUseDoubleQuotesByDefault()
    {
        var result = JavascriptConvert.SerializeObject(new ObjectWithArray { Tags = ["csharp", "dotnet"] });
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("\"csharp\""));
        Assert.That(output, Does.Contain("\"dotnet\""));
    }

    [Test]
    public void SerializeObject_StringArray_ValuesUseSingleQuotesWhenFlagSet()
    {
        var result = JavascriptConvert.SerializeObject(
            new ObjectWithArray { Tags = ["csharp", "dotnet"] },
            useSingleQuotesOnStringValues: true);
        var output = result.ToString()!;

        Assert.That(output, Does.Contain("'csharp'"));
        Assert.That(output, Does.Contain("'dotnet'"));
        Assert.That(output, Does.Not.Contain("\"csharp\""));
        Assert.That(output, Does.Not.Contain("\"dotnet\""));
    }

    #endregion
}

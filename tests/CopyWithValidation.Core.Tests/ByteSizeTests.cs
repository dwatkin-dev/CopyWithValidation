namespace CopyWithValidation.Core.Tests;

public class ByteSizeTests
{
    [Theory]
    [InlineData(0L, "0 bytes")]
    [InlineData(1023L, "1023 bytes")]
    [InlineData(1024L, "1 KB")]
    [InlineData(1536L, "1.5 KB")]
    [InlineData(1_048_576L, "1 MB")]
    [InlineData(1_610_612_736L, "1.5 GB")]
    public void Format_AnySize_ReturnsReadableText(long bytes, string expected)
    {
        // Arrange 
        string result;

        // Act
        result = ByteSize.Format(bytes);

        // Assert
        Assert.Equal(expected, result);
    }
}

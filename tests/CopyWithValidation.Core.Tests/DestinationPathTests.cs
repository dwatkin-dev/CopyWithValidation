using CopyWithValidation.Core.Copying;

namespace CopyWithValidation.Core.Tests;

public class DestinationPathTests
{
    [Theory]
    [InlineData(@"C:\source\image.bin", @"D:\backup", @"D:\backup\image.bin")]
    [InlineData(@"C:\source\image.bin", @"D:\backup\", @"D:\backup\image.bin")]  // folder already ends in \
    [InlineData(@"C:\source\no-extension", @"D:\", @"D:\no-extension")]          // drive root
    public void Build_ValidPaths_ReturnsFileInDestinationFolder(string sourceFile, string destinationFolder, string expected)
    {
        string actual = DestinationPath.Build(sourceFile, destinationFolder);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData(@"C:\source\")]   // a folder, not a file
    public void Build_SourceHasNoFileName_ThrowsArgumentException(string sourceFile)
    {
        var ex = Assert.Throws<ArgumentException>(() => DestinationPath.Build(sourceFile, @"D:\backup"));

        Assert.Equal("sourceFile", ex.ParamName);
    }
}
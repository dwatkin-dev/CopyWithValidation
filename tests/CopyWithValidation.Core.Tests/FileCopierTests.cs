namespace CopyWithValidation.Core.Tests
{
    public class FileCopierTests
    {
        [Fact]
        public void CopyAndVerify_ValidSource_CreatesIdenticalDestination()
        {
            // Arrange
            var copier = new FileCopier();
            string source = Path.GetTempFileName();
            string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(source, "hello");

                // Act
                copier.CopyAndVerify(source, destination);

                // Assert
                Assert.Equal("hello", File.ReadAllText(destination));
            }
            finally
            {
                File.Delete(source);
                File.Delete(destination);
            }
        }

        [Fact]
        public void CopyAndVerify_ValidSource_ReturnsTrue()
        {
            // Arrange
            var copier = new FileCopier();
            string source = Path.GetTempFileName();
            string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.WriteAllText(source, "hello");

                // Act
                bool result = copier.CopyAndVerify(source, destination);

                // Assert
                Assert.True(result);
            }
            finally
            {
                File.Delete(source);
                File.Delete(destination);
            }
        }
    }
}

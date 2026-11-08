using System.Text;

namespace CopyWithValidation.Core.Tests
{
    public class FileHasherTests
    {
        [Fact]
        public void ComputerHash_EmptyStream_ReturnsKnownSha256()
        {
            // Arrange
            var hasher = new FileHasher();
            using var stream = new MemoryStream();

            // Act
            string hash = hasher.ComputeHash(stream);

            // Assert
            Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
        }

        [Fact]
        public void ComputeHash_Abc_ReturnsKnownSh256()
        {
            // Arrange
            var hasher = new FileHasher();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));

            // Act
            string hash = hasher.ComputeHash(stream);

            // Assert
            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
        }

        [Fact]
        public void ComputeHash_FileContainingAbc_ReturnsKnownHash()
        {
            // Arrange
            var hasher = new FileHasher();
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "abc");

                // Act
                string hash = hasher.ComputeHash(path);

                // Assert
                Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void ComputeHash_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var hasher = new FileHasher();
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => hasher.ComputeHash(path));
        }
    }
}

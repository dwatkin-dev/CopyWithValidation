using System.Text;
using CopyWithValidation.Core;

namespace CopyWithValidation.Core.Tests
{
    public class FileHasherTests
    {
        [Fact]
        public void ComputerHash_EmptyStream_ReturnsKnownSha256()
        {
            var hasher = new FileHasher();
            using var stream = new MemoryStream();

            string hash = hasher.ComputeHash(stream);

            Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
        }

        [Fact]
        public void ComputeHash_Abc_ReturnsKnownSh256()
        {
            var hasher = new FileHasher();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));

            string hash = hasher.ComputeHash(stream);

            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
        }
    }
}

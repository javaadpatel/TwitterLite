using FluentAssertions;
using NUnit.Framework;
using TwitterLite.Services.Services;

namespace TwitterLite.Services.Tests
{
    [TestFixture]
    public class FileServiceTests
    {
        private readonly FileService _sut;

        public FileServiceTests()
        {
            _sut = new FileService();
        }

        [SetUp]
        public void Setup()
        {
        }

        //[Test]
        //public void FileService_Constructor_Should_Create_Instance()
        //{
        //    var instance = new FileService();
        //    instance.Should().NotBeNull();
        //}

        //[TestCase("./assets/existantFile.txt", ExpectedResult = true)]
        //[TestCase("non-existantFile.txt", ExpectedResult = false)]
        //public bool FileService_CheckIfFileExists_Should_ReturnCorrectResult(string fileName)
        //{
        //    return _sut.CheckIfFileExists(fileName);
        //}
    }
}
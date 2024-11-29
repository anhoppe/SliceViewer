using Moq;
using LayerSource.GCode.Parser;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class LayupParserTest
    {
        #region Fields

        private Mock<ISectionParser> _fileHeaderParseMock;

        private Mock<ISectionParser> _startCodeParseMock;

        private LayupParser _sut;

        private Mock<IZChunkParser> _zChunkParserMock;

        #endregion

        public void SetUp()
        {
            _zChunkParserMock = new Mock<IZChunkParser>();

            _sut = new LayupParser();
        }

        [Test]
        [Category("Short")]
        public void Parse_FileHeaderThenStartCodeThen1ZChunk_ParsersCalledCorrectly()
        {
            // Arrange
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "; < ProducerID > < ProducerVersion >",
                "; Estimated print time: 12min  3sec",
                "; *--START CODE--",
                "foo",
                ";*-- END OF START CODE --",
                "; - START OF ZCHUNK",
                "bar",
                "; - END OF ZCHUNK "
            });

            _fileHeaderParseMock.Setup(p => p.CanParse(gcode[0])).Returns(true);
            //_fileHeaderParseMock.Setup(p => p)
            
            // Act

            // Assert
        }
    }
}

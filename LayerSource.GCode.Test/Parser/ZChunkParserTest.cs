using System.Collections.Generic;
using Moq;
using LayerSource.GCode.Parser;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    internal class ZChunkParserTest
    {
        #region Fields

        private Mock<ISettableLayup> _layupMock;

        private Mock<IStretchesParser> _fiberStretchParserMock;

        private Mock<IStretchesParser> _plasticStretchParserMock;

        private ZChunkParser _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _layupMock = new Mock<ISettableLayup>();

            _fiberStretchParserMock = new Mock<IStretchesParser>();

            _plasticStretchParserMock = new Mock<IStretchesParser>();

            _sut = new ZChunkParser
            {
                FiberStretchParser = _fiberStretchParserMock.Object,
                PlasticStretchParser = _plasticStretchParserMock.Object
            };
        }

        #endregion

        [Test] [Category("Short")]
        public void CanParse_LineContainsStartMarker_True()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new List<string>
            {
                "; - START OF ZCHUNK #7, range=[13.13, 23.23] -"
            });

            // Act
            var canParse = _sut.CanParse(gcode[0]);

            // Assert
            Assert.IsTrue(canParse);
        }

        [Test] [Category("Short")]
        public void Parse_LinesCanBeParsedByStretchParsers_LinesPassedToStretchParsers()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new List<string>
            {
                "; - START OF ZCHUNK #7, range=[13.13, 23.23] -",
                "foo",
                "bar",
                "; - END OF ZCHUNK"
            });

            var zChunk = Mock.Of<ISettableZChunk>();

            _sut.ZChunkFactory = (count, startZ, endZ) =>
            {
                if (count == 7 && startZ == 13.13 && endZ == 23.23)
                {
                    return zChunk;
                }

                return null;
            };

            _fiberStretchParserMock.Setup(p => p.CanParse(gcode[1])).Returns(true);
            _fiberStretchParserMock.Setup(p => p.Parse(gcode, zChunk, 1)).Returns(2);

            _plasticStretchParserMock.Setup(p => p.CanParse(gcode[2])).Returns(true);
            _plasticStretchParserMock.Setup(p => p.Parse(gcode, zChunk, 2)).Returns(3);

            // Act
            var startIndex = _sut.Parse(gcode, _layupMock.Object, 0);

            // Assert
            Assert.AreEqual(4, startIndex);
            _fiberStretchParserMock.Verify(m => m.Parse(gcode, zChunk, 1), Times.Once);
            _plasticStretchParserMock.Verify(m => m.Parse(gcode, zChunk, 2), Times.Once);

            Assert.AreEqual(4, startIndex);
            _layupMock.Verify(m => m.AddZChunk(zChunk));
        }

        [Test]
        [Category("Short")]
        public void Parse_1stLineCantBeParsedByStretchParserBut2nd_2ndLinePassedToStretchParser()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new List<string>
            {
                "; - START OF ZCHUNK #7, range=[13.13, 23.23] -",
                "foo",
                "bar",
                "; - END OF ZCHUNK"
            });

            var zChunk = Mock.Of<ISettableZChunk>();

            _sut.ZChunkFactory = (count, startZ, endZ) => { return zChunk; };

            _fiberStretchParserMock.Setup(p => p.CanParse(gcode[2])).Returns(true);
            _fiberStretchParserMock.Setup(p => p.Parse(gcode, zChunk, 2)).Returns(3);

            // Act
            var startIndex = _sut.Parse(gcode, _layupMock.Object, 0);

            // Assert
            Assert.AreEqual(4, startIndex);
            _fiberStretchParserMock.Verify(m => m.Parse(gcode, zChunk, 2), Times.Once);

            Assert.AreEqual(4, startIndex);
            _layupMock.Verify(m => m.AddZChunk(zChunk));
        }
    }
}
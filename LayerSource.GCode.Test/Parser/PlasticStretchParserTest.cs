using System.Collections.Generic;
using System.Numerics;
using Moq;
using LayerSource.Contract;
using LayerSource.GCode.Parser;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class PlasticStretchParserTest
    {
        #region Fields

        private Mock<ISettableStretch> _stretchMock;

        private PlasticStretchParser _sut;

        private Mock<IZChunk> _zChunkMock;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _stretchMock = new Mock<ISettableStretch>();

            _sut = new PlasticStretchParser()
            {
                StretchFactory = () => _stretchMock.Object,
            };

            _zChunkMock = new Mock<IZChunk>();
        }

        [Test, Category("Short")]
        public void CanParse_OneLineHasValidStartTag_LineWithValidTagCanBeParsed()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
                "bar",
                "; - START OF PLASTIC STRETCH",
                "buz",
            });

            // Act
            // Assert
            Assert.IsFalse(_sut.CanParse(gcode[0]));
            Assert.IsFalse(_sut.CanParse(gcode[1]));
            Assert.IsTrue(_sut.CanParse(gcode[2]));
            Assert.IsFalse(_sut.CanParse(gcode[3]));
        }

        [Test, Category("Short")]
        public void Parse_ContainsPlasticStretch_DepositionMovesRead()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 X10 Y100 Z1000",
                "G1 X20 Y200 Z2000",
                "G1 X30 Y300 Z3000",
                "; - END OF PLASTIC STRETCH",
            });

            var stretches = new List<IStretch>();
            _zChunkMock.Setup(x => x.Stretches).Returns(stretches);

            var depositionMoves = new List<Vector4>();
            _stretchMock.Setup(p => p.DepositionPoints).Returns(depositionMoves);

            // Act
            int result = _sut.Parse(gcode, _zChunkMock.Object, 0);

            // Assert
            Assert.AreEqual(4, result);

            Assert.AreEqual(_stretchMock.Object, stretches[0]);

            Assert.AreEqual(new Vector4(10, 100, 1000, 0), depositionMoves[0]);
            Assert.AreEqual(new Vector4(20, 200, 2000, 0), depositionMoves[1]);
            Assert.AreEqual(new Vector4(30, 300, 3000, 0), depositionMoves[2]);
        }

        #endregion
    }
}

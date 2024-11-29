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
    public class FiberStretchParserTest
    {
        #region Fields

        private Mock<ISettableFiberStretch> _fiberStretchMock;

        private FiberStretchParser _sut;

        private Mock<IZChunk> _zChunkMock;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _fiberStretchMock = new Mock<ISettableFiberStretch>();

            _zChunkMock = new Mock<IZChunk>();
            
            _sut = new FiberStretchParser()
            {
                FiberStretchFactory = () => _fiberStretchMock.Object,
            };
        }

        #endregion

        [Test] [Category("Short")]
        public void Parse_ContainsAnchoringAndCutCommands_AnchoringAndDepositionAndCutRead()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "USE_ABSOLUTE_ROTARY_POSITION",
                "G1 X111 Y222 Z333 W4 ; move to anchoring start",
                "USE_RELATIVE_ROTARY_POSITION",
                "G0 E0 F0",
                "G1 X1111 Y2222 Z3333",
                "G4 P1200; anchoring dwell",
                "G1 X10 Y100 Z1000 W1",
                "G1 X20 Y200 Z2000 W2",
                "G1 X30 Y300 Z3000 W3",
                "cut_filament",
                "G1 X40 Y400 Z4000 W4",
                "G1 X50 Y500 Z5000 W5",
                "; - END OF CF STRETCH",
            });

            var depositionMoves = new List<Vector4>();
            _fiberStretchMock.Setup(p => p.DepositionPoints).Returns(depositionMoves);

            var afterCutMoves = new List<Vector4>();
            _fiberStretchMock.Setup(p => p.AfterCutDepositionPoints).Returns(afterCutMoves);

            var stretches = new List<IStretch>();
            _zChunkMock.Setup(p => p.Stretches).Returns(stretches);

            // Act
            int result = _sut.Parse(gcode, _zChunkMock.Object, 0);

            // Assert
            Assert.AreEqual(13, result);

            Assert.AreEqual(_fiberStretchMock.Object, stretches[0]);

            _fiberStretchMock.VerifySet(p => p.AnchoringStart = new Vector4(111, 222, 333, 4));
            _fiberStretchMock.VerifySet(p => p.AnchoringEnd = new Vector3(1111, 2222, 3333));

            Assert.AreEqual(new Vector4(10, 100, 1000, 1), depositionMoves[0]);
            Assert.AreEqual(new Vector4(20, 200, 2000, 2), depositionMoves[1]);
            Assert.AreEqual(new Vector4(30, 300, 3000, 3), depositionMoves[2]);

            Assert.AreEqual(new Vector4(40, 400, 4000, 4), afterCutMoves[0]);
            Assert.AreEqual(new Vector4(50, 500, 5000, 5), afterCutMoves[1]);
        }

        [Test, Category("Short")]
        public void CanParse_ValidFiberStretchHeader_LineWithValidIdCanBeParsed()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
                "; - START OF CF STRETCH",
                "bar",
            });

            // Act
            // Assert
            Assert.IsFalse(_sut.CanParse(gcode[0]));
            Assert.IsTrue(_sut.CanParse(gcode[1]));
            Assert.IsFalse(_sut.CanParse(gcode[2]));
        }
    }
}

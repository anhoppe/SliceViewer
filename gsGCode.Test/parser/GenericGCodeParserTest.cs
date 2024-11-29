using System.IO;
using gs;
using NUnit.Framework;

namespace gsGCode.Test.parser
{
    [TestFixture]
    public class GenericGCodeParserTest
    {
        #region Fields

        private GenericGCodeParser _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _sut = new GenericGCodeParser();
        }

        #endregion

        [Test]
        [Category("Short")]
        public void Parse_AddCommandsWithBlankLinesAndExcludeBlanks_EmptyLinesAreNotInResult()
        {
            // Arrange
            var gcode = new[]
            {
                "G1 X1 Y1",
                "",
                "G3 X1 Y2"
            };

            // Act
            GCodeFile result;

            using (var reader = new StringReader(string.Join("\r\n", gcode)))
            {
                result = _sut.Parse(reader, true);
            }

            // Assert
            Assert.AreEqual(2, result.LineCount);
        }

        [Test]
        [Category("Short")]
        public void Parse_AddCommandsWithBlankLinesAndIncludeBlanks_EmptyLinesAreInResult()
        {
            // Arrange
            var gcode = new[]
            {
                "G1 X1 Y1",
                "",
                "G3 X1 Y2"
            };

            // Act
            GCodeFile result;

            using (var reader = new StringReader(string.Join("\r\n", gcode)))
            {
                result = _sut.Parse(reader, false);
            }

            // Assert
            Assert.AreEqual(3, result.LineCount);
        }
    }
}

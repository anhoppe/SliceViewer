using Moq;
using LayerSource.GCode.Parser;
using Logger.Contract;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class SectionParserTest
    {
        #region Fields

        private Mock<ILogger> _loggerMock;

        private SectionParser _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();

            _sut = new SectionParser()
            {
                Logger = _loggerMock.Object,
            };
        }

        #endregion

        [Test] [Category("Short")]
        public void Parse_ValidStartEndMarker_SectionContainsGCodeBetweenMarkers()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foobar", 
                ";startmark buz", 
                "foo", 
                "bar", 
                ";endmark", 
                "barfoo"
            });

            _sut.StartMarker = ";startmark";
            _sut.EndMarker = ";endmark";

            // Act
            int startIndex = _sut.Parse(gcode, 0);

            // Assert
            Assert.AreEqual(5, startIndex);
            Assert.AreEqual(";startmark buz", _sut.FoundStartMarker);
            Assert.AreEqual(2, _sut.Gcode.Count);
            Assert.AreEqual("foo", _sut.Gcode[0].orig_string);
            Assert.AreEqual("bar", _sut.Gcode[1].orig_string);
        }

        [Test, Category("Short")]
        public void Parse_StartIndexPointsTo2ndSection_2ndSectionParsed()
        {
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                ";startmark bar",
                "foo",
                "bar",
                ";endmark",
                ";startmark foo",
                "foobar",
                "barfoo",
                ";endmark",
            });

            _sut.StartMarker = ";startmark";
            _sut.EndMarker = ";endmark";

            // Act
            int startLine = _sut.Parse(gcode, 4);

            // Assert
            Assert.AreEqual(8, startLine);
            Assert.AreEqual(";startmark foo", _sut.FoundStartMarker);
            Assert.AreEqual(2, _sut.Gcode.Count);
            Assert.AreEqual("foobar", _sut.Gcode[0].orig_string);
            Assert.AreEqual("barfoo", _sut.Gcode[1].orig_string);
        }

        [Test, Category("Short")]
        public void Parse_StartMarkerNotSet_ParsedFromBeginning()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
                "bar",
                "buz",
                ";endmark",
                "foobar"
            });

            _sut.EndMarker = ";endmark";

            // Act
            _sut.Parse(gcode, 0);

            // Assert
            Assert.AreEqual(3, _sut.Gcode.Count);
            Assert.AreEqual("foo", _sut.Gcode[0].orig_string);
            Assert.AreEqual("bar", _sut.Gcode[1].orig_string);
            Assert.AreEqual("buz", _sut.Gcode[2].orig_string);
        }

        [Test, Category("Short")]
        public void Parse_EndMarkerNotSet_ParsedToEnd()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
                ";startmark",
                "bar",
                "buz",
                "foobar"
            });

            _sut.StartMarker = ";startmark";

            // Act
            _sut.Parse(gcode, 0);

            // Assert
            Assert.AreEqual(3, _sut.Gcode.Count);
            Assert.AreEqual("bar", _sut.Gcode[0].orig_string);
            Assert.AreEqual("buz", _sut.Gcode[1].orig_string);
            Assert.AreEqual("foobar", _sut.Gcode[2].orig_string);
        }

        [Test, Category("Short")]
        public void Parse_EndMarkSetButNotFound_LogWarningAndSectionContainsCompleteCode()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foobar",
                ";startmark",
                "foo",
                "bar",
                "buz"
            });

            _sut.StartMarker = ";startmark";
            _sut.EndMarker = ";endmark";

            // Act
            _sut.Parse(gcode, 0);

            // Assert
            Assert.AreEqual(3, _sut.Gcode.Count);
            Assert.AreEqual("foo", _sut.Gcode[0].orig_string);
            Assert.AreEqual("bar", _sut.Gcode[1].orig_string);
            Assert.AreEqual("buz", _sut.Gcode[2].orig_string);

            _loggerMock.Verify(m => m.Warn(It.Is<string>(s => s.Contains("end marker")), null));
        }

        [Test, Category("Short")]
        public void Parse_StartMarkSetButNotFound_LogWarningAndSectionEmpty()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
                "bar",
                ";endmark",
            });

            _sut.StartMarker = ";startmark";
            _sut.EndMarker = ";endmark";

            // Act
            _sut.Parse(gcode, 0);

            // Assert
            Assert.AreEqual(0, _sut.Gcode.Count);
            _loggerMock.Verify(m => m.Warn(It.Is<string>(s => s.ToLower().Contains("start marker")), null));
        }
    }
}

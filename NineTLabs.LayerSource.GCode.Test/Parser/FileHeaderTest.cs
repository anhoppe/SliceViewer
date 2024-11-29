using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using LayerSource.GCode.Parser;
using Logger.Contract;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class FileHeaderTest
    {
        #region Fields

        private Mock<ISettableLayup> _layupMock;

        private Mock<ILogger> _loggerMock;

        private FileHeader _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _layupMock = new Mock<ISettableLayup>();

            _loggerMock = new Mock<ILogger>();

            _sut = new FileHeader
            {
                Logger = _loggerMock.Object
            };
        }

        #endregion

        [Test]
        [Category("Short")]
        public void Parse_HeaderContainsProducerInfoAndBte_SetLayupProducerInfoAndBteAndRowsParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; foo bar foobar",
                "; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Producer = "foo");
            _layupMock.VerifySet(p => p.ProducerVersion = "bar foobar");
            _layupMock.VerifySet(p => p.Bte = "foo");

            Assert.AreEqual(2, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void BteParse_2ndRowContainsInvalidBteId_BteNotSetAndWarningFiledAndRowsParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; foo bar",
                "; Invalid Bte: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Bte = It.IsAny<string>(), Times.Never);
            Assert.AreEqual(2, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void ProducerParsing_FirstRowContainsOnlyProducer_VersionNotSetAndWarningFiledAndRowsParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; foo",
                "; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Producer = "foo");
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(2, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void
            ProducerParsing_FirstRowContainsCommentWithOneSpace_ProducerAndVersionNotSetAndWarningFiledAndRowsParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; ",
                "; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Producer = It.IsAny<string>(), Times.Never);
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(2, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void ProducerParsing_FirstRowContainsEmptyComment_ProducerAndVersionNotSetAndWarningFiledAndRowParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                ";",
                "; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Producer = It.IsAny<string>(), Times.Never);
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(2, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void
            ProducerParsing_StartIndexIs1AndFirstRowContainsEmptyComment_ProducerAndVersionNotSetAndWarningFiledAndRowParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "ignored row",
                ";",
                "; Estimated print time: foo",

            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 1);

            // Assert
            _layupMock.VerifySet(p => p.Producer = It.IsAny<string>(), Times.Never);
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(3, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void StartIndexIncrement_StartIndexNot0AndValidRows_StartIndexIncrementedCorrectly()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; ",
                "; foo bar",
                "; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 1);

            // Assert
            Assert.AreEqual(3, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void ProducerParsing_NoCommentAtStartIndex_ProducerInfoNotSetAndWarningFiledAndRowNotParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; ignored line",
                "G1 X29"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 1);

            // Assert
            _layupMock.VerifySet(p => p.Producer = It.IsAny<string>(), Times.Never);
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(1, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void ProducerParsing_FirstRowContainsCommandAndComment_ProducerInfoNotSetAndWarningFiledAndRowNotParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 X23; invalid comment"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _layupMock.VerifySet(p => p.Producer = It.IsAny<string>(), Times.Never);
            _layupMock.VerifySet(p => p.ProducerVersion = It.IsAny<string>(), Times.Never);
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));

            Assert.AreEqual(0, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void BteParsing_NoCommentIn2ndLine_BteNotSetAndWarningFiledAndRowNotParsed()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; foo bar",
                "G1 C21"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));
            _layupMock.VerifySet(p => p.Bte = It.IsAny<string>(), Times.Never);
            Assert.AreEqual(1, nextParsingPosition);
        }

        [Test]
        [Category("Short")]
        public void BteParsing_2ndRowIsCommandWithComment_BteNotSetAndWarningFiledAndRowNotParsed()
        {
            // Arrange
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; foo bar",
                "G1 C21 ; Estimated print time: foo"
            });

            // Act
            var nextParsingPosition = _sut.Parse(_layupMock.Object, lines.ToList(), 0);

            // Assert
            _loggerMock.Verify(m => m.Warn(It.IsAny<string>(), null));
            _layupMock.VerifySet(p => p.Bte = It.IsAny<string>(), Times.Never);
            Assert.AreEqual(1, nextParsingPosition);
        }
    }
}

using System.IO;
using System.Text;
using Moq;
using Logger.Contract;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test
{
    [TestFixture]
    public class GCodeReaderToolsTest
    {
        #region Fields

        private Mock<ILogger> _loggerMock;

        private GCodeReaderTools _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();

            _sut = new GCodeReaderTools(_loggerMock.Object);
        }

        #endregion

        [Test]
        [Category("Long")]
        public void ReadLayup_ReadGCodeFile_ContentReadCorrectly()
        {
            // Arrange
            var stream = EmbeddedResources.GetAsStreamReader(".assets._Loop_Spiral_Special_FULLHEIGHT.gcode");

            // Act
            var layup =_sut.ReadLayup(stream);

            // Assert
            Assert.AreEqual(39, layup.ZChunks.Count);

            var firstStretch = layup.ZChunks[0].Stretches[0];

            //Assert.That(firstStretch.CentralPoints[0].X, Is.EqualTo(282.75099).Within(0.001));

            stream.Dispose();
        }

        [Test]
        [Category("Short")]
        public void ProducerInformation_RecognizableFibrifyInformation_ProducedAndProcuderVersionCorrectlyParsed()
        {
            // Arrange
            var content = "; Fibrifier 1.1.79.4860-g6404e6f (feature/FIBRIFIER-21-build-fibrifier-with-visual)";
            using (var streamReader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(content))))
            {
                // Act
                var layup =_sut.ReadLayup(streamReader);

                // Assert
                Assert.AreEqual("Fibrifier", layup.Producer);
                Assert.AreEqual("1.1.79.4860", layup.ProducerVersion);
            }
        }

        [Test]
        [Category("Short")]
        public void
            ProducerInformation_RecognizableFibrifyInformationWithoutVersion_ProducedCorrectAndProcuderVersionEmpty()
        {
            // Arrange
            var content = "; Fibrifier";
            using (var streamReader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(content))))
            {
                // Act
                var layup =_sut.ReadLayup(streamReader);

                // Assert
                Assert.AreEqual("Fibrifier", layup.Producer);
                Assert.IsTrue(string.IsNullOrEmpty(layup.ProducerVersion));
            }
        }

        [Test]
        [Category("Short")]
        public void ProducerInformation_ContainsNoKnownProducerInformation_UnknownProducer()
        {
            // Arrange
            var content = "; FooBar";

            using (var streamReader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(content))))
            {
                // Act
                var layup =_sut.ReadLayup(streamReader);

                // Assert
                Assert.AreEqual(Layup.UnknownProducerId, layup.Producer);
            }
        }

        [Test]
        [Category("Short")]
        public void Bte_CommentWithBte_BteParsedCorrectly()
        {
            // Arrange
            var content = ";Estimated print time: 16min 37s";
            using (var streamReader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(content))))
            {
                // Act
                var layup =_sut.ReadLayup(streamReader);

                // Assert
                Assert.AreEqual("16min 37s", layup.Bte);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Moq;
using LayerSource.GCode.Parser;
using Logger.Contract;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class CommandParserTest
    {
        #region Fields

        private Command _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _sut = new Command()
            {
                Logger = Mock.Of<ILogger>(),
            };
        }

        #endregion

        [Test]
        [Category("Short")]
        public void Parse_GCodeCommand_StartIndexIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "A123"
            });

            // Act
            int result = _sut.Parse(gcode, "A123", null, null, 0);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test, Category("Short")]
        public void Parse_GCodeCommandNotInList_StartIndexNotIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "B42"
            });

            // Act
            int result = _sut.Parse(gcode, "C4", null, null, 0);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test, Category("Short")]
        public void Parse_GCodeCommandWithParameterConstraintFulfilled_StartIndexIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 C4"
            });

            // Act
            int result = _sut.Parse(gcode, "G1", ("C", 4), null, 0);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test, Category("Short")]
        public void Parse_GCodeCommandWithParameterConstraintNotFulfilled_StartIndexNotIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 G3"
            });

            // Act
            int result = _sut.Parse(gcode, "G1", ("C", 3), null, 0);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test, Category("Short")]
        public void Parse_GCodeCommandWithParameterContraintParameterValueIncorrect_StartIndexNotIncreased()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 C3"
            });

            // Act
            int result = _sut.Parse(gcode, "G1", ("C", 4), null, 0);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test, Category("Short")]
        public void ReadParameter_CommandWithParameter_ParameterValueCanBeRead()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "G1 C64 Amiga500"
            });

            // Act
            double value1 = 0;
            double value2 = 0;

            _ = _sut.Parse(gcode,
                "G1",
                null,
                new List<(string, Action<double>)>()
                {
                    ("C", (val) => value1 = val),
                    ("Amiga", (val) => value2 = val),
                },
                0);

            // Assert
            Assert.AreEqual(64, value1);
            Assert.AreEqual(500, value2);
        }
    }
}

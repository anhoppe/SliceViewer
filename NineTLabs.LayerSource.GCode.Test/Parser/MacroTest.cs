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
    public class MacroTest
    {
        #region Fields

        private Mock<ILogger> _loggerMock;

        private Macro _sut;

        #endregion

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();

            _sut = new Macro()
            {
                Logger = _loggerMock.Object,
            };
        }

        [Test, Category("Short")]
        public void Parse_ExpectedMacroUpperCaseAndGCodeMacroUpperCase_StartIndexIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "FOO",
            });

            // Act
            var result = _sut.Parse(gcode, 
                "FOO", 
                null, 
                null, 
                null,
                0);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test, Category("Short")]
        public void Parse_ExpectedMacroUpperCaseAndMacroLowerCase_StartIndexIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
            });

            // Act
            var result = _sut.Parse(gcode, 
                "FOO", 
                null, 
                null, 
                null,
                0);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test, Category("Short")]
        public void Parse_ExpectedParameterNotFound_WarningFiledStartIndexNotIncreamented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo",
            });

            _sut.SectionId = "foobar";

            // Act
            var result = _sut.Parse(gcode, 
                "bar", 
                null, 
                null,
                null, 
                0);

            // Assert
            Assert.AreEqual(0, result);
            _loggerMock.Verify(m => m.Warn(It.Is<string>(s => s.Contains("bar") && s.Contains("foobar")), null));
        }

        [Test, Category("Short")]
        public void ParseMacroNumericParameter_Request3IntParametersAndParameterIsInGCode_ParameterSet()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo bar   =   42 foobar=  13.69 barfoo       =23 foo=7",
            });

            // Act
            int foundValue1 = 0;
            double foundValue2 = 0;
            int foundValue3 = 0;
            int foundValue4 = 0;

            _ = _sut.Parse(
                gcode,
                "foo",
                new List<(string, Action<double>)>()
                {
                    ("bar", value => foundValue1 = (int)value),
                    ("foobar", value => foundValue2 = value),
                    ("barfoo", value => foundValue3 = (int)value),
                    ("foo", value => foundValue4 = (int)value),
                },
                null,
                null,
                0
            );

            // Assert
            Assert.AreEqual(42, foundValue1);
            Assert.AreEqual(13.69, foundValue2);
            Assert.AreEqual(23, foundValue3);
            Assert.AreEqual(7, foundValue4);
        }

        [Test, Category("Short")]
        public void ParseMacroNumericParameter_RequestedParametersNotInGCode_WarningFiledAndStartIndexIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo bar=42",
            });

            _sut.SectionId = "barfoo";

            // Act
            int foundValue = 0;

            int result = _sut.Parse(
                gcode,
                "foo",
                new List<(string, Action<double>)>()
                {
                    ("foobar", value => foundValue = (int)value),
                },
                null,
                null,
                0
            );

            // Assert
            Assert.AreEqual(1, result);
            Assert.AreEqual(0, foundValue);
            _loggerMock.Verify(m => m.Warn(It.Is<string>(s => s.Contains("bar") && s.Contains("barfoo") && s.Contains("foo")), null));
        }


        [Test, Category("Short")]
        public void ParseMacroNumericParameter_RequestedMacroNotInGCode_ParameterNotParsedAndStartIndexNotIncremented()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo bar=42",
            });

            // Act
            int foundValue = 0;

            int result = _sut.Parse(
                gcode,
                "foobar",
                new List<(string, Action<double>)>()
                {
                    ("bar", value => foundValue = (int)value),
                },
                null,
                null,
                0
            );

            // Assert
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, foundValue);
        }

        [Test, Category("Short")]
        public void ParseMacroStringParameter_RequestParameterAndParameterInCode_ParameterSet()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo bar=foobar barfoo=foo_foo foo='foo bar'",
            });

            // Act
            string foundValue1 = null;
            string foundValue2 = null;
            string foundValue3 = null;

            int result = _sut.Parse(
                gcode,
                "foo",
                null,
                new List<(string, Action<string>)>()
                {
                    ("bar", value => foundValue1 = value),
                    ("barfoo", value => foundValue2 = value),
                    ("foo", value => foundValue3 = value),
                },
                null,
                0
            );

            // Assert
            Assert.AreEqual("foobar", foundValue1);
            Assert.AreEqual("foo_foo", foundValue2);
            Assert.AreEqual("foo bar", foundValue3);
        }

        [Test, Category("Short")]
        public void ParseMacroStringParameter_RequestParameterValueInSingleQuotes_ParameterSet()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo foobar=32 barbar=foo bar='foo bar barfoo'",
            });

            // Act
            string foundValue = null;

            int result = _sut.Parse(
                gcode,
                "foo",
                null,
                new List<(string, Action<string>)>()
                {
                    ("bar", value => foundValue = value),
                },
                null,
                0
            );

            // Assert
            Assert.AreEqual("foo bar barfoo", foundValue);
        }

        [Test, Category("Short")]
        public void ParameterConstraint_MacroDoesNotContainParameter_MacroNotParsed()
        {
            // Arrange
            var gcode = GCodeHelper.CreateGCodeLines(new[]
            {
                "foo bar=foobar",
            });

            // Act

            int result = _sut.Parse(
                gcode,
                "foo",
                null,
                null,
                ("foo", "bar"),
                0
            );

            // Assert
            Assert.AreEqual(0, result);
        }
    }
}

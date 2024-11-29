using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using LayerSource.Contract;
using LayerSource.GCode.Parser;
using Logger.Contract;
using UnitTestTools;
using NUnit.Framework;

namespace LayerSource.GCode.Test.Parser
{
    [TestFixture]
    public class StartCodeTest
    {
        #region Fields

        private Mock<ICommand> _commandParseMock;

        private Mock<ISettableLayup> _layupMock;

        private Mock<ILogger> _loggerMock;
        
        private Mock<IMacro> _macroParserMock;

        private StartCode _sut;

        #endregion

        #region

        [SetUp]
        public void SetUp()
        {
            _commandParseMock = new Mock<ICommand>();

            _layupMock = new Mock<ISettableLayup>();

            _loggerMock = new Mock<ILogger>();

            _macroParserMock = new Mock<IMacro>();

            _sut = new StartCode()
            {
                CommandParser = _commandParseMock.Object,
                Logger = _loggerMock.Object,
                MacroParser = _macroParserMock.Object,
            };
        }

        #endregion

        [Test, Category("Short")]
        public void Parse_ControlCommandsMissing_WarningForMissingControlCommandLogged()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; *--START CODE--",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                "; ",
                //"; begin of start gcode for foo",
                // "CLEAR_PAUSE; clear any saved pause states",


                // "G21 ; set units to millimeters",
                // "G90; use absolute coordinates",
                // "M83; extruder relative mode",

                //"SET_PIN PIN = lamps VALUE = 0.7",

                //"SET_HEATER_TEMPERATURE HEATER=ir_unit TARGET=7 ; IrSetPoint=7",

                //"M104 S13 T0 ; <FiberNozzleTempSetPoint>=13",
                //"M104 S23 T1 ; <PlasticNozzleTempSetPoint>=23",
                //"M140 S42 ; <BedTempSetPoint>=42",

                // "G28; Home all axes",

                //"M190 S42; BedSetTempPoint",

                // "G29; Meshbed leveling",


                //"G1 Z3 F5500",


                //"M109 S13 T0 ; <FiberNozzleTempSetPoint>",
                //"M109 S23 T1 ; <PlasticNozzleTempSetPoint>",


                //"G28 W ; re - home FG after it has heated up",

                //"T1; change to plastic extruder",

                //"G1 X100 Y3 Z0.5 F5500; go outside print area",
                //"G1 X60 E9 F1000; intro line",
                //"G1 X20 E12.5 F1000; intro line",
                //"G92 E0.0",

                //"SET_RETRACTION RETRACT_LENGTH = 6.0 RETRACT_SPEED = 40.0",

                //"SET_HEATER_TEMPERATURE HEATER=print_chamber target=56 ; <PrintChamberTempSetPoint>",
                //"SET_HEATER_TEMPERATURE HEATER=material_chamber target=69 ; <MaterialChamberTempSetPoint>",
                "; end of start gcode",
                "; *--END OF START CODE--",
                ";foobar"
            });


            _macroParserMock.Setup(m => m.Parse(lines, "CLEAR_PAUSE", null, null, null, 1)).Returns(2);
            _commandParseMock.Setup(m => m.Parse(lines, "G21", null, null, 2)).Returns(3);
            _commandParseMock.Setup(m => m.Parse(lines, "G90", null, null, 3)).Returns(4);
            _commandParseMock.Setup(m => m.Parse(lines, "M83", null, null, 4)).Returns(5);
            _macroParserMock.Setup(m => m.Parse(lines, "SET_HEATER_TEMPERATURE", It.IsAny<List<(string, Action<double>)>>(),null, It.IsAny<(string, string)>(),5)).Returns(6);
            _commandParseMock.Setup(m => m.Parse(lines, "M104", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 6)).Returns(7);
            _commandParseMock.Setup(m => m.Parse(lines, "M104", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 7)).Returns(8);
            _commandParseMock.Setup(m => m.Parse(lines, "M140", null,It.IsAny<IList<(string, Action<double>)>>(), 8)).Returns(9);
            _commandParseMock.Setup(m => m.Parse(lines, "G28", null,null, 9)).Returns(10);
            _commandParseMock.Setup(m => m.Parse(lines, "M190", null, It.IsAny<IList<(string, Action<double>)>>(), 10)).Returns(11);
            _commandParseMock.Setup(m => m.Parse(lines, "G29", null, null, 11)).Returns(12);
            _commandParseMock.Setup(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 12)).Returns(13);
            _commandParseMock.Setup(m => m.Parse(lines, "M109", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 13)).Returns(14);
            _commandParseMock.Setup(m => m.Parse(lines, "M109", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 14)).Returns(15);
            _commandParseMock.Setup(m => m.Parse(lines, "G28", null,null, 15)).Returns(16);
            _commandParseMock.Setup(m => m.Parse(lines, "T1", null,null, 16)).Returns(17);
            _commandParseMock.Setup(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 17)).Returns(18);
            _commandParseMock.Setup(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 18)).Returns(19);
            _commandParseMock.Setup(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 19)).Returns(20);
            _commandParseMock.Setup(m => m.Parse(lines, "G92", It.IsAny<(string, double)>(), null, 20)).Returns(21);
            _macroParserMock.Setup(m => m.Parse(lines, "SET_RETRACTION", It.IsAny<List<(string, Action<double>)>>(), null, null, 21)).Returns(22);
            _macroParserMock.Setup(m => m.Parse(lines, "SET_HEATER_TEMPERATURE", It.IsAny<List<(string, Action<double>)>>(), null, null, 22)).Returns(23);
            _macroParserMock.Setup(m => m.Parse(lines, "SET_HEATER_TEMPERATURE", It.IsAny<List<(string, Action<double>)>>(), null, null, 23)).Returns(24);


            // Act
            int nextLineToParse = _sut.Parse(_layupMock.Object, lines, 0);
            
            // Assert
            //Assert.AreEqual(";foobar", lines[nextLineToParse].orig_string);

            _macroParserMock.Verify(m => m.Parse(lines, "CLEAR_PAUSE", null, null, null, 1), Times.Once);
            _commandParseMock.Verify(m => m.Parse(lines, "G21", null, null, 2));
            _commandParseMock.Verify(m => m.Parse(lines, "G90", null, null, 3));
            _commandParseMock.Verify(m => m.Parse(lines, "M83", null, null, 4));
            _macroParserMock.Verify(m => m.Parse(lines, "SET_HEATER_TEMPERATURE",
                It.IsAny<List<(string, Action<double>)>>(), null, It.IsAny<(string, string)>(), 5));
            _commandParseMock.Verify(m => m.Parse(lines, "M104", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 6));
            _commandParseMock.Verify(m => m.Parse(lines, "M104", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 7));
            _commandParseMock.Verify(m => m.Parse(lines, "M190", null, It.IsAny<IList<(string, Action<double>)>>(), 8));
            _commandParseMock.Verify(m => m.Parse(lines, "G28", null, null, 9));
            _commandParseMock.Verify(m => m.Parse(lines, "M190", null, It.IsAny<IList<(string, Action<double>)>>(), 10));
            _commandParseMock.Verify(m => m.Parse(lines, "G29", null, null, 11));
            _commandParseMock.Verify(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 12));
            _commandParseMock.Verify(m => m.Parse(lines, "M109", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 13));
            _commandParseMock.Verify(m => m.Parse(lines, "M109", It.IsAny<(string, double)>(),
                It.IsAny<IList<(string, Action<double>)>>(), 14));
            _commandParseMock.Verify(m => m.Parse(lines, "G28", null, null, 15));
            _commandParseMock.Verify(m => m.Parse(lines, "T1", null, null, 16));
            _commandParseMock.Verify(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 17));
            _commandParseMock.Verify(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 18));
            _commandParseMock.Verify(m => m.Parse(lines, "G1", It.IsAny<(string, double)>(), null, 19));
            _commandParseMock.Verify(m => m.Parse(lines, "G92", It.IsAny<(string, double)>(), null, 20));
            _macroParserMock.Verify(m => m.Parse(lines, "SET_RETRACTION", It.IsAny<List<(string, Action<double>)>>(), null, null, 21));
            _macroParserMock.Verify(m => m.Parse(lines, "SET_HEATER_TEMPERATURE", It.IsAny<List<(string, Action<double>)>>(), null, null, 22));
            _macroParserMock.Verify(m => m.Parse(lines, "SET_HEATER_TEMPERATURE", It.IsAny<List<(string, Action<double>)>>(), null, null, 23));

        }



        [Test, Category("Short")]
        public void ParameterParsing_ContainsStartEndMarker_AllVariablesReadCorrectly()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; *--START CODE--",
                "; begin of start gcode for foo",
                "CLEAR_PAUSE; clear any saved pause states",
                
                
                "G21 ; set units to millimeters",
                "G90; use absolute coordinates",
                "M83; extruder relative mode",
                
                "SET_PIN PIN = lamps VALUE = 0.7",

                "SET_HEATER_TEMPERATURE HEATER=ir_unit TARGET=7 ; IrSetPoint=7",
                
                "M104 S13 T0 ; <FiberNozzleTempSetPoint>=13",
                "M104 S23 T1 ; <PlasticNozzleTempSetPoint>=23",
                "M140 S42 ; <BedTempSetPoint>=42",
                
                "G28; Home all axes",
                
                "M190 S42; BedSetTempPoint",
                
                "G29; Meshbed leveling",
                
                
                "G1 Z3 F5500",
                
                
                "M109 S13 T0 ; <FiberNozzleTempSetPoint>",
                "M109 S23 T1 ; <PlasticNozzleTempSetPoint>",
                
                
                "G28 W ; re - home FG after it has heated up",
                
                "T1; change to plastic extruder",
                
                "G1 X100 Y3 Z0.5 F5500; go outside print area",
                "G1 X60 E9 F1000; intro line",
                "G1 X20 E12.5 F1000; intro line",
                "G92 E0.0",
                
                "SET_RETRACTION RETRACT_LENGTH = 6.0 RETRACT_SPEED = 40.0",
                
                "SET_HEATER_TEMPERATURE HEATER=print_chamber target=56 ; <PrintChamberTempSetPoint>",
                "SET_HEATER_TEMPERATURE HEATER=material_chamber target=69 ; <MaterialChamberTempSetPoint>",
                "; end of start gcode",
                "; *--END OF START CODE--",
                ";foobar"
            });


            // Act
            int nextLineToParse = _sut.Parse(_layupMock.Object, lines, 0);

            // Assert
            Assert.AreEqual(";foobar", lines[nextLineToParse].orig_string);
            _layupMock.VerifySet(p => p.MachineType = "foo");
            _layupMock.VerifySet(p => p.IrTempSetPointC = 7);
            _layupMock.VerifySet(p => p.FiberNozzleTempSetPointC = 13);
            _layupMock.VerifySet(p => p.PlasticNozzleTempSetPointC = 23);
            _layupMock.VerifySet(p => p.BedTempTempSetPointC = 42);
            _layupMock.VerifySet(p => p.PrintChamberTempSetPointC = 56);
            _layupMock.VerifySet(p => p.MaterialChamberTempSetPointC = 69);

        }

        [Test, Category("Short")]
        public void StartEndMarker_ContainsNoStartMarker_WarningFiledStartIndexNotIncremented()
        {
            // Arrange
            var lines = GCodeHelper.CreateGCodeLines(new[]
            {
                "; a row",
                "G1 X29",
                "; end of start gcode",
                "; *--END OF START CODE--",
            });

            // Act
            int nextLineToParse = _sut.Parse(_layupMock.Object, lines, 0);

            // Assert
        }

    }
}

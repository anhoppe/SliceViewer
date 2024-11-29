using System;
using System.Collections.Generic;
using gs;
using Logger.Contract;

namespace LayerSource.GCode.Parser
{
    internal class StartCode : IParser
    {
        #region Fields

        private const string ClearPauseTag = "CLEAR_PAUSE";

        private const string SetPinTag = "SET_PIN";

        private const string StartTag1 = "; *--START CODE--";

        private const string StartTag2 = "; begin of start gcode for ";

        private const string SetPointParameterId = "S";

        private const string SetRetractionTag = "SET_RETRACTION";

        private const string ExtrusionParameterId = "E";

        private const string InitialPlasticDepositIntroLine2 = "G1 X20 E12.5 F1000";

        private const string InitialPlasticDepositIntroLine1 = "G1 X60 E9 F1000";

        private const string AngleParameterId = "W";

        private const string NozzleParameterId = "T";

        private const string SetHeaterTemperatureTag = "SET_HEATER_TEMPERATURE";

        private const string InitialPlasticDepositTravelToTag = "G1 X100 Y3 Z0.5 F5500";

        #endregion

        #region Public Methods

        public int Parse(ISettableLayup layup, IList<GCodeLine> gcode, int startIndex)
        {
            if (gcode[startIndex].comment.ToLower() == StartTag1.ToLower())
            {
                startIndex += 1;

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

                if (gcode[startIndex].orig_string.ToLower().StartsWith(StartTag2))
                {
                    var targetMachineRow = gcode[startIndex].orig_string.ToLower();
                    var position = targetMachineRow.IndexOf(StartTag2, StringComparison.InvariantCulture);
                    layup.MachineType = targetMachineRow.Substring(position + StartTag2.Length);

                    startIndex++;
                }
                else
                {
                    Logger.Warn($"did not find expected start code 2 in start code: {StartTag2}");
                }

                startIndex = MacroParser.Parse(gcode, ClearPauseTag, null, null, null, startIndex);

                startIndex = CommandParser.Parse(gcode, "G21", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G90", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "M83", null, null, startIndex);
                startIndex = MacroParser.Parse(gcode, SetPinTag, null, null, ("PIN", "lamps"), startIndex);
                startIndex = MacroParser.Parse(gcode, SetHeaterTemperatureTag, null, null, ("HEATER", "ir_unit"), startIndex);
                startIndex = CommandParser.Parse(gcode, "M104", ("T", 0), new List<(string, Action<double>)> {("S", value => layup.FiberNozzleTempSetPointC = value)}, startIndex);
                startIndex = CommandParser.Parse(gcode, "M104", ("T", 1), new List<(string, Action<double>)> {("S", value => layup.PlasticNozzleTempSetPointC = value)}, startIndex);
                startIndex = CommandParser.Parse(gcode, "M140", null, new List<(string, Action<double>)> {("S", value => layup.BedTempTempSetPointC = value)}, startIndex);

                startIndex = CommandParser.Parse(gcode, "G28", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "M190", ("S", layup.BedTempTempSetPointC), null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G29", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G21", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "M109", ("T", 0), null, startIndex);
                startIndex = CommandParser.Parse(gcode, "M109", ("T", 1), null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G28", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "T1", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G1", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G1", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G1", null, null, startIndex);
                startIndex = CommandParser.Parse(gcode, "G29", null, null, startIndex);
                startIndex = MacroParser.Parse(gcode, SetRetractionTag, null, null, null, startIndex);
                startIndex = MacroParser.Parse(gcode, SetHeaterTemperatureTag, new List<(string, Action<double>)> {("target", value => layup.PrintChamberTempSetPointC = value)}, null, ("HEATER", "print_chamber"), startIndex);
                startIndex = MacroParser.Parse(gcode, SetHeaterTemperatureTag, new List<(string, Action<double>)> {("target", value => layup.MaterialChamberTempSetPointC = value)}, null, ("HEATER", "material_chamber"), startIndex);
            }

            return startIndex + 2;
        }

        #endregion

        internal ICommand CommandParser { private get; set; }

        internal IMacro MacroParser { private get; set; }

        internal ILogger Logger { private get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using gs;
using Logger.Contract;

namespace LayerSource.GCode.Parser
{
    internal class Command : ICommand
    {
        internal ILogger Logger { private get; set; }

        internal string SectionId { private get; set; }

        public int Parse(IList<GCodeLine> gcode, string expectedCommand,
            (string parameter, double value)? parameterConstraint,
            IList<(string, Action<double>)> parsedParameters,
            int startIndex)
        {
            if (gcode[startIndex].orig_string.StartsWith(expectedCommand))
            {
                if (IsParameterConstraintSatisfied(gcode[startIndex], parameterConstraint))
                {
                    ParseParameters(gcode, parsedParameters, startIndex);
                    startIndex++;
                }
            }
            else
            {
                Logger.Warn(
                    $"Did not find expected GCode command in row {gcode[startIndex].orig_string} of section {SectionId}");
            }

            return startIndex;
        }

        private void ParseParameters(IList<GCodeLine> gcode, IList<(string, Action<double>)> parsedParameters,
            int startIndex)
        {
            foreach (var parameter in parsedParameters)
            {
                if (gcode[startIndex].parameters.Any(p => p.identifier == parameter.Item1))
                {
                    parameter.Item2.Invoke(gcode[startIndex].parameters.First(p => p.identifier == parameter.Item1)
                        .doubleValue);
                }
                else
                {
                    Logger.Warn(
                        $"Did not find expected parameter {parameter.Item1} in GCode command {gcode[startIndex].orig_string} in section {SectionId}");
                }
            }
        }

        private bool IsParameterConstraintSatisfied(GCodeLine gcodeLine,
            (string parameter, double value)? parameterConstraint)
        {
            var parameterConstraintSatisfied = true;

            if (parameterConstraint != null)
            {
                parameterConstraintSatisfied = gcodeLine.parameters.Any(p =>
                    p.identifier == parameterConstraint.Value.Item1 &&
                    Math.Abs(p.doubleValue - parameterConstraint.Value.Item2) < double.Epsilon);

                if (!parameterConstraintSatisfied)
                {
                    Logger.Warn(
                        $"Parameter constraint in GCode command not satisfied, expected {parameterConstraint.Value.Item1}={parameterConstraint.Value.Item2} in {gcodeLine.orig_string} in section {SectionId}");
                }
            }

            return parameterConstraintSatisfied;
        }
    }
}

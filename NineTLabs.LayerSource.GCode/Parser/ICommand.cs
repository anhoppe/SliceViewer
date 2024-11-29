using System;
using System.Collections.Generic;
using gs;

namespace LayerSource.GCode.Parser
{
    internal interface ICommand
    {
        int Parse(IList<GCodeLine> gcode, string expectedCommand,
            (string parameter, double value)? parameterConstraint,
            IList<(string, Action<double>)> parsedParameters,
            int startIndex);

    }
}

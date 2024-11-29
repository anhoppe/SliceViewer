using System;
using System.Collections.Generic;
using gs;

namespace LayerSource.GCode.Parser
{
    internal interface IMacro
    {
        #region Public Methods

        int Parse(IList<GCodeLine> gcode,
            string macro,
            IList<(string, Action<double>)> intParameters,
            IList<(string, Action<string>)> stringParameters,
            (string parameterName, string value)? parameterConstraint,
            int startIndex);

        #endregion
    }
}

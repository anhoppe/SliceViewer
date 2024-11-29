using System.Collections.Generic;
using gs;

namespace LayerSource.GCode.Parser
{
    internal interface IZChunkParser : ISectionParser
    {
        #region Public Methods

        int Parse(IList<GCodeLine> gcode, ISettableLayup layup, int startIndex);

        #endregion
    }
}
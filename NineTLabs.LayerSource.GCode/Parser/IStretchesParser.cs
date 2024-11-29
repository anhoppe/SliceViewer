using System.Collections.Generic;
using gs;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal interface IStretchesParser : ISectionParser
    {
        int Parse(IList<GCodeLine> gcode, IZChunk zChunk, int startIndex);
    }
}

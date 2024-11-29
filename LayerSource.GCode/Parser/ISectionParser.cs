using System.Collections.Generic;
using gs;

namespace LayerSource.GCode.Parser
{
    internal interface ISectionParser
    {
        #region Public Methods

        bool CanParse(GCodeLine line);

        #endregion
    }
}
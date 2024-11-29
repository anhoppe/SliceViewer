using System.Collections.Generic;
using gs;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal interface IParser
    {
        #region Public Methods

        /// <summary>
        /// Parses a chunk from the gcode line list.
        /// </summary>
        /// <param name="layup">Allows access to the settable layup which can be used to produce a layup</param>
        /// <param name="gcode">List of GCode lines to be parsed</param>
        /// <param name="startIndex">Start index of parsing for the chunk</param>
        /// <returns>Index of the next element after the parsed chunk, -1 if there is no such element</returns>
        int Parse(ISettableLayup layup, IList<GCodeLine> gcode, int startIndex);

        #endregion
    }
}

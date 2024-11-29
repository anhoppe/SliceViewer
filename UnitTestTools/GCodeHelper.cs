using System.Collections.Generic;
using System.IO;
using System.Linq;
using gs;

namespace UnitTestTools
{
    public static class GCodeHelper
    {
        #region Public Methods

        public static List<GCodeLine> CreateGCodeLines(IList<string> gcode)
        {
            List<GCodeLine> resultGCodeLines = null;

            var gcodeParser = new GenericGCodeParser();

            using (var reader = new StringReader(string.Join("\r\n", gcode)))
            {
                var file = gcodeParser.Parse(reader, false);

                resultGCodeLines = file.AllLines().ToList();
            }

            return resultGCodeLines;
        }

        #endregion
    }
}

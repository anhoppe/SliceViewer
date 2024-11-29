using System.Collections.Generic;
using gs;
using Logger.Contract;

namespace LayerSource.GCode.Parser
{
    internal class SectionParser
    {
        #region Fields

        private IList<GCodeLine> _gcode;

        #endregion

        #region Public Properties

        public string EndMarker { private get; set; }

        public IList<GCodeLine> Gcode
        {
            get
            {
                if (_gcode == null)
                {
                    _gcode = new List<GCodeLine>();
                }

                return _gcode;
            }
        }

        public string StartMarker { private get; set; }

        #endregion

        #region Properties

        internal ILogger Logger { private get; set; }

        #endregion

        internal string FoundStartMarker { get; private set; }

        #region Public Methods

        public int Parse(IList<GCodeLine> gcode, int startIndex)
        {
            FoundStartMarker = string.Empty;

            var startMarkerFound = StartMarker == null;
            var endMarkerFound = EndMarker == null;

            while (startIndex < gcode.Count)
            {
                var gcodeLine = gcode[startIndex++];

                if (EndMarker != null && gcodeLine.orig_string.StartsWith(EndMarker))
                {
                    endMarkerFound = true;

                    break;
                }

                if (StartMarker != null && gcodeLine.orig_string.StartsWith(StartMarker))
                {
                    startMarkerFound = true;
                    FoundStartMarker = gcodeLine.orig_string;
                }
                else if (startMarkerFound)
                {
                    Gcode.Add(gcodeLine);
                }
            }

            if (!startMarkerFound)
            {
                Logger.Warn($"Unexpected GCode section: Did not find expected start marker: {StartMarker}");
            }

            if (!endMarkerFound)
            {
                Logger.Warn($"Unexpected GCode section: Did not find expected end marker: {EndMarker}");
            }

            return startIndex;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using gs;

namespace LayerSource.GCode.Parser
{
    internal class ZChunkParser : IZChunkParser
    {
        #region Nested Types

        private enum States
        {
            None,
            ParsingStartTag,
            ParsingStretches
        }

        #endregion

        #region Fields

        private static readonly string EndMarkerZChunk = "; - END OF ZCHUNK";

        private static readonly string StartMarkerZChunk = "; - START OF ZCHUNK";

        #endregion

        #region Constructors

        #endregion

        #region Properties

        internal IStretchesParser FiberStretchParser { private get; set; }

        internal IStretchesParser PlasticStretchParser { private get; set; }

        internal Func<int, double, double, ISettableZChunk> ZChunkFactory { private get; set; }

        #endregion

        #region Public Methods

        public bool CanParse(GCodeLine line)
        {
            return line.orig_string.StartsWith(StartMarkerZChunk);
        }

        public int Parse(IList<GCodeLine> gcode, ISettableLayup layup, int startIndex)
        {
            var state = States.ParsingStartTag;

            ISettableZChunk zChunk = null;

            var index = startIndex;

            while (index < gcode.Count)
            {
                var line = gcode[index];

                if (line.orig_string.StartsWith(EndMarkerZChunk))
                {
                    index++;

                    break;
                }

                if (state == States.ParsingStartTag)
                {
                    zChunk = ParseZChunkStartMarker(line, ref state);

                    index++;
                }
                else if (state == States.ParsingStretches)
                {
                    index = ParseStretches(gcode, index, zChunk);
                }
            }

            layup.AddZChunk(zChunk);

            return index;
        }

        #endregion

        #region Methods

        private int ParseStretches(IList<GCodeLine> gcode, int index, ISettableZChunk zChunk)
        {
            if (FiberStretchParser.CanParse(gcode[index]))
            {
                index = FiberStretchParser.Parse(gcode, zChunk, index);
            }
            else if (PlasticStretchParser.CanParse(gcode[index]))
            {
                index = PlasticStretchParser.Parse(gcode, zChunk, index);
            }
            else
            {
                index++;
            }

            return index;
        }

        private ISettableZChunk ParseZChunkStartMarker(GCodeLine line, ref States state)
        {
            ISettableZChunk zChunk = null;

            if (line.orig_string.StartsWith(StartMarkerZChunk))
            {
                var indexMatcher = new Regex(@"#\d+");
                var match = indexMatcher.Match(line.orig_string);
                var zChunkIndex = int.Parse(match.Value.Substring(1));

                var zRangeMatcher = new Regex(@"\[.+\]");
                match = zRangeMatcher.Match(line.orig_string);

                var range = match.Value.TrimStart('[').TrimEnd(']').Split(',');

                var minZ = double.Parse(range[0].Trim(), CultureInfo.InvariantCulture);
                var maxZ = double.Parse(range[1].Trim(), CultureInfo.InvariantCulture);

                zChunk = ZChunkFactory.Invoke(zChunkIndex, minZ, maxZ);

                state = States.ParsingStretches;
            }

            return zChunk;
        }

        #endregion
    }
}
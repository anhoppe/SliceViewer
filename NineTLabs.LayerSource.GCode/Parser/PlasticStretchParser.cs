using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using gs;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal class PlasticStretchParser : ISectionParser
    {
        #region Fields

        private static readonly string EndPlasticStretchTag = "; - END OF PLASTIC STRETCH";

        private static readonly string StartPlasticStretchTag = "; - START OF PLASTIC STRETCH";

        #endregion

        #region Properties

        internal Func<ISettableStretch> StretchFactory { private get; set; }

        #endregion

        #region Public Methods

        public bool CanParse(GCodeLine line)
        {
            return line.orig_string == StartPlasticStretchTag;
        }

        public int Parse(List<GCodeLine> gcode, IZChunk zChunk, int startIndex)
        {
            var index = startIndex;
            ISettableStretch stretch = StretchFactory.Invoke();

            zChunk.Stretches.Add(stretch);

            for (; index < gcode.Count; index++)
            {
                var line = gcode[index];
                if (line.type == GCodeLine.LType.GCode && line.code == 1)
                {
                    var point = new Vector4((float) line.parameters.First(p => p.identifier == "X").doubleValue, (float) line.parameters.First(p => p.identifier == "Y").doubleValue, (float) line.parameters.First(p => p.identifier == "Z").doubleValue, 0);
                    stretch.DepositionPoints.Add(point);
                }

                if (line.orig_string == EndPlasticStretchTag)
                {
                    index++;

                    break;
                }
            }

            return index;
        }

        #endregion
    }
}

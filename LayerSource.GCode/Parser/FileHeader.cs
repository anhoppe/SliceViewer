using System.Collections.Generic;
using System.Linq;
using gs;
using Logger.Contract;

namespace LayerSource.GCode.Parser
{
    internal class FileHeader : IParser
    {
        #region Fields

        private static readonly string _bteId = "; Estimated print time: ";

        #endregion

        #region Public Methods

        public int Parse(ISettableLayup layup, IList<GCodeLine> gcode, int startIndex)
        {
            if (!ParseProducerInfo(layup, gcode[startIndex]))
            {
                return startIndex;
            }

            startIndex++;

            if (ParseBte(layup, gcode[startIndex]))
            {
                startIndex++;
            }

            return startIndex;
        }

        #endregion

        private bool ParseBte(ISettableLayup layup, GCodeLine gCodeLine)
        {
            var secondRow = gCodeLine.comment;

            if (string.IsNullOrEmpty(secondRow))
            {
                Logger.Warn($"Invalid file header, second row does not contain a comment: {secondRow}");
                return false;
            }

            if (gCodeLine.type != GCodeLine.LType.Comment)
            {
                Logger.Warn(
                    $"Invalid file header, expected BTE row contains command: {gCodeLine.orig_string}");
                return false;
            }

            if (secondRow.Contains(_bteId))
            {
                layup.Bte = secondRow.Substring(_bteId.Length,
                    secondRow.Length - _bteId.Length);
            }
            else
            {
                Logger.Warn($"Unexpected file header, second row does not contain BTE: {secondRow}");
            }

            return true;
        }

        private bool ParseProducerInfo(ISettableLayup layup, GCodeLine gCodeLine)
        {
            if (string.IsNullOrEmpty(gCodeLine.comment))
            {
                Logger.Warn(
                    $"Invalid file header, no comment at the beginning of header: {gCodeLine.orig_string}");
                return false;
            }

            if (gCodeLine.type != GCodeLine.LType.Comment)
            {
                Logger.Warn(
                    $"Invalid file header, beginning of header contains command: {gCodeLine.orig_string}");
                return false;
            }

            var headerElements = gCodeLine.comment.Split(' ');

            if (headerElements.Length < 2)
            {
                Logger.Warn($"Unexpected file header, empty comment at the beginning of header: {gCodeLine.comment}");
                return true;
            }

            if (!string.IsNullOrEmpty(headerElements[1]))
            {
                layup.Producer = headerElements[1];
            }
            else
            {
                Logger.Warn($"Unexpected file header, no producer found: {gCodeLine.comment}");
            }

            if (headerElements.Length > 2)
            {
                layup.ProducerVersion = string.Join(" ", headerElements.Skip(2).ToList());
            }
            else
            {
                Logger.Warn($"Unexpected file header, no producer version found: {gCodeLine.comment}");
            }

            return true;
        }

        internal ILogger Logger { private get; set; }
    }
}

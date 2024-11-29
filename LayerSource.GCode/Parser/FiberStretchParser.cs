using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using gs;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal class FiberStretchParser : IStretchesParser
    {
        #region Nested Types

        private enum States
        {
            None,
            ParseAnchoringStart,
            ParseAnchoringEnd,
            ParseDepositionMoves,
            ParseAfterCutMoves
        }

        #endregion

        #region Fields

        private static readonly string BeginOfCfStretchTag = "; - START OF CF STRETCH";

        private static readonly string CutFilamentTag = "cut_filament";

        private static readonly string EndOfCfStretchTag = "; - END OF CF STRETCH";

        private static readonly string UseAbsoluteRotaryPositionTag = "USE_ABSOLUTE_ROTARY_POSITION";

        private static readonly string UseRelativeRotaryPositionTag = "USE_RELATIVE_ROTARY_POSITION";

        #endregion

        #region Properties

        internal Func<ISettableFiberStretch> FiberStretchFactory { private get; set; }

        #endregion

        #region Public Methods

        public bool CanParse(GCodeLine line)
        {
            return line.orig_string == BeginOfCfStretchTag;
        }

        public int Parse(IList<GCodeLine> gcode, IZChunk zChunk, int startIndex)
        {
            int lineIndex;

            var state = States.None;

            ISettableFiberStretch fiberStretch = FiberStretchFactory.Invoke();
            zChunk.Stretches.Add(fiberStretch);

            for (lineIndex = startIndex; lineIndex < gcode.Count; lineIndex++)
            {
                var line = gcode[lineIndex];

                var nextState = UpdateParsingState(line);
                if (nextState != States.None)
                {
                    state = nextState;

                    continue;
                }

                if (line.orig_string == EndOfCfStretchTag)
                {
                    lineIndex++;

                    break;
                }

                ParseFiberStretch(fiberStretch, state, line);
            }

            return lineIndex;
        }

        #endregion

        #region Methods

        private static void ParseFiberStretch(ISettableFiberStretch fiberStretch, States state, GCodeLine line)
        {
            if (state == States.ParseAnchoringStart)
            {
                if (line.type == GCodeLine.LType.GCode && line.code == 1)
                {
                    fiberStretch.AnchoringStart = new Vector4((float) line.parameters.First(p => p.identifier == "X").doubleValue, (float) line.parameters.First(p => p.identifier == "Y").doubleValue, (float) line.parameters.First(p => p.identifier == "Z").doubleValue, (float) line.parameters.First(p => p.identifier == "W").doubleValue);
                }
            }
            else if (state == States.ParseAnchoringEnd)
            {
                if (line.type == GCodeLine.LType.GCode && line.code == 1)
                {
                    fiberStretch.AnchoringEnd = new Vector3((float) line.parameters.First(p => p.identifier == "X").doubleValue, (float) line.parameters.First(p => p.identifier == "Y").doubleValue, (float) line.parameters.First(p => p.identifier == "Z").doubleValue);
                }
            }
            else if (state == States.ParseDepositionMoves)
            {
                if (line.type == GCodeLine.LType.GCode && line.code == 1)
                {
                    var point = new Vector4((float) line.parameters.First(p => p.identifier == "X").doubleValue, (float) line.parameters.First(p => p.identifier == "Y").doubleValue, (float) line.parameters.First(p => p.identifier == "Z").doubleValue, (float) line.parameters.First(p => p.identifier == "W").doubleValue);
                    fiberStretch.DepositionPoints.Add(point);
                }
            }
            else if (state == States.ParseAfterCutMoves)
            {
                if (line.type == GCodeLine.LType.GCode && line.code == 1)
                {
                    var point = new Vector4((float) line.parameters.First(p => p.identifier == "X").doubleValue, (float) line.parameters.First(p => p.identifier == "Y").doubleValue, (float) line.parameters.First(p => p.identifier == "Z").doubleValue, (float) line.parameters.First(p => p.identifier == "W").doubleValue);
                    fiberStretch.AfterCutDepositionPoints.Add(point);
                }
            }
        }

        private static States UpdateParsingState(GCodeLine line)
        {
            var state = States.None;

            if (line.orig_string == UseAbsoluteRotaryPositionTag)
            {
                state = States.ParseAnchoringStart;
            }

            if (line.orig_string == UseRelativeRotaryPositionTag)
            {
                state = States.ParseAnchoringEnd;
            }

            if (line.type == GCodeLine.LType.GCode && line.code == 4)
            {
                state = States.ParseDepositionMoves;
            }

            if (line.orig_string == CutFilamentTag)
            {
                state = States.ParseAfterCutMoves;
            }

            return state;
        }

        #endregion
    }
}
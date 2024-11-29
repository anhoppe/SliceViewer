using System.Collections.Generic;
using System.Numerics;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal interface ISettableStretch : IStretch
    {
        #region Public Properties

        new IList<Vector4> DepositionPoints { get; }

        #endregion
    }
}
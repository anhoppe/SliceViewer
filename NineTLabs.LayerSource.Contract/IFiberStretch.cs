
using System.Collections.Generic;
using System.Numerics;

namespace LayerSource.Contract
{
    /// <summary>
    ///     Represents a single fiber stretch with anchoring and cut movements
    /// </summary>
    public interface IFiberStretch : IStretch
    {
        #region Public Properties

        /// <summary>
        ///     All deposition points + head angle the head moves to after the fiver cut was performed
        /// </summary>
        IList<Vector4> AfterCutDepositionPoints { get; }

        /// <summary>
        ///     End position of the anchoring move
        /// </summary>
        Vector3 AnchoringEnd { get; }

        /// <summary>
        ///     Start position of the anchoring move
        /// </summary>
        Vector4 AnchoringStart { get; }

        #endregion
    }
}
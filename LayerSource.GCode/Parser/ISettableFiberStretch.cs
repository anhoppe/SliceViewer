using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LayerSource.Contract;

namespace LayerSource.GCode.Parser
{
    internal interface ISettableFiberStretch : IFiberStretch
    {
        #region Public Properties

        /// <summary>
        ///     All deposition points + head angle the head moves to after the fiver cut was performed
        /// </summary>
        new IList<Vector4> AfterCutDepositionPoints { get; set; }

        /// <summary>
        ///     End position of the anchoring move
        /// </summary>
        new Vector3 AnchoringEnd { get; set; }

        /// <summary>
        ///     Start position of the anchoring move
        /// </summary>
        new Vector4 AnchoringStart { get; set; }

        #endregion
    }
}
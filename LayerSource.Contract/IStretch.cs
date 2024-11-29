using System.Collections.Generic;
using System.Numerics;

namespace LayerSource.Contract
{
    public interface IStretch
    {
        #region Public Properties

        double LayerHeight { get; }

        PrintType PrintType { get; }


        /// <summary>
        ///     All deposition x/y/z coordinates + head angle w. For plastic stripes the head angle is always 0
        /// </summary>
        IList<Vector4> DepositionPoints { get; }

        #endregion
    }
}
using System.Collections.Generic;

namespace LayerSource.Contract
{
    public interface IZChunk
    {
        #region Public Properties

        double Height { get; }

        IList<IStretch> Stretches { get; }

        #endregion
    }
}
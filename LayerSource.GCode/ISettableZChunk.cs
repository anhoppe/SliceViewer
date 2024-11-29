using LayerSource.Contract;

namespace LayerSource.GCode
{
    internal interface ISettableZChunk : IZChunk
    {
        #region Public Methods

        void AddStretch(IStretch stretch);

        #endregion
    }
}
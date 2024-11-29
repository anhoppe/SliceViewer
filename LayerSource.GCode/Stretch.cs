using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media.Media3D;
using LayerSource.Contract;

namespace LayerSource.GCode
{
    internal class Stretch : IStretch
    {
        #region Constructors

        public Stretch(PrintType printType, double layerHeight)
        {
            PrintType = printType;
            LayerHeight = layerHeight;
            DepositionPoints = new List<Vector4>();
        }

        #endregion

        #region Public Properties

        public PrintType PrintType { get; }

        public double LayerHeight { get; }

        public IList<Vector4> DepositionPoints { get; }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return $"PrintType: {PrintType}";
        }

        #endregion
    }
}
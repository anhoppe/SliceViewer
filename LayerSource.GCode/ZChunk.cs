using System.Collections.Generic;
using System.Linq;
using LayerSource.Contract;

namespace LayerSource.GCode
{
    internal class ZChunk : ISettableZChunk
    {
        #region Fields

        internal static readonly double DeltaZEps = 0.0001;

        private IList<IStretch> _stretches;

        #endregion

        #region Constructors

        public ZChunk(double layerHeight) { }

        #endregion

        #region Public Properties

        public IList<IStretch> FiberStretches => (from stretch in Stretches where stretch.PrintType == PrintType.Fiber select stretch).ToList();

        public double Height { get; }

        public IList<IStretch> PlasticStretches => (from stretch in Stretches where stretch.PrintType == PrintType.Plastic select stretch).ToList();

        public IList<IStretch> Stretches
        {
            get
            {
                if (_stretches == null)
                {
                    _stretches = new List<IStretch>();
                }

                return _stretches;
            }
        }

        #endregion

        #region Public Methods

        public void AddStretch(IStretch stretch)
        {
            _stretches.Add(stretch);
        }

        public override string ToString()
        {
            return $"LayerHeight: {Height}; Stretches: {Stretches.Count}; Plastic: {PlasticStretches.Count}; Fibers: {FiberStretches.Count}";
        }

        #endregion
    }
}
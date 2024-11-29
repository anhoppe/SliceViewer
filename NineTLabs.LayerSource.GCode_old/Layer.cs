using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using NineTLabs.LayerSource.Contract;

namespace NineTLabs.LayerSource.GCode
{
    internal class Layer : ILayer
    {
        public double Height { get; }

        public IList<Point3D> Points { get; }

        public IList<IStretch> Stretches { get; }

        public IList<IStretch> PlasticStretches
        {
            get =>
                (from stretch in Stretches
                 where stretch.PrintType == PrintType.Plastic
                 select stretch).ToList();
        }

        public IList<IStretch> FiberStretches
        {
            get =>
                (from stretch in Stretches
                 where stretch.PrintType == PrintType.Fiber
                 select stretch).ToList();
        }

        public Layer(double layerHeight)
        {
            Height = layerHeight;
            this.Stretches = new List<IStretch>();
        }

        public override string ToString()
        {
            return
                $"LayerHeight: {Height}; Stretches: {Stretches.Count}; Plastic: {PlasticStretches.Count}; Fibers: {FiberStretches.Count}";
        }
    }
}
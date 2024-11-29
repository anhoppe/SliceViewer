using System.Collections.Generic;
using System.Windows.Media.Media3D;
using NineTLabs.LayerSource.Contract;

namespace NineTLabs.LayerSource.GCode
{

    internal class Stretch : IStretch
    {
        public PrintType PrintType { get; }
        public double LayerHeight { get; }
        public List<Point3D> Points { get; }
        //public List<Facet3D> Facets { get; }
        public List<Vector3D> Directions { get; }


        public List<Point3D> CentralPoints { get; }

        public Stretch(PrintType printType, double layerHeight)
        {
            PrintType = printType;
            LayerHeight = layerHeight;
            Points = new List<Point3D>();
            //Facets = new List<Facet3D>();
            Directions = new List<Vector3D>();
            CentralPoints = new List<Point3D>();
        }

        public override string ToString()
        {
            return $"PrintType: {this.PrintType}; Points: {Points.Count}";
        }
    }
}

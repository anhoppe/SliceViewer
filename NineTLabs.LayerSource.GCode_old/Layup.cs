using System.Collections.Generic;
using System.Windows.Media.Media3D;
using NineTLabs.LayerSource.Contract;

namespace NineTLabs.LayerSource.GCode
{
    internal class Layup : ILayup
    {
        internal static readonly string BteId = "Estimated print time";
        
        internal static readonly string FibrifierProducerId = "Fibrifier";

        internal static readonly string UnknownProducerId = "Unknown";

        public Layup()
        {
            Layers = new List<ILayer>();
            Points = new List<Point3D>();
        }

        public string Bte { get; set; }

        public IList<ILayer> Layers { get; set; }

        public string Producer { get; set; }

        public string ProducerVersion { get; set; }

        public List<Point3D> Points { get; }

        public override string ToString()
        {
            return $"Layers: {Layers.Count}; Points: {Points.Count}";
        }
    }
}
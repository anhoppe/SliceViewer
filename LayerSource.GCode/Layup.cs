using System.Collections.Generic;
using System.Windows.Media.Media3D;
using LayerSource.Contract;

namespace LayerSource.GCode
{
    internal class Layup : ISettableLayup
    {
        #region Fields

        internal static readonly string BteId = "Estimated print time";

        internal static readonly string FibrifierProducerId = "Fibrifier";

        internal static readonly string UnknownProducerId = "Unknown";

        #endregion

        #region Constructors

        public Layup()
        {
            ZChunks = new List<Contract.IZChunk>();
            Points = new List<Point3D>();

            Producer = UnknownProducerId;
        }

        #endregion

        #region Public Properties

        public double BedTempTempSetPointC { get; set; }

        public string Bte { get; set; }

        public double FiberNozzleTempSetPointC { get; set; }

        public double IrTempSetPointC { get; set; }

        public IList<IZChunk> ZChunks { get; }

        public string MachineType { get; set; }

        public double MaterialChamberTempSetPointC { get; set; }

        public double PlasticNozzleTempSetPointC { get; set; }

        public double PrintChamberTempSetPointC { get; set; }

        public string Producer { get; set; }

        public string ProducerVersion { get; set; }

        public List<Point3D> Points { get; set; }

        #endregion

        #region Public Methods

        public void AddZChunk(IZChunk zChunk)
        {
            ZChunks.Add(zChunk);
        }

        public override string ToString()
        {
            return $"ZChunks: {ZChunks.Count}; Points: {Points.Count}";
        }

        #endregion
    }
}

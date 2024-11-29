using LayerSource.Contract;

namespace LayerSource.GCode
{
    internal interface ISettableLayup : ILayup
    {
        #region Public Properties

        new double BedTempTempSetPointC { get; set; }

        new string Bte { get; set; }

        new double FiberNozzleTempSetPointC { get; set; }

        new double IrTempSetPointC { get; set; }

        new string MachineType { get; set; }

        new double MaterialChamberTempSetPointC { get; set; }

        new double PlasticNozzleTempSetPointC { get; set; }

        new double PrintChamberTempSetPointC { get; set; }

        new string Producer { get; set; }

        new string ProducerVersion { get; set; }

        #endregion

        #region Public Methods

        void AddZChunk(IZChunk zChunk);

        #endregion
    }
}
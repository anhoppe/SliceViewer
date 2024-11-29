using System.Collections.Generic;

namespace LayerSource.Contract
{
    /// <summary>
    ///     Root-element for a build job, i.e. contains all information on layers etc.
    /// </summary>
    public interface ILayup
    {
        #region Public Properties

        /// <summary>
        ///     Temperature set point for the part bed in °C
        /// </summary>
        double BedTempTempSetPointC { get; }
        
        /// <summary>
        ///     BTE (Build time estimation) as string
        /// </summary>
        string Bte { get; }

        /// <summary>
        ///     Temperature set point for the fiber nozzle in °C
        /// </summary>
        double FiberNozzleTempSetPointC { get; }

        /// <summary>
        ///     Temperature set point for the IR unit in °C
        /// </summary>
        double IrTempSetPointC { get; }

        /// <summary>
        ///     The layer stack
        /// </summary>
        IList<IZChunk> ZChunks { get; }

        /// <summary>
        /// Type of the designated target machine of the gcode file
        /// </summary>
        string MachineType { get; }

        /// <summary>
        ///     Temperature set point for the material chamber in °C
        /// </summary>
        double MaterialChamberTempSetPointC { get; }

        /// <summary>
        ///     Temperature set point for the plastic nozzle in °C
        /// </summary>
        double PlasticNozzleTempSetPointC { get; }

        /// <summary>
        ///     Temperature set point for the print chamber in °C
        /// </summary>
        double PrintChamberTempSetPointC { get; }

        /// <summary>
        ///     Producer of the GCode file, may by 'Fibrify' or 'Unknown'
        /// </summary>
        string Producer { get; }

        /// <summary>
        ///     Version of the producer, empty if Producer is 'Unknown'
        /// </summary>
        string ProducerVersion { get; }

        #endregion
    }
}

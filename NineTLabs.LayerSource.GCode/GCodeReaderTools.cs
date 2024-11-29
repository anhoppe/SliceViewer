using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using gs;
using LayerSource.Contract;
using LayerSource.GCode.Parser;
using Logger.Contract;

namespace LayerSource.GCode
{
    public class GCodeReaderTools
    {
        #region Fields

        private readonly ILogger _logger;

        private IList<Tuple<double, IZChunk>> _layerSource;

        IZChunk _previouslyRequestedLayer = null;

        #endregion

        #region Constructors

        public GCodeReaderTools(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Public Methods

        public static void PrintInfo(ILayup layup)
        {
            Console.WriteLine(layup);
            foreach (var layer in layup.ZChunks)
            {
                Console.WriteLine($" -> {layer}");
                foreach (var stretch in layer.Stretches)
                {
                    Console.WriteLine($"   -> {stretch}");
                }
            }
        }

        public ILayup ReadLayup(StreamReader fileStream)
        {
            var layup = new Layup();
            var par = new GenericGCodeParser();
            var gcode = par.Parse(fileStream, true);

            //GenerateCentralStretches(gcode, layup);

            // Post processing step to generate nice curves
            //GenerateStretchFaces(layup);

            //layup.Layers = LayerSource.OrderBy(p => p.Item1).Select(p => p.Item2).ToList();

            return layup;
        }

        public static bool AboutEqual(double x, double y)
        {
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }

        public IZChunk Get(double zPos)
        {
            IZChunk layer = null;

            if (zPos == 0)
            {
                return _previouslyRequestedLayer;
            }

            foreach (var posLayerTuple in LayerSource)
            {
                if (Math.Abs(zPos - posLayerTuple.Item1) < ZChunk.DeltaZEps)
                {
                    layer = posLayerTuple.Item2;
                }
            }

            if (layer == null)
            {
                layer = new ZChunk(0.2);
                LayerSource.Add(new Tuple<double, IZChunk>(zPos, layer));
            }

            _previouslyRequestedLayer = layer;

            return layer;
        }

        #endregion

        private IList<Tuple<double, IZChunk>> LayerSource
        {
            get
            {
                if (_layerSource == null)
                {
                    _layerSource = new List<Tuple<double, IZChunk>>();
                }

                return _layerSource;
            }
        }

        /// <summary>
        ///     Generates Central stretches. These are lines that follow the GCode directly
        /// </summary>
        /// <param name="gcode"></param>
        /// <param name="layup"></param>
        private static void GenerateCentralStretches(GCodeFile gcode, Layup layup)
        {
            var currentMaterial = PrintType.Plastic;
            IZChunk currentLayer = new ZChunk(0.0);
            IStretch currentStretch = new Stretch(currentMaterial, currentLayer.Height);


            var vertexList = new List<Vector3D>();

            var isCurrentlyExtruding = false;

            var location = new Point3D();
            var lastLocation = new Point3D();

            foreach (var line in gcode.AllLines())
            {
                if (line.type == GCodeLine.LType.Comment)
                {
                    ParseComment(layup, line);
                }
                else if (line.type == GCodeLine.LType.UnknownString)
                {
                    if (line.orig_string.Contains("T1"))
                    {
                        if (currentMaterial != PrintType.Plastic)
                        {
                            currentMaterial = PrintType.Plastic;
                            isCurrentlyExtruding = false;
                        }
                    }
                    else if (line.orig_string.Contains("T0"))
                    {
                        if (currentMaterial != PrintType.Fiber)
                        {
                            currentMaterial = PrintType.Fiber;
                            isCurrentlyExtruding = false;
                        }
                    }
                }
                else if (line.type == GCodeLine.LType.GCode)
                {
                    // GCode move
                    if (line.code == 0 || line.code == 1)
                    {
                        var definesMove = 0;

                        foreach (var param in line.parameters)
                        {
                            if (param.identifier == "X")
                            {
                                location.X = (float) param.doubleValue;
                                definesMove++;
                            }

                            if (param.identifier == "Y")
                            {
                                location.Y = (float) param.doubleValue;
                                definesMove++;
                            }

                            if (param.identifier == "Z")
                            {
                                definesMove++;

                                //location.Z = param.doubleValue;
                                //currentLayer = layup.Layers.FirstOrDefault(layer =>
                                //    AboutEqual(layer.Height, param.doubleValue));
                                //if (currentLayer == null)
                                //{
                                //    currentLayer = new ZChunk(param.doubleValue);
                                //    layup.Layers.Add(currentLayer);
                                //}
                            }
                        }

                        // check if extruding
                        if (line.parameters.Any(p => p.identifier == "E"))
                        {
                            if (!isCurrentlyExtruding)
                            {
                                currentStretch = new Stretch(currentMaterial, currentLayer.Height);
                                //currentLayer.Stretches.Add(currentStretch);
                            }

                            isCurrentlyExtruding = true;
                        }
                        else
                        {
                            // in case of cutting
                            if (currentMaterial == PrintType.Fiber &&
                                isCurrentlyExtruding &&
                                AboutEqual(location.Z, currentStretch.LayerHeight))
                            {
                                isCurrentlyExtruding = true;
                            }
                            else
                            {
                                isCurrentlyExtruding = false;
                            }
                        }

                        //if (isCurrentlyExtruding && definesMove >= 1)
                        //{
                        //    if (!currentStretch.CentralPoints.Any())
                        //    {
                        //        currentStretch.CentralPoints.Add(lastLocation);
                        //    }

                        //    currentStretch.CentralPoints.Add(location);
                        //}

                        if (definesMove > 0)
                        {
                            lastLocation = location;
                        }
                    }
                }
            }
        }

        private static void ParseComment(Layup layup, GCodeLine line)
        {
            if (line.orig_string.Contains(Layup.FibrifierProducerId) && layup.Producer == Layup.UnknownProducerId)
            {
                layup.Producer = Layup.FibrifierProducerId;

                var startIndex = line.orig_string.IndexOf(Layup.FibrifierProducerId, StringComparison.Ordinal) +
                                 Layup.FibrifierProducerId.Length + 1;
                var endIndex = line.orig_string.IndexOf('-');

                if (endIndex != -1)
                {
                    layup.ProducerVersion = line.orig_string.Substring(startIndex, endIndex - startIndex);
                }
            }
            else if (line.orig_string.Contains(Layup.BteId))
            {
                var startIndex = line.orig_string.IndexOf(':');

                layup.Bte = line.orig_string.Substring(startIndex + 2);
            }
        }
    }
}

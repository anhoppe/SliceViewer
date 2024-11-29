using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using gs;
using NineTLabs.LayerSource.Contract;

namespace NineTLabs.LayerSource.GCode
{
    public static class GCodeReaderTools
    {
        private static readonly string FibrifierId = "Fibrifier";

        public static void PrintInfo(ILayup layup)
        {
            Console.WriteLine(layup);
            foreach (var layer in layup.Layers)
            {
                Console.WriteLine($" -> {layer}");
                foreach (var stretch in layer.Stretches) Console.WriteLine($"   -> {stretch}");
            }
        }

        public static ILayup ReadLayup(StreamReader fileStream)
        {
            var layup = new Layup();
            var par = new GenericGCodeParser();
            var gcode = par.Parse(fileStream);
            GenerateCentralStretches(gcode, layup);

            // Post processing step to generate nice curves
            //GenerateStretchFaces(layup);


            var layers = (from l in layup.Layers
                where l.Stretches.Any()
                orderby l.Height
                select l).ToList();
            layup.Layers = layers;
            return layup;
        }

        /// <summary>
        ///     Generates Central stretches. These are lines that follow the GCode directly
        /// </summary>
        /// <param name="gcode"></param>
        /// <param name="layup"></param>
        private static void GenerateCentralStretches(GCodeFile gcode, Layup layup)
        {
            PrintType currentMaterial = PrintType.Plastic;
            ILayer currentLayer = new Layer(0.0);
            IStretch currentStretch = new Stretch(currentMaterial, currentLayer.Height);



            List<Vector3D> vertexList = new List<Vector3D>();

            bool isCurrentlyExtruding = false;

            Point3D location = new Point3D();
            Point3D lastLocation = new Point3D();

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
                        int definesMove = 0;

                        foreach (var param in line.parameters)
                        {
                            if (param.identifier == "X")
                            {
                                location.X = (float)param.doubleValue;
                                definesMove++;
                            }

                            if (param.identifier == "Y")
                            {
                                location.Y = (float)param.doubleValue;
                                definesMove++;
                            }

                            if (param.identifier == "Z")
                            {
                                definesMove++;

                                location.Z = param.doubleValue;
                                currentLayer = layup.Layers.FirstOrDefault(layer =>
                                    AboutEqual(layer.Height, param.doubleValue));
                                if (currentLayer == null)
                                {
                                    currentLayer = new Layer(param.doubleValue);
                                    layup.Layers.Add(currentLayer);
                                }
                            }
                        }

                        // check if extruding
                        if (line.parameters.Any(p => p.identifier == "E"))
                        {
                            if (!isCurrentlyExtruding)
                            {
                                currentStretch = new Stretch(currentMaterial, currentLayer.Height);
                                currentLayer.Stretches.Add(currentStretch);
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

                        if (isCurrentlyExtruding && definesMove >= 1)
                        {
                            if (!currentStretch.CentralPoints.Any())
                            {
                                currentStretch.CentralPoints.Add(lastLocation);
                            }

                            currentStretch.CentralPoints.Add(location);
                        }

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
            if (line.orig_string.Contains(Layup.FibrifierProducerId) && string.IsNullOrEmpty(layup.Producer))
            {
                layup.Producer = Layup.FibrifierProducerId;

                var startIndex = line.orig_string.IndexOf(Layup.FibrifierProducerId, StringComparison.Ordinal) + Layup.FibrifierProducerId.Length + 1;
                var endIndex = line.orig_string.IndexOf('-');
                layup.ProducerVersion = line.orig_string.Substring(startIndex, endIndex - startIndex);
            }
            else if (line.orig_string.Contains(Layup.BteId))
            {
                var startIndex = line.orig_string.IndexOf(':');

                layup.Bte = line.orig_string.Substring(startIndex + 2);
            }
        }


        /// <summary>
        ///     Generates Vertices and Faces for rendering purposes and Export to
        ///     HDF5cc and other target operations requiring Face data.
        /// </summary>
        /// <param name="layup"></param>
        /// <exception cref="InvalidOperationException"></exception>
        //private static void GenerateStretchFaces(Layup layup)
        //{
        //    Vector3D upDirection = new Vector3D(0, 0, 1);
        //    upDirection.Normalize();
        //    double width = .2;

        //    foreach (var layer in layup.Layers)
        //    {
        //        foreach (var stretch in layer.Stretches)
        //        {
        //            if (!stretch.CentralPoints.Any())
        //            {
        //                continue;
        //            }

        //            if (stretch.CentralPoints.Count < 2)
        //            {
        //                throw new InvalidOperationException("Cannot have less than 2 points in Stretch");
        //            }

        //            // special case for the first point, as it does not span a face yet
        //            Vector3D direction = stretch.CentralPoints[1] - stretch.CentralPoints[0];
        //            direction.Normalize();
        //            Vector3D sideways = Vector3D.CrossProduct(direction, upDirection);
        //            var startPoint1 = stretch.CentralPoints[0] + sideways * width;
        //            var startPoint2 = stretch.CentralPoints[0] - sideways * width;
        //            layup.Points.Add(startPoint1);
        //            layup.Points.Add(startPoint2);
        //            stretch.Points.Add(startPoint1);
        //            stretch.Points.Add(startPoint2);

        //            for (int i = 1; i < stretch.CentralPoints.Count; i++)
        //            {
        //                direction = stretch.CentralPoints[i] - stretch.CentralPoints[i - 1];
        //                if (stretch.CentralPoints.Count > i + 2)
        //                {
        //                    direction += stretch.CentralPoints[i + 1] - stretch.CentralPoints[i];
        //                }

        //                direction.Normalize();
        //                sideways = Vector3D.CrossProduct(direction, upDirection);

        //                var p1 = stretch.CentralPoints[i] + sideways * width;
        //                var p2 = stretch.CentralPoints[i] - sideways * width;
        //                layup.Points.Add(p1);
        //                layup.Points.Add(p2);
        //                stretch.Points.Add(p1);
        //                stretch.Points.Add(p2);

        //                if (stretch.Points.Count >= 4)
        //                {
        //                    Facet3D facet1 = new Facet3D()
        //                    {
        //                        Vertex0 = layup.Points.Count - 3,
        //                        Vertex1 = layup.Points.Count,
        //                        Vertex2 = layup.Points.Count - 1,
        //                    };
        //                    Facet3D facet2 = new Facet3D()
        //                    {
        //                        Vertex0 = layup.Points.Count - 2,
        //                        Vertex1 = layup.Points.Count,
        //                        Vertex2 = layup.Points.Count - 3,
        //                    };
        //                    // for all faces we add a direction as well
        //                    stretch.Facets.Add(facet1);
        //                    stretch.Directions.Add(direction);
        //                    stretch.Facets.Add(facet2);
        //                    stretch.Directions.Add(direction);
        //                }
        //            }
        //        }
        //    }
        //}
        public static bool AboutEqual(double x, double y)
        {
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }
    }
}
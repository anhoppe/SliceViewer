using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LayerSource.Contract;
using LayerSource.GCode;
using Logger.Contract;
using OpenGLTest.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using MessageBox = System.Windows.MessageBox;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using ScrollEventArgs = System.Windows.Controls.Primitives.ScrollEventArgs;

namespace OpenGLTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private static readonly float DistanceCameraToScene = 1;

        private static readonly float MaxZoom = 8;

        private static readonly float MinZoom = 0.1f;

        private static readonly float StepZoom = 0.1f;

        private readonly ILogger _logger;

        private Camera _camera;

        private Vector3 _cameraDragStartPosition;

        private ILayup _layup;

        private Vector3 _modelCenter;

        private Vector3 _mouseDownWorldPos;

        private int _numOfVertices;

        private Matrix4 _rotationMatrix = Matrix4.Identity;

        private bool _sceneTranslated = false;

        private Shader _shader;

        private IList<IStretch> _stretchesToRender;

        private Vector3 _viewCenterPosition;

        private ViewMode _viewMode = ViewMode.Layer;

        private float _xAngleStart;

        private float _xAxisRotationAngleDeg;

        private int _xMouseDownScreenPosition;

        private float _yAngleStart;

        private float _yAxisRotationAngleDeg;

        private int _yMouseDownScreenPosition;

        private byte[] _pixelArray = null;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _logger = new Logger.Logger();

            glControl.MouseDown += OnMouseDown;
            glControl.MouseUp += OnMouseUp;
            glControl.MouseMove += OnMouseMove;

            glControl.MouseWheel += OnMouseWheel;
            glControl.Resize += OnResize;
        }

        #endregion

        //public override void InitializeComponent()
        //{
        //    base.InitializeComponent();
        //}

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                _mouseDownWorldPos = ScreenToWorld(e);
                _cameraDragStartPosition = _camera.Position;

                _sceneTranslated = false;
            }
            else if (_viewMode == ViewMode.Model && (e.Button & MouseButtons.Right) != 0)
            {
                _xMouseDownScreenPosition = e.X;
                _yMouseDownScreenPosition = e.Y;

                _xAngleStart = _xAxisRotationAngleDeg;
                _yAngleStart = _yAxisRotationAngleDeg;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_pixelArray != null)
            {
                int result = -1;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        int b = _pixelArray[4 * e.X + (_camera.ScreenHeight - e.Y) * _camera.ScreenWidth * 4];
                        int g = _pixelArray[4 * e.X + (_camera.ScreenHeight - e.Y) * _camera.ScreenWidth * 4 + 1];
                        int r = _pixelArray[4 * e.X + (_camera.ScreenHeight - e.Y) * _camera.ScreenWidth * 4 + 2];

                        if (r != 255 || g != 255 || b != 255)
                        {
                            result = r + (g << 8) + (b << 16);

                            textTest.Text = $"RGB = {r}, {g}, {b} ({result})";
                            break;
                        }
                    }

                    if (result != -1)
                    {
                        break;
                    }
                }
            }

            if ((e.Button & MouseButtons.Left) != 0)
            {
                var currentMousePosWorld = ScreenToWorld(e);

                var pos = currentMousePosWorld - _mouseDownWorldPos;

                _camera.Position = new Vector3(_cameraDragStartPosition.X - pos.X,
                    _cameraDragStartPosition.Y + pos.Y,
                    _cameraDragStartPosition.Z - pos.Z);

                _sceneTranslated = true;

                glControl.Invalidate();
            }
            else if (_viewMode == ViewMode.Model && (e.Button & MouseButtons.Right) != 0)
            {
                var xDist = _xMouseDownScreenPosition - e.X;
                var yDist = _yMouseDownScreenPosition - e.Y;

                _xAxisRotationAngleDeg = _xAngleStart - yDist;
                _yAxisRotationAngleDeg = _yAngleStart - xDist;

                RotateCamera();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (_sceneTranslated)
                {
                    _viewCenterPosition = _camera.Position + _camera.Front;
                }
                else
                {
                    // Picking 
                    // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-14-render-to-texture/
                    // https://stackoverflow.com/questions/57980264/whats-the-simplest-framebuffer-example-for-opengl

                    InitVertexBuffers(false);

                    int frameBuffer = GL.GenFramebuffer();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

                    int texture = GL.GenTexture();

                    // "Bind" the newly created texture : all future texture functions will modify this texture
                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    // Give an empty image to OpenGL ( the last "0" )
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _camera.ScreenWidth, _camera.ScreenHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

                    DrawBuffersEnum[] drawBuffers = new []{ DrawBuffersEnum.ColorAttachment0 };
                    GL.DrawBuffers(1, drawBuffers);

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
                    GL.Viewport(0, 0, _camera.ScreenWidth, _camera.ScreenHeight);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    var model = Matrix4.Identity;

                    _shader.SetMatrix4("model", model);
                    _shader.SetMatrix4("view", _camera.GetViewMatrix());
                    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

                    if (_numOfVertices != 0)
                    {
                        GL.DrawArrays(PrimitiveType.Lines, 0, _numOfVertices);
                    }

                    _pixelArray = new byte[4 * _camera.ScreenWidth * _camera.ScreenHeight];
                    GL.ReadPixels(0, 0, _camera.ScreenWidth, _camera.ScreenHeight, PixelFormat.Bgra,
                        PixelType.UnsignedByte, _pixelArray);

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


                    //int result = -1;
                    //for (int x = 0; x < 3; x++)
                    //{
                    //    for (int y = 0; y < 3; y++)
                    //    {
                    //        int r = _pixelArray[4 * e.X + (x - 1) + (e.Y + y - 1) * _camera.ScreenWidth * 4];
                    //        int g = _pixelArray[4 * e.X + (x - 1) + (e.Y + y - 1) * _camera.ScreenWidth * 4 + 1];
                    //        int b = _pixelArray[4 * e.X + (x - 1) + (e.Y + y - 1) * _camera.ScreenWidth * 4 + 2];

                    //        var value = r + (g << 8) + (b << 16);
                    //        if (value != 255 + (255 << 8) + (255 << 16))
                    //        {
                    //            result = value;
                    //            textTest.Text = $"RGB = {r}, {g}, {b}";
                    //            break;
                    //        }
                    //    }

                    //    if (result != -1)
                    //    {
                    //        break;
                    //    }
                    //}

                    for (int i = 0; i < _camera.ScreenWidth * _camera.ScreenHeight; i++)
                    {
                        int r = _pixelArray[4 * i];
                        int g = _pixelArray[4 * i + 1];
                        int b = _pixelArray[4 * i + 2];
                        int a = _pixelArray[4 * i + 3];

                        if (a != 255 && a != 0)
                        {
                            int tol = 0;
                        }

                        if (r != 255)
                        {
                            int tol = 0;
                        }
                    }

                    //for (int x = 0; x < _camera.ScreenWidth; x++)
                    //{
                    //    for (int y = 0; y < _camera.ScreenHeight; y++)
                    //    {
                    //        r = _pixelArray[4 * x + (/*_camera.ScreenHeight - */y) * _camera.ScreenWidth * 4];
                    //        g = _pixelArray[4 * x + (/*_camera.ScreenHeight - */y) * _camera.ScreenWidth * 4 + 1];
                    //        b = _pixelArray[4 * x + (/*_camera.ScreenHeight - */y) * _camera.ScreenWidth * 4 + 2];

                    //        if (r != 255)
                    //        {
                    //            int tol = 0;
                    //        }
                    //    }
                    //}

                    Bitmap bmp = new Bitmap(_camera.ScreenWidth, _camera.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                    //Create a BitmapData and Lock all pixels to be written 
                    BitmapData bmpData = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.WriteOnly, bmp.PixelFormat);

                    //Copy the data from the byte array into BitmapData.Scan0
                    Marshal.Copy(_pixelArray, 0, bmpData.Scan0, _pixelArray.Length);


                    //Unlock the pixels
                    bmp.UnlockBits(bmpData);

                    bmp.Save("c:\\temp\\mybmp.bmp", ImageFormat.Bmp);

                    InitVertexBuffers(true);

                }
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _camera.Zoom = Math.Min(MaxZoom, _camera.Zoom + StepZoom);
            }

            else if (e.Delta < 0)
            {
                _camera.Zoom = Math.Max(MinZoom, _camera.Zoom - StepZoom);
            }

            glControl.Invalidate();
        }

        //private void RotateCamera()
        //{
        //    var xAngle = (float)(_xAxisRotationAngleDeg / 180f * Math.PI);
        //    var yAngle = (float)(_yAxisRotationAngleDeg / 180f * Math.PI);

        //    var rotationX = Matrix4.CreateRotationX(xAngle);
        //    var rotationY = Matrix4.CreateRotationY(yAngle);
        //    _rotationMatrix = rotationY * rotationX;

        //    var vector = Vector4.Transform(Vector4.UnitZ, _rotationMatrix).Xyz;

        //    var nextCameraPosition = _viewCenterPosition + vector * DistanceCameraToScene;

        //    var nextCameraFront = vector * -1;

        //    _camera.Position = nextCameraPosition;
        //    _camera.Front = nextCameraFront;

        //    glControl.Invalidate();
        //}

        private void RotateCamera()
        {
            var xAngle = (float) (_xAxisRotationAngleDeg / 180f * Math.PI);
            var yAngle = (float) (_yAxisRotationAngleDeg / 180f * Math.PI);

            var quaternion = Quaternion.FromEulerAngles(xAngle, yAngle, 0);
            _rotationMatrix = Matrix4.CreateFromQuaternion(quaternion);

            var vector = Vector4.Transform(Vector4.UnitZ, _rotationMatrix).Xyz;

            var nextCameraPosition = _viewCenterPosition + vector * DistanceCameraToScene;

            var nextCameraFront = vector * -1;

            _camera.Position = nextCameraPosition;
            _camera.Front = nextCameraFront;

            glControl.Invalidate();
        }

        private Vector3 ScreenToWorld(MouseEventArgs e)
        {
            var inverseProjectionMatrix = Matrix4.Invert(_camera.GetProjectionMatrix());
            var inverseViewMatrix = Matrix4.Invert(_camera.GetViewMatrix());

            var rayNds = ToWorldCoor(e.X, e.Y);

            return (inverseProjectionMatrix * inverseViewMatrix * rayNds).Xyz;
        }

        private Vector4 ToWorldCoor(float x, float y)
        {
            return new Vector4(x / (glControl.Width / 2f) - 1f,
                y / (glControl.Height / 2f) - 1f, 1, 1);
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();

            ResetCamera();

            _shader = new Shader(".Shader.shader.vert", ".Shader.shader.frag");

            _shader.Use();

            GL.Viewport(0, 0, (int) Width, (int) Height);

            GL.ClearColor(new Color4(1f, 1f, 1f, 1f));
        }

        private void ResetCamera()
        {
            _xAxisRotationAngleDeg = 0;
            _yAxisRotationAngleDeg = 0;
            _viewCenterPosition = _modelCenter;
            _camera = new Camera(new Vector3(_modelCenter.X, _modelCenter.Y, _modelCenter.Z + DistanceCameraToScene),
                1f);
            _camera.ScreenWidth = glControl.Width;
            _camera.ScreenHeight = glControl.Height;
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var model = Matrix4.Identity;

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            if (_numOfVertices != 0)
            {
                GL.DrawArrays(PrimitiveType.Lines, 0, _numOfVertices);
            }

            glControl.SwapBuffers();
        }

        private void InitVertexBuffers(bool prepareScreenRendering)
        {
            var coordinates = new List<float>();
            var colors = new List<float>();

            float[] bounds =
                {float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue};


            var count = 0;
            foreach (var stretch in _stretchesToRender)
            {
                if (stretch.DepositionPoints.Count == 0)
                {
                    continue;
                }

                var prevPoint = stretch.DepositionPoints[0];

                bounds[0] = (float) Math.Min(bounds[0], prevPoint.X / 200f);
                bounds[1] = (float) Math.Max(bounds[1], prevPoint.X / 200f);
                bounds[2] = (float) Math.Min(bounds[2], prevPoint.Y / 200f);
                bounds[3] = (float) Math.Max(bounds[3], prevPoint.Y / 200f);
                bounds[4] = (float) Math.Min(bounds[4], prevPoint.Z / 200f);
                bounds[5] = (float) Math.Max(bounds[5], prevPoint.Z / 200f);

                float r = 0;
                float g = 0;
                float b = 0;

                if (prepareScreenRendering)
                {
                    if (stretch.PrintType == PrintType.Fiber)
                    {
                        r = 0.945f;
                        g = 0.745f;
                        b = 0.356f;
                    }
                    else
                    {
                        r = 1f;
                    }
                }
                else
                {
                    b = 0.01f * count;
                }

                foreach (var currentPoint in stretch.DepositionPoints.Skip(1))
                {
                    bounds[0] = (float) Math.Min(bounds[0], currentPoint.X / 200f);
                    bounds[1] = (float) Math.Max(bounds[1], currentPoint.X / 200f);
                    bounds[2] = (float) Math.Min(bounds[2], currentPoint.Y / 200f);
                    bounds[3] = (float) Math.Max(bounds[3], currentPoint.Y / 200f);
                    bounds[4] = (float) Math.Min(bounds[4], currentPoint.Z / 200f);
                    bounds[5] = (float) Math.Max(bounds[5], currentPoint.Z / 200f);

                    coordinates.Add((float) prevPoint.X / 200f);
                    coordinates.Add((float) prevPoint.Y / 200f);
                    coordinates.Add((float) prevPoint.Z / 200f);

                    coordinates.Add((float) currentPoint.X / 200f);
                    coordinates.Add((float) currentPoint.Y / 200f);
                    coordinates.Add((float) currentPoint.Z / 200f);

                    if (prepareScreenRendering)
                    {
                        colors.Add(r);
                        colors.Add(g);
                        colors.Add(b);

                        colors.Add(r);
                        colors.Add(g);
                        colors.Add(b);
                    }
                    else
                    {
                        if (count > (Math.Pow(2, 24) - 1))
                        {
                            throw new InvalidOperationException("Only 2^24 vectors can be encoded for picking");
                        }

                        var countAsByteArray = BitConverter.GetBytes(count);

                        var red = (count & 0x000000FF);
                        var green = (count & 0x0000FF00) >> 8;
                        var blue = (count & 0x00FF0000) >> 16;

                        r = red / 255f;
                        g = green / 255f;
                        b = blue / 255f;

                        colors.Add(r);
                        colors.Add(g);
                        colors.Add(b);

                        colors.Add(r);
                        colors.Add(g);
                        colors.Add(b);
                    }

                    prevPoint = currentPoint;

                    count++;

                    if (count >= (int) scrollStretches.Value)
                    {
                        break;
                    }
                }

                if (count >= (int) scrollStretches.Value)
                {
                    break;
                }
            }

            _modelCenter = new Vector3(bounds[0] + (bounds[1] - bounds[0]) / 2,
                bounds[2] + (bounds[3] - bounds[2]) / 2,
                bounds[4] + (bounds[5] - bounds[4]) / 2);

            var n = coordinates.Count / 2;


            int vertexVbo;
            GL.GenBuffers(1, out vertexVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, coordinates.Count * sizeof(float), coordinates.ToArray(),
                BufferUsageHint.StaticDraw);

            int colorVbo;
            GL.GenBuffers(1, out colorVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Count * sizeof(float), colors.ToArray(),
                BufferUsageHint.StaticDraw);

            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            _numOfVertices = n;
        }

        private IList<IStretch> GetStretchesToRender()
        {
            if (_viewMode == ViewMode.Model)
            {
                return _layup.ZChunks.SelectMany(p => p.Stretches).ToList();
            }

            if (_viewMode == ViewMode.Layer)
            {
                //return _layup.ZChunks[(int) scrollLayers.Value].Stretches;
            }

            throw new InvalidOperationException("ViewMode not set");
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "GCode files (*.gcode)|*.gcode";
            if (fd.ShowDialog(null) == System.Windows.Forms.DialogResult.OK)
            {
                using (var fileStream = File.OpenRead(fd.FileName))
                {
                    var streamReader = new StreamReader(fileStream);
                    try
                    {
                        var reader = new GCodeReaderTools(_logger);

                        _layup = reader.ReadLayup(streamReader);

                        textLayerCount.Text = _layup.ZChunks.Count.ToString();

                        fileStream.Seek(0, SeekOrigin.Begin);
                        streamReader.DiscardBufferedData();

                        //editorGCode.Text = streamReader.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open selected file: {ex.Message}", "Error");
                        return;
                    }
                }

                using (var streamReader = File.OpenText(fd.FileName))
                {
                }

                textProducer.Text = _layup.Producer;
                textVersion.Text = _layup.ProducerVersion;
                textBte.Text = _layup.Bte;

                scrollLayers.Minimum = 0;
                scrollLayers.Maximum = _layup.ZChunks.Count - 1;
                scrollLayers.Value = _layup.ZChunks.Count - 1;

                _stretchesToRender = GetStretchesToRender();
                var elementsToRender = _stretchesToRender.Sum(p => p.DepositionPoints.Count);

                scrollStretches.Minimum = 0;
                scrollStretches.Maximum = elementsToRender - 1;
                scrollStretches.Value = elementsToRender - 1;

                InitVertexBuffers(true);

                ResetCamera();
                glControl.Invalidate();
            }
        }

        private void OnResize(object sender, EventArgs args)
        {
            ResetCamera();
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            _camera.ScreenWidth = glControl.Width;
            _camera.ScreenHeight = glControl.Height;

            glControl.Invalidate();
        }

        private void OnLayerScroll(object sender, ScrollEventArgs e)
        {
            _stretchesToRender = GetStretchesToRender();
            var elementsToRender = _stretchesToRender.Sum(p => p.DepositionPoints.Count);

            scrollStretches.Minimum = 0;
            scrollStretches.Maximum = elementsToRender - 1;
            scrollStretches.Value = elementsToRender - 1;

            var resetCamera = _numOfVertices == 0;

            InitVertexBuffers(true);

            if (resetCamera)
            {
                ResetCamera();
            }

            glControl.Invalidate();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ResetCamera();
            glControl.Invalidate();
        }

        private void OnButtonLayerView(object sender, RoutedEventArgs e)
        {
            SetViewMode(ViewMode.Layer);
            InitVertexBuffers(true);
            ResetCamera();
        }

        private void OnButtonModelView(object sender, RoutedEventArgs e)
        {
            SetViewMode(ViewMode.Model);
            InitVertexBuffers(true);
            ResetCamera();
        }

        private void SetViewMode(ViewMode viewMode)
        {
            _viewMode = viewMode;
            scrollLayers.Visibility = viewMode == ViewMode.Layer ? Visibility.Visible : Visibility.Collapsed;
            _stretchesToRender = GetStretchesToRender();
            var elementsToRender = _stretchesToRender.Sum(p => p.DepositionPoints.Count);

            scrollStretches.Minimum = 0;
            scrollStretches.Maximum = elementsToRender - 1;
            scrollStretches.Value = elementsToRender - 1;

            ResetCamera();
        }

        private void OnXAxisTurn(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _xAxisRotationAngleDeg = (float) e.NewValue;
            RotateCamera();
        }

        private void OnYAxisTurn(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _yAxisRotationAngleDeg = (float) e.NewValue;
            RotateCamera();
        }

        private void OnStretchesScroll(object sender, ScrollEventArgs e)
        {
            InitVertexBuffers(true);

            glControl.Invalidate();
        }
    }
}

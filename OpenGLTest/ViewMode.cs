namespace OpenGLTest
{
    internal enum ViewMode
    {
        None,

        // Only a single layer is visible, it is possible to navigate through the layers
        Layer,

        // The entire model is visible, possible to rotate around the model (3D view)
        Model
    }
}
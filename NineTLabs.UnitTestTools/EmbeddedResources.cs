using System;
using System.IO;
using System.Reflection;

namespace UnitTestTools
{
    /// <summary>
    /// Loads embedded resources from files.
    /// For integration or system tests embed files of any type (e.g. GCode files)
    /// into the test assembly, typically into an 'assets' folder.
    /// In the properties of each file select
    /// 'Build Action=Embedded Resource' and 'Action=Copy Always'
    /// </summary>
    public static class EmbeddedResources
    {
        /// <summary>
        /// Access to an embedded resource file as StreamReader
        /// Code taken from: https://gist.github.com/kristopherjohnson/3229248
        ///
        /// The resourceName must be given qualified relative to the namespace. If e.g. a resource file is located in an
        /// 'assets' sub-directory of an assembly the resourceName must be '.assets.filename.extension'
        /// </summary>
        /// <param name="resourceName">Name of the resource as full path relative to the namespace.</param>
        /// <returns>Open StreamReader to the referenced resource. StreamReader must be disposed after usage.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException when <c>>resourceName</c> could not be found</exception>
        public static StreamReader GetAsStreamReader(string resourceName)
        {
            var thisAssembly = Assembly.GetCallingAssembly();

            var name = thisAssembly.GetName().Name;
            var stream = thisAssembly.GetManifestResourceStream(name + resourceName);

            if (stream == null)
            {
                throw new ArgumentException($"Embedded resource {resourceName} not found in {Assembly.GetCallingAssembly().FullName}");
            }

            return new StreamReader(stream);
        }
    }
}
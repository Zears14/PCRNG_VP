using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PCRNG_VP.exTPM.Helpers
{
    /// <summary>
    /// Provides methods to retrieve the GUID of the current application.
    /// </summary>
    public static class AppGuidRetrieval
    {
        /// <summary>
        /// Retrieves the GUID of the current application.
        /// </summary>
        /// <returns>The GUID of the current application.</returns>
        /// <exception cref="NotImplementedException">Thrown if the assembly GUID is not found.</exception>
        public static string GetAppGuid()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string Guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            return Guid;
        }
    }
}
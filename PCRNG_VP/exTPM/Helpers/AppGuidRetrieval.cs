using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PCRNG_VP.exTPM.Helpers
{
    public static class AppGuidRetrieval
    {
        public static string GetAppGuid()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string Guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            return Guid;
        }
    }
}

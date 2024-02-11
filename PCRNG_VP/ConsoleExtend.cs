using Console = Colorful.Console;
using System.Drawing;

namespace PCRNG_VP
{
    public static class ConsoleExtend
    {
        /// <summary>
        /// Quickly do "Press any key to exit".
        /// </summary>
        public static void AnyKeyToExit()
        {
            Console.WriteLine("Press any key to exit...", Color.White);
            Console.ReadKey();
        }
    }
}

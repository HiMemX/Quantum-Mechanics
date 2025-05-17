using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumMechanics
{
    internal static class DebugInterface
    {
        public static Action<string> WriteToLogCallback;

        public static void WriteLine(string line)
        {
            WriteToLogCallback(line + "\r\n");
        }
    }
}

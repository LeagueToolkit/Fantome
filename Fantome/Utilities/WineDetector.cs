using System;
using System.Runtime.InteropServices;

namespace Fantome.Utilities
{
    public static class WineDetector
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        public static bool IsRunningInWine()
        {
            IntPtr ntdllHandle = GetModuleHandle("ntdll.dll");

            if (ntdllHandle != IntPtr.Zero)
            {
                IntPtr wineVersion = GetProcAddress(ntdllHandle, "wine_get_version");

                return wineVersion != IntPtr.Zero;
            }

            return false;
        }
    }
}
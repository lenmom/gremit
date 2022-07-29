using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmToBytes
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: AsmToBytes <x86|x64> <pathToFasm> <inputFile> <outputFile>");
                return;
            }
            string architecture = args[0];
            string pathToFasm = args[1];
            string inputFile = args[2];
            string outputFile = args[3];
            string dirName = Path.GetDirectoryName(inputFile) ?? "";
            string tempFile = Path.Combine(dirName, "temp.asm");
            string dllFile = Path.Combine(dirName, "temp.dll");
            byte[] marker = Guid.NewGuid().ToByteArray();
            string stringMarker = string.Join(Environment.NewLine, marker.Select(x => $"db 0{x:x2}h"));
            string content;
            if (architecture == "x86")
            {
                content = string.Format(@"
format PE GUI 4.0 DLL
entry DllEntryPoint

include '{0}'

section '.text' code readable executable

proc DllEntryPoint hinstDLL, fdwReason, lpvReserved
	mov	eax, TRUE
	ret
endp

{1}
Main:
{2}
{1}

data fixups
end data

section '.edata' export data readable

export 'TEMP.DLL',\
	 Main,'Main'
", Path.Combine(pathToFasm, "include", "win32a.inc"), stringMarker, File.ReadAllText(inputFile));
            }
            else if (architecture == "x64")
            {
                content = string.Format(@"
format PE64 GUI 5.0 DLL
entry DllEntryPoint

include '{0}'

section '.text' code readable executable

proc DllEntryPoint hinstDLL, fdwReason, lpvReserved
	mov	eax, TRUE
	ret
endp

{1}
Main:
{2}
{1}

data fixups
end data

section '.edata' export data readable

export 'TEMP.DLL',\
	 Main,'Main'
", Path.Combine(pathToFasm, "include", "win64a.inc"), stringMarker, File.ReadAllText(inputFile));
            }
            else
            {
                Console.WriteLine("Architecture must be either 'x86' or 'x64'");
                return;
            }
            File.WriteAllText(tempFile, content);
            File.Delete(dllFile);
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                Arguments = $@"""{tempFile}"" ""{dllFile}""",
                FileName = Path.Combine(pathToFasm, "fasm.exe")
            };
            Process process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            if (!File.Exists(dllFile))
            {
                Console.WriteLine("ERROR: dll file has not been created");
                return;
            }
            byte[] dllContent = File.ReadAllBytes(dllFile);
            for (int i = 0; i < dllContent.Length - marker.Length; ++i)
            {
                bool ok = true;
                for (int j = 0; j < marker.Length; ++j)
                {
                    if (dllContent[i + j] != marker[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if (!ok)
                {
                    continue;
                }

                i += marker.Length;
                for (int k = i; k < dllContent.Length - marker.Length; ++k)
                {
                    ok = true;
                    for (int j = 0; j < marker.Length; ++j)
                    {
                        if (dllContent[k + j] != marker[j])
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                    {
                        StringBuilder result = new StringBuilder();
                        for (int j = i; j < k; ++j)
                        {
                            result.Append($"0x{dllContent[j]:x2},");
                        }

                        result.AppendLine();
                        result.AppendLine(Convert.ToBase64String(dllContent, i, k - i));
                        File.WriteAllText(outputFile, result.ToString());
                        Console.WriteLine("SUCCESS");
                        return;
                    }
                }
                Console.WriteLine("ERROR: Cannot find the second marker");
                return;
            }
            Console.WriteLine("ERROR: Cannot find the first marker");
        }
    }
}
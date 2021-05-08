using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Diz.Core.util;
using DiztinGUIsh.window.dialog;

namespace DiztinGUIsh.util
{
    public class LargeFilesReader : IDizTask
    {
        public IEnumerable<string> Filenames { get; init; }
        public Action<string> LineReadCallback { get; init; }

        private long sumFileLengthsInBytes;
        private long bytesReadFromPreviousFiles;
        private int lastReportedProgress;
        
        public IProgress<ProgressReport> Progress { get; set; }

        public async Task Run()
        {
            Init();
            foreach (var filename in Filenames) 
                await ReadFile(filename);
        }

        private void Init()
        {
            sumFileLengthsInBytes = 0L;
            foreach (var filename in Filenames) 
                sumFileLengthsInBytes += Util.GetFileSizeInBytes(filename);

            bytesReadFromPreviousFiles = 0L;
        }

        private async Task ReadFile(string filename)
        {
            await using var fs = File.Open(filename, FileMode.Open, FileAccess.Read);
            await using var bs = new BufferedStream(fs);
            using var sr = new StreamReader(bs);
            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                LineReadCallback(line);
                UpdateProgress(fs.Position);
            }

            bytesReadFromPreviousFiles += fs.Length;
        }

        private void UpdateProgress(long currentPositionInBytes) => 
            Progress?.ReportIfNeeded(GetCurrentProgressPercent(currentPositionInBytes), ref lastReportedProgress);
        
        private float GetCurrentProgressPercent(long currentPositionInBytes) => 
            (bytesReadFromPreviousFiles + currentPositionInBytes) / (float)sumFileLengthsInBytes;

        public static void ReadFilesLines(IEnumerable<string> filenames, Action<string> lineReadCallback)
        {
            var largeFilesReader = new LargeFilesReader
            {
                Filenames = filenames,
                LineReadCallback = lineReadCallback,
            };

            largeFilesReader.RunWithGui();
        }
    }
}

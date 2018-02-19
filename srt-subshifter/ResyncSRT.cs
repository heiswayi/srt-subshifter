using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace srt_subshifter
{
    internal class ResyncSrt
    {
        private readonly Regex useRegex;

        public string RegexToUse { get; set; }
        public bool IsComplete { get; private set; }

        public ResyncSrt()
        {
            RegexToUse = @"(\d+:\d+:\d+,\d+)\s+--\>\s+(\d+:\d+:\d+,\d+)";
            useRegex = new Regex(RegexToUse);
            IsComplete = false;
        }

        public void GetResyncedSRT(string fileDir, string filename, double offset, bool isOverwrite)
        {
            if (File.Exists(Path.Combine(fileDir, filename)) && filename.IndexOf(".srt", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                var outputLines = new List<string>();
                string[] lines = File.ReadAllLines(Path.Combine(fileDir, filename), Encoding.UTF8);
                foreach (var line in lines)
                {
                    var match = useRegex.Match(line);
                    if (match.Success)
                    {
                        outputLines.Add(timeShiftAdjustment(offset, line));
                    }
                    else
                    {
                        outputLines.Add(line);
                    }
                }

                if (isOverwrite)
                {
                    var output = Path.Combine(fileDir, filename);
                    File.WriteAllLines(output, outputLines.ToArray(), Encoding.UTF8);
                }
                else
                {
                    var output = Path.Combine(fileDir, Path.GetFileNameWithoutExtension(filename) + "-resync.srt");
                    File.WriteAllLines(output, outputLines.ToArray(), Encoding.UTF8);
                }

                IsComplete = true;
            }
        }

        private string timeShiftAdjustment(double offset, string timeString)
        {
            string[] timeRanges = timeString.Split(new string[] { "-->" }, StringSplitOptions.None);
            string[] startTime = timeRanges[0].Split(',');
            string[] endTime = timeRanges[1].Split(',');
            var first = TimeSpan.Parse(startTime[0].Trim());
            var newFirst = first.Add(new TimeSpan(0, 0, (int)offset));
            var second = TimeSpan.Parse(endTime[0].Trim());
            var newSecond = second.Add(new TimeSpan(0, 0, (int)offset));
            var newFirstString = string.Format("{0:D2}:{1:D2}:{2:D2}", newFirst.Hours, newFirst.Minutes, newFirst.Seconds);
            var newSecondString = string.Format("{0:D2}:{1:D2}:{2:D2}", newSecond.Hours, newSecond.Minutes, newSecond.Seconds);
            var adjusted = newFirstString + "," + startTime[1].Trim() + " --> " + newSecondString + "," + endTime[1].Trim();
            return adjusted;
        }
    }
}
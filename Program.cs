using System;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace GenSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ComputeIndex());
        }

        private static int ComputeIndex()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(GenerateMetricsForProject());
            var metricNode = xmlDoc.SelectSingleNode($"//Method[contains(@Name, '{new MISettings().TargetMethodName}')]/Metrics/Metric[@Name = 'MaintainabilityIndex']");
            var miValue = metricNode.Attributes["Value"].Value;
            if (!int.TryParse(miValue, out int mi))
            {
                throw new ArgumentException("Can't parse value returned from ms metrics");
            }

            return mi;
        }

        private static string GenerateMetricsForProject()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = new MISettings().MetricsExecutablePath,
                    Arguments = $"/project:{new MISettings().TargetProjectPath} /quiet",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };
            var xmlBuilder = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                xmlBuilder.AppendLine(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return xmlBuilder.ToString();
        }
    }
}

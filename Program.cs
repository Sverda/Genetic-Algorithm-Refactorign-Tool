using System.Diagnostics;
using System.Xml;

namespace GenSharp
{
    class Program
    {
        static void Main(string[] args)
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
            process.OutputDataReceived += (sender, e) =>
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(e.Data);
                var metricNode = xmlDoc.SelectSingleNode($"number(//Method[contains(@Name, '{new MISettings().TargetMethodName}')]/Metrics/Metric[@Name = 'MaintainabilityIndex'])");
                var miValue = metricNode.Attributes["Value"].Value;
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
    }
}

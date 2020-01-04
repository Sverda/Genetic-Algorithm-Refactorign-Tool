using GenSharp.Metrics.Abstractions;
using GenSharp.Metrics.Helpers;
using System;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace GenSharp.Metrics.Implementations
{
    public class MaintainabilityIndexMetrics : IEvaluateMetric
    {
        private readonly MaintainabilityIndexSettings _settings;

        public MaintainabilityIndexMetrics()
        {
            _settings = new MaintainabilityIndexSettings();
        }

        public double Evaluate()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(GenerateMetricsForProject());
            var metricNode = xmlDoc.SelectSingleNode($"//Method[contains(@Name, '{_settings.TargetMethodName}')]/Metrics/Metric[@Name = 'MaintainabilityIndex']");
            var miValue = metricNode.Attributes["Value"].Value;
            if (!double.TryParse(miValue, out var mi))
            {
                throw new ArgumentException("Can't parse value returned from ms metrics");
            }

            return mi;
        }

        private string GenerateMetricsForProject()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _settings.MetricsExecutablePath,
                    Arguments = $"/project:{_settings.TargetProjectPath} /quiet",
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

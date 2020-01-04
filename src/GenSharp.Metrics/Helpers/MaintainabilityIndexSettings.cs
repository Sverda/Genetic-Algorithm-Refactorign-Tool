using System;
using System.Configuration;

namespace GenSharp.Metrics.Helpers
{
    internal class MaintainabilityIndexSettings
    {
        private const string _miKey = "MaintainabilityIndexToolPath";
        private const string _csprojPath = "csprojPath";
        private const string _targetMethodName = "targetMethodName";

        public string MetricsExecutablePath { get; }

        public string TargetProjectPath { get; }

        public string TargetMethodName { get; }

        public MaintainabilityIndexSettings()
        {
            MetricsExecutablePath = ConfigurationManager.AppSettings[_miKey];
            if (string.IsNullOrEmpty(MetricsExecutablePath))
            {
                throw new ArgumentNullException(nameof(MetricsExecutablePath));
            }

            TargetProjectPath = ConfigurationManager.AppSettings[_csprojPath];
            if (string.IsNullOrEmpty(TargetProjectPath))
            {
                throw new ArgumentNullException(nameof(TargetProjectPath));
            }

            TargetMethodName = ConfigurationManager.AppSettings[_targetMethodName];
            if (string.IsNullOrEmpty(TargetMethodName))
            {
                throw new ArgumentNullException(nameof(TargetMethodName));
            }
        }
    }
}

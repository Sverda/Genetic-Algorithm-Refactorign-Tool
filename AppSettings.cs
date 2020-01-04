using System;
using System.Configuration;

namespace GenSharp
{
    class AppSettings
    {
        private const string _miKey = "MaintainabilityIndexToolPath";

        public string MetricsExecutablePath { get; }

        public AppSettings()
        {
            MetricsExecutablePath = ConfigurationManager.AppSettings[_miKey];
            if (string.IsNullOrEmpty(MetricsExecutablePath))
            {
                throw new ArgumentNullException(nameof(MetricsExecutablePath));
            }
        }
    }
}

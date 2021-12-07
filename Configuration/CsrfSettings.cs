namespace TestAsyncLog
{
    using System;
    using System.Configuration;
    using System.Reflection;

    /// <summary>
    /// Configuration settings for the CSRF module.
    /// </summary>
    public sealed class CsrfSettings : ConfigurationSection
    {
        //==========================

        /// <summary>
        /// Log Assembly Name : logAssemblyName.
        /// </summary>
        private const string sLogAssemblyNameConfigurationKey = "logAssemblyName";

        /// <summary>
        /// The log level (ALL, DEBUG, INFO, WARN, ERROR, FATAL)
        /// </summary>
        private const string sLogLevelConfigurationKey = "log_level";

        /// <summary>
        /// The log file name and location
        /// </summary>
        private const string sLogFileConfigurationKey = "logFile";

        /// <summary>
        /// The maximum size of current log file
        /// </summary>
        private const string sMaxSizeLogFileConfigurationKey = "maxSizeLogFile";

        //==========================

        /// <summary>
        /// Log Assembly Name.
        /// </summary>
        // TODO: AntiCsrf - La valeur initiale du paramètre sLogAssemblyName ne récupère pas comme attendu le nom de l'assembly appelante
        private static string sLogAssemblyNameInitialValue = Assembly.GetCallingAssembly().FullName;
        private const string sLogAssemblyNameDefaultValue = "<CurrentCallingAssemblyName>";

        /// <summary>
        /// The log level (ALL, DEBUG, INFO, WARN, ERROR, FATAL)
        /// </summary>
        private const string sLogLevelDefaultValue = LogCategory.ALL;

        /// <summary>
        /// The log file name and location
        /// </summary>
        private const string sLogFileDefaultValue = "%TempPath%csNETpWASPUTL_ficWASP777_ModuleXSRF.log.tsv";

        /// <summary>
        /// The maximum size of current log file (in bytes)
        /// </summary>
        private const long lMaxSizeLogFileDefaultValue = 10485760; //10 Mo = 10485760 octets = 10 * 1024 * 1024 = 10240 Ko

        //==========================

        /// <summary>
        /// The CSRF settings.
        /// </summary>
        private static CsrfSettings settings =
            ConfigurationManager.GetSection("test/csrfSettings") as CsrfSettings;


        /// <summary>
        /// Gets the CSRF Settings.
        /// </summary>
        /// <value>The CSRF Settings.</value>
        public static CsrfSettings Settings
        {
            get
            {
                // If the configuration setting is not present create one with the default values.
                if (settings == null)
                {
                    settings = new CsrfSettings
                                   {
                                        LogAssemblyName = sLogAssemblyNameInitialValue,
                                        Log_Level = sLogLevelDefaultValue,
                                        LogFile = sLogFileDefaultValue,
                                        MaxSizeLogFile = lMaxSizeLogFileDefaultValue
                    };
                }

                return settings;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the assembly name used in log file
        /// </summary>
        /// <value>Name of the assembly or any usefull tag</value>
        [ConfigurationProperty(sLogAssemblyNameConfigurationKey, DefaultValue = sLogAssemblyNameDefaultValue)]
        public string LogAssemblyName
        {
            get
            {
                string returnValue = (string)base[sLogAssemblyNameConfigurationKey];
                if (returnValue == sLogAssemblyNameDefaultValue)
                {
                    returnValue = sLogAssemblyNameInitialValue;
                }
                return returnValue;
            }

            set
            {
                base[sLogAssemblyNameConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the log level (ALL, DEBUG, INFO, WARN, ERROR, FATAL)
        /// </summary>
        /// <value>The list of log level (ALL, DEBUG, INFO, WARN, ERROR, FATAL)</value>
        [ConfigurationProperty(sLogLevelConfigurationKey, DefaultValue = sLogLevelDefaultValue)]
        public string Log_Level
        {
            get
            {
                return (string)base[sLogLevelConfigurationKey];
            }

            set
            {
                base[sLogLevelConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the log file name and location
        /// </summary>
        /// <value>The log file name and location</value>
        [ConfigurationProperty(sLogFileConfigurationKey, DefaultValue = sLogFileDefaultValue)]
        public string LogFile
        {
            get
            {
                return (string)base[sLogFileConfigurationKey];
            }

            set
            {
                base[sLogFileConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum size of current log file
        /// </summary>
        /// <value>The maximum size of current log file</value>
        [ConfigurationProperty(sMaxSizeLogFileConfigurationKey, DefaultValue = lMaxSizeLogFileDefaultValue)]
        public long MaxSizeLogFile
        {
            get
            {
                return (long)base[sMaxSizeLogFileConfigurationKey];
            }

            set
            {
                base[sMaxSizeLogFileConfigurationKey] = value;
            }
        }
    }
}

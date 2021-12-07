// <author>Samuel Villeneuve</author>
// <date>2021-07-28</date>
// <Creation auteur = 'svilleneuve' date='28/07/21'/>
// <summary>Configuration settings for the CSRF module.</summary>

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
        /// The configuration attribute for the cookie 1 name (HTTP Only = TRUE).
        /// </summary>
        private const string sCookie1NameConfigurationKey = "cookie1Name";

        /// <summary>
        /// The configuration attribute for the cookie 2 name (HTTP Only = FALSE).
        /// </summary>
        private const string sCookie2NameConfigurationKey = "cookie2Name";

        /// <summary>
        /// The configuration attribute for the header name.
        /// </summary>
        private const string sHeaderFormFieldNameConfigurationKey = "headerFormFieldName";

        /// <summary>
        /// The configuration attribute for the action to take when a CSRF attempt is detected.
        /// </summary>
        private const string sDetectionResultConfigurationKey = "detectionResult";
        
        /// <summary>
        /// Mode simulation : ReportOnly.
        /// </summary>
        private const string sReportOnlyConfigurationKey = "reportOnly";

        /// <summary>
        /// Log Assembly Name : logAssemblyName.
        /// </summary>
        private const string sLogAssemblyNameConfigurationKey = "logAssemblyName";

        /// <summary>
        /// The configuration attribute for the error page to redirect to if the detection result is configured to redirect.
        /// </summary>
        private const string sErrorPageConfigurationKey = "errorPage";

        #region "TODO: AntiCsrf - Fonctionnalité 'blocage des tokens pour la session courante' pas encore utilisée"
        /// <summary>
        /// The configuration attribute for the lock to session settings.
        /// </summary>
        private const string sSessionLockedConfigurationKey = "sessionLocked";
        #endregion

        /// <summary>
        /// The configuration attribute for the list of last segments URL exclusion WHITE LIST LEVEL 1
        /// </summary>
        private const string sWhiteList_Level1_ListLastSegmentsURLConfigurationKey = "whiteList_Level1_ListLastSegmentsURL";

        /// <summary>
        /// The configuration attribute for the list of last segments URL exclusion WHITE LIST LEVEL 2
        /// </summary>
        private const string sWhiteList_Level2_ListLastSegmentsURLConfigurationKey = "whiteList_Level2_ListLastSegmentsURL";

        /// <summary>
        /// The configuration attribute for the list of last segments URL exclusion WHITE LIST LEVEL 3
        /// </summary>
        private const string sWhiteList_Level3_ListLastSegmentsURLConfigurationKey = "whiteList_Level3_ListLastSegmentsURL";

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
        /// The default value for the cookie 1 name (Http Only = TRUE).
        /// </summary>
        private const string sCookie1NameDefaultValue = "XSRF-TOKEN-HO";

        /// <summary>
        /// The default value for the cookie 2 name (Http Only = FALSE).
        /// </summary>
        private const string sCookie2NameDefaultValue = "XSRF-TOKEN";

        /// <summary>
        /// The default value for the header name.
        /// </summary>
        private const string sHeaderFormFieldNameDefaultValue = "X-XSRF-TOKEN";

        /// <summary>
        /// The default value for the detection result.
        /// </summary>
        private const string sDetectionResultDefaultValue = "HTTP400BadRequest";

        /// <summary>
        /// The default value for the Report Only mode.
        /// </summary>
        private const bool bReportOnlyDefaultValue = false;

        /// <summary>
        /// Log Assembly Name.
        /// </summary>
        // TODO: AntiCsrf - La valeur initiale du paramètre sLogAssemblyName ne récupère pas comme attendu le nom de l'assembly appelante
        private static string sLogAssemblyNameInitialValue = Assembly.GetCallingAssembly().FullName;
        private const string sLogAssemblyNameDefaultValue = "<CurrentCallingAssemblyName>";

        /// <summary>
        /// The default value for list of last segments URL exclusion WHITE LIST LEVEL 1
        /// </summary>
        private const string sWhiteList_Level1_ListLastSegmentsURLDefaultValue = "";

        /// <summary>
        /// The default value for list of last segments URL exclusion WHITE LIST LEVEL 2
        /// </summary>
        private const string sWhiteList_Level2_ListLastSegmentsURLDefaultValue = "";

        /// <summary>
        /// The default value for list of last segments URL exclusion WHITE LIST LEVEL 3
        /// </summary>
        private const string sWhiteList_Level3_ListLastSegmentsURLDefaultValue = "";

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
            ConfigurationManager.GetSection("infomil/csrfSettings") as CsrfSettings;


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
                                        Cookie1Name = sCookie1NameDefaultValue,
                                        Cookie2Name = sCookie2NameDefaultValue,
                                        HeaderFormFieldName = sHeaderFormFieldNameDefaultValue,
                                        DetectionResult = ParseDetectionResult(sDetectionResultDefaultValue),
                                        ReportOnly = bReportOnlyDefaultValue,
                                        LogAssemblyName = sLogAssemblyNameInitialValue,
                                        WhiteList_Level1_ListLastSegmentsURL = sWhiteList_Level1_ListLastSegmentsURLDefaultValue,
                                        WhiteList_Level2_ListLastSegmentsURL = sWhiteList_Level2_ListLastSegmentsURLDefaultValue,
                                        WhiteList_Level3_ListLastSegmentsURL = sWhiteList_Level3_ListLastSegmentsURLDefaultValue,
                                        Log_Level = sLogLevelDefaultValue,
                                        LogFile = sLogFileDefaultValue,
                                        MaxSizeLogFile = lMaxSizeLogFileDefaultValue
                    };
                }

                return settings;
            }
        }

        /// <summary>
        /// Gets or sets the name of the cookie 1 used to hold the CSRF token (Http Only = TRUE).
        /// </summary>
        /// <value>The name of the cookie 1 used to hold the CSRF token.</value>
        [ConfigurationProperty(sCookie1NameConfigurationKey, DefaultValue = sCookie1NameDefaultValue)]     
        public string Cookie1Name
        {
            get
            {
                return (string)base[sCookie1NameConfigurationKey];
            }

            set
            {
                base[sCookie1NameConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the cookie 2 used to hold the CSRF token (Http Only = FALSE).
        /// </summary>
        /// <value>The name of the cookie 2 used to hold the CSRF token.</value>
        [ConfigurationProperty(sCookie2NameConfigurationKey, DefaultValue = sCookie2NameDefaultValue)]
        public string Cookie2Name
        {
            get
            {
                return (string)base[sCookie2NameConfigurationKey];
            }

            set
            {
                base[sCookie2NameConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the header or form field used to hold the CSRF token.
        /// </summary>
        /// <value>The name of the header or form field used to hold the CSRF token.</value>
        [ConfigurationProperty(sHeaderFormFieldNameConfigurationKey, DefaultValue = sHeaderFormFieldNameDefaultValue)]
        public string HeaderFormFieldName
        {
            get
            {
                return (string)base[sHeaderFormFieldNameConfigurationKey];
            }

            set
            {
                base[sHeaderFormFieldNameConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to raise an error or report only in case of potential xsrf
        /// </summary>
        /// <value>True to report only, otherwise false.</value>
        [ConfigurationProperty(sReportOnlyConfigurationKey, DefaultValue = bReportOnlyDefaultValue)]
        public bool ReportOnly
        {
            get
            {
                return (bool)base[sReportOnlyConfigurationKey];
            }

            set
            {
                base[sReportOnlyConfigurationKey] = value;
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

        #region "TODO: AntiCsrf - Fonctionnalité 'blocage des tokens pour la session courante' pas encore utilisée"
        /// <summary>
        /// Gets or sets a value indicating whether the tokens should additionally be locked
        /// to the current session.
        /// </summary>
        /// <value>True if the CSRF token should be linked to the current session, otherwise false.</value>
        [ConfigurationProperty(sSessionLockedConfigurationKey, DefaultValue = false)]
        public bool SessionLocked
        {
            get
            {
                return (bool)base[sSessionLockedConfigurationKey];
            }

            set
            {
                base[sSessionLockedConfigurationKey] = value;
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the operation to take when a CSRF attack is found.
        /// </summary>
        /// <value>The operation to take when a CSRF attack is found.</value>
        [ConfigurationProperty(sDetectionResultConfigurationKey, DefaultValue = sDetectionResultDefaultValue)]
        public DetectionResult DetectionResult
        {
            get
            {
                return ParseDetectionResult(base[sDetectionResultConfigurationKey]);
            }

            set
            {
                base[sDetectionResultConfigurationKey] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the error page when DetectionResult is set to "Redirect".
        /// </summary>
        /// <value>The URL of error page for redirection</value>
        [ConfigurationProperty(sErrorPageConfigurationKey)]
        public string ErrorPage
        {
            get
            {
                return (string)base[sErrorPageConfigurationKey];
            }

            set
            {
                base[sErrorPageConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Parses the detection result settings into the required enum.
        /// </summary>
        /// <param name="setting">The setting to parse.</param>
        /// <returns>A <see cref="DetectionResult"/> based on the setting provided.</returns>
        private static DetectionResult ParseDetectionResult(object setting)
        {
            DetectionResult detectionResult = DetectionResult.HTTP400BadRequest;
            
            if (setting is DetectionResult)
            {
                detectionResult = (DetectionResult)setting;
            }
            else if (Enum.IsDefined(typeof(DetectionResult), setting))
            {
               detectionResult = (DetectionResult)Enum.Parse(typeof(DetectionResult), setting.ToString());
            }

            return detectionResult;
        }

        /// <summary>
        /// Gets or sets a value indicating the white list - LEVEL 1 - Last Segments URL
        /// </summary>
        /// <value>The list of last segments URL exclusion WHITE LIST LEVEL 1</value>
        [ConfigurationProperty(sWhiteList_Level1_ListLastSegmentsURLConfigurationKey, DefaultValue = sWhiteList_Level1_ListLastSegmentsURLDefaultValue)]
        public string WhiteList_Level1_ListLastSegmentsURL
        {
            get
            {
                return (string)base[sWhiteList_Level1_ListLastSegmentsURLConfigurationKey];
            }

            set
            {
                base[sWhiteList_Level1_ListLastSegmentsURLConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the white list - LEVEL 2 - Last Segments URL
        /// </summary>
        /// <value>The list of last segments URL exclusion WHITE LIST LEVEL 2</value>
        [ConfigurationProperty(sWhiteList_Level2_ListLastSegmentsURLConfigurationKey, DefaultValue = sWhiteList_Level2_ListLastSegmentsURLDefaultValue)]
        public string WhiteList_Level2_ListLastSegmentsURL
        {
            get
            {
                return (string)base[sWhiteList_Level2_ListLastSegmentsURLConfigurationKey];
            }

            set
            {
                base[sWhiteList_Level2_ListLastSegmentsURLConfigurationKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the white list - LEVEL 3 - Last Segments URL
        /// </summary>
        /// <value>The list of last segments URL exclusion WHITE LIST LEVEL 3</value>
        [ConfigurationProperty(sWhiteList_Level3_ListLastSegmentsURLConfigurationKey, DefaultValue = sWhiteList_Level3_ListLastSegmentsURLDefaultValue)]
        public string WhiteList_Level3_ListLastSegmentsURL
        {
            get
            {
                return (string)base[sWhiteList_Level3_ListLastSegmentsURLConfigurationKey];
            }

            set
            {
                base[sWhiteList_Level3_ListLastSegmentsURLConfigurationKey] = value;
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

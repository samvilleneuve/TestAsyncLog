using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestAsyncLog
{
    /// <summary>
    /// Main logger class. Trace exception (or any debug/info information) asynchronously into a text file. On errors, delegate to "LoggerEventLog" class to log into event log
    /// </summary>
    public class LoggerFile
    {
        private readonly bool _bEnabled;
        // Supprime d'éventuels caractères d'échappement dans le séparateur de log (cas de la tabulation \t)
        private readonly string sSEP_LOG = Properties.Resources.sSeparator_log.Contains("\\") ? Regex.Unescape(Properties.Resources.sSeparator_log) : Properties.Resources.sSeparator_log;
        private readonly string sSEP_LOG_INTERNAL = Properties.Resources.sSeparator_log_internal.Contains("\\") ? Regex.Unescape(Properties.Resources.sSeparator_log_internal) : Properties.Resources.sSeparator_log_internal;

        private string _sParamFichierTraceRotationOnly = String.Empty;   // Fichier de trace avec le paramètre de rotation restant
        private string _sLogAssemblyName = String.Empty;
        private string _sFichierTrace = String.Empty;
        private string _sFileNameOnly = String.Empty;
        private bool _bNumericRotation = false;
        private int _iParamMaxNumericRotation = -1;
        private int _iNumericRotationLenght = -1;
        private string _sParamMaxNumericRotation = String.Empty;

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        
        public LoggerFile() : this(true)
        {
        }

        public LoggerFile(bool bEnabled)
        {
            _bEnabled = bEnabled;   // This argument is used to keep the static property LoggerFile moduleLogger in HTTP module and doesn't call InitTargetFileContext() to gain in performance when starting HTTP module
            if (_bEnabled == true)
            {
                InitTargetFileContext();
            }
        }

        private void InitTargetFileContext()
        {
            string sFICHIER_TRACE_DEFAUT = Path.GetTempPath() + Properties.Resources.sLogFileName;
            _sLogAssemblyName = TestAsyncLogSettings.Settings.LogAssemblyName;
            // NOT_USED: string sParentDir = String.Empty;
            try
            {
                string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
                string sParamFichierTrace = TestAsyncLogSettings.Settings.LogFile;
                Process curProcess = Process.GetCurrentProcess();
                string sPID = curProcess.Id.ToString();
                sParamFichierTrace = sParamFichierTrace.Replace("%TempPath%", Path.GetTempPath());
                sParamFichierTrace = sParamFichierTrace.Replace("%AppGUID%", appGuid);
                sParamFichierTrace = sParamFichierTrace.Replace("%PID%", sPID);
                sParamFichierTrace = sParamFichierTrace.Replace("%LogAssemblyName%", _sLogAssemblyName);

                // NOT_USED: sParentDir = Directory.GetParent(sParamFichierTrace).FullName;
                // NOT_USED: DirectoryInfo parentDir = Directory.GetParent(sParamFichierTrace);

                MatchCollection matchCollection = Regex.Matches(sParamFichierTrace, "%Rotate=(\\d+)%");
                if (matchCollection.Count != 0)
                {
                    // Rotation possible
                    if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 2)
                    {
                        throw new ApplicationException("Erreur de détection de la rotation dans le paramètre du fichier de log!");
                    }
                    // Rotation certaine
                    _iParamMaxNumericRotation = int.Parse(matchCollection[0].Groups[1].Value);
                    _iNumericRotationLenght = matchCollection[0].Groups[1].Value.Length;
                    _sParamMaxNumericRotation = _iParamMaxNumericRotation.ToString("D" + _iNumericRotationLenght);
                    _bNumericRotation = true;
                    int iZero = 0;
                    string sZeroRotation = iZero.ToString("D" + matchCollection[0].Groups[1].Value.Length);
                    _sParamFichierTraceRotationOnly = sParamFichierTrace;
                    // On définie le fichier cible avec la rotation zéro - la trace se fait toujours dans le fichier "zero".
                    sParamFichierTrace = Regex.Replace(sParamFichierTrace, "%Rotate=(\\d+)%", m => sZeroRotation);
                }
                _sFileNameOnly = sParamFichierTrace.Substring(sParamFichierTrace.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                _sFichierTrace = sParamFichierTrace;
            }
            catch
            {
                // On utilise la valeur par défaut dans le fichier de ressource si il y a eu une erreur dans le traitement du paramètre "LogFile" (TestAsyncSettings.Settings.LogFile)
                _sFileNameOnly = sFICHIER_TRACE_DEFAUT.Substring(sFICHIER_TRACE_DEFAUT.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                _sFichierTrace = sFICHIER_TRACE_DEFAUT;
            }
        }

        /// <summary>
        /// Raises an error based on the configuration settings.
        /// </summary>
        /// <param name="iIDLog">Log ID to facilitate the reading of log file.</param>
        /// <param name="typeLog">type of log (LogCategory): Debug, Information, Warning, Error, Fatal</param>
        /// <param name="sDetailLog">the text of be log</param>
        /// <param name="context">The current HttpContext.</param>
        /// <param name="sLogEventName">Nom de l'évènement à tracer dans les logs.</param>
        public void LogAsync(ref int iIDLog, LogCategory typeLog, string sDetailLog, HttpContext context, string sLogEventName)
        {
            if (_bEnabled == false)  // This test shouldn't be necessary. Thus, no calls to LogAsync should be done if constructor has been initialized with bEnabled = false. This test is done for security reason.
            {
                return;
            }
            LogAsync(ref iIDLog, typeLog, sDetailLog, context, sLogEventName, false);
        }

        /// <summary>
        /// Raises an error based on the configuration settings.
        /// </summary>
        /// <param name="iIDLog">Log ID to facilitate the reading of log file.</param>
        /// <param name="typeLog">type of log (LogCategory): Debug, Information, Warning, Error, Fatal</param>
        /// <param name="sDetailLog">the text of be log</param>
        /// <param name="context">The current HttpContext.</param>
        /// <param name="sLogEventName">Nom de l'évènement à tracer dans les logs.</param>
        /// <param name="bAddRequestHeaders">Ajoute les headers de la request dans les traces.</param>
        public void LogAsync(ref int iIDLog, LogCategory typeLog, string sDetailLog, HttpContext context, string sLogEventName, bool bAddRequestHeaders)
        {
            if (_bEnabled == false) // This test shouldn't be necessary. Thus, no calls to LogAsync should be done if constructor has been initialized with bEnabled = false. This test is done for security reason.
            {
                return;
            }
            DateTime dtDateTimeLog = DateTime.Now;  // On défini la date et l'heure du log
            iIDLog += 1; // On incrémente l'ID de log pour faciliter la lecture du fichier de trace en suivant plus facilement l'ordre des traces

            // Log message (exception message or anything else)
            // avec l'adresse IP de l'appelant
            // avec l'URL de la requête
            string sTraceHeader = String.Empty;
            string sTrace = String.Empty;
            string sDateTimeLog = $"{dtDateTimeLog:yyyy/MM/dd HH:mm:ss.fff}";
            string sIDLog = iIDLog.ToString();
            try
            {
                // On trace toutes les demandes de catégorie inférieure ou égale au niveau défini dans la configuration
                if (typeLog.Level <= LogCategory.GetLevel(TestAsyncLogSettings.Settings.Log_Level))
                {
                    System.Diagnostics.Debug.WriteLine("Logging through pLogAsync function.");
                    sTraceHeader = $"Type Log{sSEP_LOG}Log Assembly Name{sSEP_LOG}Log event name{sSEP_LOG}Request Http Method{sSEP_LOG}IP{sSEP_LOG}Request Url Referrer{sSEP_LOG}Request Url{sSEP_LOG}Detail Log";
                    
                    // On récupère l'@IP du client
                    string sIP = string.Empty;
                    try
                    {
                        sIP = "127.0.0.1"; // TODO: To Be Changed with a dynamic value!
                    }
                    catch
                    {
                        sIP = "NA";
                    }

                    // On ajoute les headers de la request dans les traces
                    if (bAddRequestHeaders == true)
                    {
                        sDetailLog += GetAllRequestHeaders(context.Request);
                    }
                    sTrace = $"{typeLog.Value}{sSEP_LOG}{_sLogAssemblyName}{sSEP_LOG}{sLogEventName}{sSEP_LOG}{context.Request.HttpMethod}{sSEP_LOG}{sIP}{sSEP_LOG}{context.Request.UrlReferrer}{sSEP_LOG}{context.Request.Url.ToString()}{sSEP_LOG}{sDetailLog}";

                    // sDateTimeLog & sIDLog ==> Paramètres utiles uniquement pour la fonction LoggerEventLog() en cas d'erreur de trace dans le fichier texte
                    // Place l'exécution de la tâche "pLogPotentialExceptionAsync" dans le pool de thread ("ThreadPool")
                    Task.Run(() => pLogPotentialExceptionAsync(new string[] { sTraceHeader, sTrace, sDateTimeLog, sIDLog }) );
                }
            }
            catch (Exception ex)
            {
                try
                {
                    string sMsgErrorLogging = "Error when logging through pLogAsync function.";
                    LoggerEventLog loggerEventLog = new LoggerEventLog(sMsgErrorLogging, sDateTimeLog, sIDLog, ex, sTraceHeader, sTrace, _sLogAssemblyName);
                    loggerEventLog.Log();
                }
                catch (Exception)
                {
                    // Last attempt: can't log into file neither into EventLog
                }
            }
        }

        private string GetAllRequestHeaders(HttpRequest request)
        {
            string sHeaders = String.Empty;
            sHeaders += "Request headers: ";
            foreach (var key in request.Headers.AllKeys)
            {
                sHeaders += key + "=" + request.Headers[key] + sSEP_LOG_INTERNAL;
            }
            return sHeaders;
        }

        private void WriteToFileThreadSafe(string sPath, string sTextLine1, string sTextLine2)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append Text (Line 1 and Line 2) to the file
                using (StreamWriter sw = File.AppendText(sPath))
                {
                    if (!String.IsNullOrEmpty(sTextLine1))
                        sw.Write(sTextLine1);
                    if (!String.IsNullOrEmpty(sTextLine2))
                        sw.Write(sTextLine2);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        private void pLogPotentialExceptionAsync(object objParam)
        {
            string sShortMutexName = String.Empty;
            string sFullMutexName = String.Empty;
            string sTraceHeader = String.Empty;
            string sTrace = String.Empty;
            StringBuilder sbExInfoMaxAttempt = new StringBuilder();

            string[] objListStringParams = objParam as string[];
            string sHeaderLog = objListStringParams[0];
            string sDetailLog = objListStringParams[1];
            string sDateTimeLog = objListStringParams[2];   // sDateTimeLog ==> Paramètre utile uniquement pour la fonction LoggerEventLog() en cas d'erreur de trace dans le fichier texte
            string sIDLog = objListStringParams[3];         // sIDLog ==> Paramètre utile uniquement pour la fonction LoggerEventLog() en cas d'erreur de trace dans le fichier texte
            try
            {
                System.Diagnostics.Debug.WriteLine("Logging through pLogPotentialExceptionAsync function.");
                int iMAX_ATTEMPT_LOG_FILE = int.Parse(Properties.Resources.iMaxAttemptLogFileBeforeEventLog);
                long lTAILLE_MAX = TestAsyncLogSettings.Settings.MaxSizeLogFile;

                // On remplace le paramètre %Date% par la date du jour dans le nom du fichier et dans le chemin complet
                // Cette opération est effectuée ici pour prendre en compte le changement éventuel de jour (trace effectuée avant et après minuit)
                // Les autres paramètres ont déjà été remplacés au moment du constructeur
                DateTime dtToday = DateTime.Now;
                _sFileNameOnly = _sFileNameOnly.Replace("%Date%", $"{dtToday:yyyyMMdd}");
                _sFichierTrace = _sFichierTrace.Replace("%Date%", $"{dtToday:yyyyMMdd}");
                _sParamFichierTraceRotationOnly = _sParamFichierTraceRotationOnly.Replace("%Date%", $"{dtToday:yyyyMMdd}");


                int iTentative = 0;
                bool bLogFileSuccess = false;
                sTraceHeader = $"DateTime (yyyy/MM/dd HH:mm:ss.fff) ID Log{sSEP_LOG}{sHeaderLog}{Environment.NewLine}";
                sTrace = $"{sDateTimeLog} {sIDLog}{sSEP_LOG}{sDetailLog}{Environment.NewLine}";

                sShortMutexName = _sFileNameOnly;
                using (var anySingleGlobalInstance = new SingleGlobalInstance(5000, sShortMutexName, out sFullMutexName)) // 5000 ms (5 s) timeout on global lock
                {
                    // Perform log work here.
                    // Only 1 of these runs at a time
                    do
                    {
                        try
                        {
                            iTentative += iTentative;
                            FileInfo objFileInfo = new FileInfo(_sFichierTrace);
                            if (objFileInfo.Exists && objFileInfo.Length >= lTAILLE_MAX)
                            {
                                if (_bNumericRotation == false)
                                {
                                    // Pas de rotation ==> On supprime le fichier existant.
                                    File.Delete(_sFichierTrace);
                                }
                                else
                                {
                                    // Rotation ==> On renomme les fichiers en augmentant l'indice de 1
                                    // Exemple pour une rotation sur "03", dans l'ordre :
                                    //      - on supprime le "03",
                                    //      - renommage "02" ==> "03",
                                    //      - renommage "01" ==> "02",
                                    //      - renommage "00" ==> "01"
                                    //      - on écrit dans le "00".
                                    string sLastFile = Regex.Replace(_sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => _sParamMaxNumericRotation);
                                    File.Delete(sLastFile); // Si le fichier à supprimer n’existe pas, aucune exception n’est levée.
                                    for (int iRotation = _iParamMaxNumericRotation - 1; iRotation >= 0; iRotation--)
                                    {
                                        string sIndiceRotationFrom = iRotation.ToString("D" + _iNumericRotationLenght);
                                        FileInfo objFileRotationFrom = new FileInfo(Regex.Replace(_sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => sIndiceRotationFrom));
                                        string sIndiceRotationTo = (iRotation + 1).ToString("D" + _iNumericRotationLenght);
                                        string sRotateFileName = Regex.Replace(_sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => sIndiceRotationTo);
                                        if (objFileRotationFrom.Exists)
                                        {
                                            FileInfo objFileInfoTo = new FileInfo(sRotateFileName);
                                            if (objFileInfoTo.Exists)
                                            {
                                                // On supprime le fichier cible s'il existe déjà !
                                                File.Delete(sRotateFileName);
                                            }
                                            objFileRotationFrom.MoveTo(sRotateFileName);
                                        }
                                    }
                                }
                            }
                            FileInfo objFile = new FileInfo(_sFichierTrace);
                            if ((!objFile.Exists) || (objFile.Exists && objFile.Length == 0))
                            {
                                // On loggue avec le header
                                WriteToFileThreadSafe(_sFichierTrace, sTraceHeader, sTrace);
                            }
                            else
                            {
                                // On loggue simplement (le header doit être présent au début du fichier)
                                WriteToFileThreadSafe(_sFichierTrace, null, sTrace);
                            }
                            bLogFileSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            // In case of max attempt: Information about exception is kept for later use in eventlog
                            // Else: Exception can be ignored because of max attempt in do/while
                            if (iTentative == 1)
                            {
                                // On ne loggue qua la 1ère erreur (la plus pertinente) pour éviter un éventuel dépassement de capacité du StringBuilder
                                sbExInfoMaxAttempt.AppendLine(String.Format("Error during attempt {0}/{1}", iTentative.ToString(), iMAX_ATTEMPT_LOG_FILE.ToString()));
                                AddExceptionInfoToSB(sbExInfoMaxAttempt, ex);
                            }
                        }
                    }
                    while (!bLogFileSuccess && iTentative < iMAX_ATTEMPT_LOG_FILE);

                    if (!bLogFileSuccess)
                    {
                        try
                        {
                            string sMsgErrorLogging = "Potential Exception: Maximum number of attempt reached to log into file.";
                            LoggerEventLog loggerEventLog = new LoggerEventLog(sMsgErrorLogging, sDateTimeLog, sIDLog, null, sTraceHeader, sTrace, _sLogAssemblyName);
                            loggerEventLog.SetShortMutexName(sShortMutexName);
                            loggerEventLog.SetFullMutexName(sFullMutexName);
                            loggerEventLog.SetExInfoMaxAttempt(sbExInfoMaxAttempt.ToString());
                            loggerEventLog.Log();
                        }
                        catch (Exception)
                        {
                            // Last attempt: can't log into file neither into EventLog
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    string sMsgErrorLogging = String.Empty;
                    if (ex is System.TimeoutException)
                    {
                        sMsgErrorLogging = "Potential Exception: Timeout when logging asynchronously into file.";
                    }
                    else
                    {
                        sMsgErrorLogging = "Potential Exception: Error when logging.";
                    }
                    LoggerEventLog loggerEventLog = new LoggerEventLog(sMsgErrorLogging, sDateTimeLog, sIDLog, ex, sTraceHeader, sTrace, _sLogAssemblyName);
                    loggerEventLog.SetShortMutexName(sShortMutexName);
                    loggerEventLog.SetFullMutexName(sFullMutexName);
                    loggerEventLog.Log();
                }
                catch (Exception)
                {
                    // Last attempt: can't log into file neither into EventLog
                }
            }
        }

        public static void AddExceptionInfoToSB(StringBuilder sbExInfo, Exception ex)
        {
            sbExInfo.AppendLine(" [Source exception: " + ex.Source + "]");
            sbExInfo.AppendLine(" [Message exception: " + ex.Message + "]");
            sbExInfo.AppendLine(" [StackTrace: " + ex.StackTrace + "]");
        }
    }

}
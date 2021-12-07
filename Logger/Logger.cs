using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace TestAsyncLog
{
    public class Logger
    {
        // Supprime d'éventuels caractères d'échappement dans le séparateur de log (cas de la tabulation \t)
        private readonly string sSEP_LOG = Properties.Resources.sSeparator_log.Contains("\\") ? Regex.Unescape(Properties.Resources.sSeparator_log) : Properties.Resources.sSeparator_log;

        public void LogAsync(LogCategory typeLog, string sDetailLog, HttpContext context, string sLogEventName)
        {
            // TODO: AntiCsrf - Améliorer et armoniser les traces
            //        Exemple : - Armoniser le formattage des traces dans les ressources (dans Ressources.resx)
            //                  - Mettre tous les messages de trace en français !
            //                  - Mettre un système de remplacement via des paramètres au lieu des "string.format" et des chaînes de type $"{Variable}" en dure dans le code
            //                  - Armoniser les séparateurs (espaces, tirets, ...)
            //                  - Externaliser un maximum le format et contenu des logs dans le fichier de ressource Ressources.resx (format date/heure, type de log, ...)
            //                  - Garder à l'esprit le besoin d'une exploitation aisée du fichier de trace via Excel ou Visual Code avec un séparateur reconnu (exemple : TSV pour Tab Separator Value)

            // Log message (exception message or anything else)
            // avec l'adresse IP de l'appelant
            // avec l'URL de la requête
            try
            {
                // On trace toutes les demandes de catégorie inférieure ou égale au niveau défini dans la configuration
                if (typeLog.Level <= LogCategory.GetLevel(CsrfSettings.Settings.Log_Level))
                {
                    System.Diagnostics.Debug.WriteLine("Logging through pLogAsync function.");
                    string sIP = string.Empty;
                    try
                    {
                        sIP = "Bidon";
                    }
                    catch
                    {
                        sIP = "NA";
                    }
                    string sLogAssemblyName = CsrfSettings.Settings.LogAssemblyName;
                    string sFullLineLogHeader = $"Type Log{sSEP_LOG}Log Assembly Name{sSEP_LOG}Log event name{sSEP_LOG}Request Http Method{sSEP_LOG}Detail Log{sSEP_LOG}IP{sSEP_LOG}Request Url Referrer{sSEP_LOG}Request Url";
                    string sFullLineLog = $"{typeLog.Value}{sSEP_LOG}{sLogAssemblyName}{sSEP_LOG}{sLogEventName}{sSEP_LOG}{context.Request.HttpMethod}{sSEP_LOG}{sDetailLog}{sSEP_LOG}{sIP}{sSEP_LOG}{context.Request.UrlReferrer}{sSEP_LOG}{context.Request.Url.ToString()}";

                    ThreadPool.QueueUserWorkItem(pLogPotentialCsrfException, new string[] { sFullLineLogHeader, sFullLineLog });
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Error when logging through pLogAsync function.");
            }
        }

        private void pLogPotentialCsrfException(object objParam)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Logging through pLogPotentialCsrfException function.");
                string sMSG_LOG_XSRF_EVENTLOG = Properties.Resources.sEventLogMessageFormat;
                string sSOURCE_MESSAGE_EVENTLOG = Properties.Resources.sSourceEventLog;
                int iMAX_ATTEMPT_LOG_FILE = int.Parse(Properties.Resources.iMaxAttemptLogFileBeforeEventLog);
                long lTAILLE_MAX = CsrfSettings.Settings.MaxSizeLogFile;
                string sFICHIER_TRACE_DEFAUT = Path.GetTempPath() + Properties.Resources.sLogFileName;
                string sFichierTrace = String.Empty;    // Fichier où l'on écrit (le fichier zéro pour les rotations)
                string sParamFichierTraceRotationOnly = String.Empty;   // Fichier de trace avec le paramètre de rotation restant
                bool bNumericRotation = false;
                string sParentDir = String.Empty;
                int iParamMaxNumericRotation = -1;
                int iNumericRotationLenght = -1;
                string sParamMaxNumericRotation = String.Empty;
                string sFileNameOnly = String.Empty;
                try
                {
                    string sParamFichierTrace = CsrfSettings.Settings.LogFile;
                    sParamFichierTrace = sParamFichierTrace.Replace("%TempPath%", Path.GetTempPath());
                    sParamFichierTrace = sParamFichierTrace.Replace("%Date%", $"{DateTime.Now:yyyyMMdd}");

                    sParentDir = Directory.GetParent(sParamFichierTrace).FullName;
                    DirectoryInfo parentDir = Directory.GetParent(sParamFichierTrace);

                    MatchCollection matchCollection = Regex.Matches(sParamFichierTrace, "%Rotate=(\\d+)%");
                    if (matchCollection.Count != 0)
                    {
                        // Rotation possible
                        if (matchCollection.Count != 1 || matchCollection[0].Groups.Count != 2)
                        {
                            throw new ApplicationException("Erreur de détection de la rotation dans le paramètre du fichier de log!");
                        }
                        // Rotation certaine
                        iParamMaxNumericRotation = int.Parse(matchCollection[0].Groups[1].Value);
                        iNumericRotationLenght = matchCollection[0].Groups[1].Value.Length;
                        sParamMaxNumericRotation = iParamMaxNumericRotation.ToString("D" + iNumericRotationLenght);
                        bNumericRotation = true;
                        int iZero = 0;
                        string sZeroRotation = iZero.ToString("D" + matchCollection[0].Groups[1].Value.Length);
                        sParamFichierTraceRotationOnly = sParamFichierTrace;
                        sParamFichierTrace = Regex.Replace(sParamFichierTrace, "%Rotate=(\\d+)%", m => sZeroRotation);
                    }
                    sFileNameOnly = sParamFichierTrace.Substring(sParamFichierTrace.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    sFichierTrace = sParamFichierTrace;
                }
                catch
                {
                    // On utilise la valeur par défaut dans le fichier de ressource si il y a eu une erreur dans le traitement du paramètre "LogFile" (CsrfSettings.Settings.LogFile)
                    sFileNameOnly = sFICHIER_TRACE_DEFAUT.Substring(sFICHIER_TRACE_DEFAUT.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    sFichierTrace = sFICHIER_TRACE_DEFAUT;
                }

                string[] objListStringParams = objParam as string[];
                string sHeaderLog = objListStringParams[0];
                string sDetailLog = objListStringParams[1];
                int iTentative = 0;
                bool bLogFileSuccess = false;
                string sTrace = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}{sSEP_LOG}{sDetailLog}{Environment.NewLine}";
                string sTraceHeader = $"DateTime (yyyy/MM/dd HH:mm:ss.fff){sSEP_LOG}{sHeaderLog}{Environment.NewLine}";

                using (new SingleGlobalInstance(5000, sFileNameOnly)) //5000ms timeout on global lock
                {
                    // Perform log work here.
                    // Only 1 of these runs at a time
                    do
                    {
                        try
                        {
                            iTentative += iTentative;
                            FileInfo objFileInfo = new FileInfo(sFichierTrace);
                            if (objFileInfo.Exists && objFileInfo.Length >= lTAILLE_MAX)
                            {
                                if (bNumericRotation == false)
                                {
                                    // Pas de rotation ==> On supprime le fichier existant.
                                    File.Delete(sFichierTrace);
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
                                    string sLastFile = Regex.Replace(sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => sParamMaxNumericRotation);
                                    File.Delete(sLastFile); // Si le fichier à supprimer n’existe pas, aucune exception n’est levée.
                                    for (int iRotation = iParamMaxNumericRotation - 1; iRotation >= 0; iRotation--)
                                    {
                                        string sIndiceRotationFrom = iRotation.ToString("D" + iNumericRotationLenght);
                                        FileInfo objFileRotationFrom = new FileInfo(Regex.Replace(sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => sIndiceRotationFrom));
                                        string sIndiceRotationTo = (iRotation + 1).ToString("D" + iNumericRotationLenght);
                                        string sRotateFileName = Regex.Replace(sParamFichierTraceRotationOnly, "%Rotate=(\\d+)%", m => sIndiceRotationTo);
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
                            if ((!objFileInfo.Exists) || (objFileInfo.Exists && objFileInfo.Length == 0))
                            {
                                File.AppendAllText(sFichierTrace, sTraceHeader);
                            }
                            File.AppendAllText(sFichierTrace, sTrace);
                            bLogFileSuccess = true;
                        }
                        catch
                        {
                        }
                    }
                    while (!bLogFileSuccess || iTentative == iMAX_ATTEMPT_LOG_FILE);

                    if (!bLogFileSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("EVENTLOG - " + string.Format(sMSG_LOG_XSRF_EVENTLOG, sDetailLog));
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Error when logging through pLogPotentialCsrfException function.");
            }
        }
    }
}
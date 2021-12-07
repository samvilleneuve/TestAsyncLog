using System;

namespace TestAsyncLog
{
    public class LogCategory
    {
        /// <summary>Keyword Log ALL level - Same as highest level : DEBUG</summary>
        public const string ALL = "ALL";
        /// <summary>Keyword Log ALL level - Value = 5</summary>
        public const int ALL_LEVEL = 5;

        /// <summary>Log DEBUG level - Text</summary>
        public const string DEBUG = "DEBUG";
        /// <summary>Log DEBUG level - Value = 5</summary>
        public const int DEBUG_LEVEL = 5;

        /// <summary>Log INFO level - Text</summary>
        public const string INFO = "INFO";
        /// <summary>Log INFO level - Value = 4</summary>
        public const int INFO_LEVEL = 4;

        /// <summary>Log WARN level - Text</summary>
        public const string WARN = "WARN";
        /// <summary>Log WARN level - Value = 3</summary>
        public const int WARN_LEVEL = 3;

        /// <summary>Log ERROR level - Text</summary>
        public const string ERROR = "ERROR";
        /// <summary>Log ERROR level - Value = 2</summary>
        public const int ERROR_LEVEL = 2;

        /// <summary>Log FATAL level - Text</summary>
        public const string FATAL = "FATAL";
        /// <summary>Log FATAL level - Value = 1</summary>
        public const int FATAL_LEVEL = 1;

        private LogCategory(string value, int level)
        {
            Value = value;
            Level = level;
        }

        /// <summary>
        /// Provide value of Log Category
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Provide level of Log Category
        /// </summary>
        public int Level { get; private set; }


        /// <summary>Get level value from log category</summary>
        public static int GetLevel(LogCategory logCategory)
        {
            return logCategory.Level;
        }

        /// <summary>Get level value from log category name</summary>
        public static int GetLevel(string sLogCategory)
        {
            switch (sLogCategory)
            {
                case ALL: return LogCategory.All.Level;
                case DEBUG: return LogCategory.Debug.Level;
                case INFO: return LogCategory.Information.Level;
                case WARN: return LogCategory.Warning.Level;
                case ERROR: return LogCategory.Error.Level;
                case FATAL: return LogCategory.Fatal.Level;
                default: return LogCategory.All.Level;
            }
        }

        /// <summary>Keyword Log ALL level - Same as highest level : DEBUG</summary>
        public static LogCategory All { get { return new LogCategory(ALL, ALL_LEVEL); } }


        /// <summary>Log DEBUG level</summary>
        public static LogCategory Debug { get { return new LogCategory(DEBUG, DEBUG_LEVEL); } }

        /// <summary>Log INFO level</summary>
        public static LogCategory Information { get { return new LogCategory(INFO, INFO_LEVEL); } }

        /// <summary>Log WARN level</summary>
        public static LogCategory Warning { get { return new LogCategory(WARN, WARN_LEVEL); } }

        /// <summary>Log ERROR level</summary>
        public static LogCategory Error { get { return new LogCategory(ERROR, ERROR_LEVEL); } }

        /// <summary>Log FATAL level</summary>
        public static LogCategory Fatal { get { return new LogCategory(FATAL, FATAL_LEVEL); } }
    }
}
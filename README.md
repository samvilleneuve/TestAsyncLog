Welcome to ***TestASyncLog***
=================================================

TestASyncLog is a C# project to demonstrate how to log asynchronously into a log file from an ASP.Net application.

It uses mainly:
- the [Task.Run](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-6.0#system-threading-tasks-task-run(system-action)) method to queue specified logging work on the ThreadPool.
- the [System.Threading.Mutex](https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex?view=net-6.0) class to manage exclusive access to the log file.

Getting Started
===============
# **TestASyncLog**
**TestASyncLog** is an Visual Studio 2019 solution/project.

Main files:
- [TestAsyncLog.cs](./TestAsyncLog.cs)
- [SingleGlobalInstance.cs](./SingleGlobalInstance.cs)
- [Logger\LogCategory.cs](./Logger/LogCategory.cs)
- [Logger\LoggerEventLog.cs](./Logger/LoggerEventLog.cs)
- [Logger\LoggerFile.cs](./Logger/LoggerFile.cs)
- [Configuration\TestAsyncLogSettings.cs](./Configuration/TestAsyncLogSettings.cs)
- [Properties\Resources.resx](./Properties/Resources.resx)
- [Web.config](./Web.config)

Parameter "logFile" in [Web.config](./Web.config):
```
Define the log file name and location
Use of possible parameters "%TempPath%" or "%Date%" or "%Rotate=X%" (see below)
This defaults to "%TempPath%TestASyncLog.log.tsv".
Note that after 5 attempts to write successfully in log file, the log is written on Windows Event Log.
Use "%TempPath%" for the path of the current user's temporary folder (this includes the last backslash)
    Checks for the existence of environment variables in the following order and uses the first path found :
    - The path specified by the TMP environment variable.
    - The path specified by the TEMP environment variable.
    - The path specified by the USERPROFILE environment variable.
    - The Windows directory.
    Example: "%TempPath%TestASyncLog.log" for a IIS Express (localhost) execution => "C:\Users\svilleneuve\AppData\Local\Temp\TestASyncLog.log")
Use "%Date%" for the current date time format string "yyyyMMdd"
    Example: "C:\Temp\%Date%_TestASyncLog.log" => "C:\Temp\20211225_TestASyncLog.log")
Use "%Rotate=X%" for the number of rotation starting from zero (0) with possible leading zeros.
    In case of rotation: - Log activity will be always done on the "zero" log file.
                         - Rotation will be done from the "zero" log file to the last highest log file.
Use "%AppGUID%" for an unique assembly GUID.
Use "%PID%" for the unique identifier of the associated process.
Use %LogAssemblyName% to use the setting "logAssemblyName" (see below)
```

Parameter "logAssemblyName" in [Web.config](./Web.config):
```
Specifies the assembly name or any usefull text to identify the caller in log file
If not defined, current executing assembly name will be used.
This parameter should be defined for performance reasons.
```

Parameter "maxSizeLogFile" in [Web.config](./Web.config):
```
Define the maximum size of current log file (in bytes) before a new rotation or overriding the current log file (deletion and creation of a new log file)
This defaults to 10485760 (10 Mb = 10485760 bytes = 10 * 1024 * 1024 = 10240 Kb)
```


Target framework
===============
The project TestASyncLog targets ***Microsoft .Net Framework 4.5.2***.


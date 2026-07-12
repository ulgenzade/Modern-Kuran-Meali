using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace KuranMealApp.Services
{
    public static class CrashLogger
    {
        private static readonly string CrashLogPath = Path.Combine(FileSystem.AppDataDirectory, "crash_log.txt");

        public static void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                LogCrash(args.ExceptionObject as Exception, "AppDomain.UnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                LogCrash(args.Exception, "TaskScheduler.UnobservedTaskException");
            };

#if ANDROID
            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                LogCrash(args.Exception, "AndroidEnvironment.UnhandledExceptionRaiser");
            };
#endif
        }

        public static void LogCrash(Exception? ex, string source)
        {
            if (ex == null) return;
            try
            {
                var log = $"Time: {DateTime.Now}\n" +
                          $"Source: {source}\n" +
                          $"Message: {ex.Message}\n" +
                          $"Stack Trace:\n{ex.StackTrace}\n";

                if (ex.InnerException != null)
                {
                    log += $"\nInner Exception:\n{ex.InnerException.Message}\n" +
                           $"Inner Stack Trace:\n{ex.InnerException.StackTrace}\n";
                }

                File.WriteAllText(CrashLogPath, log);
            }
            catch
            {
                // Ignore errors during logging
            }
        }

        public static bool HasCrashLog()
        {
            try
            {
                return File.Exists(CrashLogPath);
            }
            catch
            {
                return false;
            }
        }

        public static string ReadCrashLog()
        {
            try
            {
                if (File.Exists(CrashLogPath))
                {
                    return File.ReadAllText(CrashLogPath);
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public static void ClearCrashLog()
        {
            try
            {
                if (File.Exists(CrashLogPath))
                {
                    File.Delete(CrashLogPath);
                }
            }
            catch
            {
            }
        }
    }
}

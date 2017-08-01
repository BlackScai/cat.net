using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Xml;

namespace Org.Unidal.Cat.Util
{
    /// <summary>
    ///   简单记录Cat客户端的启动日志, 记录日志不会超过512K
    /// </summary>
    public class Logger
    {
        private const int LOG_MAX_BYTE_SIZE = 512 * 1024;
        private static StreamWriter _mWriter;
        private static object _mSyncLock = new object();
        private static bool _mInitialized;
        private static string _mDomain = "unknown";
        private static int byteSize = 0;

        public static void Initialize(string domain)
        {
            if (!_mInitialized)
            {
                lock (_mSyncLock)
                {
                    if (!_mInitialized)
                    {
                        if (!string.IsNullOrWhiteSpace(domain))
                        {
                            _mDomain = domain;
                        }
                        CreateWriter();
                        _mInitialized = true;
                    }
                }
            }
        }

        public static void Debug(string pattern, params object[] args)
        {
            Log("DEBUG", pattern, args);
        }

        public static void Info(string pattern, params object[] args)
        {
            Log("INFO", pattern, args);
        }

        public static void Warn(string pattern, params object[] args)
        {
            Log("WARN", pattern, args);
        }

        public static void Error(string pattern, params object[] args)
        {
            Log("ERROR", pattern, args);
        }

        public static void Error(Exception ex, string msg = "")
        {
            Log("ERROR", msg + "\n" + ex);
        }

        public static void Fatal(string pattern, params object[] args)
        {
            Log("FATAL", pattern, args);
        }

        private static void Log(string severity, string pattern, params object[] args)
        {
            if (byteSize >= LOG_MAX_BYTE_SIZE)
            {
                return;
            }
            lock (_mSyncLock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string message = string.Format(pattern, args);
                    string line = "[" + timestamp + "] [" + severity + "] " + message;

                    if (_mWriter != null)
                    {
                        _mWriter.WriteLine(line);
                        _mWriter.Flush();
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                    byteSize += line.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void CreateWriter()
        {
            if (null != _mWriter)
            {
                return;
            }
            string logFile = "cat-client-" + _mDomain + ".log";

            _mWriter = CreateStreamWriter(Path.GetTempPath(), logFile);
        }

        private static StreamWriter CreateStreamWriter(string dir, string fileName)
        {
            StreamWriter writer = null;
            try
            {
                if (Directory.Exists(dir) && isWritable(dir))
                {
                    writer = new StreamWriter(Path.Combine(dir, fileName), false);
                    byteSize = 0;
                    Console.WriteLine("Successfully opened log file [{0}\\{1}].", dir, fileName);
                }
                else
                {
                    Console.WriteLine("Log file [{0}\\{1}] does not exist, or is not writable.", dir, fileName);
                    writer = null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when opening log file [{0}\\{1}]." + ex, dir, fileName);
                writer = null;
            }
            return writer;
        }

        private static bool isWritable(string filename)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, filename);
            permissionSet.AddPermission(writePermission);

            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
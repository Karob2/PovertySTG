using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Engine.Util
{
    /// <summary>
    /// Logging and debugging features.
    /// </summary>
    public class Error
    {
        GameServices gs;
        // Path to log file.
        string logPath;
        // Whether or not logging has started.
        bool started = false;
        // For a debugging method that avoids logging the same message repeatedly.
        HashSet<string> onceIDs = new HashSet<string>();

        public Error(GameServices gs)
        {
            this.gs = gs;
        }

        /// <summary>
        /// Begins logging and prints a timestamp to the log.
        /// </summary>
        public void StartLog()
        {
            //TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            //Trace.Listeners.Add(myWriter);

#if !DEBUG
            // Log errors to %localappdata%/[company]/[game]/log.txt
            logPath = gs.ResourceManager.GetSaveDirectory("log.txt");
            File.Delete(logPath);
#else
            // While debugging, keep all previous log data.
            logPath = gs.ResourceManager.GetSaveDirectory("log-debug.txt");
            File.Delete(logPath);
#endif
            using (StreamWriter w = File.AppendText(logPath))
            {
                w.WriteLine("==== ==== ==== ==== ====");
                w.WriteLine(gs.ResourceManager.GameName);
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            }

            started = true;
        }

        /// <summary>
        /// Stops logging and prints a final message. To be used when the program is closing.
        /// </summary>
        public void EndLog()
        {
            if (!started) StartLog();

            using (StreamWriter w = File.AppendText(logPath))
            {
                w.WriteLine("Closing program normally.");
            }

            started = false;
        }

        /// <summary>
        /// Log a text message to the log file and Visual Studio console.
        /// </summary>
        public void Log(string message)
        {
            if (!started) StartLog();

            Debug.WriteLine(message);

            using (StreamWriter w = File.AppendText(logPath))
            {
                w.WriteLine(message);
            }
        }

        /// <summary>
        /// Log a warning message to the log file and Visual Studio console.
        /// </summary>
        public void LogWarning(string message)
        {
            Log("WARNING: " + message);
        }

        /// <summary>
        /// Log an error message to the log file and Visual Studio console.
        /// </summary>
        public void LogError(string message)
        {
            Log("ERROR: " + message);
        }

        /// <summary>
        /// Log an error message to the log file and Visual Studio console, then forcibly shut the game down.
        /// </summary>
        public void LogErrorAndShutdown(string message)
        {
            Log("CRITICAL ERROR: " + message);
            Log("Forcibly terminating the program.");
            gs.ResourceManager.ClearTempDirectory();
#if DEBUG
            throw new Exception(message);
#endif
            // TODO: Consider that some platforms (not desktop) don't support the program terminating itself.
            // TODO: Should Game.Exit() be used instead?
            Environment.Exit(0);
        }

        /// <summary>
        /// Print a text message to the Visual Studio console.
        /// </summary>
        public void DebugPrint(string message)
        {
            Debug.WriteLine(message);
        }

        /// <summary>
        /// Print a text message to the Visual Studio console if it hasn't been printed previously.
        /// </summary>
        public void DebugPrintOnce(string id, string message)
        {
            if (onceIDs.Contains(id)) return;
            onceIDs.Add(id);
            Debug.WriteLine(message);
        }
    }
}

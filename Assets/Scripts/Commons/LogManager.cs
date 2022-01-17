using System;
using UnityEngine;

public class LogManager
{
    public static LogManager Singleton = new LogManager();

    private System.IO.StreamWriter logFile;

    private LogManager()
    {
        logFile = new System.IO.StreamWriter("log.txt", true, System.Text.Encoding.UTF8);
        logFile.AutoFlush = true;
        WriteLog("[LogManager] Logging Started...");
    }

    public void WriteLog(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#else
        try
        {
            logFile.WriteLine(message);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            logFile.Close();
        }
#endif
    }

    public void OnApplicationQuit()
    {
        logFile.Close();
    }
}

﻿using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;

public abstract class Terminal
{
    protected bool ProcessFailed()
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/AppcoinsUnity/Tools/ProcessError.out");
        string processLog = reader.ReadToEnd();
        reader.Close();

        if (processLog.Length != 0)
        {
            UnityEngine.Debug.LogError("CUSTOM BUILD ERROR:\n" + processLog);
            return true;
        }

        return false;
    }

    public ProcessStartInfo InitializeProcessInfo(string terminalPath)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo();
        processInfo.FileName = terminalPath;
        processInfo.WorkingDirectory = "/";
        processInfo.UseShellExecute = false;
        processInfo.CreateNoWindow = true;
        return processInfo;
    }

    public abstract void RunCommand(BuildStage stage, string cmd, string cmdOptions, string cmdArgs, string path, bool debugMode);

    public void RunTerminalCommand(BuildStage stage, string terminalPath, string cmd, string cmdOptions, string cmdArgs, bool debugMode)
    {
        RunCommand(stage, cmd, "", cmdArgs, ".", debugMode);
    }
}

public class Bash : Terminal
{
    private void RunBashCommand(string terminalPath, BuildStage stage, string cmd, string cmdOptions, string cmdArgs, string path, bool debugMode)
    {
        bool GUI = false;

        cmd = "'" + cmd + "'";

        if (cmdArgs.Equals(""))
        {
            cmdArgs = cmdOptions;
        }

        else
        {
            cmdArgs = cmdOptions.Equals("") ? "'" + cmdArgs + "'" : cmdOptions + " '" + cmdArgs + "'";
        }

        path = "'" + path + "'";

        ProcessStartInfo processInfo = InitializeProcessInfo(terminalPath);
        processInfo.CreateNoWindow = false;

        if (terminalPath.Equals("/bin/bash"))
        {
            processInfo.Arguments = "-c \"'" + Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh'\"";
        }

        else
        {
            processInfo.Arguments = "'" + Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh'";
            GUI = true;
        }

        CreateSHFileToExecuteCommand(stage, cmd, cmdArgs, path, debugMode, GUI);

        Process execScript = Process.Start("/bin/bash", "-c \"chmod +x '" + Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh'\"");
        execScript.WaitForExit();


        Process newProcess = new Process();
        newProcess.StartInfo = processInfo;
        newProcess.Start();

        //For the process to complete we check with, 5s interval, for the existence of ProcessCompleted.out
        bool fileExists;
        bool condition;
        do
        {
            fileExists = File.Exists(Application.dataPath + "/AppcoinsUnity/Tools/ProcessCompleted.out");
            condition = !fileExists;
            Thread.Sleep(2000);
        }
        while (condition);

        //Now we can safely kill the process
        if (!newProcess.HasExited)
        {
            newProcess.Kill();
        }

        if (ProcessFailed() == true)
        {
            throw new TerminalProcessFailedException();
        }
    }

    public override void RunCommand(BuildStage stage, string cmd, string cmdOptions, string cmdArgs, string path, bool debugMode)
    {
        string terminalPath = null;
        int version = -1;
        int.TryParse(Application.unityVersion.Split('.')[0], out version);

        if (Directory.Exists("/Applications/Utilities/Terminal.app") && version > 5)
        {
            terminalPath = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
        }

        else
        {
            terminalPath = "/bin/bash";
        }

        RunBashCommand(terminalPath, stage, cmd, cmdOptions, cmdArgs, path, debugMode);
    }

    //This creates a bash file that gets executed in the specified path
    protected void CreateSHFileToExecuteCommand(BuildStage stage, string cmd, string cmdArgs, string path, bool debugMode, bool GUI)
    {
        if(!Directory.Exists(Application.dataPath + "/AppcoinsUnity/Tools"))
        {
            Directory.CreateDirectory(Application.dataPath + "/AppcoinsUnity/Tools");
        }
        
        else
        {
            // Delete all temporary files.
            if (File.Exists(Application.dataPath + "/AppcoinsUnity/Tools/ProcessCompleted.out"))
            {
                File.Delete(Application.dataPath + "/AppcoinsUnity/Tools/ProcessCompleted.out");
            }

            if (File.Exists(Application.dataPath + "/AppcoinsUnity/Tools/ProcessError.out"))
            {
                File.Delete(Application.dataPath + "/AppcoinsUnity/Tools/stderr.out");
            }

            if (File.Exists(Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.bat"))
            {
                File.Delete(Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.bat");
            }

            if (File.Exists(Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh"))
            {
                File.Delete(Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh");
            }
        }

        StreamWriter writer = new StreamWriter(Application.dataPath + "/AppcoinsUnity/Tools/BashCommand.sh", false);

        writer.WriteLine("#!/bin/sh");

        //Put terminal as first foreground application
        if (GUI)
        {
            writer.WriteLine("osascript -e 'activate application \"/Applications/Utilities/Terminal.app\"'");
        }

        writer.WriteLine("cd " + path);
        //writer.WriteLine(cmd);
        if (stage == BuildStage.PROJECT_INSTALL || stage == BuildStage.PROJECT_RUN)
        {
            writer.WriteLine("if [ \"$(" + cmd + " get-state)\" == \"device\" ]\nthen");
        }

        // writer.WriteLine(cmd + " " + cmdArgs + " 2> '" + Application.dataPath + "/AppcoinsUnity/Tools/ProcessLog.out' | tee '" + Application.dataPath + "/AppcoinsUnity/Tools/ProcessLog.out'");
        writer.WriteLine(cmd + " " + cmdArgs + " 2>&1 2>'" + Application.dataPath + "/AppcoinsUnity/Tools/ProcessError.out'");

        if (stage == BuildStage.PROJECT_INSTALL || stage == BuildStage.PROJECT_RUN)
        {
            writer.WriteLine("else\necho error: no usb device found > '" + Application.dataPath + "/AppcoinsUnity/Tools/ProcessError.out'");
            writer.WriteLine("fi");
        }

        if (stage == BuildStage.PROJECT_BUILD && debugMode)
        {
            writer.WriteLine("read -p '\n\nPress enter to continue...'");
        }

        writer.WriteLine("echo 'done' > '" + Application.dataPath + "/AppcoinsUnity/Tools/ProcessCompleted.out'");
        writer.WriteLine("exit");
        // writer.WriteLine("osascript -e 'tell application \"Terminal\" to close first window'");
        writer.Close();
    }
}

public class CMD : Terminal
{
    protected static string TERMINAL_PATH = "cmd.exe";
    private static bool NO_GUI = false;

    public override void RunCommand(BuildStage stage, string cmd, string cmdOptions, string cmdArgs, string path, bool debugMode)
    {
        cmd = "\"" + cmd + "\"";

        if (cmdArgs.Equals(""))
        {
            cmdArgs = cmdOptions;
        }

        else
        {
            cmdArgs = cmdOptions.Equals("") ? "\"" + cmdArgs + "\"" : cmdOptions + " \"" + cmdArgs + "\"";
        }

        path = "\"" + path + "\"";

        CreateBatchFileToExecuteCommand(stage, cmd, cmdArgs, debugMode, path);

        ProcessStartInfo processInfo = InitializeProcessInfo(TERMINAL_PATH);
        processInfo.CreateNoWindow = NO_GUI;
        processInfo.UseShellExecute = true;

        processInfo.Arguments = "/c \"" + Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.bat\"";

        Process newProcess = Process.Start(processInfo);

        bool fileExists;
        bool condition;
        do
        {
            fileExists = File.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessCompleted.out");
            condition = !fileExists;
            Thread.Sleep(2000);
        }
        while (condition);

        //Now we can safely kill the process
        if (!newProcess.HasExited)
        {
            newProcess.Kill();
        }

        if (ProcessFailed() == true)
        {
            throw new TerminalProcessFailedException();
        }
    }

    private void CreateBatchFileToExecuteCommand(BuildStage stage, string cmd, string cmdArgs, bool debugMode, string path)
    {
        
        if(!Directory.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools"))
        {
            Directory.CreateDirectory(Application.dataPath + "\\AppcoinsUnity\\Tools");
        }
        
        else
        {
            // Delete all temporary files.
            if (File.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessCompleted.out"))
            {
                File.Delete(Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessCompleted.out");
            }

            if (File.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessError.out"))
            {
                File.Delete(Application.dataPath + "\\AppcoinsUnity\\Tools\\stderr.out");
            }

            if (File.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.bat"))
            {
                File.Delete(Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.bat");
            }

            if (File.Exists(Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.sh"))
            {
                File.Delete(Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.sh");
            }
        }

        StreamWriter writer = new StreamWriter(Application.dataPath + "\\AppcoinsUnity\\Tools\\BashCommand.bat", false);

        writer.WriteLine("cd " + path);

        if (stage == BuildStage.PROJECT_INSTALL || stage == BuildStage.PROJECT_RUN)
        {
            writer.WriteLine("set var=error");
            writer.WriteLine("for /f \"tokens=*\" %%a in ('" + cmd + " get-state') do set var=%%a");
            writer.WriteLine("if \"%var%\" == \"device\" (" + cmd + " " + cmdArgs + " 2>\"" + Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessError.out\")");
            writer.WriteLine("if \"%var%\" == \"error\" ( echo error: no usb device found >\"" + Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessError.out\")");
        }

        else
        {
            writer.WriteLine("call " + cmd + " " + cmdArgs + " 2>\"" + Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessError.out\"");
        }

        if (stage == BuildStage.PROJECT_BUILD && debugMode)
        {
            writer.WriteLine("set /p DUMMY=Press ENTER to continue...");
        }

        writer.WriteLine("echo done >\"" + Application.dataPath + "\\AppcoinsUnity\\Tools\\ProcessCompleted.out\"");
        writer.Close();
    }
}

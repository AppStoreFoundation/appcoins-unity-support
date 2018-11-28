using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public class Tools
{
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

    // Change a line in a file as much times as specified. 
    // file has the same format as a .gradle file
    public static void ChangeLineInFile(string filePath,
                                        string fileLine,
                                        string[] containers,
                                        string newLine
                                       )
    {
        IterateOverLinesAndContainers(
            filePath, ReadFileToArray(filePath),
            containers,
            delegate (IList<string> fileLines, int index) { },
            delegate (IList<string> fileLines, int index, bool insideContainer)
            {
                if (insideContainer && fileLines[index].Contains(fileLine))
                {
                    int tabsNum = GetLineTabsNumber(fileLines[index]);

                    fileLines[index] = String.Concat(
                        fileLines[index].Substring(0, tabsNum - 1),
                        newLine);
                }
            }
        );
    }

    public static void AddLinesInFile(string filePath, string[] containers,
                                     string[] lines)
    {
        IterateOverLinesAndContainers(
            filePath,
            new List<string>(ReadFileToArray(filePath)),
            containers,
            delegate (IList<string> fileLines, int index)
            {
                foreach (string line in lines)
                {
                    int tabsNum = GetLineTabsNumber(fileLines[index]);
                    string addLine = String.Copy(line);

                    for (int j = 0; j < tabsNum + 1; j++)
                    {
                        addLine = String.Concat("\t", addLine);
                    }

                    fileLines.Insert(index + 1, addLine);
                }
            },
            delegate (IList<string> fileLines, int index, bool isInsCont) { }
        );
    }

    public static void RemoveLineInFileWithSpecString(string filePath,
                                                      string[] checkers)
    {
        List<string> fileLines = new List<string>(ReadFileToArray(filePath));

        fileLines.RemoveAll(
            delegate (string line)
            {
                foreach (string checker in checkers)
                {
                    if (line.Contains(checker))
                    {
                        return true;
                    }
                }
                return false;
            }
        );

        WriteToFile(filePath, fileLines.ToArray());
    }

    private static void IterateOverLinesAndContainers(
        string filePath,
        IList<string> fileList,
        string[] containers,
        Action<IList<string>, int> doInsideCont,
        Action<IList<string>, int, bool> doPerLine
    )
    {
        const string endContString = "}";
        bool insideContainer = false;

        for (int i = 0; i < fileList.Count; i++)
        {
            foreach (string container in containers)
            {
                if (fileList[i].Contains(container))
                {
                    doInsideCont(fileList, i);
                    insideContainer = true;
                }
            }

            doPerLine(fileList, i, insideContainer);

            if (fileList[i].Contains(endContString))
            {
                insideContainer = false;
            }
        }

        string[] cpyArray = new string[fileList.Count];
        fileList.CopyTo(cpyArray, 0);
        WriteToFile(filePath, cpyArray);
    }

    private static int GetLineTabsNumber(string line)
    {
        // Retrive the number of tabs before the line just 
        // to maintain the same format (we assume that
        // only exists one 'command / definition' per line)
        int tabsNum = 0;
        while (Char.IsWhiteSpace(line[tabsNum++])) { }
        return tabsNum - 1;
    }

    public static string[] ReadFileToArray(string filePath)
    {
        StreamReader fileReader = new StreamReader(filePath);
        List<string> lines = new List<string>();

        string line;
        while ((line = fileReader.ReadLine()) != null)
        {
            lines.Add(line);
        }

        fileReader.Close();
        return lines.ToArray();
    }

    public static void WriteToFile(string filePath, string[] lines)
    {
        Write(new StreamWriter(filePath, false), lines);
    }

    public static void AppendToFile(string filePath, string[] lines)
    {
        Write(new StreamWriter(filePath, true), lines);
    }

    private static void Write(StreamWriter fileWriter,
                              string[] lines)
    {
        foreach (string line in lines)
        {
            fileWriter.WriteLine(line);
        }

        fileWriter.Close();
    }

    public static string SelectPath()
    {
        return EditorUtility.SaveFolderPanel(
            "Save Android Project to folder",
            "",
            ""
        );
    }

    public static string GetUnityProjectPath()
    {
        string projPath = Application.dataPath;

        int index = projPath.LastIndexOf('/');
        projPath = projPath.Substring(0, index);

        return projPath;
    }

    // If folder already exists in the chosen directory delete it.
    public static void DeleteIfFolderAlreadyExists(string path,
                                                   string folderName)
    {
        string[] folders = Directory.GetDirectories(path);

        for (int i = 0; i < folders.Length; i++)
        {
            if ((new DirectoryInfo(folders[i]).Name).Equals(folderName))
            {
                DirectoryInfo di = new DirectoryInfo(folders[i]);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
    }

    public static Terminal GetTerminalByOS()
    {
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ||
            SystemInfo.operatingSystemFamily == OperatingSystemFamily.Linux)
        {
            return new Bash();
        }

        else if (SystemInfo.operatingSystemFamily ==
                 OperatingSystemFamily.Windows
                )
        {
            return new CMD();
        }

        return null;
    }

    //If path for app contains appName remove it
    public static string FixAppPath(string path, string AppName)
    {
        string fileName = Path.GetFileName(path);

        if (!fileName.Equals(AppName))
        {
            path = Path.GetDirectoryName(path) + "/" + AppName;
        }

        return path;
    }

    internal static void MergeMainTemplates(string baseFilePath,
                                     string fileToMergePath)
    {
        UnityEngine.Debug.Log("Copying mainTemplate.gradle");

        if (File.Exists(baseFilePath))
        {
            if (PerformAutomaticMerge(baseFilePath))
            {
                Tree<string> tCurrent = Tree<string>.CreateTreeFromFile(baseFilePath, FileParser.BUILD_GRADLE);

                Tree<string> tAppcoins = Tree<string>.CreateTreeFromFile(fileToMergePath, FileParser.BUILD_GRADLE);

                tCurrent.MergeTrees(tAppcoins);

                Tree<string>.CreateFileFromTree(tCurrent,
                                                baseFilePath,
                                                  false,
                                                  FileParser.BUILD_GRADLE);
            }
        }
        else
        {
            File.Copy(fileToMergePath, baseFilePath, true);
        }
    }

    private static bool PerformAutomaticMerge(string filePath)
    {
        string checker = "// DONT PERFORM AUTOMATIC MERGE";

        string[] fileLines = ReadFileToArray(filePath);

        foreach (string line in fileLines)
        {
            if (line.Contains(checker))
            {
                return false;
            }
        }

        return true;
    }
}
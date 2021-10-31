/*
 Welcome to the source code of SazSorter V2. Free for use, not like anyone should own any code anyways.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SazSorter
{
    class Program
    {
        static string dateToday = DateTime.Now.ToString("yyyy_MM_dd");

        enum OptionSortingOption
        {
            Copy,
            Move
        };

        enum OptionListBeforeSort
        {
            ListAllFilePaths,
            ListFileExtensions,
            DontListAnything
        };

        enum OptionSortingType
        {
            Default,
            Extension,
            Alphabetically
        };

        static OptionSortingOption userOptionSortingOption = OptionSortingOption.Copy;
        static OptionListBeforeSort userOptionListBeforeSort = OptionListBeforeSort.ListAllFilePaths;
        static OptionSortingType userOptionSortingType = OptionSortingType.Default;

        static bool startSorting = false;

        static int filesFoundOnScan = 0;
        static long totalFileSizeFoundOnScan = 0; // in bytes

        static string dirCurrent = Directory.GetCurrentDirectory();
        static string dirRandomFiles = dirCurrent + "\\" + dateToday + "_SazSorter_RandomFiles";
        static string dirSortedFiles = dirCurrent + "\\" + dateToday + "_SazSorter_SortedFiles";

        static FileStream logFileStream = new FileStream(dirCurrent + "\\SazSorterLog.log", FileMode.Append);
        static StreamWriter logWriter = new StreamWriter(logFileStream);

        static List<string> fileExtensions = new List<string>();

        static void WriteLineWithLogging(string text)
        {
            logWriter.WriteLine(text);
            Console.WriteLine(text);
        }

        static string ReadLineWithLogging()
        {
            string input = Console.ReadLine();
            logWriter.WriteLine(input);
            return input;
        }

        static void Main(string[] args)
        {
            logWriter.WriteLine("==--==--= START =--==--==");
            logWriter.WriteLine(DateTime.Now);
            /////////////////////////////////////////////////

            WriteLineWithLogging("SazSorter started up!");

            UserSelectPrintSortingOptions();
            InitialDirectoryCheck();
            UserSelectScanFilesBeforeSorting();
            UserSelectSortingType();

            if (filesFoundOnScan > 0)
                UserSelectSortFiles();
            else
            {
                WriteLineWithLogging("\nNo files found in: " + dirRandomFiles);
                WriteLineWithLogging("Give me some files next time. Then we will talk.");
            }
            WriteLineWithLogging("Logs are saved to the SazSorterLog.log");
            WriteLineWithLogging("\nPress any key to exit.");

            /////////////////////////////////////////////////
            logWriter.WriteLine(DateTime.Now);
            logWriter.WriteLine("==--==--= END =--==--==\n\n\n\n");
            logWriter.Close();
            logFileStream.Close();
            Console.ReadKey();
        }

        #region User option based
        static void UserSelectPrintSortingOptions()
        {
            do
            {
                WriteLineWithLogging("\nWhich option for sorting would you like?");
                WriteLineWithLogging("1 - copy the files to the sorted folder. Creates a copy of the original file to the sorted location.");
                WriteLineWithLogging("2 - move the files to the sorted folder. Moves the original file to the sorted location.");
                WriteLineWithLogging("!-!-!-!\n" + "NOTE: For new users, I would recommend choosing the copy option as it keeps your original files untouched.\n!-!-!-!");
                WriteLineWithLogging("After typing out the option 1 or 2, press enter.");

                string usrAnswer = ReadLineWithLogging();

                if (usrAnswer == "1")
                {
                    userOptionSortingOption = OptionSortingOption.Copy;
                    break;
                }
                else if (usrAnswer == "2")
                {
                    userOptionSortingOption = OptionSortingOption.Move;
                    break;
                }
                WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);
        }

        static void UserSelectScanFilesBeforeSorting()
        {
            do
            {
                WriteLineWithLogging("\nBefore the sorting process, scan of files is needed. Select what output you want to see?");
                WriteLineWithLogging("1 = List all files that will be in the sorting process.");
                WriteLineWithLogging("2 = Don't list anything, just scan.");
                WriteLineWithLogging("3 = List the extension of files only (example: .txt | .png | .mp4).");
                WriteLineWithLogging("After typing out the option 1, 2 or 3, press enter.");

                string usrAnswer = ReadLineWithLogging();
                if (usrAnswer == "1")
                {
                    userOptionListBeforeSort = OptionListBeforeSort.ListAllFilePaths;
                    ScanFilesBeforeSorting();
                    break;
                }
                else if (usrAnswer == "2")
                {
                    userOptionListBeforeSort = OptionListBeforeSort.DontListAnything;
                    ScanFilesBeforeSorting();
                    break;
                }
                else if (usrAnswer == "3")
                {
                    userOptionListBeforeSort = OptionListBeforeSort.ListFileExtensions;
                    ScanFilesBeforeSorting();
                    break;
                }
                else
                    WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);
        }

        static void UserSelectSortingType()
        {
            do
            {
                WriteLineWithLogging("\nWhat type of sorting?");
                WriteLineWithLogging("1 = Default: from random folder to sorted folder.");
                WriteLineWithLogging("2 = Extension based: create folder for each extension in the sorted folder." +
                    "\n    Examples: " +
                    "\n      lamb.sauce will be moved to folder 'sauce'" +
                    "\n      Picture.png will be moved to folder 'png'");
                WriteLineWithLogging("3 = Alphabetically based: create folder based on the first letter of the file name." +
                    "\n    Examples: " +
                    "\n      Input.csproj will be moved to folder 'I'" +
                    "\n      Settings.data will be moved to folder 'S'");
                WriteLineWithLogging("After typing out the option 1, 2 or 3 press enter.");

                string usrAnswer = ReadLineWithLogging();
                if (usrAnswer == "1")
                {
                    userOptionSortingType = OptionSortingType.Default;
                    break;
                }
                else if (usrAnswer == "2")
                {
                    userOptionSortingType = OptionSortingType.Extension;
                    break;
                }
                else if (usrAnswer == "3")
                {
                    userOptionSortingType = OptionSortingType.Alphabetically;
                    break;
                }
                else
                    WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);
        }

        static void UserSelectSortFiles()
        {
            do
            {
                WriteLineWithLogging("\nReady to sort the files?");
                WriteLineWithLogging("1 = yes");
                WriteLineWithLogging("2 = no");
                WriteLineWithLogging("After typing out the option 1 or 2, press enter.");

                string usrAnswer = ReadLineWithLogging();
                if (usrAnswer == "1")
                {
                    WriteLineWithLogging("\n\n====SORTING STARTED====\n");
                    startSorting = true;
                    TraverseDirectory(new DirectoryInfo(dirRandomFiles));
                    WriteLineWithLogging("\n====SORTING FINISHED====\n\n");
                    break;
                }
                else if (usrAnswer == "2")
                    break;
                else
                    WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);
        }
        #endregion

        static void InitialDirectoryCheck()
        {
            bool warnUser = false; // on random files dir creation, warn user to throw in files
            WriteLineWithLogging("\nChecking required folders.");

            if (!Directory.Exists(dirRandomFiles))
            {
                WriteLineWithLogging("Folder not found. Creating folder: " + dirRandomFiles);
                Directory.CreateDirectory(dirRandomFiles);
                warnUser = true;
            }
            else 
                WriteLineWithLogging("Found folder: " + dirRandomFiles);

            if (!Directory.Exists(dirSortedFiles))
            {
                WriteLineWithLogging("Folder not found. Creating folder: " + dirSortedFiles);
                Directory.CreateDirectory(dirSortedFiles);
            }
            else 
                WriteLineWithLogging("Found folder: " + dirSortedFiles); 

            WriteLineWithLogging("Check done.");

            if (warnUser)
            {
                WriteLineWithLogging("\n!=!=!=!=!=!=!=!" +
                    "\nPlease put in your files in the " + dirRandomFiles + " folder before continuing with the next step!" +
                    "\n!=!=!=!=!=!=!=!\n");
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(600);
                    Console.Write(".");
                }
                Thread.Sleep(800);
                Console.WriteLine("");
            }
        }

        static void ScanFilesBeforeSorting()
        {
            if (userOptionListBeforeSort == OptionListBeforeSort.ListAllFilePaths || userOptionListBeforeSort == OptionListBeforeSort.ListFileExtensions)
            {
                if (userOptionListBeforeSort == OptionListBeforeSort.ListAllFilePaths)
                    WriteLineWithLogging("\n\nThe following list will contain all the files that will be in the sorting process.");
                else if (userOptionListBeforeSort == OptionListBeforeSort.ListFileExtensions)
                    WriteLineWithLogging("\n\nThe following list will contain all the files that will be in the sorting process.");

                WriteLineWithLogging("==START==");
                TraverseDirectory(new DirectoryInfo(dirRandomFiles));
                WriteLineWithLogging("==END==");
            }
            else
                TraverseDirectory(new DirectoryInfo(dirRandomFiles));

            if (filesFoundOnScan > 0)
            {
                WriteLineWithLogging("Found '" + filesFoundOnScan + "' files to sort.");
                WriteLineWithLogging("Total size of " + ConvertBytesToMegabytes(totalFileSizeFoundOnScan).ToString("00.0") + " megabytes to sort.");
            }

            WriteLineWithLogging("\n");
        }

        static void TraverseDirectory(DirectoryInfo directoryInfo)
        {
            IEnumerable<DirectoryInfo> subdirectories = directoryInfo.EnumerateDirectories();

            foreach (DirectoryInfo subdirectory in subdirectories)
                TraverseDirectory(subdirectory);

            IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles();

            foreach (FileInfo file in files)
                HandleFile(file);
        }

        static void HandleFile(FileInfo file)
        {
            if (file.Extension == "._") // what the hell is this???
                return;

            if (startSorting)
            {
                bool applyDupePrefix = false;
                int fileSortSequenceOnDuplicate = 0;

            TryAgain:
                try
                {
                    string dstSuffix = "\\" + file.Name;
                    string dstSuffixDupe = "\\" + fileSortSequenceOnDuplicate + "_" + file.Name;
                    string dstPath = dirSortedFiles;

                    if (userOptionSortingType == OptionSortingType.Extension || userOptionSortingType == OptionSortingType.Alphabetically)
                    {
                        if (userOptionSortingType == OptionSortingType.Extension)
                            dstPath += "\\" + file.Extension.Remove(0, 1);
                        else if (userOptionSortingType == OptionSortingType.Alphabetically)
                            dstPath += "\\" + file.Name[0].ToString().ToLower();

                        if (!Directory.Exists(dstPath))
                        {
                            Directory.CreateDirectory(dstPath);
                            WriteLineWithLogging("Creating directory: " + dstPath);
                        }
                    }

                    if (applyDupePrefix)
                        dstPath += dstSuffixDupe;
                    else
                        dstPath += dstSuffix;

                    if (userOptionSortingOption == OptionSortingOption.Copy)
                    {
                        if (applyDupePrefix)
                        {
                            fileSortSequenceOnDuplicate++;
                            WriteLineWithLogging("(DUPLICATE! '" + fileSortSequenceOnDuplicate + "' PREFIX APPLIED) Copying'" + file.FullName + " to " + dstPath);
                        }
                        else
                        {
                            WriteLineWithLogging("Copying '" + file.FullName + " to " + dstPath);
                        }

                        file.CopyTo(dstPath);
                    }
                    else
                    {
                        if (applyDupePrefix)
                        {
                            fileSortSequenceOnDuplicate++;
                            WriteLineWithLogging("(DUPLICATE! '" + fileSortSequenceOnDuplicate + "' PREFIX APPLIED) Moving'" + file.FullName + " to " + dstPath);
                        }
                        else
                        {
                            WriteLineWithLogging("Moving '" + file.FullName + " to " + dstPath);
                        }

                        file.MoveTo(dstPath);
                    }
                }
                catch
                {
                    applyDupePrefix = true;
                    goto TryAgain;
                }
            }
            else
            {
                if (userOptionListBeforeSort == OptionListBeforeSort.ListAllFilePaths)
                {
                    WriteLineWithLogging("Found file: " + file.FullName);
                }
                else if (userOptionListBeforeSort == OptionListBeforeSort.ListFileExtensions)
                {
                    if (!fileExtensions.Contains(file.Extension))
                    {
                        fileExtensions.Add(file.Extension);
                        WriteLineWithLogging("Found extension: " + file.Extension);
                    }
                }

                filesFoundOnScan++;
                totalFileSizeFoundOnScan += file.Length;
            }
        }

        static double ConvertBytesToMegabytes(long bytes) => (bytes / 1024f) / 1024f;
    }
}
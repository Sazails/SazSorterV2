using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SazSorter
{
    class Program
    {
        static string dateToday = DateTime.Now.ToString("yyyy_MM_dd");
        static bool optionCopy = true;
        static bool seeFilesBeforeScan = false;
        static int filesFoundOnScan = 0;
        static long filesFoundOnScanSize = 0;
        static bool startSorting = false;

        static string dirCurrent = Directory.GetCurrentDirectory();
        static string dirRandomFiles = dirCurrent + "\\" + dateToday + "_SazSorter_RandomFiles";
        static string dirSortedFiles = dirCurrent + "\\" + dateToday + "_SazSorter_SortedFiles";

        static FileStream logFileStream = new FileStream(dirCurrent + "\\SazSorterLog.log", FileMode.Append);
        static StreamWriter logWriter = new StreamWriter(logFileStream);

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

            WriteLineWithLogging("SazSorter started up!");

            PrintSortingOptions();
            InitialDirectoryCheck();
            do
            {
                WriteLineWithLogging("\nWould you like to see all the files that will be sorted?");
                WriteLineWithLogging("1 = yes");
                WriteLineWithLogging("2 = no");
                WriteLineWithLogging("After typing out the option 1 or 2, press enter.");

                string usrAnswer = ReadLineWithLogging();
                if (usrAnswer == "1")
                {
                    seeFilesBeforeScan = true;
                    ScanFilesBeforeSorting();
                    break;
                }
                else if (usrAnswer == "2")
                {
                    seeFilesBeforeScan = false;
                    ScanFilesBeforeSorting();
                    break;
                }
                else
                    WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);

            if (filesFoundOnScan > 0)
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
            else
            {
                WriteLineWithLogging("\nNo files found in: " + dirRandomFiles);
                WriteLineWithLogging("Give me some files next time. Then we will talk.");
            }
            WriteLineWithLogging("Logs are saved to the SazSorterLog.log");
            WriteLineWithLogging("\nPress any key to exit.");

            logWriter.WriteLine(DateTime.Now);
            logWriter.WriteLine("==--==--= END =--==--==\n\n\n\n");
            logWriter.Close();
            logFileStream.Close();
            Console.ReadKey();
        }

        static void PrintSortingOptions()
        {
            do
            {
                WriteLineWithLogging("\nWhich option for sorting would you like?");
                WriteLineWithLogging("1 - copy the files to the sorted folder. Creates a copy of the original file to the sorted location.");
                WriteLineWithLogging("2 - move the files to the sorted folder. Moves the original file to the sorted location.");
                WriteLineWithLogging("After typing out the option 1 or 2, press enter.");

                string usrAnswer = ReadLineWithLogging();

                if (usrAnswer == "1")
                {
                    optionCopy = true;
                    break;
                }
                else if (usrAnswer == "2")
                {
                    optionCopy = false;
                    break;
                }
                WriteLineWithLogging("Invalid option selected. Try again.");
            } while (true);
        }

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
            else { WriteLineWithLogging("Found folder: " + dirRandomFiles); }

            if (!Directory.Exists(dirSortedFiles))
            {
                WriteLineWithLogging("Folder not found. Creating folder: " + dirSortedFiles);
                Directory.CreateDirectory(dirSortedFiles);
            }
            else { WriteLineWithLogging("Found folder: " + dirSortedFiles); }
            WriteLineWithLogging("Check done.");

            if (warnUser)
            {
                WriteLineWithLogging("\n!=!=!=!=!=!=!=!" +
                    "\nPlease put in your files in the " + dirRandomFiles + " folder before continuing with the next step!" +
                    "\n!=!=!=!=!=!=!=!\n");
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(1000);
                    Console.Write(".");
                }
                Thread.Sleep(1000);
                Console.WriteLine("");
            }
        }

        static void ScanFilesBeforeSorting()
        {
            if (seeFilesBeforeScan)
            {
                WriteLineWithLogging("\n\nThe following list will contain all the files that will be in the sorting process.");
                WriteLineWithLogging("==START==");
                TraverseDirectory(new DirectoryInfo(dirRandomFiles));
                WriteLineWithLogging("==END==");
            }
            else TraverseDirectory(new DirectoryInfo(dirRandomFiles));

            if (filesFoundOnScan > 0)
            {
                WriteLineWithLogging("Found '" + filesFoundOnScan + "' files to sort.");
                WriteLineWithLogging("Total size of " + ConvertBytesToMegabytes(filesFoundOnScanSize).ToString("00.0") + " megabytes to sort.");
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
            if (startSorting)
            {
                bool applyDupePrefix = false;
                // For sorting
                if (optionCopy)
                {
                    int fileSortSequenceOnDuplicate = 0;
                TryAgain:
                    try
                    {
                        if (applyDupePrefix)
                        {
                            fileSortSequenceOnDuplicate++;
                            file.CopyTo(dirSortedFiles + "\\" + fileSortSequenceOnDuplicate + "_" + file.Name);
                            WriteLineWithLogging("(DUPLICATE! '" + fileSortSequenceOnDuplicate + "' PREFIX APPLIED) Copying'" + file.FullName + " to " + dirSortedFiles + "\\" + fileSortSequenceOnDuplicate + "_" + file.Name);
                        }
                        else
                        {
                            file.CopyTo(dirSortedFiles + "\\" + file.Name);
                            WriteLineWithLogging("Copying '" + file.FullName + " to " + dirSortedFiles + "\\" + file.Name);
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
                    int fileSortSequenceOnDuplicate = 0;
                TryAgain:
                    try
                    {
                        if (applyDupePrefix)
                        {
                            fileSortSequenceOnDuplicate++;
                            file.MoveTo(dirSortedFiles + "\\" + fileSortSequenceOnDuplicate + "_" + file.Name);
                            WriteLineWithLogging("(DUPLICATE! '" + fileSortSequenceOnDuplicate + "' PREFIX APPLIED) Moving'" + file.FullName + " to " + dirSortedFiles + "\\" + fileSortSequenceOnDuplicate + "_" + file.Name);
                        }
                        else
                        {
                            file.MoveTo(dirSortedFiles + "\\" + file.Name);
                            WriteLineWithLogging("Moving '" + file.FullName + " to " + dirSortedFiles + "\\" + file.Name);
                        }
                    }
                    catch
                    {
                        applyDupePrefix = true;
                        goto TryAgain;
                    }
                }
            }
            else
            {
                if (seeFilesBeforeScan)
                {
                    // For displaying the file list before sorting
                    WriteLineWithLogging("Found file: " + file.FullName);
                }

                filesFoundOnScan++;
                filesFoundOnScanSize += file.Length;
            }
        }   
        
        static double ConvertBytesToMegabytes(long bytes) => (bytes / 1024f) / 1024f;
    }
}
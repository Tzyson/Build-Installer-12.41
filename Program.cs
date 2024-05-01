using System;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace FileDownloader
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetClassLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        const int GCL_HICON = -14;

        static void Main(string[] args)
        {
            IntPtr consoleHandle = GetConsoleWindow();

            IntPtr iconHandle = LoadIcon("blend.ico");

            SetClassLong(consoleHandle, GCL_HICON, (uint)iconHandle);
            Console.WriteLine("      >>>  Blend 12.41 Build Installer - Made by ZarX <<<");
            Console.WriteLine("");
            Console.WriteLine("Path where to download: ");
            string destinationDirectory = Console.ReadLine();

            if (!Directory.Exists(destinationDirectory))
            {
                Console.WriteLine("Specified Folder Path does not exist.");
                return;
            }

            string destinationPath = Path.Combine(destinationDirectory, "12.41.zip");

            bool downloadSuccess = DownloadFileWithProgressBar("https://cdn.fnbuilds.services/Fortnite%2012.41.zip", destinationPath); // Replace the URL with your specified URL

            if (downloadSuccess)
            {
                Console.WriteLine("Download complete!");
                Console.WriteLine("Enjoy Blend!");
                FindAndRunBlendLauncher();
            }
            else
            {
                Console.WriteLine("Download failed. Check your internet connection and try again later.");
            }

            Console.ReadKey();
        }

        static bool DownloadFileWithProgressBar(string fileUrl, string destinationPath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    bool downloadCompleted = false;
                    bool downloadFailed = false;

                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        double progress = (double)e.BytesReceived / e.TotalBytesToReceive * 100;
                        Console.Write($"\rDownloading... {progress:F2}% Complete: [{new string('=', (int)(progress / 2))}{new string(' ', 50 - (int)(progress / 2))}]");
                    };

                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        downloadCompleted = true;
                    };

                    client.DownloadFileAsync(new Uri(fileUrl), destinationPath);

                    while (!downloadCompleted && !downloadFailed)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        {
                            downloadFailed = true;
                        }
                    }

                    if (downloadFailed)
                    {
                        client.CancelAsync();
                        File.Delete(destinationPath);
                        return false;
                    }
                }

                ExtractAndDeleteZipFile(destinationPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        static void ExtractAndDeleteZipFile(string filePath)
        {
            string extractPath = Path.GetDirectoryName(filePath);

            try
            {
                ZipFile.ExtractToDirectory(filePath, extractPath);
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static void FindAndRunBlendLauncher()
        {
            string[] files = Directory.GetFiles(@"C:\", "BlendLauncher.exe", SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                string blendLauncherPath = files[0];
                Console.WriteLine($"BlendLauncher found at: {blendLauncherPath}");

                try
                {
                    System.Diagnostics.Process.Start(blendLauncherPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to find and run BlendLauncher: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("BlendLauncher not found on the system.");
            }
        }
        static IntPtr LoadIcon(string fileName)
        {
            // Load the icon file
            IntPtr iconHandle = IntPtr.Zero;
            if (!string.IsNullOrEmpty(fileName))
            {
                iconHandle = LoadImage(IntPtr.Zero, fileName, 1, 0, 0, 0x00000010 | 0x00000001);
            }
            return iconHandle;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    }
}

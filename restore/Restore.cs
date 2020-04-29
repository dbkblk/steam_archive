using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace git_steam_archive
{
    class Restore
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Steam restorer - Select the game to restore:");

            // Découverte du dossier d'installation de 7-zip
            string zip = "";
            if (File.Exists("C:\\Program Files\\7-Zip\\7zG.exe")) zip = "C:\\Program Files\\7-Zip\\7zG.exe";
            else if (File.Exists("C:\\Program Files (x86)\\7-Zip\\7zG.exe")) zip = "C:\\Program Files (x86)\\7-Zip\\7zG.exe";

            // Lecture des dossiers Steam
            var content = File.ReadAllLines("steamapps.txt").Where(a => !string.IsNullOrEmpty(a)).ToList();

            bool abort = false;
            foreach(var dir in content)
            {
                if (!Directory.Exists(dir))
                {
                    Console.WriteLine($"The path {dir} specified in steamapps.txt, doesn't exist.");
                    abort = true;
                }
            }
            if (abort) throw new Exception("Please review steamapps.txt paths before to continue.");

            // Liste des archives
            var archives = Directory.GetFiles("backups/", "*.7z").OrderBy(a => a).ToList();

            int nValue = 0;

            foreach (var arc in archives)
            {
                nValue++;
                var file = new FileInfo(arc);
                Console.WriteLine($"[{nValue}] {file.Name} ({GetSizeSuffix(file.Length)})");
            }

            // Attente d'une réponse
            var value = Console.ReadLine();

            try
            {
                var selectedGame = new FileInfo(archives[Int32.Parse(value) - 1]).Name;

                // Selection du dossier d'extraction
                Console.WriteLine($"-- The selected archive is {selectedGame}. Where do you want to install it? --");

                int nPath = 0;
                foreach(var c in content)
                {
                    nPath++;
                    Console.WriteLine($"[{nPath}] {c}");
                }
                Console.WriteLine("[Other] Cancel.");
                var pathValue = Console.ReadLine();
                var selectedPath = content[Int32.Parse(pathValue) - 1];

                // Préparation de la commande
                var script = new List<string>() { $"\"{zip}\" x \"{Path.Combine(Directory.GetCurrentDirectory(), "backups", selectedGame)}\" -o\"{selectedPath}\"" };

                if (!Directory.Exists("scripts")) Directory.CreateDirectory("scripts");

                File.WriteAllLines(Path.Combine("scripts", "restore.bat"), script);

                // Lancement en priorité basse
                var parent = Process.GetCurrentProcess();
                parent.PriorityClass = ProcessPriorityClass.Idle;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Path.Combine("scripts", "restore.bat");
                process.StartInfo = startInfo;
                process.Start();
            }
            catch
            {
                Console.WriteLine("The entered number is invalid.");
            }
        }

        // Retourne le suffixe correspondant à la taille : https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        static readonly string[] SizeSuffixes =
                  { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string GetSizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + GetSizeSuffix(-value); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }
    }    
}

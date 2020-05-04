using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace steam_tools
{
    class Archive
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Steam archiver - Select the game to archive: {Environment.NewLine}");

            // Découverte du dossier d'installation de 7-zip
            string zip = "";
            if (File.Exists("C:\\Program Files\\7-Zip\\7zG.exe")) zip = "C:\\Program Files\\7-Zip\\7zG.exe";
            else if (File.Exists("C:\\Program Files (x86)\\7-Zip\\7zG.exe")) zip = "C:\\Program Files (x86)\\7-Zip\\7zG.exe";

            // Expressions régulières
            Regex regID = new Regex("\"appid\"\t\t\"(.*)\"");
            Regex regName = new Regex("\"name\"\t\t\"(.*)\"");
            Regex regInstallDir = new Regex("\"installdir\"\t\t\"(.*)\"");
            Regex regSize = new Regex("\"SizeOnDisk\"\t\t\"(.*)\"");

            // Lecture des dossiers Steam
            var content = File.ReadAllLines("steamapps.txt");            

            var games = new List<Game>();
            foreach(var p in content.Where(a => !string.IsNullOrEmpty(a)))
            {
                // Test du dossier
                if(!Directory.Exists(p))
                {
                    Console.WriteLine($"Cannot reach path: {p}.");
                    continue;
                }

                // Enumération des fichiers ACF
                var acf = Directory.GetFiles(p, "*.acf");
                foreach(var a in acf)
                {
                    // Lecteur de l'information
                    var fileContent = new List<string>(File.ReadAllLines(a));

                    var app = new Game();
                    app.LibPath = p;
                    app.AcfPath = Path.GetFileName(a);

                    foreach(var line in fileContent)
                    {
                        if (regID.IsMatch(line)) app.AppID = Int32.Parse(regID.Match(line).Groups[1].ToString());
                        else if (regName.IsMatch(line)) app.Name = regName.Match(line).Groups[1].ToString().Replace(":", " -").Replace("®", "").Replace("™", "");
                        else if (regInstallDir.IsMatch(line)) app.Path = regInstallDir.Match(line).Groups[1].ToString();
                        else if (regSize.IsMatch(line)) app.Size = long.Parse(regSize.Match(line).Groups[1].ToString());
                    }

                    games.Add(app);
                }

                // Enumération des fichiers Workshop
                var workAcf = Directory.GetFiles(p, "workshop/*.acf");
                foreach(var a in workAcf)
                {
                    int id = 0;
                    long size = 0;

                    // Lecteur de l'information
                    var fileContent = new List<string>(File.ReadAllLines(a));
                    foreach (var line in fileContent)
                    {
                        if (regID.IsMatch(line)) id = Int32.Parse(regID.Match(line).Groups[1].ToString());
                        else if (regSize.IsMatch(line)) size = long.Parse(regSize.Match(line).Groups[1].ToString());
                    }

                    // Si un fichier a été trouvé
                    if(size > 0 && games.Any(b => b.AppID == id))
                    {
                        var game = games.First(b => b.AppID == id);
                        games.Remove(game);
                        game.WorkshopSize = size;
                        game.WorkshopAcfPath = Path.Combine("workshop", Path.GetFileName(a));
                        games.Add(game);
                    }
                }
            }

            // Liste des jeux
            int nValue = 0;
            games = games.OrderBy(a => a.Name).ToList();
            foreach (var game in games)
            {
                nValue++;
                Console.WriteLine($"[{nValue}] {game.Name} ({game.TotalSizeString}) [{game.LibPath}]");
            }

            // Attente
            var value = Console.ReadLine();
            try
            {
                var selectedGame = games[Int32.Parse(value)-1];

                Console.WriteLine($"-- The selected game is {selectedGame.Name}. Pick a choice: --");
                Console.WriteLine("[Y] Archive and delete the game files.");
                Console.WriteLine("[K] Archive and keep the game files installed.");
                Console.WriteLine("[Other] Cancel.");
                var key = Console.ReadLine().ToUpper();

                if (key == "Y" || key == "K")
                {
                    var paths = GetPaths(selectedGame);
                    var archiveName = $"{selectedGame.Name} ({DateTime.Now.ToString("yyyy-MM-dd")})";
                    //var script = new List<string> { $"{selectedGame.LibPath[0]}:", $"cd \"{selectedGame.LibPath}\"", $"7z a -t7z \"{Path.Combine(config.archivePath, archiveName)}\" \"{string.Join("\" \"", paths.ToArray())}\"" };
                    var script = new List<string> { 
                        $"{selectedGame.LibPath[0]}:", 
                        $"cd \"{selectedGame.LibPath}\"" };

                    script.Add($"\"{zip}\" a -t7z -mx=2 \"{Path.Combine(Directory.GetCurrentDirectory(), "backups", archiveName)}.7z\" \"{string.Join("\" \"", paths.ToArray())}\"");

                    if (key == "Y")
                    {
                        foreach (var path in paths)
                        {
                            if(path.EndsWith(".acf"))
                            {
                                script.Add($"   DEL \"{path}\"");
                            }
                            else
                            {
                                script.Add($"   @RD /S /Q \"{path}\"");
                            }                            
                        }
                    }

                    if (!Directory.Exists("scripts")) Directory.CreateDirectory("scripts");

                    File.WriteAllLines(Path.Combine("scripts", $"archive_{selectedGame.AppID}.bat"), script);

                    // Lancement en priorité basse
                    var parent = Process.GetCurrentProcess();
                    parent.PriorityClass = ProcessPriorityClass.Idle;
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = Path.Combine("scripts", $"archive_{selectedGame.AppID}.bat");
                    process.StartInfo = startInfo;
                    process.Start();            
                }
            }
            catch
            {
                Console.WriteLine("The entered number is invalid.");
                Console.ReadLine();
            }
        }

        // Retourne les chemins à sauvegarder
        public static List<string> GetPaths(Game game)
        {
            var output = new List<string>();
            output.Add(Path.Combine("common", game.Path));
            output.Add(Path.Combine(game.AcfPath));
            if(!string.IsNullOrEmpty(game.WorkshopAcfPath))
            {
                output.Add(Path.Combine(game.WorkshopAcfPath));
                output.Add(Path.Combine("workshop", "content", game.AppID.ToString()));
            }

            return output;
        }
    }    
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace steam_tools
{
    class SteamClipboard
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Select a game, then paste files in Windows Explorer to copy the game files.");

            // Expressions régulières
            Regex regID = new Regex("\"appid\"\t\t\"(.*)\"");
            Regex regName = new Regex("\"name\"\t\t\"(.*)\"");
            Regex regInstallDir = new Regex("\"installdir\"\t\t\"(.*)\"");
            Regex regSize = new Regex("\"SizeOnDisk\"\t\t\"(.*)\"");

            // Lecture des dossiers Steam
            var content = File.ReadAllLines("steamapps.txt");

            var games = new List<Game>();
            foreach (var p in content.Where(a => !string.IsNullOrEmpty(a)))
            {
                // Test du dossier
                if (!Directory.Exists(p))
                {
                    Console.WriteLine($"Cannot reach path: {p}.");
                    continue;
                }

                // Enumération des fichiers ACF
                var acf = Directory.GetFiles(p, "*.acf");
                foreach (var a in acf)
                {
                    // Lecteur de l'information
                    var fileContent = new List<string>(File.ReadAllLines(a));

                    var app = new Game();
                    app.LibPath = p;
                    app.AcfPath = Path.GetFileName(a);

                    foreach (var line in fileContent)
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
                foreach (var a in workAcf)
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
                    if (size > 0 && games.Any(b => b.AppID == id))
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
                var selectedGame = games[Int32.Parse(value) - 1];
                var paths = GetPaths(selectedGame);
                Clipboard.SetFileDropList(paths);
                Console.WriteLine("The game files are now copied to the clipboard. Use Windows Explorer to paste them wherever you want.");
            }
            catch
            {
                Console.WriteLine("The entered number is invalid.");
                Console.ReadLine();
            }

            Console.ReadLine();
        }

        // Retourne les chemins à sauvegarder
        public static StringCollection GetPaths(Game game)
        {
            var output = new StringCollection();
            output.Add(Path.Combine(game.LibPath, "common", game.Path));
            output.Add(Path.Combine(game.LibPath, game.AcfPath));
            if (!string.IsNullOrEmpty(game.WorkshopAcfPath))
            {
                output.Add(Path.Combine(game.LibPath, game.WorkshopAcfPath));
                output.Add(Path.Combine(game.LibPath, "workshop", "content", game.AppID.ToString()));
            }

            return output;
        }
    }
}

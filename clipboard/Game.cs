using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace steam_tools
{
    public class Game
    {
        // Jeu
        public int AppID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string AcfPath { get; set; }
        public long Size { get; set; }
        public string LibPath { get; set; }
        public string TotalSizeString => GetSizeSuffix(Size + WorkshopSize);

        // Workshop
        public string WorkshopAcfPath { get; set; } = "";
        public long WorkshopSize { get; set; } = 0;
        public string WorkshopSizeString => GetSizeSuffix(WorkshopSize);


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

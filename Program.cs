using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace FA_gokart
{
    internal class Program
    {

        class Versenyzo
        {
            public string Vezeteknev;
            public string Keresztnev;
            public DateTime SzuletesiDatum;
            public bool Elmúlt18;
            public string VersenyzoID;
            public string Email;
        }

        class Foglalas
        {
            public DateTime Idopont;
            public string VersenyzoID = "";
        }

        static Random rnd = new Random();
        static List<Versenyzo> versenyzok = new List<Versenyzo>();
        static List<Foglalas> foglalasLista = new List<Foglalas>();

        static void GeneralVersenyzok()//versenyzők generálása
        {
            int n = rnd.Next(1, 151);

            var vezetekRaw = File.ReadAllText("vezeteknevek.txt", Encoding.UTF8);
            var keresztRaw = File.ReadAllText("keresztnevek.txt", Encoding.UTF8);

            var vezeteknevek = vezetekRaw.Split(',')
                              .Select(nv => nv.Trim().Trim('\''))
                              .Where(nv => nv != "")
                              .ToList();

            var keresztnevek = keresztRaw.Split(',')
                              .Select(nv => nv.Trim().Trim('\''))
                              .Where(nv => nv != "")
                              .ToList();

            for (int i = 0; i < n; i++)
            {
                string vezetek = vezeteknevek[rnd.Next(vezeteknevek.Count)];
                string kereszt = keresztnevek[rnd.Next(keresztnevek.Count)];
                DateTime szuletesi = RandomSzuletesiDatum();

                bool elmult18 = (DateTime.Today.Year - szuletesi.Year - ((DateTime.Today.Month < szuletesi.Month || (DateTime.Today.Month == szuletesi.Month && DateTime.Today.Day < szuletesi.Day)) ? 1 : 0)) >= 18;

                string teljesNev = RemoveDiacritics(vezetek + kereszt);
                string versenyzoID = $"GO-{teljesNev}-{szuletesi:yyyyMMdd}";
                string email = $"{vezetek.ToLower()}.{kereszt.ToLower()}@gmail.com";

                versenyzok.Add(new Versenyzo
                {
                    Vezeteknev = vezetek,
                    Keresztnev = kereszt,
                    SzuletesiDatum = szuletesi,
                    Elmúlt18 = elmult18,
                    VersenyzoID = versenyzoID,
                    Email = email
                });
            }
        }

        static DateTime RandomSzuletesiDatum()//véletlenszerű születési dátum
        {
            DateTime dateTime = DateTime.Now;
            int nowYear = dateTime.Year;
            int minYear = nowYear - 70;
            int maxYear = nowYear - 6;
            int year = rnd.Next(minYear, maxYear);
            int month = rnd.Next(1, 13);
            int day = rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day);
        }

        static string RemoveDiacritics(string text)//ékezetek eltávolítása
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            return new string(normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        }

        static void GenerateSchedule(DateTime startDate) //időponttábla
        {
            int nyitas = 8;
            int zaras = 19;
            int napokSzama = DateTime.DaysInMonth(startDate.Year, startDate.Month) - startDate.Day + 1;

            for (int i = 0; i < napokSzama; i++)
            {
                DateTime nap = startDate.AddDays(i);
                for (int ora = nyitas; ora <= zaras; ora++)
                {
                    foglalasLista.Add(new Foglalas { Idopont = new DateTime(nap.Year, nap.Month, nap.Day, ora, 0, 0), VersenyzoID = "" });
                }
            }
        }

        static void PrintSchedule()//táblázat
        {
            var napok = foglalasLista.GroupBy(f => f.Idopont.Date);
            foreach (var napGroup in napok)
            {
                Console.Write($"{napGroup.Key:yyyy.MM.dd} | ");
                foreach (var f in napGroup)
                {
                    Console.ForegroundColor = string.IsNullOrEmpty(f.VersenyzoID) ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write($"{f.Idopont.Hour}:00 ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        static void ListVersenyzok()//versenyzők listázása
        {
            Console.WriteLine("Versenyzők listája:");
            foreach (var v in versenyzok)
            {
                Console.WriteLine($"{v.VersenyzoID} | {v.Vezeteknev} {v.Keresztnev} | {v.SzuletesiDatum:yyyy.MM.dd} | 18+? {v.Elmúlt18} | Email: {v.Email}");
            }
        }
        static void ManualFoglalas()//manuális foglalás
        {
            Console.WriteLine("Válassz versenyzőt a listából:");
            for (int i = 0; i < versenyzok.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {versenyzok[i].VersenyzoID}");
            }
            Console.Write("Versenyző száma: ");
            int vIndex = int.Parse(Console.ReadLine()) - 1;
            if (vIndex < 0 || vIndex >= versenyzok.Count)
            {
                Console.WriteLine("Érvénytelen választás!");
                return;
            }

            var vID = versenyzok[vIndex].VersenyzoID;

            Console.Write("Foglalás dátum (yyyy.MM.dd): ");
            DateTime d = DateTime.Parse(Console.ReadLine());

            Console.Write("Foglalás kezdőóra (8-19): ");
            int ora = int.Parse(Console.ReadLine());

            Console.Write("Foglalás hossza (1 vagy 2 óra): ");
            int hosszu = int.Parse(Console.ReadLine());
            if (hosszu != 1 && hosszu != 2) hosszu = 1;

            for (int i = 0; i < hosszu; i++)
            {
                var f = foglalasLista.FirstOrDefault(x => x.Idopont.Date == d.Date && x.Idopont.Hour == ora + i);
                if (f != null)
                {
                    if (string.IsNullOrEmpty(f.VersenyzoID))
                        f.VersenyzoID = vID;
                    else
                        Console.WriteLine($"{f.Idopont.Hour}:00 óra már foglalt!");
                }
            }
            Console.WriteLine("Foglalás beállítva!");
        }
        static void Main()
        {
            DateTime maiDatum = DateTime.Today;
            Console.OutputEncoding = Encoding.UTF8;

            string nev = "Meteor Gokartpálya";
            string cim = "8086 Felcsút, Fő u. 176.";
            string telefonszam = "+36-30-426-1265";
            string weboldal = "Meteor-track.hu";

            GeneralVersenyzok();
            GenerateSchedule(maiDatum);

            bool fut = true;
            while (fut)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════╗");
                Console.WriteLine("║        METEOR GOKARTPÁLYA    ║");
                Console.WriteLine("╚══════════════════════════════╝");
                Console.WriteLine($"Név: {nev}");
                Console.WriteLine($"Cím: {cim}");
                Console.WriteLine($"Telefonszám: {telefonszam}");
                Console.WriteLine($"Weboldal: {weboldal}");
                Console.WriteLine("\n=== Menü ===");
                Console.WriteLine("1 - Foglalás táblázat");
                Console.WriteLine("2 - Versenyzők listázása");
                Console.WriteLine("3 - Manuális foglalás átállítása");
                Console.WriteLine("0 - Kilépés");
                Console.Write("Válassz: ");

                string valasztas = Console.ReadLine();

                switch (valasztas)
                {
                    case "1":
                        PrintSchedule();
                        break;
                    case "2":
                        ListVersenyzok();
                        break;
                    case "3":
                        ManualFoglalas();
                        break;
                    case "0":
                        fut = false;
                        break;
                    default:
                        Console.WriteLine("Érvénytelen választás!");
                        break;
                }

                if (fut)
                {
                    Console.WriteLine("\nNyomj egy gombot a visszatéréshez a menübe...");
                    Console.ReadKey();
                }
            }
        }
    }
}

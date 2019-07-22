using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minetec.Core.Toolset.Upgrader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            string path;
            string targetVersion;
            if (!args.Any())
            {
                Console.WriteLine("Please provide a directory to upgrade:");
                path = Console.ReadLine();
                Console.WriteLine("Please provide a version to upgrade to:");
                targetVersion = Console.ReadLine();
            }
            else
            {
                path = args[0];
                targetVersion = args[1];
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("Folder doesn't exist.");
                return;
            }

            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            foreach (var csproj in csprojFiles)
            {
                try
                {
                    var changed = false;
                    var lines = File.ReadAllLines(csproj).ToArray();

                    var state = 0;      // seek "PackageReference..."
                    var lino = 0;
                    while (lino < lines.Count())
                    {
                        var line = lines[lino];
                        if (state == 0 && line.Trim().StartsWith("<PackageReference Include=\"Minetec")) state = 1;
                        if (state == 1 && line.Contains("Version"))
                        {
                            line = Regex.Replace(line, "Version=\"([^\"])*\"", $"Version=\"{targetVersion}\"");
                            line = Regex.Replace(line, "<Version>([^<])*<", $"<Version>{targetVersion}<");
                            lines[lino] = line;
                            changed = true;
                            state = 0;
                        }
                        lino++;
                    }

                    if (changed)
                    {
                        File.SetAttributes(csproj, FileAttributes.Normal);
                        File.WriteAllLines(csproj, lines);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Processed {Path.GetFileName(csproj)}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Skipped {Path.GetFileName(csproj)}");
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed {Path.GetFileName(csproj)}");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

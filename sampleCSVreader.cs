using System.IO;

namespace RadApp.Utilities
{
    public class ReadCSV()
    {
        public void Read()
        {
            string outputPath = Path.Combine(AppContext.BaseDirectory, "assets", "small-csv-data-set.csv");
            string projectPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "small-csv-data-set.csv");

            string csvPath = File.Exists(outputPath) ? outputPath : projectPath;

            if (!File.Exists(csvPath))
            {
                throw new FileNotFoundException($"CSV file was not found. Checked: '{outputPath}' and '{projectPath}'.");
            }

            string? s = File.ReadAllText(csvPath);
            Console.WriteLine(s);
        }
    }
}
using System.Globalization;
using RadApp.Engine;

namespace RadApp.Cli;

public sealed class RadiationCalculatorCli
{
    public void Run(string[] args)
    {
        Console.WriteLine("Co-60 Teletherapy Treatment-Time Calculator");
        Console.WriteLine("Uses the supplied equivalent-square, PDD, and output-calibration tables.");
        Console.WriteLine("Clinical safety: independently verify every result under your local treatment protocol before use.\n");
        var calculator = new TreatmentCalculator(MachineData.Load(FindAssetDirectory()));
        do { try { var input = ReadInputs(); Display(input, calculator.Calculate(input)); } catch (Exception exception) when (exception is ArgumentException or FormatException or IOException) { Console.WriteLine($"\nCould not calculate: {exception.Message}\n"); } } while (AskYesNo("Calculate another treatment?", false));
    }

    private static TreatmentInputs ReadInputs() => new(ReadPositiveNumber("Field size X (cm)"), ReadPositiveNumber("Field size Y (cm)"), ReadPositiveNumber("Depth (cm)"), ReadPositiveNumber("Prescribed dose at depth (cGy)"), ReadDate("Treatment date (yyyy-MM-dd; press Enter for today)", DateOnly.FromDateTime(DateTime.Today)), ReadOptionalPositiveNumber("Equivalent-square area override (cm²; press Enter to calculate)"));
    private static void Display(TreatmentInputs input, TreatmentResult result)
    {
        Console.WriteLine("\nCalculation result");
        Console.WriteLine($"  Field size:                 {input.FieldSizeXCentimetres:0.###} x {input.FieldSizeYCentimetres:0.###} cm");
        Console.WriteLine($"  Equivalent square:          {result.EquivalentSquareCentimetres:0.###} cm ({result.EquivalentSquareAreaCentimetresSquared:0.###} cm²{(input.EquivalentSquareAreaOverrideCentimetresSquared is null ? " calculated" : " override")})");
        Console.WriteLine($"  PDD at {input.DepthCentimetres:0.###} cm:          {result.PercentageDepthDose:0.###} %");
        Console.WriteLine($"  Dose at dmax:               {result.DoseAtDmaxCgy:0.###} cGy");
        Console.WriteLine($"  Reference output:           {result.ReferenceOutputCgyPerMinute:0.###} cGy/min");
        Console.WriteLine($"  Co-60 decay factor:         {result.DecayFactor:0.######}");
        Console.WriteLine($"  Output on treatment date:   {result.OutputOnTreatmentDateCgyPerMinute:0.###} cGy/min");
        Console.WriteLine($"  Treatment time:             {result.TreatmentTimeMinutes:0.###} min ({result.TreatmentTimeMinutes * 60:0.##} s)\n");
    }
    private static string FindAssetDirectory() { var outputAssets = Path.Combine(AppContext.BaseDirectory, "assets"); var projectAssets = Path.Combine(Directory.GetCurrentDirectory(), "assets"); var directory = Directory.Exists(outputAssets) ? outputAssets : projectAssets; if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("The assets directory could not be found."); return directory; }
    private static double ReadPositiveNumber(string prompt) { while (true) { Console.Write($"{prompt}: "); var text = Console.ReadLine(); if (TryParseNumber(text, out var value) && value > 0) return value; Console.WriteLine("Enter a number greater than zero."); } }
    private static double? ReadOptionalPositiveNumber(string prompt) { while (true) { Console.Write($"{prompt}: "); var text = Console.ReadLine(); if (string.IsNullOrWhiteSpace(text)) return null; if (TryParseNumber(text, out var value) && value > 0) return value; Console.WriteLine("Enter a number greater than zero, or press Enter to calculate it."); } }
    private static DateOnly ReadDate(string prompt, DateOnly defaultValue) { Console.Write($"{prompt}: "); var text = Console.ReadLine(); if (string.IsNullOrWhiteSpace(text)) return defaultValue; DateOnly date; while (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) { Console.Write("Use yyyy-MM-dd: "); text = Console.ReadLine(); } return date; }
    private static bool AskYesNo(string prompt, bool defaultValue) { Console.Write($"{prompt} {(defaultValue ? "[Y/n]" : "[y/N]")}: "); var answer = Console.ReadLine()?.Trim(); return string.IsNullOrEmpty(answer) ? defaultValue : answer.Equals("y", StringComparison.OrdinalIgnoreCase) || answer.Equals("yes", StringComparison.OrdinalIgnoreCase); }
    private static bool TryParseNumber(string? text, out double value) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
}

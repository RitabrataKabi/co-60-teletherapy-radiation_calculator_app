using System.Globalization;

namespace RadApp.Engine;

public sealed record TreatmentInputs(double FieldSizeXCentimetres, double FieldSizeYCentimetres, double DepthCentimetres, double PrescribedDoseCgy, DateOnly TreatmentDate, double? EquivalentSquareAreaOverrideCentimetresSquared = null);

public sealed record TreatmentResult(double EquivalentSquareCentimetres, double EquivalentSquareAreaCentimetresSquared, double PercentageDepthDose, double DoseAtDmaxCgy, double ReferenceOutputCgyPerMinute, double DecayFactor, double OutputOnTreatmentDateCgyPerMinute, double TreatmentTimeMinutes);

/// <summary>Reproduces the calculations in the supplied Co-60 workbook from its local data tables.</summary>
public sealed class TreatmentCalculator
{
    private static readonly DateOnly CalibrationDate = new(2018, 5, 23);
    private const double Cobalt60HalfLifeYears = 5.26;
    private readonly MachineData _data;

    public TreatmentCalculator(MachineData data) => _data = data;

    public TreatmentResult Calculate(TreatmentInputs input)
    {
        Validate(input);
        var equivalentSquare = _data.GetEquivalentSquare(input.FieldSizeXCentimetres, input.FieldSizeYCentimetres);
        var equivalentSquareArea = input.EquivalentSquareAreaOverrideCentimetresSquared ?? equivalentSquare * equivalentSquare;
        var pdd = _data.GetPdd(equivalentSquareArea, input.DepthCentimetres);
        var referenceOutput = _data.GetReferenceOutput(equivalentSquareArea);
        var daysSinceCalibration = input.TreatmentDate.DayNumber - CalibrationDate.DayNumber;
        var decayFactor = Math.Exp(-(0.693 / (Cobalt60HalfLifeYears * 365d)) * daysSinceCalibration);
        var currentOutput = referenceOutput * decayFactor;
        var doseAtDmax = input.PrescribedDoseCgy / (pdd / 100d);
        return new TreatmentResult(equivalentSquare, equivalentSquareArea, pdd, doseAtDmax, referenceOutput, decayFactor, currentOutput, doseAtDmax / currentOutput);
    }

    private static void Validate(TreatmentInputs input)
    {
        if (input.FieldSizeXCentimetres is < 0.5 or > 40 || input.FieldSizeYCentimetres is < 0.5 or > 40) throw new ArgumentOutOfRangeException(nameof(input), "Each field dimension must be from 0.5 to 40 cm.");
        if (input.DepthCentimetres is < 0.5 or > 30) throw new ArgumentOutOfRangeException(nameof(input), "Depth must be from 0.5 to 30 cm.");
        if (input.PrescribedDoseCgy <= 0) throw new ArgumentOutOfRangeException(nameof(input), "Prescribed dose must be greater than zero.");
        if (input.EquivalentSquareAreaOverrideCentimetresSquared is <= 0) throw new ArgumentOutOfRangeException(nameof(input), "Equivalent-square area override must be greater than zero.");
    }
}

public sealed class MachineData
{
    private readonly Dictionary<int, Dictionary<int, double>> _equivalentSquareRows;
    private readonly double[] _pddDepths;
    private readonly (double Area, double[] Values)[] _pddRows;
    private readonly (double Area, double Output)[] _outputRows;

    private MachineData(Dictionary<int, Dictionary<int, double>> equivalentSquareRows, double[] pddDepths, (double Area, double[] Values)[] pddRows, (double Area, double Output)[] outputRows)
    {
        _equivalentSquareRows = equivalentSquareRows;
        _pddDepths = pddDepths;
        _pddRows = pddRows;
        _outputRows = outputRows;
    }

    public static MachineData Load(string assetDirectory)
    {
        var equivalentRows = ReadCsv(Path.Combine(assetDirectory, "Eq-sq-sheet-matrix.csv"));
        var equivalentHeaders = equivalentRows[0].Skip(1).Select(Parse).ToArray();
        var equivalentTable = equivalentRows.Skip(1).ToDictionary(row => (int)Parse(row[0]), row => row.Skip(1).Select((value, index) => (Header: (int)equivalentHeaders[index], Value: value)).Where(item => !string.IsNullOrWhiteSpace(item.Value)).ToDictionary(item => item.Header, item => Parse(item.Value)));

        var pddRows = ReadCsv(Path.Combine(assetDirectory, "PDDsheet.csv"));
        var pddDepths = pddRows[0].Skip(1).Select(Parse).ToArray();
        var pddTable = pddRows.Skip(1).Select(row => (Parse(row[0]), row.Skip(1).Select(Parse).ToArray())).OrderBy(row => row.Item1).Select(row => (Area: row.Item1, Values: row.Item2)).ToArray();

        var outputRows = ReadCsv(Path.Combine(assetDirectory, "OP-Data-On-Calibration.csv"));
        var outputTable = outputRows.Skip(1).Select(row => (Area: Parse(row[1]), Output: Parse(row[2]))).OrderBy(row => row.Area).ToArray();
        return new MachineData(equivalentTable, pddDepths, pddTable, outputTable);
    }

    public double GetEquivalentSquare(double x, double y)
    {
        // The source workbook uses INT on the shorter and longer dimensions before lookup.
        var smaller = (int)Math.Min(x, y);
        var larger = (int)Math.Max(x, y);
        if (!_equivalentSquareRows.TryGetValue(smaller, out var row) || !row.TryGetValue(larger, out var value)) throw new ArgumentOutOfRangeException(nameof(x), "The field-size pair is outside the supported equivalent-square lookup table.");
        return value;
    }

    public double GetPdd(double area, double depth)
    {
        var areas = _pddRows.Select(row => row.Area).ToArray();
        var lowerArea = FindLower(areas, area); var upperArea = FindUpper(areas, area);
        var lowerDepth = FindLower(_pddDepths, depth); var upperDepth = FindUpper(_pddDepths, depth);
        var lowerValue = Interpolate(_pddDepths[lowerDepth], _pddRows[lowerArea].Values[lowerDepth], _pddDepths[upperDepth], _pddRows[lowerArea].Values[upperDepth], depth);
        var upperValue = Interpolate(_pddDepths[lowerDepth], _pddRows[upperArea].Values[lowerDepth], _pddDepths[upperDepth], _pddRows[upperArea].Values[upperDepth], depth);
        return Interpolate(_pddRows[lowerArea].Area, lowerValue, _pddRows[upperArea].Area, upperValue, area);
    }

    public double GetReferenceOutput(double area)
    {
        var areas = _outputRows.Select(row => row.Area).ToArray(); var lower = FindLower(areas, area); var upper = FindUpper(areas, area);
        return Interpolate(_outputRows[lower].Area, _outputRows[lower].Output, _outputRows[upper].Area, _outputRows[upper].Output, area);
    }

    private static List<string[]> ReadCsv(string path) => File.ReadLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split(',')).ToList();
    private static double Parse(string value) => double.Parse(value, CultureInfo.InvariantCulture);
    private static int FindLower(double[] values, double value) { if (value < values[0] || value > values[^1]) throw new ArgumentOutOfRangeException(nameof(value), $"Value must be from {values[0]} to {values[^1]}."); for (var i = values.Length - 1; i >= 0; i--) if (values[i] <= value) return i; return 0; }
    private static int FindUpper(double[] values, double value) { if (value < values[0] || value > values[^1]) throw new ArgumentOutOfRangeException(nameof(value), $"Value must be from {values[0]} to {values[^1]}."); for (var i = 0; i < values.Length; i++) if (values[i] >= value) return i; return values.Length - 1; }
    private static double Interpolate(double x0, double y0, double x1, double y1, double x) => x0 == x1 ? y0 : y0 + ((x - x0) * (y1 - y0) / (x1 - x0));
}

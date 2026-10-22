# Co-60 teletherapy treatment-time calculator

A C# command-line application based on the supplied Co-60 worksheet. It reads the included equivalent-square, PDD, and output-calibration CSV tables, calculates Co-60 decay from the 23 May 2018 calibration date, and reports treatment time in minutes and seconds.

Run it from this folder:

```powershell
dotnet run
```

The optional equivalent-square-area override exists because the original workbook's visible sample uses a manually entered 120 cm² area for a 10 × 10 cm field. Leave it blank for the calculated area, or enter the area from an approved local workflow.

This tool is not a substitute for clinical review. Independently verify every value according to your department's treatment protocol before use.

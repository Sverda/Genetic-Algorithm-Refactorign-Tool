Set-Location C:\Users\Damian\source\repos\GenSharp\src\GenSharp.Console\bin\Debug\netcoreapp3.1\

$outputFolder = "C:\Users\Damian\Downloads\Results"
$sets = 1..5
$sources = "C:\Users\Damian\Downloads\Results\source1.cs", "C:\Users\Damian\Downloads\Results\source2.cs"
foreach ($source in $sources) {
    foreach ($set in $sets) {
        Write-Host - Starting set $set
    
        $sourceIndex = $sources.IndexOf($source)
        .\GenSharp.Console.exe -s $source -m CyclomaticComplexity -o "$outputFolder\ga-cc-$sourceIndex-$set.xml"
        .\GenSharp.Console.exe -s $source -m LinesOfCode -o "$outputFolder\ga-loc-$sourceIndex-$set.xml"
        .\GenSharp.Console.exe -s $source -m MaintainabilityIndex -o "$outputFolder\ga-mi-$sourceIndex-$set.xml"
    }
}
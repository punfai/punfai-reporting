# watch the bug in VS where PackageVersion in the csproj file is not PackageVersion in the properties UI
$ver = "0.1.9"
$nuserver = "Z:\0005 IT\Software\nuget"
Write-Host -ForegroundColor Cyan "Punfai.Report..."
dotnet pack .\src\Punfai.Report --configuration Release
nuget delete Punfai.Report $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report\bin\Release\Punfai.Report.$ver.nupkg -source $nuserver

Write-Host -ForegroundColor Cyan "IronScript"
dotnet pack .\src\Punfai.Report.IronScript --configuration Release
nuget delete Punfai.Report.IronScript $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report.IronScript\bin\Release\Punfai.Report.IronScript.$ver.nupkg -source $nuserver

Write-Host -ForegroundColor Cyan "OfficeOpenXml..."
dotnet pack .\src\Punfai.Report.OfficeOpenXml --configuration Release
nuget delete Punfai.Report.OfficeOpenXml $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report.OfficeOpenXml\bin\Release\Punfai.Report.OfficeOpenXml.$ver.nupkg -source $nuserver

Write-Host -ForegroundColor Cyan "Ibex..."
nuget pack .\src\Punfai.Report.Ibex\Punfai.Report.Ibex.csproj -properties Configuration=Release
nuget delete Punfai.Report.Ibex $ver -source $nuserver -noninteractive
nuget add .\Punfai.Report.Ibex.$ver.nupkg -source $nuserver
Remove-Item .\Punfai.Report.Ibex.$ver.nupkg

Write-Host -ForegroundColor Cyan "Wpf..."
nuget pack .\src\Punfai.Report.Wpf\Punfai.Report.Wpf.csproj -properties Configuration=Release -symbols
nuget delete Punfai.Report.Wpf $ver -source $nuserver -noninteractive
nuget add .\Punfai.Report.Wpf.$ver.nupkg -source $nuserver
Remove-Item .\Punfai.Report.Wpf.$ver.nupkg
Remove-Item .\Punfai.Report.Wpf.$ver.symbols.nupkg

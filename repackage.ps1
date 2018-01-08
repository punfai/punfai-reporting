
$ver = "0.1.2"
$nuserver = "Z:\0005 IT\Software\nuget"
Write-Host -ForegroundColor Cyan "Punfai.Report..."
dotnet pack .\src\Punfai.Report --configuration Release
nuget delete Punfai.Report $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report\bin\Release\Punfai.Report.$ver.nupkg -source $nuserver

Write-Host -ForegroundColor Cyan "IronScript"
dotnet pack .\src\Punfai.Report.IronScript --configuration Release
nuget delete Punfai.Report.IronScript $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report.OfficeOpenXml\bin\Release\Punfai.Report.OfficeOpenXml.$ver.nupkg -source $nuserver

Write-Host -ForegroundColor Cyan "OfficeOpenXml..."
dotnet pack .\src\Punfai.Report.OfficeOpenXml --configuration Release
nuget delete Punfai.Report.OfficeOpenXml $ver -source $nuserver -noninteractive
nuget add .\src\Punfai.Report.IronScript\bin\Release\Punfai.Report.IronScript.$ver.nupkg -source $nuserver


# watch the bug in VS where PackageVersion in the csproj file is not PackageVersion in the properties UI
# 1. update version in .csproj for netcore libs and AssemblyInfo.cs for framework libs
# 2. change the version below
$ver = "0.2.4"
$nuserver = "https://www.myget.org/F/yummy-ag/api/v2/package"

Write-Host -ForegroundColor Cyan "Punfai.Report..."
nuget push .\Punfai.Report.$ver.nupkg 17fcb248-be21-431b-9e69-15e4b0396c5a -Source $nuserver
Remove-Item .\Punfai.Report.$ver.nupkg

Write-Host -ForegroundColor Cyan "IronScript"
nuget push .\Punfai.Report.IronScript.$ver.nupkg 17fcb248-be21-431b-9e69-15e4b0396c5a -Source $nuserver
Remove-Item .\Punfai.Report.IronScript.$ver.nupkg

Write-Host -ForegroundColor Cyan "OfficeOpenXml..."
nuget push .\Punfai.Report.OfficeOpenXml.$ver.nupkg 17fcb248-be21-431b-9e69-15e4b0396c5a -Source $nuserver
Remove-Item .\Punfai.Report.OfficeOpenXml.$ver.nupkg

Write-Host -ForegroundColor Cyan "Ibex..."
nuget push .\Punfai.Report.Ibex.$ver.nupkg 17fcb248-be21-431b-9e69-15e4b0396c5a -Source $nuserver
Remove-Item .\Punfai.Report.Ibex.$ver.nupkg

Write-Host -ForegroundColor Cyan "Wpf..."
nuget push .\Punfai.Report.Wpf.$ver.nupkg 17fcb248-be21-431b-9e69-15e4b0396c5a -Source $nuserver
Remove-Item .\Punfai.Report.Wpf.$ver.nupkg
Remove-Item .\Punfai.Report.Wpf.$ver.symbols.nupkg

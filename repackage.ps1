# watch the bug in VS where PackageVersion in the csproj file is not PackageVersion in the properties UI
# 1. update version in .csproj for netcore libs and AssemblyInfo.cs for framework libs
# 2. change the version below
# 3. change the version in pushPackage.ps1

$ver = "0.2.4"
$nuserver = "https://www.myget.org/F/yummy-ag/api/v2/package"

Write-Host -ForegroundColor Cyan "Punfai.Report..."
dotnet pack .\src\Punfai.Report --configuration Release --output .\

Write-Host -ForegroundColor Cyan "IronScript"
dotnet pack .\src\Punfai.Report.IronScript --configuration Release --output .\

Write-Host -ForegroundColor Cyan "OfficeOpenXml..."
dotnet pack .\src\Punfai.Report.OfficeOpenXml --configuration Release --output .\

Write-Host -ForegroundColor Cyan "Ibex..."
nuget pack .\src\Punfai.Report.Ibex\Punfai.Report.Ibex.csproj -properties Configuration=Release

Write-Host -ForegroundColor Cyan "Wpf..."
nuget pack .\src\Punfai.Report.Wpf\Punfai.Report.Wpf.csproj -properties Configuration=Release -symbols

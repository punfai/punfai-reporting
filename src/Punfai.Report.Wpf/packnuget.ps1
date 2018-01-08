$ver = "0.1.2"
$nuserver = "Z:\0005 IT\Software\nuget"
Write-Host -ForegroundColor Cyan "Packing..."
nuget pack .\Punfai.Report.Wpf.csproj -properties Configuration=Release -symbols
Write-Host -ForegroundColor Cyan "Deleting old ones if they are there"
nuget delete Punfai.Report.Wpf $ver -source $nuserver -noninteractive
Write-Host -ForegroundColor Cyan "Adding to network nuget feed..."
nuget add .\Punfai.Report.Wpf.$ver.nupkg -source $nuserver
Write-Host -ForegroundColor Cyan "cleaning up"
Remove-Item .\Punfai.Report.Wpf.$ver.nupkg

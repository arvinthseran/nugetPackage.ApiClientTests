$packages="SFA.Roatp.Api.Client"
# SFA.Roatp.Api.Client ,SFA.DAS.Apprenticeships.Api.Client,"SFA.DAS.AssessmentOrgs.Api.Client","SFA.DAS.Providers.Api.Client"
$a = Get-Date
foreach ($package in $packages) 
{
Write-Host "Finding available versions for package - " $package
$providerapiversions = Find-Package -Name $package -Source https://nuget.org/api/v2/ -AllVersions | Select-Object -property Version | Where-Object {$_.Version -notlike "*prerelease*"}
Write-Host $providerapiversions
foreach ($version in $providerapiversions) 
{
$currentTestVersion = $version."Version"
Write-Host "Testing " -nonewline; Write-Host $package"."$currentTestVersion -foregroundcolor red -backgroundcolor yellow

$outputfile="C:\SFA\nugetPackage.ApiClientTests\src\Sfs.Api.Client\Sfa.ApiClient.Tests\bin\Debug\testouput"+$package+$currentTestVersion+$a.Day+$a.Month+$a.Hour+$a.Minute+".txt"
C:\SFA\nugetPackage.ApiClientTests\src\Sfs.Api.Client\packages\NUnit.ConsoleRunner.3.6.1\tools\nunit3-console.exe C:\SFA\nugetPackage.ApiClientTests\src\Sfs.Api.Client\Sfa.ApiClient.Tests\bin\Debug\Sfa.ApiClient.Tests.dll --params=version=$currentTestVersion --params=packageinTest=$package --output=$outputfile
}
}
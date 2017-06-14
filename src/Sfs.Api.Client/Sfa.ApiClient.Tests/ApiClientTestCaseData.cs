using NuGet;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sfa.ApiClient.Tests
{
    public static class ApiClientTestCaseData
    {

        //Connect to the official package repository
        public static IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

        public static IEnumerable<TestCaseData> GetAsyncPackages()
        {
            return GetPackages().Where(x => new Version(x.Arguments.First().ToString()) >= new Version("0.10.64"));
        }

        public static IEnumerable<TestCaseData> GetPackages()
        {
            foreach (var item in new string[][] {
                new string[] { "SFA.DAS.Providers.Api.Client","ProviderApiClient" },
                new string[] { "SFA.DAS.AssessmentOrgs.Api.Client","AssessmentOrgsApiClient" },
                new string[] { "SFA.DAS.Apprenticeships.Api.Client", "StandardApiClient" },
                new string[] { "SFA.DAS.Apprenticeships.Api.Client", "FrameworkApiClient" },
            })
            {
                var versions = FindPackage(item[0]);
                foreach (var version in versions)
                {
                    yield return new TestCaseData(version, item[0], item[1]);
                }
            }
        }

        private static IEnumerable<string> FindPackage(string package)
        {
            //Get the list of all NuGet packages with ID 'SFA.DAS.Providers.Api.Client'       
            List<IPackage> packages = repo.FindPackagesById(package).ToList();

            //Filter the list of packages that are not Release (Stable) versions
            packages = packages.Where(x => x.IsReleaseVersion() == true && x.IsListed() == true).OrderByDescending(y => y.Version).ToList();

            //Iterate through the list and print the full name of the pre-release packages to console
            return packages.Where(p => p.Version.ToFullString() != "0.9.161").Select(q => q.Version.ToFullString());
        }
    }
}

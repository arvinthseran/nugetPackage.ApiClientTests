using NuGet;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sfa.ApiClient.Tests
{
    public static class ApiClientNuGetPackagesInTest
    {
        //Connect to the official package repository
        public static IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

        public static IEnumerable<TestCaseData> GetAsyncPackages()
        {
            return GetPackages().Where(x => new Version(x.Arguments.First().ToString()) >= new Version("0.10.64"));
        }

        public static IEnumerable<TestCaseData> GetProviderPackages()
        {
            foreach (var item in new[] {
                "SFA.DAS.Providers.Api.Client",
                "SFA.DAS.Apprenticeships.Api.Client"
            }
            )
            {
                var versions = FindPackage(item);
                foreach (var version in versions)
                {
                    yield return new TestCaseData(version, item);
                }
            }
        }

        public static IEnumerable<TestCaseData> GetApprenticehsipPackages()
        {
            var item = "SFA.DAS.Apprenticeships.Api.Client";
            var versions = FindPackage(item);
            foreach (var version in versions)
            {
                yield return new TestCaseData(version, item);
            }
        }

        public static IEnumerable<TestCaseData> GetAssessmentOrgPackages()
        {
            var item = "SFA.DAS.AssessmentOrgs.Api.Client";
            var versions = FindPackage(item);
            foreach (var version in versions)
            {
                yield return new TestCaseData(version, item);
            }
        }

        public static IEnumerable<TestCaseData> GetRoatpPackages()
        {
            var item = "SFA.Roatp.Api.Client";
            var versions = FindPackage(item);
            foreach (var version in versions)
            {
                yield return new TestCaseData(version, item);
            }
        }

        public static IEnumerable<TestCaseData> GetPackages()
        {
            foreach (var item in new[] {
                "SFA.DAS.Providers.Api.Client",
                "SFA.DAS.AssessmentOrgs.Api.Client",
                "SFA.DAS.Apprenticeships.Api.Client",
                "SFA.Roatp.Api.Client"
            }
            )
            {
                var versions = FindPackage(item);
                foreach (var version in versions)
                {
                    yield return new TestCaseData(version, item);
                }
            }
        }

        private static IEnumerable<string> FindPackage(string package)
        {

            //Get the list of all NuGet packages with ID 'SFA.DAS.Providers.Api.Client'    
            var repo = new PackageRepositoryFactory().CreateRepository("https://packages.nuget.org/api/v2");
            var packages = repo.FindPackagesById(package).ToList();

            //Filter the list of packages that are not Release (Stable) versions
            packages = packages.Where(x => x.IsReleaseVersion() == true && x.IsListed() == true).OrderByDescending(y => y.Version).ToList();

            //Iterate through the list and print the full name of the pre-release packages to console
            return packages.Where(p => p.Version.ToFullString() != "0.9.161").Select(q => q.Version.ToFullString());
        }
    }
}

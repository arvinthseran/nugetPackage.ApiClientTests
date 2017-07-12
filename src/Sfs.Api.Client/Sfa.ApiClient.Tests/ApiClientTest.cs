using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Security.Policy;

namespace Sfa.ApiClient.Tests
{
    [TestFixture]
    public class ApiClientTest
    {
        //[Test, TestCaseSource(typeof(ApiClientTestCaseData),"GetAsyncPackages")]
        //[Category("async")]
        //[Ignore("")]
        //public void CheckIfAsyncWorks(string version, string packageinTest)
        //{
        //}

        // Uncomment below line to test from VS locally
        // [Test, TestCaseSource(typeof(ApiClientNuGetPackagesInTest), "GetProviderPreReleasePackage")]
        [Test]
        public void CheckApiClients() // string version, string packageinTest
        {
             var version = TestContext.Parameters["version"];
             var packageinTest = TestContext.Parameters["packageinTest"];
            TestContext.WriteLine(version);
            TestContext.WriteLine(packageinTest);
            var nugetPackagesdlls = new List<PackageIdentifier>();
            var packageDepencies = new List<PackageDependency>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string dir = System.IO.Path.GetDirectoryName(assembly.Location);

            Func<string, string, PackageIdentifier> AddnugetPackagesdlls = (id, v) => 
              new PackageIdentifier
              {
                  packageId = id,
                  packageVersion = v
              };

            // Temp directory to download Nuget Packages
            string tempPath = "C:\\temp\\"+ version;

            // delete the directory if it exists
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            
            //Find the Package in test
            var package = ApiClientNuGetPackagesInTest.repo.FindPackage(packageinTest, SemanticVersion.Parse(version));

            //Retrive the dependency packages
            packageDepencies = package.DependencySets.First().Dependencies.ToList();

            var packageManager = new PackageManager(ApiClientNuGetPackagesInTest.repo, tempPath);
            foreach (var x in packageDepencies)
            {
                //Find and install the dependency packages
                var deppackage = ApiClientNuGetPackagesInTest.repo.FindPackage(x.Id, x.VersionSpec.MinVersion);
                packageManager.InstallPackage(deppackage, true, false);
                nugetPackagesdlls.Add(AddnugetPackagesdlls(x.Id, x.VersionSpec.MinVersion.ToFullString()));
            }

            //Install the package in test
            packageManager.InstallPackage(package, true, false);
            nugetPackagesdlls.Add(AddnugetPackagesdlls(packageinTest, version));

            // Delete dlls if its already exists in dir.
            nugetPackagesdlls.Where(x => File.Exists($"{dir}\\{x.packageId}.dll")).ToList().ForEach(y => File.Delete($"{dir}\\{y.packageId}.dll"));

            // copy package dlls to dir.
            var versionInTest = new Version(version.Replace("-prerelease", string.Empty));
            var dotnet45version = new Version("0.9.140");
            if (versionInTest >= dotnet45version || packageinTest == "SFA.Roatp.Api.Client")
            {
                nugetPackagesdlls.ForEach(x => File.Copy($"{tempPath}\\{x.packageId}.{x.packageVersion}\\lib\\net45\\{x.packageId}.dll", $"{dir}\\{x.packageId}.dll"));
            }
            else
            {
                nugetPackagesdlls.Where(a => !(a.packageId.StartsWith("SFA", StringComparison.OrdinalIgnoreCase))).ToList().ForEach(x => File.Copy($"{tempPath}\\{x.packageId}.{x.packageVersion}\\lib\\net45\\{x.packageId}.dll", $"{dir}\\{x.packageId}.dll"));
                nugetPackagesdlls.Where(a => a.packageId.StartsWith("SFA", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x => File.Copy($"{tempPath}\\{x.packageId}.{x.packageVersion}\\lib\\{x.packageId}.dll", $"{dir}\\{x.packageId}.dll"));
            }
            
            // Load the test dll in to new domain
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = dir;
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            AppDomain domain = AppDomain.CreateDomain($"{packageinTest}{version}", adevidence, domaininfo);

            //Create type of MarshalByRefObject class to create instance and unwrap
            Type type = typeof(TypeProxy);
            var value = (TypeProxy)domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                type.FullName);

            //Load the  package in contents of a dependency assembly file
            packageDepencies.ForEach(x => value.LoadFromAssembly($"{dir}\\{x}.dll"));
            var clientassembly = value.LoadFromAssembly($"{dir}\\{packageinTest}.dll");
            Console.WriteLine($"Client Test Assembly runtimeversion : {clientassembly.FullName}");
            var publicClasses = clientassembly.ExportedTypes.Where(x => x.IsPublic == true && x.IsInterface == false && x.IsAbstract == false).Select(y => y.Name);
            int excount = 0;
            int testcasecount = 0;
            foreach (var typeintest in publicClasses)
            {
                // skipping execution for Obsolete types in the packages
                if (packageinTest == "SFA.DAS.Apprenticeships.Api.Client" && typeintest == "ProviderApiClient") 
                {
                    var oldversion = new Version("0.9.140");
                    var newversion = new Version("0.10.64");
                    if (versionInTest > oldversion && versionInTest < newversion)
                    {
                        continue;
                    }
                }
                var clientType = clientassembly.GetType($"{packageinTest}.{typeintest}");
                
                var publicMethods = clientType.GetMethods().Where(x=> x.DeclaringType?.BaseType?.FullName != "System.Object" && x.DeclaringType?.BaseType != null).ToList();
                foreach (var methodInfo in publicMethods.Where(x => !x.Name.EndsWith("Async")))
                {
                    var methodName = methodInfo.Name;
                    var parameterInfo = methodInfo.GetParameters().ToList();

                    List<Type> parameterTypes = new List<Type>();
                    List<object> parameterValues = new List<object>();

                    parameterInfo.ForEach(x =>
                    {
                        parameterTypes.Add(x.ParameterType);
                        parameterValues.Add(TestData.GetDefaultParamValue(x.Name, x.ParameterType.Name));
                    });

                    string parammessage = string.Empty;
                    int index = 0;
                    foreach (var i in parameterInfo.ToDictionary(x=>x.Name, y=> y.ParameterType.Name))
                    {
                        parammessage = parammessage + $"{i.Value} {i.Key} = {parameterValues[index]} , ";
                        index++;
                    }

                    var testcasemessage = $"TestCase : {packageinTest}.{typeintest}.{methodName}({parammessage.Trim().TrimEnd(',')})";
                    Console.WriteLine(testcasemessage);

                    var method = clientType.GetMethod(methodName, parameterTypes.ToArray());
                    
                    try
                    {
                        testcasecount++;
                        var client = Activator.CreateInstance(clientType, TestData.GetbaseUri(packageinTest, !version.Contains("-prerelease")));
                        dynamic result = method.Invoke(client, parameterValues.ToArray());
                        Assert.IsNotNull(result, testcasemessage);
                    }
                    catch (TargetInvocationException ex)
                    {
                        Console.WriteLine(Environment.NewLine + ex);
                        excount++;
                    }
                }
            }
            Assert.AreEqual(testcasecount, testcasecount - excount, $"{excount} testcases failed, out of {testcasecount} ref logs for more details");

            AppDomain.Unload(domain);
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
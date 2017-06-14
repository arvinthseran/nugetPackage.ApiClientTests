using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using TestClient;
using System.Collections.Specialized;

namespace Sfa.ApiClient.Tests
{
    [TestFixture]
    public class ProvidersTest
    {
        [Test]
        public void Should()
        {
            var clientType = typeof(Class1);
            dynamic client2 = Activator.CreateInstance(typeof(Class1));

            Assert.IsTrue(client2.IsValid("test"));

            dynamic something = client2.Something("1001");
            foreach (PropertyInfo propertyInfo in something.GetType().GetProperties())
            {
                var obj = propertyInfo.GetGetMethod().Invoke(something, new object[] { });
                dynamic value = Convert.ChangeType(obj, propertyInfo.PropertyType);
                var defaultvalue = GetDefault(propertyInfo.PropertyType);
                Assert.AreNotEqual(defaultvalue, value);
            }
        }

        [Test, TestCaseSource(typeof(ApiClientTestCaseData),"GetAsyncPackages")]
        [Category("async")]
        public void CheckIfAsyncWorks(string version, string packageinTest)
        {

        }

        [Test, TestCaseSource(typeof(ApiClientTestCaseData), "GetPackages")]
        public void CheckIfNugetClientsWork(string version, string packageinTest,string typeintest)
        {
            List<PackageIdentifier> nugetPackagesdlls = new List<PackageIdentifier>();
            List<PackageDependency> packageDepencies = new List<PackageDependency>();

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
            var package = ApiClientTestCaseData.repo.FindPackage(packageinTest, SemanticVersion.Parse(version));

            //Retrive the dependency packages
            packageDepencies = package.DependencySets.First().Dependencies.ToList();

            var packageManager = new PackageManager(ApiClientTestCaseData.repo, tempPath);
            foreach (var x in packageDepencies)
            {
                //Find and install the dependency packages
                var deppackage = ApiClientTestCaseData.repo.FindPackage(x.Id, x.VersionSpec.MinVersion);
                packageManager.InstallPackage(deppackage, true, false);
                nugetPackagesdlls.Add(AddnugetPackagesdlls(x.Id, x.VersionSpec.MinVersion.ToFullString()));
            }

            //Install the package in test
            packageManager.InstallPackage(package, true, false);
            nugetPackagesdlls.Add(AddnugetPackagesdlls(packageinTest, version));

            // Delete dlls if its already exists in dir.
            nugetPackagesdlls.Where(x => File.Exists($"{dir}\\{x.packageId}.dll")).ToList().ForEach(y => File.Delete($"{dir}\\{y.packageId}.dll"));
            
            // copy package dlls to dir.
            nugetPackagesdlls.ForEach(x => File.Copy($"{tempPath}\\{x.packageId}.{x.packageVersion}\\lib\\net45\\{x.packageId}.dll", $"{dir}\\{x.packageId}.dll"));

            
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

            var clientType = clientassembly.GetType($"{packageinTest}.{typeintest}");

            var client = Activator.CreateInstance(clientType, "http://das-prd-apprenticeshipinfoservice.cloudapp.net");
            var methodsInfo = clientType.GetMethods().ToList();
            var methodsInTest = methodsInfo.Where(x => x.IsPublic == true && x.DeclaringType.Name == typeintest);
            foreach (var method in methodsInTest)
            {
                var methodName = method.Name;
                var returnType = method.ReturnType;
                var parameterInfo = method.GetParameters().ToList();
                
                List<Type> parameterTypes = new List<Type>();
                List<object> parameterValues = new List<object>();

                parameterInfo.ForEach(x =>
                {
                    parameterTypes.Add(x.ParameterType);
                    parameterValues.Add(TestData.GetDefaultParamValue(x.Name, x.ParameterType.Name));
                });

                var methodInfo = clientType.GetMethod(methodName, parameterTypes.ToArray());

                dynamic result = methodInfo.Invoke(client, parameterValues.ToArray());
                Assert.IsNotNull(result, $"{ methodName}");
            }
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
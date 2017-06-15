using System;

namespace Sfa.ApiClient.Tests
{
    public static class TestData
    {
        private static string ukprn = "10000020";
        private static string organisationid = "EPA0001";
        private static string standardid = "150";
        private static string frameworkid = "539";
        private static string pathwaycode = "3";
        private static string programmeType = "3";

        public static string GetbaseUri(string packageinTest)
        {
            return packageinTest == "SFA.Roatp.Api.Client" ? "https://roatp.apprenticeships.sfa.bis.gov.uk" : "http://das-prd-apprenticeshipinfoservice.cloudapp.net";
        }

        public static object GetDefaultParamValue(string paramName, string paramtypeName)
        {
            try
            {
                switch (paramtypeName)
                {
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "int":
                        return int.Parse(GetbyParamName(paramName));

                    case "String":
                    case "string":
                        return GetbyParamName(paramName).ToString();

                    case "long":
                    case "Long":
                        return long.Parse(GetbyParamName(paramName));
                }
                return null;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception occured when trying to get Default value for {paramtypeName} {paramName} ");
                throw ex;
            }
           
        }

        private static string GetbyParamName(string paramName)
        {
            switch (paramName.ToLower())
            {
                case "providerukprn":
                case "ukprn":
                    return ukprn;

                case "organisationid":
                    return organisationid;

                case "standardid":
                case "standardcode":
                    return standardid;

                case "frameworkid":
                case "frameworkcode":
                    return frameworkid;

                case "pathwaycode":
                    return pathwaycode;

                case "programmetype":
                case "progamtype":
                    return programmeType;
            }
            return null;
        }
    }
}

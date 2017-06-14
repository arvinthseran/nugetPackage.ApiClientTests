using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sfa.ApiClient.Tests
{
    public static class TestData
    {
        private static string ukprn = "10000020";
        private static string organisationid = "EPA0001";
        private static string standardid = "1";

        public static object GetDefaultParamValue(string paramName, string paramtypeName)
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
                    return standardid;
            }
            return null;
        }
    }
}

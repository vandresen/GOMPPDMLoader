using GOMPPDMLoaderLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.Extensions
{
    public static class CommonExtensions
    {
        public static List<ReferenceData> CreateReferenceDataObject(this List<string> refValues)
        {
            List<ReferenceData> refs = new List<ReferenceData>();
            foreach (var value in refValues)
            {
                ReferenceData refData = new ReferenceData() { Reference = value };
                refs.Add(refData);
            }
            return refs;
        }

        public static double GetDoubleFromString(this string token)
        {
            double number = 0.0;
            if (!string.IsNullOrWhiteSpace(token))
            {
                double value;
                if (double.TryParse(token, out value)) number = value;
            }
            return number;
        }

        public static DateTime? GetDateFromString(this string token, string format)
        {
            DateTime? date = null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                DateTime value;
                if (DateTime.TryParseExact(token, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value)) date = value;
            }
            return date;
        }

        public static string GetOperator(this string code, List<Company> companies)
        {
            string companyName = "UNKNOWN";
            Company company = companies.FirstOrDefault(x => x.Number == code);
            if (company != null) 
            {
                companyName = company.CompanyName;
            }
            return companyName;
        }

        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            var batch = new List<T>(size);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == size)
                {
                    yield return new List<T>(batch);
                    batch.Clear();
                }
            }
            if (batch.Any())
                yield return batch;
        }
    }
}

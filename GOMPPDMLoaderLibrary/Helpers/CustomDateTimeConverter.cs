using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.TypeConversion;

namespace GOMPPDMLoaderLibrary.Helpers
{
    public class CustomDateTimeConverter : DateTimeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            // Check if the string is not empty and has the expected format
            if (!string.IsNullOrEmpty(text) && text.Length == 8)
            {
                // Parse the date string with the specified format
                return DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture);
            }

            // If the string is empty or doesn't match the expected format, return the default DateTime
            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CompatibilityApp.Domain.Common
{public static class ParseUtils
    {
        public static int? ParseIntOrNull(object? v)
        {

            string parseValue = v?.ToString();


            parseValue = new string(parseValue.Where(i => char.IsDigit(i) || i == '-').ToArray());


            return int.TryParse(parseValue, out var d) ? d : 0;
        }

        public static decimal? ParseDecimalOrNull(object? v)
        {

            string parseValue = v?.ToString();


            parseValue = new string(parseValue.Where(i => char.IsDigit(i) || i == '.' || i == '-').ToArray());


            return decimal.TryParse(parseValue, out var d) ? d : 0;
        }
    }
}

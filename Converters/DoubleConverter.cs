using Google.Cloud.Firestore;
using System;

namespace PapeleriaApi.Converters
{
    public class DoubleConverter : IFirestoreConverter<double>
    {
        public double FromFirestore(object value)
        {
            if (value is double)
                return (double)value;
            
            if (value is string strValue && double.TryParse(strValue, out var result))
                return result;
                
            if (value is long longValue)
                return Convert.ToDouble(longValue);
                
            if (value is int intValue)
                return Convert.ToDouble(intValue);
                
            throw new ArgumentException($"Unable to convert {value?.GetType()} to double");
        }


        public object ToFirestore(double value) => value;
    }
}

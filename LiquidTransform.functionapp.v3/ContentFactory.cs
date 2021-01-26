using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v3
{
    public static class ContentFactory
    {
        public static IContentReader GetContentReader(string contentType, string csvColumnDelimiter)
        {
            switch (contentType)
            {
                case "application/xml":
                case "text/xml":
                    return new XmlContentReader();
                case "text/csv":
                    return new CsvContentReader(csvColumnDelimiter);
                default:
                    return new JsonContentReader();
            }
        }

        public static IContentWriter GetContentWriter(string contentType)
        {
            switch (contentType)
            {
                case "application/json":
                    return new JsonContentWriter(contentType);
                default:
                    return new BasicContentWriter(contentType);
            }
        }
    }
}

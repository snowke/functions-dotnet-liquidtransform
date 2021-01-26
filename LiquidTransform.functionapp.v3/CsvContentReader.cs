using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace LiquidTransform.functionapp.v3
{
    public class CsvContentReader : IContentReader
    {
        private readonly string csvColumnDelimiter;

        /// <summary>
        ///     Initiate the CvsContextReader with a column delimeter
        /// </summary>
        /// <param name="delimiter">If left null or empty will default to comma</param>
        public CsvContentReader(string csvColumnDelimiter)
        {
            if (string.IsNullOrEmpty(csvColumnDelimiter))
            {
                csvColumnDelimiter = ",";
            }
            this.csvColumnDelimiter = csvColumnDelimiter;
        }

        public async Task<Hash> ParseRequestAsync(HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();

            var transformInput = new Dictionary<string, object>();


            List<object[]> csv = new List<object[]>();

            StreamReader sr = new StreamReader(stream);
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();

                csv.Add(line.Split(csvColumnDelimiter));
            }

            transformInput.Add("content", csv.ToArray<object>());

            return Hash.FromDictionary(transformInput);
        }
    }
}

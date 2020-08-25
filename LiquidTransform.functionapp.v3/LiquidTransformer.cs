using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using DotLiquid;
using Newtonsoft.Json.Linq;

namespace LiquidTransform.functionapp.v2
{
    public static class JsonHelper
    {
        // this is a replace of JObject.Parce() because some errors when dotliquid try to render template
        public static object Deserialize(string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                        .ToDictionary(prop => prop.Name,
                                      prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }
    }
    public static class LiquidTransformer
    {
        /// <summary>
        /// Converts Json to XML using a Liquid mapping. The filename of the liquid map needs to be provided in the path. 
        /// The tranformation is executed with the HTTP request body as input.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="inputBlob"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("LiquidTransformer")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "liquidtransformer/{liquidtransformfilename}")] HttpRequestMessage req,
            [Blob("liquid-transforms/{liquidtransformfilename}", FileAccess.Read)] Stream inputBlob,
            ILogger log)
        {
            HttpResponseMessage responseMessage = null;

            log.LogInformation("C# HTTP trigger function processed a request.");

            if (inputBlob == null)
            {
                log.LogError("inputBlob null");

                responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
                responseMessage.Content = new StringContent("Liquid transform not found");
            }

            // This indicates the response content type. If set to application/json it will perform additional formatting
            // Otherwise the Liquid transform is returned unprocessed.
            string requestContentType = req.Content.Headers.ContentType.MediaType;
            string responseContentType = req.Headers.Accept.FirstOrDefault().MediaType;

            // Load the Liquid transform in a string\
            string liquidTransform = string.Empty;
            using(StreamReader sr = new StreamReader(inputBlob))
            {
                liquidTransform = await sr.ReadToEndAsync();
            }

            var contentReader = ContentFactory.GetContentReader(requestContentType);
            var contentWriter = ContentFactory.GetContentWriter(responseContentType);

            Hash inputHash = null;

            try
            {
                var json = await req.Content.ReadAsStringAsync();
                
                inputHash = Hash.FromDictionary((dynamic)JsonHelper.Deserialize(json));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                
                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.Content = new StringContent("Error parsing request body");
            }

            // Register the Liquid custom filter extensions
            Template.RegisterFilter(typeof(CustomFilters));

            // Execute the Liquid transform
            Template template = null;

            try
            {
                template = Template.Parse(liquidTransform);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.Content = new StringContent("Error parsing Liquid template");
            }

            string output = string.Empty;

            try
            {
                output = template.Render(inputHash);
                log.LogInformation(output);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.Content = new StringContent("Error rendering Liquid template");
            }

            if (template.Errors != null && template.Errors.Count > 0)
            {
                if (template.Errors[0].InnerException != null)
                {
                    log.LogError(template.Errors[0].InnerException, "Error rendering Liquid template: {error}", template.Errors[0].Message);

                    responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    responseMessage.Content = new StringContent($"Error rendering Liquid template: {template.Errors[0].Message}");
                }
                else
                {
                    log.LogError("Error rendering Liquid template: {error}", template.Errors[0].Message);

                    responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    responseMessage.Content = new StringContent($"Error rendering Liquid template: {template.Errors[0].Message}");
                }
            }
            else
            {
                try
                {
                    var content = contentWriter.CreateResponse(output);

                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    responseMessage.Content = content;
                }
                catch (Exception ex)
                {
                    // Just log the error, and return the Liquid output without parsing
                    log.LogError(ex.Message, ex);

                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                    responseMessage.Content = new StringContent(output, Encoding.UTF8, responseContentType);
                }
            }

            return responseMessage;
        }
    }
}

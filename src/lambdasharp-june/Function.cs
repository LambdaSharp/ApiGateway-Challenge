using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace lambdasharp_june
{
    public class Function
    {
        private const string BUCKET_NAME = "lambdasharp-june";
        private const string FILE_NAME = "data.json";
        Dictionary<string, string> Headers;
        JsonSerializerSettings JsonSettings;
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            Headers = new Dictionary<string, string> {
                {"Access-Control-Allow-Origin", "*"},
                {"Access-Control-Allow-Headers", "*"},
                {"Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS"}
            };
            JsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest evnt, ILambdaContext context)
        {
            Console.WriteLine(JsonConvert.SerializeObject(evnt));

            // Retrieve todos from datastore (s3)
            var todos = await GetDataFromS3();

            // Determine which method to execute
            switch(evnt.HttpMethod) {
                case "GET":
                    return GetAPIGatewayResponse(HttpStatusCode.OK, todos);
                case "POST":
                    // Add Todo
                case "PUT":
                    // Update Todo
                case "DELETE":
                    // Delete Todo
                default:
                    throw new NotImplementedException("Http Method not implemented");
            }
        }

        private async Task<IEnumerable<TodoItem>> GetDataFromS3() {
            var responseBody = "";
            try
            {
                var request = new GetObjectRequest {
                    BucketName = BUCKET_NAME,
                    Key = FILE_NAME
                };
                using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseBody = reader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error getting object {FILE_NAME} from bucket {BUCKET_NAME}. Make sure they exist and your bucket is in the same region as this function.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
            return JsonConvert.DeserializeObject<List<TodoItem>>(responseBody);
        }

        private APIGatewayProxyResponse GetAPIGatewayResponse(HttpStatusCode statusCode, object responseContent) {
            return new APIGatewayProxyResponse {
                Headers = this.Headers,
                StatusCode = (int) statusCode,
                Body = JsonConvert.SerializeObject(responseContent, JsonSettings)
            };
        }
    }
}

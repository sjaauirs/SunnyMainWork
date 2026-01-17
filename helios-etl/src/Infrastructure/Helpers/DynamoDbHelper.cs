using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using System.Net;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class DynamoDbHelper : AwsConfiguration, IDynamoDbHelper
    {
        private readonly ILogger<DynamoDbHelper> _logger;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        const string className = nameof(DynamoDbHelper);

        public DynamoDbHelper(ILogger<DynamoDbHelper> logger, IVault vault, IConfiguration configuration)
            : base(vault, configuration)
        {
            _vault = vault;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ScanResponse> ScanAsync(ScanRequest scanRequest)
        {
            const string methodName = nameof(ScanAsync);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing ScanAsync from dynamo db request: {Request}",
                    className, methodName, scanRequest.ToJson());
                using (var dynamoDbClient = new AmazonDynamoDBClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var response = await dynamoDbClient.ScanAsync(scanRequest);

                    _logger.LogInformation("{className}.{methodName}: ScanAsync from DynamoDb is successful",
                        className, methodName);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while ScanAsync from DynamoDB - ERROR Msg:{msg}, request:{Request}",
                    className, methodName, ex.Message, scanRequest.ToJson());
                throw;
            }
        }

        /// <summary>
        /// GetItemAsync
        /// </summary>
        /// <param name="getItemRequest"></param>
        /// <returns></returns>
        public async Task<T> GetItemAsync<T>(GetItemRequest getItemRequest)
        {
            const string methodName = nameof(GetItemAsync);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing GetItemAsync from dynamo db request: {Request}",
                    className, methodName, getItemRequest.ToJson());
                using (var dynamoDbClient = new AmazonDynamoDBClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var itemResponse = await dynamoDbClient.GetItemAsync(getItemRequest);
                    if (itemResponse.HttpStatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("{className}.{methodName}:Error while GetItemAsync from DynamoDB - response:{response}",
                            className, methodName, itemResponse.ToJson());
                        throw new Exception(message: $"Error occurred while fetching item from DynamoDB using request: {getItemRequest.ToJson()}");
                    }

                    var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
                    var context = new DynamoDBContext(dynamoDbClient, config);
                    var document = Document.FromAttributeMap(itemResponse.Item);
                    var response = context.FromDocument<T>(document);
                    _logger.LogInformation("{className}.{methodName}: GetItemAsync from DynamoDb is successful",
                        className, methodName);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while GetItemAsync from DynamoDB - ERROR Msg:{msg}, request:{Request}",
                    className, methodName, ex.Message, getItemRequest.ToJson());
                throw;
            }
        }

        /// <summary>
        /// PutItemAsync
        /// </summary>
        /// <param name="putItemRequest"></param>
        /// <returns></returns>
        public async Task<PutItemResponse> PutItemAsync(PutItemRequest putItemRequest)
        {
            const string methodName = nameof(PutItemAsync);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing PutItemAsync to dynamo db request: {Request}",
                    className, methodName, putItemRequest.ToJson());
                using (var dynamoDbClient = new AmazonDynamoDBClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var response = await dynamoDbClient.PutItemAsync(putItemRequest);
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("{className}.{methodName}:Error while PutItemAsync in DynamoDB - response:{response}",
                            className, methodName, response.ToJson());
                        throw new Exception(message: $"Error occurred while inserting item into DynamoDB using request: {putItemRequest.ToJson()}");
                    }
                    _logger.LogInformation("{className}.{methodName}: PutItemAsync to DynamoDb is successful",
                        className, methodName);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while PutItemAsync to DynamoDB - ERROR Msg:{msg}, request:{Request}",
                    className, methodName, ex.Message, putItemRequest.ToJson());
                throw;
            }
        }

        /// <summary>
        /// UpdateItemAsync
        /// </summary>
        /// <param name="updateItemRequest"></param>
        /// <returns></returns>
        public async Task<UpdateItemResponse> UpdateItemAsync(UpdateItemRequest updateItemRequest)
        {
            const string methodName = nameof(UpdateItemAsync);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing UpdateItemAsync to dynamo db request: {Request}",
                    className, methodName, updateItemRequest.ToJson());
                using (var dynamoDbClient = new AmazonDynamoDBClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var response = await dynamoDbClient.UpdateItemAsync(updateItemRequest);
                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("{className}.{methodName}:Error while UpdateItemAsync in DynamoDB - response:{response}",
                            className, methodName, response.ToJson());
                        throw new Exception(message: $"Error occurred while updating item in DynamoDB using request: {updateItemRequest.ToJson()}");
                    }
                    _logger.LogInformation("{className}.{methodName}: UpdateItemAsync to DynamoDb is successful",
                        className, methodName);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while UpdateItemAsync to DynamoDB - ERROR Msg:{msg}, request:{Request}",
                    className, methodName, ex.Message, updateItemRequest.ToJson());
                throw;
            }
        }
    }
}
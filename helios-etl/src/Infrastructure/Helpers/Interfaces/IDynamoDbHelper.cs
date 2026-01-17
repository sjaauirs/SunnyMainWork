using Amazon.DynamoDBv2.Model;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface IDynamoDbHelper
    {
        Task<ScanResponse> ScanAsync(ScanRequest scanRequest);
        Task<T> GetItemAsync<T>(GetItemRequest getItemRequest);
        Task<PutItemResponse> PutItemAsync(PutItemRequest putItemRequest);
        Task<UpdateItemResponse> UpdateItemAsync(UpdateItemRequest updateItemRequest);
    }
}
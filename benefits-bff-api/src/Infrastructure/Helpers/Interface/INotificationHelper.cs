namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
    public interface INotificationHelper
    {
        Task ProcessNotification(string tenantCode, string consumerCode, string eventTypeName);
    }
}

namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
    public interface ICardOperationsHelper
    {
        string? ExtractCardStatusFromFisResponse(string? fisResponse);
    }
}

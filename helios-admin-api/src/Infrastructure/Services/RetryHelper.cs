using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public static class RetryHelper
    {
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> action,
            ILogger? logger = null,
            int maxRetries = 3,
            int initialDelayMs = 500,
            Func<int, int>? delayUntil = null)
        {
            if (delayUntil == null)
                delayUntil = attempt => initialDelayMs * (int)Math.Pow(2, attempt - 1); // exponential

            Exception? lastEx = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    logger?.LogDebug("Attempt {Attempt} of {MaxRetries}", attempt, maxRetries);
                    return await action();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    logger?.LogWarning(ex, "Attempt {Attempt} failed: {Message}", attempt, ex.Message);

                    if (attempt < maxRetries)
                    {
                        var delay = delayUntil(attempt);
                        logger?.LogInformation("Retrying after {Delay}ms...", delay);
                        await System.Threading.Tasks.Task.Delay(delay);
                    }
                }
            }

            logger?.LogError(lastEx, "Operation failed after {MaxRetries} retries.", maxRetries);
            throw new Exception($"Operation failed after {maxRetries} retries.", lastEx);
        }
    }
}

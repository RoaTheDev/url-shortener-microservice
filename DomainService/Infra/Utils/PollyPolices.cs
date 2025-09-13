using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace DomainService.Infra.Utils;

public static class PollyPolicies
{
    // Retry for transient failures
    public static AsyncRetryPolicy RetryPolicy => Policy
        .Handle<Exception>() // or more specific exceptions
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
            (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
            });

    // Circuit breaker to stop hammering a failing system
    public static AsyncCircuitBreakerPolicy CircuitBreakerPolicy => Policy
        .Handle<Exception>() 
        .CircuitBreakerAsync(
            2, 
            TimeSpan.FromSeconds(30),
            onBreak: (ex, breakDelay) =>
            {
                Console.WriteLine($"Circuit opened for {breakDelay.TotalSeconds}s due to: {ex.Message}");
            },
            onReset: () => Console.WriteLine("Circuit closed, operations resumed."),
            onHalfOpen: () => Console.WriteLine("Circuit half-open, trial call allowed.")
        );

    // Wrap retry + circuit breaker
    public static AsyncPolicy WrapPolicy => Policy.WrapAsync(RetryPolicy, CircuitBreakerPolicy);
}

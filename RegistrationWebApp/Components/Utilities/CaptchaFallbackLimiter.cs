using System.Collections.Concurrent;

namespace RegistrationWebApp.Components.Utilities;

/// <summary>
/// Rate limits per-IP use of the no-reCAPTCHA fallback, for users whose browser cannot load reCAPTCHA.
/// </summary>
public class CaptchaFallbackLimiter
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, Queue<DateTime>> attempts = new();

    public bool TryAcquire(string? ipAddress)
    {
        DateTime now = DateTime.UtcNow;
        Queue<DateTime> queue = attempts.GetOrAdd(ipAddress ?? "unknown", _ => new Queue<DateTime>());
        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() > Window)
                queue.Dequeue();

            if (queue.Count >= MaxAttempts)
                return false;

            queue.Enqueue(now);
            return true;
        }
    }
}

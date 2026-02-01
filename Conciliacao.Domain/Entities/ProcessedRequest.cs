public class ProcessedRequest
{
    public Guid Id { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string ResultHash { get; private set; } = string.Empty;

    public DateTime ProcessedAt { get; private set; }

    private ProcessedRequest() { }

    public ProcessedRequest(string idempotencyKey, string resultHash)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        ResultHash = resultHash;
        ProcessedAt = DateTime.UtcNow;
    }
}
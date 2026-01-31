namespace Conciliacao.Domain.Entities
{
    public class ProcessedRequest
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string IdempotencyKey { get; private set; } = default!;

        public string ResultHash { get; private set; } = default!;

        public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;

        private ProcessedRequest() { } // EF

        public ProcessedRequest(string idempotencyKey, string resultHash)
        {
            IdempotencyKey = idempotencyKey;
            ResultHash = resultHash;
        }
    }
}
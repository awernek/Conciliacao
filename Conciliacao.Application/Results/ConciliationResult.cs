namespace Conciliacao.Application.Results
{
    /// <summary>
    /// Resultado de uma conciliação. Pode ser serializado em payload (Success:ProcessedCount)
    /// para armazenar em ProcessedRequest e reconstruir em requisições duplicadas (idempotência).
    /// </summary>
    public class ConciliationResult
    {
        public bool Success { get; }
        public int ProcessedCount { get; }

        private ConciliationResult(bool success, int processedCount)
        {
            Success = success;
            ProcessedCount = processedCount;
        }

        public static ConciliationResult SuccessResult(int count)
            => new(true, count);

        /// <summary>
        /// Serializa o resultado em string para persistir (ex.: "True:3").
        /// Usado para reconstruir o resultado em FromPayload quando a requisição é duplicada.
        /// </summary>
        public string ToPayload()
        {
            return $"{Success}:{ProcessedCount}";
        }

        /// <summary>
        /// Reconstrói o resultado a partir do payload salvo em ProcessedRequest.
        /// Garante que a segunda requisição (mesma chave idempotente) retorna o mesmo resultado.
        /// </summary>
        public static ConciliationResult FromPayload(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return new ConciliationResult(true, 0);

            var parts = payload.Split(':', 2, StringSplitOptions.None);
            if (parts.Length != 2)
                return new ConciliationResult(true, 0);

            var success = parts[0].Equals("True", StringComparison.OrdinalIgnoreCase);
            var count = int.TryParse(parts[1], out var n) ? n : 0;
            return new ConciliationResult(success, count);
        }
    }
}
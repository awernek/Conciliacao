using System.Security.Cryptography;
using System.Text;

namespace Conciliacao.Application.Results
{
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

        public string ToHash()
        {
            var raw = $"{Success}-{ProcessedCount}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }

        public static ConciliationResult FromHash(string hash)
        {
            // simplificado para o exemplo
            return new ConciliationResult(true, 0);
        }
    }
}
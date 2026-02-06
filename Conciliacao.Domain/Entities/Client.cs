namespace Conciliacao.Domain.Entities
{
    public class Client
    {
        public string Code { get; private set; }

        protected Client() { Code = string.Empty; }

        public Client(string code)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }
    }
}
using Conciliacao.Domain.Exceptions;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infra.Persistence
{
    /// <summary>
    /// Implementação do Unit of Work que coordena o commit das alterações no banco.
    /// Traduz exceções de infraestrutura (violação de chave única do SQL Server)
    /// em exceções de domínio, isolando a camada Application de dependências de EF Core.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConciliationDbContext _context;

        public UnitOfWork(ConciliationDbContext context)
        {
            _context = context;
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new DuplicateKeyException(
                    "Violação de chave única detectada.", ex);
            }
        }

        /// <summary>
        /// SQL Server: 2601 → Duplicate key row, 2627 → Violation of UNIQUE constraint.
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx
                && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}

using Dapper;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Data;

namespace Kartverket.Web.Repositories.IdentityStores
{
    // Dapper-based implementation of IRoleStore without EF Core
    public class KartverketRoleStore : IRoleStore<IdentityRole>
    {
        private readonly string _connectionString;
        private const string RolesTable = "AspNetRoles"; // default Identity roles table

        public KartverketRoleStore(IConfiguration configuration)
        {
            // Uses same connection string key as rest of app (kartverketdb)
            _connectionString = configuration.GetConnectionString("kartverketdb")!;
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            role.ConcurrencyStamp ??= Guid.NewGuid().ToString();
            const string sql = $"INSERT INTO {RolesTable} (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, role, cancellationToken: cancellationToken));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            const string sql = $"DELETE FROM {RolesTable} WHERE Id = @Id";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, new { role.Id }, cancellationToken: cancellationToken));
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            // Nothing to dispose; connections are disposed per operation.
        }

        public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = $"SELECT Id, Name, NormalizedName, ConcurrencyStamp FROM {RolesTable} WHERE Id = @Id LIMIT 1";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<IdentityRole>(new CommandDefinition(sql, new { Id = roleId }, cancellationToken: cancellationToken));
        }

        public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = $"SELECT Id, Name, NormalizedName, ConcurrencyStamp FROM {RolesTable} WHERE NormalizedName = @NormalizedName LIMIT 1";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<IdentityRole>(new CommandDefinition(sql, new { NormalizedName = normalizedRoleName }, cancellationToken: cancellationToken));
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(role);
            role.ConcurrencyStamp ??= Guid.NewGuid().ToString();
            const string sql = $"UPDATE {RolesTable} SET Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp WHERE Id = @Id";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, role, cancellationToken: cancellationToken));
            return IdentityResult.Success;
        }
    }
}

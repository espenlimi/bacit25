using MySqlConnector;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Kartverket.Aspire.AppHost.MariaDb
{
    public class MariaDbDatabaseResource(string name, string databaseName, MariaDbServerResource parent)
    : Resource(name), IResourceWithParent<MariaDbServerResource>, IResourceWithConnectionString
    {
        /// <summary>
        /// Gets the parent MySQL container resource.
        /// </summary>
        public MariaDbServerResource Parent { get; } = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <summary>
        /// Gets the connection string expression for the MySQL database.
        /// </summary>
        public ReferenceExpression ConnectionStringExpression
        {
            get
            {
                var connectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    ["Database"] = DatabaseName
                };

                return ReferenceExpression.Create($"{Parent};{connectionStringBuilder.ToString()};AllowUserVariables=True;UseAffectedRows=False");
            }
        }
        /// <summary>
        /// Gets the database name.
        /// </summary>
        public string DatabaseName { get; } = ThrowIfNullOrEmpty(databaseName);

        private static string ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            return argument;
        }
    }
}

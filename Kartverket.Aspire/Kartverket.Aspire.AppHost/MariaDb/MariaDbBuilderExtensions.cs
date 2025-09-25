using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Kartverket.Aspire.AppHost.MariaDb
{
    public static class MariaDbBuilderExtensions
    {
        private const string PasswordEnvVarName = "MARIADB_ROOT_PASSWORD";
        private const UnixFileMode FileMode644 = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead;

        /// <summary>
        /// Adds a MySQL server resource to the application model. For local development a container is used.
        /// </summary>
        /// <remarks>
        /// This version of the package defaults to the <inheritdoc cref="MySqlContainerImageTags.Tag"/> tag of the <inheritdoc cref="MySqlContainerImageTags.Image"/> container image.
        /// </remarks>
        /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
        /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
        /// <param name="password">The parameter used to provide the root password for the MySQL resource. If <see langword="null"/> a random password will be generated.</param>
        /// <param name="port">The host port for MySQL.</param>
        /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
        public static IResourceBuilder<MariaDbServerResource> AddMariaDb(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? password = null, int? port = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(name);

            var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password",true,true,true,false);

            var resource = new MariaDbServerResource(name, passwordParameter);

            string? connectionString = null;

            builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(resource, async (@event, ct) =>
            {
                connectionString = await resource.ConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false);

                if (connectionString == null)
                {
                    throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{resource.Name}' resource but the connection string was null.");
                }
            });

            builder.Eventing.Subscribe<ResourceReadyEvent>(resource, async (@event, ct) =>
            {
                if (connectionString is null)
                {
                    throw new DistributedApplicationException($"ResourceReadyEvent was published for the '{resource.Name}' resource but the connection string was null.");
                }

                using var sqlConnection = new MySqlConnection(connectionString);
                await sqlConnection.OpenAsync(ct).ConfigureAwait(false);

                if (sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    throw new InvalidOperationException($"Could not open connection to '{resource.Name}'");
                }

                foreach (var sqlDatabase in resource.DatabaseResources)
                {
                    await CreateDatabaseAsync(sqlConnection, sqlDatabase, @event.Services, ct).ConfigureAwait(false);
                }
            });

            var healthCheckKey = $"{name}_check";
            builder.Services.AddHealthChecks().AddMySql(sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"), name: healthCheckKey);

            return builder.AddResource(resource)
                          .WithEndpoint(port: port, targetPort: 3306, name: MariaDbServerResource.PrimaryEndpointName) // Internal port is always 3306.
                          .WithImage("mariadb", "lts-ubi9")
                          .WithImageRegistry("docker.io")
                          .WithEnvironment(context =>
                          {
                              context.EnvironmentVariables[PasswordEnvVarName] = resource.PasswordParameter;
                          })
                          .WithHealthCheck(healthCheckKey);
        }

        /// <summary>
        /// Adds a MySQL database to the application model.
        /// </summary>
        /// <param name="builder">The MySQL server resource builder.</param>
        /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
        /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
        /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
        public static IResourceBuilder<MariaDbDatabaseResource> AddDatabase(this IResourceBuilder<MariaDbServerResource> builder, [ResourceName] string name, string? databaseName = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(name);

            // Use the resource name as the database name if it's not provided
            databaseName ??= name;

            var mySqlDatabase = new MariaDbDatabaseResource(name, databaseName, builder.Resource);

            builder.Resource.AddDatabase(mySqlDatabase);

            string? connectionString = null;

            builder.ApplicationBuilder.Eventing.Subscribe<ConnectionStringAvailableEvent>(mySqlDatabase, async (@event, ct) =>
            {
                connectionString = await mySqlDatabase.ConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false);

                if (connectionString is null)
                {
                    throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{name}' resource but the connection string was null.");
                }
            });

            var healthCheckKey = $"{name}_check";
            builder.ApplicationBuilder.Services.AddHealthChecks().AddMySql(sp => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"), name: healthCheckKey);

            return builder.ApplicationBuilder
                .AddResource(mySqlDatabase)
                .WithHealthCheck(healthCheckKey);
        }

        private static async Task CreateDatabaseAsync(MySqlConnection sqlConnection, MariaDbDatabaseResource sqlDatabase, IServiceProvider serviceProvider, CancellationToken ct)
        {
            var logger = serviceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(sqlDatabase.Parent);

            logger.LogDebug("Creating database '{DatabaseName}'", sqlDatabase.DatabaseName);

            try
            {
                var scriptAnnotation = sqlDatabase.Annotations.OfType<MariaDbCreateDatabaseScriptAnnotation>().LastOrDefault();

                if (scriptAnnotation?.Script is null)
                {
                    var quotedDatabaseIdentifier = new MySqlCommandBuilder().QuoteIdentifier(sqlDatabase.DatabaseName);
                    using var command = sqlConnection.CreateCommand();
                    command.CommandText = $"CREATE DATABASE IF NOT EXISTS {quotedDatabaseIdentifier};";
                    await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    using var command = sqlConnection.CreateCommand();
                    command.CommandText = scriptAnnotation.Script;
                    await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                logger.LogDebug("Database '{DatabaseName}' created successfully", sqlDatabase.DatabaseName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create database '{DatabaseName}'", sqlDatabase.DatabaseName);
            }
        }

        /// <summary>
        /// Defines the SQL script used to create the database.
        /// </summary>
        /// <param name="builder">The builder for the <see cref="MySqlDatabaseResource"/>.</param>
        /// <param name="script">The SQL script used to create the database.</param>
        /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
        /// <remarks>
        /// <value>Default script is <code>CREATE DATABASE IF NOT EXISTS `QUOTED_DATABASE_NAME`;</code></value>
        /// </remarks>
        public static IResourceBuilder<MariaDbDatabaseResource> WithCreationScript(this IResourceBuilder<MariaDbDatabaseResource> builder, string script)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(script);

            builder.WithAnnotation(new MariaDbCreateDatabaseScriptAnnotation(script));

            return builder;
        }

  
        /// <summary>
        /// Adds a named volume for the data folder to a MySql container resource.
        /// </summary>
        /// <param name="builder">The resource builder.</param>
        /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
        /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public static IResourceBuilder<MariaDbServerResource> WithDataVolume(this IResourceBuilder<MariaDbServerResource> builder, string? name = null, bool isReadOnly = false)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/var/lib/mariadb", isReadOnly);
        }

        /// <summary>
        /// Adds a bind mount for the data folder to a MySql container resource.
        /// </summary>
        /// <param name="builder">The resource builder.</param>
        /// <param name="source">The source directory on the host to mount into the container.</param>
        /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public static IResourceBuilder<MariaDbServerResource> WithDataBindMount(this IResourceBuilder<MariaDbServerResource> builder, string source, bool isReadOnly = false)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(source);

            return builder.WithBindMount(source, "/var/lib/mariadb", isReadOnly);
        }

        /// <summary>
        /// Adds a bind mount for the init folder to a MySql container resource.
        /// </summary>
        /// <param name="builder">The resource builder.</param>
        /// <param name="source">The source directory on the host to mount into the container.</param>
        /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        [Obsolete("Use WithInitFiles instead.")]
        public static IResourceBuilder<MariaDbServerResource> WithInitBindMount(this IResourceBuilder<MariaDbServerResource> builder, string source, bool isReadOnly = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(source);

            return builder.WithBindMount(source, "/docker-entrypoint-initdb.d", isReadOnly);
        }

        /// <summary>
        /// Copies init files into a MySql container resource.
        /// </summary>
        /// <param name="builder">The resource builder.</param>
        /// <param name="source">The source file or directory on the host to copy into the container.</param>
        /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
        public static IResourceBuilder<MariaDbServerResource> WithInitFiles(this IResourceBuilder<MariaDbServerResource> builder, string source)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(source);

            const string initPath = "/docker-entrypoint-initdb.d";

            var importFullPath = Path.GetFullPath(source, builder.ApplicationBuilder.AppHostDirectory);

            return builder.WithContainerFiles(initPath, importFullPath);
        }

    
    }
}

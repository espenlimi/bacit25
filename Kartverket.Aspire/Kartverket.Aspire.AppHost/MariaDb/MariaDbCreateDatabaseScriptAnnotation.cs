namespace Kartverket.Aspire.AppHost.MariaDb
{
    internal sealed class MariaDbCreateDatabaseScriptAnnotation : IResourceAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MariaDbCreateDatabaseScriptAnnotation"/> class.
        /// </summary>
        /// <param name="script">The script used to create the database.</param>
        public MariaDbCreateDatabaseScriptAnnotation(string script)
        {
            ArgumentNullException.ThrowIfNull(script);
            Script = script;
        }

        /// <summary>
        /// Gets the script used to create the database.
        /// </summary>
        public string Script { get; }
    }
}

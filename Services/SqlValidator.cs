namespace SqlAgent.Services
{
    public class SqlValidator
    {
        public bool IsSafe(string sql)
        {
            if (String.IsNullOrWhiteSpace(sql)) {
                return false;
            }

            var upperSql = sql.Trim().ToUpper();

            // Must start with SELECT
            if (!upperSql.StartsWith("SELECT"))
                return false;

            if (!upperSql.Contains(";"))
                return false;

            // Block dangerous keywords
            var forbidden = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE" };

            return !forbidden.Any(f => upperSql.Contains(f));

        }

    }
}

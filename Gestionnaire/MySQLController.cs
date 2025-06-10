using MySql.Data.MySqlClient;

namespace Gestionnaire
{
    public class MySQLController
    {
        private readonly string connectionString =
            $"Server={Config.mysqlServer};Database={Config.mysqlDatabase};Uid={Config.mysqlUsername};Pwd={Config.mysqlPassword};Port={Config.mysqlPort};";

        public MySQLController()
        {
            /*
                Constructor for MySQLController.
                Initializes database connection and loads initial parameters.
            */
            Initialization();
            InsertSelekton();
        }

        private void Initialization()
        {
            try
            {
                Methodes.PrintConsole(Config.sourceMySQL, "Connexion au base de données, Veuillez patientez s'il vous plait...");

                using var controller = new MySqlConnection(connectionString);
                controller.Open();
                controller.Close();

                Methodes.PrintConsole(Config.sourceMySQL, "Connexion réussie, Initialization du program...");
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceMySQL, ex.ToString(), true);
            }
        }

        private void InsertSelekton()
        {
            try
            {
                Methodes.PrintConsole(Config.sourceMySQL, "Chargement des paramètres...");
                using var controller = new MySqlConnection(connectionString);
                controller.Open();
                string skeleton = Config.skeleton;
                var cmd = new MySqlCommand(skeleton, controller);
                _ = cmd.ExecuteNonQuery();
                controller.Close();
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceMySQL, ex.ToString(), true);
            }
        }

        public void InsertData(string query, Dictionary<string, object>? parameters = null)
        {
            /*
                Executes an insert or update query with optional parameters.
                Prints the number of affected rows in development mode.
                @param query - SQL insert/update string
                @param parameters - dictionary of query parameters (nullable)
            */
            using var controller = new MySqlConnection(connectionString);
            controller.Open();
            var command = new MySqlCommand(query, controller);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    _ = command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            int queryState = command.ExecuteNonQuery();
            controller.Close();

            if (!Config.productionRun)
            {
                if (queryState == 1) Methodes.PrintConsole(Config.sourceMySQL, "La ligne a été insérée/modifié dans la base de données.");
                else Methodes.PrintConsole(Config.sourceMySQL, $"Total de {queryState} ligne a été affectée.");   
            }
        }

        public List<QueryResultRow> ReadData(string query, Dictionary<string, object> parameters)
        {
            /*
                Executes a select query with parameters and returns the results as a list of rows.
                Each row contains column-value pairs as strings.
                @param query - SQL select string
                @param parameters - dictionary of query parameters
                @return list of QueryResultRow containing result set
            */
            var result = new List<QueryResultRow>();

            if (string.IsNullOrWhiteSpace(query))
            {
                Methodes.PrintConsole(Config.sourceMySQL, "Query cannot be null or empty", true);
                return result;
            }

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    _ = command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new QueryResultRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object? rawValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    string value = rawValue?.ToString() ?? string.Empty;
                    row.Columns[columnName] = value;
                }
                result.Add(row);
            }
            return result;
        }
    }
}

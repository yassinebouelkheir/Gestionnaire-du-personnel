using Moq;
using Xunit;
using Gestionnaire;
using MySql.Data.MySqlClient;

namespace Gestionnaire.Gestionnaire.Tests
{
    public class DatabaseTests
    {
        [Fact]
        public void InsertData_CreatesTableAndInserts()
        {
            var db = new MySQLController(true);

            db.InsertData("DROP TABLE IF EXISTS my_table");
            db.InsertData(@"CREATE TABLE my_table (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                test_col VARCHAR(255)
                            )");

            var ex = Record.Exception(() => db.InsertData(
                "INSERT INTO my_table (test_col) VALUES (@val)",
                new Dictionary<string, object> { { "@val", "clean insert" } }));

            Assert.Null(ex);
        }

        [Fact]
        public void ReadData_ReturnsInsertedValue()
        {
            var db = new MySQLController(true);

            db.InsertData("DROP TABLE IF EXISTS my_table");
            db.InsertData(@"CREATE TABLE my_table (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                test_col VARCHAR(255)
                            )");

            db.InsertData("INSERT INTO my_table (test_col) VALUES (@val)",
                new Dictionary<string, object> { { "@val", "read test" } });

            var result = db.ReadData("SELECT * FROM my_table WHERE test_col = @val",
                new Dictionary<string, object> { { "@val", "read test" } });

            Assert.Single(result);
            Assert.Equal("read test", result[0].Columns["test_col"]);
        }
    }
}

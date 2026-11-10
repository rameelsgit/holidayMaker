using MySqlConnector;

string connectionString = "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord;";

await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();

Console.WriteLine("funkar :)");
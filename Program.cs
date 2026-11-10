using HolidayMaker;
using MySqlConnector;

string connectionString = "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord;";

await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();

Console.WriteLine("funkar :)");

//repo variable executes queries and customers gets saved in the list 
CustomerRepository repo = new CustomerRepository(connection);
List<Customer> customers = await repo.GetCustomers();

Console.WriteLine("All Customerss");
foreach (Customer c in customers)
{
    Console.WriteLine($" {c.Id} |{c.Name}| {c.Email}");
}


// CustomerRepository customerRepo = new CustomerRepository(connection);
// Console.WriteLine("creating customer");
// int newCustomerId = await customerRepo.CreateCustomer(
//     "New Newsome", 
//     "Newsome@gmail.com", 
//     "999000999"
// );

// Console.WriteLine($"customer created,  {newCustomerId}");
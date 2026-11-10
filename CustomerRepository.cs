namespace HolidayMaker;

using MySqlConnector;

// class recieves the connection from program.cs
public class CustomerRepository(MySqlConnection connection)
{
    // method that will get all customers
    public async Task<List<Customer>> GetCustomers()
    {
        // await using, clears after getting information       
        await using var command = new MySqlCommand(
            "SELECT id, name, email, phone_number FROM customers ORDER BY id ASC;", 
            connection
        );
        // reads the matching information back 
        await using var reader = await command.ExecuteReaderAsync();
        // saves all customers that were found in empty list
        List<Customer>customers = new List<Customer>();

        // while loop that looks through the database line by line
        while (await reader.ReadAsync())
        {
            // adds every customer to the list 
            customers.Add(new Customer(
                reader.GetInt32("id"),          
                reader.GetString("name"),    
                reader.GetString("email"),        
                reader.GetString("phone_number")
            ));
        }
        return customers;
    }
    
    // method to create a customer
    public async Task<int> CreateCustomer(string name, string email, string phone)
    {
        await using var command = new MySqlCommand(
            "INSERT INTO customers (name, email, phone_number) VALUES (@name, @email, @phone);", 
            connection
        );
        // placeholder gets connected with actual data of customer
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@phone", phone);
        // excecutes insert query 
        await command.PrepareAsync();
        await command.ExecuteNonQueryAsync();
        // returns the auto increment id
        return (int)command.LastInsertedId;
    }
    
    
    };
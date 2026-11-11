using HolidayMaker;
using MySqlConnector;

string connectionString = "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord;";

await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();
//instanciates cutomerrepository class, opens connection
CustomerRepository customerRepo = new CustomerRepository(connection);

//variable stores selected customer, starts as null
Customer? selectedCustomer = null;

// while-loop active until operator exits menu
bool activeMenu = true;
while (activeMenu)
{
    Console.WriteLine("-- -- HOLIDAY MAKER -- --");
    
    // checks if customer is selected
    if (selectedCustomer != null)
    {
        Console.WriteLine($"Customer: {selectedCustomer.Name}");
    }
    else
    {
        Console.WriteLine("No Customer Chosen\n");
    }
    // menu options
    Console.WriteLine("1. Show all customers");
    Console.WriteLine("2. Register new customer");
    Console.WriteLine("3. Choose customer");
    Console.WriteLine("4. Exit");
    Console.WriteLine("\n");
    Console.Write("Choose an option 1-4: ");

    // stores operators choice in choice variable to know which case to use
    string? choice = Console.ReadLine();
    Console.WriteLine();
    switch (choice)
    {
        case "1":
            await ListCustomers(customerRepo);
            break;
        case "2":
            selectedCustomer = await CreatedCustomer(customerRepo);
            break;
        case "3":
            selectedCustomer = await ChooseCustomer(customerRepo);
            break;
        case "4":
            activeMenu = false;
            Console.WriteLine("Menu has been exited");
            break;
        default:
            Console.WriteLine("Not a valid option, try again");
            Wait();
            break;
    }
}
// method that shows existing customers
static async Task ListCustomers(CustomerRepository repo)
{
    Console.WriteLine("--EXISTING CUSTOMERS--");
    // returns list of customers
    List<Customer> customers = await repo.GetCustomers();
    //checks if list is empty
    if (customers.Count == 0)
    {
        Console.WriteLine("No customers found in the database.");
    }
    else
    {
        foreach (Customer c in customers)
        {
            Console.WriteLine($"ID: {c.Id}| Name: {c.Name} | Email: {c.Email} | Phone: {c.PhoneNumber}");
        }
    }
    Wait();
}
// method that creates customer 
static async Task<Customer?> CreatedCustomer(CustomerRepository repo)
{
    Console.WriteLine("--REGISTER NEW CUSTOMER--");
    Console.Write("Name: ");
    string name = Console.ReadLine()!;
    // checks if name is valid 
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Customer name cannot be empty");
        Wait();
        return null;
    }
    Console.Write("Email: ");
    string email = Console.ReadLine()!;
    // checks if email is valid
    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Email cannot be empty");
        Wait();
        return null;
    }

    Console.Write("Phone Number: ");
    string phone = Console.ReadLine()!;
    // check if phone number is empty
    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Number cannot be empty");
        Wait();
        return null;
    }
    // save customer to database with a new id
    int newId = await repo.CreateCustomer(name, email, phone);
    
    // creates the  new customer object
    Customer newCustomer = new Customer(newId, name, email, phone);
    Console.WriteLine($"\nCustomer has been registered. New Customer: id {newId}, {newCustomer}");
    Wait();
    
    return newCustomer;
}
// method for chooseing a customer
static async Task<Customer?> ChooseCustomer(CustomerRepository repo)
{
    Console.WriteLine("--CHOOSE A CUSTOMER--");
    // gets list of existing customers
    List<Customer> customers = await repo.GetCustomers();
    if (customers.Count == 0)
    {
        Console.WriteLine("No customers in the databas.");
        Wait();
        return null;
    }
    foreach (Customer c in customers)
    {
        Console.WriteLine($"ID: {c.Id} |{c.Name} |{c.Email}");
    }

    Console.Write("\nEnter the ID of the customer you wish to choose: ");
    string input = Console.ReadLine()!;

    // checks if input is empty    
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Customer ID cannot be empty, try again");
        Wait();
        return null;
    }

    // makes sure input gets converted to int
    if (int.TryParse(input, out int selectedId))
    {
        Customer? foundCustomer = null;

    // loops through each customer comparing the inputted id with the customers id
    foreach (Customer c in customers)
    {
        if (c.Id == selectedId)
        {
            foundCustomer = c;
            break;
        }
    }
        if (foundCustomer != null)
        {
            Console.WriteLine($"\nChosen Customer: {foundCustomer.Name}");
            Wait();
            return foundCustomer;
        }
    }
    // if customer id is not found
    Console.WriteLine($"\nNo customer found with that ID");
    Wait();
    return null;
}

static void Wait()
{
    Console.WriteLine("\nPress ENTER to return to Menu");
    Console.ReadLine();
}
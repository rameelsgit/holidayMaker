using HolidayMaker;
using MySqlConnector;

string connectionString = "Server=localhost;Database=holiday_maker;User ID=root;Password=kurslosenord;";

await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();
//instanciates cutomerrepository & bookingrepository class, opens connection
CustomerRepository customerRepo = new CustomerRepository(connection);
BookingRepository bookingRepo = new BookingRepository(connection);

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
    Console.WriteLine("4. Find available rooms");
    Console.WriteLine("5. Create a booking");
    Console.WriteLine("6. Exit");
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
            await SearchRooms(bookingRepo);
                break;
        case "5":
            await CreateBooking(bookingRepo, selectedCustomer);
            break;
        case "6":
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
    if (string.IsNullOrWhiteSpace(phone))
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

// method handles the searching of rooms
static async Task SearchRooms(BookingRepository repo)
{
    Console.WriteLine("-- SEARCH ROOMS --");
    // changed to input helpers as too much code was being repeated
    string city = InputHelper.ReadString("City: ");
    DateTime checkIn = InputHelper.ReadDate("Check-in (YYYY-MM-DD): ");
    DateTime checkOut = InputHelper.ReadCheckOutDate("Check-out (YYYY-MM-DD): ", checkIn);
    int guests = InputHelper.ReadInt("Guests: ");
    // matching available rooms with the database
    List<Room> rooms = await repo.SearchRooms(city, checkIn, checkOut, guests);

    // shows resukt of searched rooms
    Console.WriteLine($"\n--SEARCH RESULTS ({rooms.Count} found)--");
    if (rooms.Count == 0)
    {
        Console.WriteLine("No available rooms matched your search");
    }
    else
    {
        foreach (Room r in rooms)
        {
            Console.WriteLine($"Room ID: {r.Id} | Hotel: {r.HotelName} | Room No: {r.RoomNumber} | Price: {r.PricePerNight:C}/night");
        }
    }
    Wait();
}
// method to create a booking 
static async Task CreateBooking(BookingRepository repo, Customer? activeCustomer)
{
    // checks if a customer has been chosen for a booking
    if (activeCustomer == null)
    {
        Console.WriteLine("Choose a customer before creating a booking");
        Wait();
        return;
    }

    Console.WriteLine($"-- CREATE BOOKING FOR: {activeCustomer.Name} --");
    // using input helpers as the validation for these input fields was the same and too long
    int roomId = InputHelper.ReadInt("Enter Room ID to book: ");
    DateTime checkIn = InputHelper.ReadDate("Check-in (YYYY-MM-DD): ");
    DateTime checkOut = InputHelper.ReadCheckOutDate("Check-out (YYYY-MM-DD): ", checkIn);
    int guests = InputHelper.ReadInt("Number of guests: ");
    // calculating total price 
    int nights = (checkOut - checkIn).Days;
    decimal totalPrice = nights * 1000;
    // booking gets added to the database
    int bookingId = await repo.CreateBooking(activeCustomer.Id, roomId, checkIn, checkOut, guests, totalPrice);
    Console.WriteLine($"Booking completed | booking ID: {bookingId}");
    Console.WriteLine($"{nights} nights | Total price: {totalPrice:C}");
    Wait();
}

static void Wait()
{
    Console.WriteLine("\nPress ENTER to return to Menu");
    Console.ReadLine();
}
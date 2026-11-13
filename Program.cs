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
    Console.WriteLine("4. Search & book a room (choose customer first)");
    Console.WriteLine("5. Show customer bookings");
    Console.WriteLine("6. Update amount of guests");
    Console.WriteLine("7. Cancel a booking");
    Console.WriteLine("8. Exit");
    Console.WriteLine("\n");
    Console.Write("Choose an option 1-8: ");

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
            await SearchAndBookRoom(bookingRepo, selectedCustomer);
            break;
        case "5":
            await ShowCustomerBookings(bookingRepo, selectedCustomer);
            break;
        case "6":
            await UpdateGuests(bookingRepo);
            break;
        case "7":
            await DeleteBooking(bookingRepo);
            break;
        case "8":
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
    // checks if name, email, number is valid using the input helper
    string name = InputHelper.ReadString("Full name: ");
    string email = InputHelper.ReadString("Email: ");
    string phone = InputHelper.ReadString("Phone Number: ");
    // save customer to database with a new id
    int newId = await repo.CreateCustomer(name, email, phone);
    
    // creates the  new customer object
    Customer newCustomer = new Customer(newId, name, email, phone);
    Console.WriteLine($"\nCustomer has been registered. New Customer: id {newId}, {newCustomer}");
    Wait();
    
    return newCustomer;
}

// method for choosing a customer
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
    Console.WriteLine("\nNo customer found with that ID");
    Wait();
    return null;
}
// method combines search and book available room
static async Task SearchAndBookRoom(BookingRepository bookingRepo, Customer? activeCustomer)
{
    while (true)
    {
        Console.WriteLine("--SEARCH AVAILABLE ROOMS--");
        // search input using helpers for available rooms
        string city = InputHelper.ReadString("City: ");
        DateTime checkIn = InputHelper.ReadDate("Check-in (YYYY-MM-DD): ");
        DateTime checkOut = InputHelper.ReadCheckOutDate("Check-out (YYYY-MM-DD): ", checkIn);
        int guests = InputHelper.ReadInt("Number of guests: ");

        // optional filters to specify search using input helper
        Console.WriteLine("\n--FILTERS--");
        bool requirePool = InputHelper.ReadYesNo("Pool required? (y/n): ");
        bool requireRestaurant = InputHelper.ReadYesNo("Restaurant required? (y/n): ");

        // searches available rooms that match the criteria & displays results
        List<Room> availableRooms = await bookingRepo.SearchRooms(city, checkIn, checkOut, guests);
        Console.WriteLine($"\n-- SEARCH RESULTS ({availableRooms.Count} available) --");
        if (availableRooms.Count == 0)
        {
            Console.WriteLine("No rooms matched your search");
            if (InputHelper.ReadYesNo("Do you want to search again? (y/n): "))
            {
                Console.WriteLine();
                continue;
            }
            return;
        }

        foreach (Room r in availableRooms)
        {
            Console.WriteLine($"Room ID: {r.Id} Hotel: {r.HotelName} ({r.City}) | Room: {r.RoomNumber} | Capacity: {r.Capacity} | Price: {r.PricePerNight:C}/night");
        }

        // operator gets option to book a room or exit results
        if (!InputHelper.ReadYesNo("\nDo you want to book one of these rooms? (y/n): "))
        {
            return;
        }
        // makes sure a customer is chosen for a booking
        if (activeCustomer == null)
        {
            Console.WriteLine("\nNo customer chosen, choose or register a customer from the main menu first");
            Wait();
            return;
        }

        Room? selectedRoom = null;
        while (selectedRoom == null)
        {
            // operator choses an id to book 
            int chosenRoomId = InputHelper.ReadInt($"\nBooking for {activeCustomer.Name}, Enter room ID: ");
            // find the chosen room in availableRooms list to check if available
            foreach (Room r in availableRooms)
            {
                if (r.Id == chosenRoomId)
                {
                    selectedRoom = r;
                    break;
                }
            }
            // stop if id doesnt exist
            if (selectedRoom == null)
            {
                Console.WriteLine($"Room ID {chosenRoomId} is not in the available rooms list above. Try again.");
            }
        }

        // calculate nights and total price using the rooms pricepernight
        int nights = (checkOut - checkIn).Days;
        decimal totalPrice = nights * selectedRoom.PricePerNight;
        //saves booking to databas and displays result
        int bookingId = await bookingRepo.CreateBooking(activeCustomer.Id, selectedRoom.Id, checkIn, checkOut, guests, totalPrice);
        Console.WriteLine($"\nBooking completed. Booking ID: {bookingId}");
        Console.WriteLine($"Customer: {activeCustomer.Name}");
        Console.WriteLine($"Hotel: {selectedRoom.HotelName} | Room: {selectedRoom.RoomNumber}");
        Console.WriteLine($"{nights} nights x {selectedRoom.PricePerNight:C}/night | Total price: {totalPrice:C}");

        Wait();
        return;
    }
}

// method shows all of a customers bookings 
static async Task ShowCustomerBookings(BookingRepository repo, Customer? activeCustomer)
{   //makes sure customer is chosen
    if (activeCustomer == null)
    {
        Console.WriteLine("Please choose a customer first");
        Wait();
        return;
    }

    Console.WriteLine($"--BOOKINGS FOR {activeCustomer.Name} --");
    //gets list of bookings and checks if customer has a booking
    List<Booking> bookings = await repo.GetCustomerBookings(activeCustomer.Id);
    if (bookings.Count == 0)
    {
        Console.WriteLine("No bookings found");
    }
    else
    {
        // foreach booking that exixts, it gets displayed 
        foreach (Booking b in bookings)
        {
            Console.WriteLine($"Booking ID: {b.Id} Hotel: {b.HotelName} | Room: {b.RoomNumber}");
            Console.WriteLine($"Dates: {b.CheckIn:yyyy-MM-dd} to {b.CheckOut:yyyy-MM-dd} | Guests: {b.AmountGuests} | Total: {b.TotalPrice:C}\n");
        }
    }
    Wait();
}
// method updates a customers amount of guests
static async Task UpdateGuests(BookingRepository repo)
{
    Console.WriteLine("--UPDATE GUEST COUNT--");
    //uses my input helpers to validate input 
    int bookingId = InputHelper.ReadInt("Enter Booking ID: ");
    int newGuests = InputHelper.ReadInt("Enter new number of guests: ");
    
    // return true if update was successful
    bool success = await repo.UpdateGuests(bookingId, newGuests);
    if (success)
    {
        Console.WriteLine("Booking update completed");
    }
    else
    {
        Console.WriteLine("Failed to update. check booking ID or if guests exceed room capacity");
    }
    Wait();
}
// deletes abooking 
static async Task DeleteBooking(BookingRepository repo)
{
    Console.WriteLine("--CANCEL BOOKING--");

    // using inputhelper to validate input
    int bookingId = InputHelper.ReadInt("Enter Booking ID to cancel: ");

    // executes query to delete
    bool success = await repo.DeleteBooking(bookingId);
    if (success)
    {
        Console.WriteLine($"Booking ID {bookingId} canceled");
    }
    else
    {
        Console.WriteLine("Booking ID not found");
    }
    Wait();
}

static void Wait()
{
    Console.WriteLine("\nPress ENTER to return to Menu");
    Console.ReadLine();
}
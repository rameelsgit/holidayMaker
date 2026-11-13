namespace HolidayMaker;
using MySqlConnector;

public class BookingRepository(MySqlConnection connection)
{
    // gets all cities from the hotels table to show what cities exist
    public async Task<List<string>> GetAvailableCities()
    {
        string sql = "SELECT DISTINCT city FROM hotels ORDER BY city ASC;";
        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        
        List<string> cities = new List<string>();
        while (await reader.ReadAsync())
        {
            cities.Add(reader.GetString("city"));
        }
        return cities;
    }

    // method to search with filters and sorting
    public async Task<List<Room>> SearchRooms(
        string city,
        DateTime checkIn,
        DateTime checkOut,
        int guests,
        bool requirePool,
        bool requireRestaurant,
        int? maxBeach,
        int? maxCenter,
        bool sortByRating)
    {
        // if else to sort order 
        string orderByClause;
        if (sortByRating)
        {
            orderByClause = "h.rating DESC";
        }
        else
        {
            orderByClause = "r.price_per_night ASC";
        }
        // query for filtration
        string sql = $@"
            SELECT h.name AS hotel_name, h.city, r.id, r.hotels_id, r.room_number, 
                   r.capacity, r.price_per_night, h.rating
            FROM rooms r
            JOIN hotels h ON r.hotels_id = h.id
            WHERE h.city = @city
              AND r.capacity >= @guests
              AND (@requirePool = FALSE OR h.pool = TRUE)
              AND (@requireRestaurant = FALSE OR h.restaurant = TRUE)
              AND (@maxBeach IS NULL OR h.distance_beach <= @maxBeach)
              AND (@maxCenter IS NULL OR h.distance_center <= @maxCenter)
              AND r.id NOT IN (
                  SELECT room_id
                  FROM bookings
                  WHERE check_in < @checkOut
                  AND check_out > @checkIn
              )
            ORDER BY {orderByClause};";

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        command.Parameters.AddWithValue("@requirePool", requirePool);
        command.Parameters.AddWithValue("@requireRestaurant", requireRestaurant);
        command.Parameters.AddWithValue("@maxBeach", (object?)maxBeach ?? DBNull.Value);
        command.Parameters.AddWithValue("@maxCenter", (object?)maxCenter ?? DBNull.Value);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        List<Room> rooms = new List<Room>();

        while (await reader.ReadAsync())
        {
            rooms.Add(new Room(
                reader.GetInt32("id"),
                reader.GetInt32("hotels_id"),
                reader.GetString("room_number"),
                reader.GetInt32("capacity"),
                reader.GetDecimal("price_per_night"),
                reader.GetString("hotel_name"),
                reader.GetString("city"),
                reader.GetInt32("rating")
            ));
        }
        return rooms;
    }

    // method creates a booking 
    public async Task<int> CreateBooking(int customerId, int roomId, DateTime checkIn, DateTime checkOut, int guests, decimal totalPrice)
    {
        string sql = @"
            INSERT INTO bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price)
            VALUES (@customerId, @roomId, @checkIn, @checkOut, @guests, @totalPrice);";
        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@totalPrice", totalPrice);
        // insert query executes and return newly created
        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }
    // method to see a single customers bookings 
    public async Task<List<Booking>> GetCustomerBookings(int customerId)
    {
        // query for customers bookings
        string sql = @"
            SELECT b.id, b.customer_id, b.room_id, b.check_in, b.check_out, 
            b.amount_guests, b.total_price, h.name AS hotel_name, r.room_number
            FROM bookings b
            JOIN rooms r ON b.room_id = r.id
            JOIN hotels h ON r.hotels_id = h.id
            WHERE b.customer_id = @customerId
            ORDER BY b.check_in ASC;";
        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        // get results and store object in bookings list
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        List<Booking> bookings = new List<Booking>();

        while (await reader.ReadAsync())
        {
            bookings.Add(new Booking(
                reader.GetInt32("id"),
                reader.GetInt32("customer_id"),
                reader.GetInt32("room_id"),
                reader.GetDateTime("check_in"),
                reader.GetDateTime("check_out"),
                reader.GetInt32("amount_guests"),
                reader.GetDecimal("total_price"),
                reader.GetString("hotel_name"),
                reader.GetString("room_number")
            ));
        }
        // return list of booking
        return bookings;
    }

    // method uppdates the amount of guests 
    public async Task<bool> UpdateGuests(int bookingId, int newGuests)
    {
        // makes sure new guest count doent exceed room capacit
        string sql = @"
            UPDATE bookings
            SET amount_guests = @newGuests
            WHERE id = @bookingId
              AND @newGuests <= (SELECT capacity FROM rooms WHERE id = room_id);";

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@newGuests", newGuests);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        //return uppdated changes if above 0
        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
    // this method helps delete a booking
    public async Task<bool> DeleteBooking(int bookingId)
    {
        // finds specified booking id
        string sql = "DELETE FROM bookings WHERE id = @bookingId;";
        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        // executes the delete statement & return if id was found
        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
namespace HolidayMaker;
using MySqlConnector;

public class BookingRepository(MySqlConnection connection)
{
    // search for available rooms based on city and dates
    public async Task<List<Room>> SearchRooms(string city, DateTime checkIn, DateTime checkOut, int guests)
    {
        string sql = @"
            SELECT r.id, r.hotels_id, r.room_number, r.capacity, r.price_per_night,
            h.name AS hotel_name, h.city, h.rating
            FROM rooms r
            JOIN hotels h ON r.hotels_id = h.id
            WHERE h.city = @city
            AND r.capacity >= @guests
            AND r.id NOT IN (
                SELECT b.room_id 
                FROM bookings b
                WHERE b.check_in < @checkOut AND b.check_out > @checkIn
            );";

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        // executes query, creates list and holds room object 
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        List<Room> rooms = new List<Room>();

        // while-loop loops through room records and adds it to the list
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
            INSERT INTO bookings (customers_id, room_id, check_in, check_out, amount_guests, total_price)
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
}
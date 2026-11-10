namespace HolidayMaker;

// data for customers
public record Customer(
    int Id, 
    string Name, 
    string Email, 
    string PhoneNumber
);

// data for room and its hotel
public record Room(
    int Id, 
    int HotelsId, 
    string RoomNumber, 
    int Capacity, 
    decimal PricePerNight, 
    string HotelName, 
    string City, 
    int Rating
);

// data for bookings 
public record Booking(
    int Id, 
    int CustomerId, 
    int RoomId, 
    DateTime CheckIn, 
    DateTime CheckOut, 
    int AmountGuests, 
    decimal TotalPrice, 
    string HotelName, 
    string RoomNumber
);
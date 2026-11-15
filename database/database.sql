USE holiday_maker;

-- tabell 1 hotell
CREATE TABLE hotels (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    pool BOOLEAN NOT NULL DEFAULT FALSE,
    restaurant BOOLEAN NOT NULL DEFAULT FALSE,
    distance_beach INT NOT NULL CHECK (distance_beach >= 0),
    distance_center INT NOT NULL CHECK (distance_center >= 0),
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5)
);

-- tabell 2 rooms
CREATE TABLE rooms (
    id INT AUTO_INCREMENT PRIMARY KEY,
    hotels_id INT NOT NULL,
    room_number VARCHAR(20) NOT NULL,
    capacity INT NOT NULL CHECK (capacity > 0),
    price_per_night DECIMAL(10, 2) NOT NULL CHECK (price_per_night > 0),
    CONSTRAINT fk_rooms_hotels FOREIGN KEY (hotels_id) REFERENCES hotels(id) ON DELETE CASCADE,
    CONSTRAINT unique_room_per_hotels UNIQUE (hotels_id, room_number)
);

-- tabell 3 customer
CREATE TABLE customers (
     id INT AUTO_INCREMENT PRIMARY KEY,
      name VARCHAR(100) NOT NULL,
     email VARCHAR(100) NOT NULL UNIQUE,
     phone_number VARCHAR(50) NOT NULL
);

-- tabell 4 bookings
CREATE TABLE bookings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    room_id INT NOT NULL,
    check_in DATE NOT NULL,
    check_out DATE NOT NULL,
    amount_guests INT NOT NULL CHECK (amount_guests > 0),
    total_price DECIMAL(10, 2) NOT NULL CHECK (total_price > 0),
    CONSTRAINT fk_bookings_customers FOREIGN KEY (customer_id) REFERENCES customers(id) ON DELETE CASCADE,
    CONSTRAINT fk_bookings_rooms FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE,
    CONSTRAINT check_dates CHECK (check_out > check_in)
);

-- hotell data
INSERT INTO hotels (name, city, pool, restaurant, distance_beach, distance_center, rating) VALUES
    ('Landskrona Sleeps', 'Landskrona', FALSE, FALSE, 50, 400, 3),
    ('Chaplin Hotel', 'Landskrona', FALSE, TRUE, 600, 200, 4),
    ('Landskrona B&B', 'Landskrona', TRUE, TRUE, 2400, 200, 4),
    ('Zlatan City Hotel', 'Malmö', TRUE, TRUE, 3000, 400, 5),
    ('Bed no Breakfast Malmö', 'Malmö', TRUE, FALSE, 100, 1100, 2);

-- rum data
-- landskrona sleeps, id 1
INSERT INTO rooms(hotels_id, room_number, capacity, price_per_night) VALUES
     (1, 'Room a1', 2, 995.00),
     (1, 'Room a2', 2, 995.00),
     (1, 'Room b1', 3, 1100.00),
     (1, 'Room c1', 4, 2100.00),

-- chaplin hotel, id 2
      (2, 'Chaplins Door 1', 1, 700.00),
      (2, 'Caplins Door 2', 2, 889.00),
      (2, 'Chaplins Door 3', 4, 2200.00),
      (2, 'Chaplins Door 4', 4, 2200.00),

-- landskrona b&b, id 3
        (3, 'BB1', 2, 3000.00),
        (3, 'BB2', 3, 4100.00),
        (3, 'BB3', 3, 4100.00),
        (3, 'BB4', 4, 5000.00),

-- zlatan city hotel, id 4
     (4, 'z1', 2, 4000.00),
     (4, 'z2', 3, 5100.00),
     (4, 'z3', 3, 5100.00),
     (4, 'z4', 1, 2000.00),

-- bed no breakfast, id 5
    (5, 'Door1', 2, 600.00),
    (5, 'Door2', 3, 800.00),
    (5, 'Door3', 1, 500.00),
    (5, 'Door4', 1, 500.00);

-- kunders data
INSERT INTO customers(id, name, email, phone_number) VALUES
    (1, 'Leo Tolstoy', 'toltol@gmail.com', '090012345'),
    (2, 'Mikhail Bulgakov', 'bulgakov@gmail.com', '090023456'),
    (3, 'Rahat Khan', 'rkhan@gmail.com', '090034567'),
    (4, 'Atif Aslam', 'aa@gmail.com', '090056789'),
    (5, 'Gai Eaton', 'eaton@gmail.com', '091111111');

-- 25 bokningar
INSERT INTO bookings (customer_id, room_id, check_in, check_out, amount_guests, total_price) VALUES
-- tolstoy, id 1
(1, 1, '2026-12-06', '2026-12-10', 2, 3980.00),
(1, 2, '2026-12-11', '2026-12-15', 2, 3980.00),
(1, 3, '2026-12-16', '2026-12-20', 3, 4400.00),
(1, 9, '2026-12-21', '2026-12-24', 2, 9000.00),
(1, 10, '2026-12-25', '2026-12-28', 3, 12300.00),
(1, 16, '2026-12-29', '2026-12-31', 1, 4000.00),

-- eaton, id 5
(5, 1, '2026-12-01', '2026-12-05', 2, 3980.00),
(5, 9, '2026-12-10', '2026-12-15', 2, 15000.00),
(5, 17, '2026-12-24', '2026-12-28', 2, 2400.00),

-- bulgakov, id 2
(2, 2, '2026-12-01', '2026-12-04', 2, 2985.00),
(2, 3, '2026-12-05', '2026-12-08', 3, 3300.00),
(2, 10, '2026-12-12', '2026-12-16', 3, 16400.00),
(2, 13, '2026-12-20', '2026-12-23', 1, 12000.00),
(2, 18, '2026-12-27', '2026-12-31', 3, 3200.00),

-- khan, id 3
(3, 4, '2026-12-01', '2026-12-03', 1, 4200.00),
(3, 5, '2026-12-04', '2026-12-07', 1, 2100.00),
(3, 6, '2026-12-08', '2026-12-12', 2, 3556.00),
(3, 11, '2026-12-15', '2026-12-20', 3, 20500.00),
(3, 14, '2026-12-22', '2026-12-25', 2, 15300.00),
(3, 19, '2026-12-29', '2026-12-31', 1, 1000.00),

-- aslam, id 4
(4, 7, '2026-12-02', '2026-12-06', 2, 8800.00),
(4, 8, '2026-12-07', '2026-12-10', 3, 6600.00),
(4, 12, '2026-12-11', '2026-12-14', 2, 15000.00),
(4, 15, '2026-12-18', '2026-12-21', 2, 15300.00),
(4, 20, '2026-12-23', '2026-12-26', 1, 1500.00);

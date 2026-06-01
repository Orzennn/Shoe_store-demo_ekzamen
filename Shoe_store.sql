-- ===========================
-- СОЗДАНИЕ БАЗЫ ДАННЫХ
-- ===========================
DROP DATABASE IF EXISTS shoe_store;
CREATE DATABASE shoe_store;

\c shoe_store;

-- ===========================
-- СОЗДАНИЕ ТАБЛИЦ
-- ===========================

-- Роли пользователей
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL UNIQUE
);

-- Пользователи
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    full_name VARCHAR(200) NOT NULL,
    login VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    role_id INTEGER REFERENCES roles(role_id)
);

-- Категории товаров
CREATE TABLE categories (
    category_id SERIAL PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL UNIQUE
);

-- Производители
CREATE TABLE manufacturers (
    manufacturer_id SERIAL PRIMARY KEY,
    manufacturer_name VARCHAR(100) NOT NULL UNIQUE
);

-- Поставщики
CREATE TABLE suppliers (
    supplier_id SERIAL PRIMARY KEY,
    supplier_name VARCHAR(100) NOT NULL UNIQUE
);

-- Единицы измерения
CREATE TABLE units (
    unit_id SERIAL PRIMARY KEY,
    unit_name VARCHAR(50) NOT NULL UNIQUE
);

-- Товары
CREATE TABLE products (
    product_id SERIAL PRIMARY KEY,
    article VARCHAR(50) UNIQUE NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    unit_id INTEGER REFERENCES units(unit_id),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    supplier_id INTEGER REFERENCES suppliers(supplier_id),
    manufacturer_id INTEGER REFERENCES manufacturers(manufacturer_id),
    category_id INTEGER REFERENCES categories(category_id),
    discount INTEGER DEFAULT 0 CHECK (discount >= 0 AND discount <= 100),
    quantity_in_stock INTEGER DEFAULT 0 CHECK (quantity_in_stock >= 0),
    description TEXT,
    image_path VARCHAR(255)
);

-- Пункты выдачи
CREATE TABLE pickup_points (
    point_id SERIAL PRIMARY KEY,
    postal_code VARCHAR(10),
    city VARCHAR(100),
    street VARCHAR(100),
    house_number VARCHAR(10),
    UNIQUE(postal_code, city, street, house_number)
);

-- Статусы заказов
CREATE TABLE order_statuses (
    status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL UNIQUE
);

-- Заказы
CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    order_number INTEGER UNIQUE NOT NULL,
    order_code VARCHAR(10) UNIQUE,
    order_date DATE NOT NULL,
    delivery_date DATE,
    pickup_point_id INTEGER REFERENCES pickup_points(point_id),
    user_id INTEGER REFERENCES users(user_id),
    status_id INTEGER REFERENCES order_statuses(status_id)
);

-- Состав заказа
CREATE TABLE order_items (
    item_id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES orders(order_id) ON DELETE CASCADE,
    product_article VARCHAR(50) REFERENCES products(article),
    quantity INTEGER NOT NULL CHECK (quantity > 0)
);

-- ===========================
-- ЗАПОЛНЕНИЕ СПРАВОЧНИКОВ
-- ===========================

INSERT INTO roles (role_name) VALUES 
    ('Администратор'),
    ('Менеджер'),
    ('Авторизированный клиент');

INSERT INTO order_statuses (status_name) VALUES 
    ('Новый'),
    ('Завершен');

INSERT INTO units (unit_name) VALUES ('шт.');

INSERT INTO categories (category_name) VALUES 
    ('Женская обувь'),
    ('Мужская обувь');

INSERT INTO manufacturers (manufacturer_name) VALUES 
    ('Kari'),
    ('Marco Tozzi'),
    ('Рос'),
    ('Rieker'),
    ('Alessio Nesca'),
    ('CROSBY');

INSERT INTO suppliers (supplier_name) VALUES 
    ('Kari'),
    ('Обувь для вас');

-- ===========================
-- ИМПОРТ ПОЛЬЗОВАТЕЛЕЙ
-- ===========================

INSERT INTO users (full_name, login, password, role_id) VALUES
    ('Никифорова Весения Николаевна', '94d5ous@gmail.com', 'uzWC67', 1),
    ('Сазонов Руслан Германович', 'uth4iz@mail.com', '2L6KZG', 1),
    ('Одинцов Серафим Артёмович', 'yzls62@outlook.com', 'JlFRCZ', 1),
    ('Степанов Михаил Артёмович', '1diph5e@tutanota.com', '8ntwUp', 2),
    ('Ворсин Петр Евгеньевич', 'tjde7c@yahoo.com', 'YOyhfR', 2),
    ('Старикова Елена Павловна', 'wpmrc3do@tutanota.com', 'RSbvHv', 2),
    ('Михайлюк Анна Вячеславовна', '5d4zbu@tutanota.com', 'rwVDh9', 3),
    ('Ситдикова Елена Анатольевна', 'ptec8ym@yahoo.com', 'LdNyos', 3),
    ('Ворсин Петр Евгеньевич', '1qz4kw@mail.com', 'gynQMT', 3),
    ('Старикова Елена Павловна', '4np6se@mail.com', 'AtnDjr', 3);

-- ===========================
-- ИМПОРТ ТОВАРОВ
-- ===========================

INSERT INTO products (article, product_name, unit_id, price, supplier_id, manufacturer_id, category_id, discount, quantity_in_stock, description, image_path) VALUES
    ('А112Т4', 'Ботинки', 1, 4990, 1, 1, 1, 3, 6, 'Женские Ботинки демисезонные kari', '1.jpg'),
    ('F635R4', 'Ботинки', 1, 3244, 2, 2, 1, 2, 13, 'Ботинки Marco Tozzi женские демисезонные, размер 39, цвет бежевый', '2.jpg'),
    ('H782T5', 'Туфли', 1, 4499, 1, 1, 2, 4, 5, 'Туфли kari мужские классика MYZ21AW-450A, размер 43, цвет: черный', '3.jpg'),
    ('G783F5', 'Ботинки', 1, 5900, 1, 3, 2, 2, 8, 'Мужские ботинки Рос-Обувь кожаные с натуральным мехом', '4.jpg'),
    ('J384T6', 'Ботинки', 1, 3800, 2, 4, 2, 2, 16, 'B3430/14 Полуботинки мужские Rieker', '5.jpg'),
    ('D572U8', 'Кроссовки', 1, 4100, 2, 3, 2, 3, 6, '129615-4 Кроссовки мужские', '6.jpg'),
    ('F572H7', 'Туфли', 1, 2700, 1, 2, 1, 2, 14, 'Туфли Marco Tozzi женские летние, размер 39, цвет черный', '7.jpg'),
    ('D329H3', 'Полуботинки', 1, 1890, 2, 5, 1, 4, 4, 'Полуботинки Alessio Nesca женские 3-30797-47, размер 37, цвет: бордовый', '8.jpg'),
    ('B320R5', 'Туфли', 1, 4300, 1, 4, 1, 2, 6, 'Туфли Rieker женские демисезонные, размер 41, цвет коричневый', '9.jpg'),
    ('G432E4', 'Туфли', 1, 2800, 1, 1, 1, 3, 15, 'Туфли kari женские TR-YR-413017, размер 37, цвет: черный', '10.jpg'),
    ('S213E3', 'Полуботинки', 1, 2156, 2, 6, 2, 3, 6, '407700/01-01 Полуботинки мужские CROSBY', NULL),
    ('E482R4', 'Полуботинки', 1, 1800, 1, 1, 1, 2, 14, 'Полуботинки kari женские MYZ20S-149, размер 41, цвет: черный', NULL),
    ('S634B5', 'Кеды', 1, 5500, 2, 6, 2, 3, 0, 'Кеды Caprice мужские демисезонные, размер 42, цвет черный', NULL),
    ('K345R4', 'Полуботинки', 1, 2100, 2, 6, 2, 2, 3, '407700/01-02 Полуботинки мужские CROSBY', NULL),
    ('O754F4', 'Туфли', 1, 5400, 2, 4, 1, 4, 18, 'Туфли женские демисезонные Rieker артикул 55073-68/37', NULL),
    ('G531F4', 'Ботинки', 1, 6600, 1, 1, 1, 12, 9, 'Ботинки женские зимние ROMER арт. 893167-01 Черный', NULL),
    ('J542F5', 'Тапочки', 1, 500, 1, 1, 2, 13, 0, 'Тапочки мужские Арт.70701-55-67син р.41', NULL),
    ('B431R5', 'Ботинки', 1, 2700, 2, 4, 2, 2, 5, 'Мужские кожаные ботинки/мужские ботинки', NULL),
    ('P764G4', 'Туфли', 1, 6800, 1, 6, 1, 15, 15, 'Туфли женские, ARGO, размер 38', NULL),
    ('C436G5', 'Ботинки', 1, 10200, 1, 5, 1, 15, 9, 'Ботинки женские, ARGO, размер 40', NULL),
    ('F427R5', 'Ботинки', 1, 11800, 2, 4, 1, 15, 11, 'Ботинки на молнии с декоративной пряжкой FRAU', NULL),
    ('N457T5', 'Полуботинки', 1, 4600, 1, 6, 1, 3, 13, 'Полуботинки Ботинки черные зимние, мех', NULL),
    ('D364R4', 'Туфли', 1, 12400, 1, 1, 1, 16, 5, 'Туфли Luiza Belly женские Kate-lazo черные из натуральной замши', NULL),
    ('S326R5', 'Тапочки', 1, 9900, 2, 6, 2, 17, 15, 'Мужские кожаные тапочки "Профиль С.Дали"', NULL),
    ('L754R4', 'Полуботинки', 1, 1700, 1, 1, 1, 2, 7, 'Полуботинки kari женские WB2020SS-26, размер 38, цвет: черный', NULL),
    ('M542T5', 'Кроссовки', 1, 2800, 2, 4, 2, 18, 3, 'Кроссовки мужские TOFA', NULL),
    ('D268G5', 'Туфли', 1, 4399, 2, 4, 1, 3, 12, 'Туфли Rieker женские демисезонные, размер 36, цвет коричневый', NULL),
    ('T324F5', 'Сапоги', 1, 4699, 1, 6, 1, 2, 5, 'Сапоги замша Цвет: синий', NULL),
    ('K358H6', 'Тапочки', 1, 599, 1, 4, 2, 20, 2, 'Тапочки мужские син р.41', NULL),
    ('H535R5', 'Ботинки', 1, 2300, 2, 4, 1, 2, 7, 'Женские Ботинки демисезонные', NULL);

-- ===========================
-- ИМПОРТ ПУНКТОВ ВЫДАЧИ
-- ===========================

INSERT INTO pickup_points (postal_code, city, street, house_number) VALUES
    ('420151', 'г.Лесной', 'Вишневая', '32'),
    ('125061', 'г.Лесной', 'Подгорная', '8'),
    ('630370', 'г.Лесной', 'Шоссейная', '24'),
    ('400562', 'г.Лесной', 'Зеленая', '32'),
    ('614510', 'г.Лесной', 'Маяковского', '47'),
    ('410542', 'г.Лесной', 'Светлая', '46'),
    ('620839', 'г.Лесной', 'Цветочная', '8'),
    ('443890', 'г.Лесной', 'Коммунистическая', '1'),
    ('603379', 'г.Лесной', 'Спортивная', '46'),
    ('603721', 'г.Лесной', 'Гоголя', '41'),
    ('410172', 'г.Лесной', 'Северная', '13'),
    ('614611', 'г.Лесной', 'Молодежная', '50'),
    ('454311', 'г.Лесной', 'Новая', '19'),
    ('660007', 'г.Лесной', 'Октябрьская', '19'),
    ('603036', 'г.Лесной', 'Садовая', '4'),
    ('394060', 'г.Лесной', 'Фрунзе', '43'),
    ('410661', 'г.Лесной', 'Школьная', '50'),
    ('625590', 'г.Лесной', 'Коммунистическая', '20'),
    ('625683', 'г.Лесной', '8 Марта', '0'),
    ('450983', 'г.Лесной', 'Комсомольская', '26');

-- ===========================
-- ИМПОРТ ЗАКАЗОВ
-- ===========================

-- Заказ 1
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (1, '901', '2025-02-27', '2025-04-20', 1, 4, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (1, 'А112Т4', 2),
    (1, 'F635R4', 2);

-- Заказ 2
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (2, '902', '2022-09-28', '2025-04-21', 11, 1, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (2, 'H782T5', 1),
    (2, 'G783F5', 1);

-- Заказ 3
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (3, '903', '2025-03-21', '2025-04-22', 2, 2, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (3, 'J384T6', 10),
    (3, 'D572U8', 10);

-- Заказ 4
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (4, '904', '2025-02-20', '2025-04-23', 11, 3, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (4, 'F572H7', 5),
    (4, 'D329H3', 4);

-- Заказ 5
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (5, '905', '2025-03-17', '2025-04-24', 2, 4, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (5, 'А112Т4', 2),
    (5, 'F635R4', 2);

-- Заказ 6
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (6, '906', '2025-03-01', '2025-04-25', 15, 1, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (6, 'H782T5', 1),
    (6, 'G783F5', 1);

-- Заказ 7
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (7, '907', '2025-02-28', '2025-04-26', 3, 2, 2);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (7, 'J384T6', 10),
    (7, 'D572U8', 10);

-- Заказ 8
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (8, '908', '2025-03-31', '2025-04-27', 19, 3, 1);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (8, 'F572H7', 5),
    (8, 'D329H3', 4);

-- Заказ 9
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (9, '909', '2025-04-02', '2025-04-28', 5, 4, 1);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (9, 'B320R5', 5),
    (9, 'G432E4', 1);

-- Заказ 10
INSERT INTO orders (order_number, order_code, order_date, delivery_date, pickup_point_id, user_id, status_id)
VALUES (10, '910', '2025-04-03', '2025-04-29', 19, 4, 1);

INSERT INTO order_items (order_id, product_article, quantity) VALUES
    (10, 'S213E3', 5),
    (10, 'E482R4', 5);
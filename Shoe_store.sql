-- Создание базы данных
CREATE DATABASE shoe_store;

-- Таблица ролей пользователей
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL
);

-- Таблица пользователей
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    surname VARCHAR(100) NOT NULL,
    name VARCHAR(100) NOT NULL,
    patronymic VARCHAR(100),
    login VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role_id INTEGER REFERENCES roles(role_id)
);

-- Таблица категорий
CREATE TABLE categories (
    category_id SERIAL PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL
);

-- Таблица производителей
CREATE TABLE manufacturers (
    manufacturer_id SERIAL PRIMARY KEY,
    manufacturer_name VARCHAR(100) NOT NULL
);

-- Таблица поставщиков
CREATE TABLE suppliers (
    supplier_id SERIAL PRIMARY KEY,
    supplier_name VARCHAR(100) NOT NULL
);

-- Таблица единиц измерения
CREATE TABLE units (
    unit_id SERIAL PRIMARY KEY,
    unit_name VARCHAR(50) NOT NULL
);

-- Таблица товаров
CREATE TABLE products (
    product_id SERIAL PRIMARY KEY,
    product_name VARCHAR(200) NOT NULL,
    description TEXT,
    category_id INTEGER REFERENCES categories(category_id),
    manufacturer_id INTEGER REFERENCES manufacturers(manufacturer_id),
    supplier_id INTEGER REFERENCES suppliers(supplier_id),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    discount INTEGER DEFAULT 0 CHECK (discount >= 0 AND discount <= 100),
    quantity_in_stock INTEGER DEFAULT 0 CHECK (quantity_in_stock >= 0),
    unit_id INTEGER REFERENCES units(unit_id),
    image_path VARCHAR(255)
);

-- Таблица пунктов выдачи
CREATE TABLE pickup_points (
    point_id SERIAL PRIMARY KEY,
    address VARCHAR(255) NOT NULL
);

-- Таблица статусов заказа
CREATE TABLE order_statuses (
    status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

-- Таблица заказов
CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    order_date DATE NOT NULL DEFAULT CURRENT_DATE,
    delivery_date DATE,
    pickup_point_id INTEGER REFERENCES pickup_points(point_id),
    user_id INTEGER REFERENCES users(user_id),
    status_id INTEGER REFERENCES order_statuses(status_id),
    order_code VARCHAR(50) UNIQUE
);

-- Таблица состава заказа
CREATE TABLE order_items (
    order_item_id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES orders(order_id) ON DELETE CASCADE,
    product_id INTEGER REFERENCES products(product_id),
    quantity INTEGER NOT NULL CHECK (quantity > 0)
);

-- Заполнение справочных данных
INSERT INTO roles (role_name) VALUES 
('Администратор'), ('Менеджер'), ('Клиент');

INSERT INTO order_statuses (status_name) VALUES 
('Новый'), ('В обработке'), ('Доставлен'), ('Отменён');

INSERT INTO units (unit_name) VALUES 
('шт'), ('пара');
# estoreapp
A robust Electronics Store backend API developed using .NET and PostgreSQL, implementing JWT authentication with 2FA (OTP), role-based authorization, product catalog management, cart and order processing, and clean service-based architecture with DTO patterns.
# 🛒 Electronics Store Backend API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![2FA](https://img.shields.io/badge/Security-2FA-FF6F00)
![Swagger](https://img.shields.io/badge/API-Swagger-85EA2D?logo=swagger&logoColor=black)
![Status](https://img.shields.io/badge/Status-Active-2ECC71)
![License](https://img.shields.io/badge/License-MIT-F1C40F)
A scalable and production-ready backend for an **Electronics Store** built using **.NET**, following clean architecture principles. This project implements secure authentication, role-based access control, 2FA via OTP, and full e-commerce workflows including product management, cart handling, and order processing.

---

## 🚀 Features

### 🔐 Authentication & Security
- JWT-based authentication
- Role-Based Access Control (RBAC)
- Two-Factor Authentication (2FA) using OTP
- Secure password hashing
- Token validation & extendable refresh strategy

---

### 👤 User Management
- User registration & login
- Role assignment (Admin / Customer)
- OTP verification flow
- Secure profile handling

---

### 📦 Product Management
- Create, update, delete products
- Stock management
- Admin-controlled operations

---

### 🛒 Cart System
- Add items to cart
- Update quantities
- Remove items
- View cart summary

---

### 📑 Order Management
- Create orders from cart
- Order tracking
- User order history
- Structured response DTOs

---

## 🧱 Architecture

- Clean Architecture (Controller → Service → Repository)
- DTO-driven design
- Separation of concerns
- Scalable modular services

---

## 🛠️ Tech Stack

- ASP.NET Core Web API
- C#
- PostgreSQL
- Entity Framework Core
- JWT Authentication + OTP (2FA)

---

## 📂 Project Structure

ElectronicsStoreBackend/
│
├── Controllers/
├── Services/
├── Interfaces/
├── DTOs/
├── Models/
├── Data/
├── Migrations/
├── Helpers/
└── Program.cs

---

## 🔑 Authentication Flow

1. User logs in
2. OTP is generated
3. User verifies OTP
4. JWT token is issued

---

## 📌 API Endpoints

### Auth
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/verify-otp

### Products
- GET /api/products
- POST /api/products (Admin)
- PUT /api/products/{id} (Admin)
- DELETE /api/products/{id} (Admin)

### Cart
- POST /api/cart/add
- GET /api/cart
- DELETE /api/cart/remove/{id}

### Orders
- POST /api/orders
- GET /api/orders
- GET /api/orders/{id}

---

## ⚙️ Setup Instructions

### Clone Repository
git clone https://github.com/sunilsoft9x/estoreapp.git
cd electronics-store-backend

### Configure Database
Update appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=ElectronicsStore;Username=postgres;Password=yourpassword"
}

### Run Migrations
dotnet ef database update

### Run Application
dotnet run

---

## 🧪 Testing

- Swagger UI
- Postman
- Thunder Client

---

## 🔄 DTOs Used

- LoginDto
- VerifyOtpDto
- CreateOrderDto
- OrderResponseDto
- CartResponseDto

---

## 🔐 Security Practices

- Password hashing
- JWT authentication
- Role-based authorization
- OTP verification
- DTO validation

---

## 📈 Future Improvements

- Refresh tokens
- Email/SMS OTP
- Payment gateway
- Redis caching
- Rate limiting
- Microservices architecture

---

## 🤝 Contributing

1. Fork the repo
2. Create branch
3. Commit changes
4. Open PR

---

## 📜 License

MIT License

---

## 👨‍💻 Author

Sunil Dhawan

---

## ⭐ Support

Give a star if you find this useful!


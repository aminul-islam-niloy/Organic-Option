## Organic Option

**Organic Option** is an ASP.NET Core 6 web application designed to facilitate direct farming, allowing customers to buy fresh products directly from local farmers without the need for a middleman. This platform connects Farmers, Riders, Customers, and Admins in a seamless environment, ensuring the delivery of fresh produce while supporting local agriculture.

## Features

- **Customer to FarmerShop**: Customers can directly purchase products from local FarmerShops, eliminating the middleman.
- **Category-Based Products**: Shops are categorized by the types of products they offer, making it easy for customers to find what they need.
- **Location-Based Shopping**: Shops are associated with latitude and longitude, allowing customers to find nearby FarmerShops.
- **Farmer Blog**: Farmers can share blogs related to farming tips, news, and more.
- **Daily Price List**: A daily price list is available to inform customers of current product prices.
- **Payment System**: Integrated with Stripe for secure and convenient payments.
- **Notification System**: Real-time notifications for customers, riders, and farmers.
- **Email Verification**: Secure registration process with email verification.
- **Google Authentication**: Quick and easy sign-in with Google.
- **Admin Panel**: Comprehensive admin panel to manage the platform, including shops, users, blogs, and more.

### Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (recommended IDE)

### Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/aminul-islam-niloy/Organic-Option.git
    cd Organic-Option
    ```

2. **Set up the database:**

   - Update the connection string in `appsettings.json` to point to your SQL Server instance.
   
 
    ```
        "ConnectionStrings": {
        "DefaultConnection": "Server=AMINUL\\SQLEXPRESS;Database=TheOrganicOption;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
    ```

   - 
   - Run the following command to apply migrations and set up the database:

     ```bash
     dotnet ef database update
     ```

3. **Run the application:**

    ```bash
    dotnet run
    ```

4. **Access the application:**

   Open your browser and go to `https://localhost:5001` (or the port specified in your `launchSettings.json`).

## Project Structure

- **Models**: Contains the data models for the application, such as `Farmer`, `Customer`, `Rider`, `Admin`, `Product`, `Order`, etc.
- **Area**: Handles incoming HTTP requests and returns responses, managing the application’s logic for different user roles.
- **Views**: Contains Razor views for rendering the UI.
- **Services**: Custom services for handling business logic, such as payment processing, notification sending, etc.
- **Data**: Contains the DbContext and migration files for database management.

## Key Features Implementation

### 1. **Location-Based FarmerShop Search**

   Customers can search for nearby FarmerShops based on their current location (latitude and longitude). This is achieved using geolocation APIs and spatial queries in SQL.

### 2. **Payment System**

   Integrated with Stripe, allowing customers to make secure payments for their purchases directly through the platform.

### 3. **Notification System**

   Real-time notifications are sent to customers, farmers, and riders using SignalR, ensuring everyone is informed about their activities.

### 4. **Blog and Daily Price List**

   Farmers can create blog posts to share knowledge, while the daily price list keeps customers informed of current prices for fresh products.

### 5. **Authentication and Authorization**

   - Email verification is implemented for secure user registration.
   - Google authentication is available for easy login.
   - Role-based access control ensures that only authorized users can access specific features.

## Contributing

Contributions are welcome! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes.
4. Submit a pull request with a description of your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any questions or support, please reach out to [Aminul Islam Niloy](https://github.com/aminul-islam-niloy).

---

Thank you for using **Organic Option**! Supporting local farmers and delivering fresh produce directly to your doorstep.

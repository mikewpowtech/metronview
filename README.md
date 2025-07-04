# MetronView - Telemetry Management System

A comprehensive telemetry data management and monitoring system built with ASP.NET Core 9 and React. MetronView provides real-time monitoring, alarm management, and data visualization for industrial sensor networks and telemetry devices.

## Features

- **Real-time Telemetry Monitoring**: Live data collection and monitoring from Metron devices
- **Interactive Data Visualization**: Chart-based visualization of sensor readings over time
- **Alarm Management**: Configurable alarms and triggers with notification systems
- **User Management**: JWT-based authentication with role-based access control
- **Device Configuration**: Management of units, sensors, and telemetry configurations
- **Company & User Administration**: Multi-tenant support for different organizations
- **Responsive UI**: Modern React interface with Ant Design components

## Architecture

### Backend (ASP.NET Core 9)
- **Presentation.Api**: REST API controllers and endpoints
- **Application**: Business logic and services layer
- **Domain**: Core entities and domain models
- **Infrastructure**: Data access and external integrations
- **TelemetryService**: Specialized telemetry data processing services

### Frontend (React + TypeScript)
- **Modern React 19**: Component-based UI with TypeScript
- **Ant Design**: Professional UI component library
- **Redux Toolkit**: State management for application data
- **Recharts**: Interactive data visualization and charting
- **React Router**: Client-side routing and navigation

## API Endpoints

### Authentication
- **POST /api/auth/register**: Create new user accounts
- **POST /api/auth/login**: Authenticate and receive JWT token
- **POST /api/auth/refresh**: Renew access tokens

### Telemetry Management
- **GET /api/readings**: Retrieve sensor readings with filtering
- **POST /api/readings**: Add new sensor readings
- **GET /api/sensors**: Manage sensor configurations
- **GET /api/units**: Device and unit management

### Alarm System
- **GET /api/alarms**: Active and historical alarms
- **POST /api/triggers**: Configure alarm triggers
- **GET /api/recipients**: Notification recipient management

### Administration
- **GET /api/users**: User management (admin only)
- **GET /api/companies**: Company and organization management


## Key Features

### Dashboard
- Real-time telemetry data overview
- Device status monitoring
- Quick access to recent alarms and readings

### Telemetry Visualization
- **Table View**: Detailed readings list with sorting and filtering
- **Graph View**: Interactive line charts showing sensor data over time
- **Multi-sensor Support**: Visualize multiple sensors on the same chart
- **Time-based Analysis**: Historical data trending and analysis

### Alarm Management
- **Smart Triggers**: Configurable alarm conditions and thresholds
- **Notification System**: Email and SMS alert capabilities
- **Status Tracking**: Active/inactive alarm management
- **Recipient Management**: Organize notification groups and individuals

### Device Administration
- **Unit Management**: Configure and monitor telemetry devices
- **Sensor Configuration**: Set up individual sensor parameters
- **Configuration Uploads**: Bulk device configuration management
- **Company Organization**: Multi-tenant device grouping

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 22.15.0+
- SQL Server or PostgreSQL database

### Running the Application

1. **Start the API Server**:
   ```bash
   cd src/Presentation.Api
   dotnet run
   ```

2. **Start the Frontend**:
   ```bash
   cd src/Presentation.Web
   npm install
   npm run dev
   ```

3. **Access the Application**:
   - Frontend: `http://localhost:3000`
   - API: `http://localhost:5202`
   - Swagger UI: `http://localhost:5202/swagger`
## Development

### Database Migrations

To add new migrations:
```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Presentation.Api
```

To update the database:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation.Api
```

### Building for Production

**Backend**:
```bash
dotnet build --configuration Release
```

**Frontend**:
```bash
cd src/Presentation.Web
npm run build
```

### Testing

Run backend tests:
```bash
dotnet test
```

Run frontend tests:
```bash
cd src/Presentation.Web
npm test
```

## Technology Stack

- **Backend**: ASP.NET Core 9, Entity Framework Core, JWT Authentication
- **Frontend**: React 19, TypeScript, Ant Design, Redux Toolkit, Recharts
- **Database**: SQL Server / PostgreSQL
- **DevOps**: Docker support, GitHub Actions CI/CD ready

## Project Structure

```
├── src/
│   ├── Application/          # Business logic layer
│   ├── Domain/              # Core domain models
│   ├── Infrastructure/      # Data access and external services
│   ├── Presentation.Api/    # REST API controllers
│   ├── Presentation.Web/    # React frontend application
│   └── TelemetryService/    # Telemetry processing services
├── test/                    # Unit and integration tests
└── App.sln                 # Solution file
```
## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

If you encounter any issues or have questions, please [raise a new issue](https://github.com/your-repo/metronview/issues).

## License

This project is licensed under the [MIT License](LICENSE).

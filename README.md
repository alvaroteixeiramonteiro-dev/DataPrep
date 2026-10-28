# DataPrep

**CSV Data Validation and Import Preparation**

DataPrep is a C#/.NET application under active development, designed to help users inspect, validate, and prepare CSV datasets before importing them into relational databases.

The project aims to make data preparation more reliable through structured diagnostics, reusable validation rules, and clear guidance for identifying and correcting data quality issues.

> **Project status:** Early development. CSV reading and preview are implemented, while the complete validation and database import workflows are still in progress.

## Current Features

- CSV parsing using CsvHelper.
- Support for semicolon, comma, and tab delimiters.
- Reading CSV files with or without headers.
- UTF-8 input handling.
- Detection and reporting of malformed CSV syntax.
- Preservation of original field values and record locations.
- Web-based CSV upload with a 5 MiB file size limit.
- Preview of up to 20 data records.
- Reusable required-field validation logic.
- Structured, language-independent validation diagnostics.
- Initial Portuguese (Portugal) and English localization support.
- Automated unit tests covering CSV parsing and required-field validation.

The existing validation components are not yet fully integrated into the web-based CSV workflow.

## Technology Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Application development |
| ASP.NET Core Razor Pages | Web interface |
| CsvHelper | CSV parsing |
| xUnit | Unit testing |
| Git / GitHub | Version control and source hosting |

## Architecture

The solution is organized into four projects:

```text
DataPrep/
├── DataPrep.Core/
├── DataPrep.Infrastructure/
├── DataPrep.Tests/
├── DataPrep.Web/
└── README.md
```

- **DataPrep.Core:** Validation rules, diagnostic models, and core application concepts.
- **DataPrep.Infrastructure:** CSV reading and supporting infrastructure.
- **DataPrep.Tests:** Automated tests for core and infrastructure functionality.
- **DataPrep.Web:** ASP.NET Core Razor Pages interface and localization resources.

The architecture separates validation logic from file handling and presentation, supporting maintainability and future extensibility.

## Getting Started

### Requirements

- .NET 10 SDK

### Clone the repository

```bash
git clone https://github.com/alvaroteixeiramonteiro-dev/DataPrep.git
cd DataPrep
```

### Restore dependencies

```bash
dotnet restore
```

### Run the web application

```bash
dotnet run --project DataPrep.Web
```

Open the local address displayed in the terminal and navigate to `/Csv`.

### Run automated tests

```bash
dotnet test
```

These commands are provided for local development. Automated tests and application execution should be verified in the target environment.

## Current Limitations

- CSV uploads are limited to 5 MiB.
- The application currently expects UTF-8 input.
- CSV records are loaded into memory during processing.
- The web interface displays only the first 20 data records.
- Successful CSV parsing does not guarantee that the dataset passes field-level or structural validation.
- Database import and the complete data preparation workflow are not yet implemented.
- The web interface is still at an early stage of development.

## Development Roadmap

The following capabilities are planned and are not yet presented as completed functionality:

- Integrate validation rules into the web processing workflow.
- Expand field-level and dataset-level validation.
- Implement source-to-target column mapping.
- Provide detailed validation reports and correction guidance.
- Introduce data preparation and transformation operations.
- Implement controlled import into relational databases.
- Expand automated tests and improve error handling.
- Improve performance, accessibility, and multilingual support.
- Refine the user interface and application documentation.

## Engineering Goals

DataPrep is also a software development portfolio project focused on:

- Modular application architecture.
- Separation of concerns.
- Reusable and testable components.
- Structured error reporting.
- Internationalization and localization.
- Reliable data processing.
- Incremental, maintainable software development.

## License

No software license has been specified. Public availability of the source code does not imply permission to reuse, modify, or redistribute it.

## Author

Developed by [alvaroteixeiramonteiro-dev](https://github.com/alvaroteixeiramonteiro-dev).

# CSV Mapper Application

## Overview

CSV Mapper is a powerful WPF desktop application that allows users to map CSV columns to database schema tables with advanced validation and persistence. Built with .NET 8, this application uses the MVVM architecture pattern and dependency injection to provide a robust, maintainable solution for data mapping challenges.

## Key Features

- **Multiple CSV Support**: Map both Patient/Study CSV files and Series/Instance CSV files to their corresponding database tables
- **Auto-Mapping**: Intelligent algorithm to suggest column mappings based on name similarity
- **Data Type Validation**: Ensures that CSV data types are compatible with database column types
- **Interactive Visual Mapping**: User-friendly interface with real-time validation feedback
- **Sample Value Preview**: See actual data samples for each CSV column to verify mappings
- **Mapping Persistence**: Save and load mapping configurations for future use
- **Multi-Mapping Sessions**: Create and manage multiple mapping configurations in a single session

## System Requirements

- Windows 10/11 (64-bit)
- .NET 8 Runtime
- 4GB RAM (minimum)
- 100MB disk space

## Installation

1. Download the latest release from the [Releases](https://github.com/dasabhishk/csv_mapping_demo/releases) page
2. Run the exe 

Alternatively, you can build from source:

```bash
git clone https://github.com/dasabhishk/csv_mapping_demo.git
cd csv-mapper
dotnet build
dotnet run --project CsvMapper
```

## Quick Start Guide

1. **Launch the application**
2. **Load the schema file**: By default, the application looks for `stagingdb_details.json` in the application directory. You can browse for a different schema file if needed.
3. **Select CSV Type**: Choose between PatientStudy or SeriesInstance mapping
4. **Load a CSV file**: Browse for your CSV file
5. **Review Auto-Mappings**: The application will attempt to auto-match columns
6. **Adjust Mappings**: Use the dropdown for each database column to select the appropriate CSV column
7. **Validate Mappings**: Check validation errors and fix any issues
8. **Save Mappings**: Save your mappings to a JSON file for future use
9. **Repeat for other CSV type if needed**: Switch to the other CSV type and map it as well

## Sample Files

### Patient Study CSV (`sample_patient_study.csv`)

This CSV contains patient demographic information and associated study data:

```csv
pat_id,patient_name,age,gender,study_id,study_date,modality
1001,John Doe,45,M,S1001,2024-03-01,CT
1002,Jane Smith,37,F,S1002,2024-03-02,MR
1003,Ravi Kumar,50,M,S1003,2024-03-03,CT
1004,Emily Zhang,28,F,S1004,2024-03-04,US
1005,Aarav Patel,60,M,S1005,2024-03-05,XR
```

### Series Instance CSV (`sample_series_instance.csv`)

This CSV contains detailed information about imaging series and instances:

```csv
series_id,study_id,instance_id,series_date,series_description,instance_number,slice_location
SER1001,S1001,I1001,2024-03-01,Brain CT,1,10.5
SER1002,S1001,I1002,2024-03-01,Brain CT,2,15.2
SER1003,S1002,I1003,2024-03-02,Knee MRI,1,5.0
SER1004,S1002,I1004,2024-03-02,Knee MRI,2,10.0
SER1005,S1003,I1005,2024-03-03,Chest CT,1,20.3
```

### Database Schema (`stagingdb_details.json`)

The database schema defines the structure of the target database tables:

```json
{
  "DatabaseName": "StagingDb",
  "Tables": [
    {
      "TableName": "PatientStudy",
      "CsvType": "PatientStudy",
      "Columns": [
        { "Name": "PatientId", "DataType": "int" },
        { "Name": "PatientName", "DataType": "string" },
        { "Name": "Age", "DataType": "int" },
        { "Name": "Gender", "DataType": "string" },
        { "Name": "StudyId", "DataType": "string" },
        { "Name": "StudyDate", "DataType": "datetime" },
        { "Name": "Modality", "DataType": "string" }
      ]
    },
    {
      "TableName": "SeriesInstance",
      "CsvType": "SeriesInstance",
      "Columns": [
        { "Name": "SeriesId", "DataType": "string" },
        { "Name": "StudyId", "DataType": "string" },
        { "Name": "InstanceId", "DataType": "string" },
        { "Name": "SeriesDate", "DataType": "datetime" },
        { "Name": "SeriesDescription", "DataType": "string" },
        { "Name": "InstanceNumber", "DataType": "int" },
        { "Name": "SliceLocation", "DataType": "decimal" }
      ]
    }
  ]
}
```

### Mapping Output (`mappings.json`)

After mapping both CSV types, the application generates a mapping file like this:

```json
{
  "Mappings": [
    {
      "TableName": "PatientStudy",
      "CsvType": "PatientStudy",
      "ColumnMappings": [
        { "CsvColumn": "pat_id", "DbColumn": "PatientId" },
        { "CsvColumn": "patient_name", "DbColumn": "PatientName" },
        { "CsvColumn": "age", "DbColumn": "Age" },
        { "CsvColumn": "gender", "DbColumn": "Gender" },
        { "CsvColumn": "study_id", "DbColumn": "StudyId" },
        { "CsvColumn": "study_date", "DbColumn": "StudyDate" },
        { "CsvColumn": "modality", "DbColumn": "Modality" }
      ]
    },
    {
      "TableName": "SeriesInstance",
      "CsvType": "SeriesInstance",
      "ColumnMappings": [
        { "CsvColumn": "series_id", "DbColumn": "SeriesId" },
        { "CsvColumn": "study_id", "DbColumn": "StudyId" },
        { "CsvColumn": "instance_id", "DbColumn": "InstanceId" },
        { "CsvColumn": "series_date", "DbColumn": "SeriesDate" },
        { "CsvColumn": "series_description", "DbColumn": "SeriesDescription" },
        { "CsvColumn": "instance_number", "DbColumn": "InstanceNumber" },
        { "CsvColumn": "slice_location", "DbColumn": "SliceLocation" }
      ]
    }
  ]
}
```

## Application Architecture

The application follows the MVVM (Model-View-ViewModel) pattern with Dependency Injection:

### Core Components

1. **Models**: Data structures representing schema and mapping information
   - `CsvColumn`: Represents a column from the CSV file with sample values
   - `DatabaseColumn`: Represents a column in the database schema
   - `SchemaTable`: Represents a table in the database schema
   - `ColumnMapping`: Maps a CSV column to a database column

2. **ViewModels**: Business logic and UI state management
   - `MainViewModel`: Manages the application state and operations
   - `ColumnMappingViewModel`: Handles individual column mapping UI interactions
   - `CsvMappingTypeViewModel`: Manages a specific CSV type mapping

3. **Views**: User interface components
   - `MappingView`: The main UI for mapping operations
   - `MainWindow`: Application container

4. **Services**: Business logic components
   - `CsvParserService`: Handles CSV file parsing and data type inference
   - `SchemaLoaderService`: Loads database schema from JSON
   - `MappingService`: Handles validation and persistence of mappings

### Workflow

1. User loads database schema
2. User selects CSV type (PatientStudy/SeriesInstance)
3. User loads CSV file
4. Application parses CSV headers and sample data
5. Application auto-matches columns where possible
6. User manually adjusts mappings as needed
7. Application validates mappings in real-time
8. User saves mappings to JSON file
9. User can switch to other CSV type and repeat the process

## Development

This project was built using:

- .NET 8
- WPF (Windows Presentation Foundation)
- MVVM Pattern with CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection for DI
- Newtonsoft.Json for JSON handling

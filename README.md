# CSV Mapper

A .NET 8 WPF application for mapping CSV files to database staging tables with powerful transformation capabilities.

## Overview

This application allows users to load CSV files containing medical imaging data and map their columns to a staging database schema. The application supports various column transformations including name splitting, date formatting, and category standardization.

## Features

- Load and parse CSV files with automatic column detection
- Load database schema from JSON configuration
- Map CSV columns to database columns with automatic matching
- Support for multiple CSV types (PatientStudy and SeriesInstance)
- Validate mappings against schema requirements
- Save mappings to JSON for reuse
- Advanced column transformation capabilities

## Transformation Framework

The application includes a comprehensive transformation framework that allows for:

1. **Text Transformations**
   - Split names into components (first name, last name)
   - Extract specific tokens from text

2. **Date Transformations**
   - Standardize dates to consistent formats
   - Extract components like year from dates

3. **Category Transformations**
   - Map various gender representations to standard codes (M/F/O/U)
   - Standardize other categorical values

## Sample Files

The repository includes sample files to demonstrate the application's capabilities:

- `sample_patient_study.csv` - Contains patient and study information
- `sample_series_instance.csv` - Contains series and image information
- `stagingdb_details.json` - Contains the database schema definition

### Transformation Examples

The following transformations can be demonstrated using the sample files:

1. **Name Splitting**: Convert "Smith, John" to separate FirstName and LastName fields
   - Source: PatientName column in PatientStudy.csv
   - Transformation: SplitTextTransformation
   - Target: FirstName and LastName columns in the database

2. **Date Standardization**: Convert various date formats to ISO standard
   - Source: PatientDOB column in PatientStudy.csv or SeriesDate in SeriesInstance.csv
   - Transformation: DateFormatTransformation
   - Target: BirthDate or SeriesDateTime columns in the database

3. **Gender Standardization**: Map "Male", "M", "male" all to a single "M" value
   - Source: PatientGender column in PatientStudy.csv
   - Transformation: CategoryMappingTransformation
   - Target: GenderCode column in the database

4. **Date Component Extraction**: Extract year from dates
   - Source: PatientDOB or StudyDate columns
   - Transformation: DateFormatTransformation with special formatting
   - Target: BirthYear or StudyYear columns in the database

## Using the Application

1. Start the application
2. The application will attempt to load the schema from `stagingdb_details.json`
3. Select a CSV type from the dropdown (PatientStudy or SeriesInstance)
4. Click "Browse" to select the corresponding sample CSV file
5. The application will automatically attempt to match columns
6. For columns requiring transformation (like PatientName to FirstName/LastName), use the transformation options
7. Validate the mappings to ensure they meet schema requirements
8. Save the mappings for future use

## Technologies Used

- .NET 8
- WPF (Windows Presentation Foundation)
- MVVM Architecture
- C# 12
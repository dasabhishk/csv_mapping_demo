using CsvMapper.Helpers;
using CsvMapper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service for parsing CSV files and extracting column information
    /// </summary>
    public class CsvParserService : ICsvParserService
    {
        private const int MAX_SAMPLE_ROWS = 5;

        /// <summary>
        /// Parses a CSV file and extracts column headers and sample data
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>List of CSV columns with sample data and inferred types</returns>
        public async Task<List<CsvColumn>> ParseCsvFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified CSV file was not found.", filePath);
            }

            List<CsvColumn> columns = new List<CsvColumn>();
            
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Read the header line
                    string? headerLine = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        throw new InvalidOperationException("CSV file is empty or has no headers.");
                    }

                    // Split the header by comma and initialize columns
                    string[] headers = headerLine.Split(',');
                    for (int i = 0; i < headers.Length; i++)
                    {
                        columns.Add(new CsvColumn 
                        { 
                            Name = headers[i].Trim(), 
                            Index = i,
                            SampleValues = new List<string>() 
                        });
                    }

                    // Read up to MAX_SAMPLE_ROWS to get sample values
                    int rowCount = 0;
                    while (rowCount < MAX_SAMPLE_ROWS && !reader.EndOfStream)
                    {
                        string? line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        // Split by comma, with handling for quoted values containing commas
                        string[] values = SplitCsvLine(line);
                        
                        // Add each value to the corresponding column's sample values
                        for (int i = 0; i < Math.Min(values.Length, columns.Count); i++)
                        {
                            columns[i].SampleValues.Add(values[i].Trim());
                        }
                        
                        rowCount++;
                    }
                }

                // Infer data types for each column
                foreach (var column in columns)
                {
                    column.InferredType = InferColumnType(column.SampleValues);
                }

                return columns;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing CSV file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Splits a CSV line handling quoted fields that may contain commas
        /// </summary>
        private string[] SplitCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string currentField = "";
            
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            
            // Add the last field
            result.Add(currentField);
            
            return result.ToArray();
        }

        /// <summary>
        /// Infers the data type from a list of sample values
        /// </summary>
        private string InferColumnType(List<string> sampleValues)
        {
            if (sampleValues.Count == 0)
                return "string";

            // Try to infer types in order of specificity
            if (sampleValues.All(DataTypeHelper.IsInteger))
                return "int";

            if (sampleValues.All(DataTypeHelper.IsDecimal))
                return "decimal";

            if (sampleValues.All(DataTypeHelper.IsDateTime))
                return "datetime";

            // Default to string if no specific type is detected
            return "string";
        }
    }
}

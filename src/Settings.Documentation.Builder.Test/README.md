# Settings.Documentation.Builder Tests

This test project contains unit tests for the Settings Documentation Builder.

## Test Coverage

### Configuration Binding Tests

1. **WhenBuildingFromDictionary_ShouldDeserializeCorrectly**
   - Tests that configuration values from a dictionary are correctly deserialized into a strongly-typed options object
   - Verifies various data types: int, string, bool, TimeSpan

2. **WhenBuildingFromDictionary_WithMissingValues_ShouldUseDefaults**
   - Tests that when configuration values are missing, the default values from the class are preserved

3. **WhenBuildingFromDictionary_WithCollections_ShouldDeserializeCorrectly**
   - Tests that collection properties (IReadOnlyCollection<string>) are correctly deserialized from configuration

### DataContract/DataMember Tests

4. **WhenObjectIsDecoratedWithDataContract_ShouldBindAllPublicProperties**
   - Tests that configuration binding works correctly with classes decorated with [DataContract] and [DataMember] attributes
   - Verifies that all public properties are bound, as the configuration binder doesn't restrict based on DataMember attributes

5. **WhenCreatingSettingsValue_WithDataMemberName_ShouldUsePropertyName**
   - Tests that the Settings Documentation Builder correctly extracts property metadata from DataContract types

6. **WhenCreatingSettingsValue_FromDataContract_ShouldExtractMetadata**
   - Tests that the builder correctly extracts descriptions, required attributes, and secret attributes from DataContract-decorated classes

7. **WhenCreatingSettingsValue_WithRequiredAttribute_ShouldMarkAsRequired**
   - Tests that properties marked with [Required] attribute are correctly identified as required settings

## Test Classes

### TestOptions
A test configuration class with various property types:
- Integer (Port)
- String (Host)
- Boolean (IsEnabled)
- TimeSpan (Timeout)
- Integer (MaxRetries)
- Collection (Tags)

### DataContractOptions
A test configuration class decorated with [DataContract] to verify behavior with serialization attributes:
- Properties marked with [DataMember]
- Properties marked with [SettingsSecret]
- Properties marked with [Required]
- Properties without [DataMember] to verify configuration binding behavior

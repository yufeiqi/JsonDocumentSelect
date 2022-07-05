# JsonDocumentSelect
JsonDocumentSelect is a class library to extract key and values from JSON (System.Text.Json.JsonDocument) with single line expressions (JsonPath)

The JsonPath parser is based on the [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) and [JsonDocumentPath](https://github.com/azambrano/JsonDocumentPath)

But the JsonDocumentPath not update for a long time and not accept my pr for a long time, so I decide to create the JsonDocumentSelect library.

[NuGet Package](https://www.nuget.org/packages/JsonDocumentSelect)

### How to use it
```csharp
  string json = @"{
    ""persons"": [
      {
        ""name""  : ""John"",
        ""age"": ""26""
      },
      {
        ""name""  : ""Jane"",
        ""age"": ""2""
      }
    ]
  }";

var models = JsonDocument.Parse(json);

var results = models.SelectElements("$.persons[*].name").ToList();
```
The result 
```csharp
[ { "Element": "John", "Name": "name" }, { "Element": "Jane", "Name": "name" } ]
```

### Implementation
Because JsonDocumentSelect is using the same Json.net strategic for parsing and evaluation the following is a list of pieces were already implemented:

#### Filters:
- [x] ArrayIndexFilter
- [x] ArrayMultipleIndexFilter
- [x] ArraySliceFilter
- [x] FieldFilter
- [x] FieldMultipleFilter
- [x] QueryFilter
- [x] QueryScanFilter
- [x] RootFilter
- [x] ScanFilter
- [x] ScanMultipleFilter

#### Unit Test:
- [x] JPathParseTests
- [x] QueryExpressionTests
- [x] JPathExecuteTests

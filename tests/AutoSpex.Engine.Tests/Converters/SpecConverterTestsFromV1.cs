using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Converters;

[TestFixture]
public class SpecConverterTestsFromV1
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonSpecConverter() }
    };

    [Test]
    public Task Deserialize_Example01_ShouldBeExpected()
    {
        const string json = """
                            {
                              "SchemaVersion": 1,
                              "SpecId": "1dbb81da-42dd-40b9-a77b-75db7042ce77",
                              "Element": "Module",
                              "Filters": [],
                              "Verifications": [
                                {
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": "L5Sharp.Core.Module:Inhibited",
                                  "Negation": "Is",
                                  "Operation": "Equal To",
                                  "Argument": {
                                    "Group": "Boolean",
                                    "Data": false
                                  }
                                },
                                {
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": "L5Sharp.Core.Module:Vendor",
                                  "Negation": "Is",
                                  "Operation": "Greater Than Or Equal To",
                                  "Argument": {
                                    "Group": "Number",
                                    "Data": "12"
                                  }
                                },
                                {
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": "L5Sharp.Core.Module:Revision",
                                  "Negation": "Is",
                                  "Operation": "Greater Than",
                                  "Argument": {
                                    "Group": "Number",
                                    "Data": "5.1"
                                  }
                                }
                              ]
                            }
                            """;

        var spec = JsonSerializer.Deserialize<Spec>(json, Options);
        spec.Should().NotBeNull();
        
        var result = JsonSerializer.Serialize(spec, Options);
        return Verify(result);
    }

    [Test]
    public Task Deserialize_Example02_ShouldBeExpected()
    {
        const string json = """
                            {
                              "SchemaVersion": 1,
                              "SpecId": "4379b169-be95-48c3-a1ee-40e90d74540b",
                              "Element": "Program",
                              "Filters": [],
                              "Verifications": [
                                {
                                  "Type": "L5Sharp.Core.Program",
                                  "Property": "L5Sharp.Core.Program:Disabled",
                                  "Negation": "Is",
                                  "Operation": "Equal To",
                                  "Argument": {
                                    "Group": "Boolean",
                                    "Data": false
                                  }
                                },
                                {
                                  "Type": "L5Sharp.Core.Program",
                                  "Property": "L5Sharp.Core.Program:Children",
                                  "Negation": "Is",
                                  "Operation": "Any",
                                  "Argument": {
                                    "Group": "Criterion",
                                    "Data": {
                                      "Type": "System.String",
                                      "Property": "System.String:This",
                                      "Negation": "Is",
                                      "Operation": "Containing",
                                      "Argument": {
                                        "Group": "Text",
                                        "Data": "Main"
                                      }
                                    }
                                  }
                                }
                              ]
                            }
                            """;

        var spec = JsonSerializer.Deserialize<Spec>(json, Options);
        spec.Should().NotBeNull();
        
        var result = JsonSerializer.Serialize(spec, Options);
        return Verify(result);
    }

    [Test]
    public Task Deserialize_Example03_ShouldBeExpected()
    {
      const string json = """
                          {
                            "SchemaVersion": 1,
                            "SpecId": "b5019ba9-81a3-451a-ad2e-c612ffad97c6",
                            "Element": "None",
                            "Filters": [],
                            "Verifications": []
                          }
                          """;

      var spec = JsonSerializer.Deserialize<Spec>(json, Options);
      spec.Should().NotBeNull();
        
      var result = JsonSerializer.Serialize(spec, Options);
      return Verify(result);
    }
}
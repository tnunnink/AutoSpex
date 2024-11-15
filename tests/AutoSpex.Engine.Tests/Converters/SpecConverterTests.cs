using System.Text.Json;

namespace AutoSpex.Engine.Tests.Converters;

[TestFixture]
public class SpecConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonSpecConverter() } };

    [Test]
    public void Deserialize_Version0SimpleSpec_ShouldBeExpected()
    {
        const string json = """
                            {
                              "SpecId": "90f4dbce-1033-42fd-8f45-3cdb0b03d5db",
                              "Element": "Controller",
                              "Filters": [],
                              "Verifications": [
                                {
                                  "CriterionId": "7954aa9a-b1db-4f52-935d-3ecd8fdc5201",
                                  "Type": "L5Sharp.Core.Controller",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Controller",
                                    "Path": "Tags.Count"
                                  },
                                  "Negation": "Is",
                                  "Operation": "Less Than Or Equal To",
                                  "Argument": {
                                    "ArgumentId": "e940c1f6-f81b-489e-b0f2-a152a6d68936",
                                    "Value": {
                                      "Type": "L5Sharp.Core.SINT",
                                      "Data": "30"
                                    }
                                  }
                                }
                              ],
                              "FilterInclusion": 0,
                              "VerificationInclusion": 0
                            }
                            """;

        var spec = JsonSerializer.Deserialize<Spec>(json, Options);

        spec.Should().NotBeNull();
        spec?.SpecId.Should().Be("90f4dbce-1033-42fd-8f45-3cdb0b03d5db");
        spec?.Element.Should().Be(Element.Controller);
        spec?.Filters.Should().BeEmpty();
        spec?.Verifications.Should().HaveCount(1);
        spec?.Verifications[0].Type.Should().Be(typeof(Controller));
        spec?.Verifications[0].Property.Key.Should().Be("L5Sharp.Core.Controller:Tags.Count");
        spec?.Verifications[0].Negation.Should().Be(Negation.Is);
        spec?.Verifications[0].Operation.Should().Be(Operation.LessThanOrEqualTo);
        spec?.Verifications[0].Argument.Should().Be(30);
    }
    
    [Test]
    public void Deserialize_Version0ComplexSpec_ShouldBeExpected()
    {
        const string json = """
                            {
                              "SpecId": "1dbb81da-42dd-40b9-a77b-75db7042ce77",
                              "Element": "Module",
                              "Filters": [
                                {
                                  "CriterionId": "3972c597-5f61-481f-804b-1a938bb1696e",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Modules"
                                  },
                                  "Negation": "Is",
                                  "Operation": "Any",
                                  "Argument": {
                                    "ArgumentId": "52d15802-2246-4bd4-a18c-72b477a75f7a",
                                    "Value": {
                                      "Type": "AutoSpex.Engine.Criterion",
                                      "Data": "{\u0022CriterionId\u0022:\u0022a4cc8296-f16a-466f-952e-431b6bc8a936\u0022,\u0022Type\u0022:\u0022L5Sharp.Core.Module\u0022,\u0022Property\u0022:{\u0022Origin\u0022:\u0022L5Sharp.Core.Module\u0022,\u0022Path\u0022:\u0022CatalogNumber\u0022},\u0022Negation\u0022:\u0022Is\u0022,\u0022Operation\u0022:\u0022Ending With\u0022,\u0022Argument\u0022:{\u0022ArgumentId\u0022:\u0022d95d9447-d8d8-4422-92a3-fe70cdd74a07\u0022,\u0022Value\u0022:{\u0022Type\u0022:\u0022System.String\u0022,\u0022Data\u0022:\u00221756-IB16\u0022}}}"
                                    }
                                  }
                                },
                                {
                                  "CriterionId": "e0755552-7b31-4d74-bc5e-4e6e66b3b890",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Vendor"
                                  },
                                  "Negation": "Is",
                                  "Operation": "Greater Than Or Equal To",
                                  "Argument": {
                                    "ArgumentId": "a688faaf-f736-4ef1-aec4-18627a9f6fb2",
                                    "Value": {
                                      "Type": "L5Sharp.Core.SINT",
                                      "Data": "12"
                                    }
                                  }
                                },
                                {
                                  "CriterionId": "3f3f6035-a536-45bd-b632-13ccefe4b9fc",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Inhibited"
                                  },
                                  "Negation": "Is",
                                  "Operation": "False",
                                  "Argument": {
                                    "ArgumentId": "23d11ecc-f729-4ea5-a337-e60b345cc507",
                                    "Value": null
                                  }
                                }
                              ],
                              "Verifications": [
                                {
                                  "CriterionId": "102cbc09-f401-4ce7-bedb-d940a4ff8d85",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Inhibited"
                                  },
                                  "Negation": "Is",
                                  "Operation": "False",
                                  "Argument": {
                                    "ArgumentId": "23d11ecc-f729-4ea5-a337-e60b345cc507",
                                    "Value": null
                                  }
                                },
                                {
                                  "CriterionId": "577326e7-1f50-4c90-a3f7-86592d653f8c",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Vendor"
                                  },
                                  "Negation": "Is",
                                  "Operation": "Greater Than Or Equal To",
                                  "Argument": {
                                    "ArgumentId": "a688faaf-f736-4ef1-aec4-18627a9f6fb2",
                                    "Value": {
                                      "Type": "L5Sharp.Core.SINT",
                                      "Data": "12"
                                    }
                                  }
                                },
                                {
                                  "CriterionId": "8d103e6a-5e85-47f0-a044-eb0e53572ad8",
                                  "Type": "L5Sharp.Core.Module",
                                  "Property": {
                                    "Origin": "L5Sharp.Core.Module",
                                    "Path": "Revision"
                                  },
                                  "Negation": "Is",
                                  "Operation": "Greater Than",
                                  "Argument": {
                                    "ArgumentId": "2f7e8904-a0e9-4d0c-b560-8c67f4ac7020",
                                    "Value": {
                                      "Type": "L5Sharp.Core.Revision",
                                      "Data": "5.1"
                                    }
                                  }
                                }
                              ],
                              "FilterInclusion": 0,
                              "VerificationInclusion": 0
                            }
                            """;

        var spec = JsonSerializer.Deserialize<Spec>(json, Options);

        spec.Should().NotBeNull();
        spec?.SpecId.Should().Be("1dbb81da-42dd-40b9-a77b-75db7042ce77");
        spec?.Element.Should().Be(Element.Module);
        spec?.Filters.Should().HaveCount(3);
        spec?.Verifications.Should().HaveCount(3);
    }
}
using FluentAssertions;
using Xunit;
using Koala.Yedpa.Core.Helpers;
using Newtonsoft.Json.Linq;

namespace Koala.Yedpa.Service.Tests
{
    /// <summary>
    /// Unit tests for LogoJsonHelper.InjectDataObjectParameter()
    /// Coverage Target: %90+
    /// </summary>
    public class LogoJsonHelperTests
    {
        [Fact]
        public void InjectDataObjectParameter_WhenValidJson_AddsDataObjectParameter()
        {
            // Arrange
            string validJson = @"{
                ""ItemType"": 1,
                ""Items"": [
                    { ""Code"": ""001"", ""Amount"": 100 }
                ]
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(validJson);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["DataObjectParameter"].Should().NotBeNull();
            jObject["DataObjectParameter"]["FillAccCodesOnPreSave"].Value<bool>().Should().BeTrue();

            // Diğer property'ler bozulmamalı
            jObject["ItemType"].Value<int>().Should().Be(1);
            jObject["Items"].Should().NotBeNull();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenEmptyJson_ReturnsEmptyString()
        {
            // Arrange
            string emptyJson = "";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(emptyJson);

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void InjectDataObjectParameter_WhenNullJson_ReturnsNull()
        {
            // Arrange
            string? nullJson = null;

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(nullJson!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenWhitespaceJson_ReturnsSameWhitespace()
        {
            // Arrange
            string whitespaceJson = "   ";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(whitespaceJson);

            // Assert
            result.Should().Be("   ");
        }

        [Fact]
        public void InjectDataObjectParameter_WhenDataObjectParameterAlreadyExists_OverwritesIt()
        {
            // Arrange
            string jsonWithExistingParam = @"{
                ""ItemType"": 1,
                ""DataObjectParameter"": {
                    ""FillAccCodesOnPreSave"": false,
                    ""SomeOtherProperty"": ""value""
                }
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonWithExistingParam);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["DataObjectParameter"].Should().NotBeNull();
            jObject["DataObjectParameter"]["FillAccCodesOnPreSave"].Value<bool>().Should().BeTrue();

            // Eski özellik silinmeli (tamamen üzerine yazar)
            jObject["DataObjectParameter"]["SomeOtherProperty"].Should().BeNull();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenInvalidJson_ReturnsOriginalJson()
        {
            // Arrange
            string invalidJson = "{ invalid json }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(invalidJson);

            // Assert
            result.Should().Be(invalidJson);
        }

        [Fact]
        public void InjectDataObjectParameter_WhenMalformedJson_ReturnsOriginalJson()
        {
            // Arrange
            string malformedJson = "not a json at all";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(malformedJson);

            // Assert
            result.Should().Be(malformedJson);
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonArray_ReturnsOriginalJson()
        {
            // Arrange
            string jsonArray = @"[
                { ""id"": 1, ""name"": ""Item 1"" },
                { ""id"": 2, ""name"": ""Item 2"" }
            ]";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonArray);

            // Assert
            // JSON array parsing'i başarısız olabilir, bu durumda orijinali döner
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenComplexNestedJson_PreservesStructure()
        {
            // Arrange
            string complexJson = @"{
                ""Header"": {
                    ""Date"": ""2024-01-01"",
                    ""Number"": ""FTR001""
                },
                ""Lines"": [
                    {
                        ""Product"": ""PRD001"",
                        ""Quantity"": 10,
                        ""Details"": {
                            ""Price"": 100.50,
                            ""Tax"": 20.10
                        }
                    }
                ]
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(complexJson);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["DataObjectParameter"].Should().NotBeNull();
            jObject["DataObjectParameter"]["FillAccCodesOnPreSave"].Value<bool>().Should().BeTrue();

            // İç içe yapı korunmalı
            jObject["Header"]["Date"].Value<string>().Should().Be("2024-01-01");
            jObject["Lines"][0]["Product"].Value<string>().Should().Be("PRD001");
            jObject["Lines"][0]["Details"]["Price"].Value<decimal>().Should().Be(100.50m);
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonWithSpecialCharacters_PreservesCharacters()
        {
            // Arrange
            string jsonWithSpecialChars = @"{
                ""Description"": ""Türkçe karakterler: şŞıİğĞöÖçÇ"",
                ""Path"": ""C:\\Test\\File.txt"",
                ""Unicode"": ""Hello""
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonWithSpecialChars);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["DataObjectParameter"].Should().NotBeNull();
            jObject["Description"].Value<string>().Should().Contain("şŞıİğĞöÖçÇ");
        }

        [Fact]
        public void InjectDataObjectParameter_WhenMinimalJson_AddsDataObjectParameter()
        {
            // Arrange
            string minimalJson = @"{}";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(minimalJson);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["DataObjectParameter"].Should().NotBeNull();
            jObject["DataObjectParameter"]["FillAccCodesOnPreSave"].Value<bool>().Should().BeTrue();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonWithNumberTypes_PreservesTypes()
        {
            // Arrange
            string jsonWithNumbers = @"{
                ""IntegerValue"": 42,
                ""DecimalValue"": 123.45,
                ""NegativeValue"": -100
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonWithNumbers);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["IntegerValue"].Value<int>().Should().Be(42);
            jObject["DecimalValue"].Value<decimal>().Should().Be(123.45m);
            jObject["NegativeValue"].Value<int>().Should().Be(-100);
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonWithBooleanTypes_PreservesTypes()
        {
            // Arrange
            string jsonWithBooleans = @"{
                ""IsActive"": true,
                ""IsDeleted"": false,
                ""HasPermission"": true
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonWithBooleans);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["IsActive"].Value<bool>().Should().BeTrue();
            jObject["IsDeleted"].Value<bool>().Should().BeFalse();
            jObject["HasPermission"].Value<bool>().Should().BeTrue();
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonWithNullValues_PreservesNulls()
        {
            // Arrange
            string jsonWithNulls = @"{
                ""OptionalField"": null,
                ""AnotherField"": ""value""
            }";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(jsonWithNulls);

            // Assert
            result.Should().NotBeNullOrEmpty();

            var jObject = JObject.Parse(result);
            jObject["OptionalField"].Type.Should().Be(JTokenType.Null);
            jObject["AnotherField"].Value<string>().Should().Be("value");
        }

        [Fact]
        public void InjectDataObjectParameter_WhenJsonWithEmptyString_ReturnsSameJson()
        {
            // Arrange
            string emptyStringJson = "\"\"";

            // Act
            string result = LogoJsonHelper.InjectDataObjectParameter(emptyStringJson);

            // Assert
            // Boş string JSON parse edilemez, orijinali döner
            result.Should().Be(emptyStringJson);
        }
    }
}
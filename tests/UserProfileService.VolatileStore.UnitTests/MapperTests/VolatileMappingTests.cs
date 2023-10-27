using System.Text.Json.Nodes;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Adapter.Marten.EntityModels;
using UserProfileService.Adapter.Marten.Helpers;
using Xunit;

namespace UserProfileService.VolatileStore.UnitTests.MapperTests;

public class VolatileMappingTests
{
    private readonly IMapper? _mapper;
    private static readonly DateTime _createAt = DateTime.Now;
    // ReSharper disable StringLiteralTypo
    private const string JsonString = "{\"Meat\":[\"Chicke\", \"Steak\"] }";

    private const string JsonJsonObjectString = "{\r\n      \"userName\": \"kolja.sch\u00FCmann\",\r\n      \"firstName\": \"Kolja\",\r\n      \"lastName\": \"Sch\u00FCmann\",\r\n      \"email\": \"kolja.sch\u00FCmann@av360.org\",\r\n  \"userStatus\": null,\r\n      \"id\": \"992c3985-14b7-4c1f-8128-759590317999\",\r\n      \"name\": \"Kolja Sch\u00FCmann\",\r\n      \"displayName\": \"Kolja Sch\u00FCmann\",\r\n      \"kind\": \"User\",\r\n      \"createdAt\": \"2023-01-02T19:15:11.2484644Z\",\r\n      \"updatedAt\": \"2023-01-02T19:15:11.2484644Z\",\r\n   \"synchronizedAt\": null,\r\n      \"source\": \"Ldap\",\r\n      \"domain\": \"ad.av360.org\",\r\n      \"externalIds\": [\r\n        {\r\n          \"id\": \"S-1-5-21-966539559-2079964620-3194842515-1455\",\r\n          \"source\": \"Ldap\",\r\n          \"isConverted\": false\r\n        }\r\n      ]\r\n    }";

    // ReSharper restore StringLiteralTypo

    public VolatileMappingTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAutoMapper(typeof(UserSettingMapper));
        _mapper = serviceCollection.BuildServiceProvider().GetService<IMapper>();
    }
    
    
    public static List<object[]> TestParameterSectionToSectionDb = new List<object[]>
                                                                    {
                                                                        new object[]
                                                                        {
                                                                            new UserSettingSection
                                                                            {
                                                                                Name = "Favorites",
                                                                                Id =
                                                                                    "46C07198-8DB1-4DB5-BE69-7521BE9AAAFF",
                                                                                CreatedAt = _createAt
                                                                            },

                                                                            new UserSettingSectionDbModel
                                                                            {
                                                                                Name = "Favorites",
                                                                                Id =
                                                                                    "46C07198-8DB1-4DB5-BE69-7521BE9AAAFF",
                                                                                CreatedAt = _createAt
                                                                            }
                                                                        }
                                                                    };

    public static List<object[]> TestParameterSectionDbToSection = new List<object[]>
                                                                    {
                                                                        new object[]
                                                                        {
                                                                            new UserSettingSectionDbModel
                                                                            {
                                                                                Name = "Favorites",
                                                                                Id =
                                                                                    "81ED5B22-6DB7-4DF4-8E24-820B07DD5D83",
                                                                                CreatedAt = _createAt.AddDays(-2)
                                                                            },
                                                                            new UserSettingSection
                                                                            {
                                                                                Name = "Favorites",
                                                                                Id =
                                                                                    "81ED5B22-6DB7-4DF4-8E24-820B07DD5D83",
                                                                                CreatedAt = _createAt.AddDays(-2)
                                                                            }
                                                                        }
                                                                    };

    public static List<object[]> TestParameterUserSettingObjectToUserSettingsObjectDb = new List<object[]>
        {
            new object[]
            {
                new UserSettingObject
                {
                    CreatedAt = _createAt,
                    UpdatedAt = _createAt,
                    UserSettingSection = new UserSettingSection
                                         {
                                             Name = "FavoritesMeat",
                                             CreatedAt = _createAt.AddDays(-17),
                                             Id = "A1216A9C-C615-4287-932C-8F11585829DC"
                                         },
                    Id = "28A02BFA-A7AD-4A29-97D0-7669CBB8BB11",
                    UserId = "0CC12C38-F5E7-40F9-8F19-50EAB21191C3",
                    UserSettingsObject = JsonNode.Parse(JsonString)?.AsObject()
                },
                new UserSettingObjectDbModel
                {
                    CreatedAt = _createAt,
                    UpdatedAt = _createAt,
                    UserSettingSection = new UserSettingSectionDbModel
                                         {
                                             Name = "FavoritesMeat",
                                             CreatedAt = _createAt.AddDays(-17),
                                             Id = "A1216A9C-C615-4287-932C-8F11585829DC"
                                         },
                    Id = "28A02BFA-A7AD-4A29-97D0-7669CBB8BB11",
                    UserId = "0CC12C38-F5E7-40F9-8F19-50EAB21191C3",
                    UserSettingsObject = JsonNode.Parse(JsonString)?.AsObject() ?? new JsonObject()
                }
            }
        };

    public static List<object[]> TestParameterUserSettingsObjectDbToUserSettingObject = new List<object[]>
        {
            new object[]
            {
                new UserSettingObjectDbModel
                {
                    CreatedAt = _createAt,
                    UpdatedAt = _createAt,
                    UserSettingSection = new UserSettingSectionDbModel
                                         {
                                             Name = "FavoritesMeat",
                                             CreatedAt = _createAt.AddDays(-10),
                                             Id = "241A5C65-9D30-4851-AF5F-FB53254DE4DB"
                                         },
                    Id = "9D10CEB2-8B28-4533-8E68-2C58353A9B83",
                    UserId = "5EDBB37D-CC37-414A-8C94-4969E780D762",
                    UserSettingsObject = JsonNode.Parse(JsonString)?.AsObject() ?? new JsonObject()
                },

                new UserSettingObject
                {
                    CreatedAt = _createAt,
                    UpdatedAt = _createAt,
                    UserSettingSection = new UserSettingSection
                                         {
                                             Name = "FavoritesMeat",
                                             CreatedAt = _createAt.AddDays(-10),
                                             Id = "241A5C65-9D30-4851-AF5F-FB53254DE4DB"
                                         },
                    Id = "9D10CEB2-8B28-4533-8E68-2C58353A9B83",
                    UserId = "5EDBB37D-CC37-414A-8C94-4969E780D762",
                    UserSettingsObject = JsonNode.Parse(JsonString)?.AsObject()
                }
            }
        };
     
    public static List<object[]> JObjectToJObject = new List<object[]>
        {
            new object[]
            {
                JsonNode.Parse(JsonJsonObjectString)?.AsObject() ?? new JsonObject(),
                JsonNode.Parse(JsonJsonObjectString)?.AsObject() ?? new JsonObject()
            }
        };
    
    [Theory]
    [MemberData(nameof(TestParameterSectionToSectionDb))]
    public void Section_To_Section_DBMapping_Test_Should_Work(UserSettingSection section, UserSettingSectionDbModel result)
    {
        var sectionMapping = _mapper?.Map<UserSettingSectionDbModel>(section);
        sectionMapping.Should().BeEquivalentTo(result);
    }
    
    [Theory]
    [MemberData(nameof(TestParameterSectionDbToSection))]
    public void SectionDB_To_Section_Test_should_work(UserSettingSectionDbModel section, UserSettingSection result)
    {
        var sectionMapping = _mapper?.Map<UserSettingSection>(section);
        sectionMapping.Should().BeEquivalentTo(result);
    }
    
    [Theory]
    [MemberData(nameof(TestParameterUserSettingObjectToUserSettingsObjectDb))]
    public void UserSettingObject_To_UserSettingObjectDb_Test_should_work(UserSettingObject userSettingObject, UserSettingObjectDbModel result)
    {
        var userSettingObjectResult  = _mapper?.Map<UserSettingObjectDbModel>(userSettingObject);
        userSettingObjectResult.Should().BeEquivalentTo(result, opt => opt.Excluding(p => p.UserSettingsObject));
        var resultOfMapping = userSettingObjectResult?.UserSettingsObject.ToJsonString();
        var resultGiven = result.UserSettingsObject.ToJsonString();
        resultOfMapping.Should().Be(resultGiven);
    }

    [Theory]
    [MemberData(nameof(TestParameterUserSettingsObjectDbToUserSettingObject))]
    public void UserSettingObjectDb_To_UserSettingObject_Test_should_work(UserSettingObjectDbModel userSettingObject, UserSettingObject result)
    {
        var userSettingObjectResult  = _mapper?.Map<UserSettingObject>(userSettingObject);
        userSettingObjectResult.Should().BeEquivalentTo(result, opt => opt.Excluding(p => p.UserSettingsObject));
        var resultOfMapping = userSettingObjectResult?.UserSettingsObject?.ToJsonString();
        var resultGiven = result.UserSettingsObject?.ToJsonString();
        resultOfMapping.Should().Be(resultGiven);
    }

    [Theory]
    [MemberData(nameof(JObjectToJObject))]
    public void JObjectToJObject_test_should_work(JsonObject jObjectMapping, JsonObject resultJObject)
    {
        var resultMappingJsonObject = _mapper?.Map<JsonObject>(jObjectMapping);
        (resultMappingJsonObject ?? new JsonObject())
            .ToJsonString().Should().BeEquivalentTo(resultJObject.ToJsonString());
    }
    
    
}
using System.Collections;
using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Configuration;

namespace UserProfileService.Sync.UnitTests.Validation.TestData;

/// <summary>
///     Contains some test data for <see cref="SyncConfigurationValidationTests" />.
/// </summary>
public class LdapSystemConfigurationCorrectTestData : IEnumerable<object[]>
{
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
                     {
                         new LdapSystemConfiguration
                         {
                             EntitiesMapping = new Dictionary<string, string>
                                               {
                                                   { "DisplayName", "displayname" },
                                                   { "Email", "mail" },
                                                   { "FirstName", "givenName" },
                                                   { "LastName", "sn" },
                                                   { "Name", "Name" },
                                                   { "UserName", "cn" }
                                               },

                             LdapConfiguration = new[]
                                                 {
                                                     new ActiveDirectory
                                                     {
                                                         Connection = new ActiveDirectoryConnection
                                                                      {
                                                                          ConnectionString =
                                                                              "LDAP://A365DirectoryService.a365dev.de",
                                                                          ServiceUserPassword = "1",
                                                                          BasePath = "dc=a365dev,dc=de",
                                                                          AuthenticationType = "None",
                                                                          ServiceUser =
                                                                              "CN=Administrator,CN=Users,DC=A365DEV,DC=DE",
                                                                          Description =
                                                                              "Default AD of A365 DEV environment",
                                                                          Port = 636,
                                                                          IgnoreCertificate = true,
                                                                          UseSsl = true
                                                                      },
                                                         LdapQueries = new[]
                                                                       {
                                                                           new LdapQueries
                                                                           {
                                                                               Filter =
                                                                                   "(&(|(objectClass=user)(objectClass=inetOrgPerson))(!(objectClass=computer))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))",
                                                                               SearchBase = "OU=DEV-ACCOUNTS"
                                                                           }
                                                                       }
                                                     }
                                                 }
                         },
                         true
                     };
    }
}

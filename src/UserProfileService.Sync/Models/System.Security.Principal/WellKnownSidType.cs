//
// System.Security.Principal.WellKnownSidType
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;

namespace UserProfileService.Sync.Models.System.Security.Principal;

/// <summary>
///     The Ldap types as an enumeration.
/// </summary>
[ComVisible(false)]
public enum WellKnownSidType
{
    /// <summary>
    ///     The null sid type.
    /// </summary>
    NullSid,

    /// <summary>
    ///     The world sid type.
    /// </summary>
    WorldSid,

    /// <summary>
    ///     The local sid type.
    /// </summary>
    LocalSid,

    /// <summary>
    ///     The create owner sid type.
    /// </summary>
    CreatorOwnerSid,

    /// <summary>
    ///     The creator group sid type.
    /// </summary>
    CreatorGroupSid,

    /// <summary>
    ///     The creator owner server sid type.
    /// </summary>
    CreatorOwnerServerSid,

    /// <summary>
    ///     The creator group server sid type.
    /// </summary>
    CreatorGroupServerSid,

    /// <summary>
    ///     The nt-authority sid type.
    /// </summary>
    NtAuthoritySid,

    /// <summary>
    ///     The dial up sid type.
    /// </summary>
    DialupSid,

    /// <summary>
    ///     The network sid type.
    /// </summary>
    NetworkSid,

    /// <summary>
    ///     The batch sid type.
    /// </summary>
    BatchSid,

    /// <summary>
    ///     The interactive sid type.
    /// </summary>
    InteractiveSid,

    /// <summary>
    ///     The service sid type.
    /// </summary>
    ServiceSid,

    /// <summary>
    ///     The anonymous sid type.
    /// </summary>
    AnonymousSid,

    /// <summary>
    ///     The proxy sid type.
    /// </summary>
    ProxySid,

    /// <summary>
    ///     The enterprise controller sid type.
    /// </summary>
    EnterpriseControllersSid,

    /// <summary>
    ///     The self sid type.
    /// </summary>
    SelfSid,

    /// <summary>
    ///     The authenticated user sid type.
    /// </summary>
    AuthenticatedUserSid,

    /// <summary>
    ///     The restricted code sid type.
    /// </summary>
    RestrictedCodeSid,

    /// <summary>
    ///     The terminal server sid type.
    /// </summary>
    TerminalServerSid,

    /// <summary>
    ///     The remote logon id sid type.
    /// </summary>
    RemoteLogonIdSid,

    /// <summary>
    ///     The logon ids sid type.
    /// </summary>
    LogonIdsSid,

    /// <summary>
    ///     The local system sid type.
    /// </summary>
    LocalSystemSid,

    /// <summary>
    ///     The local service sid type.
    /// </summary>
    LocalServiceSid,

    /// <summary>
    ///     The network service sid type.
    /// </summary>
    NetworkServiceSid,

    /// <summary>
    ///     The builtin domain sid type.
    /// </summary>
    BuiltinDomainSid,

    /// <summary>
    ///     The builtin administrators sid type.
    /// </summary>
    BuiltinAdministratorsSid,

    /// <summary>
    ///     The builtin users sid type.
    /// </summary>
    BuiltinUsersSid,

    /// <summary>
    ///     The builtin guests sid type.
    /// </summary>
    BuiltinGuestsSid,

    /// <summary>
    ///     The builtin power users sid type.
    /// </summary>
    BuiltinPowerUsersSid,

    /// <summary>
    ///     The builtin account operators sid type.
    /// </summary>
    BuiltinAccountOperatorsSid,

    /// <summary>
    ///     The builtin system operators sid type.
    /// </summary>
    BuiltinSystemOperatorsSid,

    /// <summary>
    ///     The builtin print operators sid type.
    /// </summary>
    BuiltinPrintOperatorsSid,

    /// <summary>
    ///     The builtin backup operators sid type.
    /// </summary>
    BuiltinBackupOperatorsSid,

    /// <summary>
    ///     The builtin replicator sid type.
    /// </summary>
    BuiltinReplicatorSid,

    /// <summary>
    ///     The builtin PreWindows2000 compatible access sid type.
    /// </summary>
    BuiltinPreWindows2000CompatibleAccessSid,

    /// <summary>
    ///     The builtin remote desktop users sid type.
    /// </summary>
    BuiltinRemoteDesktopUsersSid,

    /// <summary>
    ///     The builtin network configuration operators sid type.
    /// </summary>
    BuiltinNetworkConfigurationOperatorsSid,

    /// <summary>
    ///     The account administrator sid type.
    /// </summary>
    AccountAdministratorSid,

    /// <summary>
    ///     The account guest sid type.
    /// </summary>
    AccountGuestSid,

    /// <summary>
    ///     The account Krbtgt sid type.
    /// </summary>
    AccountKrbtgtSid,

    /// <summary>
    ///     The account domain admin sid type.
    /// </summary>
    AccountDomainAdminsSid,

    /// <summary>
    ///     The account domain user sid type.
    /// </summary>
    AccountDomainUsersSid,

    /// <summary>
    ///     The account domain guest sid type.
    /// </summary>
    AccountDomainGuestsSid,

    /// <summary>
    ///     The account computers sid type.
    /// </summary>
    AccountComputersSid,

    /// <summary>
    ///     The account controllers sid type.
    /// </summary>
    AccountControllersSid,

    /// <summary>
    ///     The account cert admins sid type.
    /// </summary>
    AccountCertAdminsSid,

    /// <summary>
    ///     The account schema admins sid type.
    /// </summary>
    AccountSchemaAdminsSid,

    /// <summary>
    ///     The account enterprise admins sid type.
    /// </summary>
    AccountEnterpriseAdminsSid,

    /// <summary>
    ///     The account policy admins sid type.
    /// </summary>
    AccountPolicyAdminsSid,

    /// <summary>
    ///     The account Ras and Ias servers sid type.
    /// </summary>
    AccountRasAndIasServersSid,

    /// <summary>
    ///     The Ntlm authentication sid type.
    /// </summary>
    NtlmAuthenticationSid,

    /// <summary>
    ///     The digest authentication sid type.
    /// </summary>
    DigestAuthenticationSid,

    /// <summary>
    ///     The S-channel authentication sid type.
    /// </summary>
    SChannelAuthenticationSid,

    /// <summary>
    ///     The this organization sid type.
    /// </summary>
    ThisOrganizationSid,

    /// <summary>
    ///     The other organization sid type.
    /// </summary>
    OtherOrganizationSid,

    /// <summary>
    ///     The builtin incoming forest trust builders sid type.
    /// </summary>
    BuiltinIncomingForestTrustBuildersSid,

    /// <summary>
    ///     The builtin performance monitoring users sid type.
    /// </summary>
    BuiltinPerformanceMonitoringUsersSid,

    /// <summary>
    ///     The builtin performance logging users sid type.
    /// </summary>
    BuiltinPerformanceLoggingUsersSid,

    /// <summary>
    ///     The builtin authorization access sid type.
    /// </summary>
    BuiltinAuthorizationAccessSid,

    /// <summary>
    ///     The Win builtin terminal server license servers sid type.
    /// </summary>
    WinBuiltinTerminalServerLicenseServersSid,

    /// <summary>
    ///     The max defined sid type.
    /// </summary>
    MaxDefined = WinBuiltinTerminalServerLicenseServersSid,

    /// <summary>
    ///     The Win builtin DCOM-users sid type.
    /// </summary>
    WinBuiltinDcomUsersSid,

    /// <summary>
    ///     The win builtin I-users sid type.
    /// </summary>
    WinBuiltinIUsersSid,

    /// <summary>
    ///     The Win I-user sid type.
    /// </summary>
    WinIUserSid,

    /// <summary>
    ///     The Win builtin crypto operators sid type.
    /// </summary>
    WinBuiltinCryptoOperatorsSid,

    /// <summary>
    ///     The Win untrusted label sid type.
    /// </summary>
    WinUntrustedLabelSid,

    /// <summary>
    ///     The Win untrusted label sid type.
    /// </summary>
    WinLowLabelSid,

    /// <summary>
    ///     The Win medium label sid type.
    /// </summary>
    WinMediumLabelSid,

    /// <summary>
    ///     The Win high label sid type.
    /// </summary>
    WinHighLabelSid,

    /// <summary>
    ///     The Win system label sid type.
    /// </summary>
    WinSystemLabelSid,

    /// <summary>
    ///     The Win write restricted code sid type.
    /// </summary>
    WinWriteRestrictedCodeSid,

    /// <summary>
    ///     The Win creator owner rights sid type.
    /// </summary>
    WinCreatorOwnerRightsSid,

    /// <summary>
    ///     The Win cacheable principals group sid type.
    /// </summary>
    WinCacheablePrincipalsGroupSid,

    /// <summary>
    ///     The Win non-cache able principals group sid type.
    /// </summary>
    WinNonCacheablePrincipalsGroupSid,

    /// <summary>
    ///     The Win enterprise readonly controllers sid type.
    /// </summary>
    WinEnterpriseReadonlyControllersSid,

    /// <summary>
    ///     The Win account readonly controllers sid type.
    /// </summary>
    WinAccountReadonlyControllersSid,

    /// <summary>
    ///     The Win builtin event log readers group sid type.
    /// </summary>
    WinBuiltinEventLogReadersGroup,

    /// <summary>
    ///     The Win new enterprise readonly controllers sid type.
    /// </summary>
    WinNewEnterpriseReadonlyControllersSid,

    /// <summary>
    ///     The Win new enterprise readonly controllers sid type.
    /// </summary>
    WinBuiltinCertSvcDComAccessGroup,

    /// <summary>
    ///     The Win medium plus label sid type.
    /// </summary>
    WinMediumPlusLabelSid,

    /// <summary>
    ///     The Win local logon sid type.
    /// </summary>
    WinLocalLogonSid,

    /// <summary>
    ///     The Win console logon sid type.
    /// </summary>
    WinConsoleLogonSid,

    /// <summary>
    ///     The Win this organization certificate sid type.
    /// </summary>
    WinThisOrganizationCertificateSid,

    /// <summary>
    ///     The Win application package authority sid type.
    /// </summary>
    WinApplicationPackageAuthoritySid,

    /// <summary>
    ///     The Win builtin any package sid type.
    /// </summary>
    WinBuiltinAnyPackageSid,

    /// <summary>
    ///     The Win application package authority sid type.
    /// </summary>
    WinCapabilityInternetClientSid,

    /// <summary>
    ///     The Win capability internet client server sid type.
    /// </summary>
    WinCapabilityInternetClientServerSid,

    /// <summary>
    ///     The Win capability private network client server sid type.
    /// </summary>
    WinCapabilityPrivateNetworkClientServerSid,

    /// <summary>
    ///     The Win capability pictures library sid type.
    /// </summary>
    WinCapabilityPicturesLibrarySid,

    /// <summary>
    ///     The Win capability videos library sid type.
    /// </summary>
    WinCapabilityVideosLibrarySid,

    /// <summary>
    ///     The Win capability music library sid type.
    /// </summary>
    WinCapabilityMusicLibrarySid,

    /// <summary>
    ///     The Win capability documents library sid type.
    /// </summary>
    WinCapabilityDocumentsLibrarySid,

    /// <summary>
    ///     The Win capability shared user certificates sid type.
    /// </summary>
    WinCapabilitySharedUserCertificatesSid,

    /// <summary>
    ///     The Win capability enterprise authentication sid type.
    /// </summary>
    WinCapabilityEnterpriseAuthenticationSid,

    /// <summary>
    ///     The Win capability removable storage sid type.
    /// </summary>
    WinCapabilityRemovableStorageSid
}

//
// System.Security.Policy.SecurityIdentifier class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Kenneth Bell
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012       James Bellinger
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

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace UserProfileService.Sync.Models.System.Security.Principal;

/// <summary>
///     Specifies the security identifier for ldap.
/// </summary>
[ComVisible(false)]
public sealed class SecurityIdentifier : IdentityReference, IComparable<SecurityIdentifier>
{
    /// <summary>
    ///     The maximal length for the security identifier.
    /// </summary>
    public static readonly int MaxBinaryLength = 68;

    /// <summary>
    ///     The minimal length for the security identifier.
    /// </summary>
    public static readonly int MinBinaryLength = 8;

    private byte[] _buffer;

    /// <summary>
    ///     The count domain sid.
    /// </summary>
    public SecurityIdentifier AccountDomainSid
    {
        get
        {
            string strForm = Value;

            // Check prefix, and ensure at least 4 sub authorities
            if (!strForm.StartsWith("S-1-5-21") || _buffer[1] < 4)
            {
                return null;
            }

            // Domain is first four sub-authorities
            var temp = new byte[8 + 4 * 4];
            Array.Copy(_buffer, 0, temp, 0, temp.Length);
            temp[1] = 4;

            return new SecurityIdentifier(temp, 0);
        }
    }

    /// <summary>
    ///     The body length.
    /// </summary>
    public int BinaryLength => _buffer.Length;

    /// <summary>
    ///     The value of the <see cref="SecurityIdentifier" />.
    /// </summary>
    public override string Value
    {
        get
        {
            var s = new StringBuilder();

            ulong authority = GetSidAuthority();
            s.AppendFormat(CultureInfo.InvariantCulture, "S-1-{0}", authority);

            for (byte i = 0; i < GetSidSubAuthorityCount(); ++i)
            {
                s.AppendFormat(CultureInfo.InvariantCulture, "-{0}", GetSidSubAuthority(i));
            }

            return s.ToString();
        }
    }

    /// <summary>
    ///     Created the instance of the object <see cref="SecurityIdentifier" />
    /// </summary>
    /// <param name="sddlForm">The form that is used to create the <see cref="SecurityIdentifier" /></param>
    public SecurityIdentifier(string sddlForm)
    {
        if (sddlForm == null)
        {
            throw new ArgumentNullException(nameof(sddlForm));
        }

        _buffer = ParseSddlForm(sddlForm);
    }

    /// <summary>
    ///     Created the instance of the object <see cref="SecurityIdentifier" />
    /// </summary>
    /// <param name="binaryForm">The bin form of the <see cref="SecurityIdentifier" />.</param>
    /// <param name="offset">The offset of the <see cref="SecurityIdentifier" />.</param>
    public unsafe SecurityIdentifier(byte[] binaryForm, int offset)
    {
        if (binaryForm == null)
        {
            throw new ArgumentNullException(nameof(binaryForm));
        }

        if (offset < 0 || offset > binaryForm.Length - 2)
        {
            throw new ArgumentException("offset");
        }

        fixed (byte* binaryFormPtr = binaryForm)
        {
            CreateFromBinaryForm((IntPtr)(binaryFormPtr + offset), binaryForm.Length - offset);
        }
    }

    /// <summary>
    ///     Created the instance of the object <see cref="SecurityIdentifier" />.
    /// </summary>
    /// <param name="binaryForm">The pointer for creating the <see cref="SecurityIdentifier" />.</param>
    public SecurityIdentifier(IntPtr binaryForm)
    {
        CreateFromBinaryForm(binaryForm, int.MaxValue);
    }

    /// <summary>
    ///     Created the instance of the object <see cref="SecurityIdentifier" />.
    /// </summary>
    /// <param name="sidType">The sid type that the <see cref="SecurityIdentifier" /> should be.</param>
    /// <param name="domainSid">The domain sid.</param>
    public SecurityIdentifier(
        WellKnownSidType sidType,
        SecurityIdentifier domainSid)
    {
        WellKnownAccount acct = WellKnownAccount.LookupByType(sidType);

        if (acct == null)
        {
            throw new ArgumentException("Unable to convert SID type: " + sidType);
        }

        if (acct.IsAbsolute)
        {
            _buffer = ParseSddlForm(acct.Sid);
        }
        else
        {
            if (domainSid == null)
            {
                throw new ArgumentNullException(nameof(domainSid));
            }

            _buffer = ParseSddlForm(domainSid.Value + "-" + acct.Rid);
        }
    }

    private void CreateFromBinaryForm(IntPtr binaryForm, int length)
    {
        int revision = Marshal.ReadByte(binaryForm, 0);
        int numSubAuthorities = Marshal.ReadByte(binaryForm, 1);

        if (revision != 1 || numSubAuthorities > 15)
        {
            throw new ArgumentException("Value was invalid.");
        }

        if (length < 8 + numSubAuthorities * 4)
        {
            throw new ArgumentException("offset");
        }

        _buffer = new byte[8 + numSubAuthorities * 4];
        Marshal.Copy(binaryForm, _buffer, 0, _buffer.Length);
    }

    private ulong GetSidAuthority()
    {
        return ((ulong)_buffer[2] << 40)
            | ((ulong)_buffer[3] << 32)
            | ((ulong)_buffer[4] << 24)
            | ((ulong)_buffer[5] << 16)
            | ((ulong)_buffer[6] << 8)
            | ((ulong)_buffer[7] << 0);
    }

    private byte GetSidSubAuthorityCount()
    {
        return _buffer[1];
    }

    private uint GetSidSubAuthority(byte index)
    {
        // Note sub authorities little-endian, authority (above) is big-endian!
        int offset = 8 + index * 4;

        return ((uint)_buffer[offset + 0] << 0)
            | ((uint)_buffer[offset + 1] << 8)
            | ((uint)_buffer[offset + 2] << 16)
            | ((uint)_buffer[offset + 3] << 24);
    }

    private static byte[] ParseSddlForm(string sddlForm)
    {
        string sid = sddlForm;

        // If only 2 characters long, can't be a full SID string - so assume
        // it's an attempted alias.  Do that conversion first.
        if (sddlForm.Length == 2)
        {
            WellKnownAccount acct = WellKnownAccount.LookupBySddlForm(sddlForm);

            if (acct == null)
            {
                throw new ArgumentException(
                    "Invalid SDDL string - unrecognized account: " + sddlForm,
                    nameof(sddlForm));
            }

            if (!acct.IsAbsolute)
            {
                throw new NotImplementedException(
                    "Mono unable to convert account to SID: " + (acct.Name ?? sddlForm));
            }

            sid = acct.Sid;
        }

        string[] elements = sid.ToUpperInvariant().Split('-');
        int numSubAuthorities = elements.Length - 3;

        if (elements.Length < 3 || elements[0] != "S" || numSubAuthorities > 15)
        {
            throw new ArgumentException("Value was invalid.");
        }

        if (elements[1] != "1")
        {
            throw new ArgumentException("Only SIDs with revision 1 are supported");
        }

        var buffer = new byte[8 + numSubAuthorities * 4];
        buffer[0] = 1;
        buffer[1] = (byte)numSubAuthorities;

        if (!TryParseAuthority(elements[2], out ulong authority))
        {
            throw new ArgumentException("Value was invalid.");
        }

        buffer[2] = (byte)((authority >> 40) & 0xFF);
        buffer[3] = (byte)((authority >> 32) & 0xFF);
        buffer[4] = (byte)((authority >> 24) & 0xFF);
        buffer[5] = (byte)((authority >> 16) & 0xFF);
        buffer[6] = (byte)((authority >> 8) & 0xFF);
        buffer[7] = (byte)((authority >> 0) & 0xFF);

        for (var i = 0; i < numSubAuthorities; ++i)
        {
            if (!TryParseSubAuthority(
                    elements[i + 3],
                    out uint subAuthority))
            {
                throw new ArgumentException("Value was invalid.");
            }

            // Note sub authorities little-endian!
            int offset = 8 + i * 4;
            buffer[offset + 0] = (byte)(subAuthority >> 0);
            buffer[offset + 1] = (byte)(subAuthority >> 8);
            buffer[offset + 2] = (byte)(subAuthority >> 16);
            buffer[offset + 3] = (byte)(subAuthority >> 24);
        }

        return buffer;
    }

    private static bool TryParseAuthority(string s, out ulong result)
    {
        if (s.StartsWith("0X"))
        {
            return ulong.TryParse(
                s.Substring(2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out result);
        }

        return ulong.TryParse(
            s,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static bool TryParseSubAuthority(string s, out uint result)
    {
        if (s.StartsWith("0X"))
        {
            return uint.TryParse(
                s.Substring(2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out result);
        }

        return uint.TryParse(
            s,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out result);
    }

    internal string GetSddlForm()
    {
        string sidString = Value;

        WellKnownAccount acct = WellKnownAccount.LookupBySid(sidString);

        if (acct?.SddlForm == null)
        {
            return sidString;
        }

        return acct.SddlForm;
    }

    internal static SecurityIdentifier ParseSddlForm(string sddlForm, ref int pos)
    {
        if (sddlForm.Length - pos < 2)
        {
            throw new ArgumentException("Invalid SDDL string.", nameof(sddlForm));
        }

        string sid;
        int len;

        string prefix = sddlForm.Substring(pos, 2).ToUpperInvariant();

        if (prefix == "S-")
        {
            // Looks like a SID, try to parse it.
            int endPos = pos;

            char ch = char.ToUpperInvariant(sddlForm[endPos]);

            while (ch == 'S' || ch == '-' || ch == 'X' || (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F'))
            {
                ++endPos;
                ch = char.ToUpperInvariant(sddlForm[endPos]);
            }

            if (ch == ':' && sddlForm[endPos - 1] == 'D')
            {
                endPos--;
            }

            sid = sddlForm.Substring(pos, endPos - pos);
            len = endPos - pos;
        }
        else
        {
            sid = prefix;
            len = 2;
        }

        var ret = new SecurityIdentifier(sid);
        pos += len;

        return ret;
    }

    /// <summary>
    ///     Compare two <see cref="SecurityIdentifier" /> objects.
    /// </summary>
    /// <param name="sid">The <inheritdoc cref="SecurityIdentifier" /> that should ne compared.</param>
    /// <returns>Returns true if they match, otherwise false.</returns>
    // The CompareTo ordering was determined by unit test applied to MS.NET implementation,
    // necessary because the CompareTo has no details in its documentation.
    // (See MonoTests.System.Security.AccessControl.DiscretionaryAclTest.)
    // The comparison was determined to be: authority, then sub authority count, then sub authority.
    public int CompareTo(SecurityIdentifier sid)
    {
        if (sid == null)
        {
            throw new ArgumentNullException(nameof(sid));
        }

        int result;

        if (0 != (result = GetSidAuthority().CompareTo(sid.GetSidAuthority())))
        {
            return result;
        }

        if (0 != (result = GetSidSubAuthorityCount().CompareTo(sid.GetSidSubAuthorityCount())))
        {
            return result;
        }

        for (byte i = 0; i < GetSidSubAuthorityCount(); ++i)
        {
            if (0 != (result = GetSidSubAuthority(i).CompareTo(sid.GetSidSubAuthority(i))))
            {
                return result;
            }
        }

        return 0;
    }

    /// <summary>
    ///     Checks if two <see cref="SecurityIdentifier" /> are equal.
    /// </summary>
    /// <param name="o">The object to compare.</param>
    /// <returns>Returns true if two <see cref="SecurityIdentifier" /> are equal, otherwise false.</returns>
    public override bool Equals(object o)
    {
        return Equals(o as SecurityIdentifier);
    }

    /// <summary>
    ///     Checks if two <see cref="SecurityIdentifier" /> are equal.
    /// </summary>
    /// <param name="sid">The <see cref="SecurityIdentifier" /> object to compare.</param>
    /// <returns>Returns true if two <see cref="SecurityIdentifier" /> are equal, otherwise false.</returns>
    public bool Equals(SecurityIdentifier sid)
    {
        if (sid == null)
        {
            return false;
        }

        return sid.Value == Value;
    }

    /// <summary>
    ///     Gets the security identifier in binary form.
    /// </summary>
    /// <param name="binaryForm">The security identifies in a byte form.</param>
    /// <param name="offset">The offset.</param>
    public void GetBinaryForm(byte[] binaryForm, int offset)
    {
        if (binaryForm == null)
        {
            throw new ArgumentNullException(nameof(binaryForm));
        }

        if (offset < 0 || offset > binaryForm.Length - _buffer.Length)
        {
            throw new ArgumentException("offset");
        }

        Array.Copy(_buffer, 0, binaryForm, offset, _buffer.Length);
    }

    /// <summary>
    ///     Gets the hash of the value.
    /// </summary>
    /// <returns>Hash of the property value.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    ///     Checks if it is an account sid.
    /// </summary>
    /// <returns>True if it is an account sid, otherwise false.</returns>
    public bool IsAccountSid()
    {
        return AccountDomainSid != null;
    }

    /// <summary>
    ///     Checks if a <see cref="SecurityIdentifier" /> is a domain sid.
    /// </summary>
    /// <param name="sid">The sid to check.</param>
    /// <returns>True if it is a domain sid, otherwise false.</returns>
    public bool IsEqualDomainSid(SecurityIdentifier sid)
    {
        SecurityIdentifier domSid = AccountDomainSid;

        if (domSid == null)
        {
            return false;
        }

        return domSid.Equals(sid.AccountDomainSid);
    }

    /// <summary>
    ///     Checks if it is a valid target type.
    /// </summary>
    /// <param name="targetType">The target type to check.</param>
    /// <returns>
    ///     If the target type is a <see cref="SecurityIdentifier" /> type or <see cref="NtAccount" />-type true,
    ///     otherwise false.
    /// </returns>
    public override bool IsValidTargetType(Type targetType)
    {
        if (targetType == typeof(SecurityIdentifier))
        {
            return true;
        }

        if (targetType == typeof(NtAccount))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Checks, if the sid type is known.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>When it is well-known true, otherwise false.</returns>
    public bool IsWellKnown(WellKnownSidType type)
    {
        WellKnownAccount acct = WellKnownAccount.LookupByType(type);

        if (acct == null)
        {
            return false;
        }

        string sid = Value;

        if (acct.IsAbsolute)
        {
            return sid == acct.Sid;
        }

        return sid.StartsWith("S-1-5-21", StringComparison.OrdinalIgnoreCase)
            && sid.EndsWith("-" + acct.Rid, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Get the <see cref="SecurityIdentifier" /> as string.
    /// </summary>
    /// <returns>The <see cref="SecurityIdentifier" /> as string.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    ///     Translate the target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns> A <see cref="SecurityIdentifier" />.</returns>
    public override IdentityReference Translate(Type targetType)
    {
        if (targetType == typeof(SecurityIdentifier))
        {
            return this;
        }

        if (targetType == typeof(NtAccount))
        {
            WellKnownAccount acct = WellKnownAccount.LookupBySid(Value);

            if (acct == null || acct.Name == null)
            {
                throw new IdentityNotMappedException("Unable to map SID: " + Value);
            }

            return new NtAccount(acct.Name);
        }

        throw new ArgumentException("Unknown type.", nameof(targetType));
    }

    /// <summary>
    ///     Compare two <see cref="SecurityIdentifier" />s with the "=="-operator.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// ///
    /// <returns>True when both values are equal, otherwise false.</returns>
    public static bool operator ==(SecurityIdentifier left, SecurityIdentifier right)
    {
        if ((object)left == null)
        {
            return (object)right == null;
        }

        if ((object)right == null)
        {
            return false;
        }

        return left.Value == right.Value;
    }

    /// <summary>
    ///     Compare two <see cref="SecurityIdentifier" />s with the "!="-operator.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>True when both values are not equal, otherwise false.</returns>
    public static bool operator !=(SecurityIdentifier left, SecurityIdentifier right)
    {
        if ((object)left == null)
        {
            return (object)right != null;
        }

        if ((object)right == null)
        {
            return true;
        }

        return left.Value != right.Value;
    }
}

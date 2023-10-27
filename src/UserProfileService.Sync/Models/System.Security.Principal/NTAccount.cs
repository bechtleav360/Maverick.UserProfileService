//
// System.Security.Policy.NTAccount.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Kenneth Bell
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace UserProfileService.Sync.Models.System.Security.Principal;

/// <summary>
///     The nt account class.
/// </summary>
[ComVisible(false)]
public sealed class NtAccount : IdentityReference
{
    /// <summary>
    ///     The value of the <see cref="NtAccount" />.
    /// </summary>
    public override string Value { get; }

    /// <summary>
    ///     Creates an instance of <see cref="NtAccount" />.
    /// </summary>
    /// <param name="name">The name of the <see cref="NtAccount" />.</param>
    public NtAccount(string name)
    {
        Value = name;
    }

    /// <summary>
    ///     Creates an instance of <see cref="NtAccount" />.
    /// </summary>
    /// <param name="domainName">The domain name of the <see cref="NtAccount" />.</param>
    /// <param name="accountName">The account name of the <see cref="NtAccount" />.</param>
    public NtAccount(string domainName, string accountName)
    {
        if (domainName == null)
        {
            Value = accountName;
        }
        else
        {
            Value = domainName + "\\" + accountName;
        }
    }

    /// <summary>
    ///     If two <see cref="NtAccount" />s are equal.
    /// </summary>
    /// <param name="o">The object that has to be compared.</param>
    /// <returns>True if equals, false otherwise.</returns>
    public override bool Equals(object o)
    {
        var nt = o as NtAccount;

        if (nt == null)
        {
            return false;
        }

        return nt.Value == Value;
    }

    /// <summary>
    ///     Gets the hash code from the value.
    /// </summary>
    /// <returns>Returns the has code.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    ///     Checks if it is a valid target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>If it is a valid target type that true, otherwise false.</returns>
    public override bool IsValidTargetType(Type targetType)
    {
        if (targetType == typeof(NtAccount))
        {
            return true;
        }

        if (targetType == typeof(SecurityIdentifier))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     The  to print out the object.
    /// </summary>
    /// <returns>A string that hold the hole object value.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    ///     Translate the target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>The <see cref="IdentityReference" />.</returns>
    public override IdentityReference Translate(Type targetType)
    {
        if (targetType == typeof(NtAccount))
        {
            return this; // ? copy
        }

        if (targetType == typeof(SecurityIdentifier))
        {
            WellKnownAccount acct = WellKnownAccount.LookupByName(Value);

            if (acct == null || acct.Sid == null)
            {
                throw new IdentityNotMappedException("Cannot map account name: " + Value);
            }

            return new SecurityIdentifier(acct.Sid);
        }

        throw new ArgumentException("Unknown type", nameof(targetType));
    }

    /// <summary>
    ///     Compare two <see cref="IdentityReference" />s with the "=="-operator.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>True when both values are equal, otherwise false.</returns>
    public static bool operator ==(NtAccount left, NtAccount right)
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
    ///     Compare two <see cref="IdentityReference" />s with the "!="-operator.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>True when both values are not equal, otherwise false.</returns>
    public static bool operator !=(NtAccount left, NtAccount right)
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

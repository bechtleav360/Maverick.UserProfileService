//
// System.Security.Policy.IdentityReference.cs
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

using System;
using System.Runtime.InteropServices;

namespace UserProfileService.Sync.Models.System.Security.Principal;

/// <summary>
///     The abstract class for the identity reference.
/// </summary>
[ComVisible(false)]
public abstract class IdentityReference
{
    /// <summary>
    ///     The abstract value of the identity reference.
    /// </summary>
    public abstract string Value { get; }

    // yep, this means it cannot be inherited outside corlib
    // not sure if this is "by design" reported as FDBK30180
    internal IdentityReference()
    {
    }

    /// <summary>
    ///     The abstract method to compare two object if the <see cref="IdentityReference" />-
    /// </summary>
    /// <param name="o">The object to compare.</param>
    /// <returns>True if equals, false otherwise.</returns>
    public abstract override bool Equals(object o);

    /// <summary>
    ///     The abstract method to get the hash.
    /// </summary>
    /// <returns>The hash of the object.</returns>
    public abstract override int GetHashCode();

    /// <summary>
    ///     The abstract method that validates if a valid target type is valid.
    /// </summary>
    /// <param name="targetType">The type that has to be checked.</param>
    /// <returns>True if the target type is valid, otherwise false.</returns>
    public abstract bool IsValidTargetType(Type targetType);

    /// <summary>
    ///     The abstract method to print out the object.
    /// </summary>
    /// <returns>A string that hold the hole object.</returns>
    public abstract override string ToString();

    /// <summary>
    ///     Translate the type to an <see cref="IdentityReference" />
    /// </summary>
    /// <param name="targetType">The type to translate.</param>
    /// <returns>The translated <see cref="IdentityReference" />.</returns>
    public abstract IdentityReference Translate(Type targetType);

    /// <summary>
    ///     Compare two <see cref="IdentityReference" />s with the "=="-operator.
    /// </summary>
    /// <param name="left">Left value to compare.</param>
    /// <param name="right">Right value to compare.</param>
    /// <returns>True when both values are equal, otherwise false.</returns>
    public static bool operator ==(IdentityReference left, IdentityReference right)
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
    public static bool operator !=(IdentityReference left, IdentityReference right)
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

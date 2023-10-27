//
// System.Security.Policy.IdentityReferenceCollection.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UserProfileService.Sync.Models.System.Security.Principal;

/// <summary>
///     The collection of the <see cref="IdentityReference" />.
/// </summary>
[ComVisible(false)]
public class IdentityReferenceCollection : ICollection<IdentityReference>
{
    private ArrayList _list;

    /// <summary>
    ///     The count of the collection.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    ///     If the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     The index of the collection.
    /// </summary>
    /// <param name="index">The index of the collection to get the instance of <see cref="IdentityReference" />.</param>
    /// <returns>An <see cref="IdentityReference" />.</returns>
    public IdentityReference this[int index]
    {
        get
        {
            if (index >= _list.Count)
            {
                return null;
            }

            return (IdentityReference)_list[index];
        }
        set => _list[index] = value;
    }

    /// <summary>
    ///     Creates an instance of <see cref="IdentityReferenceCollection" />
    /// </summary>
    public IdentityReferenceCollection()
    {
        _list = new ArrayList();
    }

    /// <summary>
    ///     Creates an instance of <see cref="IdentityReferenceCollection" />.
    /// </summary>
    /// <param name="capacity">The capacity of the collection.</param>
    public IdentityReferenceCollection(int capacity)
    {
        _list = new ArrayList(capacity);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Adds an element <see cref="IdentityReference" /> to the list.
    /// </summary>
    /// <param name="identity">The element that has to be add in the collection.</param>
    public void Add(IdentityReference identity)
    {
        _list.Add(identity);
    }

    /// <summary>
    ///     Clear the collection from all elements.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    ///     Check if a <see cref="IdentityReference" /> is in the collection.
    /// </summary>
    /// <param name="identity">The element that has to be checked, if it is part of the collection.</param>
    /// <returns>True when the element is part of the collection, false otherwise.</returns>
    public bool Contains(IdentityReference identity)
    {
        foreach (IdentityReference id in _list)
        {
            if (id.Equals(identity))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Copy an array of <see cref="IdentityReference" /> elements to the collection.
    /// </summary>
    /// <param name="array">The array that has to be copied.</param>
    /// <param name="offset">The offset of the array.</param>
    public void CopyTo(IdentityReference[] array, int offset)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Remove an <see cref="IdentityReference" /> from the collection.
    /// </summary>
    /// <param name="identity">The element that has to be deleted.</param>
    /// <returns>True if the element could be deleted, false otherwise.</returns>
    public bool Remove(IdentityReference identity)
    {
        foreach (IdentityReference id in _list)
        {
            if (id.Equals(identity))
            {
                _list.Remove(id);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Get the enumerator from the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator" /> for the collection.</returns>
    public IEnumerator<IdentityReference> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Translate the targetType.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    public IdentityReferenceCollection Translate(Type targetType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Translate the targetType.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <param name="forceSuccess">The force success.</param>
    /// <returns>An <see cref="IdentityReferenceCollection" />.</returns>
    public IdentityReferenceCollection Translate(Type targetType, bool forceSuccess)
    {
        throw new NotImplementedException();
    }
}

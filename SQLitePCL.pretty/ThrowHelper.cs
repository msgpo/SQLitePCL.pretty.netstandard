/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SQLitePCL.pretty
{
    /// <summary>
    /// Provides methods for throwing common exceptions.
    /// </summary>
    /// <remarks>
    /// When a method contains a throw statement, the JIT will not inline it.
    /// The common trick to solve this is to add a static "throw helper" method to do the dirty work for you.
    /// </remarks>
    public static class ThrowHelper
    {
        /// <summary>
        /// Throws a new <see cref="ArgumentException" />.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowArgumentException()
        {
            throw new ArgumentException();
        }

        /// <summary>
        /// Throws a new <see cref="ArgumentException" /> with the specified parameters.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        [DoesNotReturn]
        public static void ThrowArgumentException(string message)
        {
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Throws a new <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Throws a new <see cref="ArgumentOutOfRangeException" /> with the specified parameters.
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        /// <summary>
        /// Throws a new <see cref="NotSupportedException" /> with the specified parameters.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowNotSupportedException()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws a new <see cref="InvalidOperationException" /> with the specified parameters.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Throws a new <see cref="IOException" /> with the specified parameters.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        [DoesNotReturn]
        public static void ThrowIOException(string message, Exception innerException)
        {
            throw new IOException(message, innerException);
        }

        /// <summary>
        /// Throws a new <see cref="IOException" />.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowIOException()
        {
            throw new IOException();
        }

        /// <summary>
        /// Throws a new <see cref="KeyNotFoundException" />.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowKeyNotFoundException()
        {
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Throws a new <see cref="FormatException" />.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowFormatException()
        {
            throw new FormatException();
        }

        /// <summary>
        /// Throws a new <see cref="ObjectDisposedException" /> with the specified parameters.
        /// </summary>
        /// <param name="objectName">The name of the disposed object.</param>
        [DoesNotReturn]
        public static void ThrowObjectDisposedException(string objectName)
        {
            throw new ObjectDisposedException(objectName);
        }
    }
}

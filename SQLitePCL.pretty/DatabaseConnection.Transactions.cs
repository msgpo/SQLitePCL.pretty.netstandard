﻿// Copyright (c) 2015 David Bordoley
// Copyright (c) 2009-2015 Krueger Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Threading;

namespace SQLitePCL.pretty
{
    internal sealed class TransactionDatabaseConnection : DelegatingDatabaseConnection
    {
        internal TransactionDatabaseConnection(IDatabaseConnection db) : base(db)
        {
        }
    }

    /// <summary>
    /// Database transaction modes.
    /// </summary>
    /// <seealso href="https://www.sqlite.org/lang_transaction.html"/>
    public enum TransactionMode
    {
        /// <summary>
        /// No locks are acquired on the database until the database is first accessed.
        /// </summary>
        Deferred,

        /// <summary>
        /// RESERVED locks are acquired on all databases as soon as the BEGIN command is
        /// executed, without waiting for the database to be used.
        /// </summary>
        Immediate,

        /// <summary>
        /// EXCLUSIVE locks acquired on all databases. No other database connection except for read_uncommitted connections
        /// will be able to read the database and no other connection without exception will be able to write the database
        /// until the transaction is complete.
        /// </summary>
        Exclusive
    }

    public static partial class DatabaseConnection
    {
        private static readonly ThreadLocal<Random> _rand = new ThreadLocal<Random>(() => new Random());

        private static void BeginTransaction(this IDatabaseConnection This, TransactionMode mode)
        {
            var beginTransactionString = SQLBuilder.BeginTransactionWithMode(mode);
            This.Execute(beginTransactionString);
        }

        private static string SaveTransactionPoint(this IDatabaseConnection This)
        {
            var savePoint = $"S{_rand.Value.Next(short.MaxValue)}";
            This.Execute(SQLBuilder.SavePoint(savePoint));
            return savePoint;
        }

        private static void ReleaseTransaction(this IDatabaseConnection This, string savepoint) =>
            This.Execute(SQLBuilder.Release(savepoint));

        private static void CommitTransaction(this IDatabaseConnection This) =>
            This.Execute(SQLBuilder.CommitTransaction);

        private static void RollbackTransactionTo(this IDatabaseConnection This, string savepoint) =>
            This.Execute(SQLBuilder.RollbackTransactionTo(savepoint));

        private static void RollbackTransaction(this IDatabaseConnection This) =>
            This.Execute(SQLBuilder.RollbackTransaction);

        /// <summary>
        /// Runs the Action <paramref name="action"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// the provided TransactionMode and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <param name="This">The database connection.</param>
        /// <param name="action">The Action to run in a transaction.</param>
        /// <param name="mode">
        /// The transaction mode to use begin a new transaction. Ignored if the transaction is run
        /// within an existing transaction.
        /// </param>
        /// <exception cref="Exception">The exception that caused the transaction to be aborted and rolled back.</exception>
        public static void RunInTransaction(this IDatabaseConnection This, Action<IDatabaseConnection> action, TransactionMode mode)
        {
            Contract.Requires(action != null);

            This.RunInTransaction<object>(db =>
                {
                    action(db);
                    return null;
                }, mode);
        }

        /// <summary>
        /// Runs the Action <paramref name="action"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// BeginTransaction with TransactionMode.Deferred and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <param name="This">The database connection.</param>
        /// <param name="action">The Action to run in a transaction.</param>
        /// <exception cref="Exception">The exception that caused the transaction to be aborted and rolled back.</exception>
        public static void RunInTransaction(this IDatabaseConnection This, Action<IDatabaseConnection> action) =>
            This.RunInTransaction(action, TransactionMode.Deferred);

        /// <summary>
        /// Runs the function <paramref name="f"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// the provided TransactionMode and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns>The function result.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="f">
        /// The function to run in a transaction.
        /// </param>
        /// <param name="mode">
        /// The transaction mode to use begin a new transaction. Ignored if the transaction is run
        /// within an existing transaction.
        /// </param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <exception cref="Exception">The exception that caused the transaction to be aborted and rolled back.</exception>
        public static T RunInTransaction<T>(this IDatabaseConnection This, Func<IDatabaseConnection, T> f, TransactionMode mode)
        {
            Contract.Requires(This != null);
            Contract.Requires(f != null);

            using(var db = new TransactionDatabaseConnection(This))
            {
                string savePoint = null;

                if (This is TransactionDatabaseConnection)
                {
                    savePoint = db.SaveTransactionPoint();
                }
                else
                {
                    db.BeginTransaction(mode);
                }

                try
                {
                    var retval = f(db);

                    if (savePoint != null)
                    {
                        db.ReleaseTransaction(savePoint);
                    }
                    else
                    {
                        db.CommitTransaction();
                    }

                    return retval;
                }
                catch (Exception)
                {
                    if (savePoint != null)
                    {
                        db.RollbackTransactionTo(savePoint);
                    }
                    else
                    {
                        db.RollbackTransaction();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs the function <paramref name="f"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// BeginTransaction with TransactionMode.Deferred and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns>The function result.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="f">
        /// The function to run in a transaction.
        /// </param>
        /// <typeparam name="T">The result type.</typeparam>
        /// <exception cref="Exception">The exception that caused the transaction to be aborted and rolled back.</exception>
        public static T RunInTransaction<T>(this IDatabaseConnection This, Func<IDatabaseConnection, T> f) =>
            This.RunInTransaction(f, TransactionMode.Deferred);

        /// <summary>
        /// Runs the Action <paramref name="action"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// the provided TransactionMode and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns><c>true</c>, if the transaction was committed or released <c>false</c> if it was rolledback.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="action">The Action to run in a transaction.</param>
        /// <param name="mode">
        /// The transaction mode to use begin a new transaction. Ignored if the transaction is run
        /// within an existing transaction.
        /// </param>
        public static bool TryRunInTransaction(this IDatabaseConnection This, Action<IDatabaseConnection> action, TransactionMode mode)
        {
            Contract.Requires(action != null);

            return This.TryRunInTransaction<object>(db =>
                {
                    action(db);
                    return null;
                }, mode, out _);
        }

        /// <summary>
        /// Runs the Action <paramref name="action"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// BeginTransaction with TransactionMode.Deferred and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns><c>true</c>, if the transaction was committed or released <c>false</c> if it was rolledback.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="action">The Action to run in a transaction.</param>
        public static bool TryRunInTransaction(this IDatabaseConnection This, Action<IDatabaseConnection> action) =>
            This.TryRunInTransaction(action, TransactionMode.Deferred);

        /// <summary>
        /// Runs the function <paramref name="f"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// the provided TransactionMode and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns><c>true</c>, if the transaction was committed or released <c>false</c> if it was rolledback.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="f">F.</param>
        /// <param name="mode">
        /// The transaction mode to use begin a new transaction. Ignored if the transaction is run
        /// within an existing transaction.
        /// </param>
        /// <param name="result">The function result.</param>
        /// <typeparam name="T">The result type.</typeparam>
        public static bool TryRunInTransaction<T>(this IDatabaseConnection This, Func<IDatabaseConnection, T> f, TransactionMode mode, out T result)
        {
            Contract.Requires(This != null);
            Contract.Requires(f != null);

            try
            {
                result = This.RunInTransaction(f, mode);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Runs the function <paramref name="f"/> in a transaction and returns the function result.
        /// If the database is not currently in a transaction, a new transaction is created using
        /// BeginTransaction with TransactionMode.Deferred and committed. Otherwise the transaction is created within
        /// a savepoint block but not fully committed to the database until the enclosing transaction is committed.
        /// </summary>
        /// <returns><c>true</c>, if the transaction was committed or released <c>false</c> if it was rolledback.</returns>
        /// <param name="This">The database connection.</param>
        /// <param name="f">F.</param>
        /// <param name="result">The function result.</param>
        /// <typeparam name="T">The result type.</typeparam>
        public static bool TryRunInTransaction<T>(this IDatabaseConnection This, Func<IDatabaseConnection, T> f, out T result) =>
            This.TryRunInTransaction(f, TransactionMode.Deferred, out result);
    }
}

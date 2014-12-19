/*
   Copyright 2014 David Bordoley

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace SQLitePCL.pretty
{
    /// <summary>
    /// An interface for scheduling asynchronous access to an <see cref="IDatabaseConnection"/> in FIFO order.
    /// </summary>
    [ContractClass(typeof(IAsyncDatabaseConnectionContract))]
    public interface IAsyncDatabaseConnection : IDisposable
    {
        /// <summary>
        /// A hot <see cref="IObservable&lt;DatabaseTraceEventArgs&gt;"/> of this connection's SQLite trace events.
        /// </summary>
        /// <seealso cref="IDatabaseConnection.Trace"/>
        /// <seealso href="https://sqlite.org/c3ref/profile.html"/>
        IObservable<DatabaseTraceEventArgs> Trace { get; }

        /// <summary>
        /// A hot <see cref="IObservable&lt;DatabaseProfileEventArgs&gt;"/> of this connection's SQLite profile events.
        /// </summary>
        /// /// <seealso cref="IDatabaseConnection.Profile"/>
        /// <seealso href="https://sqlite.org/c3ref/profile.html"/>
        IObservable<DatabaseProfileEventArgs> Profile { get; }

        /// <summary>
        /// A hot <see cref="IObservable&lt;DatabaseUpdateEventArgs&gt;"/> of this connection's SQLite update events.
        /// </summary>
        /// /// <seealso cref="IDatabaseConnection.Update"/>
        /// <seealso href="https://sqlite.org/c3ref/update_hook.html"/>
        IObservable<DatabaseUpdateEventArgs> Update { get; }

        /// <summary>
        /// Shutdown the underlying operations queue used by the <see cref="IAsyncDatabaseConnection"/>
        /// and prevents the queuing of additional database access requests. Requests to access the database
        /// prior to the call to <see cref="DisposeAsync"/> are allowed to complete, after which underlying
        /// <see cref="IDatabaseConnection"/> is disposed.
        /// </summary>
        /// <returns>
        /// A task which completes once all previously queue operations are completed and the
        /// underlying<see cref="IDatabaseConnection"/> is disposed.</returns>
        Task DisposeAsync();

        /// <summary>
        /// Returns a cold IObservable which schedules the function f on the database operation queue each
        /// time it is is subscribed to. The published values are generated by enumerating the IEnumerable returned by f.
        /// </summary>
        /// <param name="f">
        /// A function that may synchronously use the provided IDatabaseConnection and returns
        /// an IEnumerable of produced values that are published to the subscribed IObserver.
        /// The returned IEnumerable may block. This allows the IEnumerable to provide the results of
        /// enumerating a SQLite prepared statement for instance.
        /// <note type="implement">
        /// The IDatabaseConnection may be used to prepare statements and open database blobs,
        /// but these must not be captured or otherwise externally referenced unless their use is
        /// scheduled on the connections's operation queue via future calls to <see cref="Use&lt;T&gt;"/>.
        /// </note>
        /// <typeparam name="T">The result type.</typeparam>
        /// </param>
        /// <returns>A cold observable of the values produced by the function f.</returns>
        IObservable<T> Use<T>(Func<IDatabaseConnection, CancellationToken, IEnumerable<T>> f);
    }

    /// <summary>
    /// An interface for scheduling asynchronous access to an <see cref="IStatement"/> in FIFO order.
    /// </summary>
    [ContractClass(typeof(IAsyncStatementContract))]
    public interface IAsyncStatement : IDisposable
    {
        /// <summary>
        /// Returns a cold IObservable which schedules the function f on the statement's database operation queue each
        /// time it is is subscribed to. The published values are generated by enumerating the IEnumerable returned by f.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="f">
        /// A function that may synchronously use the provided IStatement and returns
        /// an IEnumerable of produced values that are published to the subscribed IObserver.
        /// The returned IEnumerable may block. This allows the IEnumerable to provide the results of
        /// enumerating the prepared statement for instance.
        /// </param>
        /// <returns>A cold observable of the values produced by the function f.</returns>
        IObservable<T> Use<T>(Func<IStatement, CancellationToken, IEnumerable<T>> f);
    }
}

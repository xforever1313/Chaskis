//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace ChaskisCore
{
    /// <summary>
    /// Interface for adding scheduled events to the event queue.
    /// ALL Events, when their timers expire, have their actions enqueued to the
    /// Chaskis Event queue, where they will be later executed when the come
    /// off the queue.
    /// </summary>
    public interface IChaskisEventScheduler
    {
        // ---------------- Functions ----------------

        /// <summary>
        /// Schedules a recurring event to be run.
        /// </summary>
        /// <param name="interval">The interval to fire the event at.</param>
        /// <param name="action">
        /// The action to perform after the delay.
        /// Its parameter is an <see cref="IIrcWriter"/> so messages can be sent
        /// out to the channel.
        /// </param>
        /// <returns>The id of the event which can be used to stop it</returns>
        int ScheduleRecurringEvent( TimeSpan interval, Action<IIrcWriter> action );

        /// <summary>
        /// Schedules a single event
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="delay">How long to wait until we fire the first event.</param>
        /// <param name="action">
        /// The action to perform after the delay.
        /// Its parameter is an <see cref="IIrcWriter"/> so messages can be sent
        /// out to the channel.
        /// </param>
        /// <returns>The id of the event which can be used to stop it</returns>
        int ScheduleEvent( TimeSpan delay, Action<IIrcWriter> action );

        /// <summary>
        /// Stops the event from running.
        /// No-Op if the event is not running.
        /// </summary>
        /// <param name="id">ID of the event to stop.</param>
        void StopEvent( int id );
    }
}

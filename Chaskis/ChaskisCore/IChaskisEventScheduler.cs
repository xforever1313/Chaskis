//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis.Core
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
        /// <param name="startRightAway">
        /// If set to false, the event will not start executing until <see cref="StartEvent(int)"/> is called.
        /// </param>
        /// <returns>The id of the event which can be used to start or stop it</returns>
        int ScheduleRecurringEvent( TimeSpan interval, Action<IIrcWriter> action, bool startRightAway = true );

        /// <summary>
        /// Enables the given event.
        /// </summary>
        /// <exception cref="ArgumentException">If the event does not exist.</exception>
        /// <param name="id">ID of the event to stop.</param>
        void StartEvent( int id );

        /// <summary>
        /// Stops the event from running.
        /// </summary>
        /// <exception cref="ArgumentException">If the event does not exist.</exception>
        /// <param name="id">ID of the event to stop.</param>
        void StopEvent( int id );

        /// <summary>
        /// Stops and disposes the event, and removes its ID from the event scheduler.
        /// No-Op if the event doesn't exist.
        /// </summary>
        void DisposeEvent( int id );
    }
}

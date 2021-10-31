//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Threading;
using Chaskis.Core;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public sealed class ChaskisChannelTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Checks to see if we rapid-fire tasks, they happen in the correct order.
        /// </summary>
        [Test]
        public void RapidSynchroniousTest()
        {
            int nextId = 0;
            object intLock = new object();
            int GetNextId()
            {
                lock( intLock )
                {
                    return nextId++;
                }
            }

            const int count = 10000;
            List<TaskObject> taskObjects = new List<TaskObject>( count );

            using( ManualResetEventSlim waitEvent = new ManualResetEventSlim( false ) )
            {
                using( ActionChannelImpl uut = new ActionChannelImpl() )
                {
                    uut.Start();

                    for( int i = 0; i < count; ++i )
                    {
                        TaskObject obj = new TaskObject
                        {
                            ExpectedId = i
                        };
                        taskObjects.Add( obj );

                        uut.BeginInvoke(
                            () =>
                            {
                                int nextId = GetNextId();
                                obj.ActualId = nextId;
                            }
                        );
                    }

                    uut.BeginInvoke( () => waitEvent.Set() );

                    Assert.IsTrue( waitEvent.Wait( TimeSpan.FromMinutes( 2 ) ) );
                }
            }

            foreach( TaskObject obj in taskObjects )
            {
                Assert.AreEqual( obj.ExpectedId, obj.ActualId );
            }
        }

        // Ensures that if a bunch of fast tasks and some slow tasks happen,
        // everything still happens in synchroniously.
        [Test]
        public void DelayedSynchroniousTest()
        {
            int nextId = 0;
            object intLock = new object();
            int GetNextId()
            {
                lock( intLock )
                {
                    return nextId++;
                }
            }

            const int count = 100;
            List<TaskObject> taskObjects = new List<TaskObject>( count );

            using( ManualResetEventSlim waitEvent = new ManualResetEventSlim( false ) )
            {
                using( ActionChannelImpl uut = new ActionChannelImpl() )
                {
                    uut.Start();

                    for( int i = 0; i < count; ++i )
                    {
                        TaskObject obj = new TaskObject
                        {
                            ExpectedId = i
                        };
                        taskObjects.Add( obj );

                        uut.BeginInvoke(
                            () =>
                            {
                                if( obj.ExpectedId % 20 == 0 )
                                {
                                    Thread.Sleep( TimeSpan.FromSeconds( 1 ) );
                                }
                                int nextId = GetNextId();
                                obj.ActualId = nextId;
                            }
                        );
                    }

                    uut.BeginInvoke( () => waitEvent.Set() );

                    Assert.IsTrue( waitEvent.Wait( TimeSpan.FromMinutes( 2 ) ) );
                }
            }

            foreach( TaskObject obj in taskObjects )
            {
                Assert.AreEqual( obj.ExpectedId, obj.ActualId );
            }
        }

        // ---------------- Test Helpers ----------------

        private class TaskObject
        {
            public int ExpectedId { get; set; }
            public int ActualId { get; set; }
        }

        private class ActionChannelImpl : ChaskisActionChannel
        {
            // ---------------- Constructor ----------------

            public ActionChannelImpl() :
                base( nameof( ActionChannelImpl ) )
            {
            }

            // ---------------- Functions ----------------

            public new void Start()
            {
                base.Start();
            }
        }
    }
}

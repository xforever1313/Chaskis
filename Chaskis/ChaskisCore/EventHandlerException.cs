//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Diagnostics;
using System.Text;

namespace Chaskis.Core
{
    public class EventHandlerException : Exception
    {
        // ---------------- Constructor ----------------

        public EventHandlerException( string context, Exception innerException ) :
            base( $"A(n) {context} threw a(n) {innerException.GetType().Name}: {innerException.Message} ", innerException )
        {
            this.ServerMessage = string.Empty;
            this.EventHandlerPlugin = context;
            this.EventHandlerCreationStackTrace = new StackTrace( true );
        }

        public EventHandlerException( string plugin, IIrcHandler handler, string serverMessage, Exception innerException ) :
            base( $"An event handler in {plugin} threw a(n) {innerException.GetType().Name} on this message from the server '{serverMessage}': {innerException.Message}", innerException )
        {
            this.EventHandlerPlugin = plugin;
            this.EventHandlerCreationStackTrace = handler.CreationStack;
            this.ServerMessage = serverMessage;
        }

        // ---------------- Properties ----------------

        public string ServerMessage { get; private set; }

        public string EventHandlerPlugin { get; private set; }

        public StackTrace EventHandlerCreationStackTrace { get; private set; }

        // ---------------- Functions ----------------

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( this.Message );
            builder.AppendLine();
            builder.AppendLine( "Inner Exception was thrown at:" );
            builder.AppendLine( this.InnerException.StackTrace );
            builder.AppendLine();
            builder.AppendLine( "Event handler that threw this exception was created at: " );
            builder.AppendLine( this.EventHandlerCreationStackTrace.ToString() );

            return builder.ToString();
        }
    }
}

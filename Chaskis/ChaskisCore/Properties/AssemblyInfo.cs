//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Reflection;
using ChaskisCore;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle( "ChaskisCore" )]
[assembly: AssemblyDescription(
@"Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux (with the use of Mono).  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages."
)] // End Description
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Seth Hendrick" )]
[assembly: AssemblyProduct( "" )]
[assembly: AssemblyCopyright( IrcBot.CopyRight )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion( IrcBot.VersionString )]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
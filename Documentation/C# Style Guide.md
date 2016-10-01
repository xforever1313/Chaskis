C# Style Guide
========

Spacing
====

  * Replace tabs with four spaces.
  * When calling a function or declaring a function with arguments, add a space between the first argument and the '(' and after the second argument and ')'

```c#

/// <summary>
/// Function with no arguments has no space.
/// No space between function name and '('
/// </summary>
public void MyFunction()
{

}

/// <summary>
/// Function with arguments
/// </summary>
/// <param name="arg1">First argument.  Space between it and '('</param>
/// <param name="arg2">Second argument.  Space between it and ')'</param>
public void MyFunctionWithArg( int arg1, int arg2 )
{
    MyFunction(); // When calling a function with no arguments, do not add spaces.
    SomeOtherFunction( arg1, arg2 ); // When calling a function with arguments, add spacing.
}
```

  * When calling or declaring a function with many arguments or long arguments, put each argument on a separate line.

```c#
public void MyVeryLongFunctionName(
    longArgumentName1,   // Each argument goes on a new line.
    longArgumentName2,
    longArgumentName3,
    longArgumentName4
)
{
    ShortFunction( longArgument1 );  // Short enough, no need to put it on new line.
    
    // This is long, it should have an argument on each line.
    OtherLongFunction( longArgumentName1, longArgumentName2, longArgumentName3, longArgumentName4 );

    OtherLongFunction( // '(' does not go on new line.
        longArgumentName1,   // Each argument goes on a new line.
        longArgumentName2,
        longArgumentName3,
        longArgumentName4
    ); // ')' goes on new line and tabbed in equal to the function call.
}

```

  * Array Indexes do not have spaces

```c#
    someArray[ i ] = 3; // Prefer not to do this.
    someArray[i] = 3; // Good.
```

Using Statements
====

System using statements should come first, followed by non-system using statements in alphabetical order.

Visual Studio has an option for this.  Tools -> Options -> Text Editor -> C# -> Advanced -> Place 'System' directives first when sorting usings.

```c#
// Using statements should be in the following order.
// System comes first.
using System;
using System.Xml;
using MyThing;        // User-defined includes comes next.
using SomeOtherThing; // In alphabetical order.
```

Interfaces
====
  * Interfaces start with a capital I
  * Properties get defined first, followed by functions.

```c#
namespace MyNamespace // Namespace is camel case starting with a capital letter.
{
    /// <summary>
    /// Interfaces should start with a capital I.
    /// </summary>
    public interface IMyInterface
    {
        // -------- Properties --------

        /// <summary>
        /// A description of what the property is.
        /// </summary>
        int MyProperty{ get; }

        // -------- Functions ---------

        /// <summary>
        /// A description of what the function does.
        /// </summary>
        void MyFunction1();

        /// <summary>
        /// A description of what the function does.
        /// </summary>
        /// <param name="someArg">A description of what the argument does.</param>
        /// <returns>What the function returns.</returns>
        int MyOtherFunction( int someArg );

        /// <summary>
        /// Do not do this.  Now everything in the interface has 1 as the default argument.
        /// The implementation should determine what the default value is.
        /// <summary>
        /// <param name="someArg">Do not do this</param>
        void MyBadFunctionWithDefaultArg( int someArg = 1 );

        /// <summary>
        /// Prefer this.  The implementation will determine the default value via
        /// someArg ?? ImplmentationDefaultValue
        /// </summary>
        /// <param name="someArg">Prefer this to default values</param>
        void MyCorrectFunctionWithDefaultArg( int? someArg = null );
    }

}
```

Classes/Structs
====

  * Classes should start with a captial letter.
  * Fields/constants go first
  * Constructors go next
  * Properties go next
  * Functions come last.
  * Include regional comments between each section.

```c#

namespace MyNamespace
{
    /// <summary>
    /// My super cool class.
    /// </summary>
    public class MyClass : MyBaseClass
    {
        // -------- Fields --------

        /// <summary>
        /// A public const string.
        /// </summary>
        public const string MyString = "Public const fields start with capital letters";

        /// <summary>
        /// A private const string
        /// </summary>
        private const string myInnerString = "Private const fields start with lower-case letters";

        /// <summary>
        /// A mutable list that this class can modify.
        /// </summary>
        private List<int> mutableList;

        // -------- Constructor --------

        /// <summary>
        /// Some static method that creates an instance of
        /// the class for us goes in this section
        /// </summary>
        /// <returns>An instance of MyClass</returns>
        public static MyClass SomeFactoryMethod()
        {
            MyClass myClass = new MyClass();
            // Init
            return myClass;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MyClass() :
            base( baseArg1, baseArg2 )  // Base class goes on next line, indented.
        {
            // Init members.

            this.mutableList = new List<int>(); // Always use this in classes when specifying fields or properties.
            this.ReadOnlyList = this.mutableList.AsReadOnly();
        }

        // -------- Properties --------

        /// <summary>
        /// Read-only list of this class's mutable list.
        /// A list property should generally always be read-only.
        /// </summary>
        public IList<int> ReadOnlyList { get; private set; } // Space between name and {.

        // -------- Functions --------

        /// <summary>
        /// Adds an int to this class's list.
        /// </summary>
        /// <param="x">The int to add</param>
        public void Add( int x ) // No space between function name and '('.
        {
            this.mutableList.Add( x );
        }
    }
}
```

  * Use camel case for everything.
  * Private fields/variables start with a lower case.
  * Public fields/variables start with a upper case.
  * All functions and properties start with an upper case letter.

Regions
====

  * Do not use regions.  Separate areas of code with dashes.  Divide the number of dashes by 2 for each sub-region.
  * There are times when a region will allowed to be used, but its very sparse.

```c#

// -------- My Region --------

// ---- My sub-region ----

// -- My sub-sub region

// -------- Properties --------

// -------- Functions --------

// ---- UI Functions ----

// -- Button 1 Functions --

// -- Button 2 Functions --

// ---- Callbacks ----
```

var
====

  * Avoid var.  It should only be used in exceptional cases such as dealing with sqlite.

if statements
====

  * Do not use '!' in boolean logic.  Use == false instead:

```c#

// Bad
if( !something )
{

}

// Good
if( something == false )
{

}
```

  * Indent if statements as such:

```c#
if( something ) // No space between if and (.
{
    // ALWAYS USE CURLY BRACES!  Even if 1 line.
}
else if( somethingElse )
{
    // Else if goes on new line under if's closing curly brace.
}
else
{
    // Else goes under if or else if's closing curly brace.
}
```

Switch Statements
====

  * Indent switch statements as such:
```c#

switch( x )
{
    // Indent cases.
    case 1:
        // Do stuff indented.
        break; // Break indented.

    case 2:
        // Do more stuff indented.
        break; // Break indented.

    default:
        // Do default stuff indented.
        break;
}
```

While Loops
====
  * Do no use '!', use == false
  * Always use curly braces, even when there's only one line.

```c#

while( something == false )
{
    // Loop logic
}

// Do-while loops have do and while on separate lines.
do
{
    // Loop logic
}
while( something == false );
```

Scoping
====

  * Reduce scope as much as possible.

```c#
public void SomeFunction()
{
    int aNumber;  // Scope can be reduced to inside the while loop.
    while( this.Something )
    {
        aNumber = this.SomethingElse.GetNumberOfThings + 1;
        Console.WriteLine( aNumber );
    }
    
    while( this.Something )
    {
        // aNumber is now scoped correctly.
        int aNumber = this.SomethingElse.GetNumberOfThings + 1;
        Console.WriteLine( aNumber );
    }
}
```
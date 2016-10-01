using System;
using Chaskis.Plugins.MathBot;
using NUnit.Framework;

namespace Tests.Plugins.MathBot
{
    [TestFixture]
    public class MathBotCalculatorTest
    {
        // -------- Tests --------

        /// <summary>
        /// Verifies the calculator is working.
        /// </summary>
        [Test]
        public void CalculatorTest()
        {
            // Simple arithmetic
            TestForCorrectAnswer( "1 + 1", "2" );
            TestForCorrectAnswer( "1+1", "2" );
            TestForCorrectAnswer( "2 * 3 + 3", "9" );
            TestForCorrectAnswer( "2 * (3 + 3)", "12" );
            TestForCorrectAnswer( "2.5 + 2.5", "5.0" );

            // Boolean logic
            TestForCorrectAnswer( "True and True", "True" );
            TestForCorrectAnswer( "False and True", "False" );
            TestForCorrectAnswer( "False or True", "True" );
            TestForCorrectAnswer( "not false and true", "True" );

            // Boolean Logic and arithmetic
            TestForCorrectAnswer( "(1 + 1) > (1 * 1)", "True" );
            TestForCorrectAnswer( "(1) > (1 * 1)", "False" );
            TestForCorrectAnswer( "(1) >= (1 * 1)", "True" );

            TestForCorrectAnswer( "(1 + 1) < (1 * 1)", "False" );
            TestForCorrectAnswer( "(1) < (1 * 1)", "False" );
            TestForCorrectAnswer( "(1) <= (1 * 1)", "True" );

            TestForCorrectAnswer( "(1 + 1) > 0 AND -1 < 3", "True" );
            TestForCorrectAnswer( "(1 + 1) > 0 AND NOT -1 < 3", "False" );
        }

        /// <summary>
        /// Ensures various bad inputs produce syntax errors (Exceptions).
        /// </summary>
        [Test]
        public void SyntaxErrorTest()
        {
            TestForSyntaxError( "Derp" );
            TestForSyntaxError( "1//3" );
            TestForSyntaxError( "Hello world!" );
            TestForSyntaxError( "(3 + 2" );
            TestForSyntaxError( "derp + 3 + 2" );
            TestForSyntaxError( "derp 3 + 2" );
            TestForSyntaxError( "AND OR" );
            TestForSyntaxError( "2 =! 3" );
            TestForSyntaxError( "(1 + 1) > 0 AND d NOT -1 < 3" );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Tests that the given expression results in the expected answer.
        /// </summary>
        /// <param name="expression">The expression to feed our calculator.</param>
        /// <param name="expectedAnswer">The expected answer we expect.</param>
        private void TestForCorrectAnswer( string expression, string expectedAnswer )
        {
            string answer = MathBotCalculator.Calculate( expression );
            Assert.AreEqual( expectedAnswer, answer );
        }

        /// <summary>
        /// Ensures that bad input results in an exception.
        /// </summary>
        /// <param name="expression">The bad expression to test.</param>
        private void TestForSyntaxError( string expression )
        {
            // Need to use this instead of Assert.Throws since Throws expects a specific type,
            // we don't care about the type of exception that is thrown.
            try
            {
                MathBotCalculator.Calculate( expression );
                Assert.Fail( "Did not get expected exception for expression " + expression );
            }
            // We don't care what exception occurrs, as long as one happens.
            catch( Exception )
            {
                Assert.Pass();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Codaris.Common
{
    /// <summary>
    /// Assertion class to capture runtime errors
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Raises an exception if the parameter is null.  Returns the parameter unchanged.
        /// </summary>
        /// <typeparam name="T">Reference type or nullable</typeparam>
        /// <param name="value">The value to test</param>
        /// <returns>The value unchanged</returns>
        /// <param name="message">Assertion failure message</param>
        [return: NotNull]
        public static T IsNotNull<T>([NotNull] T value, string message = "Assertion failed: '{0}' is null.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (value == null) throw new AssertionFailedException(string.Format(message, expression ?? "value"));
            return value;
        }

        /// <summary>
        /// Raises an exception if the parameter is not null.  Returns the parameter unchanged.
        /// </summary>
        /// <typeparam name="T">Reference type or nullable</typeparam>
        /// <param name="value">The value to test</param>
        /// <returns>The value unchanged</returns>
        /// <param name="message">Assertion failure message</param>
        public static T IsNull<T>([MaybeNull] T value, string message = "Assertion failed: '{0}' is not null.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (value != null) throw new AssertionFailedException(string.Format(message, expression ?? "value"));
            return value;
        }

        /// <summary>
        /// Raises an exception if the string is null or empty.  Returns the parameter unchanged
        /// </summary>
        /// <param name="value">String reference to test</param>
        /// <returns>String value</returns>
        /// <param name="message">Assertion failure message</param>
        [return: NotNull]
        public static string IsNotNullOrEmpty(string? value, string message = "Assertion failed: '{0}' is null or empty.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (string.IsNullOrEmpty(value)) throw new AssertionFailedException(string.Format(message, expression ?? "value"));
            return value;
        }

        /// <summary>
        /// Raises an exception if the string is null or empty.  Returns the parameter unchanged
        /// </summary>
        /// <param name="value">String reference to test</param>
        /// <returns>String value</returns>
        /// <param name="message">Assertion failure message</param>
        public static string? IsNullOrEmpty(string? value, string message = "Assertion failed: '{0}' is not null or empty.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (!string.IsNullOrEmpty(value)) throw new AssertionFailedException(string.Format(message, expression ?? "value"));
            return value;
        }

        /// <summary>
        /// Raises an exception if the string is null or whitespace.  Returns the parameter unchanged
        /// </summary>
        /// <param name="value">String reference to test</param>
        /// <returns>String value</returns>
        /// <param name="message">Assertion failure message</param>
        [return: NotNull]
        public static string IsNotNullOrWhiteSpace(string? value, string message = "Assertion failed: '{0}' is null or whitespace.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new AssertionFailedException(string.Format(message, expression ?? "value"));
            return value;
        }

        /// <summary>
        /// Raises an exception if the string value doesn't contain the specified other value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="otherValue">The other value.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="AssertionFailedException"></exception>
        public static string Contains(string value, string otherValue, string message = "Assertion failed: '{0}' does not contain '{1}'.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (!value.Contains(otherValue)) throw new AssertionFailedException(string.Format(message, expression, otherValue));
            return value;
        }

        /// <summary>
        /// Raises an exception if the boolean value is false.
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="message">Assertion failure message</param>
        public static void IsTrue(bool value, string message = "Assertion failed: value is false.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (!value) throw new AssertionFailedException(string.Format(message, expression));
        }

        /// <summary>
        /// Raises an exception if the boolean value is true.
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <param name="message">Assertion failure message</param>
        public static void IsFalse(bool value, string message = "Assertion failed: '{0}' is true.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (value) throw new AssertionFailedException(string.Format(message, expression));
        }

        /// <summary>
        /// Raises an exception if called.
        /// </summary>
        /// <param name="message">Assertion failure message</param>
        /// <exception cref="AssertionFailedException">Always triggered</exception>
        public static void IsUnreachable(string message = "Unreachable code executed.")
        {
            throw new AssertionFailedException(message);
        }

        /// <summary>
        /// Raises an assertion failed exception.
        /// </summary>
        /// <param name="message">Optional error message</param>
        public static void Fail(string message = "Assertion failed")
        {
            throw new AssertionFailedException(message);
        }

        /// <summary>
        /// Determines whether the value is of the specififed type
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="AssertionFailedException"></exception>
        public static T IsTypeOf<T>(object value, string message = "Assertion failed: '{0}' is not of type '{1}'.", [CallerArgumentExpression(nameof(value))] string? expression = null)
        {
            if (value is T result) return result;
            throw new AssertionFailedException(string.Format(message, expression, typeof(T).Name));
        }

        /// <summary>
        /// Determines whether enumerable is not empty.
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="message">The message.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="Davis.AssertionFailedException"></exception>
        public static IEnumerable<T> IsNotEmpty<T>(IEnumerable<T> items, string message = "Assertion failed: collection '{0}' is empty.", [CallerArgumentExpression(nameof(items))] string? expression = null)
        {
            if (items.Any()) return items;
            throw new AssertionFailedException(string.Format(message, expression));
        }
    }
}

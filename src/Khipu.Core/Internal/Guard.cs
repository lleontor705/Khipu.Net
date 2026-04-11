namespace Khipu.Core.Internal;

using System.Runtime.CompilerServices;

/// <summary>
/// Argument validation polyfills for multi-targeting net6.0+.
/// Uses native APIs when available, falls back to manual checks on older TFMs.
/// </summary>
internal static class Guard
{
    internal static void NotNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null) throw new ArgumentNullException(paramName);
#endif
    }

    internal static void NotNullOrWhiteSpace(string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
#else
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
#endif
    }
}

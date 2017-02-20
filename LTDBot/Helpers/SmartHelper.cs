using System;
using System.Runtime.CompilerServices;

namespace LTDBot.Helpers
{
    public static class SmartHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrCreate<T>(ref T field) where T : class, new()
        {
            var result = field;
            if (ReferenceEquals(result, null))
                field = result = new T();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrProvide<T>(ref T field, Func<T> provider) where T : class
        {
            ThrowIfArgIsNull(provider, nameof(provider));

            var result = field;
            if (ReferenceEquals(result, null))
                field = result = provider();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgIsNull<T>(T arg, string argumentName) where T : class
        {
            if (ReferenceEquals(arg, null)) throw new ArgumentNullException(nameof(argumentName));
        }
    }
}
using System;

namespace Wikimedia
{
    static class Check
    {
        internal static T NotNull<T>(T value, string paramName) where T : class
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(paramName);
            return value;
        }
    }
}

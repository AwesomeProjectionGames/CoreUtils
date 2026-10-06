using System;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Computes a deterministic 32-bit FNV-1a hash of the type's FullName (or Name if FullName is null).
        /// Consistent across application runs and platforms, unlike Type.GetHashCode() or string.GetHashCode().
        /// </summary>
        public static uint GetDeterministicTypeHash(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            string name = type.FullName ?? type.Name;
            return name.GetDeterministicHashCode();
        }
    }
}

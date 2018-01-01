﻿using System;

namespace MetroOverhaul.NEXT.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// No type-safety check.
        /// </summary>
        public static bool HasFlag(this Enum e1, Enum e2)
        {
            ulong e = Convert.ToUInt64(e1);
            ulong f = Convert.ToUInt64(e2);

            return (e & f) == f;
        }
    }
}

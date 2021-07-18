using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsPulumiPoc
{
    public static class Utils
    {
        /// <summary>
        /// Determines if list has > 0 non-empty items 
        /// </summary>
        public static bool HasEmptyItems(this List<string> l) => l.Count == 0 || l.Any(x => string.IsNullOrEmpty(x));
    }
}

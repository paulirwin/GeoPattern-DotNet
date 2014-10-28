using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPattern
{
    internal static class ExtendHelper
    {
        public static IDictionary<string, object> Extend(params IDictionary<string, object>[] maps)
        {
            if (maps == null || maps.Length == 0)
                return new Dictionary<string, object>();

            var dict = new Dictionary<string, object>(maps[0]);

            for (int i = 1; i < maps.Length; i++)
            {
                foreach (var kvp in maps[i])
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }

        public static IDictionary<string, string> Extend(params IDictionary<string, string>[] maps)
        {
            if (maps == null || maps.Length == 0)
                return new Dictionary<string, string>();

            var dict = new Dictionary<string, string>(maps[0]);

            for (int i = 1; i < maps.Length; i++)
            {
                foreach (var kvp in maps[i])
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }
    }
}

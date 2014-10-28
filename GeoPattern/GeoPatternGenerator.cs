using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoPattern
{
    public static class GeoPatternGenerator
    {
        public static string Generate(IDictionary<string, string> args)
        {
            return Pattern.New(args).ToSvgString();
        }

        public static string Base64String(IDictionary<string, string> args)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Generate(args)));
        }

        public static string UriImage(IDictionary<string, string> args)
        {
            return string.Format("url(data:image/svg+xml;base64,{0});", Base64String(args));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeoPattern
{
    public class Color
    {
        private static readonly Regex _shorthandRegex = new Regex("^#?([a-f\\d])([a-f\\d])([a-f\\d])$", RegexOptions.IgnoreCase);
        private static readonly Regex _hexRegex = new Regex("^#?([a-f\\d]{2})([a-f\\d]{2})([a-f\\d]{2})$", RegexOptions.IgnoreCase);

        public static RgbColor HexToRGB(string hex)
        {
            hex = _shorthandRegex.Replace(hex, m =>
            {
                string r = m.Groups[1].Value;
                string g = m.Groups[2].Value;
                string b = m.Groups[3].Value;

                return r + r + g + g + b + b;
            });

            var match = _hexRegex.Match(hex);

            if (!match.Success)
                return null;

            string red = match.Groups[1].Value;
            string green = match.Groups[2].Value;
            string blue = match.Groups[3].Value;

            byte rb = byte.Parse(red, NumberStyles.HexNumber);
            byte gb = byte.Parse(green, NumberStyles.HexNumber);
            byte bb = byte.Parse(blue, NumberStyles.HexNumber);

            return new RgbColor(rb, gb, bb);
        }

        public static HslColor RgbToHsl(RgbColor rgb)
        {
            byte r = rgb.Red;
            byte g = rgb.Green;
            byte b = rgb.Blue;

            r /= 255; g /= 255; b /= 255;
            
            byte max = Math.Max(r, Math.Max(g, b));
            byte min = Math.Min(r, Math.Min(g, b));

            float h = 0, s, l;
            l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                if (max == r) { h = (g - b) / d + (g < b ? 6 : 0); }
                else if (max == g) { h = (b - r) / d + 2; }
                else if (max == b) { h = (r - g) / d + 4; }

                h /= 6;
            }

            return new HslColor(h, s, l);
        }

        public static RgbColor HslToRgb(HslColor hsl)
        {
            Func<float, float, float, float> hue2rgb = (p, q, t) =>
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1 / 6) return p + (q - p) * 6 * t;
                if (t < 1 / 2) return q;
                if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                return p;
            };

            float h = hsl.Hue, s = hsl.Sat, l = hsl.Lum;
            float r = 0.0f, g = 0.0f, b = 0.0f;

            if (s == 0.0f)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = hue2rgb(p, q, h + 1 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3);
            }

            return new RgbColor((byte)Math.Round(r * 255), (byte)Math.Round(g * 255), (byte)Math.Round(b * 255));
        }
    }

    public class RgbColor
    {
        public RgbColor(byte r, byte g, byte b)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
        }

        public RgbColor()
        {
        }

        public byte Red { get; private set; }

        public byte Green { get; private set; }

        public byte Blue { get; private set; }

        public override string ToString()
        {
            return "rgb(" + string.Join(",", new[] { this.Red, this.Green, this.Blue }) + ")";
        }
    }

    public class HslColor
    {
        public HslColor(float h, float s, float l)
        {
            this.Hue = h;
            this.Sat = s;
            this.Lum = l;
        }

        public HslColor()
        {
        }

        public float Hue { get; set; }

        public float Sat { get; set; }

        public float Lum { get; set; }
    }
}

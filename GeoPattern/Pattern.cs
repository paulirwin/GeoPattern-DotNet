using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPattern
{
    public class Pattern
    {
        private static readonly IDictionary<string, string> Defaults = new Dictionary<string, string>
        {
            { "baseColor", "#933c3c"}
        };

        private static readonly IDictionary<Patterns, Action<Pattern>> Generators = new Dictionary<Patterns, Action<Pattern>>
        {
            { Patterns.Hexagons, p => p.GeoHexagons() }
        };

        public Pattern(string str, IDictionary<string, string> options)
        {
            FillColorDark = "#222";
            FillColorLight = "#ddd";
            StrokeColor = "#000";
            StrokeOpacity = 0.02f;
            OpacityMin = 0.02f;
            OpacityMax = 0.15f;

            this.Options = Extend(Defaults, options);
            this.Hash = options.ContainsKey("hash") ? options["hash"].ToString() : Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "");
            this.Svg = new SVG();

            this.GenerateBackground();
            this.GeneratePattern();
        }

        public IDictionary<string, string> Options { get; set; }

        public string Hash { get; set; }

        public SVG Svg { get; set; }

        public string FillColorDark { get; set; }

        public string FillColorLight { get; set; }

        public string StrokeColor { get; set; }

        public float StrokeOpacity { get; set; }

        public float OpacityMin { get; set; }

        public float OpacityMax { get; set; }

        public static int HexVal(string hash, int index, int len = 1)
        {
            return int.Parse(hash.Substring(index, len), NumberStyles.HexNumber);
        }

        public static float Map(float value, float vMin, float vMax, float dMin, float dMax)
        {
            var vValue = value;
            var vRange = vMax - vMin;
            var dRange = dMax - dMin;

            return (vValue - vMin) * dRange / vRange + dMin;
        }

        public string FillColor(int val)
        {
            return (val % 2 == 0) ? FillColorLight : FillColorDark;
        }

        public float FillOpacity(float val)
        {
            return Map(val, 0, 15, OpacityMin, OpacityMax);
        }

        public string ToSvgString()
        {
            return this.Svg.ToString();
        }

        public override string ToString()
        {
            return this.ToSvgString();
        }

        public void GenerateBackground()
        {
            RgbColor rgb;

            if (this.Options.ContainsKey("color"))
            {
                rgb = Color.HexToRGB(this.Options["color"]);
            }
            else
            {
                var hueOffset = Map(HexVal(this.Hash, 14, 3), 0, 4095, 0, 359);
                var satOffset = HexVal(this.Hash, 17);
                var baseColor = Color.RgbToHsl(Color.HexToRGB(this.Options["baseColor"]));

                baseColor.Hue = (((baseColor.Hue * 360 - hueOffset) + 360) % 360) / 360;

                if (satOffset % 2 == 0)
                {
                    baseColor.Sat = Math.Min(1, ((baseColor.Sat * 100) + satOffset) / 100);
                }
                else
                {
                    baseColor.Sat = Math.Max(0, ((baseColor.Sat * 100) - satOffset) / 100);
                }

                rgb = Color.HslToRgb(baseColor);
            }

            this.Svg.Rect(0, 0, "100%", "100%", new Dictionary<string, object>
            {
                { "fill", rgb.ToString() }
            });
        }

        public void GeneratePattern()
        {
            Patterns pattern;

            if (this.Options.ContainsKey("generator"))
            {
                var generator = this.Options["generator"];

                pattern = (Patterns)Enum.Parse(typeof(Patterns), generator, true);
            }
            else
            {
                pattern = (Patterns)(HexVal(this.Hash, 20) + 1);
            }

            Generators[pattern](this);
        }

        private static string BuildHexagonShape(int sideLength)
        {
            var c = sideLength;
            var a = c / 2;
            var b = Math.Sin(60 * Math.PI / 180) * c;
            return string.Join(",", new[] {
		        0, b,
		        a, 0,
		        a + c, 0,
		        2 * c, b,
		        a + c, 2 * b,
		        a, 2 * b,
		        0, b
            });
        }

        public void GeoHexagons()
        {
            var scale = HexVal(this.Hash, 0);
            int sideLength = (int)Map(scale, 0, 15, 8, 60);
            float hexHeight = sideLength * (float)Math.Sqrt(3);
            float hexWidth = sideLength * 2;
            var hex = BuildHexagonShape((int)sideLength);
            float dy, opacity, x, y;
            int i, val;
            string fill;
            IDictionary<string, object> styles;

            this.Svg.SetWidth(hexWidth * 3 + sideLength * 3);
            this.Svg.SetHeight((int)(hexHeight * 6));

            i = 0;
            for (y = 0; y < 6; y++)
            {
                for (x = 0; x < 6; x++)
                {
                    val = HexVal(this.Hash, i);
                    dy = x % 2 == 0 ? y * hexHeight : y * hexHeight + hexHeight / 2;
                    opacity = FillOpacity(val);
                    fill = FillColor(val);

                    styles = new Dictionary<string, object> {
				        { "fill", fill },
				        { "fill-opacity", opacity},
				        { "stroke", StrokeColor},
				        { "stroke-opacity", StrokeOpacity }
			        };

                    this.Svg.Polyline(hex, styles).Transform(new Dictionary<string, IEnumerable<object>> {
				        { "translate", new object[] {
					        x * sideLength * 1.5 - hexWidth / 2,
					        dy - hexHeight / 2
				        }}
			        });

                    // Add an extra one at top-right, for tiling.
                    if (x == 0)
                    {
                        this.Svg.Polyline(hex, styles).Transform(new Dictionary<string, IEnumerable<object>> {
					        { "translate", new object[] {
						        6 * sideLength * 1.5 - hexWidth / 2,
						        dy - hexHeight / 2
					        }}
				        });
                    }

                    // Add an extra row at the end that matches the first row, for tiling.
                    if (y == 0)
                    {
                        dy = x % 2 == 0 ? 6 * hexHeight : 6 * hexHeight + hexHeight / 2;
                        this.Svg.Polyline(hex, styles).Transform(new Dictionary<string, IEnumerable<object>> {
					        { "translate", new object[] {
						        x * sideLength * 1.5 - hexWidth / 2,
						        dy - hexHeight / 2
					        }}
				        });
                    }

                    // Add an extra one at bottom-right, for tiling.
                    if (x == 0 && y == 0)
                    {
                        this.Svg.Polyline(hex, styles).Transform(new Dictionary<string, IEnumerable<object>> {
					        { "translate", new object[] {
						        6 * sideLength * 1.5 - hexWidth / 2,
						        5 * hexHeight + hexHeight / 2
					        }}
				        });
                    }

                    i++;
                }
            }
        }

        public static Pattern New(IDictionary<string, string> args)
        {
            return new Pattern(Guid.NewGuid().ToString(), args);
        }

        private static IDictionary<string, string> Extend(params IDictionary<string, string>[] maps)
        {
            return ExtendHelper.Extend(maps);
        }
    }
}

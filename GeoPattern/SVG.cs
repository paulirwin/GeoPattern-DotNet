using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeoPattern
{
    public class SVG
    {
        private static readonly XNamespace _namespace = "http://www.w3.org/2000/svg";

        public SVG()
        {
            this.Width = 100;
            this.Height = 100;

            this.Svg = new XElement(_namespace + "svg");
            
            this.Context = new Queue<XElement>(); // Track nested nodes
            this.SetAttributes(this.Svg, new Dictionary<string, object>
                {
                    //{ "xmlns", "http://www.w3.org/2000/svg" },
                    { "width", this.Width },
                    { "height", this.Height }
                });
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public XElement Svg { get; private set; }

        public Queue<XElement> Context { get; private set; }

        public XElement CurrentContext
        {
            get { return this.Context.Count > 0 ? this.Context.Peek() : this.Svg; }
        }

        public SVG End()
        {
            this.Context.Dequeue();
            return this;
        }

        public XElement CurrentNode
        {
            get
            {
                var context = this.CurrentContext;
                return context.HasElements ? context.Elements().Last() : context;
            }
        }

        public SVG Transform(IDictionary<string, IEnumerable<object>> transformations)
        {
            this.CurrentNode.SetAttributeValue("transform",
                string.Join(" ", transformations.Keys.Select(t =>
                {
                    return t + '(' + string.Join(",", transformations[t]) + ')';
                }))
            );

            return this;
        }

        public void SetAttributes(XElement el, IDictionary<string, object> attrs)
        {
            foreach (var attr in attrs.Keys)
            {
                el.SetAttributeValue(attr, attrs[attr]);
            }
        }

        public void SetWidth(float width)
        {
            this.Svg.SetAttributeValue("width", width);
        }

        public void SetHeight(float height)
        {
            this.Svg.SetAttributeValue("height", height);
        }

        public override string ToString()
        {
            return this.Svg.ToString();
        }

        public SVG Rect(int x, int y, string width, string height, IDictionary<string, object> args)
        {
            var rect = new XElement(_namespace + "rect");
            this.CurrentContext.Add(rect);
            this.SetAttributes(rect, Extend(new Dictionary<string, object>
                {
                    { "x", x },
                    { "y", y },
                    { "width", width },
                    { "height", height }
                }, args));

            return this;
        }

        public SVG Circle(int cx, int cy, int r, IDictionary<string, object> args)
        {
            var circle = new XElement(_namespace + "circle");
            this.CurrentContext.Add(circle);
            this.SetAttributes(circle, Extend(new Dictionary<string, object>
                {
                    { "cx", cx },
                    { "cy", cy },
                    { "r", r }
                }, args));

            return this;
        }

        public SVG Path(string str, IDictionary<string, object> args)
        {
            var path = new XElement(_namespace + "path");
            this.CurrentContext.Add(path);
            this.SetAttributes(path, Extend(new Dictionary<string, object>
                {
                    { "d", str }
                }, args));

            return this;
        }

        public SVG Polyline(string str, IDictionary<string, object> args)
        {
            var polyline = new XElement(_namespace + "polyline");
            this.CurrentContext.Add(polyline);
            this.SetAttributes(polyline, Extend(new Dictionary<string, object>
                {
                    { "points", str }
                }, args));

            return this;
        }

        public SVG Polyline(IEnumerable<string> str, IDictionary<string, object> args)
        {
            foreach (var s in str)
            {
                this.Polyline(s, args);
            }

            return this;
        }

        public SVG Group(IDictionary<string, object> args)
        {
            var group = new XElement(_namespace + "g");
            this.CurrentContext.Add(group);
            this.Context.Enqueue(group);
            this.SetAttributes(group, Extend(new Dictionary<string, object>(), args));

            return this;
        }

        private static IDictionary<string, object> Extend(IDictionary<string, object> source, IDictionary<string, object> second)
        {
            return ExtendHelper.Extend(source, second);
        }
    }
}

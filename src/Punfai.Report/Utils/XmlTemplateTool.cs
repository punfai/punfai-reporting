using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Punfai.Report.Utils
{
    public class XmlTemplateTool
    {
        public static string[] TrueValues = new string[] { "True", "true", "1" };
        public static string IfAttribute { get; set; }
        public static string RepeatAttribute { get; set; }
        public static string RepeatAttributeStart { get; set; }
        public static string RepeatAttributeEnd { get; set; }
        public static string DefaultItemAlias { get; set; }
        static XmlTemplateTool()
        {
            IfAttribute = "mesh-if";
            RepeatAttribute = "mesh-repeat";
            RepeatAttributeStart = "mesh-repeat-start";
            RepeatAttributeEnd = "mesh-repeat-end";
            DefaultItemAlias = "item";
        }
        public static void ReplaceKey(XElement element, string key, dynamic dvalue)
        {
            /* 
             * a) actual placeholder replacement occurs when dvalue is a string, also where ifs are handled
             * b) repeaters are searched for and generated if dvalue is IEnumerable
             * c) other objects are .ToString()ed.
             */
            string placeHolder = "{{" + key + "}}";
            if (dvalue == null) dvalue = "";
            if (dvalue is string sval)
            {
                // a) This is where the actual placeholder replacement happens!
                // a.i) but hold on it might be an if
                IEnumerable<XElement> iflist =
                    from el in element.Descendants()
                    where el.Attributes(IfAttribute).Count() > 0 &&
                        ((string)el.Attribute(IfAttribute) == key ||
                        ((string)el.Attribute(IfAttribute)).StartsWith($"{key}=") ||
                        ((string)el.Attribute(IfAttribute)).EndsWith($"={key}"))
                    select el;
                // process this here and we are doing one at a time. we can compare with a constant but not another variable.
                // process this at the end and all replacing is done then we can look for ifs
                // todo: do the better way when we rise above ReplaceKey and have a higher vantage point.
                foreach (var el in iflist.ToArray())
                {
                    var attvalue = (string)el.Attribute(IfAttribute);
                    if (attvalue == key && TrueValues.Contains(sval))
                    {
                        GenerateIf(el);
                    }
                    else
                    {
                        var bits = attvalue.Split('=');
                        if (bits.Length == 2)
                        {
                            if (bits[0] == key && bits[1] == sval ||
                                bits[1] == key && bits[0] == sval)
                                GenerateIf(el);
                            else
                                el.Remove();
                        }
                        else
                            el.Remove();
                    }
                }

                // ok just replace node content
                var holders = element.DescendantNodes().OfType<XText>().Where(n => n.Value.Contains(placeHolder));
                foreach (var t in holders)
                {
                    //News.AddDebug("text node match: {1}::{0}", t.Value, t.Parent.Name);
                    StringBuilder s = new StringBuilder(t.Value);
                    s.Replace(placeHolder, sval);
                    t.Value = s.ToString();
                };
                // let's see if we can replace attribute content
                var attholders = element.Descendants().Where(n => n.Attributes().Any(att => att.Value.Contains(placeHolder)));
                foreach (var el in attholders)
                {
                    foreach (var att in el.Attributes().Where(att => att.Value.Contains(placeHolder)))
                    {
                        StringBuilder s = new StringBuilder(att.Value);
                        s.Replace(placeHolder, sval);
                        att.Value = s.ToString();
                    }
                };
            }
            else if (dvalue is IEnumerable)
            {
                // b) cope with it
                /* if it's a dictionary there are 2 ways of using it
                 *   i. flattened: {{parentDic.childKey}} with keys explicitly specified in template
                 *  ii. recursive: mesh-repeat="pair in parentDic" {{pair.Key}} {{pair.Value}} for auto key:pair use in template (template doesn't need to know keys in advance)
                 */
                //Type dtype = dvalue.GetType(); // commonly IDictionary<string, object> for ExpandoObject and IDictionary<object, object> from python dict
                //if (dtype.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(dtype.GetGenericTypeDefinition())) // doesn't work for PythonDic
                // can't work out a generic way to handle it, PythonDic needs to be forced into IDictionary<object, object> mode
                if (dvalue is IDictionary<object, object>)
                {
                    // i. flattened
                    foreach (var pair in (IDictionary<object, object>)dvalue)
                    {
                        string keywithdot = string.Format("{0}.{1}", key, pair.Key.ToString());
                        ReplaceKey(element, keywithdot, pair.Value);
                        // if no alias was specified, let em omit the item dot prefix
                        if (key == DefaultItemAlias)
                            ReplaceKey(element, pair.Key.ToString(), pair.Value);
                    }
                }
                else if (dvalue is IDictionary<string, object>)
                {
                    // i. flattened
                    foreach (var pair in (IDictionary<string, object>)dvalue)
                    {
                        string keywithdot = string.Format("{0}.{1}", key, pair.Key);
                        ReplaceKey(element, keywithdot, pair.Value);
                        // if no alias was specified, let em omit the item dot prefix
                        if (key == DefaultItemAlias)
                            ReplaceKey(element, pair.Key, pair.Value);
                    }
                }
                else if (dvalue is IList<object>)
                {
                    // allow indexers
                    for (int i = 0; i < ((IList<object>)dvalue).Count; i++)
                    {
                        string indexerkey = string.Format("{0}[{1}]", key, i);
                        ReplaceKey(element, indexerkey, dvalue[i]);
                        // if no alias was specified, let em omit the item dot prefix
                        if (key == DefaultItemAlias) ReplaceKey(element, "[" + i.ToString() + "]", dvalue[i]);
                    }
                }
                // ii. recursive
                // since it's enumerable, whatever it is, give it a looping chance
                //IEnumerable<XElement> ellist = element.XPathSelectElements(".//*[@mesh-repeat = '" + key + "']");
                IEnumerable<XElement> ellist =
                    from el in element.Descendants()
                    where el.Attributes(RepeatAttribute).Count() > 0 &&
                        ((string)el.Attribute(RepeatAttribute) == key ||
                        ((string)el.Attribute(RepeatAttribute)).EndsWith(" in " + key))
                    select el;
                // since we're messing with the tree the linq result will be altered and complain, so ToArray()
                foreach (var el in ellist.ToArray())
                {
                    string itemAlias;
                    string att = (string)el.Attribute(RepeatAttribute);
                    if (att.EndsWith(" in " + key)) itemAlias = att.Remove(att.IndexOf(" in " + key));
                    else itemAlias = DefaultItemAlias;
                    GenerateRepeat(el, dvalue, itemAlias);
                }
                // an enumerable should be repeated, but maybe ToString()ing it is best for  {{list}}
                ReplaceKey(element, key, dvalue.ToString());
            }
            else
            {
                // c)
                ReplaceKey(element, key, dvalue.ToString());
            }
        }
        public static void GenerateIf(XElement template)
        {
            bool disappearing = template.Name.LocalName == IfAttribute;
            if (disappearing)
            {
                XElement ifblock = new XElement(template);
                template.AddBeforeSelf(ifblock.Nodes().ToArray());
                template.Remove();
            }
            else
            {
                template.Attribute(IfAttribute).Remove();
            }
        }
        public static void GenerateRepeat(XElement template, dynamic list, string itemAlias)
        {
            bool disappearing = template.Name.LocalName == RepeatAttribute;
            template.Attribute(RepeatAttribute).Remove();
            // handle dictionaries
            if (list is IDictionary<object, object>)
            {
                foreach (var pair in (IDictionary<object, object>)list)
                {
                    XElement repeat = new XElement(template);
                    string dicKey = string.Format("{0}.{1}", itemAlias, "Key");
                    ReplaceKey(repeat, dicKey, pair.Key.ToString());
                    string dicValue = string.Format("{0}.{1}", itemAlias, "Value");
                    ReplaceKey(repeat, dicValue, pair.Value);
                    // can omit the item dot prefix if no item alias was specified, though it could screw up complex nested situations by stepping on the wrong toes.
                    if (itemAlias == DefaultItemAlias) ReplaceKey(repeat, "Key", pair.Key.ToString());
                    if (itemAlias == DefaultItemAlias) ReplaceKey(repeat, "Value", pair.Value);

                    if (disappearing) template.AddBeforeSelf(repeat.Nodes().ToArray());
                    else template.AddBeforeSelf(repeat);
                }
            }
            else if (list is IDictionary<string, object>) // same as above but without the Key.ToString()
            {
                foreach (var pair in (IDictionary<string, object>)list)
                {
                    XElement repeat = new XElement(template);
                    string dicKey = string.Format("{0}.{1}", itemAlias, "Key");
                    ReplaceKey(repeat, dicKey, pair.Key);
                    string dicValue = string.Format("{0}.{1}", itemAlias, "Value");
                    ReplaceKey(repeat, dicValue, pair.Value);
                    // can omit the item dot prefix if no item alias was specified, though it could screw up complex nested situations by stepping on the wrong toes.
                    if (itemAlias == DefaultItemAlias) ReplaceKey(repeat, "Key", pair.Key);
                    if (itemAlias == DefaultItemAlias) ReplaceKey(repeat, "Value", pair.Value);

                    if (disappearing) template.AddBeforeSelf(repeat.Nodes().ToArray());
                    else template.AddBeforeSelf(repeat);
                }
            }
            // handle lists and other enumerables
            else
            {
                foreach (dynamic dvalue in list)
                {
                    XElement repeat = new XElement(template);
                    ReplaceKey(repeat, itemAlias, dvalue);
                    if (disappearing) template.AddBeforeSelf(repeat.Nodes().ToArray());
                    else template.AddBeforeSelf(repeat);
                }
            }
            template.Remove();
        }
        public static void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            // const int bufSize = 1024;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, (int)bytesRead);
            }
        }
        public static void CopyToStream(StringBuilder xml, Stream target)
        {
            char[] chars = new char[xml.Length];
            xml.CopyTo(0, chars, 0, xml.Length);
            byte[] xmlbuffer = UTF8Encoding.UTF8.GetBytes(chars);
            target.Write(xmlbuffer, 0, (int)xmlbuffer.Length);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace deneme6
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Bir XSD dosya yolu giriniz...");
                return;
            }

            var path = args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine("Girilen dosya konumu doğru değil...");
                return;
            }

            try
            {
                var cls = "using System;\nusing System.Collections.Generic;\n\nnamespace Tsoft{\n";
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                GenerateClass(xml.ChildNodes[1], ref cls);
                cls += "}";
                //Console.WriteLine(cls);
                var extension = path.Split('.').Last();
                var newPath = path.Replace($".{extension}", ".cs");
                File.WriteAllText(newPath, cls);
                Console.WriteLine($"{newPath} dosyası oluşturuldu.");


            }
            catch (Exception e)
            {
                Console.WriteLine("Aşağıdaki Hatadan Dolayı İşlem Sonlandırıldı...");
                Console.WriteLine(e.Message);
            }

        }

        private static void GenerateClass(XmlNode xml, ref string cls)
        {


            foreach (XmlNode node in xml.ChildNodes)
            {
               //Console.WriteLine(node.LocalName);
                switch (node.LocalName)
                {
                    case "complexType":
                        {
                            var isAbstract= node.Attributes.GetNamedItem("abstract");
                            cls += $"\tpublic{ (isAbstract!=null?" abstract":"") } class {node.Attributes["name"].Value}[~]";
                            GenerateClass(node, ref cls);
                            cls = cls = cls.Replace("[~]", "\n\t{\n");
                            cls += "\t}\n\n";
                            break;
                        }
                    case "complexContent":
                    case "sequence":
                        {
                            GenerateClass(node, ref cls);
                            break;
                        }
                    case "extension":
                        {
                            cls = cls.Replace("[~]", "");
                            cls += $" : {node.Attributes["base"].Value}\n\t{{\n";
                            GenerateClass(node, ref cls);
                            break;
                        }
                    case "element":
                        {

                            cls += $"\t\tpublic {GetType(node)} {node.Attributes["name"].Value}  {{ get; set; }}\n";
                            break;
                        }
                    default:
                        break;
                }

            }
        }

        private static string GetType(XmlNode node)
        {
            var type = node.Attributes["type"].Value;
            var isList = node.Attributes.GetNamedItem("maxOccurs");
            type = type == "xs:anyType" ? "object" : type;
            return isList != null
                ? $"List<{ConvertSystemType(type.Replace("xs:", ""))}>": (ConvertSystemType(type.Replace("xs:", "")));
        }

        static string ConvertSystemType(string s)
        {
            switch (s)
            {
                case "integer":
                    return "int";
                case "dateTime":
                    return "DateTime";
                case "boolean":
                    return "bool";
                default:
                    return s;
            }

        }
    }
}

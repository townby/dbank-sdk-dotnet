using System;
using System.Collections;
using System.Text;

namespace dbank_sdk_dotnet
{
    #region json解析类
    public class NSPJson
    {
        public static Hashtable ConvertToList(string text)
        {
            int count = text.Length;
            if (count == 0)
            {
                return null;
            }
            int index = 0;
            return ParseList(text, ref index, count);
        }

        private static Hashtable ParseList(string text, ref int index, int count)
        {
            Hashtable ht = new Hashtable();
            for (; index < count; index++)
            {
                if (text[index] == '}')
                {
                    break;
                }
                if (text[index] == '"' || text[index] == '{' || text[index] == ',')
                {
                    continue;
                }
                string tmpKey = ParseString(text, ref index, count);
                index++;
                if (text[index] == ':')
                {
                    index++;
                    if (text[index] == '{')
                    {
                        ht[tmpKey] = ParseList(text, ref index, count);
                    }
                    else
                    {
                        if (text[index] == '"')
                        {
                            index++;
                        }
                        ht[tmpKey] = ParseString(text, ref index, count);
                    }
                }
            }
            return ht;
        }

        private static string ParseString(string text, ref int index, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (; index < count; index++)
            {
                //if (text[index] == '\\' && text[index + 1] == 'u')
                //{
                //    sb.Append((char)int.Parse(text.Substring(index + 2, 4), System.Globalization.NumberStyles.HexNumber));
                //    index += 5;
                //    continue;
                //}
                if (text[index] == '"' || text[index] == ',' || text[index] == '}')
                {
                    break;
                }
                sb.Append(text[index]);
            }
            return sb.ToString();
        }
    }
    #endregion
}


using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mahas.Helpers
{
    public static class MahasConverter
    {
        public static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string AutoIdGetTempCode(string baseCode, string dateCode)
        {
            var date = DateTime.Now.ToString(dateCode);
            var tempCode = Regex.Replace(baseCode, "@{DATE}", date);
            tempCode = Regex.Replace(tempCode, "#", "");
            return tempCode;
        }

        public static string AutoId(string baseCode, string dateCode, string nomor)
        {
            var date = DateTime.Now.ToString(dateCode);
            var lengthCode = baseCode.Where(x => x == '#').Count();
            var id = "";
            if (string.IsNullOrEmpty(nomor))
            {
                id = baseCode.Replace(new string('#', lengthCode), new string('0', lengthCode - 1) + '1').Replace("@{DATE}", date);
            }
            else
            {
                var tempCode = AutoIdGetTempCode(baseCode, dateCode);
                int.TryParse(nomor.Replace(tempCode, ""), out int num);
                var stringNum = (num + 1).ToString();
                id = baseCode.Replace(new string('#', lengthCode), new string('0', lengthCode - stringNum.Length) + stringNum).Replace("@{DATE}", date);
            }
            return id;
        }
    }
}
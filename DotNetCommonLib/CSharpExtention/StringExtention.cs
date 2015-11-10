using System;
using System.Text.RegularExpressions;

namespace DotNetCommonLib
{
    public static partial class CSharpExtention
    {
        /// <summary>
        /// 判斷字符串是否為數字形式
        /// </summary>
        /// <returns>是則返回TRUE，否則返回FALSE</returns>
        public static bool IsNumeric(this string str)
        {
            Regex reg = new Regex(@"^-?\d+(\.\d+)?$");
            return reg.IsMatch(str);
        }

        /// <summary>
        /// 將數字形式的字符串轉換為整數
        /// </summary>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            if (str.IsNumeric())
                return Int32.Parse(str);
            else
                throw new Exception("不能將非數字形態的字符串強制轉換為數字!");
        }

        /// <summary>
        /// 將數字形式的字符串轉換為浮點數
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float ToFloat(this string str)
        {
            if (str.IsNumeric())
                return float.Parse(str);
            else
                throw new Exception("不能將非數字形態的字符串強制轉換為數字!");
        }

        /// <summary>
        /// 統計字符串中某字符出現的次數
        /// </summary>
        /// <param name="c">需要統計的字符</param>
        /// <returns>統計出現的次數</returns>
        public static int charCount(this string str, char c)
        {
            //1、據網上測試，使用遍歷的方法性能要快一些
            int k = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                    k++;
            }
            return k;

            //2、還有替換後比對長度的方法：
            //return str.Length - str.Replace(c, String.Empty.ToCharArray()[0]).Length;
            //或者：
            //return str.Length - str.Replace(c.ToString(), String.Empty).Length;

            //3、還有分割法，但是這種方法在應付字符緊靠在一起時可能會出錯
            //return str.Split(new char[] { c }).Length - 1;
        }

        /// <summary>
        /// 將\n換行符轉為WEB的<br />換行符
        /// 如將textarea中的輸入的換行從數據庫調出到WEB中是不能顯示的，轉成Web中的換行<br />後，就能正常顯示了。
        /// </summary>
        /// <returns>轉換後的字符串</returns>
        public static string Nl2Br(this string str)
        {
            return str.Replace("\n", "<br />");
        }

        /// <summary>
        /// 判斷字符串是否為空字符串或null。
        /// </summary>
        /// <param name="str">被擴展的對象</param>
        /// <returns>是返回True，否則返回False</returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 將字符串使用一組符號包含起來。
        /// </summary>
        /// <param name="str">被擴展的對象</param>
        /// <param name="wrapChat"></param>
        /// <returns>返回包裝好的字符串</returns>
        public static string Wrap(this string str, string wrapChat)
        {
            return wrapChat[0] + str + wrapChat[1];
        }

        /// <summary>
        /// 與Split方法相反，Join方法將字符串數組的元素用給定的分隔字符拼接成一個字符串返回。
        /// </summary>
        /// <param name="strArr">被擴展的對象</param>
        /// <param name="separator">用來拼接字符串的字符</param>
        /// <returns>拼接好的字符串</returns>
        public static string Join(this string[] strArr, char separator)
        {
            string res = string.Empty;
            foreach (string str in strArr)
            {
                res += str + separator;
            }
            return res.TrimEnd(separator);
        }

        /// <summary>
        /// 與Split方法相反，Join方法將字符串數組中的元素拼接成一個字符串返回。
        /// </summary>
        /// <param name="strArr">被擴展的對象</param>
        /// <returns>拼接好的字符串</returns>
        public static string Join(this string[] strArr)
        {
            string res = string.Empty;
            foreach (string str in strArr)
            {
                res += str;
            }
            return res;
        }

        /// <summary>
        /// 判斷字符串數組中是否包含給定的字符串元素。
        /// </summary>
        /// <param name="strArr">被擴展的對象</param>
        /// <param name="str">要判斷是否被包含的字符串</param>
        /// <returns>有則返回True，無則返回False</returns>
        public static bool Contains(this string[] strArr, string str)
        {
            foreach (string s in strArr)
            {
                if (s == str)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

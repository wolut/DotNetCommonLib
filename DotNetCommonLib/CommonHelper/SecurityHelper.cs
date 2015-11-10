using System;
using System.Text;

namespace DotNetCommonLib
{
    /// <summary>
    /// 加密處理幫助類
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// 對字符串進行MD5加密處理。
        /// </summary>
        /// <param name="text">需要加密的字符串</param>
        /// <returns></returns>
        public static string MD5(string text)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(text, "MD5");
        }

        /// <summary>
        /// 對字符串進行Base-64加密處理。
        /// </summary>
        /// <param name="text">需要加密的字符串</param>
        /// <returns></returns>
        public static string Base64Encoding(string text)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 對字符串進行Base-64解密處理。
        /// </summary>
        /// <param name="base64text">需要加密的字符串</param>
        /// <returns></returns>
        public static string Base64Decoding(string base64text)
        {
            byte[] bytes = Convert.FromBase64String(base64text);
            return Encoding.Default.GetString(bytes);
        }
    }
}

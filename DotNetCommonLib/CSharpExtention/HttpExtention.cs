using System;
using System.Web;

namespace DotNetCommonLib
{
    public static partial class CSharpExtention
    {
        /// <summary>
        /// 判斷一個WEB請求是否為AJAX請求
        /// </summary>
        /// <param name="hr"></param>
        /// <returns></returns>
        public static bool isAjaxRequest(this HttpRequest hr)
        {
            return hr.Headers["x-requested-with"] != null && hr.Headers["x-requested-with"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 輸出JSON格式的內容
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="json"></param>
        public static void EchoJson(this HttpContext Context, string json)
        {
            Context.Response.Clear();
            Context.Request.ContentType = "text/json";
            Context.Response.Write(json);
        }
    }
}

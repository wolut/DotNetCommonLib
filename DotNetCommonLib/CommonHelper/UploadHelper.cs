using System;
using System.IO;
using System.Web;
using System.Configuration;

namespace DotNetCommonLib
{
    /// <summary>
    /// Web文件上傳輔助類
    /// </summary>
    public class UploadHelper
    {
        /// <summary>
        /// 文件上傳大小限制，單位是bit
        /// 會優先讀取用戶設定的值，如果沒有設定值，會去讀取AppSettings中的UpoadSizeLimit配置段的值，如果讀取不到，會返回默認的值2M。
        /// </summary>
        public int? SizeLimit
        {
            get
            {
                if (ConfigurationManager.AppSettings["UploadSizeLimit"] == null)
                    return _sizeLimit ?? 2048 * 1024;
                else
                    return _sizeLimit ?? ConfigurationManager.AppSettings["UploadSizeLimit"].ToInt();
            }
            set
            {
                _sizeLimit = value;
            }
        }
        private int? _sizeLimit = null;

        /// <summary>
        /// 允許上傳的文件類型後綴名，如有多個請用;隔開
        /// </summary>
        private string UploadTypes
        {
            get
            {
                return _uploadTypes.Join(';');
            }
            set
            {
                _uploadTypes = value.Split(';');
                for (int i = 0; i < _uploadTypes.Length; i++)
                {
                    _uploadTypes[i] = "." + _uploadTypes[i].TrimStart('.');
                }
            }
        }
        private string[] _uploadTypes = null;

        /// <summary>
        /// 構造函數
        /// </summary>
        public UploadHelper()
        {
            if (ConfigurationManager.AppSettings["UploadFileType"] != null)
                UploadTypes = ConfigurationManager.AppSettings["UploadFileType"];
        }

        /// <summary>
        /// <para>將通過Web上傳的文件保存到服務器，如果沒有指定保存的完整路徑，則會自動以上傳的文件名保存到指定的上傳文件夾中。</para>
        /// <para>上傳文件夾可以通過在AppSettings中添加"UploadFolder"配置段來設置。</para>
        /// <para>比如添加&lt;add key="UploadFolder" value="UploadFile"/&gt;配置段，則上傳的文件將自動保存到應用程序的根目錄下的UploadFile目錄下。</para>
        /// </summary>
        /// <param name="uploadFile">上傳的文件</param>
        /// <param name="savePath">保存的完整路徑</param>
        public void WebUpload(HttpPostedFile uploadFile, string savePath)
        {
            //1、檢查文件大小
            if (uploadFile.ContentLength > SizeLimit)
                throw new Exception("來自UploadHelper.Upload的錯誤:上傳的文件大小超過限制！");
            //2、檢查文件類型
            string ext = Path.GetExtension(uploadFile.FileName);
            if (_uploadTypes != null && _uploadTypes.Contains(ext))
                throw new Exception("來自UploadHelper.Upload的錯誤:上傳的文件類型非法！");
            //3、保存文件
            uploadFile.SaveAs(savePath);
        }

        /// <summary>
        /// <para>將通過Web上傳的文件保存到服務器，如果沒有指定保存的完整路徑，則會自動以上傳的文件名保存到指定的上傳文件夾中。</para>
        /// <para>上傳文件夾可以通過在AppSettings中添加"UploadFolder"配置段來設置。</para>
        /// <para>比如添加&lt;add key="UploadFolder" value="UploadFile"/&gt;配置段，則上傳的文件將自動保存到應用程序的根目錄下的UploadFile目錄下。</para>
        /// </summary>
        /// <param name="uploadFile">上傳的文件</param>
        public string WebUpload(HttpPostedFile uploadFile)
        {
            string uploadDirectory = ConfigurationManager.AppSettings["UploadFolder"] ?? "UploadFile";
            uploadDirectory = uploadDirectory.Replace("$yyyy", DateTime.Now.Year.ToString());
            uploadDirectory = uploadDirectory.Replace("$mm", DateTime.Now.Month.ToString());
            uploadDirectory = uploadDirectory.Replace("$dd", DateTime.Now.Day.ToString());
            uploadDirectory = uploadDirectory.Replace('/', '\\');

            string uploadPath = AppDomain.CurrentDomain.BaseDirectory + uploadDirectory.TrimStart('\\');
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string fileName = uploadPath + "\\" + Path.GetFileName(uploadFile.FileName);

            WebUpload(uploadFile, fileName);

            return fileName;
        }

        /// <summary>
        /// 從Web網頁上下載指定的文件。
        /// </summary>
        /// <param name="filePath">要下載的文件路徑，必須是絕對路徑</param>
        public void WebDownload(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo finfo = new FileInfo(filePath);
                HttpResponse rs = HttpContext.Current.Response;
                rs.Clear();
                rs.ClearContent();
                rs.ClearHeaders();
                //Http協議中文件下載的頭格式，以此通知瀏覽器：本次響應是一個需要下載的文件。
                rs.AddHeader("Content-Disposition", "attachment;filename=" + HttpContext.Current.Server.UrlEncode(finfo.Name));
                //指明Content-Length，這樣下載時就會顯示進度了
                rs.AddHeader("Content-Length", finfo.Length.ToString());
                //以二進制流的方式響應，可以適應全部文件格式，詳細可以查閱：http://tool.oschina.net/commons
                rs.ContentType = "application/octet-stream";
                rs.ContentEncoding = System.Text.Encoding.Default;
                rs.WriteFile(filePath);
                rs.Flush();
                rs.End();
            }
            else
            {
                throw new FileNotFoundException("來自UploadHelper.Download的錯誤:下載的文件不存在！", filePath);
            }
        }
    }
}

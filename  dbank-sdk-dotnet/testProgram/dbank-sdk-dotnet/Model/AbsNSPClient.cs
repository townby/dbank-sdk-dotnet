using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Web;

namespace dbank_sdk_dotnet
{
    public delegate void ProgressDelegate(long donebytes,long totalbytes);

    #region 客户端操作父类
    public class AbsNSPClient
    {
        #region 内部变量
        protected const string NSP_APP = "nsp_app";
        protected const string NSP_SID = "nsp_sid";
        protected const string NSP_CLIENT = "client";
        protected const string NSP_KEY = "nsp_key";
        protected const string NSP_SVC = "nsp_svc";
        protected const string NSP_TS = "nsp_ts";
        protected const string NSP_PARAMS = "nsp_params";
        protected const string NSP_FMT = "nsp_fmt";
        protected const string NSP_TSTR = "nsp_tstr";

        protected const string NSP_URL = "http://api.dbank.com/rest.php";

        protected const string COMMON_UPLOAD = "/upload/up.php";

        Encoding utf8 = Encoding.UTF8;
        Encoding gb2312 = Encoding.GetEncoding("gb2312");
        #endregion

        #region 内部函数
        protected string getNSPKey(string secret,SortedDictionary<string, string> dics) {
            String md5str = new String(secret.ToCharArray());

            foreach (KeyValuePair<string, string> kv in dics)
            {
                md5str += kv.Key.ToString();
                md5str += kv.Value.ToString();
            }
            string key = getMd5Hash(md5str).ToUpper();
            return key;
        }

        /**
         * 计算post内容
         */
        protected string getPostData(string secret, SortedDictionary<string, string> dics)
        {
            String data = "";
            String md5str = new String(secret.ToCharArray());
            HttpUtility coder = new HttpUtility();

            foreach(KeyValuePair<string,string>kv in dics)
            {
                string k = kv.Key.ToString();
                string v = kv.Value.ToString();

                byte[] unicodebytes = gb2312.GetBytes(v);
                byte[] asciibytes = Encoding.Convert(gb2312, utf8, unicodebytes);

                try
                {
                    data += utf8.GetString(gb2312.GetBytes(k.ToCharArray())) + "=";
                    data += HttpUtility.UrlEncode(v, utf8) + "&";
                }
                catch (Exception err)
                {
                    throw err;
                }

                md5str += k;
                md5str += v;
            }
            string key = getMd5Hash(md5str).ToUpper();
            data += NSP_KEY + "=" + key;

            return data;
        }

        protected NSPResponse request(string httpurl, string data) {
            NSPResponse response = new NSPResponse();

            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "请求url " + httpurl + "?" + data);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpurl+"?"+data);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Accept-Encoding", "gzip");
            string strResult;
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream smRes = res.GetResponseStream();
            {
                strResult = res.ContentEncoding;
                if (strResult == "gzip")
                {
                    smRes = new GZipStream(smRes, CompressionMode.Decompress);
                }
                StreamReader sr = new StreamReader(smRes,System.Text.Encoding.UTF8);
                strResult = sr.ReadToEnd();
                sr.Close();
            }
            if (res != null)
            {
                res.Close();
            }
            if (smRes != null)
            {
                smRes.Close();
            }
            response.status_code = Convert.ToInt32(res.StatusCode);
            response.nsp_code = Convert.ToInt32(res.Headers["NSP_STATUS"]);
            response.server_time = res.Headers["Date"];
            response.content = strResult;

            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "请求url返回 " + strResult);
            return response;
        }

        /**
         * 计算字符串MD5值
         */
        protected string getMd5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(utf8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /**
		 * 构造mkfile字符串
		 */
		protected string mkfilestr(string jsonstr){

            Hashtable ht = NSPJson.ConvertToList(jsonstr);
            IDictionaryEnumerator ide = ht.GetEnumerator();

            string retstr = "{";

            if ((string)ht["success"] == "true")
            {
                while (ide.MoveNext())
                {
                    if (ide.Value.GetType().Name == "Hashtable")
                    {
                        Hashtable tt = (Hashtable)ide.Value;
                        retstr += "\"url\"" + ":\"" + tt["path"].ToString() + "\",";
                        retstr += "\"sig\"" + ":\"" + tt["sig"].ToString() + "\",";
                        retstr += "\"md5\"" + ":\"" + tt["nsp_fid"].ToString() + "\",";
                        retstr += "\"name\"" + ":\"" + tt["name"].ToString() + "\",";
                        retstr += "\"size\"" + ":" + tt["size"].ToString() + ",";
                        retstr += "\"ts\"" + ":" + tt["ts"].ToString() + ",";
                        retstr += "\"type\":\"File\"}";
                    }
                }
            }
			return retstr;
		}
		
        /**
         * 上传单个文件
         */
        protected string uploadFile(string appid,string secret,string host,string tstr,string filepath,ProgressDelegate progress=null) {

            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "上传文件 "+filepath);

            SortedDictionary<string, string> dt = new SortedDictionary<string, string>();
            dt.Add(NSP_APP, appid);
            dt.Add(NSP_FMT, "JSON");
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            dt.Add(NSP_TS, unixTime.ToString());
            dt.Add(NSP_TSTR, tstr);
            string keyres = getNSPKey(secret, dt);
			dt.Add (NSP_KEY,keyres);
			
            string url = "http://" + host + "/upload/up.php?";
            Uri uri = new Uri(url);

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			
            //请求
            WebRequest req = WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;

            //组织表单数据
            StringBuilder sb = new StringBuilder();
			
			foreach(KeyValuePair<string,string>kv in dt)
            {
                string k = kv.Key.ToString();
                string v = kv.Value.ToString();
				
				sb.Append("--" + boundary);
	            sb.Append("\r\n");
	            sb.Append("Content-Disposition: form-data; name=\""+ k +"\"");
	            sb.Append("\r\n\r\n");
	            sb.Append(v);
	            sb.Append("\r\n");
			}
			
			string filename = Path.GetFileName (filepath);
			
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\""+filename+"\"; filename=\""+filepath+"\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: text/plain");
            sb.Append("\r\n\r\n");

            string head = sb.ToString();
            byte[] form_data = Encoding.UTF8.GetBytes(head);
            //结尾
            byte[] foot_data = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            //文件
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception) {
                NSPLog.log(LogMsgType.NSP_LOG_ERR, "无法打开文件 " + filepath);
                throw new Exception("无法打开指定上传文件: " + filepath);
            }
            //post总长度
            long length = form_data.Length + fileStream.Length + foot_data.Length;
            req.ContentLength = length;

            Stream requestStream = req.GetRequestStream();
            //发送表单参数
            requestStream.Write(form_data, 0, form_data.Length);
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
            int bytesRead = 0;
            long donebytes = 0;
            long totalbytes = fileStream.Length;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
                if (progress != null) {
                    donebytes += bytesRead;
                    progress(donebytes, totalbytes);
                }
            }
            requestStream.Write(foot_data, 0, foot_data.Length);
            requestStream.Close();

            WebResponse pos = null;
            try
            {
                pos = req.GetResponse();
            }
            catch (WebException err)
            {
                NSPLog.log(LogMsgType.NSP_LOG_ERR, "文件 " + filepath + " 上传出错");
                NSPLog.log(LogMsgType.NSP_LOG_ERR, err.Message);
                throw new Exception("文件传输错误: "+filepath);
            }
            StreamReader sr = new StreamReader(pos.GetResponseStream(), Encoding.UTF8);
            string html = sr.ReadToEnd().Trim();
            sr.Close();
            if (pos != null)
            {
                pos.Close();
                pos = null;
            }
            if (req != null)
            {
                req = null;
            }
            return html;

        }

        /**
         * 上传文件
         */
        protected string uploadFiles(Object upauth, ArrayList filelist, ProgressDelegate progress=null) {

            string upresults = "[";

            Dictionary<Object, Object> dt =(Dictionary<Object, Object>)upauth;

            object nsp_host,nsp_tstr,secret,nsp_tapp;
            if (dt.TryGetValue((object)"nsp_host", out nsp_host) == false ||
                dt.TryGetValue((object)"nsp_tstr", out nsp_tstr) == false ||
                dt.TryGetValue((object)"secret", out secret) == false ||
                dt.TryGetValue((object)"nsp_tapp", out nsp_tapp)== false) {
                    throw new Exception("上传鉴权出错");
            }
			
            foreach (Object filename in filelist) {
                string uploadresult = null;
                string mksinglefile = null;
                try
                {
                    uploadresult = uploadFile(nsp_tapp.ToString(), secret.ToString(), nsp_host.ToString(), nsp_tstr.ToString(), filename.ToString(),progress);
                    mksinglefile = mkfilestr(uploadresult);
                }
                catch (Exception err) {
                    NSPLog.log(LogMsgType.NSP_LOG_ERR, err.Message);
                    throw err;
                }
				upresults+=mksinglefile+",";
            }
			upresults=upresults.Substring(0,upresults.Length-1)+"]";
			
            return upresults;
        }

        /**
         * 下载文件
         */
        protected bool downloadfile(string url,string savefullpath, ProgressDelegate progress=null)
        {
            //处理下载url不符合rfc规范的情况
            MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (getSyntax != null && flagsField != null)
            {
                foreach (string scheme in new[] { "http", "https" })
                {
                    UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                    if (parser != null)
                    {
                        int flagsValue = (int)flagsField.GetValue(parser);
                        // Clear the CanonicalizeAsFilePath attribute
                        if ((flagsValue & 0x1000000) != 0)
                            flagsField.SetValue(parser, flagsValue & ~0x1000000);
                    }
                }
            }

            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "请求下载 " + url);
            //文件下载
            WebRequest req = WebRequest.Create(url);
            WebResponse pos = req.GetResponse();
            long totalbytes = pos.ContentLength;
            Stream s = pos.GetResponseStream();
            FileStream fs = new FileStream(savefullpath, FileMode.OpenOrCreate, FileAccess.Write);
            byte[] buffer = new byte[1024];

            int bytesRead = s.Read(buffer, 0, buffer.Length);
            long donebytes = 0;
            while (bytesRead > 0)
            {
                fs.Write(buffer, 0, bytesRead);
                bytesRead = s.Read(buffer, 0, buffer.Length);
                if (progress != null)
                {
                    donebytes += bytesRead;
                    progress(donebytes, totalbytes);
                }
            }

            fs.Close();
            s.Close();
            pos.Close();
            return true;
        }

        /**
         * 判断字符串是否为数字，系统级服务or用户级服务
         */
        protected bool isNumeric(string str, int res)
        {
            res = -1;
            return int.TryParse(str,out res);
        }
        #endregion
    }
    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace dbank_sdk_dotnet
{
    #region 日志记录
    public enum LogMsgType{
        NSP_LOG_ERR = 0,
        NSP_LOG_NOTICE = 1
    }
    public class NSPLog
    {
        //日志文件名
        static string logname = "nsp_sdk.log";

        //设置记录日志开关
        static bool whetherlog = false;

        #region 记录日志
        public static void log(LogMsgType logtype, string logmsg) {
            if (whetherlog == false)
            {
                return;
            }
            try
            {
                DateTime nowTime = DateTime.Now;

                FileStream fs = new FileStream(logname, FileMode.Append | FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);

                string type = null;

                if (logtype.Equals(LogMsgType.NSP_LOG_ERR)) {
                    type = "[error]";
                }
                else if (logtype.Equals(LogMsgType.NSP_LOG_NOTICE))
                {
                    type = "[notice]";
                }
                else {
                    type = "[unknown]";
                }

                sw.WriteLine(nowTime.ToString()+" # " + type + " # "+logmsg);

                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception) {
                throw new Exception("记录log日志出错");
            }
        }
        #endregion
        
        #region 打开或者关闭记录
        public static void turnOnLog(bool choice){
            whetherlog = choice;
            if (whetherlog == false) { return; }

            IPHostEntry oIPHost = Dns.GetHostEntry(Environment.MachineName);   
            if(oIPHost.AddressList.Length>0)
            {
                string ipinfo = string.Empty;
                foreach(IPAddress ipa in oIPHost.AddressList){
                    ipinfo += ipa.ToString() + " ";
                }
                NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "开始记录日志: " + ipinfo);

            }
        }
        #endregion
    }
    #endregion
}

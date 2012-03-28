using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbank_sdk_dotnet
{
    #region 请求返回信息
    public class NSPResponse
    {
        /**
         * HTTP连接的返回结果
         */
        public int status_code
        {
            get;
            set;
        }

        /**
         * NSP_STATUS
         */
        public int nsp_code
        {
            get;
            set;
        }

        /**
         * 返回的Server时间
         */
        public string server_time
        {
            get;
            set;
        }


        /**
         * 返回的内容
         */
        public string content
        {
            get;
            set;
        }
    }
    #endregion
}

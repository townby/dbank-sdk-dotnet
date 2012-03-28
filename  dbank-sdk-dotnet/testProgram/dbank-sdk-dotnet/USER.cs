using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbank_sdk_dotnet
{
    #region 用户信息操作类
    public class USER
    {
        NSPClient nspClient;

        #region 构造函数
        /**
         * 参数为客户端操作对象nspClient（通过session和secret构造）
         */
        public USER(NSPClient nspClient) {
            this.nspClient = nspClient;
        }
        #endregion

        #region 获取用户信息  getInfo
        public Dictionary<object, object> getInfo(string[] attrs) {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < attrs.Count(); i++)
            {
                combi.Add(i, attrs[i]);
            }
            sendParams[0] = combi;
            Object res = this.nspClient.callService("nsp.user.getInfo", sendParams);
            return (Dictionary<object, object>)res;
        }
        #endregion

        #region 更新用户信息  update
        public bool update(Dictionary<string, object> attrs)
        {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            foreach (KeyValuePair<string, Object> kv in attrs)
            {
                sendParams.Add(kv.Key, kv.Value);
            }
            Object res = this.nspClient.callService("nsp.user.getInfo", sendParams);
            return (bool)res;
        }
        #endregion
    }
    #endregion
}

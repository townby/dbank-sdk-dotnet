using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;
using System.Reflection;
using System.IO;

using Conversive.PHPSerializationLibrary;

namespace dbank_sdk_dotnet
{
    #region 客户端操作类
    //
    //  NSPClient 统一的dbank对外api类，主要提供以下四个接口
    //
    //    1:NSPClient(string session, string secret)    构造函数，提供session+secret和appid+appkey两种方式
    //
    //    2:callService(string srvname,Object reqs)     调用系统级接口和用户级接口
    //
    //          dbank 开放平台上的接口均可通过这个函数调用服务，只需传入正确的api名称和参数即可
    //          参数 reqs 类型为Dictionary<Object,Object>,不支持ArrayList,Hashtable.构造参数方式参照示例
    //
    //    3:uploadFile(string dirpath,ArrayList filelist)  上传文件(路径，文件数组)
    //
    //    4:uploadFileProgress(string dirpath, string file, ProgressDelegate progress) 带回调函数
    //
    //    4:downloadFile(string url, string savefullname, ProgressDelegate progress)  下载文件(url路径，本地存储名称)
    //
    //    5:service()调用服务对象(更加方便接口调用)
    //
    public class NSPClient:AbsNSPClient
    {
        #region 内部变量
        protected Serializer seri;
        //whether systemlevel or userlevel
        private bool isSysLevel;

        private string id;
        private string secret;

        private long ts_adjustment = 0;
        #endregion

        #region 构造函数
        //session stands for appid or session
        //secret stands for appkey or secret
        public NSPClient(string session, string secret) {
            if (isNumeric(session, 0))
            {
                isSysLevel = true;
            }
            else {
                isSysLevel = false;
            }
            this.id = session;
            this.secret = secret;
            seri = new Serializer();
        }
        #endregion

        #region 调用服务接口
        public Object callService(string svrname,Object reqs){

            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "调用平台服务 " + svrname);
            //记录系统级post数据
            SortedDictionary<string, string> dt = new SortedDictionary<string, string>();
            if (this.isSysLevel)
            {
                dt.Add(NSP_APP, this.id);
            }
            else {
                dt.Add(NSP_SID, this.id);
            }

            try
            {
                if (reqs == null) {
                    Dictionary<Object, Object> empty = new Dictionary<object, object>();
                    reqs = empty;
                }

                string serires = seri.Serialize(reqs);
                dt.Add(NSP_PARAMS, serires);
                dt.Add(NSP_SVC, svrname);
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                DateTime nowTime = DateTime.Now;
                long unixTime = (long)Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
                if (ts_adjustment != 0)
                {
                    dt.Add(NSP_TS, (unixTime + ts_adjustment).ToString());
                }
                else
                {
                    dt.Add(NSP_TS, unixTime.ToString());
                }
                dt.Add(NSP_FMT, "php-rpc");

                //模拟post并获取结果
                string data = getPostData(this.secret, dt);
                NSPResponse response = request(NSP_URL, data);

                if (response.nsp_code == 109) {
                    NSPLog.log(LogMsgType.NSP_LOG_ERR, "客户端系统时间有误");
                    ts_adjustment = (long)(DateTime.Parse(response.server_time) - nowTime).TotalSeconds;
                    dt[NSP_TS] = (unixTime + ts_adjustment).ToString();
                    data = getPostData(this.secret, dt);
                    response = request(NSP_URL, data);
                }
                return seri.Deserialize(response.content);
            }
            catch(Exception err){
                NSPLog.log(LogMsgType.NSP_LOG_ERR, "调用服务 " + svrname + " 返回信息解析错误,"+err.ToString());
                throw err;
            }
        }
        #endregion

        #region 上传文件接口
        public Result uploadFile(string dirpath, params string[] files) {
            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "调用上传服务接口 uploadFile");
            // call upauth 上传鉴权
            Object upauth = this.callService("nsp.vfs.upauth", null);

            ArrayList filesArray = new ArrayList();
            foreach(string file in files){
                filesArray.Add(file);
            }
            // upload 上传
            string upfiles = uploadFiles(upauth, filesArray);
            // call mkfile 创建上传文件
            Dictionary<Object,Object>dt = new Dictionary<object,object>();
            dt["files"]=upfiles;
            dt["path"]=dirpath;
            Object res = this.callService("nsp.vfs.mkfile", dt);
            return (Result)NSPWrapper.toBean(res, typeof(Result));
        }
        #endregion

        #region 上传单个文件进度接口
        public Result uploadFileProgress(string dirpath, string file, ProgressDelegate progress)
        {
            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "调用上传服务回调函数接口 uploadFileProgress");
            // call upauth 上传鉴权
            Object upauth = this.callService("nsp.vfs.upauth", null);

            ArrayList filesArray = new ArrayList();
            filesArray.Add(file);

            // upload 上传
            string upfiles = uploadFiles(upauth, filesArray, progress);
            // call mkfile 创建上传文件
            Dictionary<Object, Object> dt = new Dictionary<object, object>();
            dt["files"] = upfiles;
            dt["path"] = dirpath;
            Object res = this.callService("nsp.vfs.mkfile", dt);
            return (Result)NSPWrapper.toBean(res, typeof(Result));
        }
        #endregion

        #region 下载文件接口
        public bool downloadFile(string objfile, string savefullname, ProgressDelegate progress=null) {
            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "下载文件 " + objfile);
            //获取文件url
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> files = new Dictionary<object, object>();
            Dictionary<Object, Object> fields = new Dictionary<object, object>();
            files.Add(0, objfile);
            fields.Add(0, "url");
            sendParams.Add(0, files);
            sendParams.Add(1, fields);
            Result res = (Result)NSPWrapper.toBean(this.callService("nsp.VFS.getattr", sendParams), typeof(Result));
            if (res == null || res.successList.Count()==0) {
                return false;
            }
            string url = res.successList[0].url;
            return downloadfile(url, savefullname, progress);
        }
        #endregion

        #region 调用服务对象
        //
        //暂时提供 VFS 和 user两个操作对象
        //
        public T service<T>(Type item){
            if (item.Equals(typeof(VFS))) { 
                VFS v = new VFS(this);
                return (T)(object)v;
            }
            else if (item.Equals(typeof(USER)))
            {
                USER u = new USER(this);
                return (T)(object)u;
            }
            NSPLog.log(LogMsgType.NSP_LOG_NOTICE, "调用服务对象 "+item.Name);

            return default(T);
        }
        #endregion
    }
    #endregion

    #region 返回结果解析类
    //
    //  NSPWrapper 用于处理callService返回的对象生成指定结果
    //
    //  使用示例：(Object res = nC.callService(srvname, params);)
    //
    //          Result result = (Result)NSPWrapper.toBean(res,typeof(Result));
    //          LsResult result = (LsResult)NSPWrapper.toBean(res,typeof(LsResult));
    //
    public class NSPWrapper
    {
        public static Object toBean(Object obj, Type type) {
            return convert(obj, type);
        }

        private static Object convert(Object obj, Type type) {
            Object ret = null;
            if (obj is int || obj is double || obj is string || obj is bool)
            {
                ret = obj;
            }
            else if (obj is Dictionary<Object, Object>)
            {
                Dictionary<Object, Object> arr = (Dictionary<Object, Object>)obj;
                Type eletype = null;
                if (type.IsArray)
                {
                    eletype = Type.GetType(type.GetElementType().ToString());
                    ret = Array.CreateInstance(eletype, arr.Count);
                }
                else
                {
                    ret = Activator.CreateInstance(type);
                }

                foreach (KeyValuePair<Object, Object> kv in arr)
                {
                    object k = toBean(kv.Key, kv.Key.GetType());
                    object v = null;

                    PropertyInfo pi = null;
                    if (type.IsArray)
                    {
                        v = toBean(kv.Value, eletype);
                        ((Array)ret).SetValue(v, (int)k);
                    }
                    else
                    {
                        pi = type.GetProperty(k.ToString());
                        if (pi != null)
                        {
                            v = toBean(kv.Value, pi.PropertyType);
                            if (v is int)
                            {
                                type.GetProperty(k.ToString()).SetValue(ret, v.ToString(), null);
                            }
                            else
                            {
                                type.GetProperty(k.ToString()).SetValue(ret, v, null);
                            }
                        }
                        
                    }
                }
            }
            return ret;
        }
    }
    #endregion
}

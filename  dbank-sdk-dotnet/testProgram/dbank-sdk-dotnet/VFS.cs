using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace dbank_sdk_dotnet
{
    #region 网盘操作类 VFS
    public class VFS
    {
        NSPClient nspClient;

        #region 构造函数
        /**
         * 参数为客户端操作对象nspClient（通过session和secret构造）
         */
        public VFS(NSPClient nspClient) {
            this.nspClient = nspClient;
        }
        #endregion

        #region 列举目录 lsdir
        //
        //      path        string      文件夹路径                 "/Netdisk/" or "/Netdisk/软件/" ...
        //      fields      string[]    需要查询的文件属性数组       type name createTime size url ...
        //      type        int         1-文件，2-文件夹，3-文件与文件夹
        //      或者 option  dictionay<string,object>
        //
        public LsResult lsdir(string path,string[]fields,int type) {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < fields.Count(); i++) {
                combi.Add(i, fields[i]);
            }
            sendParams[0] = path;
            sendParams[1] = combi;
            sendParams[2] = type;
            Object res = this.nspClient.callService("nsp.vfs.lsdir", sendParams);
            return (LsResult)NSPWrapper.toBean(res, typeof(LsResult));
        }
        public LsResult lsdir(string path, string[] fields, Dictionary<string,object> option)
        {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < fields.Count(); i++)
            {
                combi.Add(i, fields[i]);
            }
            sendParams[0] = path;
            sendParams[1] = combi;
            sendParams[2] = option;
            Object res = this.nspClient.callService("nsp.vfs.lsdir", sendParams);
            return (LsResult)NSPWrapper.toBean(res, typeof(LsResult));
        }
        #endregion

        #region 拷贝文件与目录 copyfile
        //
        //      files       string[]    文件路径数组。该接口支持拷贝文件以及文件夹，文件夹会递归拷贝内容。
        //      path        string      拷贝目标文件夹文件路径
        //      attribute   map<string,string> 复制文件夹时的可选参数配置
        //
        public Result copyfile(string[] files, string path, Dictionary<string, string> attribute) {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < files.Count(); i++)
            {
                combi.Add(i, files[i]);
            }
            sendParams[0] = combi;
            sendParams[1] = path;
            if (attribute != null && attribute.Count != 0)
            {
                sendParams[2] = attribute;
            }
            Object res = this.nspClient.callService("nsp.vfs.copyfile", sendParams);
            return (Result)NSPWrapper.toBean(res, typeof(Result));
        }
        #endregion

        #region 文件移动 movefile
        //
        //      files       string[]    文件路径数组。该接口支持移动文件以及文件夹，文件夹会递归移动内容。
        //      path        string      移动目标文件夹文件路径
        //      attribute   map<string,string> 移动文件夹时的可选参数配置
        //
        public Result movefile(string[] files, string path, Dictionary<string, string> attribute) {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < files.Count(); i++)
            {
                combi.Add(i, files[i]);
            }
            sendParams[0] = combi;
            sendParams[1] = path;
            if (attribute != null && attribute.Count != 0)
            {
                sendParams[2] = attribute;
            }
            Object res = this.nspClient.callService("nsp.vfs.movefile", sendParams);
            return (Result)NSPWrapper.toBean(res, typeof(Result));       
        }
        #endregion

        #region 文件删除 rmfile
        //
        //      files       string[]    文件路径数组
        //      path        string      删除文件或文件夹时是否保留，默认为false，直接删除文件或文件夹。如果为true的话，将文件或文件夹移动到系统回收站进行暂时保留。
        //      attribute   map<string,string> 对文件删除操作附加一些属性控制
        //
        public Result rmfile(string[] files, bool reserve, Dictionary<string, string> attribute)
        {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi = new Dictionary<object, object>();
            for (int i = 0; i < files.Count(); i++)
            {
                combi.Add(i, files[i]);
            }
            sendParams[0] = combi;
            sendParams[1] = reserve;
            if (attribute != null && attribute.Count != 0)
            {
                sendParams[2] = attribute;
            }
            Object res = this.nspClient.callService("nsp.vfs.rmfile", sendParams);
            return (Result)NSPWrapper.toBean(res, typeof(Result));
        }
        #endregion

        #region 获取文件属性 getattr
        //
        //      files       string[]    文件路径数组
        //      fields      string[]    要查询的文件属性数组。“type,name”，两个属性，系统会自动返回，可以不用加入到列表中。
        //
        public Result getattr(string[] files, string[] fields) {
            Dictionary<Object, Object> sendParams = new Dictionary<object, object>();
            Dictionary<Object, Object> combi1 = new Dictionary<object, object>();
            Dictionary<Object, Object> combi2 = new Dictionary<object, object>();
            int i;
            for (i = 0; i < files.Count(); i++)
            {
                combi1.Add(i, files[i]);
            }
            for (i = 0; i < fields.Count(); i++)
            {
                combi2.Add(i, fields[i]);
            }
            sendParams[0] = combi1;
            if (combi2.Count != 0)
            {
                sendParams[1] = combi2;
            }
            Object res = this.nspClient.callService("nsp.vfs.getattr", sendParams);
            return (Result)NSPWrapper.toBean(res, typeof(Result));
        }
        #endregion
    }
    #endregion
}

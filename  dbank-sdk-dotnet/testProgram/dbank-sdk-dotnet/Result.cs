using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbank_sdk_dotnet
{
    #region 返回文档信息
    public class File : Dictionary<string,Object> {

        public string type {
            get { return getProperties("type").ToString(); }
            set { setProperties("type", value); }           
        }
        public string name {
            get { return getProperties("name").ToString(); }
            set { setProperties("name", value); } 
        }
        public string createTime
        {
            get { return getProperties("createTime").ToString(); }
            set { setProperties("createTime", value); }
        }
        public string modifyTime
        {
            get { return getProperties("modifyTime").ToString(); }
            set { setProperties("modifyTime", value); }
        }
        public string url
        {
            get { return getProperties("url").ToString(); }
            set { setProperties("url", value); }
        }
        public string size
        {
            get { return getProperties("size").ToString(); }
            set { setProperties("size", value); }
        }
        public Object getProperties(String key)
        {
            Object val;
            if (this.TryGetValue(key,out val)==true)
            {
                return val;
            }
            return "";
        }
        public void setProperties(String key, Object val)
        {
            Add(key, val);
        }
    }
    #endregion

    #region 返回错误信息
    public class Error : Dictionary<string,Object>
    {
        public string name
        {
            get { return getProperties("name").ToString(); }
            set { setProperties("name", value); }
        }
        public string type
        {
            get { return getProperties("type").ToString(); }
            set { setProperties("type", value); }
        }
        public string errCode
        {
            get { return getProperties("errCode").ToString(); }
            set { setProperties("errCode", value); }
        }
        public string errMsg
        {
            get { return getProperties("errMsg").ToString(); }
            set { setProperties("errMsg", value); }
        }
        public Object getProperties(String key)
        {
            Object val;
            if (this.TryGetValue(key, out val) == true)
            {
                return val;
            }
            return "";
        }
        public void setProperties(String key, Object val)
        {
            Add(key, val);
        }
    }
    #endregion

    #region 返回结果类型一
    //
    // nsp.vfs.lsdir 返回值
    //
    public class LsResult {
        private File[] ChildList;
        private string ErrCode;
        private string ErrMsg;
        public File[] childList
        {
            get { return this.ChildList; }
            set { this.ChildList = value; }
        }
        public string errCode
        {
            get { return this.ErrCode; }
            set { this.ErrCode = value; }
        }
        public string errMsg
        {
            get { return this.ErrMsg; }
            set { this.ErrMsg = value; }
        }
    }
    #endregion

    #region 返回结果类型二
    //
    // 普通接口返回值
    //
    public class Result
    {
        private File[] SuccessList;
        private Error[] FailList;

        public File[] successList {
            get { return this.SuccessList; }
            set { this.SuccessList = value; }
        }

        public Error[] failList
        {
            get { return this.FailList; }
            set { this.FailList = value; }
        }
    }
    #endregion
}

//你懂的
using System;
//
//获取用户信息 返回结果
//
//  dictionary<>    用户信息
//
//需要用到下面的空间
//
using System.Collections.Generic;

//
//包含这个命名空间，调用NSPClient
//
using dbank_sdk_dotnet;


namespace testProgram
{
    #region 测试项目
    class Program
    {
        #region 初始化操作对象
        //
        //  使用“鉴权接口”返回的session和secret构造客户端接口类
        //
        //  然后使用 NSPClient 的成员函数
        //
        NSPClient nC = new NSPClient("iuTeAN9uaQ6xYuCt8f7uaL4Hwua5CgiU2J0kYJq01KtsA4DY", "c94f61061b46668c25d377cead92f898");
        #endregion

        static void Main(string[] args)
        {
            Program p = new Program();

            #region 打开日志记录
            //可以选择记录日志文件nsp_sdk.log方便测试程序，查看运行信息
            NSPLog.turnOnLog(true);
            #endregion

            try
            {
                Console.WriteLine("\n测试一：获取用户信息\n");
                p.getInfoTest();    //获取用户信息

                Console.WriteLine("\n测试二：目录列举\n");
                p.LsdirTest();      //目录列举

                Console.WriteLine("\n测试三：文件删除\n");
                p.RmfileTest();     //文件删除

                Console.WriteLine("\n测试四：下载文件并显示进度信息\n");
                p.DownloadTest();   //下载文件并显示进度信息

                Console.WriteLine("\n测试五：上传文件(不显示进度)\n");
                p.UploadTest();     //上传文件(不显示进度)

                Console.WriteLine("\n测试六：上传文件显示进度消息\n");
                p.UploadProgressTest(); //上传文件显示进度消息
            }
            catch (Exception err) {
                Console.WriteLine(err.ToString());
            }

            Console.WriteLine("\nHappy 2012!");
        }

        #region 文件删除
        //
        //文件删除测试
        //api http://open.dbank.com/wiki/index.php?title=Nsp.VFS.rmfile
        //
        public void RmfileTest()
        {
            //获取VFS服务对象
            VFS vfs = nC.service<VFS>(typeof(VFS));

            try
            {
                string[] files = { "/Netdisk/test.cpp", "/Netdisk/penjin.txt", "/Netdisk/libiconv.rar" };
                Result rmres = vfs.rmfile(files, false, null);

                //打印结果
                int i;
                Console.Write("删除成功:\n");
                for (i = 0; i < rmres.successList.Length;i++)
                {
                    Console.Write(rmres.successList[i].name+" ");
                }
                Console.WriteLine();
                Console.Write("删除失败:\n");
                for (i = 0; i < rmres.failList.Length; i++)
                {
                    Console.WriteLine(rmres.failList[i].name+" 原因 "+rmres.failList[i].errMsg);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region 文件上传
        //
        //文件上传测试
        //
        public void UploadTest()
        {
            Result upres = nC.uploadFile("/Netdisk/", "D://我的测试.php", "D://dbank-sdk-dotnet-0.3.zip");

            int i;
            Console.Write("上传成功:\n");
            for (i = 0; i < upres.successList.Length; i++)
            {
                Console.Write(upres.successList[i].name + " ");
            }
            Console.WriteLine();
            Console.Write("上传失败:\n");
            for (i = 0; i < upres.failList.Length; i++)
            {
                Console.WriteLine(upres.failList[i].name + " 原因 " + upres.failList[i].errMsg);
            }
            Console.WriteLine();
        }
        #endregion

        #region 文件上传进度
        //
        //文件上传测试(单个文件)
        //
        public void UploadProgressTest()
        {
            Result upres = nC.uploadFileProgress("/Netdisk/", "D://dbank-sdk-dotnet-0.3.zip", new ProgressDelegate(new Program().ShowProgressBar));

            if (upres.failList.Length > 0)
            {
                Console.WriteLine(upres.failList[0].name + " 原因 " + upres.failList[0].errMsg);
            }
            else {
                Console.WriteLine(upres.successList[0].name + "上传成功");
            }
        }
        public void ShowProgressBar(long donebytes,long totalbytes) {
            Console.WriteLine(">>> " + ((double)donebytes / totalbytes).ToString("P"));
        }
        #endregion

        #region 文件下载进度
        //
        //文件下载测试
        //
        public void DownloadTest()
        {
            //参数：下载文件的路径，存于本地的目录
            if (nC.downloadFile("/Netdisk/dbank-sdk-dotnet-0.3.zip", "D://dbank-sdk-dotnet-0.3.zip", new ProgressDelegate(new Program().ShowProgressBar)))
            {
                Console.WriteLine("/Netdisk/dbank-sdk-dotnet-0.3.zip 文件下载成功");
            }
            else
            {
                Console.WriteLine("/Netdisk/dbank-sdk-dotnet-0.3.zip 文件下载失败");
            }
        }
        #endregion

        #region 列举目录
        //
        //列举目录测试
        //api http://open.dbank.com/wiki/index.php?title=Nsp.VFS.lsdir
        //
        public void LsdirTest()
        {
            VFS vfs = nC.service<VFS>(typeof(VFS));

            try
            {
                string[] fields = { "name", "url", "size", "type" };
                LsResult lsres = vfs.lsdir("/Netdisk/", fields, 3);
                Console.WriteLine(lsres.childList[0].name + " " + lsres.childList[0].type);

                //lsdir新接口参数
                Dictionary<string, object> option = new Dictionary<string, object>();
                option.Add("type", 3);
                option.Add("vertion", null);
                lsres = vfs.lsdir("/Netdisk/", fields, option);

                Console.WriteLine("{0,-32}{1,16}{2,16}", "名称", "类型", "大小");
                for (int i = 0; i < lsres.childList.Length ; i++)
                {
                    Console.WriteLine("{0,-32}{1,16}{2,16:N}",lsres.childList[i].name,lsres.childList[i].type,lsres.childList[i].size);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region 获取用户信息
        public void getInfoTest() { 
            //构造用户信息操作对象
            USER user = nC.service<USER>(typeof(USER));

            try
            {
                string[] attrs = {"user.username","product.productname","profile.usedspacecapacity"};
                Dictionary<object,object>res = user.getInfo(attrs);
                Console.WriteLine("网盘用户名称:" + res["user.username"] + "\n网盘用户类型：" + res["product.productname"] + "\n网盘剩余空间：" + res["profile.usedspacecapacity"]);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
    #endregion
}

//author: QQ(290979182) or mailto: 290979182@qq.com or http://weibo.com/littley
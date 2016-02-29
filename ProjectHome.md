<h1>dbank-sdk-dotnet</h1>

华为网盘(Dbank)开放平台的CSharp的SDK接口，可以轻松实现华为网盘的文件上传，文件下载，查询用户信息以及各种逻辑业务操作。（VS2010下开发测试可用）
<br />
第三方开发者可以通过这个SDK开发自己的application(此SDK也会长期保持优化改进)
<br />
平台使用：<a href='http://blog.csdn.net/ciaos/article/details/7653481'>dbank开放平台使用流程简介</a>
<br />
SDK demo: <a href='http://blog.csdn.net/ciaos/article/details/7818741'>用SDK实现属于自己的网盘</a>
<br />
<i>"如果您的应用能为网盘（个人云存储服务）带来更多优质的数据资源，吸引更多优秀的网盘客户，或者有其他更好的应用Idea，请Contact us。"</i>
<br />
<h2>更新信息</h2>
0.5版本增加sdk运行信息记录到日志文件(nsp\_sdk.log)功能,方便调试查看运行情况<br />
0.5.1版本平台接口传输采用gzip压缩，节省时间流量<br />
0.5.5提供直链生成功能,具体demo参照华为网盘外链 http://dl.dbank.com/c03v3behmg<br />



<h1>最新版本下载地址</h1>
增加直链生成功能
<br />
dbank-sdk-dotnet:
http://dbank-sdk-dotnet.googlecode.com/files/dbank-sdk-dotnet-0.5.5.zip
<br />
一个客户端demo
http://dbank-sdk-dotnet.googlecode.com/files/dbank-sdk-dotnet-demo.zip
<br />
CSDN 博客介绍 <a href='http://blog.csdn.net/ciaos/article/details/7818741'>用SDK实现属于自己的网盘</a>
<br />
<h1>API文档</h1>

请参照华为网盘官方API文档 http://open.dbank.com

<h1>意见与反馈</h1>

如有疑问和建议，请联系热爱移动互联网的Coder彭景@<a href='http://weibo.com/littley'>littley</a>
<h1>使用示例</h1>
<ul>
<li>引入类库<br>
<pre><code><br>
//<br>
//包含这个命名空间，调用NSPClient<br>
//<br>
using dbank_sdk_dotnet;<br>
</code></pre>
</li>
<li>初始化服务对象<br>
<pre><code><br>
//<br>
//  使用“鉴权接口”返回的session和secret构造客户端接口类<br>
//<br>
//  然后使用 NSPClient 的两个方法 callService 与 uploadFile<br>
//<br>
NSPClient nC = new NSPClient("iuTeAN9uaQ6xYuCt8f7uaL4Hwua5CgiU2J0kYJq01KtsA4DY","c94f61061b46668c25d377cead92f898");<br>
</code></pre>
</li>
<li>调用具体服务<br>
<pre><code><br>
public void LsdirTest()<br>
{<br>
//获取 网盘操作对象（内置文件拷贝，移动，删除等常用接口，调用更方便），0.3版本提供<br>
VFS vfs = nC.service&lt;VFS&gt;(typeof(VFS));<br>
<br>
try<br>
{<br>
string[] fields = { "name", "url", "size" };<br>
LsResult lsres = vfs.lsdir("/Netdisk/", fields, 3);<br>
Console.WriteLine(lsres.childList[0].name + " " + lsres.childList[0].type);<br>
}<br>
catch (Exception ex) {<br>
Console.WriteLine(ex.ToString());<br>
}<br>
}<br>
</code></pre>
</li>
<li>调用上传服务接口<br>
<pre><code><br>
public void UploadTest()<br>
{<br>
//增加可变参数，将多个上传文件追加参数末尾即可，直接返回操作结果，（0.3版本修改）<br>
Result upres = nC.uploadFile("/Netdisk/", "D://我的测试.php", "D://test.cpp");<br>
Console.WriteLine(upres.failList[0].errMsg);<br>
}<br>
</code></pre>
</li>
<li>调用文件下载接口(sdk 0.2以后 版本提供,sdk0.4版本增加上传下载的进度回调功能)<br>
<pre><code><br>
public void DownloadTest()<br>
{<br>
//参数：下载文件的路径，存于本地的目录，显示下载进度的函数（通过委托方式传递，0.4版本提供，上传进度回调方式类似）<br>
if (nC.downloadFile("/Netdisk/我的测试.php", "D://我的测试.php", new ProgressDelegate(new Program().ShowProgressBar))))<br>
{<br>
Console.WriteLine("文件下载成功");<br>
}<br>
else<br>
{<br>
Console.WriteLine("文件下载失败");<br>
}<br>
}<br>
//回调函数参数：（已上传/下载字节数，总字节数）<br>
public void ShowProgressBar(long donebytes,long totalbytes);<br>
</code></pre>
</li>

<li>查询用户信息<br>
<pre><code><br>
public void getInfoTest() {<br>
//构造用户信息操作对象（0.3版本提供）<br>
USER user = nC.service&lt;USER&gt;(typeof(USER));<br>
<br>
try<br>
{<br>
string[] attrs = {"user.username","product.productname","profile.usedspacecapacity"};<br>
Dictionary&lt;object,object&gt;res = user.getInfo(attrs);<br>
Console.WriteLine("用户名称:" + res["user.username"] + " 用户类型：" + res["product.productname"] + "  剩余空间：" + res["profile.usedspacecapacity"]);<br>
}<br>
catch (Exception ex) {<br>
Console.WriteLine(ex.ToString());<br>
}<br>
}<br>
</code></pre>
</li>
<li>生成直链(0.5.5版本提供)<br>
<pre><code><br>
public void getDirectUrlTest() {<br>
//直链说明请参照http://dl.dbank.com/c03v3behmg<br>
//使用用户对直链鉴权后返回的直链APPID以及直链APPSECRET<br>
//此处不应使用用户级的sessionid和secret！！！<br>
NSPClient nC2 = new NSPClient("51345", "6ykOuxbeL68502d9FVRI766W1drADlwn");<br>
<br>
VFS_LINK vfs_link = nC2.service&lt;VFS_LINK&gt;(typeof(VFS_LINK));<br>
try<br>
{<br>
string path = "/我的网盘/我的应用/PublicFiles/testlua.zip"; // 请确定文件的路径，不是"/Netdisk/"下，而是在直链目录下<br>
string clientIp = nC.getExtIpAddress();<br>
<br>
Dictionary&lt;object, object&gt; res = vfs_link.getDirectUrl(path, clientIp);<br>
<br>
Console.WriteLine("retcode=" + res["retcode"]);<br>
Console.WriteLine("url=" + res["url"]);<br>
}<br>
catch (Exception err) {<br>
Console.WriteLine(err.ToString());<br>
}<br>
}<br>
</code></pre>
</li>
<li>记录日志(0.5版本新增功能)<br>
<pre><code><br>
//打开(或关闭)日志记录功能（可选,默认关闭）<br>
//(同时提供NSPLog.log接口用于记录客户端日志信息一并存于log文件中)<br>
NSPLog.turnOnLog(true);<br>
<br>
//nsp_sdk.log中记录如下<br>
//<br>
//2012/3/9 9:50:44 # [notice] # 调用平台服务 nsp.user.getInfo<br>
//2012/3/9 9:50:44 # [notice] # 调用平台服务 nsp.vfs.lsdir<br>
//2012/3/9 9:50:44 # [notice] # 调用平台服务 nsp.vfs.lsdir<br>
//2012/3/9 9:50:44 # [notice] # 调用平台服务 nsp.vfs.rmfile<br>
//2012/3/9 9:50:44 # [notice] # 下载文件 /Netdisk/dbank-sdk-dotnet-0.3.zip<br>
//2012/3/9 9:50:44 # [notice] # 调用平台服务 nsp.VFS.getattr<br>
//2012/3/9 9:50:45 # [notice] # 调用上传服务接口 uploadFile<br>
//2012/3/9 9:50:45 # [notice] # 调用平台服务 nsp.vfs.upauth<br>
//2012/3/9 9:50:45 # [notice] # 上传文件 D://我的测试.php<br>
//2012/3/9 9:50:46 # [notice] # 上传文件 D://dbank-sdk-dotnet-0.3.zip<br>
//2012/3/9 9:50:48 # [notice] # 调用平台服务 nsp.vfs.mkfile<br>
//2012/3/9 9:50:48 # [notice] # 调用上传服务回调函数接口 uploadFileProgress<br>
//2012/3/9 9:50:48 # [notice] # 调用平台服务 nsp.vfs.upauth<br>
//2012/3/9 9:50:48 # [notice] # 上传文件 D://dbank-sdk-dotnet-0.3.zip<br>
//... ...<br>
//<br>
</code></pre>
</li>
</ul>



详细使用方法参照sdk中示例程序。
<h1>友情提示</h1>
获取网盘用户sessionid和secret的方法请参照开放平台鉴权说明 http://open.dbank.com/wiki/index.php?title=开放平台鉴权<br />
第三方开发者须遵循华为网盘开放平台的开发规范，首先申请网盘开发注册使用的appid，访问授权网页，用户在填入授权信息后将返回sessionid以及secret给应用程序
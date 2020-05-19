using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using UpdateFileCommon;


namespace UpdateFileCommon
{
    public partial class UpdateForm : Form
    {
        WebClient client;
        int fileIndex = 0;
        int fileListCount = 0;
        string ServerPath = null;
        string UpdateProgramDic = null;
        string UpdateProgramName = null;
        List<string> fileList = new List<string>();

        public UpdateForm()
        {
            InitializeComponent();

            this.Load += delegate
            {
                lblState.Text = "正在连接服务器...";
                lblRemark.Visible = false;

                if (VersionHelper.IsRequiredUpdate())
                {
                    Thread thread = new Thread(DownLoadFileMethod);
                    thread.IsBackground = true;
                    thread.Start();
                }
                else
                {
                    MessageBox.Show("已是最新版本", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            };

            this.FormClosing += delegate
            {
                if (client != null)
                    client.CancelAsync();
            };
        }

        /// <summary>
        /// 开始下载文件
        /// </summary>
        public void DownLoadFileMethod()
        {
            string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "UpdateFile.xml";
            fileList = VersionHelper.GetNeedUpdateFile();
            XmlDocument localDoc = new XmlDocument();
            localDoc.Load(xmlPath);
            var serverurl = VersionHelper.GetLoaclServerConfigURL(localDoc);
            ServerPath = XMLHelper.GetXmlNodeValueByXpath(serverurl, "/Config/ServerURL");
            UpdateProgramDic = XMLHelper.GetXmlNodeValueByXpath(localDoc, "/Config/UpdateProgramDirectory");
            UpdateProgramName = XMLHelper.GetXmlNodeValueByXpath(localDoc, "/Config/ClientProgramName");
            fileListCount = fileList.Count;
            this.BeginInvoke((ThreadStart)delegate
            {
                lblState.Text = "正在下载文件：";
                lblRemark.Visible = true;
                DownloadFile();
            });
        }

        public void DownloadFile()
        {
            //下载文件
            if (fileIndex < fileListCount)
            {
                try
                {
                    client = new WebClient();
                    lblRemark.Text = string.Format("正在下载第 {0} 个文件（共 {1} 个）", fileIndex + 1, fileListCount);
                    var uri = new Uri(ServerPath + fileList[fileIndex]);
                    var savePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    if (fileList[fileIndex] == UpdateProgramName)
                        savePath += UpdateProgramDic + "\\";
                    else
                        savePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    if (!Directory.Exists(savePath))//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    //判断文件是否存在
                    if (UrlFileExists(uri.ToString()))
                    {
                        //开始下载文件
                        client.DownloadProgressChanged += OnDownloadProgressChanged;
                        client.DownloadFileAsync(uri, savePath + fileList[fileIndex].Replace("/", "\\"));
                        client.DownloadFileCompleted += OnDownloadComplete;
                    }
                    else
                    {
                        MessageBox.Show(string.Format("下载更新出错：{0}文件不存在", uri.ToString()), "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("下载更新出错：" + ex.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            //下载完成
            else
            {
                string xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "UpdateFile.xml";
                XmlDocument clientDoc = new XmlDocument();
                clientDoc.Load(xmlPath); //加载XML文档
                var serverurl = VersionHelper.GetLoaclServerConfigURL(clientDoc);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(serverurl); //加载XML文档
                //获取服务器版本号
                var vernode = XMLHelper.GetXmlNodeByXpath(xmlDoc, "/Config/Version/Item");
                var ver = XMLHelper.GetNodeAttributeValue(vernode, "Version");
                var vervalue = XMLHelper.GetNodeAttributeValue(vernode, "VersionValue");
                //更新本地版本号
                XMLHelper.CreateOrUpdateXmlNodeByXPath(clientDoc, "/Config", "LocalVersion", ver);
                XMLHelper.CreateOrUpdateXmlNodeByXPath(clientDoc, "/Config", "LocalVersionValue", vervalue);
                //更新文件列表
                var serverNodes = XMLHelper.GetXmlNodeListByXpath(xmlDoc, "/Config/File/Item");
                var xmlText = "";
                foreach (XmlNode item in serverNodes)
                {
                    var name = XMLHelper.GetNodeAttributeValue(item, "Name");
                    var version = XMLHelper.GetNodeAttributeValue(item, "Version");
                    var date = XMLHelper.GetNodeAttributeValue(item, "Date");
                    xmlText += String.Format("<Item Name=\"{0}\" Version=\"{1}\" Date=\"{2}\"></Item>", name, version, date);
                }
                XMLHelper.CreateOrUpdateXmlNodeByXPath(clientDoc, "/Config", "File", xmlText);
                clientDoc.Save(xmlPath);
                MessageBox.Show("下载更新完成，请重新启动程序", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool UrlFileExists(string uri)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "HEAD";
                req.Timeout = 100;
                res = (HttpWebResponse)req.GetResponse();
                return (res.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (res != null)
                {
                    res.Close();
                    res = null;
                }
                if (req != null)
                {
                    req.Abort();
                    req = null;
                }
            }
        }

        /// <summary>
        /// 进度改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            fileIndex += 1;
            DownloadFile();
        }
    }
}

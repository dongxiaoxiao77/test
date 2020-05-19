using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using UpdateFileCommon;

namespace UpdateFileServer
{
    public partial class ConfigForm : Form, INotifyPropertyChanged
    {
        private BindingList<UpdateFileInfo> _fileList;

        /// <summary>
        /// 文件集合
        /// </summary>
        public BindingList<UpdateFileInfo> FileList
        {
            get { return _fileList; }
            set { _fileList = value; ChangedProperty("FileList"); }
        }

        public ConfigForm()
        {
            InitializeComponent();
            this.Load += delegate
            {
                FileList = new BindingList<UpdateFileInfo>();
                var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "UpdateServer.xml";
                //获取所有文件，封装成集合
                var nodeList = XMLHelper.GetXmlNodeListByXpath(xmlPath, "/Config/File/Item");

                int tryInt = 0;
                foreach (XmlNode item in nodeList)
                {
                    var name = XMLHelper.GetNodeAttributeValue(item, "Name");
                    int.TryParse(XMLHelper.GetNodeAttributeValue(item, "Version"), out tryInt);
                    var version = tryInt;
                    var date = XMLHelper.GetNodeAttributeValue(item, "Date");
                    FileList.Add(new UpdateFileInfo() { Name = name, Version = version, Date = date });
                }
                this.dataGridView1.DataBindings.Add("DataSource", this, "FileList");

                //获取服务器URL
                var serverurl = XMLHelper.GetXmlNodeValueByXpath(xmlPath, "/Config/ServerURL");
                this.txtServerURL.Text = serverurl;
                //获取版本信息
                var vernode = XMLHelper.GetXmlNodeByXpath(xmlPath, "/Config/Version/Item");
                var ver = XMLHelper.GetNodeAttributeValue(vernode, "Version");
                var vervalue = XMLHelper.GetNodeAttributeValue(vernode, "VersionValue");
                this.lblVersionValue.Text = vervalue;
                this.txtVersion.Text = ver;
                //this.lblk
                //获取更新描述
                var releaseNote = XMLHelper.GetXmlNodeValueByXpath(xmlPath, "/Config/Version/Item/ReleaseNote");
                this.txtDescription.Text = releaseNote;
                //获取是否强制更新
                var isMustUpdate = XMLHelper.GetXmlNodeValueByXpath(xmlPath, "/Config/Version/Item/IsMustUpdate");
                this.chkIsMustUpdate.Checked = isMustUpdate.ToUpper() == "TRUE";
                //获取安装包版本
                var setupNode = XMLHelper.GetXmlNodeByXpath(xmlPath, "/Config/Setup");
                this.txtSetupURL.Text = setupNode.InnerText;
                this.txtSetupVersion.Text = XMLHelper.GetNodeAttributeValue(setupNode, "Version");
            };

            this.btnAdd.Click += delegate
            {
                ShowSelectFile();
            };

            this.btnApply.Click += delegate
            {
                var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "UpdateServer.xml";
                //保存设置到XML
                var xmlText = "";
                foreach (var item in FileList)
                {
                    xmlText += String.Format("<Item Name=\"{0}\" Version=\"{1}\" Date=\"{2}\"></Item>", item.Name, item.Version, item.Date);
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath); //加载XML文档
                XMLHelper.CreateOrUpdateXmlNodeByXPath(xmlDoc, "/Config", "File", xmlText);
                XMLHelper.CreateOrUpdateXmlNodeByXPath(xmlDoc, "/Config", "ServerURL", this.txtServerURL.Text);
                XMLHelper.CreateOrUpdateXmlNodeByXPath(xmlDoc, "/Config", "Setup", this.txtSetupURL.Text);
                XMLHelper.CreateOrUpdateXmlAttributeByXPath(xmlDoc, "/Config/Setup", "Version", this.txtSetupVersion.Text);
                XMLHelper.CreateOrUpdateXmlNodeByXPath(xmlDoc, "/Config/Version/Item", "ReleaseNote", this.txtDescription.Text);
                XMLHelper.CreateOrUpdateXmlAttributeByXPath(xmlDoc, "/Config/Version/Item", "Version", this.txtVersion.Text);
                XMLHelper.CreateOrUpdateXmlAttributeByXPath(xmlDoc, "/Config/Version/Item", "VersionValue", this.lblVersionValue.Text);
                XMLHelper.CreateOrUpdateXmlNodeByXPath(xmlDoc, "/Config/Version/Item", "IsMustUpdate", this.chkIsMustUpdate.Checked.ToString());
                xmlDoc.Save(xmlPath);
                MessageBox.Show("UpdateServer.xml 文件已更新", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.dataGridView1.CellValueChanged += (sender, e) =>
            {
                int verValue = 0;
                int.TryParse(lblVersionValue.Text, out verValue);
                lblVersionValue.Text = (verValue + 1).ToString();
            };

            #region 右键菜单
            this.menuAdd.Click += delegate { ShowSelectFile(); };
            this.menuEdit.Click += delegate
            {
                if (this.dataGridView1.SelectedRows.Count > 0)
                    this.dataGridView1.BeginEdit(false);
            };
            this.menuDelete.Click += delegate
            {
                DialogResult dr = MessageBox.Show("确定要删除吗？", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                    this.dataGridView1.Rows.Remove(this.dataGridView1.SelectedRows[0]);
            };
            #endregion
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        private void ShowSelectFile()
        {
            var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            //弹窗选择文件
            OpenFileDialog file = new OpenFileDialog();
            file.InitialDirectory = currentPath;
            file.Multiselect = true;
            DialogResult rs = file.ShowDialog();
            if (rs == DialogResult.OK)
            {
                var files = file.FileNames;
                //判断选择的文件路径是否是当前运行程序中的文件夹路径
                for (int i = 0; i < files.Length; i++)
                {
                    //添加到集合
                    if (files[i].Contains(currentPath))
                    {
                        //添加
                        FileList.Insert(0, new UpdateFileInfo() { Name = file.SafeFileNames[i], Version = 1, Date = DateTime.Now.ToString("yyyy-MM-dd") });
                    }
                    else
                        MessageBox.Show(String.Format("所选 {0} 文件在当前程序所在目录，请复制到当前程序所在目录", file.SafeFileNames[i]), "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知属性已改变
        /// </summary>
        /// <param name="propertyName"></param>
        public void ChangedProperty(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

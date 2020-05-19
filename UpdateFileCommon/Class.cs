using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UpdateFileCommon
{
    public class UpdateFileInfo:INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; ChangedProperty("Name"); }
        }

        private int _version;

        public int Version
        {
            get { return _version; }
            set { _version = value; ChangedProperty("Version"); }
        }


        private string _date;

        public string Date
        {
            get { return _date; }
            set { _date = value; ChangedProperty("Date"); }
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

    public class VersionInfo
    {
        public string Version { get; set; }
        public int VersionValue { get; set; }
        public string ReleaseNote { get; set; }
        public bool IsMustUpdate { get; set; }
    }

    public class LocalModel
    {
        public string ServerURL { get; set; }
        public string LocalVersion { get; set; }
        public int VersionValue { get; set; }
    }
}

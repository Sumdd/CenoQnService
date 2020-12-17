using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    public class m_cFile
    {
        /// <summary>
        /// 序号
        /// </summary>
        public string sno
        {
            get; set;
        }
        /// <summary>
        /// 证件号MD5
        /// </summary>
        public string cid
        {
            get; set;
        }
        /// <summary>
        /// 用户编号
        /// </summary>
        public string username
        {
            get; set;
        }
    }

    public class m_cAudio
    {
        public string sessionId { get; set; }
        public string audioUrl { get; set; }
        public string audioType { get; set; }
    }
}
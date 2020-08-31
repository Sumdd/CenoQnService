using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    public class m_cCmn
    {
        /// <summary>
        /// ID
        /// </summary>
        public static string ID
        {
            get
            {
                return $"{DateTime.Now.ToString(m_cCfg.DATE_PATTERN)}{Guid.NewGuid().ToString().Replace("-", "")}";
            }
        }
        /// <summary>
        /// UUID
        /// </summary>
        public static string UUID(string entID)
        {
            if (entID == null) return null;
            return $"{entID}{DateTime.Now.ToString(m_cCfg.DATE_PATTERN)}{Guid.NewGuid().ToString().Substring(0, 3)}";
        }

        /// <summary>
        /// 续联UUID
        /// </summary>
        public static string XxUUID
        {
            get
            {
                return $"{m_cXxCfg.entID}{DateTime.Now.ToString(m_cCfg.DATE_PATTERN)}{Guid.NewGuid().ToString().Substring(0, 3)}";
            }
        }
        /// <summary>
        /// 续联UUID
        /// </summary>
        public static string CcUUID
        {
            get
            {
                return $"{m_cCcCfg.entID}{DateTime.Now.ToString(m_cCfg.DATE_PATTERN)}{Guid.NewGuid().ToString().Substring(0, 3)}";
            }
        }
    }
}
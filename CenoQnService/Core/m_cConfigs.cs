using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    /// <summary>
    /// 通用Config
    /// </summary>
    public class m_cCfg
    {
        /*
         * 系统编码: UTF-8
         */
        public const string SYSTEM_ENCODING = "UTF-8";
        /*
         * 请求时间格式
         */
        public const string DATE_PATTERN = "yyyyMMddHHmmss";
    }
    /// <summary>
    /// 续联Config
    /// </summary>
    public class m_cXxCfg
    {
        /*
         * 接口地址前缀
         */
        public static string QN_API_URL = m_cGcfg.m_fGetAppSetting("m_cXxCfg[QN_API_URL]");
        /*
         * 企业编号
         */
        public static string entID = m_cGcfg.m_fGetAppSetting("m_cXxCfg[entID]");
        /*
         * 企业安全码
         */
        public static string entSecret = m_cGcfg.m_fGetAppSetting("m_cXxCfg[entSecret]");
    }
    /// <summary>
    /// 呼叫中心Config
    /// </summary>
    public class m_cCcCfg
    {
        /*
         * 接口地址前缀
         */
        public static string QN_API_URL = m_cGcfg.m_fGetAppSetting("m_cCcCfg[QN_API_URL]");
        /*
         * 企业编号
         */
        public static string entID = m_cGcfg.m_fGetAppSetting("m_cCcCfg[entID]");
        /*
         * 企业安全码
         */
        public static string entSecret = m_cGcfg.m_fGetAppSetting("m_cCcCfg[entSecret]");
    }

    public class m_cGcfg
    {
        public static string m_fGetAppSetting(string m_sKey)
        {
            try
            {
                return ConfigurationManager.AppSettings.Get(m_sKey);
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cGcfg][m_fGetAppSetting][Exception][键:{m_sKey},Get时错误:{ex.Message}]");
                return string.Empty;
            }
        }
    }
}
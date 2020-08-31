using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    /// <summary>
    /// MySQL语法糖
    /// </summary>
    public class m_cSugar
    {
        public m_cSugar(string sConNameStr = null)
        {
            string sConStr = string.Empty;

            if (!string.IsNullOrWhiteSpace(sConNameStr))
            {
                try
                {
                    sConStr = ConfigurationManager.ConnectionStrings[sConNameStr].ToString();
                }
                catch (Exception ex) { }
            }

            if (string.IsNullOrWhiteSpace(sConStr))
            {
                sConStr = sConNameStr;
            }

            if (string.IsNullOrWhiteSpace(sConStr))
            {
                try
                {
                    sConStr = ConfigurationManager.ConnectionStrings[(ConfigurationManager.ConnectionStrings.Count - 1)].ToString();
                }
                catch (Exception ex) { }
            }

            if (string.IsNullOrWhiteSpace(sConStr))
            {
                throw new Exception("请配置正确的连接字符串");
            }

            this.EasyClient = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = sConStr,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true,
            });
        }
        public m_cSugar(ConnectionConfig config)
        {
            this.EasyClient = new SqlSugarClient(config);
        }
        public SqlSugarClient EasyClient { get; set; }
    }
}
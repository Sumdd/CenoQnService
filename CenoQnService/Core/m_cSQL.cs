using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace CenoQnService
{
    public class m_cSQL
    {
        public static bool m_fSaveReqFile(string RespMsg, int RespState, string RequestID, string ReqJson)
        {
            try
            {
                string m_sSQL = $@"
INSERT INTO [dbo].[call_repair]
(
    [Id],
    [RespMsg],
    [RespState],
    [RequestID],
    [ReqJson]
)
VALUES
(@Id, @RespMsg, @RespState, @RequestID, @ReqJson);
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                int m_uCount = m_pEsyClient.Ado.ExecuteCommand(m_sSQL, new
                {
                    Id = m_cCmn.ID,
                    RespMsg = RespMsg,
                    RespState = RespState,
                    RequestID = RequestID,
                    ReqJson = ReqJson
                });
                return m_uCount > 0;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fSaveReqFile][Exception][{ex.Message}]");
            }
            return false;
        }

        public static bool m_fSaveReqInfo(string RequestID, DataTable m_pDataTable)
        {
            try
            {
                if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                {
                    List<string> m_lSQL = new List<string>();
                    foreach (DataRow item in m_pDataTable.Rows)
                    {
                        string m_sSQL = $@"
INSERT INTO [dbo].[call_repair_info]
(
    [Id],
    [Xm],
    [Ywy],
    [Shfzh],
    [sno],
    [requestID],
    [username]
)
VALUES
('{m_cCmn.ID}', '{item["Xm"]}', '{item["Ywy"]}', '{item["cid"]}', '{item["sno"]}', @RequestID, '{item["username"]}');
";
                        m_lSQL.Add(m_sSQL);
                    }
                    SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                    int m_uCount = m_pEsyClient.Ado.ExecuteCommand(string.Join("", m_lSQL), new
                    {
                        RequestID = RequestID
                    });
                    return m_uCount > 0;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fSaveReqInfo][Exception][{ex.Message}]");
            }
            return false;
        }

        public static DataTable m_fQueryList()
        {
            try
            {
                string m_sSQL = $@"
SELECT *
FROM call_repair WITH (NOLOCK)
WHERE 1 = 1
      AND ISNULL(IsDel, 0) = 0
      AND ISNULL(RespState, 2) IN ( 1, -201 )
      AND
      (
          UpdateTime <= '{DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss")}'
          OR UpdateTime IS NULL
      );
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                return m_pDataTable;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fQueryList][Exception][{ex.Message}]");
            }
            return null;
        }

        public static bool m_fSaveRecord(string sessionId)
        {
            try
            {
                string m_sSQL = $@"
INSERT INTO [dbo].[call_repair_record]
(
    [sessionId],
    [auto_status]
)
VALUES
(@sessionId, 0);
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                int m_uCount = m_pEsyClient.Ado.ExecuteCommand(m_sSQL, new
                {
                    sessionId = sessionId
                });
                return m_uCount > 0;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fSaveRecord][Exception][{ex.Message}]");
            }
            return false;
        }

        public static DataTable m_fRecordList()
        {
            try
            {
                string m_sSQL = $@"
SELECT TOP 100
    call_repair_record.sessionId
FROM call_repair_record WITH (NOLOCK)
WHERE 1 = 1
      AND ISNULL(IsDel, 0) = 0
      AND ISNULL(auto_status, 0) IN ( 0, 2 )
      AND
      (
          UpdateTime <= '{DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss")}'
          OR UpdateTime IS NULL
      );
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                return m_pDataTable;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fRecordList][Exception][{ex.Message}]");
            }
            return null;
        }

        public static DataTable m_fGetRecUrl(string sessionIds)
        {
            try
            {
                string m_sSQL = $@"
SELECT TOP 100
    call_repair_record.sessionId
FROM call_repair_record WITH (NOLOCK)
WHERE 1 = 1
      AND ISNULL(IsDel, 0) = 0
      AND ISNULL(auto_status, 0) IN ( 0, 2 )
      AND
      (
          UpdateTime <= '{DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss")}'
          OR UpdateTime IS NULL
      );
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                return m_pDataTable;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fRecordList][Exception][{ex.Message}]");
            }
            return null;
        }

        public static bool m_fLogin(string username, string localpwd, out string userpwd)
        {
            userpwd = null;
            try
            {
                string m_sSQL = $@"
SELECT userpwd
FROM call_repair_user WITH (NOLOCK)
WHERE username = @username
      AND localpwd = @localpwd
      AND ISNULL(IsDel, 0) = 0;
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL, new
                {
                    username = username,
                    localpwd = localpwd
                });
                if (m_pDataTable != null && m_pDataTable.Rows.Count == 1)
                {
                    userpwd = m_pDataTable.Rows[0]["userpwd"].ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fSaveRecord][Exception][{ex.Message}]");
            }
            return false;
        }

        private static string _m_sAgentID;
        public static string m_sAgentID
        {
            get
            {
                try
                {
                    if (_m_sAgentID == null)
                    {
                        string m_sSQL = $@"
SELECT TOP 1
    username
FROM call_repair_user WITH (NOLOCK)
ORDER BY username ASC;
";
                        SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                        DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                        if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                        {
                            _m_sAgentID = m_pDataTable.Rows[0]["username"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Debug($"[CenoQnService][m_cSQL][m_sAgentID][Exception][{ex.Message}]");
                }
                return _m_sAgentID;
            }
        }

        private static List<string> _m_lAgentID;
        public static List<string> m_lAgentID
        {
            get
            {
                try
                {
                    if (_m_lAgentID == null)
                    {
                        string m_sSQL = $@"
SELECT
    username
FROM call_repair_user WITH (NOLOCK)
ORDER BY username ASC;
";
                        SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                        DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                        if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                        {
                            _m_lAgentID = m_pDataTable.AsEnumerable().Select(x => x.Field<object>("username")?.ToString())?.ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Debug($"[CenoQnService][m_cSQL][m_lAgentID][Exception][{ex.Message}]");
                }
                return _m_lAgentID;
            }
        }
        public static bool m_bHasAgentID(string m_aAgentID)
        {
            try
            {
                if (_m_lAgentID == null)
                {
                    _m_lAgentID = m_lAgentID;
                }
                if (_m_lAgentID != null && _m_lAgentID.Count > 0 && _m_lAgentID.Contains(m_aAgentID))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_bHasAgentID][Exception][{ex.Message}]");
            }
            return false;
        }

        public static bool m_fSetDialRecord(string number, string m_sHost)
        {
            try
            {
                string m_sSQL = $@"
INSERT INTO [dbo].[call_repair_dial]
(
    [dialno],
    [dialtime],
    [dialip]
)
VALUES
(@number, GETDATE(), @m_sHost);
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                int m_uCount = m_pEsyClient.Ado.ExecuteCommand(m_sSQL, new
                {
                    number = number,
                    m_sHost = m_sHost
                });
                return m_uCount > 0;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fSetDialRecord][Exception][{ex.Message}]");
            }
            return false;
        }

        public static DataTable m_fQueryRecordDownload()
        {
            try
            {
                ///保存路径非空
                if (string.IsNullOrWhiteSpace(m_cSQL.m_sSaveRecordPath)) throw new Exception("录音保存路径非空");

                string m_sSQL = $@"
SELECT TOP 100
    call_repair_record.sessionId,
    call_repair_record.startTime,
    call_repair_record.talkDuration
FROM call_repair_record WITH (NOLOCK)
WHERE 1 = 1
      AND ISNULL(IsDel, 0) = 0
      AND ISNULL(auto_status, 0) IN ( 1, 5 )
      AND
      (
          UpdateTime <= '{DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss")}'
          OR UpdateTime IS NULL
      );
";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                DataTable m_pDataTable = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                return m_pDataTable;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fQueryRecordDownload][Exception][{ex.Message}]");
            }
            return null;
        }

        public static bool m_fQueryRepeat(DataTable m_pDataTable, out DataTable m_pSheet1, out string m_sErrMsg)
        {
            m_pSheet1 = null;
            m_sErrMsg = string.Empty;
            try
            {
                using (SqlSugarClient m_pEsyClient = new m_cSugar(null, false).EasyClient)
                {
                    m_pEsyClient.Ado.CommandTimeOut = 0;
                    m_pEsyClient.Open();

                    ///创建临时表
                    m_pEsyClient.Ado.ExecuteCommand(m_cSQL.m_fCreateTempTable(m_pDataTable, true));

                    ///先写入临时表
                    SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)(m_pEsyClient.Ado.Connection), SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints, null);
                    bulkCopy.BulkCopyTimeout = int.MaxValue;
                    bulkCopy.BatchSize = 800;//经过实验获得的最快的数值
                    bulkCopy.DestinationTableName = "#tUserData";
                    bulkCopy.WriteToServer(m_pDataTable);
                    bulkCopy.Close();

                    string m_sSQL = $@"
SELECT T0.*,
       CASE
           WHEN T2.N1 > 1 THEN
               CONCAT('重复', T2.N1, '次')
           ELSE
               NULL
       END AS 'Excel_Err',
       CASE
           WHEN T3.N2 > 0 THEN
               CONCAT('已传', T3.N2, '次:', T3.R2)
           ELSE
               CONVERT(VARCHAR(20), T3.N2)
       END AS 'SQLDB_Err'
FROM [#tUserData] T0 WITH (NOLOCK)
    OUTER APPLY
(
    SELECT COUNT(1) AS N1
    FROM [#tUserData] T1 WITH (NOLOCK)
    WHERE T1.[cid] = T0.[cid]
) T2
    OUTER APPLY
(
    SELECT COUNT(1) AS N2,
           MAX([dbo].[call_repair_info].[requestID]) AS R2
    FROM [dbo].[call_repair_info] WITH (NOLOCK)
        LEFT JOIN [dbo].[call_repair] WITH (NOLOCK)
            ON [dbo].[call_repair_info].[requestID] = [dbo].[call_repair].[RequestID]
    WHERE ISNULL([dbo].[call_repair_info].[IsDel], 0) = 0
          AND ISNULL([dbo].[call_repair].[IsDel], 0) = 0
          AND [dbo].[call_repair_info].[Shfzh] = T0.[cid]
    GROUP BY [dbo].[call_repair_info].[Shfzh]
) T3;
DROP TABLE [#tUserData];
";
                    ///得到新结果表
                    DataTable Sheet1 = m_pEsyClient.Ado.GetDataTable(m_sSQL);
                    if (Sheet1 != null && Sheet1.Rows.Count > 0)
                    {
                        DataRow[] m_lDataRow = Sheet1.Select($" ISNULL([Excel_Err],'') <> '' OR ISNULL([SQLDB_Err],'') <> '' ");
                        if (m_lDataRow != null && m_lDataRow.Count() > 0)
                        {
                            m_pSheet1 = m_lDataRow.CopyToDataTable();
                            m_sErrMsg = "含重复数据";
                            return true;
                        }
                        else
                        {
                            m_sErrMsg = "成功";
                            return false;
                        }
                    }
                    else
                    {
                        m_sErrMsg = "数据结果集非空";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fQueryRepeat][Exception][{ex.Message}]");
                m_sErrMsg = ex.Message;
                return true;
            }
        }

        #region ***数据库参数

        #region ***录音下载Http地址
        private static string _m_sSaveRecordHttp;
        public static string m_sSaveRecordHttp
        {
            get
            {
                if (_m_sSaveRecordHttp == null) _m_sSaveRecordHttp = m_cSQL.m_fGetPValue("m_sSaveRecordHttp");
                return _m_sSaveRecordHttp;
            }
        }
        private static string _m_sSaveRecordHttpWithoutIP;
        public static string m_sSaveRecordHttpWithoutIP
        {
            get
            {
                if (_m_sSaveRecordHttpWithoutIP == null)
                {
                    ///置换IP
                    Regex m_pRegex = new Regex(@"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)");
                    _m_sSaveRecordHttpWithoutIP = m_pRegex.Replace(m_sSaveRecordHttp, "##IP##");
                    return _m_sSaveRecordHttpWithoutIP;
                }
                return _m_sSaveRecordHttpWithoutIP;
            }
        }
        #endregion

        #region ***录音下载绝对路径
        private static string _m_sSaveRecordPath;
        public static string m_sSaveRecordPath
        {
            get
            {
                if (_m_sSaveRecordPath == null) _m_sSaveRecordPath = m_cSQL.m_fGetPValue("m_sSaveRecordPath");
                return _m_sSaveRecordPath;
            }
        }
        #endregion

        #region ***参数获取方法
        private static string m_fGetPValue(string m_sPCode)
        {
            try
            {
                string m_sSQL = $@"SELECT call_repair_p.pvalue
FROM call_repair_p WITH (NOLOCK)
WHERE call_repair_p.pcode = @m_sPCode;";
                SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                return m_pEsyClient.Ado.GetString(m_sSQL, new
                {
                    m_sPCode = m_sPCode
                });
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fGetPValue][Exception][{ex.Message}]");
            }
            return null;
        }
        #endregion

        #region ***静态变量重载
        public static void m_fReload()
        {
            try
            {
                m_cSQL._m_sSaveRecordHttp = null;
                m_cSQL._m_sSaveRecordHttpWithoutIP = null;
                m_cSQL._m_sSaveRecordPath = null;
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fReload][清理静态变量]");
            }
            catch (Exception ex)
            {
                Log.Instance.Debug($"[CenoQnService][m_cSQL][m_fReload][Exception][{ex.Message}]");
            }
        }
        #endregion

        #endregion

        #region ***创建临时表
        public static string m_fCreateTempTable(DataTable dtUserData, bool m_bString)
        {
            string createTempTablesql = @"CREATE TABLE #tUserData
                                            (
                                                {0}
                                            );";
            Dictionary<Type, string> dataDictionary = new Dictionary<Type, string>();
            dataDictionary[typeof(decimal)] = dataDictionary[typeof(double)] = " decimal(18,6) ";
            dataDictionary[typeof(int)] = dataDictionary[typeof(long)] = " bigint ";
            dataDictionary[typeof(string)] = " nvarchar(max) ";
            dataDictionary[typeof(DateTime)] = " DateTime ";
            dataDictionary[typeof(object)] = " nvarchar(max) ";

            List<string> colSqlList = new List<string>();
            foreach (DataColumn col in dtUserData.Columns)
            {
                string colDataType = dataDictionary[col.DataType];
                if (col.DataType != typeof(DateTime) && m_bString) colDataType = " nvarchar(max) ";

                string colSql = string.Format(@" [{0}] {1} ", col.ColumnName, colDataType);
                colSqlList.Add(colSql);
            }
            createTempTablesql = string.Format(createTempTablesql,
                string.Join(",", colSqlList));
            return createTempTablesql;
        }
        #endregion
    }
}
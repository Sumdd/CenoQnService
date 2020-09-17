using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;
using System.Data;
using System.Collections;

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
                    if (_m_sAgentID == null)
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
    }
}
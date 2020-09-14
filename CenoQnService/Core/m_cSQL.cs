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
    }
}
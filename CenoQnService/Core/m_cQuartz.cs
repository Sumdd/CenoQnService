using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using System.Data;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace CenoQnService
{
    [DisallowConcurrentExecution]
    public class m_cQuartzJob : IJob
    {
        private static bool m_bJobDoing = false;
        public void Execute(IJobExecutionContext context)
        {
            if (!m_bJobDoing)
            {
                try
                {
                    m_bJobDoing = true;

                    ///查询
                    DataTable m_pDataTable = m_cSQL.m_fQueryList();

                    if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                    {
                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][执行任务,{m_pDataTable?.Rows?.Count}]");

                        List<string> m_lUpd = new List<string>();
                        List<string> m_lIns = new List<string>();

                        foreach (DataRow m_pDataRow in m_pDataTable.Rows)
                        {
                            int status = -1;
                            string msg = "";
                            int count = 0;
                            ///获取续联结果名单URL
                            string url = $"{m_cXxCfg.QN_API_URL}/trace/retrieve/list";
                            ///entID企业编号
                            string entID = m_cXxCfg.entID;
                            ///entSecret企业安全码
                            string entSecret = m_cXxCfg.entSecret;
                            ///requestID非空
                            string requestID = m_pDataRow["RequestID"].ToString();
                            ///参数构造
                            IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                            m_pDic["entID"] = entID;
                            m_pDic["entSecret"] = entSecret;
                            m_pDic["requestID"] = requestID;
                            m_pDic["page"] = 1;
                            m_pDic["size"] = 1000;

                            ///发送请求
                            string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                            if (!string.IsNullOrWhiteSpace(m_sResultString))
                            {
                                JObject m_pJObject = JObject.Parse(m_sResultString);
                                status = Convert.ToInt32(m_pJObject["code"]);
                                msg = m_pJObject["msg"].ToString();
                                if (status == 0)
                                {
                                    count = Convert.ToInt32(m_pJObject["total"]);
                                    if (count > 0)
                                    {
                                        JToken m_pJToken = m_pJObject["callList"];
                                        foreach (JToken item in m_pJToken)
                                        {
                                            ///内部定义变量
                                            var sno = string.Empty;
                                            var tag = Convert.ToInt32(item["tag"]);
                                            var cid = string.Empty;
                                            var extendColumn = string.Empty;
                                            var serialNO = string.Empty;
                                            var hostNum = string.Empty;

                                            if (tag == 1)
                                            {
                                                JToken _m_pJToken = item["rsno"];
                                                foreach (JToken _item in _m_pJToken)
                                                {
                                                    sno = item["sno"]?.ToString();
                                                    cid = item["cid"]?.ToString();
                                                    extendColumn = item["extendColumn"]?.ToString();
                                                    serialNO = _item["serialNO"]?.ToString();
                                                    hostNum = _item["hostNum"]?.ToString();
                                                }
                                            }
                                            else
                                            {
                                                sno = item["sno"]?.ToString();
                                                cid = item["cid"]?.ToString();
                                                extendColumn = item["extendColumn"]?.ToString();
                                            }

                                            m_lIns.Add($@"
INSERT INTO [dbo].[call_repair_list]
           (
            [Id]
           ,[sno]
           ,[cid]
           ,[tag]
           ,[extendColumn]
           ,[serialNO]
           ,[hostNum]
           ,[requestID])
     VALUES
           (
            '{m_cCmn.ID}'
           ,'{sno}'
           ,'{cid}'
           ,'{tag}'
           ,'{extendColumn}'
           ,'{serialNO}'
           ,'{hostNum}'
           ,'{requestID}');
");
                                        }
                                    }
                                    m_lUpd.Add($@"
DELETE FROM [dbo].[call_repair_list]
WHERE [requestID] = '{requestID}';
UPDATE call_repair
	  SET RespState = 3,
          RespMsg = '{msg}',
          UpdateTime = GETDATE()
	  WHERE RequestID = '{requestID}';
");
                                }
                                else
                                {
                                    m_lUpd.Add($@"
UPDATE call_repair
	  SET RespState = {(status == -201 ? status : 4)},
          RespMsg = '{msg}',
          UpdateTime = GETDATE() 
	  WHERE RequestID = '{requestID}';
");
                                }
                            }
                            else
                            {
                                m_lUpd.Add($@"
UPDATE call_repair
	  SET RespState = 4,
          RespMsg = '请求无返回',
          UpdateTime = GETDATE() 
	  WHERE RequestID = '{requestID}';
");
                            }
                        }
                        ///执行插入操作即可
                        string m_sSQL = $@"
{(string.Join("", m_lUpd))}
{(string.Join("", m_lIns))}
";
                        SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                        m_pEsyClient.Ado.ExecuteCommand(m_sSQL);
                    }
                    else
                    {
                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][无任务]");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Debug(ex);
                }
                finally
                {
                    m_bJobDoing = false;
                }
            }
            else
            {
                Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][任务执行中,跳出]");
            }
        }
    }

    public class m_cQuartzJobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<m_cQuartzJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("qr", "gq")
              .WithSimpleSchedule(t =>
                t.WithIntervalInSeconds(60)
                 .RepeatForever())
                 .Build();
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
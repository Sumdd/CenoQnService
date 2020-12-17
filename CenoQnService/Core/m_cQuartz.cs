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

                    #region ***查询续联结果
                    try
                    {
                        ///查询
                        DataTable m_pDataTable = m_cSQL.m_fQueryList();

                        if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                        {
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][执行续联结果获取任务,{m_pDataTable?.Rows?.Count}]");

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
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][无续联任务]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][续联任务][Exception][{ex.Message}]");
                    }
                    #endregion

                    #region ***获取明细
                    try
                    {
                        DataTable m_pRecord = m_cSQL.m_fRecordList();

                        if (m_pRecord != null && m_pRecord.Rows.Count > 0)
                        {
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][执行明细获取任务,{m_pRecord?.Rows?.Count}]");

                            List<string> m_lUpd = new List<string>();

                            int status = -1;
                            string msg = "";
                            ///获取续联结果名单URL
                            string url = $"{m_cCcCfg.QN_API_URL}/agent/dps/query";
                            ///entID企业编号
                            string entID = m_cXxCfg.entID;
                            ///entSecret企业安全码
                            string entSecret = m_cXxCfg.entSecret;
                            ///requestID非空
                            string requestID = m_cCmn.UUID(entID);
                            ///参数构造
                            IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                            m_pDic["entID"] = entID;
                            m_pDic["entSecret"] = entSecret;
                            m_pDic["requestID"] = requestID;
                            m_pDic["sessionIds"] = string.Join(",", m_pRecord.AsEnumerable().Select(x => x.Field<object>("sessionId").ToString()));

                            ///发送请求
                            string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                            if (!string.IsNullOrWhiteSpace(m_sResultString))
                            {
                                JObject m_pJObject = JObject.Parse(m_sResultString);
                                status = Convert.ToInt32(m_pJObject["code"]);
                                msg = m_pJObject["msg"].ToString();
                                if (status == 0)
                                {
                                    JToken m_pJToken = m_pJObject["data"];
                                    foreach (JToken item in m_pJToken)
                                    {
                                        m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [username] = '{item["agentId"]}',
    [userData] = '{item["userData"]}',
    [ani] = '{item["ani"]}',
    [dnis] = '{item["dnis"]}',
    [dani] = '{item["dani"]}',
    [ddnis] = '{item["ddnis"]}',
    [startTime] = '{item["startTime"]}',
    [endTime] = '{item["endTime"]}',
    [callResult] = {item["callResult"]},
    [alertDuration] = {item["alertDuration"]},
    [talkDuration] = {item["talkDuration"]},
    [endType] = {item["endType"]},
    [UpdateTime] = GETDATE(),
    [auto_status] = 1
WHERE [sessionId] = '{item["sessionId"]}';
");
                                    }
                                }
                                else
                                {
                                    m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [UpdateTime] = GETDATE(),
    [auto_status] = 2,
    [auto_err] = '{msg}'
WHERE [sessionId] IN ('{string.Join("','", m_pRecord.AsEnumerable().Select(x => x.Field<object>("sessionId").ToString()))}');
");
                                }
                            }
                            else
                            {
                                m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [UpdateTime] = GETDATE(),
    [auto_status] = 2,
    [auto_err] = '请求无返回'
WHERE [sessionId] IN ('{string.Join("','", m_pRecord.AsEnumerable().Select(x => x.Field<object>("sessionId").ToString()))}');
");
                            }
                            ///执行语句
                            SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                            m_pEsyClient.Ado.ExecuteCommand(string.Join("", m_lUpd));
                        }
                        else
                        {
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][无明细任务]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][明细任务][Exception][{ex.Message}]");
                    }
                    #endregion

                    #region ***录音下载
                    try
                    {
                        DataTable m_pRecordDownload = m_cSQL.m_fQueryRecordDownload();

                        if (m_pRecordDownload != null && m_pRecordDownload.Rows.Count > 0)
                        {
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][执行录音下载任务,{m_pRecordDownload?.Rows?.Count}]");

                            List<string> m_lUpd = new List<string>();

                            int status = -1;
                            string msg = "";
                            ///获取续联结果名单URL
                            string url = $"{m_cCcCfg.QN_API_URL}/agent/audio/query";
                            ///entID企业编号
                            string entID = m_cXxCfg.entID;
                            ///entSecret企业安全码
                            string entSecret = m_cXxCfg.entSecret;
                            ///requestID非空
                            string requestID = m_cCmn.UUID(entID);
                            ///参数构造
                            IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                            m_pDic["entID"] = entID;
                            m_pDic["entSecret"] = entSecret;
                            m_pDic["requestID"] = requestID;
                            m_pDic["sessionIds"] = string.Join(",", m_pRecordDownload.AsEnumerable().Select(x => x.Field<object>("sessionId").ToString()));

                            ///录音缓存
                            List<m_cAudio> m_lAudio = new List<m_cAudio>();

                            ///发送请求
                            string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                            if (!string.IsNullOrWhiteSpace(m_sResultString))
                            {
                                JObject m_pJObject = JObject.Parse(m_sResultString);
                                status = Convert.ToInt32(m_pJObject["code"]);
                                msg = m_pJObject["msg"].ToString();
                                if (status == 0)
                                {
                                    JToken m_pJToken = m_pJObject["data"];
                                    foreach (JToken item in m_pJToken)
                                    {
                                        m_cAudio m_mAudio = new m_cAudio();
                                        m_mAudio.sessionId = item["sessionId"]?.ToString();
                                        m_mAudio.audioUrl = item["audioUrl"]?.ToString();
                                        m_mAudio.audioType = item["audioType"]?.ToString();
                                        m_lAudio.Add(m_mAudio);
                                    }
                                }
                            }

                            ///循环下载录音
                            foreach (DataRow m_pDataRow in m_pRecordDownload.Rows)
                            {
                                ///会话ID
                                string sessionId = m_pDataRow["sessionId"].ToString();
                                ///通话时长
                                int talkDuration = Convert.ToInt32(m_pDataRow["talkDuration"]);
                                ///开始时间
                                string startTime = m_pDataRow["startTime"].ToString();

                                ///查询是否有次会话ID的录音信息
                                m_cAudio m_mAudio = m_lAudio.Where(x => x.sessionId == sessionId)?.FirstOrDefault();
                                if (m_mAudio != null)
                                {
                                    ///录音下载路径
                                    string audioUrl = m_mAudio.audioType;

                                    ///是否下载成功
                                    bool m_bRecLoad = false;
                                    string m_sSavePuffixFileName = string.Empty;
                                    string errMsg = string.Empty;

                                    #region ***直接下载录音并保存
                                    try
                                    {
                                        using (System.IO.Stream m_pRecIDResult = m_cHttp.HttpGetStream(audioUrl))
                                        {
                                            if (m_pRecIDResult != null)
                                            {
                                                m_pRecIDResult.Position = 0;
                                                //创建对应文件,改变文件名称,形如Rec_20200202;
                                                DateTime m_dtDateTime = Convert.ToDateTime(startTime);
                                                string m_sFileName = $"Rec_{m_dtDateTime.ToString("yyyyMMddHHmmss")}_{System.IO.Path.GetFileName(audioUrl)}";
                                                m_sSavePuffixFileName = $"/{m_dtDateTime.ToString("yyyy")}/{m_dtDateTime.ToString("yyyyMM")}/{m_dtDateTime.ToString("yyyyMMdd")}/{m_sFileName}";
                                                string m_sSavePath = $"{m_cSQL.m_sSaveRecordPath}{m_sSavePuffixFileName}";
                                                string m_sDirectory = System.IO.Path.GetDirectoryName(m_sSavePath);
                                                //判断文件是否存在
                                                if (!System.IO.File.Exists(m_sSavePath))
                                                {
                                                    if (!System.IO.Directory.Exists(m_sDirectory)) System.IO.Directory.CreateDirectory(m_sDirectory);
                                                    //将流写入文件
                                                    using (var fileStream = System.IO.File.Create(m_sSavePath))
                                                    {
                                                        m_pRecIDResult.CopyTo(fileStream);
                                                    }
                                                    m_bRecLoad = true;
                                                }
                                                else errMsg = "录音文件已存在无需再次下载";
                                            }
                                            else errMsg = "下载流非空";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsg = $"录音下载错误:{ex.Message}";
                                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][for][录音下载][Exception][{ex.Message}]");
                                    }
                                    ///更新下载状态
                                    if (m_bRecLoad)
                                    {
                                        m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [UpdateUserId] = GETDATE(),
    [auto_status] = 3,
    [suffixAudio] = '{m_sSavePuffixFileName}' 
WHERE [sessionId] = '{sessionId}';");
                                    }
                                    else
                                    {
                                        m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [UpdateUserId] = GETDATE(),
    [auto_status] = 5,
    [auto_err] = '{errMsg}'
WHERE [sessionId] = '{sessionId}';");
                                    }
                                    #endregion
                                }
                                else
                                {
                                    m_lUpd.Add($@"
UPDATE [dbo].[call_repair_record]
SET [UpdateUserId] = GETDATE(),
    [auto_status] = (CASE WHEN {talkDuration} <= 0 THEN 4 ELSE 5 END),
    [auto_err] = (CASE WHEN {talkDuration} <= 0 THEN [auto_err] ELSE '无可获取的录音文件' END)
WHERE [sessionId] = '{sessionId}';");
                                }
                            }

                            ///执行语句
                            SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
                            m_pEsyClient.Ado.ExecuteCommand(string.Join("", m_lUpd));
                        }
                        else
                        {
                            Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][无录音下载任务]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.Debug($"[CenoQnService][m_cQuartzJob][Execute][录音下载][Exception][{ex.Message}]");
                    }
                    #endregion
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
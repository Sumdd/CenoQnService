using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Collections;

namespace CenoQnService.Controllers
{
    public class HomeController : Controller
    {
        private int status;
        private string msg;
        private int count;
        private object data;

        #region ***需求名单上传(CSV文件)
        public ActionResult V_1FILE(string queryString)
        {
            ViewBag.Title = "需求名单上传(CSV文件)";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_1FILE(string queryString)
        {
            try
            {
                ///需求名单上传URL
                string url = $"{m_cXxCfg.QN_API_URL}/trace/collect/file";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cXxCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cXxCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///subId非空
                string subId = m_cQuery.m_fGetQueryString(m_lQueryList, "subId");
                if (string.IsNullOrWhiteSpace(subId))
                    throw new ArgumentNullException("subId");
                ///是否保存至本地数据库
                bool isSave = m_cQuery.m_fGetQueryString(m_lQueryList, "isSave") == "1";
                ///需要登录
                string ua = Request.Cookies["ua"]?["agentId"]?.ToString();
                if (isSave && string.IsNullOrWhiteSpace(ua))
                {
                    throw new Exception("请先登录");
                }

                ///是否返回文件的内容JSON
                bool hasJson = m_cQuery.m_fGetQueryString(m_lQueryList, "hasJson") == "1";
                ///CSV压缩文件
                int m_uCount = Request.Files.Count;
                if (m_uCount <= 0)
                    throw new ArgumentNullException("file");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["subId"] = subId;

                ///加载CSV文档
                List<m_cFile> m_lFiles = new List<m_cFile>();
                using (StreamReader reader = new StreamReader(Request.Files[0].InputStream, Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING)))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        m_lFiles = csv.GetRecords<m_cFile>().ToList();
                    }
                }

                ///将文件加载至文档
                int i = 0;
                foreach (m_cFile item in m_lFiles)
                {
                    ///数据唯一标识容错
                    i++;
                    if (string.IsNullOrWhiteSpace(item.sno))
                        item.sno = i.ToString();
                    ///自动MD5身份证
                    item.cid = m_cDigest.m_fMD5(item.cid);
                    ///自动填充坐席登录ID
                    if (string.IsNullOrWhiteSpace(item.username))
                        item.username = ua;
                }

                ///转类至新文件并提交
                byte[] m_lByte;
                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(ms))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(m_lFiles.Select(x => { return new { sno = x.sno, cid = x.cid, username = x.username }; }));
                    writer.Flush();
                    m_lByte = ms.ToArray();
                }

                ///写入压缩
                MemoryStream zipMs = new MemoryStream();
                GZipStream compressedStream = new GZipStream(zipMs, CompressionMode.Compress, true);
                compressedStream.Write(m_lByte, 0, m_lByte.Length);
                compressedStream.Flush();
                compressedStream.Close();

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic, zipMs.ToArray());

                ///需要将结果写入数据库,这里催收系统交付即可
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    #region ***保存请求至本地数据库
                    if (isSave)
                    {
                        int RespState = (status == 0 ? 1 : 2);
                        string ReqJson = (hasJson ? JsonConvert.SerializeObject(m_lFiles) : null);
                        m_cSQL.m_fSaveReqFile(msg, RespState, requestID, ReqJson);
                    }
                    #endregion

                    return rJson(m_cXxCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                    return eJson(m_cXxCfg.entID);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
                return eJson(m_cXxCfg.entID);
            }
        }
        #endregion

        #region ***需求名单上传(JSON转文件)
        public ActionResult V_1JSONTOFILE(string queryString)
        {
            ViewBag.Title = "需求名单上传(JSON转文件)";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_1JSONTOFILE(string queryString)
        {
            try
            {
                Log.Instance.Debug(queryString);
                ///需求名单上传URL
                string url = $"{m_cXxCfg.QN_API_URL}/trace/collect/file";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cXxCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cXxCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///subId非空
                string subId = m_cQuery.m_fGetQueryString(m_lQueryList, "subId");
                if (string.IsNullOrWhiteSpace(subId))
                    throw new ArgumentNullException("subId");
                ///JSON不得为空
                string file = m_cQuery.m_fGetQueryString(m_lQueryList, "file");
                ///是否保存至本地数据库,默认必须保存
                string isSaveStr = m_cQuery.m_fGetQueryString(m_lQueryList, "isSave");
                ///需要登录
                string ua = Request.Cookies["ua"]?["agentId"]?.ToString();
                if (isSaveStr == "1" && string.IsNullOrWhiteSpace(ua))
                {
                    throw new Exception("请先登录");
                }
                ///是否自动赋值的不保存
                if (string.IsNullOrWhiteSpace(isSaveStr)) isSaveStr = "1";
                bool isSave = isSaveStr == "1";
                ///是否返回文件的内容JSON
                string hasJsonStr = m_cQuery.m_fGetQueryString(m_lQueryList, "hasJson");
                if (string.IsNullOrWhiteSpace(hasJsonStr)) hasJsonStr = "1";
                bool hasJson = hasJsonStr == "1";

                ///简易测试命令
                if (file == "json1")
                {
                    List<m_cFile> m_lFiles = new List<m_cFile>();
                    m_cFile m_mFile = new m_cFile();
                    m_mFile.sno = "1";
                    m_mFile.cid = "370830199308120516";
                    m_mFile.username = "90001";
                    m_lFiles.Add(m_mFile);
                    file = JsonConvert.SerializeObject(m_lFiles);
                }

                if (string.IsNullOrWhiteSpace(file))
                    throw new ArgumentNullException("file");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["subId"] = subId;

                ///手动构建CSV文件,将其直接和可视化连用
                JArray m_pJArray = JArray.Parse(file);
                if (m_pJArray == null)
                    throw new Exception("无数据");
                if (m_pJArray.Count > 1000)
                    throw new Exception("数据最多1000行");
                if (m_pJArray.Count <= 0)
                    throw new Exception("无数据");

                ///构造dt即可
                DataTable dt = new DataTable();
                dt.Columns.Add("sno", typeof(string));
                dt.Columns.Add("cid", typeof(string));
                dt.Columns.Add("username", typeof(string));
                dt.Columns.Add("Xm", typeof(string));
                dt.Columns.Add("Ywy", typeof(string));
                dt.Columns.Add("MD5Shfzh", typeof(string));

                ///默认赋值ua
                string username = ua == "" ? null : ua;
                ///是否使用默认坐席
                bool m_uUseDefAgentID = System.Configuration.ConfigurationManager.AppSettings["m_uUseDefAgentID"] == "1";
                if (string.IsNullOrWhiteSpace(username) && m_uUseDefAgentID)
                {
                    ///查询出默认坐席,以便后续使用
                    username = m_cSQL.m_sAgentID;
                }

                ///默认唯一标识
                int j = 0;
                /// 自动MD5
                foreach (JToken item in m_pJArray)
                {
                    ///自定义编号
                    if (string.IsNullOrWhiteSpace(item.Value<string>("sno")))
                        item["sno"] = (++j).ToString();
                    ///坐席ID判断
                    if (string.IsNullOrWhiteSpace(item.Value<string>("username")))
                    {
                        if (string.IsNullOrWhiteSpace(username))
                        {
                            throw new Exception("信修提交前需设置信修坐席ID");
                        }
                        item["username"] = username;
                    }
                    ///判断信修是否在本数据库中存在
                    if (!m_cSQL.m_bHasAgentID(item["username"].ToString()))
                    {
                        throw new Exception("信修坐席ID不存在");
                    }

                    ///数据表
                    DataRow dr = dt.NewRow();
                    dr["sno"] = item["sno"];
                    dr["cid"] = item["cid"];
                    dr["username"] = item["username"];
                    dr["Xm"] = item["Xm"];
                    dr["Ywy"] = item["Ywy"];
                    dr["MD5Shfzh"] = m_cDigest.m_fMD5(item.Value<string>("cid"));
                    dt.Rows.Add(dr);
                }

                ///转类至新文件并提交
                byte[] m_lByte;
                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(ms))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    ///写入CSV文件
                    csv.WriteRecords(
                        dt.AsEnumerable().Select(x =>
                        {
                            return new
                            {
                                sno = x.Field<object>("sno")?.ToString(),
                                cid = x.Field<object>("MD5Shfzh")?.ToString(),
                                username = x.Field<object>("username")?.ToString()
                            };

                        }));

                    writer.Flush();
                    m_lByte = ms.ToArray();
                }

                ///写入压缩
                MemoryStream zipMs = new MemoryStream();
                GZipStream compressedStream = new GZipStream(zipMs, CompressionMode.Compress, true);
                compressedStream.Write(m_lByte, 0, m_lByte.Length);
                compressedStream.Flush();
                compressedStream.Close();

                ///发送请求
                string m_sResultString = string.Empty;
                if (file == "json1")
                    m_sResultString = "{\"code\":0,\"msg\":\"测试成功\"}";
                else
                    m_sResultString = m_cHttp.m_fPost(url, m_pDic, zipMs.ToArray());

                ///需要将结果写入数据库,这里催收系统交付即可
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    #region ***保存请求至本地数据库
                    if (isSave)
                    {
                        int RespState = (status == 0 ? 1 : 2);
                        string ReqJson = (hasJson ? m_cJSON.Parse(dt) : null);
                        ///保存请求头
                        m_cSQL.m_fSaveReqFile(msg, RespState, requestID, ReqJson);
                    }

                    ///保存数据内容
                    m_cSQL.m_fSaveReqInfo(requestID, dt);
                    #endregion

                    ///返回请求ID
                    data = requestID;

                    return rJson(m_cXxCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                    return eJson(m_cXxCfg.entID);
                }

            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
                return eJson(m_cXxCfg.entID);
            }
        }
        #endregion

        #region +++ api.asmx 续联完成通知
        public void BF_1FILE()
        {
            try
            {
                string m_sJSONStr = this.m_fGetBodyJSON();
                if (!string.IsNullOrWhiteSpace(m_sJSONStr))
                {
                    Log.Instance.Debug($"[CenoQnService][HomeController][BF_1FILE][{m_sJSONStr}]");
                    ///JSON参数解析
                    JObject m_pJObject = JObject.Parse(m_sJSONStr);
                    string entID = m_pJObject["entID"].ToString();
                    string requestID = m_pJObject["requestID"].ToString();
                    string timestamp = m_pJObject["timestamp"].ToString();
                    string mac = m_pJObject["mac"].ToString();
                    ///参数解析,首先验签
                    string _mac = m_cDigest.m_fMD5($"{entID}{m_cXxCfg.entSecret}{timestamp}{requestID}");
                    if (_mac != mac)
                    {
                        m_fResponse(m_cJSON.Err(requestID, -1, "权限验证失败"));
                        return;
                    }
                    ///自己的逻辑即可
                    string m_sReturnStr = m_cJSON.OKString(requestID);
                    Log.Instance.Debug(m_sReturnStr);
                    m_fResponse(m_sReturnStr);
                }
                else
                {
                    m_fResponse(m_cJSON.Err(null, -99, "JSON参数非空"));
                }
            }
            catch (Exception ex)
            {
                m_fResponse(m_cJSON.Err(null, -99, ex.Message));
            }
        }
        #endregion

        #region ***获取续联结果名单
        public ActionResult V_2LIST(string queryString)
        {
            ViewBag.Title = "获取续联结果名单";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_2LIST(int page, int limit, string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cXxCfg.QN_API_URL}/trace/retrieve/list";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cXxCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cXxCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    throw new ArgumentNullException("requestID");
                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["page"] = page;
                m_pDic["size"] = limit;

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
                        List<object> m_lObject = new List<object>();
                        if (count > 0)
                        {
                            JToken m_pJToken = m_pJObject["callList"];
                            foreach (JToken item in m_pJToken)
                            {
                                int tag = Convert.ToInt32(item["tag"]);
                                if (tag == 1)
                                {
                                    JToken _m_pJToken = item["rsno"];
                                    foreach (JToken _item in _m_pJToken)
                                    {
                                        object m_oObject = new
                                        {
                                            sno = item["sno"]?.ToString(),
                                            tag = item["tag"]?.ToString(),
                                            cid = item["cid"]?.ToString(),
                                            extendColumn = item["extendColumn"]?.ToString(),
                                            serialNO = _item["serialNO"]?.ToString(),
                                            hostNum = _item["hostNum"]?.ToString()
                                        };
                                        m_lObject.Add(m_oObject);
                                    }
                                }
                                else
                                {
                                    object m_oObject = new
                                    {
                                        sno = item["sno"]?.ToString(),
                                        tag = item["tag"]?.ToString(),
                                        cid = item["cid"]?.ToString(),
                                        extendColumn = item["extendColumn"]?.ToString(),
                                    };
                                    m_lObject.Add(m_oObject);
                                }
                            }
                        }
                        data = m_lObject;
                        return rJson();
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson();
        }
        #endregion

        #region ***坐席登录接口
        public ActionResult V_3LOGIN(string queryString)
        {
            ViewBag.Title = "坐席登录接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_3LOGIN(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/login";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentId = m_cQuery.m_fGetQueryString(m_lQueryList, "agentId");
                if (string.IsNullOrWhiteSpace(agentId))
                    throw new ArgumentNullException("agentId");
                ///密码非空
                string passWord = m_cQuery.m_fGetQueryString(m_lQueryList, "passWord");
                if (string.IsNullOrWhiteSpace(passWord))
                    throw new ArgumentNullException("passWord");
                ///dn修正
                string dn = m_cQuery.m_fGetQueryString(m_lQueryList, "dn");
                ///呼叫中心IP
                string m_sIP = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sIP");
                ///呼叫中心登录名
                string m_sLoginName = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sLoginName");
                ///续联号码绑定
                string m_sBindNumber = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sBindNumber");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentId"] = agentId;
                m_pDic["passWord"] = passWord;
                m_pDic["dn"] = dn;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    ///保存登录用户
                    Response.Cookies["ua"]["agentId"] = agentId;
                    Response.Cookies["ua"]["m_sIP"] = m_sIP;
                    Response.Cookies["ua"]["m_sLoginName"] = m_sLoginName;
                    Response.Cookies["ua"]["m_sBindNumber"] = m_sBindNumber;
                    Response.Cookies["ua"].Expires = DateTime.Now.AddDays(1);

                    return rJson(m_cCcCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***坐席登出接口
        public ActionResult V_4LOGOUT(string queryString)
        {
            ViewBag.Title = "坐席登出接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_4LOGOUT(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/logout";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentId = m_cQuery.m_fGetQueryString(m_lQueryList, "agentId");
                if (string.IsNullOrWhiteSpace(agentId))
                    throw new ArgumentNullException("agentId");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentId"] = agentId;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    ///清除登录缓存
                    string ua = Request.Cookies["ua"]?["agentId"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(ua) && ua == agentId)
                    {
                        Response.Cookies["ua"].Expires = DateTime.Now.AddDays(-1);
                    }

                    return rJson(m_cCcCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***坐席外呼接口
        public ActionResult V_5CALL(string queryString)
        {
            ViewBag.Title = "坐席外呼接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_5CALL(string queryString)
        {
            ///是否使用缓存坐席ID
            bool useUa = false;
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/makeCall";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentId = m_cQuery.m_fGetQueryString(m_lQueryList, "agentId");
                ///是否使用缓存坐席ID
                useUa = m_cQuery.m_fGetQueryString(m_lQueryList, "useUa") == "1";
                ///来源判断
                if (string.IsNullOrWhiteSpace(agentId))
                {
                    if (useUa)
                    {
                        string ua = Request.Cookies["ua"]?["agentId"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(ua))
                        {
                            agentId = ua;
                        }
                        else
                        {
                            throw new Exception("请先登录");
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("agentId");
                    }
                }
                ///外呼号码非空
                string number = m_cQuery.m_fGetQueryString(m_lQueryList, "number");
                if (string.IsNullOrWhiteSpace(number))
                    throw new ArgumentNullException("number");
                ///可空
                string telecomAni = m_cQuery.m_fGetQueryString(m_lQueryList, "telecomAni");
                string unicomAni = m_cQuery.m_fGetQueryString(m_lQueryList, "unicomAni");
                string mobileAni = m_cQuery.m_fGetQueryString(m_lQueryList, "mobileAni");
                string userData = m_cQuery.m_fGetQueryString(m_lQueryList, "userData");
                string dpsEventUrl = m_cQuery.m_fGetQueryString(m_lQueryList, "dpsEventUrl");
                string dpsDetailUrl = m_cQuery.m_fGetQueryString(m_lQueryList, "dpsDetailUrl");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentId"] = agentId;
                m_pDic["number"] = number;
                m_pDic["telecomAni"] = telecomAni;
                m_pDic["unicomAni"] = unicomAni;
                m_pDic["mobileAni"] = mobileAni;
                m_pDic["userData"] = userData;
                m_pDic["dpsEventUrl"] = dpsEventUrl;
                m_pDic["dpsDetailUrl"] = dpsDetailUrl;

                ///追加录音ID
                string sessionId = null;

                ///记录拨打者IP
                string m_sHost = Request.UserHostAddress;
                ///保存录音
                m_cSQL.m_fSetDialRecord(number, m_sHost);

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);

                    if (status == 0)
                    {
                        ///获取代码,兼容未登录
                        if (m_pJObject.ContainsKey("data"))
                        {
                            string m_sData = m_pJObject["data"].ToString();
                            JObject m_pJData = JObject.Parse(m_sData);
                            ///响应码
                            if (m_pJData.ContainsKey("code"))
                            {
                                data = m_pJData["code"].ToString();
                            }

                            ///追加录音保存,方便统计
                            if (m_pJData.ContainsKey("ext"))
                            {
                                string ext = m_pJData["ext"].ToString();
                                JObject m_pJExt = JObject.Parse(ext);
                                if (m_pJExt.ContainsKey("sessionId"))
                                {
                                    sessionId = m_pJExt["sessionId"].ToString();
                                    ///把sessionId直接存入,待后续查询通话记录
                                    m_cSQL.m_fSaveRecord(sessionId);
                                }
                            }
                        }
                    }

                    msg = m_pJObject["msg"].ToString();
                    ///返回分支,追加不影响原逻辑的录音ID
                    if (useUa)
                    {
                        return Json(new
                        {
                            status = status,
                            msg = msg,
                            count = count,
                            data = data,
                            uuid = m_cCmn.UUID(null),
                            sessionId = sessionId
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            status = status,
                            msg = msg,
                            count = count,
                            data = data,
                            uuid = m_cCmn.UUID(entID),
                            sessionId = sessionId
                        });
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            ///返回分支
            if (useUa)
                return eJson();
            else
                return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***坐席外呼接口(自动MD5身份证)
        public ActionResult V_5MD5CALL(string queryString)
        {
            ViewBag.Title = "坐席外呼接口(自动MD5身份证)";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_5MD5CALL(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/makeCall";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentId = m_cQuery.m_fGetQueryString(m_lQueryList, "agentId");
                if (string.IsNullOrWhiteSpace(agentId))
                    throw new ArgumentNullException("agentId");
                ///外呼号码非空
                string number = m_cQuery.m_fGetQueryString(m_lQueryList, "number");
                if (string.IsNullOrWhiteSpace(number))
                    throw new ArgumentNullException("number");
                number = m_cDigest.m_fMD5(number);

                ///可空
                string telecomAni = m_cQuery.m_fGetQueryString(m_lQueryList, "telecomAni");
                string unicomAni = m_cQuery.m_fGetQueryString(m_lQueryList, "unicomAni");
                string mobileAni = m_cQuery.m_fGetQueryString(m_lQueryList, "mobileAni");
                string userData = m_cQuery.m_fGetQueryString(m_lQueryList, "userData");
                string dpsEventUrl = m_cQuery.m_fGetQueryString(m_lQueryList, "dpsEventUrl");
                string dpsDetailUrl = m_cQuery.m_fGetQueryString(m_lQueryList, "dpsDetailUrl");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentId"] = agentId;
                m_pDic["number"] = number;
                m_pDic["telecomAni"] = telecomAni;
                m_pDic["unicomAni"] = unicomAni;
                m_pDic["mobileAni"] = mobileAni;
                m_pDic["userData"] = userData;
                m_pDic["dpsEventUrl"] = dpsEventUrl;
                m_pDic["dpsDetailUrl"] = dpsDetailUrl;

                ///追加录音ID
                string sessionId = null;
                ///记录拨打者IP
                string m_sHost = Request.UserHostAddress;
                ///保存录音
                m_cSQL.m_fSetDialRecord(number, m_sHost);

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);

                    if (status == 0)
                    {
                        ///获取代码,兼容未登录
                        if (m_pJObject.ContainsKey("data"))
                        {
                            string m_sData = m_pJObject["data"].ToString();
                            JObject m_pJData = JObject.Parse(m_sData);
                            ///响应码
                            if (m_pJData.ContainsKey("code"))
                            {
                                data = m_pJData["code"].ToString();
                            }

                            ///追加录音保存,方便统计
                            if (m_pJData.ContainsKey("ext"))
                            {
                                string ext = m_pJData["ext"].ToString();
                                JObject m_pJExt = JObject.Parse(ext);
                                if (m_pJExt.ContainsKey("sessionId"))
                                {
                                    sessionId = m_pJExt["sessionId"].ToString();
                                    ///把sessionId直接存入,待后续查询通话记录
                                    m_cSQL.m_fSaveRecord(m_pJExt["sessionId"].ToString());
                                }
                            }
                        }
                    }

                    msg = m_pJObject["msg"].ToString();
                    ///返回分支,追加不影响原逻辑的录音ID
                    return Json(new
                    {
                        status = status,
                        msg = msg,
                        count = count,
                        data = data,
                        uuid = m_cCmn.UUID(entID),
                        sessionId = sessionId
                    });
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region +++ api.asmx dps事件推送
        public void BF_2DPSEVENT()
        {
            try
            {
                string m_sJSONStr = this.m_fGetBodyJSON();
                if (!string.IsNullOrWhiteSpace(m_sJSONStr))
                {
                    Log.Instance.Debug($"[CenoQnService][HomeController][BF_2DPSEVENT][{m_sJSONStr}]");
                    ///JSON参数解析
                    JObject m_pJObject = JObject.Parse(m_sJSONStr);
                    string entID = m_pJObject["entID"].ToString();
                    string requestID = m_pJObject["requestID"].ToString();
                    string timestamp = m_pJObject["timestamp"].ToString();
                    string mac = m_pJObject["mac"].ToString();
                    ///参数解析,首先验签
                    string _mac = m_cDigest.m_fMD5($"{entID}{m_cXxCfg.entSecret}{timestamp}{requestID}");
                    if (_mac != mac)
                    {
                        m_fResponse(m_cJSON.Err(requestID, -1, "权限验证失败"));
                        return;
                    }
                    ///自己的逻辑即可
                    string m_sReturnStr = m_cJSON.OKString(requestID);
                    Log.Instance.Debug(m_sReturnStr);
                    m_fResponse(m_sReturnStr);
                }
                else
                {
                    m_fResponse(m_cJSON.Err(null, -99, "JSON参数非空"));
                }
            }
            catch (Exception ex)
            {
                m_fResponse(m_cJSON.Err(null, -99, ex.Message));
            }
        }
        #endregion

        #region +++ api.asmx dps明细推送
        public void BF_3DPSDETAIL()
        {
            try
            {
                string m_sJSONStr = this.m_fGetBodyJSON();
                if (!string.IsNullOrWhiteSpace(m_sJSONStr))
                {
                    Log.Instance.Debug($"[CenoQnService][HomeController][BF_3DPSDETAIL][{m_sJSONStr}]");
                    ///JSON参数解析
                    JObject m_pJObject = JObject.Parse(m_sJSONStr);
                    string entID = m_pJObject["entID"].ToString();
                    string requestID = m_pJObject["requestID"].ToString();
                    string timestamp = m_pJObject["timestamp"].ToString();
                    string mac = m_pJObject["mac"].ToString();
                    ///参数解析,首先验签
                    string _mac = m_cDigest.m_fMD5($"{entID}{m_cXxCfg.entSecret}{timestamp}{requestID}");
                    if (_mac != mac)
                    {
                        m_fResponse(m_cJSON.Err(requestID, -1, "权限验证失败"));
                        return;
                    }
                    ///自己的逻辑即可
                    string m_sReturnStr = m_cJSON.OKString(requestID);
                    Log.Instance.Debug(m_sReturnStr);
                    m_fResponse(m_sReturnStr);
                }
                else
                {
                    m_fResponse(m_cJSON.Err(null, -99, "JSON参数非空"));
                }
            }
            catch (Exception ex)
            {
                m_fResponse(m_cJSON.Err(null, -99, ex.Message));
            }
        }
        #endregion

        #region ***明细查询接口
        public ActionResult V_6DPSQUERY(string queryString)
        {
            ViewBag.Title = "明细查询接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_6DPSQUERY(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/dps/query";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///外呼标识号非空
                string sessionIds = m_cQuery.m_fGetQueryString(m_lQueryList, "sessionIds");
                if (string.IsNullOrWhiteSpace(sessionIds))
                    throw new ArgumentNullException("sessionIds");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["sessionIds"] = sessionIds;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();
                    if (status == 0)
                    {
                        List<object> m_lObject = new List<object>();
                        JToken m_pJToken = m_pJObject["data"];
                        foreach (JToken item in m_pJToken)
                        {
                            object m_oObject = new
                            {
                                entID = item["entID"]?.ToString(),
                                sessionId = item["sessionId"]?.ToString(),
                                userData = item["userData"]?.ToString(),
                                agentId = item["agentId"]?.ToString(),
                                ani = item["ani"]?.ToString(),
                                dnis = item["dnis"]?.ToString(),
                                dani = item["dani"]?.ToString(),
                                ddnis = item["ddnis"]?.ToString(),
                                startTime = item["startTime"]?.ToString(),
                                endTime = item["endTime"]?.ToString(),
                                callResult = item["callResult"]?.ToString(),
                                alertDuration = item["alertDuration"]?.ToString(),
                                talkDuration = item["talkDuration"]?.ToString(),
                                endType = item["endType"]?.ToString()
                            };
                            m_lObject.Add(m_oObject);
                        }
                        data = m_lObject;
                        return rJson(m_cCcCfg.entID);
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***录音查询接口
        public ActionResult V_7AUDIOQUERY(string queryString)
        {
            ViewBag.Title = "录音查询接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_7AUDIOQUERY(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/audio/query";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///外呼标识号非空
                string sessionIds = m_cQuery.m_fGetQueryString(m_lQueryList, "sessionIds");
                if (string.IsNullOrWhiteSpace(sessionIds))
                    throw new ArgumentNullException("sessionIds");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["sessionIds"] = sessionIds;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();
                    if (status == 0)
                    {
                        List<object> m_lObject = new List<object>();
                        JToken m_pJToken = m_pJObject["data"];
                        foreach (JToken item in m_pJToken)
                        {
                            object m_oObject = new
                            {
                                sessionId = item["sessionId"]?.ToString(),
                                audioUrl = item["audioUrl"]?.ToString(),
                                audioType = item["audioType"]?.ToString()
                            };
                            m_lObject.Add(m_oObject);
                        }
                        data = m_lObject;
                        return rJson(m_cCcCfg.entID);
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***回呼明细查询接口
        public ActionResult V_8RECORDGET(string queryString)
        {
            ViewBag.Title = "回呼明细查询接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_8RECORDGET(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/ivr/record/get";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///开始时间非空
                string startTime = m_cQuery.m_fGetQueryString(m_lQueryList, "startTime");
                if (string.IsNullOrWhiteSpace(startTime))
                    throw new ArgumentNullException("startTime");
                ///结束时间非空
                string endTime = m_cQuery.m_fGetQueryString(m_lQueryList, "endTime");
                if (string.IsNullOrWhiteSpace(endTime))
                    throw new ArgumentNullException("endTime");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["startTime"] = startTime;
                m_pDic["endTime"] = endTime;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();
                    if (status == 0)
                    {
                        List<object> m_lObject = new List<object>();
                        JToken m_pJToken = m_pJObject["data"];
                        foreach (JToken item in m_pJToken)
                        {
                            object m_oObject = new
                            {
                                hostNum = item["hostNum"]?.ToString(),
                                serialNo = item["serialNo"]?.ToString(),
                                agentDn = item["agentDn"]?.ToString(),
                                ivrCallTime = item["ivrCallTime"]?.ToString(),
                                agentPhone = item["agentPhone"]?.ToString(),
                                ivrCallType = item["ivrCallType"]?.ToString(),
                                agentId = item["agentId"]?.ToString()
                            };
                            m_lObject.Add(m_oObject);
                        }
                        data = m_lObject;
                        return rJson(m_cCcCfg.entID);
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***坐席当日通时通次查询接口
        public ActionResult V_9DIALQUERY(string queryString)
        {
            ViewBag.Title = "坐席当日通时通次查询接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_9DIALQUERY(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/dial/query";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentIds = m_cQuery.m_fGetQueryString(m_lQueryList, "agentIds");
                if (string.IsNullOrWhiteSpace(agentIds))
                    throw new ArgumentNullException("agentIds");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentIds"] = agentIds;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();
                    if (status == 0)
                    {
                        List<object> m_lObject = new List<object>();
                        JToken m_pJToken = m_pJObject["data"];
                        foreach (JToken item in m_pJToken)
                        {
                            object m_oObject = new
                            {
                                agentId = item["agentId"]?.ToString(),
                                totalDuration = item["totalDuration"]?.ToString(),
                                dialTimes = item["dialTimes"]?.ToString(),
                                dialSuccTimes = item["dialSuccTimes"]?.ToString(),
                                agentStatus = item["agentStatus"]?.ToString()
                            };
                            m_lObject.Add(m_oObject);
                        }
                        data = m_lObject;
                        return rJson(m_cCcCfg.entID);
                    }
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region +++定时器激活网站
        public void BT_1REQ()
        {
            Log.Instance.Debug($"[CenoQnService][HomeController][BT_1REQ][激活]");
            m_fResponse("+OK");
        }
        #endregion

        #region ***续联结果列表
        public ActionResult V_10XLIST(string queryString)
        {
            ViewBag.Title = "续联结果列表";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_10XLIST(Pager pager, string queryString)
        {
            try
            {
                QueryPager qop = new QueryPager();
                qop.pager = pager;
                qop.queryString = queryString;
                qop.FieldsSqlPart = $@"SELECT call_repair_list.[Id],
       call_repair_list.[sno],
       T0.Shfzh as [cid],
       T0.Xm,
       T0.Ywy,
       T0.username,
       [tag],
       [extendColumn],
       [serialNO],
       [hostNum],
       call_repair_list.[requestID],
       ISNULL(T1.WorkCount, 0) AS WorkCount,
       CONVERT(VARCHAR(20), T1.LastWorkTime, 20) AS LastWorkTime,
       ISNULL(T2.DialCount, 0) AS DialCount,
       CONVERT(VARCHAR(20), T2.LastDialTime, 20) AS LastDialTime";
                qop.FromSqlPart = $@"FROM [dbo].[call_repair_list] WITH (NOLOCK)
    OUTER APPLY
(
    SELECT TOP 1
        call_repair_info.*
    FROM call_repair_info
    WHERE call_repair_list.requestID = call_repair_info.requestID
          AND ISNULL(call_repair_info.IsDel, 0) = 0
          AND call_repair_list.sno = call_repair_info.sno
) T0
    OUTER APPLY
(
    SELECT COUNT(1) AS WorkCount,
           MAX(call_repair_dial.dialtime) AS LastWorkTime
    FROM call_repair_dial WITH (NOLOCK)
    WHERE call_repair_dial.dialno = call_repair_list.serialNO
          AND ISNULL(call_repair_dial.IsDel, 0) = 0
    GROUP BY call_repair_dial.dialno
) T1
    OUTER APPLY
(
    SELECT COUNT(1) AS DialCount,
           MAX(call_repair_record.startTime) AS LastDialTime
    FROM call_repair_record WITH (NOLOCK)
    WHERE call_repair_record.dnis = call_repair_list.serialNO
          AND ISNULL(call_repair_record.IsDel, 0) = 0
    GROUP BY call_repair_record.dnis
) T2";
                qop.WhereSqlPart = $@"
WHERE ISNULL(call_repair_list.IsDel,0) = 0
";

                ///请求编号
                qop.setQuery("call_repair_list.requestID", "requestID");
                ///姓名
                qop.setQuery("Xm", "Xm");
                ///用户标识
                qop.setQuery("T0.Shfzh", "cid");
                ///业务员
                qop.setQuery("Ywy", "Ywy");
                ///匿号
                qop.setQuery("hostNum", "hostNum");
                ///坐席ID
                qop.setQuery("T0.username", "username");
                ///查询
                IList list = qop.QiList();
                status = 0;
                msg = "成功";
                count = qop.count;
                data = list;
                return rJson();
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson();
        }
        #endregion

        #region ***后台调用通用接口
        public JsonResult m_fApplyXx(string m_sPhoneNumber, string m_sMD5 = "")
        {
            try
            {
                string url = $"{System.Configuration.ConfigurationManager.AppSettings["m_sHttp"]}";

                string m_sLoginName = Request.Cookies["ua"]?["m_sLoginName"]?.ToString();
                if (string.IsNullOrWhiteSpace(m_sLoginName)) throw new Exception("呼叫中心登录名非空,请重新登录");

                string m_sIP = Request.Cookies["ua"]?["m_sIP"]?.ToString();
                if (string.IsNullOrWhiteSpace(m_sIP)) throw new Exception("呼叫中心IP非空,请重新登录");

                string m_sResultString = m_cHttp.m_fGet(url, $"m_sIP={m_sIP}&m_sLoginName={m_sLoginName}&m_sPhoneNumber={m_sPhoneNumber}&m_sMD5={m_sMD5}");
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["status"]);
                    msg = m_pJObject["msg"].ToString();
                    return rJson();
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson();
        }
        #endregion

        #region ***后台调用通用接口2,支持固定信修线路
        public JsonResult m_fApplyXx2(string m_sPhoneNumber, string m_sMD5 = "")
        {
            try
            {
                string url = $"{System.Configuration.ConfigurationManager.AppSettings["m_sHttp"]}";

                string m_sLoginName = Request.Cookies["ua"]?["m_sLoginName"]?.ToString();
                if (string.IsNullOrWhiteSpace(m_sLoginName)) throw new Exception("呼叫中心登录名非空,请重新登录");

                string m_sIP = Request.Cookies["ua"]?["m_sIP"]?.ToString();
                if (string.IsNullOrWhiteSpace(m_sIP)) throw new Exception("呼叫中心IP非空,请重新登录");

                string m_sBindNumber = Request.Cookies["ua"]?["m_sBindNumber"]?.ToString();

                string m_sResultString = m_cHttp.m_fGet(url, $"m_sIP={m_sIP}&m_sLoginName={m_sLoginName}&m_sPhoneNumber={m_sPhoneNumber}&m_sMD5={m_sMD5}&m_sBindNumber={m_sBindNumber}");
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["status"]);
                    msg = m_pJObject["msg"].ToString();
                    return rJson();
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson();
        }
        #endregion

        #region ***需求名单上传(Excel文件)
        public ActionResult V_11FILE(string queryString)
        {
            ViewBag.Title = "需求名单上传(Excel文件)";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_11FILE(string queryString)
        {
            try
            {
                ///需求名单上传URL
                string url = $"{m_cXxCfg.QN_API_URL}/trace/collect/file";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cXxCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cXxCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///subId非空
                string subId = m_cQuery.m_fGetQueryString(m_lQueryList, "subId");
                if (string.IsNullOrWhiteSpace(subId))
                    throw new ArgumentNullException("subId");
                ///是否保存至本地数据库
                bool isSave = m_cQuery.m_fGetQueryString(m_lQueryList, "isSave") == "1";
                ///需要登录
                string ua = Request.Cookies["ua"]?["agentId"]?.ToString();
                if (isSave && string.IsNullOrWhiteSpace(ua))
                {
                    throw new Exception("请先登录");
                }
                ///设定Ywy是否必填
                bool isMustYwy = m_cQuery.m_fGetQueryString(m_lQueryList, "isMustYwy") == "1";
                ///是否判重
                bool isRepeat = m_cQuery.m_fGetQueryString(m_lQueryList, "isRepeat") == "1";
                ///是否返回文件的内容JSON
                string hasJsonStr = m_cQuery.m_fGetQueryString(m_lQueryList, "hasJson");
                if (string.IsNullOrWhiteSpace(hasJsonStr)) hasJsonStr = "1";
                bool hasJson = hasJsonStr == "1";
                ///CSV压缩文件
                int m_uCount = Request.Files.Count;
                if (m_uCount <= 0)
                    throw new ArgumentNullException("file");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["subId"] = subId;

                ///读取文件至DataSet
                DataSet ds = m_cExcel.m_fToDataSet(Request.Files[0]);
                ///仅获取Sheet1数据
                DataTable dt = m_cExcel.m_fGetSheet1(ds);
                if (dt == null)
                    throw new Exception("未找到Sheet1");
                if (dt.Rows.Count > 1000)
                    throw new Exception("数据最多1000行");
                if (dt.Rows.Count <= 0)
                    throw new Exception("无数据");
                ///判断列
                if (!dt.Columns.Contains("sno"))
                    throw new Exception("Sheet1中无sno列");
                if (!dt.Columns.Contains("cid"))
                    throw new Exception("Sheet1中无cid列");
                if (!dt.Columns.Contains("username"))
                    throw new Exception("Sheet1中无username列");
                if (!dt.Columns.Contains("Xm"))
                    throw new Exception("Sheet1中无Xm列");
                if (!dt.Columns.Contains("Ywy"))
                    throw new Exception("Sheet1中无Ywy列");

                ///增加MD5身份证列
                dt.Columns.Add("MD5Shfzh", typeof(string));

                ///处理数据
                int i = 0;
                foreach (DataRow item in dt.Rows)
                {
                    ///递增
                    i++;
                    ///数据唯一标识
                    if (string.IsNullOrWhiteSpace(item["sno"]?.ToString()))
                        item["sno"] = i.ToString();
                    ///客户标识
                    item["MD5Shfzh"] = m_cDigest.m_fMD5(item["cid"]?.ToString());
                    ///坐席ID
                    if (string.IsNullOrWhiteSpace(item["username"]?.ToString()))
                        item["username"] = ua;
                    ///判断Ywy是否非空
                    if (isMustYwy && string.IsNullOrWhiteSpace(item["Ywy"]?.ToString()))
                        throw new Exception($"业务员必填：第{i}行");
                }

                ///增加判重逻辑
                if (isRepeat)
                {
                    DataTable m_pSheet1 = null;
                    bool m_bIsRepeat = m_cSQL.m_fQueryRepeat(dt, out m_pSheet1, out msg);
                    if (m_bIsRepeat)
                    {
                        if (m_pSheet1 != null && m_pSheet1.Rows.Count > 0) count = m_pSheet1.Rows.Count;
                        return Json(new
                        {
                            status = 403,
                            msg = msg,
                            count = count,
                            data = m_cJSON.m_fDataTableToIList(m_pSheet1),
                            uuid = m_cCmn.UUID(entID)
                        });
                    }
                }

                ///转类至新文件并提交
                byte[] m_lByte;
                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(ms))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    ///写入CSV文件
                    csv.WriteRecords(
                        dt.AsEnumerable().Select(x =>
                        {
                            return new
                            {
                                sno = x.Field<object>("sno")?.ToString(),
                                cid = x.Field<object>("MD5Shfzh")?.ToString(),
                                username = x.Field<object>("username")?.ToString()
                            };

                        }));

                    writer.Flush();
                    m_lByte = ms.ToArray();
                }

                ///写入压缩
                MemoryStream zipMs = new MemoryStream();
                GZipStream compressedStream = new GZipStream(zipMs, CompressionMode.Compress, true);
                compressedStream.Write(m_lByte, 0, m_lByte.Length);
                compressedStream.Flush();
                compressedStream.Close();

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic, zipMs.ToArray());

                ///需要将结果写入数据库,这里催收系统交付即可
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    #region ***保存请求至本地数据库
                    if (isSave)
                    {
                        int RespState = (status == 0 ? 1 : 2);
                        string ReqJson = (hasJson ? m_cJSON.Parse(dt) : null);
                        ///保存请求头
                        m_cSQL.m_fSaveReqFile(msg, RespState, requestID, ReqJson);
                    }

                    ///保存数据内容
                    m_cSQL.m_fSaveReqInfo(requestID, dt);
                    #endregion

                    return rJson(m_cXxCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                    return eJson(m_cXxCfg.entID);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
                return eJson(m_cXxCfg.entID);
            }
        }
        #endregion

        #region ***本数据库验证登录
        public ActionResult V_12LOGIN(string queryString)
        {
            ViewBag.Title = "坐席登录接口";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }
        public JsonResult F_12LOGIN(string queryString)
        {
            try
            {
                ///获取续联结果名单URL
                string url = $"{m_cCcCfg.QN_API_URL}/agent/user/login";
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///entID企业编号
                string entID = m_cQuery.m_fGetQueryString(m_lQueryList, "entID");
                if (string.IsNullOrWhiteSpace(entID))
                    entID = m_cCcCfg.entID;
                ///entSecret企业安全码
                string entSecret = m_cQuery.m_fGetQueryString(m_lQueryList, "entSecret");
                if (string.IsNullOrWhiteSpace(entSecret))
                    entSecret = m_cCcCfg.entSecret;
                ///requestID非空
                string requestID = m_cQuery.m_fGetQueryString(m_lQueryList, "requestID");
                if (string.IsNullOrWhiteSpace(requestID))
                    requestID = m_cCmn.UUID(entID);
                ///坐席ID非空
                string agentId = m_cQuery.m_fGetQueryString(m_lQueryList, "agentId");
                if (string.IsNullOrWhiteSpace(agentId))
                    throw new ArgumentNullException("agentId");
                ///密码非空
                string passWord = m_cQuery.m_fGetQueryString(m_lQueryList, "passWord");
                if (string.IsNullOrWhiteSpace(passWord))
                    throw new ArgumentNullException("passWord");
                ///dn修正
                string dn = m_cQuery.m_fGetQueryString(m_lQueryList, "dn");
                ///呼叫中心IP
                string m_sIP = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sIP");
                ///呼叫中心登录名
                string m_sLoginName = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sLoginName");
                ///续联号码绑定
                string m_sBindNumber = m_cQuery.m_fGetQueryString(m_lQueryList, "m_sBindNumber");

                ///查询本地数据库密码是否正确
                bool m_bValid = m_cSQL.m_fLogin(agentId, passWord, out passWord);
                if (!m_bValid) throw new Exception("用户名或密码错误");

                ///参数构造
                IDictionary<object, object> m_pDic = new Dictionary<object, object>();
                m_pDic["entID"] = entID;
                m_pDic["entSecret"] = entSecret;
                m_pDic["requestID"] = requestID;
                m_pDic["agentId"] = agentId;
                m_pDic["passWord"] = passWord;
                m_pDic["dn"] = dn;

                ///发送请求
                string m_sResultString = m_cHttp.m_fPost(url, m_pDic);
                if (!string.IsNullOrWhiteSpace(m_sResultString))
                {
                    JObject m_pJObject = JObject.Parse(m_sResultString);
                    status = Convert.ToInt32(m_pJObject["code"]);
                    msg = m_pJObject["msg"].ToString();

                    ///保存登录用户
                    Response.Cookies["ua"]["agentId"] = agentId;
                    Response.Cookies["ua"]["m_sIP"] = m_sIP;
                    Response.Cookies["ua"]["m_sLoginName"] = m_sLoginName;
                    Response.Cookies["ua"]["m_sBindNumber"] = m_sBindNumber;
                    Response.Cookies["ua"].Expires = DateTime.Now.AddDays(1);

                    return rJson(m_cCcCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***本数据库密码修改

        #endregion

        #region ***本数据库续联统计
        public ActionResult V_13XP(string queryString)
        {
            ViewBag.Title = "续联报表";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_13XP(Pager pager, string queryString)
        {
            try
            {
                QueryPager qop = new QueryPager();
                qop.pager = pager;
                qop.queryString = queryString;
                qop.FieldsSqlPart = $@"SELECT *";
                qop.FromSqlPart = $@"FROM
(
    SELECT ISNULL(T1.ResqCount, 0) AS ResqCount,
           ISNULL(T2.RespCount, 0) AS RespCount,
           ISNULL(T2.EffectCount, 0) AS EffectCount,
           call_repair.Id,
           CONVERT(VARCHAR(20), call_repair.AddTime, 20) AddTime,
           CONVERT(VARCHAR(20), call_repair.UpdateTime, 20) UpdateTime,
           call_repair.RequestID AS requestID,
           call_repair.RespState,
           call_repair.RespMsg
    FROM call_repair WITH (NOLOCK)
        OUTER APPLY
    (
        SELECT COUNT(1) AS ResqCount
        FROM call_repair_info WITH (NOLOCK)
        WHERE call_repair_info.requestID = call_repair.RequestID
              AND ISNULL(call_repair_info.IsDel, 0) = 0
    ) T1
        OUTER APPLY
    (
        SELECT COUNT(1) AS RespCount,
               SUM(   CASE
                          WHEN ISNULL(call_repair_list.serialNO, '') != '' THEN
                              1
                          ELSE
                              0
                      END
                  ) AS EffectCount
        FROM call_repair_list WITH (NOLOCK)
        WHERE call_repair_list.requestID = call_repair.RequestID
              AND ISNULL(call_repair_list.IsDel, 0) = 0
    ) T2
    WHERE ISNULL(call_repair.IsDel, 0) = 0
) T0";
                ///请求编号
                qop.setQuery("requestID", "requestID");
                ///添加时间起
                qop.setQuery("AddTime", "AddTimeStart");
                ///添加时间止
                qop.setQuery("AddTime", "AddTimeEnd");
                ///响应信息
                qop.setQuery("RespMsg", "RespMsg");
                ///页脚语句
                qop.SumSqlPart = "SUM(ResqCount) AS ResqCount,SUM(RespCount) AS RespCount,SUM(EffectCount) AS EffectCount";
                ///查询
                IList list = qop.QiList();
                status = 0;
                msg = "成功";
                count = qop.count;
                data = list;
                ///返回拼接
                return Json(new
                {
                    status = status,
                    msg = msg,
                    count = count,
                    data = data,
                    totalRow = qop.totalRow
                });
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***本数据库明细统计
        public ActionResult V_13DPSP(string queryString)
        {
            ViewBag.Title = "明细统计";
            ViewBag.queryString = HttpUtility.UrlEncode(queryString);
            return View();
        }

        public JsonResult F_13DPSP(Pager pager, string queryString)
        {
            try
            {
                QueryPager qop = new QueryPager();
                qop.pager = pager;
                qop.queryString = queryString;
                qop.FieldsSqlPart = $@"SELECT *";
                qop.FromSqlPart = $@"FROM
(
    SELECT (CASE
                WHEN ISNULL(call_repair_record.auto_status, 0) = 1 THEN
                    '明细获取成功'
                WHEN ISNULL(call_repair_record.auto_status, 0) = 2 THEN
                    '明细获取失败'
                WHEN ISNULL(call_repair_record.auto_status, 0) = 3 THEN
                    '录音下载成功'
                WHEN ISNULL(call_repair_record.auto_status, 0) = 4 THEN
                    '录音无需下载'
                WHEN ISNULL(call_repair_record.auto_status, 0) = 5 THEN
                    '录音下载失败'
                ELSE
                    '未获取明细'
            END
           ) AS auto_status_msg,
           (CASE
                WHEN ISNULL(call_repair_record.callResult, 0) = 1 THEN
                    '外呼成功'
                WHEN ISNULL(call_repair_record.callResult, 0) = 2 THEN
                    '外呼失败'
                ELSE
                    '-'
            END
           ) AS callResult_msg,
           (CASE
                WHEN ISNULL(call_repair_record.endType, 0) = 1 THEN
                    '客户挂断'
                WHEN ISNULL(call_repair_record.endType, 0) = 2 THEN
                    '坐席挂断'
                WHEN ISNULL(call_repair_record.endType, 0) = -1 THEN
                    '未知'
                ELSE
                    '-'
            END
           ) AS endType_msg,
           call_repair_record.sessionId,
           call_repair_record.username,
           call_repair_record.userData,
           call_repair_record.ani,
           call_repair_record.dnis,
           call_repair_record.dani,
           call_repair_record.ddnis,
           CONVERT(VARCHAR(20), call_repair_record.startTime, 20) startTime,
           CONVERT(VARCHAR(20), call_repair_record.endTime, 20) endTime,
           call_repair_record.callResult,
           call_repair_record.alertDuration,
           call_repair_record.talkDuration,
           call_repair_record.endType,
           call_repair_record.AddUserId,
           CONVERT(VARCHAR(20), call_repair_record.AddTime, 20) AddTime,
           call_repair_record.UpdateUserId,
           CONVERT(VARCHAR(20), call_repair_record.UpdateTime, 20) UpdateTime,
           call_repair_record.auto_status,
           call_repair_record.auto_err,
           call_repair_record.suffixAudio
    FROM call_repair_record WITH (NOLOCK)
    WHERE ISNULL(call_repair_record.IsDel, 0) = 0
) AS T0";
                ///会话ID
                qop.setQuery("sessionId", "sessionId");
                ///随路信令
                qop.setQuery("userData", "userData");
                ///坐席ID
                qop.setQuery("username", "agentId");
                ///主叫号码
                qop.setQuery("ani", "ani");
                ///被叫号码
                qop.setQuery("dnis", "dnis");
                ///主叫外显号码
                qop.setQuery("dani", "dani");
                ///被叫外显号码
                qop.setQuery("ddnis", "ddnis");
                ///通话开始时间起
                qop.setQuery("startTime", "startTimeStart");
                ///通话开始时间止
                qop.setQuery("startTime", "startTimeEnd");
                ///通话结束时间起
                qop.setQuery("endTime", "endTimeStart");
                ///通话结束时间止
                qop.setQuery("endTime", "endTimeEnd");
                ///外呼结果
                qop.setQuery("callResult", "callResult");
                ///振铃时长起
                qop.setQuery("alertDuration", "alertDurationStart");
                ///振铃时长止
                qop.setQuery("alertDuration", "alertDurationEnd");
                ///通话时长起
                qop.setQuery("talkDuration", "talkDurationStart");
                ///通话时长止
                qop.setQuery("talkDuration", "talkDurationEnd");
                ///挂断类型
                qop.setQuery("endType", "endType");
                ///添加时间起
                qop.setQuery("AddTime", "AddTimeStart");
                ///添加时间止
                qop.setQuery("AddTime", "AddTimeEnd");
                ///获取结果
                qop.setQuery("auto_status", "auto_status");
                ///页脚语句
                qop.SumSqlPart = "SUM(alertDuration) AS alertDuration,SUM(talkDuration) AS talkDuration";
                ///查询
                IList list = qop.QiList();
                status = 0;
                msg = "成功";
                count = qop.count;
                data = list;
                ///返回拼接
                return Json(new
                {
                    status = status,
                    msg = msg,
                    count = count,
                    data = data,
                    totalRow = qop.totalRow
                });
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***录音下载本地模式
        public JsonResult F_7RecLoad(string queryString)
        {
            try
            {
                List<m_cQuery> m_lQueryList = m_cQuery.m_fSetQueryList(queryString);
                ///外呼标识号非空
                string sessionIds = m_cQuery.m_fGetQueryString(m_lQueryList, "sessionIds");
                if (string.IsNullOrWhiteSpace(sessionIds))
                    throw new ArgumentNullException("sessionIds");

                DataTable m_pDataTable = new DataTable();
                if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)
                {
                    List<object> m_lObject = new List<object>();
                    foreach (DataRow item in m_pDataTable.Rows)
                    {
                        object m_oObject = new
                        {
                            sessionId = item["sessionId"]?.ToString(),
                            audioUrl = item["audioUrl"]?.ToString(),
                            audioType = item["audioType"]?.ToString()
                        };
                        m_lObject.Add(m_oObject);
                    }
                    data = m_lObject;
                    return rJson(m_cCcCfg.entID);
                }
                else
                {
                    msg = "请求无返回";
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                msg = ex.Message;
            }
            return eJson(m_cCcCfg.entID);
        }
        #endregion

        #region ***JSON字符串封装返回
        private JsonResult rJson(string entID = null)
        {
            return Json(new
            {
                status = status,
                msg = msg,
                count = count,
                data = data,
                uuid = m_cCmn.UUID(entID)
            });
        }

        private JsonResult eJson(string entID = null)
        {
            return Json(new
            {
                status = 1,
                msg = string.IsNullOrWhiteSpace(msg) ? (string.IsNullOrWhiteSpace(string.Empty) ? "未果" : string.Empty) : msg,
                count = 0,
                data = new { },
                uuid = m_cCmn.UUID(entID)
            });
        }
        #endregion

        #region ***上下文解析JSON参数
        private string m_fGetBodyJSON()
        {
            Stream stream = Request.InputStream;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            string m_sJSONStr = Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING).GetString(bytes);
            return m_sJSONStr;
        }
        #endregion

        #region ***返回流转格式
        private void m_fResponse(string m_sResponse)
        {
            Response.Charset = m_cCfg.SYSTEM_ENCODING;
            Response.ContentType = $"application/json;charset={m_cCfg.SYSTEM_ENCODING}";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING);
            Response.Write(m_sResponse);
        }
        #endregion
    }
}

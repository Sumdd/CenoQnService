using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Web;
using Newtonsoft.Json;
using System.Text;

namespace CenoQnService
{
    public class m_cHttp
    {
        public static string m_fPost(string url, IDictionary<object, object> param)
        {
            HttpWebRequest post;
            ///处理HTTPS
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((a, b, c, d) => { return true; });
                post = WebRequest.Create(url) as HttpWebRequest;
                post.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                post = WebRequest.Create(url) as HttpWebRequest;
            }

            post.Method = "POST"; //提交方式
            post.ContentType = "application/json;charset=UTF-8;"; //内容类型
            post.Accept = "*/*";
            post.Timeout = 15000;
            post.AllowAutoRedirect = false;

            ///处理企业编号
            string entID = string.Empty;
            if (!param.ContainsKey("entID"))
                throw new ArgumentNullException("entID");
            entID = param["entID"].ToString();
            ///处理企业安全码
            string entSecret = string.Empty;
            if (!param.ContainsKey("entSecret"))
                throw new ArgumentNullException("entSecret");
            entSecret = param["entSecret"].ToString();
            ///处理timestamp
            string timestamp = string.Empty;
            if (param.ContainsKey("timestamp"))
                param.Remove("timestamp");
            timestamp = DateTime.Now.ToString(m_cCfg.DATE_PATTERN);
            param["timestamp"] = timestamp;
            ///处理requestID
            string requestID = string.Empty;
            if (!param.ContainsKey("requestID"))
                throw new ArgumentNullException("requestID");
            else
                requestID = param["requestID"].ToString();
            ///处理mac
            string mac = string.Empty;
            if (param.ContainsKey("mac"))
                param.Remove("mac");
            mac = m_cDigest.m_fMD5($"{entID}{entSecret}{timestamp}{requestID}");
            param["mac"] = mac;

            ///移除头参数
            //param.Remove("entID");
            param.Remove("entSecret");
            //param.Remove("timestamp");
            //param.Remove("requestID");
            //param.Remove("mac");

            ///全部加载即可
            post.Headers.Add("entID", entID);
            post.Headers.Add("requestID", requestID);
            post.Headers.Add("timestamp", timestamp);
            post.Headers.Add("mac", mac);

            ///日志打印
            Log.Instance.Debug($"URL:{url}");
            Log.Instance.Debug($"entID:{entID}");
            //Log.Instance.Debug($"entSecret:{entSecret}");
            Log.Instance.Debug($"timestamp:{timestamp}");
            Log.Instance.Debug($"requestID:{requestID}");
            Log.Instance.Debug($"mac:{mac}");

            string jsonParamStr = JsonConvert.SerializeObject(param);
            Log.Instance.Debug($"JSON:{jsonParamStr}");

            StreamWriter requestStream = null;

            try
            {
                requestStream = new StreamWriter(post.GetRequestStream());
                requestStream.Write(jsonParamStr);
                requestStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            WebResponse response = null;
            string responseStr = null;

            try
            {
                //发送请求
                response = post.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING));
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Log.Instance.Debug($"Response:{responseStr}");

            return responseStr;
        }

        #region Post With Pic
        /// <summary>
        /// HTTP POST方式请求数据(带图片)
        /// </summary>
        /// <param name="url">URL</param>        
        /// <param name="param">POST的数据</param>
        /// <param name="fileByte">图片</param>
        /// <returns></returns>
        public static string m_fPost(string url, IDictionary<object, object> param, byte[] fileByte)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            ///处理HTTPS
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((a, b, c, d) => { return true; });
                wr = WebRequest.Create(url) as HttpWebRequest;
                wr.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                wr = WebRequest.Create(url) as HttpWebRequest;
            }

            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            ///处理企业编号
            string entID = string.Empty;
            if (!param.ContainsKey("entID"))
                throw new ArgumentNullException("entID");
            entID = param["entID"].ToString();
            ///处理企业安全码
            string entSecret = string.Empty;
            if (!param.ContainsKey("entSecret"))
                throw new ArgumentNullException("entSecret");
            entSecret = param["entSecret"].ToString();
            ///处理timestamp
            string timestamp = string.Empty;
            if (param.ContainsKey("timestamp"))
                param.Remove("timestamp");
            timestamp = DateTime.Now.ToString(m_cCfg.DATE_PATTERN);
            param["timestamp"] = timestamp;
            ///处理requestID
            string requestID = string.Empty;
            if (!param.ContainsKey("requestID"))
                throw new ArgumentNullException("requestID");
            else
                requestID = param["requestID"].ToString();
            ///处理mac
            string mac = string.Empty;
            if (param.ContainsKey("mac"))
                param.Remove("mac");
            mac = m_cDigest.m_fMD5($"{entID}{entSecret}{timestamp}{requestID}");
            param["mac"] = mac;

            ///日志打印
            Log.Instance.Debug($"URL:{url}");
            Log.Instance.Debug($"entID:{entID}");
            //Log.Instance.Debug($"entSecret:{entSecret}");
            Log.Instance.Debug($"timestamp:{timestamp}");
            Log.Instance.Debug($"requestID:{requestID}");
            Log.Instance.Debug($"mac:{mac}");

            Stream rs = wr.GetRequestStream();
            string responseStr = null;

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in param.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, param[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "file", "file.csv.gz", "application/gzip");//image/jpeg
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            rs.Write(fileByte, 0, fileByte.Length);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                responseStr = reader2.ReadToEnd();
                // logger.Error(string.Format("File uploaded, server response is: {0}", responseStr));
            }
            catch
            {
                //logger.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                throw;
            }

            Log.Instance.Debug($"Response:{responseStr}");

            return responseStr;
        }
        #endregion

        #region Get Test
        public static string m_fGet(string url, string postDataStr = "")
        {
            HttpWebRequest post;
            ///处理HTTPS
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((a, b, c, d) => { return true; });
                post = WebRequest.Create(url) as HttpWebRequest;
                post.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                post = WebRequest.Create(url) as HttpWebRequest;
            }

            post.Method = "POST"; //提交方式
            post.ContentType = "application/x-www-form-urlencoded; charset=UTF-8;"; //内容类型
            post.Accept = "*/*";
            post.Timeout = 15000;
            post.AllowAutoRedirect = false;

            StreamWriter requestStream = null;

            if (!string.IsNullOrWhiteSpace(postDataStr))
            {
                try
                {
                    requestStream = new StreamWriter(post.GetRequestStream());
                    requestStream.Write(postDataStr);
                    requestStream.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            WebResponse response = null;
            string responseStr = null;

            try
            {
                //发送请求
                response = post.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING));
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Log.Instance.Debug($"Response:{responseStr}");

            return responseStr;
        }
        #endregion

        #region ***下载内容
        public static Stream HttpGetStream(string Url, string postDataStr = "")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();

                ///将流转换成内存流
                var memoryStream = new MemoryStream();
                //将基础流写入内存流
                const int bufferLength = 1024;
                byte[] buffer = new byte[bufferLength];
                int actual = myResponseStream.Read(buffer, 0, bufferLength);
                while (actual > 0)
                {
                    memoryStream.Write(buffer, 0, actual);
                    actual = myResponseStream.Read(buffer, 0, bufferLength);
                }
                memoryStream.Position = 0;

                myResponseStream.Close();

                return memoryStream;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
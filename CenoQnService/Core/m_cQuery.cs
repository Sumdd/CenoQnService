using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    public class m_cQuery
    {
        public string m_sKey;
        public string m_sValue;

        #region ***方法
        public static List<m_cQuery> m_fSetQueryList(string queryString)
        {
            List<m_cQuery> m_lQueryList = new List<m_cQuery>();
            try
            {
                var IList = JsonConvert.DeserializeObject<IDictionary<string, string>>(queryString);
                foreach (KeyValuePair<string, string> item in IList)
                {
                    if (item.Value != null)
                    {
                        m_lQueryList.Add(
                            new m_cQuery()
                            {
                                m_sKey = item.Key,
                                m_sValue = item.Value
                            });
                    }
                }
                return m_lQueryList;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
            }
            return m_lQueryList;
        }

        public static string m_fGetQueryString(List<m_cQuery> m_lQueryList, string m_sKey)
        {
            try
            {
                return m_lQueryList?.Where(x => x.m_sKey == m_sKey)?.FirstOrDefault()?.m_sValue;
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
                return string.Empty;
            }
        }
        #endregion
    }
}
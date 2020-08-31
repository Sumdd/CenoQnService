using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Collections;

namespace CenoQnService
{
    public class m_cJSON
    {
        public static IList m_fDataTableToIList(DataTable m_pDataTable)
        {
            if (m_pDataTable != null && m_pDataTable.Rows.Count > 0)

                return m_pDataTable.Rows.Cast<DataRow>().Select(x =>
                {
                    return m_pDataTable.Columns.Cast<DataColumn>().Select(y =>
                    {
                        return new KeyValuePair<string, object>(y.ColumnName, x[y.ColumnName]);

                    }).ToDictionary(z => z.Key, z => z.Value);

                }).ToList();

            else return null;
        }

        public static string OKString(string requestID)
        {
            return "{\"code\":0,\"msg\":\"成功\",\"requestID\":\"" + requestID + "\"}";
        }

        public static string Err(string requestID, int code = -99, string m_sMsg = "失败")
        {
            m_mResponseJSON _m_mResponseJSON = new m_mResponseJSON();
            _m_mResponseJSON.code = code;
            _m_mResponseJSON.msg = m_sMsg;
            _m_mResponseJSON.requestID = requestID;
            return JsonConvert.SerializeObject(_m_mResponseJSON);
        }

        public static string Parse(object m_oObject)
        {
            if (m_oObject.GetType() == typeof(DataTable))
            {
                return JsonConvert.SerializeObject(m_cJSON.m_fDataTableToIList(m_oObject as DataTable));
            }

            return JsonConvert.SerializeObject(m_oObject);
        }
    }

    public class m_mResponseJSON
    {
        public int code
        {
            get;
            set;
        }
        public string msg
        {
            get;
            set;
        }
        public string requestID
        {
            get;
            set;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace CenoQnService
{
    public class QueryPager
    {
        private string _queryString;
        private QuerySample querySample;

        private string _WhereSqlPart = "WHERE 1=1\r\n";
        private string CountSql = string.Empty;
        private string QuerySql = string.Empty;
        private string SumSql = string.Empty;
        private bool isDistinct = false;
        private int SubIndex = 6;

        #region 兼容MySQL写法
        private string _paramMark = "@";
        private string _Provide = "SQL";
        public string Provide
        {
            get
            {
                return _Provide;
            }
            set
            {
                if (value == "MySQL")
                {
                    _paramMark = "?";
                }
                else
                {
                    _paramMark = "@";
                }
                _Provide = value;
            }
        }
        #endregion

        public bool isGetTotal = false;
        public string[] PrimaryKey;
        public string FieldsSqlPart = string.Empty;
        public string FromSqlPart = string.Empty;
        public string WhereSqlPart
        {
            get
            {
                return _WhereSqlPart + "\r\n";
            }
            set
            {
                _WhereSqlPart = (string.IsNullOrWhiteSpace(value)) ? "WHERE 1=1\r\n" : value;
            }
        }
        public string OrderSqlPart = string.Empty;
        public string SumSqlPart = string.Empty;

        public int count { get; set; } = 0;
        public object totalRow { get; set; }
        public Pager pager
        {
            get; set;
        }
        public string queryString
        {
            set
            {
                _queryString = value;
                querySample = QuerySwitch.ToQuerySample(_queryString);
                return;
            }
        }
        /// <summary>
        /// 设置查询参数
        /// </summary>
        /// <param name="args">参数字典集合</param>
        public void setQuerySample(Dictionary<string, object> args)
        {
            WhereSqlPart = _WhereSqlPart;
            querySample = new QuerySample();
            if (args == null)
                return;
            foreach (KeyValuePair<string, object> item in args)
            {
                querySample.QueryList.Add(new QueryModel() { Key = item.Key, Object = item.Value, Exist = false });
            }
        }
        /// <summary>
        /// 追加查询参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        public void setQuerySample(string key, object value)
        {
            if (querySample == null)
                querySample = new QuerySample();
            querySample.QueryList.Add(new QueryModel() { Key = key, Object = value, Exist = false });
        }
        /// <summary>
        /// 设置统计语句
        /// </summary>
        private void SetCountSql()
        {
            if (string.IsNullOrWhiteSpace(FromSqlPart))
                throw new Exception("未找到谓词FROM");

            //是否加入了去重,这里可能有问题,也没有测试复杂语句的问题
            if (isDistinct)
            {
                CountSql = string.Format("SELECT COUNT(1) AS total FROM ({0} {1} {2}) AS R0", FieldsSqlPart, FromSqlPart, WhereSqlPart);
                return;
            }
            CountSql = string.Format("SELECT COUNT(1) AS total {0} {1}", FromSqlPart, WhereSqlPart);
        }
        /// <summary>
        /// 设置分页查询语句
        /// </summary>
        private void SetQuerySql()
        {
            if (!FieldsSqlPart.StartsWith("SELECT DISTINCT", StringComparison.OrdinalIgnoreCase))
            {
                if (!FieldsSqlPart.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("查询语句需SELECT开头或SELECT DISTINCT");
            }
            else
            {
                isDistinct = true;
                SubIndex = 15;
            }

            if (pager == null)
                throw new Exception("未设置分页参数");

            var RightOrderSqlPart = OrderSqlPart;
            if (!string.IsNullOrWhiteSpace(pager.field))
                RightOrderSqlPart = string.Format("ORDER BY {0} {1}", pager.field, pager.type);

            if (string.IsNullOrWhiteSpace(RightOrderSqlPart))
                throw new Exception("未设置排序");

            if (isGetTotal)
            {
                QuerySql = string.Format("{0} {1} {2}", FieldsSqlPart, FromSqlPart, WhereSqlPart, RightOrderSqlPart);
                return;
            }
            else
            {

                if (Provide == "MySQL")
                {
                    #region MySQL分页查询语句
                    QuerySql = string.Format("SELECT {0}\r\n"
                            + "{1}\r\n"
                            + "{2}\r\n"
                            + "{3}\r\n"
                            + "LIMIT @limitStartValue,@limit",
                            FieldsSqlPart.Substring(SubIndex), FromSqlPart, WhereSqlPart, RightOrderSqlPart);
                    #endregion
                }
                else
                {
                    #region SQL分页查询语句
                    if (PrimaryKey != null)
                    {
                        string p1 = string.Empty;
                        string p2 = string.Empty;
                        if (PrimaryKey.Length == 2)
                        {
                            p1 = PrimaryKey[0];
                            p2 = PrimaryKey[1];
                        }
                        else
                        {
                            throw new Exception("主键数组必须是一对");
                        }

                        if (string.IsNullOrWhiteSpace(p1) || string.IsNullOrWhiteSpace(p2))
                            throw new Exception("主键数组数据不能为空或空字符串");

                        QuerySql = string.Format("SELECT TOP {7} {0}\r\n"
                            + "{1}\r\n"
                            + "{2}\r\n"
                            + "AND {3} NOT IN\r\n"
                            + "    (\r\n"
                            + "        SELECT TOP {8} {4} FROM\r\n"
                            + "            (\r\n"
                            + "                {5}\r\n"
                            + "                {1}\r\n"
                            + "                {2}\r\n"
                            + "            ) AS R0\r\n"
                            + "        {6}\r\n"
                            + "    )\r\n"
                            + "{6}\r\n",
                            FieldsSqlPart.Substring(SubIndex), FromSqlPart, WhereSqlPart, p1, p2, FieldsSqlPart, RightOrderSqlPart, pager.limit, (pager.limit * (pager.page - 1)));
                        return;
                    }
                    QuerySql = string.Format("SELECT * FROM\r\n"
                        + "(SELECT *,ROW_NUMBER() OVER ({3}) AS [rownum] FROM\r\n"
                        + "({0}\r\n{1}\r\n{2}) AS R0\r\n"
                        + ") AS R1\r\n"
                        + "WHERE [rownum] > ((@pageIndex-1)*@pageSize)\r\n"
                        + "AND [rownum] <= (@pageIndex*@pageSize)",
                        FieldsSqlPart, FromSqlPart, WhereSqlPart, RightOrderSqlPart);
                    #endregion
                }
            }
        }
        /// <summary>
        /// 写入查询条件
        /// </summary>
        /// <param name="aLikeName">相同名称</param>
        public void setQuery(string[] aLikeNameCollection)
        {
            foreach (var aLikeName in aLikeNameCollection)
            {
                setQuery(aLikeName, aLikeName);
            }
        }
        /// <summary>
        /// 写入查询条件
        /// </summary>
        /// <param name="sName">字段名称</param>
        /// <param name="qName">参数名称</param>
        public void setQuery(string sName, string qName)
        {
            if (querySample == null)
                throw new Exception("请在拼写查询前设置所有查询参数");
            var eqli = pager.eqli ?? "like";
            //拼接参数
            if (QuerySample.ExistKey(querySample, qName))
            {
                QueryModel queryModel = querySample.QueryList.FirstOrDefault(x => x.Key == $"{qName}Mark");
                if (queryModel != null)
                {
                    eqli = queryModel.Object.ToString();
                }
                if (eqli.Equals("Like", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Provide == "MySQL")
                        WhereSqlPart += string.Format("AND {0} {1} CONCAT('%',{3}{2},'%')\r\n", sName, eqli, qName, _paramMark);
                    else
                        WhereSqlPart += string.Format("AND {0} {1} '%' + {3}{2} + '%'\r\n", sName, eqli, qName, _paramMark);
                }
                else
                    WhereSqlPart += string.Format("AND {0} {1} {3}{2}\r\n", sName, eqli, qName, _paramMark);
            }
        }
        /// <summary>
        /// 判断是否存在即可
        /// </summary>
        /// <param name="qName"></param>
        public void existQuery(string qName)
        {
            if (querySample == null)
                throw new Exception("请在拼写查询前设置所有查询参数");
            QuerySample.ExistKey(querySample, qName);
        }
        /// <summary>
        /// 写入查询条件
        /// </summary>
        /// <param name="sName">字段名称</param>
        /// <param name="qName">参数名称</param>
        /// <param name="fun">参数名称</param>
        public void setQuery(string sName, string qName, string fun)
        {
            if (querySample == null)
                throw new Exception("请在拼写查询前设置所有查询参数");
            var eqli = pager.eqli ?? "like";
            //拼接参数
            if (QuerySample.ExistKey(querySample, qName))
            {
                QueryModel queryModel = querySample.QueryList.FirstOrDefault(x => x.Key == $"{qName}Mark");
                if (queryModel != null)
                {
                    eqli = queryModel.Object.ToString();
                }
                if (eqli.Equals("Like", StringComparison.InvariantCultureIgnoreCase))
                    WhereSqlPart += string.Format("AND {0} {1} CONCAT('%',{3}{2},'%')\r\n", sName, eqli, qName, _paramMark);
                else
                    WhereSqlPart += string.Format("AND {0} {1} {4}({3}{2})\r\n", sName, eqli, qName, _paramMark, fun);
            }
        }
        /// <summary>
        /// 直接拼接参数
        /// </summary>
        public void appQuery(string appSQL)
        {
            WhereSqlPart += appSQL;
        }
        #region 弃用
        /// <summary>
        /// 写入查询条件
        /// </summary>
        /// <param name="sName">字段名称</param>
        /// <param name="eqli">比较类型</param>
        /// <param name="qName">参数名称</param>
        /// <param name="vExtend">参数值拓展</param>
        [Obsolete("本项目弃用")]
        public void setQuery(string sName, string eqli, string qName, params string[] vExtend)
        {
            if (querySample == null)
                throw new Exception("请在拼写查询设置所有查询参数");
            if (QuerySample.ExistKey(querySample, qName))
            {
                var isLike = eqli.Equals("like", StringComparison.InvariantCultureIgnoreCase) ? "%" : "";
                if (vExtend != null)
                {
                    if (vExtend.Length == 1)
                    {
                        WhereSqlPart += string.Format("AND {0} {1} CONCAT('{5}',{4}{2},'{3}','{5}')\r\n", sName, eqli, qName, vExtend[0], _paramMark, isLike);
                        return;
                    }
                    else if (vExtend.Length == 2)
                    {
                        WhereSqlPart += string.Format("AND {0} {1} CONCAT('{6}','{3}',{5}{2},'{4}','{6}')\r\n", sName, eqli, qName, vExtend[0], vExtend[1], _paramMark, isLike);
                        return;
                    }
                }
                WhereSqlPart += string.Format("AND {0} {1} CONCAT('{4}',{3}{2},'{4}')\r\n", sName, eqli, qName, _paramMark, isLike);
            }
        }
        #endregion
        public void SetSumSql(string m_sSUMSQL = null)
        {
            if (m_sSUMSQL != null)
            {
                SumSql = m_sSUMSQL;
                return;
            }

            if (string.IsNullOrWhiteSpace(SumSqlPart))
                return;
            SumSql = string.Format("SELECT {0}\r\n"
                + "{1}\r\n"
                + "{2}",
              SumSqlPart, FromSqlPart, WhereSqlPart);
        }
        /// <summary>
        /// 执行并得到数据结果集
        /// </summary>
        /// <returns></returns>
        public DataSet QdataSet()
        {
            SqlSugarClient m_pEsyClient = new m_cSugar().EasyClient;
            List<SugarParameter> paramList = new List<SugarParameter>();
            foreach (QueryModel queryModel in querySample.QueryList)
            {
                if (queryModel.Exist)
                {
                    paramList.Add(new SugarParameter(string.Format("{1}{0}", queryModel.Key, _paramMark), queryModel.Object));
                }
            }
            SetQuerySql();
            if (isGetTotal)
            {
                isGetTotal = false;
                return m_pEsyClient.Ado.GetDataSetAll(QuerySql, paramList.ToArray());
            }
            SetCountSql();
            SetSumSql();
            var asMySQL = $"{CountSql};\r\n{QuerySql};\r\n{SumSql}"
                  .Replace("@limitStartValue", ((pager.page - 1) * pager.limit).ToString())
                  .Replace("@limit", pager.limit.ToString())
                  .Replace("@pageIndex", pager.page.ToString())
                  .Replace("@pageSize", pager.limit.ToString())
                  ;
            return m_pEsyClient.Ado.GetDataSetAll(asMySQL, paramList.ToArray());
        }
        /// <summary>
        /// 执行并得到数据结果集
        /// </summary>
        /// <returns></returns>
        public IList QiList()
        {
            //返回查询到的数据结果集
            DataSet ds = QdataSet();

            ///处理页脚的
            if (ds.Tables.Count == 3)
            {
                DataTable m_pDataTable3 = ds.Tables[2].Copy();

                if (m_pDataTable3 != null && m_pDataTable3.Rows.Count > 0)
                {
                    IDictionary<string, object> idi = new Dictionary<string, object>();
                    for (int j = 0; j < m_pDataTable3.Columns.Count; j++)
                    {
                        idi.Add(new KeyValuePair<string, object>(m_pDataTable3.Columns[j].ColumnName, m_pDataTable3.Rows[0][j]));
                    }
                    totalRow = idi;
                }
                ds.Tables.RemoveAt(2);
            }

            //处理成直接可用的数据
            if (ds.Tables.Count == 2)
            {
                count = Convert.ToInt32(ds.Tables[0].Rows[0]["total"]);
                IList iList = new List<IDictionary<string, object>>();

                DataTable dt = ds.Tables[1];
                //循环写入值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IDictionary<string, object> idi = new Dictionary<string, object>();
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        idi.Add(new KeyValuePair<string, object>(dt.Columns[j].ColumnName, dt.Rows[i][j]));
                    }
                    iList.Add(idi);
                }
                //返回处理好的泛型,方便使用
                return iList;
            }

            throw new Exception("结果集需为总行、数据俩张表");
        }
    }

    /// <summary>
    /// 分页
    /// </summary>
    public class Pager
    {
        /// <summary>
        /// 第几页
        /// </summary>
        public int page
        {
            get; set;
        }
        /// <summary>
        /// 每页多少条
        /// </summary>
        public int limit
        {
            get; set;
        }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string field
        {
            get; set;
        }
        /// <summary>
        /// 私人排序类型变量
        /// </summary>
        private string _type;
        /// <summary>
        /// 排序类型
        /// </summary>
        public string type
        {
            get
            {
                return _type == null ? "asc" : _type;
            }
            set
            {
                _type = value;
            }
        }
        /// <summary>
        /// 公开查询类型变量
        /// </summary>
        public string eqli
        {
            get; set;
        }
    }

    /// <summary>
    /// 查询字符串转换帮助类
    /// </summary>
    public class QuerySwitch
    {
        /// <summary>
        /// 将查询字符串解析成QueryModel数组
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static QuerySample ToQuerySample(string queryString)
        {
            List<QueryModel> queryList = new List<QueryModel>();

            //无次参数直接返回
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return new QuerySample() { QueryList = new List<QueryModel>() };
            }

            var IList = JsonConvert.DeserializeObject<IDictionary<string, object>>(queryString);

            foreach (KeyValuePair<string, object> item in IList)
            {
                List<QueryModel> list = queryList.Where(q => q.Key == item.Key).ToList();

                //存在则移除
                if (list.Count > 0)
                {
                    list.RemoveAll(q => q.Key == item.Key);
                }

                //添加非空字段
                if (item.Value != null)
                {
                    //然后添加,保证参数唯一
                    queryList.Add(
                        new QueryModel()
                        {
                            Key = item.Key,
                            Object = item.Value
                        });
                }
            }

            return new QuerySample() { QueryList = queryList };
        }
    }

    /// <summary>
    /// 查询简类
    /// </summary>
    public class QuerySample
    {
        public List<QueryModel> QueryList { get; set; } = new List<QueryModel>();
        public object Object
        {
            get; set;
        }

        public static bool ExistKey(QuerySample querySample, string Key)
        {
            foreach (QueryModel queryModel in querySample.QueryList)
            {
                if (queryModel.Key == Key)
                {
                    queryModel.Exist = true;
                    querySample.Object = queryModel.Object;
                    return true;
                }
            }
            querySample.Object = null;
            return false;
        }
    }
    /// <summary>
    /// 查询模型
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// 查询键
        /// </summary>
        public string Key
        {
            get; set;
        }
        /// <summary>
        /// 查询值
        /// </summary>
        public object Object
        {
            get; set;
        }
        /// <summary>
        /// 被证明存在
        /// </summary>
        public bool Exist { get; set; } = false;
    }
}

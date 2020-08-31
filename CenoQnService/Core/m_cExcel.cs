using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace CenoQnService
{
    public class m_cExcel
    {
        public static DataSet m_fToDataSet(HttpPostedFileBase file)
        {
            ///直接返回
            if (file == null) return null;

            IExcelDataReader excelReader;

            if (Path.GetExtension(file.FileName).ToUpper() == ".XLS")
            {
                //1.1 Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateBinaryReader(file.InputStream);
            }
            else
            {
                //1.2 Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(file.InputStream);
            }

            return excelReader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = x => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });
        }

        public static DataTable m_fGetSheet1(DataSet ds)
        {
            try
            {
                DataTable dt = null;
                foreach (DataTable item in ds.Tables)
                {
                    if (new List<string>() { "Sheet1", "Sheet1$" }.Contains(item.TableName))
                    {
                        dt = item.Copy();
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Debug(ex);
            }
            return null;
        }
    }
}
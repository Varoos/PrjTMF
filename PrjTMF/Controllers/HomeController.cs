using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Focus.Common.DataStructs;
using Newtonsoft.Json;
using PrjTMF.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace PrjTMF.Controllers
{
    public class HomeController : Controller
    {
        string errors1 = "";
        public ActionResult CFIndex(int CompanyId)
        {
            try
            {
                ViewBag.CompId = CompanyId;
                DBClass.SetLog("Entered to external Module with CompanyId = " + CompanyId);
                return View();
            }
            catch (Exception ex)
            {
                DBClass.SetLog("INdex. Exception = " + ex.Message);
                return null;
            }
        }
        public ActionResult CFReport2(CFFilter obj)
        {
            try
            {
                CFCls _cls = new CFCls();
                CFFilter _filter = new CFFilter();
                _cls._filter = obj;
                int CompanyId = Convert.ToInt32(obj.CompanyId);
                string retrievequery = string.Format($@"exec CashFlowReport @year={obj.Year}, @type='{obj.Type}'");
                DBClass.SetLog("CFReport retrievequery = " + retrievequery);
                DataSet ds1 = DBClass.GetData(retrievequery, CompanyId, ref errors1);
                if (ds1 != null)
                {
                    DBClass.SetLog("Getting Report View. CFReport DataSet is not null");
                    if (ds1.Tables.Count > 0)
                    {
                        DBClass.SetLog("Getting Report View. CFReport DataSet Table count >0 ");
                        List<CFList> listobj = new List<CFList>();
                        if (ds1.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr1 in ds1.Tables[0].Rows)
                            {
                                listobj.Add(new CFList
                                {
                                    Head = dr1["head"].ToString(),
                                    Jan = dr1["1"].ToString(),
                                    Feb = dr1["2"].ToString(),
                                    Mar = dr1["3"].ToString(),
                                    Apr = dr1["4"].ToString(),
                                    May = dr1["5"].ToString(),
                                    Jun = dr1["6"].ToString(),
                                    Jul = dr1["7"].ToString(),
                                    Aug = dr1["8"].ToString(),
                                    Sep = dr1["9"].ToString(),
                                    Oct = dr1["10"].ToString(),
                                    Nov = dr1["11"].ToString(),
                                    Dec = dr1["12"].ToString(),
                                    Year = dr1["1"].ToString(),
                                    FirstQtr = dr1["1"].ToString(),
                                    SecondQtr = dr1["2"].ToString(),
                                    ThirdQtr = dr1["3"].ToString(),
                                    FourthQtr = dr1["4"].ToString(),
                                    b = Convert.ToInt32(dr1["b"].ToString()),
                                    cf = Convert.ToInt32(dr1["cf"].ToString()),
                                });
                            }
                            var cf1count = listobj.Where(x => x.cf == 1).Count();
                            var cf2count = listobj.Where(x => x.cf == 2).Count();
                            var cf3count = listobj.Where(x => x.cf == 3).Count();
                            if(cf1count == 0)
                            {
                                listobj = listobj.Where(x => x.cf != 5).ToList();
                            }
                            if (cf2count == 0)
                            {
                                listobj = listobj.Where(x => x.cf != 6).ToList();
                            }
                            if (cf3count == 0)
                            {
                                listobj = listobj.Where(x => x.cf != 7).ToList();
                            }
                            _cls._list = listobj;
                            Session["CFData"] = _cls;
                            if (_cls == null)
                            {
                                return Json("No Data", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("Success", JsonRequestBehavior.AllowGet);
                            }
                            DBClass.SetLog("Getting Report View. ExpenseReportData body data is ready");
                        }
                        else
                        {
                            return Json("No Data", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("No Data", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(errors1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DBClass.SetLog("Getting Report View. EXCEPTION = " + ex.Message);
                return null;
            }
        }
        public ActionResult CFReport()
        {
            CFCls _data = (CFCls)Session["CFData"];
            return View(_data);
        }
        public FileResult CFExcel()
        {
            CFCls _data = (CFCls)Session["CFData"];
            CFFilter _head = new CFFilter();
            _head = _data._filter;
            List<CFList> _list = new List<CFList>();
            _list = _data._list;
            System.Data.DataTable data = new System.Data.DataTable("Cash Flow Statements");
            #region DataColumns
            data.Columns.Add("Head", typeof(string));
            data.Columns.Add("Year", typeof(decimal));
            data.Columns.Add("FirstQtr", typeof(decimal));
            data.Columns.Add("SecondQtr", typeof(decimal));
            data.Columns.Add("ThirdQtr", typeof(decimal));
            data.Columns.Add("FourthQtr", typeof(decimal));
            data.Columns.Add("Jan", typeof(decimal));
            data.Columns.Add("Feb", typeof(decimal));
            data.Columns.Add("Mar", typeof(decimal));
            data.Columns.Add("Apr", typeof(decimal));
            data.Columns.Add("May", typeof(decimal));
            data.Columns.Add("Jun", typeof(decimal));
            data.Columns.Add("Jul", typeof(decimal));
            data.Columns.Add("Aug", typeof(decimal));
            data.Columns.Add("Sep", typeof(decimal));
            data.Columns.Add("Oct", typeof(decimal));
            data.Columns.Add("Nov", typeof(decimal));
            data.Columns.Add("Dec", typeof(decimal));
            data.Columns.Add("b", typeof(int));
            data.Columns.Add("cf", typeof(int));
            #endregion


            if (_head.Type == "Year")
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Cash Flow Statements");
                    var dataTable = data;

                    var wsReportNameHeaderRange = ws.Range(ws.Cell(1, 2), ws.Cell(1, 5));
                    wsReportNameHeaderRange.Style.Font.Bold = true;
                    wsReportNameHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsReportNameHeaderRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                    wsReportNameHeaderRange.Merge();
                    wsReportNameHeaderRange.Value = "Cash Flow Statements";

                    int r = 3;
                    int cell = 2;
                    ws.Cell(r, 2).Value = "Year";
                    ws.Cell(r, 3).Value = _head.Year;
                    ws.Cell(r, 4).Value = "Report Type";
                    ws.Cell(r, 5).Value = _head.Type;



                    var TableRange = ws.Range(ws.Cell(3, 2), ws.Cell(r, 5));
                    TableRange.Style.Fill.BackgroundColor = XLColor.White;
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    r = 5;
                    cell = 2;
                    for (int i = 1; i < data.Columns.Count; i++)
                    {
                        cell = 2;
                        #region Headers
                        ws.Range(ws.Cell(r, 2), ws.Cell(r, 4)).Merge().Value = "";
                        ws.Cell(r, 5).Value = _head.Year;
                        #endregion
                    }
                    TableRange = ws.Range(ws.Cell(r, 2), ws.Cell(r, 5));
                    TableRange.Style.Font.FontColor = XLColor.White;
                    TableRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 115, 170);
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int c = 2;

                    #region TableLoop
                    foreach (var obj in _list)
                    {
                        c = 2;
                        r++;
                        ws.Range(ws.Cell(r, 2), ws.Cell(r, 4)).Merge().Value = obj.Head;
                        ws.Cell(r, 5).Value = obj.Year;
                        if (obj.b == 1)
                        {
                            ws.Range(ws.Cell(r, 2), ws.Cell(r, 5)).Style.Font.Bold = true;
                        }
                    }

                    #endregion

                    TableRange = ws.Range(ws.Cell(6, 2), ws.Cell(r, 5));
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range(ws.Cell(6, 5), ws.Cell(r, 5)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(ws.Cell(6, 5), ws.Cell(r, 5)).Style.NumberFormat.Format = "0.00";
                    ws.Columns("A:BZ").AdjustToContents();

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashFlowStatements_Yearly" + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                    }
                }

            }
            else if (_head.Type == "Quarter")
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Cash Flow Statements");
                    var dataTable = data;

                    var wsReportNameHeaderRange = ws.Range(ws.Cell(1, 2), ws.Cell(1, 6));
                    wsReportNameHeaderRange.Style.Font.Bold = true;
                    wsReportNameHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsReportNameHeaderRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                    wsReportNameHeaderRange.Merge();
                    wsReportNameHeaderRange.Value = "Cash Flow Statements";

                    int r = 3;
                    int cell = 2;
                    ws.Cell(r, 2).Value = "Year";
                    ws.Cell(r, 3).Value = _head.Year;
                    ws.Cell(r, 5).Value = "Report Type";
                    ws.Cell(r, 6).Value = _head.Type;

                    var TableRange = ws.Range(ws.Cell(3, 2), ws.Cell(r, 6));
                    TableRange.Style.Fill.BackgroundColor = XLColor.White;
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    r = 5;
                    cell = 2;
                    for (int i = 1; i < data.Columns.Count; i++)
                    {
                        cell = 2;
                        #region Headers
                        ws.Cell(r, cell++).Value = "";
                        ws.Cell(r, cell++).Value = "First Qtr " + _head.Year;
                        ws.Cell(r, cell++).Value = "Second Qtr " + _head.Year;
                        ws.Cell(r, cell++).Value = "Third Qtr " + _head.Year;
                        ws.Cell(r, cell++).Value = "Fourth Qtr " + _head.Year;
                        #endregion
                    }
                    TableRange = ws.Range(ws.Cell(r, 2), ws.Cell(r, 6));
                    TableRange.Style.Font.FontColor = XLColor.White;
                    TableRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 115, 170);
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int c = 2;

                    #region TableLoop
                    foreach (var obj in _list)
                    {
                        c = 2;
                        r++;
                        ws.Cell(r, c++).Value = obj.Head;
                        ws.Cell(r, c++).Value = obj.FirstQtr;
                        ws.Cell(r, c++).Value = obj.SecondQtr;
                        ws.Cell(r, c++).Value = obj.ThirdQtr;
                        ws.Cell(r, c++).Value = obj.FourthQtr;
                        if (obj.b == 1)
                        {
                            ws.Range(ws.Cell(r, 2), ws.Cell(r, 6)).Style.Font.Bold = true;
                        }
                    }

                    #endregion

                    TableRange = ws.Range(ws.Cell(6, 2), ws.Cell(r, 6));
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range(ws.Cell(6, 3), ws.Cell(r, 6)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(ws.Cell(6, 3), ws.Cell(r, 6)).Style.NumberFormat.Format = "0.00";
                    ws.Columns("A:BZ").AdjustToContents();

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashFlowStatements_Quarterly" + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                    }
                }
            }
            else
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Cash Flow Statements");
                    var dataTable = data;

                    var wsReportNameHeaderRange = ws.Range(ws.Cell(1, 2), ws.Cell(1, 14));
                    wsReportNameHeaderRange.Style.Font.Bold = true;
                    wsReportNameHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsReportNameHeaderRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                    wsReportNameHeaderRange.Merge();
                    wsReportNameHeaderRange.Value = "Cash Flow Statements";

                    int r = 3;
                    int cell = 2;
                    ws.Range(ws.Cell(r, 2), ws.Cell(r, 4)).Merge().Value = "Year";
                    ws.Range(ws.Cell(r, 5), ws.Cell(r, 7)).Merge().Value =_head.Year;
                    ws.Range(ws.Cell(r, 9), ws.Cell(r, 11)).Merge().Value = "Report Type";
                    ws.Range(ws.Cell(r, 12), ws.Cell(r, 14)).Merge().Value = _head.Type;

                    var TableRange = ws.Range(ws.Cell(3, 2), ws.Cell(r, 14));
                    TableRange.Style.Fill.BackgroundColor = XLColor.White;
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(ws.Cell(3, 2), ws.Cell(r, 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(ws.Cell(3, 9), ws.Cell(r, 11)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Range(ws.Cell(3, 5), ws.Cell(r, 7)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range(ws.Cell(3, 12), ws.Cell(r, 14)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    r = 5;
                    cell = 2;
                    for (int i = 1; i < data.Columns.Count; i++)
                    {
                        cell = 2;
                        #region Headers
                        ws.Cell(r, cell++).Value = "";
                        ws.Cell(r, cell++).Value = "Jan_"+_head.Year;
                        ws.Cell(r, cell++).Value = "Feb_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Mar_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Apr_" + _head.Year;
                        ws.Cell(r, cell++).Value = "May_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Jun_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Jul_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Aug_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Sep_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Oct_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Nov_" + _head.Year;
                        ws.Cell(r, cell++).Value = "Dec_" + _head.Year;
                        #endregion
                    }
                    TableRange = ws.Range(ws.Cell(r, 2), ws.Cell(r, 14));
                    TableRange.Style.Font.FontColor = XLColor.White;
                    TableRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 115, 170);
                    TableRange.Style.Font.Bold = true;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    int c = 2;

                    #region TableLoop
                    foreach (var obj in _list)
                    {
                        c = 2;
                        r++;
                        ws.Cell(r, c++).Value = obj.Head;
                        ws.Cell(r, c++).Value = obj.Jan;
                        ws.Cell(r, c++).Value = obj.Feb;
                        ws.Cell(r, c++).Value = obj.Mar;
                        ws.Cell(r, c++).Value = obj.Apr;
                        ws.Cell(r, c++).Value = obj.May;
                        ws.Cell(r, c++).Value = obj.Jun;
                        ws.Cell(r, c++).Value = obj.Jul;
                        ws.Cell(r, c++).Value = obj.Aug;
                        ws.Cell(r, c++).Value = obj.Sep;
                        ws.Cell(r, c++).Value = obj.Oct;
                        ws.Cell(r, c++).Value = obj.Nov;
                        ws.Cell(r, c++).Value = obj.Dec;
                        if (obj.b == 1)
                        {
                            ws.Range(ws.Cell(r, 2), ws.Cell(r, 14)).Style.Font.Bold = true;
                        }
                    }

                    #endregion

                    TableRange = ws.Range(ws.Cell(6, 2), ws.Cell(r, 14));
                    TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Range(ws.Cell(6, 3), ws.Cell(r, 14)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Range(ws.Cell(6, 3), ws.Cell(r, 14)).Style.NumberFormat.Format = "0.00";
                    ws.Columns("A:BZ").AdjustToContents();

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashFlowStatemets_Monthly" + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                    }
                }
            }
        }
    }
}
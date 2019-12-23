using Daimler.Api.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Daimler.Api.Operations
{
    public class ExcelCreator
    {
        public static void Create(UserPasswordInfo userinfo)
        {
            //TODO exceli burada oluştur
           Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
           Excel.Workbook xlWorkBook;
           Excel.Worksheet xlWorkSheet;
           object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlWorkSheet.Cells[1, 1] = "Application Name";
            xlWorkSheet.Cells[1, 2] = "Email";
            xlWorkSheet.Cells[1, 3] = "Username";
            xlWorkSheet.Cells[2, 1] = userinfo.Application;
            xlWorkSheet.Cells[2, 3] = userinfo.UserName;
            xlWorkSheet.Cells[2, 2] = userinfo.Email;

            xlApp.DisplayAlerts = false;
            xlWorkBook.SaveAs("C:\\Users\\BaranOzsarac\\Documents\\" + userinfo.UserName + ".xlsx", Excel.XlFileFormat.xlOpenXMLWorkbook, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

        }
    }
}

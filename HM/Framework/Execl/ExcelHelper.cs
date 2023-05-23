using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace HM.Framework.Execl
{
	public class ExcelHelper
	{
		private static int maxRow = 50000;

		private static Stream ExportDataSetToExcel(DataSet sourceDs)
		{
			HSSFWorkbook hSSFWorkbook = new HSSFWorkbook();
			MemoryStream memoryStream = new MemoryStream();
			for (int i = 0; i < sourceDs.Tables.Count; i++)
			{
				DataTable dataTable = sourceDs.Tables[i];
				ISheet sheet = hSSFWorkbook.CreateSheet(dataTable.TableName);
				IRow row = sheet.CreateRow(0);
				foreach (DataColumn column in dataTable.Columns)
				{
					row.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
				}
				int num = 1;
				foreach (DataRow row3 in dataTable.Rows)
				{
					IRow row2 = sheet.CreateRow(num);
					foreach (DataColumn column2 in dataTable.Columns)
					{
						row2.CreateCell(column2.Ordinal).SetCellValue(row3[column2].ToString());
					}
					num++;
				}
			}
			hSSFWorkbook.Write(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			hSSFWorkbook = null;
			return memoryStream;
		}

		private static Stream ExportDataSetToExcel(DataTable sourceDs, string filed)
		{
			var enumerable = from t in sourceDs.AsEnumerable()
			group t by new
			{
				t1 = t.Field<string>(filed)
			};
			HSSFWorkbook hSSFWorkbook = new HSSFWorkbook();
			MemoryStream memoryStream = new MemoryStream();
			foreach (var item in enumerable)
			{
				int num = 1;
				string text = Convert.ToString(item.Key.t1);
				text = (string.IsNullOrEmpty(text) ? " " : text);
				if (sourceDs.Rows.Count > maxRow)
				{
					text += num;
				}
				ISheet sheet = hSSFWorkbook.CreateSheet(text);
				IRow row = sheet.CreateRow(0);
				foreach (DataColumn column in sourceDs.Columns)
				{
					row.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
				}
				int num2 = 1;
				foreach (DataRow item2 in item.ToList())
				{
					if (num2 > maxRow)
					{
						num++;
						num2 = 1;
						sheet = hSSFWorkbook.CreateSheet(item.Key.t1.ToString() + num);
						row = sheet.CreateRow(0);
						foreach (DataColumn column2 in sourceDs.Columns)
						{
							row.CreateCell(column2.Ordinal).SetCellValue(column2.ColumnName);
						}
					}
					IRow row2 = sheet.CreateRow(num2);
					foreach (DataColumn column3 in sourceDs.Columns)
					{
						row2.CreateCell(column3.Ordinal).SetCellValue(item2[column3].ToString());
					}
					num2++;
				}
			}
			hSSFWorkbook.Write(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			hSSFWorkbook = null;
			return memoryStream;
		}

		public static void ExportDataTableToExcelByField(DataTable sourceDs, string filed, string fileName)
		{
			MemoryStream memoryStream = ExportDataSetToExcel(sourceDs, filed) as MemoryStream;
			HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
			HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
			HttpContext.Current.Response.End();
			memoryStream.Close();
			memoryStream = null;
		}

		public static void ExportDataSetToExcel(DataSet sourceDs, string fileName)
		{
			MemoryStream memoryStream = ExportDataSetToExcel(sourceDs) as MemoryStream;
			HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
			HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
			HttpContext.Current.Response.End();
			memoryStream.Close();
			memoryStream = null;
		}

		private static Stream ExportDataTableToExcel(DataTable sourceTable, string sheetName)
		{
			int num = 1;
			HSSFWorkbook hSSFWorkbook = new HSSFWorkbook();
			MemoryStream memoryStream = new MemoryStream();
			string text = sheetName;
			if (sourceTable.Rows.Count > maxRow)
			{
				text += num;
			}
			ISheet sheet = hSSFWorkbook.CreateSheet(text);
			IRow row = sheet.CreateRow(0);
			foreach (DataColumn column in sourceTable.Columns)
			{
				row.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
			}
			int num2 = 1;
			foreach (DataRow row3 in sourceTable.Rows)
			{
				if (num2 > maxRow)
				{
					num++;
					sheet = hSSFWorkbook.CreateSheet(sheetName + num);
					row = sheet.CreateRow(0);
					foreach (DataColumn column2 in sourceTable.Columns)
					{
						row.CreateCell(column2.Ordinal).SetCellValue(column2.ColumnName);
					}
					num2 = 1;
				}
				IRow row2 = sheet.CreateRow(num2);
				int num3 = 0;
				foreach (DataColumn column3 in sourceTable.Columns)
				{
					string text2 = row3[column3].ToString();
					if (text2.ToLower().StartsWith("images:"))
					{
						AddPieChart(sheet, hSSFWorkbook, "http://www.baidu.com/img/baidu_jgylogo3.gif", num2, num3);
					}
					else
					{
						row2.CreateCell(column3.Ordinal).SetCellValue(text2);
					}
					num3++;
				}
				num2++;
			}
			hSSFWorkbook.Write(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			sheet = null;
			row = null;
			hSSFWorkbook = null;
			return memoryStream;
		}

		public static void ExportDataTableToExcel(DataTable sourceTable, string fileName, string sheetName)
		{
			MemoryStream memoryStream = ExportDataTableToExcel(sourceTable, sheetName) as MemoryStream;
			HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
			HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
			HttpContext.Current.Response.End();
			memoryStream.Close();
			memoryStream = null;
		}

		public static void ExportDataTableToExcelModel(DataTable sourceTable, string modelpath, string modelName, string fileName, string sheetName)
		{
			int num = 2;
			int num2 = 0;
			FileStream fileStream = new FileStream(modelpath + modelName + ".xls", FileMode.Open, FileAccess.Read);
			HSSFWorkbook hSSFWorkbook = new HSSFWorkbook(fileStream);
			HSSFSheet hSSFSheet = (HSSFSheet)hSSFWorkbook.GetSheet("Sheet1");
			hSSFSheet.GetRow(0).GetCell(0).SetCellValue("excelTitle");
			foreach (DataRow row2 in sourceTable.Rows)
			{
				num++;
				num2 = 0;
				IRow row = hSSFSheet.CreateRow(num);
				foreach (DataColumn column in sourceTable.Columns)
				{
					row.CreateCell(num2).SetCellValue(row2[column.ColumnName].ToString());
					num2++;
				}
			}
			hSSFSheet.ForceFormulaRecalculation = true;
			FileStream fileStream2 = new FileStream(modelpath + fileName + ".xls", FileMode.Create);
			hSSFWorkbook.Write(fileStream2);
			fileStream2.Close();
			fileStream.Close();
		}

		public static DataTable ImportDataTableFromExcel(Stream excelFileStream, string sheetName, int headerRowIndex)
		{
			ISheet sheet = WorkbookFactory.Create(excelFileStream).GetSheet(sheetName);
			DataTable dataTable = new DataTable();
			IRow row = sheet.GetRow(headerRowIndex);
			int lastCellNum = row.LastCellNum;
			for (int i = row.FirstCellNum; i < lastCellNum; i++)
			{
				DataColumn column = new DataColumn(row.GetCell(i).StringCellValue);
				dataTable.Columns.Add(column);
			}
			for (int j = sheet.FirstRowNum + 1; j <= sheet.LastRowNum; j++)
			{
				IRow row2 = sheet.GetRow(j);
				DataRow dataRow = dataTable.NewRow();
				for (int k = row2.FirstCellNum; k < lastCellNum; k++)
				{
					dataRow[k] = row2.GetCell(k).ToString();
				}
			}
			excelFileStream.Close();
			sheet = null;
			return dataTable;
		}

		public static DataTable ImportDataTableFromExcel(string excelFilePath, string sheetName, int headerRowIndex)
		{
			using (FileStream excelFileStream = File.OpenRead(excelFilePath))
			{
				return ImportDataTableFromExcel(excelFileStream, sheetName, headerRowIndex);
			}
		}

		public static DataTable ImportDataTableFromExcel(Stream excelFileStream, int sheetIndex = 0, int headerRowIndex = 0)
		{
			ISheet sheetAt = WorkbookFactory.Create(excelFileStream).GetSheetAt(sheetIndex);
			DataTable dataTable = new DataTable();
			IRow row = sheetAt.GetRow(headerRowIndex);
			int num = row.LastCellNum;
			for (int i = row.FirstCellNum; i < num; i++)
			{
				if (row.GetCell(i) == null || row.GetCell(i).StringCellValue.Trim() == "")
				{
					num = i + 1;
					break;
				}
				DataColumn column = new DataColumn(row.GetCell(i).StringCellValue);
				dataTable.Columns.Add(column);
			}
			for (int j = sheetAt.FirstRowNum + 1; j <= sheetAt.LastRowNum; j++)
			{
				IRow row2 = sheetAt.GetRow(j);
				if (row2 == null || row2.GetCell(0) == null || row2.GetCell(0).ToString().Trim() == "")
				{
					break;
				}
				DataRow dataRow = dataTable.NewRow();
				for (int k = row2.FirstCellNum; k < num; k++)
				{
					dataRow[k] = row2.GetCell(k);
				}
				dataTable.Rows.Add(dataRow);
			}
			excelFileStream.Close();
			sheetAt = null;
			return dataTable;
		}

		public static DataTable ImportDataTableFromExcel(string excelFilePath, int sheetIndex, int headerRowIndex)
		{
			using (FileStream excelFileStream = File.OpenRead(excelFilePath))
			{
				return ImportDataTableFromExcel(excelFileStream, sheetIndex, headerRowIndex);
			}
		}

		public static DataSet ImportDataSetFromExcel(Stream excelFileStream, int headerRowIndex)
		{
			DataSet dataSet = new DataSet();
			IWorkbook workbook = WorkbookFactory.Create(excelFileStream);
			int i = 0;
			for (int numberOfSheets = workbook.NumberOfSheets; i < numberOfSheets; i++)
			{
				ISheet sheetAt = workbook.GetSheetAt(i);
				DataTable dataTable = new DataTable();
				IRow row = sheetAt.GetRow(headerRowIndex);
				int num = row.LastCellNum;
				for (int j = row.FirstCellNum; j < num; j++)
				{
					if (row.GetCell(j) == null || row.GetCell(j).StringCellValue.Trim() == "")
					{
						num = j + 1;
						break;
					}
					DataColumn column = new DataColumn(row.GetCell(j).StringCellValue);
					dataTable.Columns.Add(column);
				}
				for (int k = sheetAt.FirstRowNum + 1; k <= sheetAt.LastRowNum; k++)
				{
					IRow row2 = sheetAt.GetRow(k);
					if (row2 == null || row2.GetCell(0) == null || row2.GetCell(0).ToString().Trim() == "")
					{
						break;
					}
					DataRow dataRow = dataTable.NewRow();
					for (int l = row2.FirstCellNum; l < num; l++)
					{
						if (row2.GetCell(l) != null)
						{
							dataRow[l] = row2.GetCell(l).ToString();
						}
					}
					dataTable.Rows.Add(dataRow);
				}
				dataSet.Tables.Add(dataTable);
			}
			excelFileStream.Close();
			workbook = null;
			return dataSet;
		}

		public static DataSet ImportDataSetFromExcel(string excelFilePath, int headerRowIndex)
		{
			using (FileStream excelFileStream = File.OpenRead(excelFilePath))
			{
				return ImportDataSetFromExcel(excelFileStream, headerRowIndex);
			}
		}

		public static string ConvertColumnIndexToColumnName(int index)
		{
			index++;
			int num = 26;
			char[] array = new char[100];
			int num2 = 0;
			while (index > 0)
			{
				int num3 = index % num;
				if (num3 == 0)
				{
					num3 = num;
				}
				array[num2++] = (char)(num3 - 1 + 65);
				index = (index - 1) / 26;
			}
			StringBuilder stringBuilder = new StringBuilder(num2);
			for (int num5 = num2 - 1; num5 >= 0; num5--)
			{
				stringBuilder.Append(array[num5]);
			}
			return stringBuilder.ToString();
		}

		public static DateTime ConvertDate(string date)
		{
			DateTime dateTime = default(DateTime);
			string[] array = date.Split('-');
			int value = Convert.ToInt32(array[2]);
			int value2 = Convert.ToInt32(array[0]);
			int value3 = Convert.ToInt32(array[1]);
			string text = Convert.ToString(value);
			string text2 = Convert.ToString(value2);
			string text3 = Convert.ToString(value3);
			if (text2.Length == 4)
			{
				return Convert.ToDateTime(date);
			}
			if (text.Length == 1)
			{
				text = "0" + text;
			}
			if (text2.Length == 1)
			{
				text2 = "0" + text2;
			}
			if (text3.Length == 1)
			{
				text3 = "0" + text3;
			}
			return Convert.ToDateTime("20" + text + "-" + text2 + "-" + text3);
		}

		private static void AutoSizeColumns(ISheet sheet)
		{
			if (sheet.PhysicalNumberOfRows > 0)
			{
				IRow row = sheet.GetRow(0);
				int i = 0;
				for (int lastCellNum = row.LastCellNum; i < lastCellNum; i++)
				{
					sheet.AutoSizeColumn(i);
				}
			}
		}

		public static void AddPieChart(ISheet sheet, HSSFWorkbook workbook, string fileurl, int row, int col)
		{
			try
			{
				string text = UpImg(fileurl);
				if (!string.IsNullOrEmpty(text) && File.Exists(text))
				{
					byte[] pictureData = File.ReadAllBytes(text);
					int num = 0;
					num = workbook.AddPicture(pictureData, PictureType.JPEG);
					HSSFPatriarch obj = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
					HSSFClientAnchor anchor = new HSSFClientAnchor(10, 0, 2, 0, col, row, col + 1, row + 1);
					HSSFPicture hSSFPicture = (HSSFPicture)obj.CreatePicture(anchor, num);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static string UpImg(string imgUrl)
		{
			string text = "D:\\NPOI\\DownLoad\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string str = imgUrl.Replace("/", "_").Replace(".", "").Replace(":", "");
			if (File.Exists(text + str))
			{
				return text + str;
			}
			string text2 = string.Empty;
			Path.GetExtension(imgUrl);
			string text3 = ".jpeg";
			text3 = "image/" + text3.Replace(".", "");
			ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo imageCodecInfo = null;
			ImageCodecInfo[] array = imageEncoders;
			foreach (ImageCodecInfo imageCodecInfo2 in array)
			{
				if (imageCodecInfo2.MimeType == text3)
				{
					imageCodecInfo = imageCodecInfo2;
					break;
				}
			}
			if (imageCodecInfo != null)
			{
				try
				{
					WebRequest webRequest = WebRequest.Create(imgUrl);
					webRequest.Timeout = 600000000;
					webRequest.Method = "GET";
					EncoderParameters encoderParameters = new EncoderParameters(1);
					encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
					using (WebResponse webResponse = webRequest.GetResponse())
					{
						MemoryStream memoryStream = null;
						Bitmap bitmap = null;
						using (Stream stream = webResponse.GetResponseStream())
						{
							try
							{
								text2 = text + str;
								memoryStream = new MemoryStream();
								bitmap = new Bitmap(stream);
								bitmap.Save(memoryStream, imageCodecInfo, encoderParameters);
								memoryStream.ToArray();
								bitmap.Save(text2);
								return text2;
							}
							catch (Exception)
							{
								text2 = string.Empty;
								return text2;
							}
							finally
							{
								memoryStream?.Dispose();
								bitmap?.Dispose();
							}
						}
					}
				}
				catch (Exception)
				{
					return text2;
				}
			}
			return "";
		}
	}
}

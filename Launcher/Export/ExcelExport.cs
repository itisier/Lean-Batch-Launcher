using ClosedXML.Excel;
using Panoptes.Model;
using QLNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanBatchLauncher.Launcher.Export
{
    public class ExcelExport
    {
        public class ResultTypeFormatInfo
        {
            public ResultTypeFormatInfo(string key)
            {
                NumberFormatType(key);
            }

            public Type type { get; set; }
            public enum SpecificTypeEnum
            {
                Regular,
                Percent,
                Currency
            }
            public SpecificTypeEnum SpecificType{ get; set; }

            public object FormatNumber(string value)
            {
                if (SpecificType == SpecificTypeEnum.Percent)
                {
                    value = value.Replace("%", "");
                }

                if (SpecificType == SpecificTypeEnum.Currency)
                {
                    char[] arr = value.ToCharArray();

                    arr = Array.FindAll<char>(arr, (c => (char.IsDigit(c)
                                                      || char.IsPunctuation(c)
                                                      || c == '-')));
                    value = new string(arr);
                }

                if (type == typeof(decimal))
                {
                    decimal result = decimal.Parse(value, CultureInfo.InvariantCulture);
                    if (SpecificType == SpecificTypeEnum.Percent)
                        return result / 100.0m;
                    else
                        return result;
                }
                else if(type == typeof(int))
                {
                    int result = int.Parse(value, CultureInfo.InvariantCulture);
                    if (SpecificType == SpecificTypeEnum.Percent)
                        return result / 100;
                    else
                        return result;

                }
                else
                    return value;
            }

            private void NumberFormatType(string key)
            {
                switch (key)
                {
                    case "Total Trades":
                        type = typeof(int);
                        SpecificType = SpecificTypeEnum.Regular;
                        break;
                    case "Average Win":
                    case "Average Loss":
                    case "Compounding Annual Return":
                    case "Drawdown":
                    case "Net Profit":
                    case "Probabilistic Sharpe Ratio":
                    case "Loss Rate":
                    case "Win Rate":
                        type = typeof (decimal);
                        SpecificType= SpecificTypeEnum.Percent;
                        break;
                    case "Expectancy":
                    case "Sharpe Ratio":
                    case "Profit-Loss Ratio":
                    case "Alpha":
                    case "Beta":
                    case "Annual Standard Deviation":
                    case "Annual Variance":
                    case "Information Ratio":
                    case "Tracking Error":
                    case "Treynor Ratio":
                        type = typeof(decimal);
                        SpecificType = SpecificTypeEnum.Regular;
                        break;
                    case "Total Fees":
                    case "Estimated Strategy Capacity":
                        type = typeof(decimal);
                        SpecificType = SpecificTypeEnum.Currency;
                        break;
                    default:
                        type = typeof(string);
                        SpecificType = SpecificTypeEnum.Regular;
                        break;
                }
            }

        }


        public static void Export(IEnumerable<Result> results)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();



            Dictionary<string, ResultTypeFormatInfo> values = new();
            foreach (var stat in results.Select(x => x.Statistics))
            {
                foreach (var statKey in stat.Keys)
                {
                    if (!values.ContainsKey(statKey))
                        values.Add(statKey, new ResultTypeFormatInfo(statKey));
                }
            }


            foreach (var stat in values)
                dataTable.Columns.Add(stat.Key, stat.Value.type);

            foreach (var stat in results.Select(x => x.Statistics))
            {
                var row = dataTable.NewRow();
                foreach (var statKey in stat.Keys)
                {
                    //row[statKey] = FormatNumber(NumberFormatType(statKey), stat[statKey]);
                    row[statKey] = values[statKey].FormatNumber(stat[statKey]);
                }
                dataTable.Rows.Add(row);
            }


            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Results");

            var algoPerformance = results.Select(x => x.Statistics);

            ws.Cell(1, 1).Value = "Results";
            ws.Range(1, 1, 1, 3).Merge().AddToNamed("Titles");
            var tableWithPeople = ws.Cell(2, 1).InsertTable(dataTable);
            for (int i = 0; i < values.Count; i++)
            {
                var value = values.Values.ElementAt(i);
                if (value.type == typeof(int))
                    tableWithPeople.Column(i + 1).Style.NumberFormat.NumberFormatId = 1;
                else if (value.type == typeof(decimal))
                {
                    if (value.SpecificType == ResultTypeFormatInfo.SpecificTypeEnum.Percent)                    
                        tableWithPeople.Column(i + 1).Style.NumberFormat.NumberFormatId = 10;
                    else
                        tableWithPeople.Column(i +1).Style.NumberFormat.NumberFormatId = 2;
                }
            }

            // Prepare the style for the titles
            var titlesStyle = wb.Style;
            titlesStyle.Font.Bold = true;
            titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titlesStyle.Fill.BackgroundColor = XLColor.AliceBlue;

            // Format all titles in one shot
            wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;

            ws.Columns().AdjustToContents();

            wb.SaveAs("c:/temp/InsertingTables2.xlsx");
        }





    }
}

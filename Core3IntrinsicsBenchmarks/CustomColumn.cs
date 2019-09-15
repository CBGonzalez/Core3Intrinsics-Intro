using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Core3IntrinsicsBenchmarks
{
    public class CustomColumn : IColumn
    {
        private readonly Func<string, string> getTag;
        private int currentLine = 0;
        private string firstLine;

        public string Id => nameof(CustomColumn);
        public string ColumnName { get; }

        public CustomColumn(string columnName, Func<string, string> getTag)
        {
            this.getTag = getTag;
            ColumnName = columnName;
        }

        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;

        public UnitType UnitType => UnitType.Size;

        public string Legend => $"Custom '{ColumnName}' tag column";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            /*string desiredHeader;
            int counter = 0;
            foreach (SummaryTable.SummaryTableColumn s in summary.Table.Columns)
            {               
                if(s.Header == "Mean")
                {
                    desiredHeader = s.Header;
                    break;
                }
                counter++;
            }*/

            //return benchmarkCase.Descriptor.Categories.Length.ToString();
            //return benchmarkCase.Parameters.Count.ToString();
            //return benchmarkCase.Parameters[0].Value.ToString();
            string oneMeasur = summary.Reports[currentLine].ExecuteResults[0].Data[0];
            var table = summary.Table;
            if(table != null)
            {
                string[][] full = table.FullContent;
                firstLine = full[0][0];
            }
            
            int pos = oneMeasur.IndexOf("/op");
            double timeFactor = -1.0;
            if(pos >= 0)
            {
                string unit = oneMeasur.Substring(pos - 2, 5);
                switch (unit)
                {
                    case "ns/op":
                        timeFactor = 1.0 / 1_000_000_000.0;
                        break;
                    case "us/op":
                        timeFactor = 1.0 / 1_000_000.0;
                        break;
                    case "ms/op":
                        timeFactor = 1.0 / 1_000.0;
                        break;
                    case " s/op":
                        timeFactor = 1.0;
                        break;
                }
            }
            double floatCount = double.Parse(summary.Reports[currentLine].BenchmarkCase.Parameters[0].Value.ToString());
            //string oneMeasurementNano = summary.Reports[0].AllMeasurements[0].Nanoseconds.ToString();
            string res = summary.Reports[currentLine].ResultStatistics.Mean.ToString();
            double time = double.Parse(res);
            double thrpt = floatCount * 4 / time / timeFactor / 1024 / 1024 / 1024;
            currentLine++;
            return thrpt.ToString("N4");
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            return GetValue(summary, benchmarkCase);
        }

        public bool IsAvailable(Summary summary)
        {
            return true;
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        
    }
}

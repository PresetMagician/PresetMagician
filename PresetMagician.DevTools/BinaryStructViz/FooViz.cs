using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using Color = System.Drawing.Color;

namespace PresetMagician.Utils.BinaryStructViz
{
    public class FooViz
    {
        public Dictionary<int, FooVizData> Memory = new Dictionary<int, FooVizData>();
        public Dictionary<int, List<FooVizData>> Structures = new Dictionary<int, List<FooVizData>>();

        private Random rnd = new Random();
        private const int colOffset = 1;
        private const int rowOffset = 1;
        
        public void SetMemory(byte[] data)
        {
            Memory.Clear();
            for (var i = 0; i < data.Length; i++)
            {
                Memory.Add(i, new FooVizData() {Content = $"{data[i]:X2}", Length = 1});
            }
        }

        public void DumpToFile(string filename)
        {
          
            var sl = new SLDocument();
         
            DumpHeader(sl, colOffset, Memory.Keys.Max()+1);
            DumpRow(sl, colOffset+1, Memory);
            DumpStructures(sl, colOffset+2);
            
            sl.AutoFitColumn(1, 20);
            sl.AutoFitRow(1, Memory.Keys.Max()+1);
            sl.SaveAs(filename);
         
        }

        public void AddStructure(int start, int length, string content)
        {
            if (length == 0)
            {
                return;
            }
            if (!Structures.ContainsKey(start))
            {
                Structures.Add(start, new List<FooVizData>());
            }
            
            Structures[start].Add(new FooVizData() { Length = length, Content = content}); 
        }

        public void DumpHeader(SLDocument slDocument, int column, int maxAddress)
        {
            var odd = false;
            for (var i = 0; i < maxAddress; i++)
            {
                odd = !odd;
                slDocument.SetCellValue(rowOffset+i, column, StringUtils.Int32ToHexString(i));
                
                if (odd)
                {
                    slDocument.SetCellStyle(rowOffset + i, column, GetStyle(slDocument, Color.LightGray));                        
                }
                else
                {
                    slDocument.SetCellStyle(rowOffset + i, column, GetStyle(slDocument, Color.DarkGray));    
                }
            }

        }
        public void DumpRow(SLDocument slDocument, int column, Dictionary<int, FooVizData> data, bool randomBg = false)
        {
            var sortedKeys = data.Keys.ToList();
            sortedKeys.Sort();

            var previousEnd = 0;
            var odd = false;

            
            foreach (var addr in sortedKeys)
            {
                odd = !odd;
                
                if (addr - previousEnd > 0)
                {
                    slDocument.SetCellValue(rowOffset+addr, column, "");
                    slDocument.MergeWorksheetCells(rowOffset+previousEnd, column, rowOffset+addr-1, column);
                    
                }
                var item = data[addr];
                previousEnd = addr + item.Length;


                slDocument.SetCellValue(rowOffset+addr, column, item.Content);
                
                if (randomBg)
                {
                    slDocument.SetCellStyle(rowOffset + addr, column, GetStyle(slDocument));
                }
                else
                {
                    if (odd)
                    {
                        slDocument.SetCellStyle(rowOffset + addr, column, GetStyle(slDocument, Color.LightGray));                        
                    }
                    else
                    {
                        slDocument.SetCellStyle(rowOffset + addr, column, GetStyle(slDocument, Color.DarkGray));    
                    }
                }

                

                if (item.Length > 1)
                {
                    slDocument.MergeWorksheetCells(rowOffset + addr, column, rowOffset + addr + item.Length-1, column);
                }
            }
            
        }

        private SLStyle GetStyle(SLDocument slDocument)
        {
            return GetStyle(slDocument, GetRandomColor());
        }

        private SLStyle  GetStyle(SLDocument slDocument, Color color)
        {
            var foo = slDocument.CreateStyle();
            foo.SetFont("Courier New", 10);
            foo.SetVerticalAlignment(VerticalAlignmentValues.Center);
            foo.SetTopBorder(BorderStyleValues.Hair, Color.Black);
            foo.SetBottomBorder(BorderStyleValues.Hair, Color.Black);
            foo.SetLeftBorder(BorderStyleValues.Hair, Color.Black);
            foo.SetRightBorder(BorderStyleValues.Hair, Color.Black);
            foo.SetPatternFill(PatternValues.Solid, color, color);

            return foo;
        }

        private Color GetRandomColor()
        {
            return Color.FromArgb(rnd.Next(128, 255), rnd.Next(128, 255), rnd.Next(128, 255));

            

            
        }

        public void DumpStructures(SLDocument slDocument, int column)
        {
            var rows = new List<Dictionary<int, FooVizData>>();
            
            rows.Add(new Dictionary<int, FooVizData>());
            var sortedKeys = Structures.Keys.ToList();
            sortedKeys.Sort();

            foreach (var addr in sortedKeys)
            {
                foreach (var structs in Structures[addr])
                {
                    var rowFound = false;
                    foreach (var row in rows)
                    {
                        if (!IsOccupied(row, addr, structs.Length))
                        {
                            row.Add(addr, structs);
                            rowFound = true;
                            break;
                        }
                    }

                    if (!rowFound)
                    {
                        var newRow = new Dictionary<int, FooVizData>();
                        newRow.Add(addr, structs);
                        rows.Add(newRow);
                    }
                }
            }

            foreach (var row in rows)
            {
                var idx = rows.IndexOf(row);
                DumpRow(slDocument, column+idx, row, true);
            }
        }

        public bool IsOccupied(Dictionary<int, FooVizData> row, int start, int length)
        {
            foreach (var startAddr in row.Keys)
            {
                var end = start + length;
                var endAddr = startAddr + row[startAddr].Length;

                if (start >= startAddr && start < endAddr)
                {
                    return true;
                }

                if (end >= startAddr && end < endAddr)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class FooVizData
    {
        public string Content;
        public int Length;
    }

    public class FooVizDataWithStart: FooVizData
    {
        public int Start;
    }
}
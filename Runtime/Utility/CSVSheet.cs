using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shared.Foundation
{
    public class CSVSheet
    {
        List<List<string>> data = new List<List<string>>();

        char fieldDelimiter;
        char textDelimiter;

        public CSVSheet()
        {
            fieldDelimiter = ',';
            textDelimiter = '"';
        }

        public void LoadContent(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                List<string> lineData = new List<string>();

                int nbtextDelimiter = 0;
                string colValue = "";
                for (int j = 0; j < line.Length; j++)
                {
                    if (nbtextDelimiter == 0)
                    {
                        if (line[j] == textDelimiter)
                        {
                            nbtextDelimiter++;
                        }

                        if (line[j] == fieldDelimiter)
                        {
                            if (colValue.Length >= 2 && colValue[0] == textDelimiter && colValue[colValue.Length - 1] == textDelimiter)
                            {
                                colValue = colValue.Substring(1, colValue.Length - 2);
                            }


                            lineData.Add(colValue.Replace(textDelimiter.ToString() + textDelimiter.ToString(), textDelimiter.ToString()));
                            colValue = "";
                        }
                        else
                        {
                            colValue += line[j];
                        }
                    }
                    else
                    {
                        if (line[j] == textDelimiter)
                        {
                            nbtextDelimiter--;
                        }
                        colValue += line[j];
                    }
                }
                if (colValue.Length >= 2 && colValue[0] == textDelimiter && colValue[colValue.Length - 1] == textDelimiter)
                {
                    colValue = colValue.Substring(1, colValue.Length - 2);
                }
                lineData.Add(colValue.Replace(textDelimiter.ToString() + textDelimiter.ToString(), textDelimiter.ToString()));

                data.Add(new List<string>(lineData));
            }
        }

        public void LoadContent(string content)
        {
            LoadCsvSafe(content);
            //string[] lines = System.Text.RegularExpressions.Regex.Split(content, @"\r?\n|\r");
            //LoadContent(lines);
        }

        public void LoadCsvSafe(string rawText)
        {
            //var lines = new List<string[]>();
            //List<List<string>> data
            using (var reader = new StringReader(rawText))
            {
                string? line;
                var currentRow = new List<string>();
                var currentCell = new StringBuilder();
                bool insideQuote = false;

                while ((line = reader.ReadLine()) != null)
                {
                    int i = 0;
                    while (i < line.Length)
                    {
                        char c = line[i];
                        if (c == '"')
                        {
                            insideQuote = !insideQuote;
                        }
                        else if (c == ',' && !insideQuote)
                        {
                            currentRow.Add(currentCell.ToString());
                            currentCell.Clear();
                        }
                        else
                        {
                            currentCell.Append(c);
                        }
                        i++;
                    }

                    if (insideQuote)
                    {
                        currentCell.Append('\n'); // continue next line
                    }
                    else
                    {
                        currentRow.Add(currentCell.ToString());
                        currentCell.Clear();
                        data.Add(currentRow);
                        currentRow = new List<string>();
                    }
                }
            }
            //return lines;
        }

        public void Save(string filepath)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                string line = "";
                List<string> lineData = data[i];
                if (lineData.Count > 0)
                {
                    for (int j = 0; j < lineData.Count; j++)
                    {
                        string colValue = "";
                        if (!string.IsNullOrEmpty(lineData[j]))
                        {
                            colValue = string.Format("{0}{1}{2}", textDelimiter, lineData[j].Replace(textDelimiter.ToString(), textDelimiter.ToString() + textDelimiter.ToString()), textDelimiter);
                        }
                        if (j < lineData.Count - 1)
                        {
                            colValue += fieldDelimiter;
                        }
                        line += colValue;
                    }
                    lines.Add(line);
                }
            }

            File.WriteAllLines(filepath, lines.ToArray());
        }

        public int NumberRow
        {
            get
            {
                return data.Count;
            }
        }

        public void AddRow(params object[] list)
        {
            List<string> lineData = new List<string>();
            for (int i = 0; i < list.Length; i++)
            {
                lineData.Add(list[i].ToString());
            }
            data.Add(lineData);
        }

        public List<string> GetRow(int row)
        {
            if (row >= 0 && row < data.Count)
            {
                return data[row];
            }
            return new List<string>();
        }


        public string GetValue(int row, int col)
        {
            if (row >= 0 && row < data.Count && col >= 0 && col < data[row].Count)
            {
                return data[row][col];
            }
            return "";
        }

        public string GetValue(int row, string colName)
        {
            if (data.Count > 0)
            {
                for (int i = 0; i < data[0].Count; i++)
                {
                    if (data[0][i] == colName)
                    {
                        return GetValue(row, i);
                    }
                }

            }
            return "";
        }

        public int GetValueInt(int row, int col, int defaultVal = 0)
        {
            int no = defaultVal;
            string strVal = GetValue(row, col);
            if (string.IsNullOrEmpty(strVal))
            {
                return defaultVal;
            }
            if (!int.TryParse(strVal, out no))
            {
                return defaultVal;
            }
            return no;
        }

        public int GetValueInt(int row, string colName, int defaultVal = 0)
        {
            int no = defaultVal;
            string strVal = GetValue(row, colName);
            if (string.IsNullOrEmpty(strVal))
            {
                return defaultVal;
            }
            if (!int.TryParse(strVal, out no))
            {
                return defaultVal;
            }
            return no;
        }

        public float GetValueFloat(int row, int col, float defaultVal = 0.0f)
        {
            float no = defaultVal;
            string strVal = GetValue(row, col);
            if (string.IsNullOrEmpty(strVal))
            {
                return defaultVal;
            }
            if (!float.TryParse(strVal, out no))
            {
                return defaultVal;
            }
            return no;
        }

        public float GetValueFloat(int row, string colName, float defaultVal = 0.0f)
        {
            float no = defaultVal;
            string strVal = GetValue(row, colName);
            if (string.IsNullOrEmpty(strVal))
            {
                return defaultVal;
            }
            if (!float.TryParse(strVal, out no))
            {
                return defaultVal;
            }
            return no;
        }
    }

}

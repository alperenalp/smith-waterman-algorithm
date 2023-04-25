using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SmithWatermanAlgorithm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static int match = 0;
        public static int mismatch = 0;
        public static int gap = 0;


        private int CalculateMatchState(int i, int j)
        {
            if (dataGridView1.Rows[0].Cells[i].Value.Equals(dataGridView1.Rows[j].Cells[0].Value))
            {
                return match;
            }
            else
            {
                return mismatch;
            }
        }

        private string findMaxValueIndex(string blackList, int rowIndex, int columnIndex)
        {
            string maxValueIndex = string.Empty;
            int max = -999;
            for (int j = rowIndex; j > 1; j--)
            {
                for (int i = columnIndex; i > 1; i--)
                {
                    int cellValue = Convert.ToInt32(dataGridView1.Rows[j].Cells[i].Value);
                    string currentPath = i + "" + j;
                    bool isBlack = isValueInBlackList(currentPath, blackList);
                    if (!isBlack && cellValue > match && cellValue > max)  // cellValue >= max olursa 4-2-4-2-0 gibi bir path da diger 4'u alabilir!
                    {
                        max = cellValue;
                        maxValueIndex = i + "" + j;
                    }
                }
            }
            return maxValueIndex;
        }

        private bool isValueInBlackList(string currentPath, string blackList)
        {
            bool isBlack = false;
            string[] blackListArray = blackList.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int k = 0; k < blackListArray.Length; k++)
            {
                string blackPath = blackListArray[k].ToString();
                if (currentPath.Equals(blackPath))
                {
                    isBlack = true;
                }
            }
            return isBlack;
        }

        private string findPath(string maxValueIndex)
        {
            string path = string.Empty;
            int columnValue = Convert.ToInt32(maxValueIndex[0].ToString());
            int rowValue = Convert.ToInt32(maxValueIndex[1].ToString());
            while (true)
            {
                int cellValue = Convert.ToInt32(dataGridView1.Rows[rowValue].Cells[columnValue].Value);
                if (cellValue > 0)
                {
                    path += columnValue + "" + rowValue + ",";
                    columnValue--;
                    rowValue--;
                }
                else
                {
                    break;
                }
            }
            return path;
        }

        private void drawPath(string path)
        {
            string[] paths = path.Split(',', StringSplitOptions.RemoveEmptyEntries);
            int lastColumnValue = 0;
            int lastRowValue = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                string index = paths[i].ToString();
                int columnValue = Convert.ToInt32(index[0].ToString());
                int rowValue = Convert.ToInt32(index[1].ToString());
                dataGridView1.Rows[rowValue].Cells[columnValue].Style.BackColor = Color.SkyBlue;
                lastColumnValue = columnValue;
                lastRowValue = rowValue;
            }

            if (lastColumnValue > 1 && lastRowValue > 1)
            {
                lastColumnValue--;
                lastRowValue--;
                dataGridView1.Rows[lastRowValue].Cells[lastColumnValue].Style.BackColor = Color.SkyBlue;
            }
        }

        private string findBestPath(List<string> allPaths)
        {
            string bestPath = string.Empty;
            int bestPathLength = -999;
            for (int i = 0; i < allPaths.Count; i++)
            {
                string[] paths = allPaths[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                // en uzun yolu bul
                if (bestPathLength < paths.Length)
                {
                    bestPathLength = paths.Length;
                }
            }

            for (int i = 0; i < allPaths.Count; i++)
            {
                string[] paths = allPaths[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (bestPathLength == paths.Length)
                {
                    bestPath = allPaths[i].ToString();
                    break;
                }
            }

            return bestPath;
        }

        private string getColumnMatchBases(string bestPath)
        {
            string newColumnBases = string.Empty;
            string[] paths = bestPath.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paths.Length; i++)
            {
                string index = paths[i].ToString();
                int columnValue = Convert.ToInt32(index[0].ToString());
                string columnBase = dataGridView1.Rows[0].Cells[columnValue].Value.ToString();
                newColumnBases = columnBase + newColumnBases;
            }
            return newColumnBases;
        }

        private string getRowMatchBases(string bestPath)
        {
            string newRowBases = string.Empty;
            string[] paths = bestPath.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paths.Length; i++)
            {
                string index = paths[i].ToString();
                int rowValue = Convert.ToInt32(index[1].ToString());
                string rowBase = dataGridView1.Rows[rowValue].Cells[0].Value.ToString();
                newRowBases = rowBase + newRowBases;
            }
            return newRowBases;
        }

        private int calculateScore(string newSeq1, string newSeq2)
        {
            int score = 0;
            for (int i = 0; i < newSeq1.Length; i++)
            {
                if (newSeq1[i].Equals(newSeq2[i]))
                {
                    score += match;
                }
                else if (newSeq1[i] == '-' || newSeq2[i] == '-')
                {
                    score += gap;
                }
                else
                {
                    score += mismatch;
                }
            }
            return score;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            match = Convert.ToInt32(textBox1.Text);
            mismatch = Convert.ToInt32(textBox2.Text);
            gap = Convert.ToInt32(textBox3.Text);

            label5.Text = "Seq1:  ";
            label6.Text = "Seq2:  ";
            label7.Text = "Score:  ";

            try
            {
                // dosya islemleri
                int seq1Length = 0;
                string seq1Text = string.Empty;
                int seq2Length = 0;
                string seq2Text = string.Empty;
                for (int i = 1; i <= 2; i++)
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string path = @$"{currentDirectory}\seq{i}.txt";
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    if (i == 1)
                    {
                        seq1Length = Convert.ToInt32(sr.ReadLine());
                        seq1Text = sr.ReadLine();
                        sr.Close();
                    }
                    else
                    {
                        seq2Length = Convert.ToInt32(sr.ReadLine());
                        seq2Text = sr.ReadLine();
                        sr.Close();
                    }

                }

                // tablo boyutu
                int tableColumnLength = seq1Length + 2;
                int tableRowLength = seq2Length + 2;
                dataGridView1.ColumnCount = tableColumnLength;
                dataGridView1.RowCount = tableRowLength;

                // satirlarin yuksekligini tabloya sigdirma
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    row.Height = Convert.ToInt32((dataGridView1.ClientRectangle.Height - dataGridView1.ColumnHeadersHeight) / (double)(dataGridView1.Rows.Count - 0.3));
                }


                //tabloyu doldurma
                for (int i = 0; i < seq1Length; i++)
                {
                    dataGridView1.Rows[0].Cells[i + 2].Value = seq1Text[i];
                }

                for (int i = 0; i < seq2Length; i++)
                {
                    dataGridView1.Rows[i + 2].Cells[0].Value = seq2Text[i];
                }


                // hesaplama islemlerinin yapilmasi

                // match = +1
                // mismatch = -1;
                // gap = -2
                //
                // T[i,j] = max(   (T[i-1, j-1] + (match || mismatch) )  ||
                //                 (T[i-1, j] + gap) ||
                //                 (T[i, j-1] + gap)

                dataGridView1.Rows[1].Cells[1].Value = 0;
                // ilk satirin hucrelerinin doldurulmasi
                for (int i = 2; i < tableColumnLength; i++)
                {
                    // 0 ile manuel dolduruldu
                    dataGridView1.Rows[1].Cells[i].Value = 0;
                }
                // ilk kolonun hucrelerinin doldurulmasi
                for (int j = 2; j < tableRowLength; j++)
                {
                    // 0 ile manuel dolduruldu
                    dataGridView1.Rows[j].Cells[1].Value = 0;
                }

                // diger hucrelerin doldurulmasi
                for (int j = 1; j < tableRowLength; j++)
                {
                    for (int i = 1; i < tableColumnLength; i++)
                    {
                        if (i != 1 && j != 1) // ilk satýr ve sutun zaten dolduruldu
                        {
                            int[] results = new int[3];
                            // ilk formul
                            int cellValueOfCross = Convert.ToInt32(dataGridView1.Rows[j - 1].Cells[i - 1].Value);
                            int matchState = CalculateMatchState(i, j);
                            results[0] = cellValueOfCross + matchState;
                            // ikinci formul
                            int cellValueOfLeft = Convert.ToInt32(dataGridView1.Rows[j].Cells[i - 1].Value);
                            results[1] = cellValueOfLeft + gap;
                            // ucuncu formul
                            int cellValueOfTop = Convert.ToInt32(dataGridView1.Rows[j - 1].Cells[i].Value);
                            results[2] = cellValueOfTop + gap;

                            // max degeri bulma
                            int max = -999;
                            for (int k = 0; k < results.Length; k++)
                            {
                                if (max < results[k])
                                {
                                    max = results[k];
                                    if (max >= 0)
                                    {
                                        max = results[k];
                                    }
                                    else
                                    {
                                        max = 0;
                                    }
                                }
                            }

                            // sonucu tabloya isleme
                            dataGridView1.Rows[j].Cells[i].Value = max;
                        }
                    }
                }

                // BACK TRACE
                int columnIndex = tableColumnLength - 1;
                int rowIndex = tableRowLength - 1;
                string blackList = string.Empty;
                List<string> allPaths = new List<string>();
                while (true)
                {
                    // max deðeri bul 
                    string maxValueIndex = findMaxValueIndex(blackList, rowIndex, columnIndex);
                    // yol listesi ver
                    if (maxValueIndex == "")
                    {
                        break;
                    }
                    string path = findPath(maxValueIndex);
                    // normal ve kara listeye ekle
                    allPaths.Add(path);
                    blackList += path;
                }

                // yollari ciz
                for (int i = 0; i < allPaths.Count; i++)
                {
                    string path = allPaths[i];
                    drawPath(path);
                }

                // en ideal yolu bul
                string bestPath = findBestPath(allPaths);

                // Bazlarin yeni konumlarini guncelle
                string newSeq1 = getColumnMatchBases(bestPath);
                string newSeq2 = getRowMatchBases(bestPath);
                label5.Text += newSeq1;
                label6.Text += newSeq2;

                // score hesaplanmasý
                int score = calculateScore(newSeq1, newSeq2);
                label7.Text += score.ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hatayla karþýlaþýldý...\n{ex}");
            }

            timer.Stop();
            label8.Text = "Geçen Süre: " + timer.Elapsed.ToString() + "  ------>  " + timer.ElapsedMilliseconds.ToString() + " milisaniye";
        }
    }
}
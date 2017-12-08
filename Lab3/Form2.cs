using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Lab3
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        string filename = "C:/Users/lebox/Desktop/Учеба/ООП/Lab.3/Lab3/res_score.txt";
        string[] result;
        /// <summary>
        /// добавление имени игрока в файл и в result
        /// </summary>
        /// <param name="t"></param>
        public void add_result(string t)
        {
            result[result.Count()-1] +="                " + t;
            Sort_Result(result);
            using (StreamWriter writer = new StreamWriter(filename, true))
                {
                    writer.Write("                " + t + "\r\n");
                }
        }

        /// <summary>
        /// считывание информации из файла
        /// </summary>
        public void read_result()
        {
            result = File.ReadAllLines(filename);
        }
        /// <summary>
        /// отображение в листбоксе
        /// </summary>
        public void RefreshListBox()
        {
            Sort_Result(result);

            foreach (var item in result)
            {
                listBox_Result.Items.Add(item);
            }
        }
        /// <summary>
        /// сортировка по возрастанию
        /// </summary>
        /// <param name="result"></param>
        public void Sort_Result(string[] result)
        {
            for (int i=0;i<result.Count()-1;i++)
            {
                for (int j=i+1;j<result.Count();j++)
                {
                    if (Int32.Parse(result[i].Split(' ')[5])< Int32.Parse(result[j].Split(' ')[5]))
                    {
                        string k = result[i];
                        result[i] = result[j];
                        result[j] = k;
                    }
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox_Name.Enabled = false;
            label1.Enabled = false;
            button_AddResult.Enabled = false;
            read_result();
            add_result(textBox_Name.Text);
            RefreshListBox();
        }
    }
}

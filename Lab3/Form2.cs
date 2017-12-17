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

        string filename = @"C:\Users\Михаил\Google Диск\Учеба\Третий семестр\ООП\Лабораторная работа №3\Lab3\res_score.txt"; //Переписать на свой путь
        string file_image = @"C:\Users\Михаил\Google Диск\Учеба\Третий семестр\ООП\Лабораторная работа №3\Lab3\Images\"; //Переписать на свой путь
        string[] result;
        /// <summary>
        /// добавление имени игрока в файл и в result
        /// </summary>
        /// <param name="t"></param>
        public void add_result(string t)
        {
            result[result.Count()-1] +=t;
            Sort_Result(result);
            using (StreamWriter writer = new StreamWriter(filename, true))
                {
                    writer.Write(t + "\r\n");
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
            listBox_Result.Items.Clear();
            foreach (var item in result)
            {
                ListViewItem c = new ListViewItem(item.Split(' ')[1]);
                c.SubItems.Add(Math.Round(double.Parse(item.Split(' ')[0])).ToString());
                listBox_Result.Items.Add(c);
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
                    if (double.Parse(result[i].Split(' ')[0])< double.Parse(result[j].Split(' ')[0]))
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




        private void button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listBox_Result_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var item in result)
            {
                if (listBox_Result.SelectedItems[0].Text == item.Split(' ')[1])
                {
                    pictureBox.Image = new Bitmap(file_image + item.Split(' ')[0] + ".bmp", true);
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            read_result();
            Sort_Result(result);
            RefreshListBox();
        }
    }
}

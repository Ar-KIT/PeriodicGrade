using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Azil_maGUIc
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            //Accepts only Numeric Digits
            foreach (Control panelControl in this.Controls)
            {
                if (panelControl is Panel panel)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control is System.Windows.Forms.TextBox textbox)
                        {
                            textbox.KeyPress += TextBox_KeyPress;
                            textbox.Leave += TextBox_Leave;
                        }
                    }
                }
            }
        }

        private void clrButton_Click_1(object sender, EventArgs e)
        {
            // iterate all Controls
            foreach (Control panelControl in this.Controls)
            {
                // iterate all Panels
                if (panelControl is Panel panel)
                {
                    // iterate all Controls in Panels
                    foreach (Control control in panel.Controls)
                    {
                        // Check if Control is a TextBox
                        if (control is System.Windows.Forms.TextBox textbox)
                        {
                            // Clear Content
                            textbox.Clear();
                        }
                    }
                }
            }
        }

        private void sbtButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ScoreValidator.CheckItems(this)) return;

                double[,] quizzes = PeriodicGradeCalculator.ScoresAndItems(2, this, "q");
                double[,] laboratoryActs = PeriodicGradeCalculator.ScoresAndItems(3, this, "l");
                double[,] classroomActs = PeriodicGradeCalculator.ScoresAndItems(3, this, "c");
                double[,] exam = PeriodicGradeCalculator.ScoresAndItems(1, this, "e");

                double periodicGrade = PeriodicGradeCalculator.SetPeriodicGrade(quizzes, laboratoryActs, classroomActs, exam);
                if (periodicGrade < 0)
                {
                    throw new Exception();
                }
                double equivalentGrade = PeriodicGradeCalculator.ComputeEquivalenttGrade(periodicGrade);
                string remarks = equivalentGrade <= 3.0 ? "Passed" : "Failed";

                ComputeedGrades.Text = periodicGrade.ToString();
                EquivalentGrade.Text = equivalentGrade.ToString();
                Remarks.Text = remarks;
            }catch(Exception ex){}
        }


        //SCORE VALIDATION
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            ScoreValidator.DigitValidator(e);
        }
        private void TextBox_Leave(object sender, EventArgs e)
        {
            ScoreValidator.TotalItemsValidator(sender);
        }
    }

    static class ScoreValidator
    {
        public static void DigitValidator(KeyPressEventArgs e)
        {
            try
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
                {
                    e.Handled = true;
                    throw new ArgumentException();
                }
            }catch(ArgumentException ex)
            {
                MessageBox.Show("Only Numeric Digits and WhiteSpaces are allowed", "INVALID INPUT");
            }
        }
        public static void TotalItemsValidator(object sender)
        {
            try
            {
                if (sender is System.Windows.Forms.TextBox textBox && textBox.Name.IndexOf("items", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    char textBoxName = char.ToLower(textBox.Name[0]);
                    int maxItems = 0;
                    int minItems = 0;
                    int items;
                    if (int.TryParse(textBox.Text, out items))

                        switch (textBoxName)
                    {
                        case 'q':
                            minItems = 5;
                            maxItems = 50;
                            break;
                        case 'l':
                            minItems = 50;
                            maxItems = 100;
                            break;
                        case 'c':
                            minItems = 30;
                            maxItems = 50;
                            break;
                        case 'e':
                            minItems = maxItems = 100;
                            break;
                        default:
                            return;
                    }
                    if (items < minItems || items > maxItems)
                    {
                        MessageBox.Show($"Invalid value for {textBox.Name}: must be between {minItems} and {maxItems}", "INAVLID RANGE");
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    textBox.Clear();
                }
            }
        }
        public static bool CheckItems(Control parentControl)
        {
            try
            {
                foreach (Control panelControl in parentControl.Controls)
                {
                    if (panelControl is Panel panel)
                    {
                        foreach (Control control in panel.Controls)
                        {
                            if (control is System.Windows.Forms.TextBox textBox && textBox.Name.ToLower().Contains("items"))
                            {
                                if (string.IsNullOrWhiteSpace(textBox.Text))
                                {
                                    throw new ArgumentException($"Textbox '{textBox.Name}' cannot have blank or null value.");
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }
        }
    }
    static class PeriodicGradeCalculator
    {
        public static double[,] ScoresAndItems(int numbers, Control parentControl, string prefix)
        {
            double[,] values = new double[numbers, 2];
            int index1 = 0;
            int index2 = 0;

            try
            {
                foreach (Control panelControl in parentControl.Controls)
                {
                    if (panelControl is Panel panel)
                    {
                        foreach (Control control in panel.Controls)
                        {
                            if (control is System.Windows.Forms.TextBox textBox)
                            {
                                if (textBox.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (textBox.Name.ToLower().Contains("score"))
                                    {
                                        if (double.TryParse(textBox.Text, out double score))
                                        {
                                            values[index1, 0] = score; // Store score at index 0
                                            index1++;
                                            if (index1 > 0 && index2 > 0 && values[index1 - 1, 0] > values[index2 - 1, 1])
                                            {
                                                throw new ArgumentException($"Score cannot exceed items for {textBox.Name}.");
                                            }
                                        }
                                    }
                                    else if (textBox.Name.ToLower().Contains("items"))
                                    {
                                        if (double.TryParse(textBox.Text, out double item))
                                        {
                                            values[index2, 1] = item; // Store item at index 1
                                            index2++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            return values;
        }


        public static double SetPeriodicGrade(double[,] quizzes, double[,] laboratoryActs, double[,] classroomActs, double[,] exam)
        {
            try
            {
                double qWeight = 0;
                for (int i = 0; i < quizzes.GetLength(0); i++)
                {
                    qWeight += quizzes[i, 0] / quizzes[i, 1] * 50 + 50;
                }
                qWeight /= quizzes.GetLength(0);
                double qComponent = Math.Round(0.1 * qWeight, 3);

                double labScoreSum = Enumerable.Range(0, laboratoryActs.GetLength(0)).Sum(i => laboratoryActs[i, 0]);
                double labTotalItemSum = Enumerable.Range(0, laboratoryActs.GetLength(0)).Sum(i => laboratoryActs[i, 1]);
                double lWeight = labScoreSum / labTotalItemSum * 50 + 50;
                double lComponent = lWeight * 0.5;

                double classScoreSum = Enumerable.Range(0, classroomActs.GetLength(0)).Sum(i => classroomActs[i, 0]);
                double classTotalSum = Enumerable.Range(0, classroomActs.GetLength(0)).Sum(i => classroomActs[i, 1]);
                double cWeight = classScoreSum / classTotalSum * 50 + 50;
                double cComponent = cWeight * 0.1;

                double examScoreSum = Enumerable.Range(0, exam.GetLength(0)).Sum(i => exam[i, 0]);
                double examTotalSum = Enumerable.Range(0, exam.GetLength(0)).Sum(i => exam[i, 1]);
                double xWeight = examScoreSum / examTotalSum * 50 + 50;
                double xComponent = xWeight * 0.3;

                return Math.Round(qComponent + lComponent + cComponent + xComponent, 3);
            }
            catch (Exception ex)
            {
                return -1;
            }
            
        }
        public static double ComputeEquivalenttGrade(double percentage)
        {
            double[,] gradeTable = {
                { 97.5, 100, 1.0 },
                { 94.5, 97.49, 1.25 },
                { 91.5, 94.49, 1.5 },
                { 88.5, 91.49, 1.75 },
                { 85.5, 88.49, 2.0 },
                { 82.5, 85.49, 2.25 },
                { 79.5, 82.49, 2.5 },
                { 76.5, 79.49, 2.75 },
                { 74.5, 76.49, 3.0 },
                { 50, 74.49, 5.0 }
            };
            for (int i = 0; i < gradeTable.GetLength(0); i++)
            {
                double lowerBound = gradeTable[i, 0];
                double upperBound = gradeTable[i, 1];
                double equivalent = gradeTable[i, 2];

                if (percentage >= lowerBound && percentage <= upperBound)
                {
                    return equivalent;
                }
            }
            return 5.0;
        }
    }
}

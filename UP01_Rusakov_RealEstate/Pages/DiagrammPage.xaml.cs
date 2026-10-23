using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class DiagrammPage : Page
    {
        private string[] chartNames = { "Гистограмма (столбцы)", "Линейчатая (полосы)", "Круговая", "Кольцевая", "График" };
        private SeriesChartType[] chartTypes = { SeriesChartType.Column, SeriesChartType.Bar, SeriesChartType.Pie,
                                                 SeriesChartType.Doughnut, SeriesChartType.Line };

        public DiagrammPage()
        {
            InitializeComponent();

            ChartProperties.ChartAreas.Add(new ChartArea("Main"));
            Series currentSeries = new Series("Стоимость объектов, руб.");
            currentSeries.IsValueShownAsLabel = true;
            currentSeries.LabelFormat = "N0";
            ChartProperties.Series.Add(currentSeries);

            CmbUser.ItemsSource = GetSortedUsers();
            CmbDiagram.ItemsSource = chartNames;
            CmbUser.SelectedIndex = 0;
            CmbDiagram.SelectedIndex = 0;
            CmbExportScope.SelectedIndex = 0;
        }

        private List<User> GetSortedUsers()
        {
            List<User> users = new List<User>(Entities.GetContext().Users);
            users.Sort((a, b) => a.FIO.CompareTo(b.FIO));
            return users;
        }

        private List<PropertyType> GetSortedTypes()
        {
            List<PropertyType> types = new List<PropertyType>(Entities.GetContext().PropertyTypes);
            types.Sort((a, b) => a.Name.CompareTo(b.Name));
            return types;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Entities.Refresh();
                UpdateChart(null, null);
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            User currentUser = CmbUser.SelectedItem as User;
            if (currentUser == null || CmbDiagram.SelectedIndex < 0)
                return;

            Series currentSeries = ChartProperties.Series[0];
            currentSeries.ChartType = chartTypes[CmbDiagram.SelectedIndex];
            currentSeries.Points.Clear();

            foreach (PropertyType type in GetSortedTypes())
            {
                decimal sum = 0;
                foreach (Property p in currentUser.Properties)
                {
                    if (p.TypeID == type.ID)
                        sum += p.Price;
                }
                currentSeries.Points.AddXY(type.Name, sum);
            }
            ApplyChartTheme();
        }

        public void ApplyChartTheme()
        {
            Color back, fore;
            if (Helper.IsDarkTheme)
            {
                back = Color.FromArgb(0x1E, 0x21, 0x29);
                fore = Color.White;
            }
            else
            {
                back = Color.FromArgb(0xF3, 0xF3, 0xF3);
                fore = Color.FromArgb(0x19, 0x3D, 0x57);
            }

            ChartProperties.BackColor = back;
            ChartArea area = ChartProperties.ChartAreas[0];
            area.BackColor = back;
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.ForeColor = fore;
            area.AxisY.LabelStyle.ForeColor = fore;
            area.AxisX.LineColor = fore;
            area.AxisY.LineColor = fore;
            ChartProperties.Legends[0].BackColor = back;
            ChartProperties.Legends[0].ForeColor = fore;
            ChartProperties.Series[0].LabelForeColor = fore;
        }

        private List<Property> GetPropertiesForExport()
        {
            User selectedUser = CmbUser.SelectedItem as User;
            List<Property> properties = new List<Property>();
            foreach (Property p in Entities.GetContext().Properties)
            {
                if (CmbExportScope.SelectedIndex == 1 && p.UserID != selectedUser.ID)
                    continue;
                properties.Add(p);
            }
            properties.Sort((a, b) => a.User.FIO.CompareTo(b.User.FIO));
            return properties;
        }

        private string GetFileName(string extension)
        {
            string name = "все";
            if (CmbExportScope.SelectedIndex == 1)
                name = (CmbUser.SelectedItem as User).Login;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                "Объекты_" + name + extension);
        }

        private void ButtonWord_Click(object sender, RoutedEventArgs e)
        {
            ExportToWord(false);
        }

        private void ButtonPdf_Click(object sender, RoutedEventArgs e)
        {
            ExportToWord(true);
        }

        private void ExportToWord(bool toPdf)
        {
            List<Property> properties = GetPropertiesForExport();
            decimal totalPrice = 0;
            foreach (Property p in properties)
                totalPrice += p.Price;

            try
            {
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                Word.Paragraph titleParagraph = document.Paragraphs.Add();
                titleParagraph.Range.Text = "Список объектов недвижимости";
                titleParagraph.Range.Font.Size = 16;
                titleParagraph.Range.Font.Bold = 1;
                titleParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titleParagraph.Range.InsertParagraphAfter();

                Word.Paragraph dateParagraph = document.Paragraphs.Add();
                dateParagraph.Range.Text = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy");
                dateParagraph.Range.Font.Size = 12;
                dateParagraph.Range.Font.Bold = 0;
                dateParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                dateParagraph.Range.InsertParagraphAfter();

                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Table table = document.Tables.Add(tableParagraph.Range, properties.Count + 1, 5);
                table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                table.Cell(1, 1).Range.Text = "Агент";
                table.Cell(1, 2).Range.Text = "Тип";
                table.Cell(1, 3).Range.Text = "Адрес";
                table.Cell(1, 4).Range.Text = "Площадь, м²";
                table.Cell(1, 5).Range.Text = "Цена, руб.";
                table.Rows[1].Range.Font.Bold = 1;

                for (int i = 0; i < properties.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = properties[i].User.FIO;
                    table.Cell(i + 2, 2).Range.Text = properties[i].PropertyType.Name;
                    table.Cell(i + 2, 3).Range.Text = properties[i].Address;
                    table.Cell(i + 2, 4).Range.Text = properties[i].Area.ToString();
                    table.Cell(i + 2, 5).Range.Text = properties[i].Price.ToString("N0");
                }

                Word.Paragraph totalParagraph = document.Paragraphs.Add();
                totalParagraph.Range.Text = "Всего объектов: " + properties.Count +
                                            ". Общая стоимость: " + totalPrice.ToString("N0") + " руб.";
                totalParagraph.Range.Font.Bold = 0;

                document.Content.Font.Name = "Times New Roman";

                if (toPdf)
                {
                    string pdfPath = GetFileName(".pdf");
                    document.SaveAs2(pdfPath, Word.WdSaveFormat.wdFormatPDF);
                    ((Word._Application)application).Quit(false);
                    Helper.ShowInfo("Отчёт сохранён:\n" + pdfPath);
                    System.Diagnostics.Process.Start(pdfPath);
                }
                else
                {
                    string docxPath = GetFileName(".docx");
                    document.SaveAs2(docxPath);
                    application.Visible = true;
                    Helper.ShowInfo("Отчёт сохранён:\n" + docxPath);
                }
            }
            catch (Exception ex)
            {
                Helper.ShowError("Не удалось сформировать отчёт. Проверьте, что установлен Microsoft Word " +
                                 "и файл отчёта не открыт.\n" + ex.Message);
            }
        }

        private void ButtonExcel_Click(object sender, RoutedEventArgs e)
        {
            List<Property> properties = GetPropertiesForExport();

            try
            {
                var application = new Excel.Application();
                application.SheetsInNewWorkbook = 1;
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = "Объекты";

                worksheet.Cells[1, 1] = "Агент";
                worksheet.Cells[1, 2] = "Тип";
                worksheet.Cells[1, 3] = "Адрес";
                worksheet.Cells[1, 4] = "Площадь, м²";
                worksheet.Cells[1, 5] = "Цена, руб.";
                worksheet.Range["A1:E1"].Font.Bold = true;

                int row = 2;
                foreach (Property p in properties)
                {
                    worksheet.Cells[row, 1] = p.User.FIO;
                    worksheet.Cells[row, 2] = p.PropertyType.Name;
                    worksheet.Cells[row, 3] = p.Address;
                    worksheet.Cells[row, 4] = p.Area;
                    worksheet.Cells[row, 5] = p.Price;
                    row++;
                }

                worksheet.Cells[row, 4] = "Итого:";
                ((Excel.Range)worksheet.Cells[row, 5]).Formula = "=SUM(E2:E" + (row - 1) + ")";
                worksheet.Range["A" + row + ":E" + row].Font.Bold = true;
                worksheet.Columns.AutoFit();

                string xlsxPath = GetFileName(".xlsx");
                workbook.SaveAs(xlsxPath);
                application.Visible = true;
                Helper.ShowInfo("Данные сохранены:\n" + xlsxPath);
            }
            catch (Exception ex)
            {
                Helper.ShowError("Не удалось выгрузить данные в Excel. Проверьте, что установлен Microsoft Excel " +
                                 "и файл не открыт.\n" + ex.Message);
            }
        }
    }
}

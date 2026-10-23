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
        private string[] chartNames = { "Гистограмма (столбцы)", "Линейчатая (полосы)", "Круговая", "Кольцевая",
                                        "График", "Сглаженный график", "С областями", "Сглаженные области",
                                        "Точечная", "Воронка", "Пирамида", "Лепестковая" };
        private SeriesChartType[] chartTypes = { SeriesChartType.Column, SeriesChartType.Bar, SeriesChartType.Pie,
                                                 SeriesChartType.Doughnut, SeriesChartType.Line, SeriesChartType.Spline,
                                                 SeriesChartType.Area, SeriesChartType.SplineArea, SeriesChartType.Point,
                                                 SeriesChartType.Funnel, SeriesChartType.Pyramid, SeriesChartType.Radar };

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

        private List<User> GetUsersForExport()
        {
            if (CmbExportScope.SelectedIndex == 1)
                return new List<User> { CmbUser.SelectedItem as User };
            return GetSortedUsers();
        }

        private string GetFileName(string extension)
        {
            string name = "все";
            if (CmbExportScope.SelectedIndex == 1)
                name = (CmbUser.SelectedItem as User).Login;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                "Отчёт_" + name + extension);
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
            List<User> allUsers = GetUsersForExport();
            List<PropertyType> allTypes = GetSortedTypes();

            try
            {
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (User user in allUsers)
                {
                    List<Property> userProperties = new List<Property>(user.Properties);

                    decimal totalSum = 0;
                    Property maxProperty = null;
                    Property minProperty = null;
                    foreach (Property p in userProperties)
                    {
                        totalSum += p.Price;
                        if (maxProperty == null || p.Price > maxProperty.Price)
                            maxProperty = p;
                        if (minProperty == null || p.Price < minProperty.Price)
                            minProperty = p;
                    }

                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style(Word.WdBuiltinStyle.wdStyleHeading1);
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allTypes.Count + 2, 3);
                    paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle =
                        Word.WdLineStyle.wdLineStyleSingle;

                    paymentsTable.Cell(1, 1).Range.Text = "Тип недвижимости";
                    paymentsTable.Cell(1, 2).Range.Text = "Количество объектов";
                    paymentsTable.Cell(1, 3).Range.Text = "Суммарная стоимость";
                    paymentsTable.Rows[1].Range.Bold = 1;

                    for (int i = 0; i < allTypes.Count; i++)
                    {
                        int count = 0;
                        decimal sum = 0;
                        foreach (Property p in userProperties)
                        {
                            if (p.TypeID == allTypes[i].ID)
                            {
                                count++;
                                sum += p.Price;
                            }
                        }
                        paymentsTable.Cell(i + 2, 1).Range.Text = allTypes[i].Name;
                        paymentsTable.Cell(i + 2, 2).Range.Text = count.ToString();
                        paymentsTable.Cell(i + 2, 3).Range.Text = sum.ToString("N2") + " руб.";
                    }

                    int lastRow = allTypes.Count + 2;
                    paymentsTable.Cell(lastRow, 1).Range.Text = "Итого";
                    paymentsTable.Cell(lastRow, 2).Range.Text = userProperties.Count.ToString();
                    paymentsTable.Cell(lastRow, 3).Range.Text = totalSum.ToString("N2") + " руб.";
                    paymentsTable.Rows[lastRow].Range.Bold = 1;

                    if (userProperties.Count > 0)
                    {
                        Word.Paragraph maxParagraph = document.Paragraphs.Add();
                        Word.Range maxRange = maxParagraph.Range;
                        maxRange.Text = "Самый дорогой объект — " + maxProperty.Address + ": " +
                                        maxProperty.Price.ToString("N2") + " руб.";
                        maxRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxRange.InsertParagraphAfter();

                        Word.Paragraph minParagraph = document.Paragraphs.Add();
                        Word.Range minRange = minParagraph.Range;
                        minRange.Text = "Самый дешёвый объект — " + minProperty.Address + ": " +
                                        minProperty.Price.ToString("N2") + " руб.";
                        minRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minRange.InsertParagraphAfter();
                    }

                    if (user != allUsers[allUsers.Count - 1])
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = "AuroraApartments · отчёт от " + DateTime.Now.ToString("dd.MM.yyyy");
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

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
            List<User> allUsers = GetUsersForExport();

            try
            {
                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count;
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                for (int i = 0; i < allUsers.Count; i++)
                {
                    User user = allUsers[i];
                    List<Property> userProperties = new List<Property>(user.Properties);
                    userProperties.Sort((a, b) => a.Date.CompareTo(b.Date));

                    Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[i + 1];
                    worksheet.Name = user.Login;

                    worksheet.Cells[1, 1] = user.FIO;
                    worksheet.Cells[2, 1] = "Дата";
                    worksheet.Cells[2, 2] = "Тип";
                    worksheet.Cells[2, 3] = "Адрес";
                    worksheet.Cells[2, 4] = "Площадь, м²";
                    worksheet.Cells[2, 5] = "Комнат";
                    worksheet.Cells[2, 6] = "Цена, руб.";
                    worksheet.Range["A1:F2"].Font.Bold = true;

                    int row = 3;
                    foreach (Property p in userProperties)
                    {
                        worksheet.Cells[row, 1] = p.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[row, 2] = p.PropertyType.Name;
                        worksheet.Cells[row, 3] = p.Address;
                        worksheet.Cells[row, 4] = p.Area;
                        worksheet.Cells[row, 5] = p.Rooms;
                        worksheet.Cells[row, 6] = p.Price;
                        row++;
                    }

                    if (userProperties.Count > 0)
                    {
                        worksheet.Cells[row, 3] = "Итого:";
                        ((Excel.Range)worksheet.Cells[row, 4]).Formula = "=SUM(D3:D" + (row - 1) + ")";
                        ((Excel.Range)worksheet.Cells[row, 6]).Formula = "=SUM(F3:F" + (row - 1) + ")";
                        worksheet.Cells[row + 1, 3] = "Средняя цена:";
                        ((Excel.Range)worksheet.Cells[row + 1, 6]).Formula = "=AVERAGE(F3:F" + (row - 1) + ")";
                        worksheet.Range["A" + row + ":F" + (row + 1)].Font.Bold = true;
                    }

                    worksheet.Columns.AutoFit();
                }

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

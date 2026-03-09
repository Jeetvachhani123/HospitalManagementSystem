using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.DTOs.Appointment;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HospitalMS.BL.Services;

public class ExportService : IExportService
{
    private readonly IAppointmentService _appointmentService;
    public ExportService(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // export to csv
    public Task<byte[]> ExportAppointmentsToCSVAsync(List<AppointmentExportDto> appointments)
    {
        static string Q(string? s) =>
            "\"" + (s ?? "").Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") + "\"";
        const int C = 9;
        string E = string.Join(",", Enumerable.Repeat("", C));
        string T(string v) => v + string.Join("", Enumerable.Repeat(",", C - 1));
        var sb = new StringBuilder();
        sb.AppendLine(T(Q("HOSPITAL MANAGEMENT SYSTEM")));
        sb.AppendLine(T(Q("Appointment Export Report")));
        sb.AppendLine(T(Q($"Generated: {DateTime.Now:dd MMM yyyy   HH:mm:ss}")));
        sb.AppendLine(T(Q($"Total Records: {appointments.Count}")));
        sb.AppendLine(E);
        sb.AppendLine(string.Join(",", new[]
        {
            Q("No."), Q("Patient Name"), Q("Doctor Name"),
            Q("Appointment Date"), Q("Start Time"), Q("End Time"),
            Q("Status"), Q("Reason"), Q("Diagnosis")
        }));
        int no = 1;
        foreach (var apt in appointments)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Q(no++.ToString()),
                Q(apt.PatientName),
                Q(apt.DoctorName),
                Q(apt.AppointmentDate.ToString("dd MMM yyyy")),
                Q(apt.StartTime.ToString(@"hh\:mm")),
                Q(apt.EndTime.ToString(@"hh\:mm")),
                Q(apt.Status),
                Q(apt.Reason),
                Q(apt.Diagnosis ?? "")
            }));
        }
        sb.AppendLine(E);
        sb.AppendLine(T(Q($"End of Report — {appointments.Count} record(s) exported")));
        var bytes = Encoding.UTF8.GetPreamble()
                             .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                             .ToArray();
        return Task.FromResult(bytes);
    }

    // export to excel
    public Task<byte[]> ExportAppointmentsToExcelAsync(List<AppointmentExportDto> appointments)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Appointments");
        ws.Range("A1:I1").Merge();
        ws.Cell("A1").Value = "Hospital Management System — Appointment Export";
        ws.Cell("A1").Style
            .Font.SetBold(true)
            .Font.SetFontSize(16)
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#0D6EFD"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws.Row(1).Height = 30;
        ws.Range("A2:I2").Merge();
        ws.Cell("A2").Value = $"Generated: {DateTime.Now:dd MMM yyyy   HH:mm}   |   Total Records: {appointments.Count}";
        ws.Cell("A2").Style
            .Font.SetItalic(true)
            .Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.FromHtml("#555555"))
            .Fill.SetBackgroundColor(XLColor.FromHtml("#EEF3FF"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws.Row(2).Height = 22;
        ws.Row(3).Height = 8;
        string[] headers = { "#", "Patient Name", "Doctor Name", "Date", "Start Time", "End Time", "Status", "Reason", "Diagnosis" };
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(4, c + 1);
            cell.Value = headers[c];
            cell.Style
                .Font.SetBold(true)
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#0D6EFD"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorderColor(XLColor.FromHtml("#AAAAAA"));
        }
        ws.Row(4).Height = 22;
        int row = 5;
        foreach (var apt in appointments)
        {
            bool isEven = (row % 2 == 0);
            var rowBg = XLColor.FromHtml(isEven ? "#F0F5FF" : "#FFFFFF");
            void StyleDataCell(IXLCell cell, XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Left)
            {
                cell.Style
                    .Fill.SetBackgroundColor(rowBg)
                    .Font.SetFontSize(10)
                    .Alignment.SetHorizontal(align)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Alignment.SetWrapText(false)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetOutsideBorderColor(XLColor.FromHtml("#D5D5D5"));
            }
            ws.Cell(row, 1).Value = row - 4;
            StyleDataCell(ws.Cell(row, 1), XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 2).Value = apt.PatientName;
            StyleDataCell(ws.Cell(row, 2));
            ws.Cell(row, 3).Value = apt.DoctorName;
            StyleDataCell(ws.Cell(row, 3));
            ws.Cell(row, 4).Value = apt.AppointmentDate.ToString("dd MMM yyyy");
            StyleDataCell(ws.Cell(row, 4), XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 5).Value = apt.StartTime.ToString(@"hh\:mm");
            StyleDataCell(ws.Cell(row, 5), XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 6).Value = apt.EndTime.ToString(@"hh\:mm");
            StyleDataCell(ws.Cell(row, 6), XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 7).Value = apt.Status;
            StyleDataCell(ws.Cell(row, 7), XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 7).Style.Font.SetBold(true);
            ws.Cell(row, 7).Style.Font.SetFontColor(apt.Status switch
            {
                "Completed" => XLColor.FromHtml("#198754"),
                "Cancelled" => XLColor.FromHtml("#DC3545"),
                "Scheduled" => XLColor.FromHtml("#0D6EFD"),
                "NoShow" => XLColor.FromHtml("#FFC107"),
                _ => XLColor.FromHtml("#333333")
            });
            ws.Cell(row, 8).Value = apt.Reason ?? "";
            StyleDataCell(ws.Cell(row, 8));
            ws.Cell(row, 9).Value = apt.Diagnosis ?? "";
            StyleDataCell(ws.Cell(row, 9));
            ws.Row(row).Height = 18;
            row++;
        }
        ws.Range(row, 1, row, 9).Merge();
        ws.Cell(row, 1).Value = $"Total: {appointments.Count} appointment(s)   |   Exported on {DateTime.Now:dd MMM yyyy HH:mm}";
        ws.Cell(row, 1).Style
            .Font.SetItalic(true)
            .Font.SetFontSize(9)
            .Font.SetFontColor(XLColor.FromHtml("#888888"))
            .Fill.SetBackgroundColor(XLColor.FromHtml("#F8F9FA"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromHtml("#CCCCCC"));
        ws.Row(row).Height = 18;
        ws.Column(1).Width = 6;
        ws.Column(2).Width = 24;
        ws.Column(3).Width = 22;
        ws.Column(4).Width = 14;
        ws.Column(5).Width = 12;
        ws.Column(6).Width = 12;
        ws.Column(7).Width = 14;
        ws.Column(8).Width = 28;
        ws.Column(9).Width = 28;
        ws.SheetView.FreezeRows(4);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
    // export to pdf
    public Task<byte[]> ExportAppointmentsToPdfAsync(List<AppointmentExportDto> appointments)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));
                page.Header().Column(col =>
                {
                    col.Item().Background("#0D6EFD").Padding(10).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Hospital Management System")
                                .Bold().FontSize(18).FontColor(Colors.White);
                            c.Item().Text("Appointment Export Report")
                                .FontSize(11).FontColor("#CCE5FF");
                        });
                        r.ConstantItem(200).AlignRight().AlignMiddle().Column(c =>
                        {
                            c.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy  HH:mm}")
                                .FontSize(9).FontColor("#CCE5FF").Italic();
                            c.Item().Text($"Total Records: {appointments.Count}")
                                .FontSize(9).FontColor(Colors.White).Bold();
                        });
                    });
                    col.Item().Height(6);
                });
                page.Content().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(28);
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(2f);
                        cols.ConstantColumn(72);
                        cols.ConstantColumn(52);
                        cols.ConstantColumn(52);
                        cols.ConstantColumn(68);
                        cols.RelativeColumn(2.5f);
                    });
                    table.Header(header =>
                    {
                        void HeaderCell(string text) =>
                            header.Cell().Background("#0D6EFD").Border(1).BorderColor("#0A56CA")
                                .Padding(5).AlignCenter()
                                .Text(text).Bold().FontColor(Colors.White).FontSize(9);
                        HeaderCell("#");
                        HeaderCell("Patient Name");
                        HeaderCell("Doctor Name");
                        HeaderCell("Date");
                        HeaderCell("Start Time");
                        HeaderCell("End Time");
                        HeaderCell("Status");
                        HeaderCell("Reason");
                    });
                    int no = 1;
                    foreach (var apt in appointments)
                    {
                        bool isEven = no % 2 == 0;
                        string rowBg = isEven ? "#F0F5FF" : Colors.White;
                        void DataCell(string text, bool center = false, string? color = null) =>
                            table.Cell().Background(rowBg).Border(1).BorderColor("#E0E8F5")
                                .Padding(4).Element(e => center ? e.AlignCenter() : e.AlignLeft())
                                .Text(text).FontSize(9)
                                .FontColor(color ?? Colors.Black);
                        void DataCellBold(string text, bool center = false, string? color = null) =>
                            table.Cell().Background(rowBg).Border(1).BorderColor("#E0E8F5")
                                .Padding(4).Element(e => center ? e.AlignCenter() : e.AlignLeft())
                                .Text(text).FontSize(9).Bold()
                                .FontColor(color ?? Colors.Black);
                        string statusColor = apt.Status switch
                        {
                            "Completed" => "#198754",
                            "Cancelled" => "#DC3545",
                            "Scheduled" => "#0D6EFD",
                            "NoShow" => "#CC8800",
                            _ => "#333333"
                        };
                        DataCell(no++.ToString(), center: true);
                        DataCell(apt.PatientName);
                        DataCell(apt.DoctorName);
                        DataCell(apt.AppointmentDate.ToString("dd MMM yyyy"), center: true);
                        DataCell(apt.StartTime.ToString(@"hh\:mm"), center: true);
                        DataCell(apt.EndTime.ToString(@"hh\:mm"), center: true);
                        DataCellBold(apt.Status, center: true, color: statusColor);
                        DataCell(apt.Reason ?? "");
                    }
                });
                page.Footer().BorderTop(1).BorderColor("#DDDDDD").PaddingTop(5).Row(r =>
                {
                    r.RelativeItem().Text("Hospital Management System — Confidential")
                        .FontSize(8).FontColor("#AAAAAA").Italic();
                    r.ConstantItem(100).AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor("#AAAAAA");
                        x.CurrentPageNumber().FontSize(8).FontColor("#AAAAAA");
                        x.Span(" of ").FontSize(8).FontColor("#AAAAAA");
                        x.TotalPages().FontSize(8).FontColor("#AAAAAA");
                    });
                });
            });
        });
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}
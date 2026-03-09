using ClosedXML.Excel;
using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Enums;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;
    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    //  GET api/appointments  (role-based filtering)
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        IEnumerable<AppointmentResponseDto> appointments;
        if (role == "Admin" || role == "Doctor")
        {
            appointments = await _appointmentService.GetAllAsync(cancellationToken);
        }
        else
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(GetCurrentUserId(), cancellationToken);
            appointments = patient == null
                ? Enumerable.Empty<AppointmentResponseDto>()
                : await _appointmentService.GetByPatientIdAsync(patient.Id, cancellationToken);
        }
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(appointments));
    }

    //  GET api/appointments/{id}  (ownership validation) 
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound(ApiResponse<AppointmentResponseDto>.ErrorResponse("Appointment not found."));
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin" && role != "Doctor")
        {
            var userId = GetCurrentUserId();
            var hasAccess = await _appointmentService.UserHasAccessToAppointmentAsync(userId, id, cancellationToken);
            if (!hasAccess)
                return Forbid();
        }
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(appointment));
    }

    //  GET api/appointments/patient/{patientId} 
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByPatientId(int patientId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetByPatientIdAsync(patientId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(appointments));
    }

    //  GET api/appointments/doctor/{doctorId} 
    [HttpGet("doctor/{doctorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByDoctorId(int doctorId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(appointments));
    }

    // GET api/appointments/date-range
    [HttpGet("date-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        if (startDate > endDate)
            return BadRequest(ApiResponse<IEnumerable<AppointmentResponseDto>>.ErrorResponse("Start date must be before or equal to end date."));
        var appointments = await _appointmentService.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(appointments));
    }

    // GET api/appointments/status/{status} 
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetByStatus(AppointmentStatus status, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetByStatusAsync(status, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(appointments));
    }

    //  POST api/appointments  (create)
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> Create([FromBody] AppointmentCreateDto dto, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} created for patient {PatientId}", appointment.Id, dto.PatientId);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id },
            ApiResponse<AppointmentResponseDto>.SuccessResponse(appointment, "Appointment created successfully."));
    }

    // PUT api/appointments/{id}  (update) 
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> Update(int id, [FromBody] AppointmentUpdateDto dto, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.UpdateAsync(id, dto, cancellationToken);
        if (appointment == null)
            return NotFound(ApiResponse<AppointmentResponseDto>.ErrorResponse("Appointment not found."));
        _logger.LogInformation("Appointment {AppointmentId} updated", id);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(appointment, "Appointment updated successfully."));
    }

    //  PATCH api/appointments/{id}/status  (Admin / Doctor only) 
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> UpdateStatus(int id, [FromBody] AppointmentStatusDto dto, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var appointment = await _appointmentService.UpdateStatusAsync(id, dto, currentUserId, cancellationToken);
        if (appointment == null)
            return NotFound(ApiResponse<AppointmentResponseDto>.ErrorResponse("Appointment not found."));
        _logger.LogInformation("Appointment {AppointmentId} status updated to {Status} by user {UserId}", id, dto.Status, currentUserId);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(appointment, "Appointment status updated successfully."));
    }

    // DELETE api/appointments/{id}  (cancel)
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Cancel(int id, [FromQuery] string? reason = null, CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.CancelAsync(id, reason, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} cancelled", id);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Appointment cancelled successfully."));
    }

    //  POST api/appointments/check-conflict 
    [HttpPost("check-conflict")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckConflict([FromBody] AppointmentConflictRequest req, CancellationToken cancellationToken)
    {
        if (req.StartTime >= req.EndTime)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Start time must be before end time."));
        var hasConflict = await _appointmentService.HasConflictAsync(
            req.DoctorId, req.AppointmentDate, req.StartTime, req.EndTime, req.ExcludeAppointmentId, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(hasConflict,
            hasConflict ? "A scheduling conflict exists for the requested slot." : "The time slot is available."));
    }

    //  GET api/appointments/export/csv 
    [HttpGet("export/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCsv([FromQuery] int? patientId = null, [FromQuery] int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var data = await GetFilteredAsync(patientId, doctorId, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Id,PatientId,PatientName,DoctorId,DoctorName,Specialization,Date,StartTime,EndTime,Status,ApprovalStatus,Reason,Diagnosis,Prescription,Notes,CreatedAt");
        foreach (var a in data)
        {
            sb.AppendLine(string.Join(",",
                a.Id, a.PatientId, Csv(a.PatientName),
                a.DoctorId, Csv(a.DoctorName), Csv(a.DoctorSpecialization),
                a.AppointmentDate.ToString("yyyy-MM-dd"),
                a.StartTime.ToString(@"hh\:mm"), a.EndTime.ToString(@"hh\:mm"),
                a.Status, a.ApprovalStatus,
                Csv(a.Reason), Csv(a.Diagnosis), Csv(a.Prescription), Csv(a.Notes),
                a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        _logger.LogInformation("Exported {Count} appointments to CSV", data.Count());
        return File(bytes, "text/csv", $"appointments_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    //  GET api/appointments/export/excel 
    [HttpGet("export/excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel([FromQuery] int? patientId = null, [FromQuery] int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var data = (await GetFilteredAsync(patientId, doctorId, cancellationToken)).ToList();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Appointments");
        var headers = new[] { "ID", "Patient", "Doctor", "Specialization", "Date", "Start", "End", "Status", "Approval", "Reason", "Diagnosis", "Prescription", "Notes", "Created At" };
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        for (int r = 0; r < data.Count; r++)
        {
            var a = data[r];
            int row = r + 2;
            ws.Cell(row, 1).Value = a.Id;
            ws.Cell(row, 2).Value = a.PatientName;
            ws.Cell(row, 3).Value = a.DoctorName;
            ws.Cell(row, 4).Value = a.DoctorSpecialization;
            ws.Cell(row, 5).Value = a.AppointmentDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = a.StartTime.ToString(@"hh\:mm");
            ws.Cell(row, 7).Value = a.EndTime.ToString(@"hh\:mm");
            ws.Cell(row, 8).Value = a.Status;
            ws.Cell(row, 9).Value = a.ApprovalStatus;
            ws.Cell(row, 10).Value = a.Reason ?? "";
            ws.Cell(row, 11).Value = a.Diagnosis ?? "";
            ws.Cell(row, 12).Value = a.Prescription ?? "";
            ws.Cell(row, 13).Value = a.Notes ?? "";
            ws.Cell(row, 14).Value = a.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            if (r % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        _logger.LogInformation("Exported {Count} appointments to Excel", data.Count);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"appointments_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    //  GET api/appointments/export/pdf 
    [HttpGet("export/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPdf([FromQuery] int? patientId = null, [FromQuery] int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var data = (await GetFilteredAsync(patientId, doctorId, cancellationToken)).ToList();
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new PdfDocument(writer);
        var doc = new Document(pdf);
        doc.Add(new Paragraph("Hospital Appointment Report")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(4));
        doc.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC  |  Total: {data.Count}")
            .SetFont(normalFont)
            .SetFontSize(9)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(12));
        float[] colWidths = { 1.5f, 4f, 4f, 3f, 3f, 2.5f, 2.5f, 3f, 3.5f };
        var table = new Table(UnitValue.CreatePercentArray(colWidths)).UseAllAvailableWidth();
        var headerBg = new DeviceRgb(30, 64, 175);
        foreach (var h in new[] { "ID", "Patient", "Doctor", "Date", "Time", "Status", "Approval", "Reason", "Diagnosis" })
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(8).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(headerBg)
                .SetPadding(4));
        }
        var shadeColor = new DeviceRgb(241, 245, 249);
        bool shade = false;
        foreach (var a in data)
        {
            var rowBg = shade ? shadeColor : null;
            foreach (var val in new[]
            {
                a.Id.ToString(), a.PatientName, a.DoctorName,
                a.AppointmentDate.ToString("yyyy-MM-dd"),
                $"{a.StartTime:hh\\:mm}–{a.EndTime:hh\\:mm}",
                a.Status, a.ApprovalStatus,
                a.Reason ?? "—", a.Diagnosis ?? "—"
            })
            {
                var cell = new Cell().Add(new Paragraph(val).SetFontSize(8)).SetPadding(3);
                if (rowBg != null) cell.SetBackgroundColor(rowBg);
                table.AddCell(cell);
            }
            shade = !shade;
        }
        doc.Add(table);
        doc.Close();
        _logger.LogInformation("Exported {Count} appointments to PDF", data.Count);
        return File(ms.ToArray(), "application/pdf", $"appointments_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf");
    }

    //  GET api/appointments/upcoming 
    [HttpGet("upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetUpcoming([FromQuery] int? patientId = null, [FromQuery] int? doctorId = null, CancellationToken cancellationToken = default)
    {
        if (User.FindFirstValue(ClaimTypes.Role) == "Patient" && patientId == null)
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(GetCurrentUserId(), cancellationToken);
            patientId = patient?.Id;
        }
        var today = DateTime.UtcNow.Date;
        var (items, _) = await _appointmentService.SearchAsync(
            searchTerm: null,
            doctorId: doctorId,
            patientId: patientId,
            fromDate: today,
            toDate: today.AddYears(2),
            status: null,
            page: 1,
            pageSize: 1000,
            cancellationToken: cancellationToken);
        var upcoming = items.Where(a =>
            a.StatusEnum != AppointmentStatus.Cancelled &&
            a.StatusEnum != AppointmentStatus.Completed &&
            a.StatusEnum != AppointmentStatus.NoShow);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(upcoming));
    }

    //  GET api/appointments/history 
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetHistory([FromQuery] int? patientId = null, [FromQuery] int? doctorId = null, CancellationToken cancellationToken = default)
    {
        if (User.FindFirstValue(ClaimTypes.Role) == "Patient" && patientId == null)
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(GetCurrentUserId(), cancellationToken);
            patientId = patient?.Id;
        }
        var today = DateTime.UtcNow.Date;
        var (items, _) = await _appointmentService.SearchAsync(
            searchTerm: null,
            doctorId: doctorId,
            patientId: patientId,
            fromDate: null,
            toDate: today.AddDays(-1),
            status: null,
            page: 1,
            pageSize: 1000,
            cancellationToken: cancellationToken);
        var history = items.Where(a =>
            a.AppointmentDate < today ||
            a.StatusEnum == AppointmentStatus.Completed ||
            a.StatusEnum == AppointmentStatus.Cancelled ||
            a.StatusEnum == AppointmentStatus.NoShow)
            .OrderByDescending(a => a.AppointmentDate);
        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(history));
    }

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private async Task<IEnumerable<AppointmentResponseDto>> GetFilteredAsync(int? patientId, int? doctorId, CancellationToken ct)
    {
        var (items, _) = await _appointmentService.SearchAsync(
            searchTerm: null,
            doctorId: doctorId,
            patientId: patientId,
            fromDate: null,
            toDate: null,
            status: null,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken: ct);
        return items;
    }

    private static string Csv(string? v)
    {
        if (string.IsNullOrEmpty(v)) return string.Empty;
        v = v.Replace("\"", "\"\"");
        return v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{v}\"" : v;
    }
}

// Inline request model (same file, no extra file needed) 
public sealed record AppointmentConflictRequest(
    int DoctorId,
    DateTime AppointmentDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int? ExcludeAppointmentId = null
);
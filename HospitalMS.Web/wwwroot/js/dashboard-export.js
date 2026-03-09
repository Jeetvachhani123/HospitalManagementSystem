// Dashboard Export Functions - Professional Edition
function exportReport(type) {
    if (typeof showToast === 'function') {
        showToast('Export Started', 'Generating the report, please wait...', 'info');
    }
    fetch('/Admin/GetFullReportData')
        .then(function (res) { return res.json(); })
        .then(function (fullData) {
            var dateStr = new Date().toISOString().split('T')[0];
            if (type === 'excel') {
                exportToExcel(fullData, dateStr);
            } else if (type === 'pdf') {
                exportToPdf(fullData, dateStr);
            } else if (type === 'print') {
                printReport(fullData, dateStr);
            }
            if (typeof showToast === 'function') {
                showToast('Success', 'Report generated successfully!', 'success');
            }
        })
        .catch(function (err) {
            console.error('Export error:', err);
            if (typeof showToast === 'function') {
                showToast('Error', 'Failed to generate report.', 'danger');
            }
        });
}

function exportToExcel(fullData, dateStr) {
    var ExcelJS = window.ExcelJS;
    var workbook = new ExcelJS.Workbook();
    workbook.creator = 'Hospital Management System';
    workbook.created = new Date();
    var stats = fullData.stats || {};
    var doctors = fullData.doctors || [];
    var patients = fullData.patients || [];
    var appts = fullData.todayAppointments || [];
    var docStats = stats.doctorStats || stats.DoctorStats || [];
    var BORDER_THIN = { style: 'thin', color: { argb: 'FFD0D7E2' } };
    var CELL_BORDER = { top: BORDER_THIN, left: BORDER_THIN, bottom: BORDER_THIN, right: BORDER_THIN };
    function applyTitleBanner(sheet, text, colCount, bgARGB) {
        var row = sheet.addRow([text]);
        sheet.mergeCells(row.number, 1, row.number, colCount);
        row.height = 36;
        var cell = row.getCell(1);
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgARGB } };
        cell.font = { bold: true, size: 16, color: { argb: 'FFFFFFFF' }, name: 'Calibri' };
        cell.alignment = { horizontal: 'center', vertical: 'middle' };
        return row;
    }

    function applySubtitle(sheet, text, colCount, bgARGB, textARGB) {
        var row = sheet.addRow([text]);
        sheet.mergeCells(row.number, 1, row.number, colCount);
        row.height = 20;
        var cell = row.getCell(1);
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgARGB } };
        cell.font = { italic: true, size: 10, color: { argb: textARGB || 'FF555555' }, name: 'Calibri' };
        cell.alignment = { horizontal: 'center', vertical: 'middle' };
        return row;
    }

    function applySectionHeader(sheet, text, colCount, bgARGB) {
        var row = sheet.addRow([text]);
        sheet.mergeCells(row.number, 1, row.number, colCount);
        row.height = 24;
        var cell = row.getCell(1);
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgARGB } };
        cell.font = { bold: true, size: 12, color: { argb: 'FFFFFFFF' }, name: 'Calibri' };
        cell.alignment = { horizontal: 'left', vertical: 'middle', indent: 1 };
        return row;
    }

    function applyColumnHeaders(sheet, headers, bgARGB) {
        var row = sheet.addRow(headers);
        row.height = 22;
        row.eachCell(function (cell) {
            cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgARGB } };
            cell.font = { bold: true, size: 10, color: { argb: 'FFFFFFFF' }, name: 'Calibri' };
            cell.alignment = { horizontal: 'center', vertical: 'middle', wrapText: false };
            cell.border = CELL_BORDER;
        });
        return row;
    }

    function applyDataRow(sheet, values, isEven, centerCols) {
        var row = sheet.addRow(values);
        row.height = 18;
        var bgColor = isEven ? 'FFF0F5FF' : 'FFFFFFFF';
        row.eachCell({ includeEmpty: true }, function (cell, colN) {
            cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgColor } };
            cell.font = { size: 10, name: 'Calibri' };
            cell.border = CELL_BORDER;
            cell.alignment = { vertical: 'middle', horizontal: (centerCols && centerCols.indexOf(colN) > -1) ? 'center' : 'left' };
        });
        return row;
    }

    function addEmptyRow(sheet, colCount, bgARGB) {
        var row = sheet.addRow(Array(colCount).fill(''));
        sheet.mergeCells(row.number, 1, row.number, colCount);
        if (bgARGB) {
            row.getCell(1).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bgARGB } };
        }
        row.height = 8;
        return row;
    }

    var ws1 = workbook.addWorksheet('Summary', { views: [{ showGridLines: false }] });
    ws1.columns = [
        { key: 'a', width: 32 },
        { key: 'b', width: 22 },
        { key: 'c', width: 22 },
        { key: 'd', width: 22 }
    ];
    applyTitleBanner(ws1, 'Hospital Management System', 4, 'FF0D6EFD');
    applySubtitle(ws1, 'Full Administration Report   |   Generated: ' + (fullData.generatedAt || dateStr), 4, 'FF1E7AF5', 'FFD6ECFF');
    addEmptyRow(ws1, 4, 'FFF8FAFF');
    applySectionHeader(ws1, '  \uD83D\uDCC8  Appointment Statistics', 4, 'FF0D6EFD');
    applyColumnHeaders(ws1, ['Metric', 'Value', '', ''], 'FF3D8EFD');
    var statsDataArr = [
        ['Total Appointments', stats.totalAppointments || stats.TotalAppointments || 0],
        ['Completed', stats.completedCount || stats.CompletedCount || 0],
        ['Scheduled', stats.scheduledCount || stats.ScheduledCount || 0],
        ['Cancelled', stats.cancelledCount || stats.CancelledCount || 0],
        ['No Show', stats.noShowCount || stats.NoShowCount || 0],
        ['Completion Rate', (stats.completionRate || stats.CompletionRate || 0) + '%'],
        ['No Show Rate', (stats.noShowRate || stats.NoShowRate || 0) + '%'],
        ['Cancellation Rate', (stats.cancellationRate || stats.CancellationRate || 0) + '%']
    ];
    statsDataArr.forEach(function (item, i) {
        var r = ws1.addRow([item[0], item[1], '', '']);
        ws1.mergeCells(r.number, 2, r.number, 4);
        r.height = 18;
        var isEven = i % 2 === 0;
        var bg = isEven ? 'FFF0F5FF' : 'FFFFFFFF';
        r.eachCell({ includeEmpty: true }, function (cell) {
            cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: bg } };
            cell.font = { size: 10, name: 'Calibri' };
            cell.border = CELL_BORDER;
            cell.alignment = { vertical: 'middle' };
        });
        r.getCell(2).font = { bold: true, size: 11, name: 'Calibri' };
        r.getCell(2).alignment = { horizontal: 'center', vertical: 'middle' };
    });
    addEmptyRow(ws1, 4, 'FFF8FAFF');
    applySectionHeader(ws1, '  \uD83D\uDC68\u200D\u2695\uFE0F  Doctor Appointment Summary', 4, 'FF198754');
    if (docStats.length > 0) {
        applyColumnHeaders(ws1, ['Doctor Name', 'Specialization', 'Total Appts', 'Completed'], 'FF28A968');
        docStats.forEach(function (d, i) {
            applyDataRow(ws1, [
                d.doctorName || d.DoctorName || '',
                d.specialization || d.Specialization || '',
                d.totalAppointments || d.TotalAppointments || 0,
                d.completedAppointments || d.CompletedAppointments || 0
            ], i % 2 === 0, [3, 4]);
        });
    } else {
        var noDsRow = ws1.addRow(['No doctor statistics available.', '', '', '']);
        ws1.mergeCells(noDsRow.number, 1, noDsRow.number, 4);
        noDsRow.getCell(1).font = { italic: true, color: { argb: 'FF888888' }, size: 10 };
        noDsRow.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
    }
    var ws2 = workbook.addWorksheet('Doctors', { views: [{ showGridLines: false }] });
    ws2.columns = [
        { key: 'name', width: 28 },
        { key: 'email', width: 32 },
        { key: 'spec', width: 24 },
        { key: 'exp', width: 16 },
        { key: 'phone', width: 18 }
    ];
    applyTitleBanner(ws2, '\uD83D\uDC68\u200D\u2695\uFE0F  All Registered Doctors', 5, 'FF198754');
    applySubtitle(ws2, 'Total: ' + doctors.length + ' doctor(s)   |   Generated: ' + (fullData.generatedAt || dateStr), 5, 'FFEAFAF1', 'FF198754');
    addEmptyRow(ws2, 5, 'FFF8FAFF');
    applyColumnHeaders(ws2, ['Full Name', 'Email Address', 'Specialization', 'Experience', 'Phone'], 'FF1E9E63');
    if (doctors.length > 0) {
        doctors.forEach(function (d, i) {
            applyDataRow(ws2, [
                d.name || '', d.email || '', d.specialization || '',
                (d.experience || 0) + ' yrs', d.phone || 'N/A'
            ], i % 2 === 0, [4]);
        });
    } else {
        var noDocRow = ws2.addRow(['No doctors registered.', '', '', '', '']);
        ws2.mergeCells(noDocRow.number, 1, noDocRow.number, 5);
        noDocRow.getCell(1).font = { italic: true, color: { argb: 'FF888888' }, size: 10 };
        noDocRow.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
    }
    var ws3 = workbook.addWorksheet('Patients', { views: [{ showGridLines: false }] });
    ws3.columns = [
        { key: 'name', width: 28 },
        { key: 'email', width: 32 },
        { key: 'phone', width: 18 },
        { key: 'blood', width: 14 },
        { key: 'dob', width: 18 }
    ];
    applyTitleBanner(ws3, '\uD83D\uDC65  All Registered Patients', 5, 'FF495057');
    applySubtitle(ws3, 'Total: ' + patients.length + ' patient(s)   |   Generated: ' + (fullData.generatedAt || dateStr), 5, 'FFF8F9FA', 'FF495057');
    addEmptyRow(ws3, 5, 'FFF8FAFF');
    applyColumnHeaders(ws3, ['Full Name', 'Email Address', 'Phone', 'Blood Group', 'Date of Birth'], 'FF5A6268');
    if (patients.length > 0) {
        patients.forEach(function (p, i) {
            applyDataRow(ws3, [
                p.name || '', p.email || '', p.phone || '',
                p.bloodGroup || 'N/A', p.dateOfBirth || ''
            ], i % 2 === 0, [4]);
        });
    } else {
        var noPatRow = ws3.addRow(['No patients registered.', '', '', '', '']);
        ws3.mergeCells(noPatRow.number, 1, noPatRow.number, 5);
        noPatRow.getCell(1).font = { italic: true, color: { argb: 'FF888888' }, size: 10 };
        noPatRow.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
    }
    var ws4 = workbook.addWorksheet("Today's Appointments", { views: [{ showGridLines: false }] });
    ws4.columns = [
        { key: 'patient', width: 26 },
        { key: 'doctor', width: 26 },
        { key: 'date', width: 18 },
        { key: 'time', width: 12 },
        { key: 'status', width: 16 }
    ];
    var todayStr = new Date().toLocaleDateString('en-IN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    applyTitleBanner(ws4, '\uD83D\uDCC5  Today\'s Appointments', 5, 'FFE67E22');
    applySubtitle(ws4, todayStr + '   |   Total: ' + appts.length + ' appointment(s)', 5, 'FFFFF8E1', 'FFB05A00');
    addEmptyRow(ws4, 5, 'FFF8FAFF');
    applyColumnHeaders(ws4, ['Patient Name', 'Doctor Name', 'Date', 'Time', 'Status'], 'FFEE8C37');
    if (appts.length > 0) {
        appts.forEach(function (a, i) {
            applyDataRow(ws4, [
                a.patient || '', a.doctor || '', a.date || '', a.time || '', a.status || ''
            ], i % 2 === 0, [3, 4, 5]);
        });
    } else {
        var noApptRow = ws4.addRow(['No appointments scheduled for today.', '', '', '', '']);
        ws4.mergeCells(noApptRow.number, 1, noApptRow.number, 5);
        noApptRow.getCell(1).font = { italic: true, color: { argb: 'FF888888' }, size: 10 };
        noApptRow.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
    }
    workbook.xlsx.writeBuffer().then(function (buffer) {
        var blob = new Blob([buffer], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = 'Hospital_Full_Report_' + dateStr + '.xlsx';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    });
}

function exportToPdf(fullData, dateStr) {
    var jsPDF = window.jspdf.jsPDF;
    var doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    var stats = fullData.stats || {};
    var pageWidth = doc.internal.pageSize.getWidth();
    var y = 15;
    doc.setFillColor(13, 110, 253);
    doc.rect(0, 0, pageWidth, 28, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(20);
    doc.setFont('helvetica', 'bold');
    doc.text('Hospital Management System', pageWidth / 2, 12, { align: 'center' });
    doc.setFontSize(11);
    doc.setFont('helvetica', 'normal');
    doc.text('Full Administration Report', pageWidth / 2, 20, { align: 'center' });
    doc.setTextColor(0, 0, 0);
    y = 35;
    doc.setFontSize(9);
    doc.setTextColor(100, 100, 100);
    doc.text('Generated: ' + (fullData.generatedAt || dateStr), pageWidth - 14, y, { align: 'right' });
    doc.setTextColor(0, 0, 0);
    y += 8;
    doc.setFontSize(13);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(13, 110, 253);
    doc.text('1. Appointment Statistics', 14, y);
    doc.setTextColor(0, 0, 0);
    y += 2;
    doc.autoTable({
        startY: y,
        head: [['Metric', 'Value']],
        body: [
            ['Total Appointments', String(stats.totalAppointments || stats.TotalAppointments || 0)],
            ['Completed', String(stats.completedCount || stats.CompletedCount || 0)],
            ['Scheduled', String(stats.scheduledCount || stats.ScheduledCount || 0)],
            ['Cancelled', String(stats.cancelledCount || stats.CancelledCount || 0)],
            ['No Show', String(stats.noShowCount || stats.NoShowCount || 0)],
            ['Completion Rate', (stats.completionRate || stats.CompletionRate || 0) + '%'],
            ['No Show Rate', (stats.noShowRate || stats.NoShowRate || 0) + '%'],
            ['Cancellation Rate', (stats.cancellationRate || stats.CancellationRate || 0) + '%']
        ],
        styles: { fontSize: 10, cellPadding: 3 },
        headStyles: { fillColor: [13, 110, 253], textColor: 255, fontStyle: 'bold' },
        alternateRowStyles: { fillColor: [240, 248, 255] },
        theme: 'grid',
        margin: { left: 14, right: 14 }
    });
    y = doc.lastAutoTable.finalY + 10;
    var docStats = (stats.doctorStats || stats.DoctorStats || []);
    if (docStats.length > 0) {
        if (y > 220) { doc.addPage(); y = 20; }
        doc.setFontSize(13); doc.setFont('helvetica', 'bold'); doc.setTextColor(13, 110, 253);
        doc.text('2. Doctor Appointment Statistics', 14, y); doc.setTextColor(0, 0, 0); y += 2;
        doc.autoTable({
            startY: y,
            head: [['Doctor Name', 'Specialization', 'Total', 'Completed']],
            body: docStats.map(function (d) { return [String(d.doctorName || d.DoctorName || ''), String(d.specialization || d.Specialization || ''), String(d.totalAppointments || d.TotalAppointments || 0), String(d.completedAppointments || d.CompletedAppointments || 0)]; }),
            styles: { fontSize: 10, cellPadding: 3 }, headStyles: { fillColor: [13, 110, 253], textColor: 255, fontStyle: 'bold' },
            alternateRowStyles: { fillColor: [240, 248, 255] }, theme: 'grid', margin: { left: 14, right: 14 }
        });
        y = doc.lastAutoTable.finalY + 10;
    }
    var doctors = fullData.doctors || [];
    if (doctors.length > 0) {
        if (y > 200) { doc.addPage(); y = 20; }
        doc.setFontSize(13); doc.setFont('helvetica', 'bold'); doc.setTextColor(25, 135, 84);
        doc.text('3. All Registered Doctors (' + doctors.length + ')', 14, y); doc.setTextColor(0, 0, 0); y += 2;
        doc.autoTable({
            startY: y, head: [['Name', 'Email', 'Specialization', 'Experience', 'Phone']],
            body: doctors.map(function (d) { return [String(d.name || ''), String(d.email || ''), String(d.specialization || ''), String(d.experience || 0) + ' yrs', String(d.phone || 'N/A')]; }),
            styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [25, 135, 84], textColor: 255, fontStyle: 'bold' },
            alternateRowStyles: { fillColor: [240, 255, 247] }, theme: 'grid', margin: { left: 14, right: 14 }
        });
        y = doc.lastAutoTable.finalY + 10;
    }
    var patients = fullData.patients || [];
    if (patients.length > 0) {
        if (y > 200) { doc.addPage(); y = 20; }
        doc.setFontSize(13); doc.setFont('helvetica', 'bold'); doc.setTextColor(108, 117, 125);
        doc.text('4. All Registered Patients (' + patients.length + ')', 14, y); doc.setTextColor(0, 0, 0); y += 2;
        doc.autoTable({
            startY: y, head: [['Name', 'Email', 'Phone', 'Blood Group', 'Date of Birth']],
            body: patients.map(function (p) { return [String(p.name || ''), String(p.email || ''), String(p.phone || ''), String(p.bloodGroup || 'N/A'), String(p.dateOfBirth || '')]; }),
            styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [108, 117, 125], textColor: 255, fontStyle: 'bold' },
            alternateRowStyles: { fillColor: [248, 249, 250] }, theme: 'grid', margin: { left: 14, right: 14 }
        });
        y = doc.lastAutoTable.finalY + 10;
    }
    var appts = fullData.todayAppointments || [];
    if (y > 200) { doc.addPage(); y = 20; }
    doc.setFontSize(13); doc.setFont('helvetica', 'bold'); doc.setTextColor(230, 126, 34);
    doc.text("5. Today's Appointments (" + appts.length + ')', 14, y); doc.setTextColor(0, 0, 0); y += 2;
    if (appts.length === 0) {
        doc.setFontSize(10); doc.setFont('helvetica', 'italic'); doc.setTextColor(120, 120, 120);
        doc.text('No appointments scheduled for today.', 14, y + 6);
    } else {
        doc.autoTable({
            startY: y, head: [['Patient', 'Doctor', 'Date', 'Time', 'Status']],
            body: appts.map(function (a) { return [String(a.patient || ''), String(a.doctor || ''), String(a.date || ''), String(a.time || ''), String(a.status || '')]; }),
            styles: { fontSize: 9, cellPadding: 3 }, headStyles: { fillColor: [230, 126, 34], textColor: 255, fontStyle: 'bold' },
            alternateRowStyles: { fillColor: [255, 251, 235] }, theme: 'grid', margin: { left: 14, right: 14 }
        });
    }
    var pageCount = doc.internal.getNumberOfPages();
    for (var i = 1; i <= pageCount; i++) {
        doc.setPage(i);
        doc.setFontSize(8); doc.setTextColor(150, 150, 150);
        doc.text('Hospital Management System - Confidential', 14, doc.internal.pageSize.getHeight() - 8);
        doc.text('Page ' + i + ' of ' + pageCount, pageWidth - 14, doc.internal.pageSize.getHeight() - 8, { align: 'right' });
    }
    doc.save('Hospital_Full_Report_' + dateStr + '.pdf');
}

function printReport(fullData, dateStr) {
    var stats = fullData.stats || {};
    var doctors = fullData.doctors || [];
    var patients = fullData.patients || [];
    var appts = fullData.todayAppointments || [];
    var docStats = stats.doctorStats || stats.DoctorStats || [];
    var printWindow = window.open('', '_blank');
    function buildTable(headers, rows, headerColor, emptyMsg) {
        if (!rows || rows.length === 0) {
            return '<p class="empty">' + (emptyMsg || 'No data available.') + '</p>';
        }
        var t = '<table><thead><tr style="background:' + headerColor + ';">';
        headers.forEach(function (h) { t += '<th>' + h + '</th>'; });
        t += '</tr></thead><tbody>';
        rows.forEach(function (row, ri) {
            t += '<tr style="background:' + (ri % 2 === 0 ? '#f5f9ff' : '#fff') + ';">';
            row.forEach(function (cell) { t += '<td>' + cell + '</td>'; });
            t += '</tr>';
        });
        t += '</tbody></table>';
        return t;
    }

    var html = '<!DOCTYPE html><html><head><title>Hospital Full Report - ' + dateStr + '</title><style>';
    html += '@page{margin:15mm}body{font-family:Calibri,Arial,sans-serif;padding:0;color:#333;font-size:11pt}';
    html += '.banner{background:#0d6efd;color:#fff;padding:16px 20px;margin-bottom:4px}';
    html += '.banner h1{margin:0;font-size:20pt}.banner p{margin:4px 0 0;font-size:10pt;opacity:.9}';
    html += '.meta{background:#eef3ff;padding:6px 20px;font-size:9pt;color:#555;border-bottom:2px solid #0d6efd;margin-bottom:20px}';
    html += '.section{margin-bottom:28px}.section-title{padding:8px 12px;color:#fff;font-size:12pt;font-weight:bold;margin-bottom:0;border-radius:4px 4px 0 0}';
    html += 'table{width:100%;border-collapse:collapse;font-size:10pt;margin-bottom:0}';
    html += 'th{padding:7px 10px;text-align:left;color:#fff;font-weight:bold}';
    html += 'td{border:1px solid #e0e8f0;padding:6px 10px}';
    html += '.empty{color:#888;font-style:italic;padding:8px 12px;background:#fafafa;border:1px solid #eee}';
    html += '@media print{.section{page-break-inside:avoid}}';
    html += '</style></head><body>';
    html += '<div class="banner"><h1>Hospital Management System</h1><p>Full Administration Report</p></div>';
    html += '<div class="meta">Generated: ' + (fullData.generatedAt || dateStr) + '</div>';
    html += '<div class="section"><div class="section-title" style="background:#0d6efd">1. Appointment Statistics</div>';
    html += buildTable(['Metric', 'Value'], [
        ['Total Appointments', stats.totalAppointments || stats.TotalAppointments || 0],
        ['Completed', stats.completedCount || stats.CompletedCount || 0],
        ['Scheduled', stats.scheduledCount || stats.ScheduledCount || 0],
        ['Cancelled', stats.cancelledCount || stats.CancelledCount || 0],
        ['No Show', stats.noShowCount || stats.NoShowCount || 0],
        ['Completion Rate', (stats.completionRate || stats.CompletionRate || 0) + '%']
    ], '#0d6efd') + '</div>';
    if (docStats.length > 0) {
        html += '<div class="section"><div class="section-title" style="background:#0d6efd">2. Doctor Appointment Summary</div>';
        html += buildTable(['Doctor Name', 'Specialization', 'Total Appointments', 'Completed'],
            docStats.map(function (d) { return [d.doctorName || d.DoctorName || '', d.specialization || d.Specialization || '', d.totalAppointments || d.TotalAppointments || 0, d.completedAppointments || d.CompletedAppointments || 0]; }),
            '#0d6efd') + '</div>';
    }
    html += '<div class="section"><div class="section-title" style="background:#198754">3. All Registered Doctors (' + doctors.length + ')</div>';
    html += buildTable(['Name', 'Email', 'Specialization', 'Experience', 'Phone'],
        doctors.map(function (d) { return [d.name, d.email, d.specialization, d.experience + ' yrs', d.phone || 'N/A']; }),
        '#198754', 'No doctors registered.') + '</div>';
    html += '<div class="section"><div class="section-title" style="background:#495057">4. All Registered Patients (' + patients.length + ')</div>';
    html += buildTable(['Name', 'Email', 'Phone', 'Blood Group', 'Date of Birth'],
        patients.map(function (p) { return [p.name, p.email, p.phone, p.bloodGroup, p.dateOfBirth]; }),
        '#495057', 'No patients registered.') + '</div>';
    html += "<div class=\"section\"><div class=\"section-title\" style=\"background:#e67e22\">5. Today's Appointments (" + appts.length + ')</div>';
    html += buildTable(['Patient', 'Doctor', 'Date', 'Time', 'Status'],
        appts.map(function (a) { return [a.patient, a.doctor, a.date, a.time, a.status]; }),
        '#e67e22', 'No appointments scheduled for today.') + '</div>';
    html += '</body></html>';
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(function () { printWindow.print(); printWindow.close(); }, 500);
}
USE HRManagementDB;
GO

-- Xem số lượng dữ liệu trong các bảng chính của phần mềm.
SELECT N'HR_Employees' AS TableName, COUNT(*) AS TotalRows FROM dbo.HR_Employees
UNION ALL SELECT N'HR_Departments', COUNT(*) FROM dbo.HR_Departments
UNION ALL SELECT N'HR_JobPositions', COUNT(*) FROM dbo.HR_JobPositions
UNION ALL SELECT N'HR_Applicants', COUNT(*) FROM dbo.HR_Applicants
UNION ALL SELECT N'HR_Attendances', COUNT(*) FROM dbo.HR_Attendances
UNION ALL SELECT N'HR_LeaveRequests', COUNT(*) FROM dbo.HR_LeaveRequests
UNION ALL SELECT N'HR_Appraisals', COUNT(*) FROM dbo.HR_Appraisals
UNION ALL SELECT N'HR_Payslips', COUNT(*) FROM dbo.HR_Payslips
UNION ALL SELECT N'HR_Users', COUNT(*) FROM dbo.HR_Users
UNION ALL SELECT N'HR_AuditLogs', COUNT(*) FROM dbo.HR_AuditLogs;
GO

-- Xem quân số và trưởng phòng từng phòng ban.
SELECT
    d.DepartmentID,
    d.Name AS DepartmentName,
    COUNT(e.EmployeeID) AS EmployeeCount,
    ISNULL(m.FullName, N'Chưa phân công') AS ManagerName
FROM dbo.HR_Departments d
LEFT JOIN dbo.HR_Employees e ON e.DepartmentID = d.DepartmentID
LEFT JOIN dbo.HR_Employees m ON d.ManagerID = m.EmployeeID
GROUP BY d.DepartmentID, d.Name, m.FullName
ORDER BY d.DepartmentID;
GO

-- Xem dữ liệu nhân viên đúng của phần mềm.
SELECT TOP 100
    e.EmployeeID,
    e.EmployeeCode,
    e.FullName,
    d.Name AS DepartmentName,
    p.Name AS PositionName,
    e.JoinDate,
    e.IsActive
FROM dbo.HR_Employees e
LEFT JOIN dbo.HR_Departments d ON e.DepartmentID = d.DepartmentID
LEFT JOIN dbo.HR_JobPositions p ON e.PositionID = p.PositionID
ORDER BY e.EmployeeID;
GO

-- Không dùng bảng dbo.Employees nếu có, vì phần mềm WPF này dùng dbo.HR_Employees.

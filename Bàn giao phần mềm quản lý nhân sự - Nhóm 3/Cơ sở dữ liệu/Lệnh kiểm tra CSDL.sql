USE HRManagementDB;
GO

SELECT name AS TenBang
FROM sys.tables
WHERE name LIKE 'HR_%'
ORDER BY name;
GO

SELECT COUNT(*) AS SoPhongBan FROM dbo.HR_Departments;
SELECT COUNT(*) AS SoNhanVien FROM dbo.HR_Employees;
SELECT COUNT(*) AS SoTaiKhoan FROM dbo.HR_Users;
SELECT COUNT(*) AS SoNhatKy FROM dbo.HR_AuditLogs;
GO

SELECT
    d.DepartmentID,
    d.Name AS TenPhongBan,
    COUNT(e.EmployeeID) AS QuanSo,
    ISNULL(m.FullName, N'Chưa phân công') AS TruongPhong
FROM dbo.HR_Departments d
LEFT JOIN dbo.HR_Employees e ON e.DepartmentID = d.DepartmentID
LEFT JOIN dbo.HR_Employees m ON d.ManagerID = m.EmployeeID
GROUP BY d.DepartmentID, d.Name, m.FullName
ORDER BY d.DepartmentID;
GO

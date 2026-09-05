using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Department Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Department Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;
}






using System;
using System.Collections.Generic;

namespace BusinessManagement.Domain.Entities;

public class WorkflowDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetEntityName { get; set; } = string.Empty; // e.g. "Booking", "JournalEntry"

    public virtual ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowState : BaseEntity
{
    public Guid WorkflowDefinitionId { get; set; }
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public string StateName { get; set; } = string.Empty; // e.g. "Draft", "PendingApproval", "Approved"
    public bool IsInitial { get; set; } = false;
    public bool IsFinal { get; set; } = false;
}

public class WorkflowTransition : BaseEntity
{
    public Guid WorkflowDefinitionId { get; set; }
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public Guid FromStateId { get; set; }
    public virtual WorkflowState FromState { get; set; } = null!;

    public Guid ToStateId { get; set; }
    public virtual WorkflowState ToState { get; set; } = null!;

    public string TransitionName { get; set; } = string.Empty; // e.g. "SubmitForApproval", "Approve"
    public string RequiredPermissionCode { get; set; } = string.Empty;
}

public class ApprovalRequest : BaseEntity
{
    public Guid TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty; // e.g. "Booking"
    public Guid EntityId { get; set; }

    public Guid WorkflowDefinitionId { get; set; }
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public Guid CurrentStateId { get; set; }
    public virtual WorkflowState CurrentState { get; set; } = null!;

    public Guid RequesterUserId { get; set; }
    public virtual User RequesterUser { get; set; } = null!;
}

public class ApprovalStep : BaseEntity
{
    public Guid ApprovalRequestId { get; set; }
    public virtual ApprovalRequest ApprovalRequest { get; set; } = null!;

    public Guid RequiredRoleId { get; set; }
    public virtual Role RequiredRole { get; set; } = null!;

    public int SequenceOrder { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
}

public class ApprovalHistory : BaseEntity
{
    public Guid ApprovalRequestId { get; set; }
    public virtual ApprovalRequest ApprovalRequest { get; set; } = null!;

    public Guid ApprovalStepId { get; set; }
    public virtual ApprovalStep ApprovalStep { get; set; } = null!;

    public string Action { get; set; } = string.Empty; // Approved, Rejected
    public Guid ApproverUserId { get; set; }
    public virtual User ApproverUser { get; set; } = null!;
    public string Comments { get; set; } = string.Empty;
}






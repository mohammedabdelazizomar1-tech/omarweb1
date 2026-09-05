using System;

namespace BusinessManagement.Domain.Entities;

public class PassengerNote : BaseEntity
{
    public Guid PassengerId { get; set; }
    public virtual Passenger Passenger { get; set; } = null!;

    public string NoteText { get; set; } = string.Empty;
}

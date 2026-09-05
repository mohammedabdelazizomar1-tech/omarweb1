using System;

namespace BusinessManagement.Domain;

public interface ICurrentUserProvider
{
    string GetCurrentUsername();
    Guid GetCurrentTenantId();
}

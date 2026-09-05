# Authentication & MFA Architecture

The authentication system is built using ASP.NET Core Cookie Authentication and enhanced with Multi-Factor Authentication (MFA).

## Session Security Configuration
- **SameSite**: Enforced to `Strict` to prevent Cross-Site Request Forgery (CSRF).
- **HttpOnly**: Cookie is set as `HttpOnly = true` to prevent accessibility from client-side JavaScript.
- **SecurePolicy**: Set to `SameAsRequest` or `Always` to enforce SSL encryption in transmission.

## MFA Login Flow Chart
```mermaid
sequenceDiagram
    actor User as User
    participant App as Web Server
    participant DB as PostgreSQL DB
    
    User->>App: Submit Username & Password
    App->>DB: Query User & Validate Hash (PBKDF2)
    alt Invalid Password
        App-->>User: Display Error
    else Valid Password
        App->>DB: Check if MFA is Enabled
        alt MFA Enabled
            App-->>User: Request TOTP Code
            User->>App: Submit TOTP Code
            App->>App: Validate code using TotpHelper
            alt TOTP Valid
                App-->>User: Setup Cookie & Login
            else TOTP Invalid / Recovery Code used
                App->>DB: Query UserMfaRecoveryCode
                alt Valid Recovery Code
                    App->>DB: Consume Recovery Code (Mark Used)
                    App-->>User: Setup Cookie & Login
                else
                    App-->>User: Reject Login
                end
            end
        else MFA Disabled
            App-->>User: Setup Cookie & Login
        end
    end
```

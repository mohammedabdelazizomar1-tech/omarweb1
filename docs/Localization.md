# System Localization Specification

The system is localized, defaulting to Arabic (Egypt) with support for English (US).

## Culture Management Configuration
Enforced in `Program.cs` via:
```csharp
var supportedCultures = new[] { "ar-EG", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ar-EG")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
```

## Resources Folder Strategy
Localization strings are managed in XML `.resx` files inside a dedicated `Resources` folder matching the controllers/views namespace structure.
- **Arabic Translation**: Default resource files.
- **English Translation**: `[View/ControllerName].en-US.resx` files.

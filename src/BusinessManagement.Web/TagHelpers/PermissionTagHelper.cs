using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace BusinessManagement.Web.TagHelpers;

[HtmlTargetElement(Attributes = "asp-permission")]
public class PermissionTagHelper : TagHelper
{
    private readonly IAuthorizationService _authorizationService;

    [HtmlAttributeName("asp-permission")]
    public string Permission { get; set; } = string.Empty;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public PermissionTagHelper(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Permission)) return;

        var user = ViewContext.HttpContext.User;
        var authorized = await _authorizationService.AuthorizeAsync(user, Permission);

        if (!authorized.Succeeded)
        {
            output.SuppressOutput();
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessManagement.Web.Middlewares;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Generate a secure cryptographic nonce for this request (Point 15/v2 & Point 7/v3)
        byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);
        string nonce = Convert.ToBase64String(nonceBytes);
        context.Items["ScriptNonce"] = nonce;

        // 2. Add security headers (Point 20/v2 & Point 7/v3)
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
        context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "unsafe-none");
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
        context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
        context.Response.Headers.Append("Origin-Agent-Cluster", "?1");

        // CSP including nonce for script-src
        // Note: 'unsafe-inline' is required to support inline onclick/onchange handlers used throughout views.
        // 'nonce-{nonce}' was removed from script-src because modern browsers ignore 'unsafe-inline' if a nonce is present.
        // This blocks all inline event attributes (onclick=, onchange=, etc.) breaking all button interactions.
        string cspHeader = $"default-src 'self'; " +
                           $"script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                           $"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                           $"font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                           $"img-src 'self' data: https://images.unsplash.com; " +
                           $"connect-src 'self'; " +
                           $"frame-ancestors 'none'; " +
                           $"object-src 'none'; " +
                           $"base-uri 'none';";
        
        context.Response.Headers.Append("Content-Security-Policy", cspHeader);

        await _next(context);
    }
}

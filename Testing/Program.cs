using Testing.Components;
using Testing.Components.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. REGISTER SERVICES
// ==========================================

// Razor Components & Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Custom Services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<EsiarService>();

// HTTP Client
builder.Services.AddHttpClient();

// Controllers (for API endpoints like PdfController)
builder.Services.AddControllers();

var app = builder.Build();

// ==========================================
// 2. CONFIGURE HTTP PIPELINE (ORDER MATTERS!)
// ==========================================

// Development vs Production handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();  // HSTS should come early
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Static Files (CSS, JS, images)
app.UseStaticFiles();

// Anti-forgery (for Blazor forms)
app.UseAntiforgery();

// Map Controllers (API endpoints) - MUST come before Razor Components
app.MapControllers();

// Map Razor Components (Blazor)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Run the application
app.Run();
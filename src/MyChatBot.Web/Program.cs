using MyChatBot.Web;
using MyChatBot.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Aspire plumbing: logging, tracing, health checks, service discovery.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// "https+http://api" is NOT a real web address. Aspire's service discovery swaps "api"
// for the real address of the API project (whatever port Aspire chose).
builder.Services.AddHttpClient<ChatApiClient>(client =>
    client.BaseAddress = new Uri("https+http://api"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

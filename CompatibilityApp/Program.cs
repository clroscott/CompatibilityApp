using AutoMapper;
using CompatibilityApp.Components;
using CompatibilityApp.Domain.DayFiance.People;
using CompatibilityApp.Domain.DayFiance.Ratings;
using CompatibilityApp.Domain.DayFiance.Relationships;
using CompatibilityApp.Domain.DayFiance.Seasons;
using CompatibilityApp.Infrastructure.Data;
using CompatibilityApp.Infrastructure.DayFiance.People;
using CompatibilityApp.Infrastructure.DayFiance.Ratings;
using CompatibilityApp.Infrastructure.DayFiance.Relationships;
using CompatibilityApp.Infrastructure.DayFiance.Seasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CompatibilityApp.Domain.Common.Images;
using CompatibilityApp.Infrastructure.Common.Images;
using CompatibilityApp.Domain.DayFiance.Calculations;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


#if DEBUG
var connName = "Test";
#else
    var connName = "Prod";
#endif


var connectionString = builder.Configuration.GetConnectionString(connName)
    ?? throw new InvalidOperationException($"Connection string '{connName}' not found.");

builder.Services.AddDbContext<CompatibilityDBContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddAutoMapper(cfg => { }, typeof(PersonProfile));
builder.Services.AddAutoMapper(cfg => { }, typeof(RatingTypeProfile));
builder.Services.AddAutoMapper(cfg => { }, typeof(RelationshipProfile));
builder.Services.AddAutoMapper(cfg => { }, typeof(SeasonProfile));


builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IRatingTypeService, RatingTypeService>();
builder.Services.AddScoped<IRelationshipService, RelationshipService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IRatingScoreCalculator, RatingScoreCalculator>();


builder.Services.AddScoped<IImageStorageService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new FileSystemImageStorageService(env.WebRootPath);
});
builder.Host.UseWindowsService();
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

app.Run();

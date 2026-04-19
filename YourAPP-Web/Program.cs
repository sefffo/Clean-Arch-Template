using Microsoft.EntityFrameworkCore;
using YourAPP_Persistence.Data.DbContext;
using YourAPP_Services.FluentValidationMiddleWare;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region Dependency Injection / Registrerations

#region DB Registerations and Connections
// NOTE: Make sure to add the matching EF Core provider NuGet package to YourAPP-Persistence.csproj
// e.g. Microsoft.EntityFrameworkCore.SqlServer  --> UseSqlServer
//      Npgsql.EntityFrameworkCore.PostgreSQL    --> UseNpgsql
builder.Services.AddDbContext<YourAPPDbContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("")));

builder.Services.AddDbContext<YourAPPDbContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("")));

#endregion


#region Service Registerations

builder.Services.AddApplicationServices();

#endregion



#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

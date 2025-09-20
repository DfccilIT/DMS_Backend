using ModuleManagementBackend.API.ActionFilter;
using ModuleManagementBackend.API.Extension;
using ModuleManagementBackend.API.SecurityHandler;
using ModuleManagementBackend.BAL.Extension;
using ModuleManagementBackend.DAL.Extension;
using ModuleManagementBackend.Logger.ExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option =>
{
    option.Filters.Add<SSOTokenValidateFilter>();

});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiProjectServices(builder.Configuration);
builder.Services.AddBusinessLayerServices(builder.Configuration);
builder.Services.AddDataAccessLayerServices(builder.Configuration);
app.MapGet("/health", () =>
 {
     return Results.Ok(new { status = "Healthy" });
 });
 app.MapGet("/", () => Results.Ok("API is running"));
var app = builder.Build();
app.ExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwaggerSecurity();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerSecurity();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();
app.UseCors("AllowOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

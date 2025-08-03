var builder = WebApplication.CreateBuilder(args);

// Enable MVC with Views
builder.Services.AddControllersWithViews(); // ✅ Required for Razor views

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();            // ✅ Required for CSS/JS/images
app.UseRouting();
app.UseAuthorization();

// ✅ Correct MVC routing - only define once
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TeacherPage}/{action=New}/{id?}");

// ✅ Allow API routes too
app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProjectDb>(opt => opt.UseInMemoryDatabase("ProjectList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/projectitems", async (ProjectDb db) =>
    await db.Projects.ToListAsync());

app.MapGet("/projectitems/complete", async (ProjectDb db) =>
    await db.Projects.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/projectitems/{id}", async (int id, ProjectDb db) =>
    await db.Projects.FindAsync(id)
        is Project project
            ? Results.Ok(project)
            : Results.NotFound());

app.MapPost("/projectitems", async (Project project, ProjectDb db) =>
{
    db.Projects.Add(project);
    await db.SaveChangesAsync();

    return Results.Created($"/projectitems/{project.Id}", project);
});

app.MapPut("/projectitems/{id}", async (int id, Project inputProject, ProjectDb db) =>
{
    var project = await db.Projects.FindAsync(id);

    if (project is null) return Results.NotFound();

    project.Name = inputProject.Name;
    project.IsComplete = inputProject.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/projectitems/{id}", async (int id, ProjectDb db) =>
{
    if (await db.Projects.FindAsync(id) is Project project)
    {
        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return Results.Ok(project);
    }

    return Results.NotFound();
});

app.Run();

class Project
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class ProjectDb : DbContext
{
    public ProjectDb(DbContextOptions<ProjectDb> options)
        : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
}
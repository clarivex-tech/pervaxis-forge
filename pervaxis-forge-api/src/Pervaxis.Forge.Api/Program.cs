// TODO: Phase 0 — wire DI, map endpoints
// See FORGE_BLUEPRINT_BFF.md Phase 0 for implementation details

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();

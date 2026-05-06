using Microsoft.SemanticKernel;
using PropertySupport.RagApi.Configuration;
using PropertySupport.RagApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

builder.Services.Configure<AzureAiSearchOptions>(
    builder.Configuration.GetSection(AzureAiSearchOptions.SectionName));

builder.Services.Configure<ChatbotOptions>(
    builder.Configuration.GetSection(ChatbotOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IKernelBuilderService, KernelBuilderService>();
builder.Services.AddSingleton<Kernel>(sp => sp.GetRequiredService<IKernelBuilderService>().BuildKernel());

builder.Services.AddSingleton<IKnowledgeSearchService, AzureAiSearchKnowledgeService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("LocalDevelopment");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

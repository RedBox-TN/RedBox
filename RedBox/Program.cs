using RedBox.Email_utility;
using RedBox.Permission_Utility;
using RedBox.PermissionUtility;
using RedBox.Providers;
using RedBox.Services;
using RedBox.Settings;
using RedBoxAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcReflection();

builder.Services.Configure<RedBoxDatabaseSettings>(builder.Configuration.GetSection("RedBoxDB"));
var settings = builder.Configuration.GetSection("RedBoxApplicationSettings");
builder.Services.Configure<RedBoxApplicationSettings>(settings);
builder.Services.Configure<RedBoxEmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton<IRedBoxEmailUtility, RedBoxEmailUtility>();
builder.Services.AddSingleton<IPermissionUtility, PermissionUtility>();
builder.Services.AddSingleton<IClientsRegistryProvider, ClientsRegistryProvider>();

builder.AddRedBoxAuthenticationAndAuthorization();

builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize =
        (settings.GetValue<int>("MaxAttachmentSizeMb") * (settings.GetValue<int>("MaxAttachmentsPerMsg") + 1) +
         settings.GetValue<int>("MaxMessageSizeMb")) * 1024 * 1024;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorAppOrigin",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});


var app = builder.Build();

app.UseCors("AllowBlazorAppOrigin");

app.UseGrpcWeb();

app.MapGrpcService<AccountServices>().EnableGrpcWeb();
app.MapGrpcService<AdminService>().EnableGrpcWeb();
app.MapGrpcService<ConversationService>().EnableGrpcWeb();
app.MapGrpcService<SupervisedConversationService>().EnableGrpcWeb();

app.UseRedBoxAuthenticationAndAuthorization();

if (app.Environment.IsDevelopment()) app.MapGrpcReflectionService();

app.Run();
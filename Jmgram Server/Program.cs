using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using System.Net;
using System.Net.WebSockets;
using Jmgram_mk1.src.JMgram.Core.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JMgramDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("Jmgram mk1")));

builder.Services.AddIdentity<AppIdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<JMgramDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<AppIdentityUser>>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/Contact") || context.Request.Path.StartsWithSegments("/User")) // Проверьте, что это API-запрос
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApps", builder =>
    {
        builder.WithOrigins("https://localhost:3000", "https://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

options.AddPolicy("AllowReactAppWebSocket", builder =>
    {
        builder.WithOrigins("https://localhost:3000", "https://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jmgram Chat API", Version = "v1" });
    c.EnableAnnotations();
});

builder.Services.AddAuthorization();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<GetUserProfileUseCase>();
builder.Services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
builder.Services.AddScoped<IGetLastChatMessageUseCase, GetLastChatMessageUseCase>();
builder.Services.AddScoped<IGetContactRequestsUseCase, GetContactRequestsUseCase>();
builder.Services.AddScoped<CreateChatUseCase>();
builder.Services.AddScoped<AddUserToChatUseCase>();
builder.Services.AddScoped<IGetChatListUseCase, GetChatListUseCase>();
builder.Services.AddScoped<ISendMessageUseCase,SendMessageUseCase>();
builder.Services.AddScoped<UpdateMessageStatusUseCase>();
builder.Services.AddScoped<GetChatMessagesUseCase>();
builder.Services.AddScoped<AddContactUseCase>();
builder.Services.AddScoped<SendNotificationUseCase>();
builder.Services.AddScoped<GetNotificationListUseCase>();
builder.Services.AddScoped<CreateContactRequestUseCase>();
builder.Services.AddScoped<AcceptContactRequestUseCase>();
builder.Services.AddScoped<DeleteContactUseCase>();
builder.Services.AddScoped<UpdateContactNameUseCase>();
builder.Services.AddScoped<GetContactListUseCase>();
builder.Services.AddScoped<RespondToChatInviteUseCase>();
builder.Services.AddScoped<AddUserToChatNotificationUseCase>();
builder.Services.AddScoped<RemoveUserFromChatUseCase>();
builder.Services.AddScoped<DeleteChatUseCase>();
builder.Services.AddScoped<GetUserChatsUseCase>();
builder.Services.AddScoped<CreatePrivateChatUseCase>();
builder.Services.AddScoped<ILogger<AcceptContactRequestUseCase>, Logger<AcceptContactRequestUseCase>>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IContactRequestRepository, ContactRequestRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowReactApps");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");
app.UseWebSockets();


app.MapControllers();
app.Run();

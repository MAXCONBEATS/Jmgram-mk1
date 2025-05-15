using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.UseCases;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка DbContext
builder.Services.AddDbContext<JMgramDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("Jmgram mk1")));

// 2. Настройка Identity
builder.Services.AddIdentity<AppIdentityUser, IdentityRole>(options =>
{
    // Настройки Identity (Пароли, Lockout, User)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false; // Если требуется подтверждение учетной записи
})
    .AddEntityFrameworkStores<JMgramDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<AppIdentityUser>>(); // Добавляем SignInManager

// 3. Настройка Cookie Authentication (Упрощенная версия)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Только для HTTPS!
    options.Cookie.SameSite = SameSiteMode.None; // Требуется для работы с CORS
});

// 4. Добавляем Authentication и Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".AspNetCore.Identity.Application";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    });

builder.Services.AddAuthorization();

// 5. Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7142")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 6. Регистрируем сервисы (Repositories, UseCases)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddScoped<GetUserProfileUseCase>();
builder.Services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
builder.Services.AddScoped<CreateChatUseCase>();
builder.Services.AddScoped<AddUserToChatUseCase>();
builder.Services.AddScoped<GetChatListUseCase>();
builder.Services.AddScoped<SendMessageUseCase>();
builder.Services.AddScoped<UpdateMessageStatusUseCase>();

// 7. Регистрируем контроллеры и другие сервисы
builder.Services.AddScoped<ChatController>(); // Убедитесь, что ChatController существует и находится в правильном namespace
builder.Services.AddHttpContextAccessor();

// 8. Добавляем MVC и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 9. Включаем CORS, Authentication и Authorization
app.UseCors();
app.UseStaticFiles(); // Для статических файлов (CSS, JS, Images)
app.UseRouting(); // Добавляем routing

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using Application.Common.Exceptions;
using Application.Common.Exciptions;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        // إعدادات الـ JSON عشان تطلع بالتنسيق الصغير (camelCase) المتوافق مع الـ Frontend
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // تسجيل الخطأ في الـ Logs بالتفصيل للمهندس المسؤول
            _logger.LogError(ex,
                "Unhandled Exception | Path: {Path} | Method: {Method} | Message: {Message}",
                context.Request.Path,
                context.Request.Method,
                ex.Message);

            context.Response.ContentType = "application/json";

            // فك تشفير الإيرور ومعرفة الـ StatusCode والرسالة والـ لستة الفرعية
            var (statusCode, message, errors) = MapException(ex);

            context.Response.StatusCode = (int)statusCode;

            // توحيد شكل الرد (API Response Structure) لكل الأبلكيشن
            var response = new
            {
                Success = false,
                Message = message,
                Data = (object?)null,
                Errors = errors
            };

            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await context.Response.WriteAsync(json);
        }

        private (HttpStatusCode statusCode, string message, List<string>? errors) MapException(Exception ex)
        {
            return ex switch
            {
                // 1. لقط خطأ الـ Validation المخصص وقراءة الـ Errors اللي جواه
                FluentValidation.ValidationException validationEx => (
                    HttpStatusCode.BadRequest,
                    "Validation failed",
                    validationEx.Errors.Select(e => e.ErrorMessage).ToList()
                ),

                // 2. لقط الـ BadRequestException المخصص وقراءة الـ Errors لو موجودة
                BadRequestException badRequestEx => (
                    HttpStatusCode.BadRequest,
                    badRequestEx.Message,
                    badRequestEx.Errors.Any() ? badRequestEx.Errors : null
                ),

                // 3. لقط أي كلاس بيكبر ويورث من الـ AppException الأساسي (قراءة ديناميكية للـ StatusCode)
                AppException appEx => (
                    (HttpStatusCode)appEx.StatusCode,
                    appEx.Message,
                    null
                ),

                // 4. أخطاء الـ .NET الشائعة الأخرى
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "Unauthorized access",
                    null
                ),

                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "The requested resource was not found.",
                    null
                ),

                // 5. الأخطاء غير المتوقعة (مأمنة حسب البيئة)
                _ => (
                    HttpStatusCode.InternalServerError,
                    _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                    null
                )
            };
        }
    }
}
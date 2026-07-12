using Application.Common.Exciptions;
namespace Application.Common.Exceptions
{
    public class BadRequestException : AppException
    {
        public List<string> Errors { get; } = new();

        public BadRequestException(string message)
            : base(message, 400)
        {
        }

        public BadRequestException(List<string> errors)
            : base("Bad request", 400)
        {
            Errors = errors;
        }
    }
}

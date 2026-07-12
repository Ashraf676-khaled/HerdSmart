using Application.Common.Exciptions;
namespace Application.Common.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, 409)
        {
        }
    }
}

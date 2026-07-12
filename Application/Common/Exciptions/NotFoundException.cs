using Application.Common.Exciptions;

namespace Application.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, 404)
        {
        }
    }
}

namespace Application.Common.Interfaces
{
    public interface ITenantProvider
    {
        Ulid GetTenantId ();
        Guid GetUserId();
        String GetUserRole ();
        bool IsAuthenticated();

    }
}

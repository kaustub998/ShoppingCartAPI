namespace EcorpAPI.Services
{
    public static class CommonService
    {
        public static Int32 GetUserId(HttpContext context)
        {
            int userId = 0;
            var user = context.User;

            try
            {
                var userIdClaim = user.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (userIdClaim != null)
                    Int32.TryParse(userIdClaim.Value, out userId);
            }
            catch (Exception ex) { }

            return userId;
        }
    }
}

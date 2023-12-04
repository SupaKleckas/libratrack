using Microsoft.AspNetCore.Identity;

namespace LibraTrack.Auth.Model
{
    public class User : IdentityUser
    {
        public bool forceRelogin { get; set; }
		//public int? AssignedLibrary { get; set; }
	}
}

using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTree
{
    // todo
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission UseContentTree = new Permission("UseContentTree", "Use the content tree");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { UseContentTree };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { UseContentTree }
                }
            };
        }
    }
}
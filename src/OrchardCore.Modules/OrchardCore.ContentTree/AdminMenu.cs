using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.ContentTree
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(S["Content"], content => content
                    .Add(S["Tree"], "1.5", layers => layers
                        .Permission(Permissions.UseContentTree)
                        .Action("List", "Admin", new { area = "OrchardCore.ContentTree" })
                        .LocalNav()
                    ));
        }
    }
}

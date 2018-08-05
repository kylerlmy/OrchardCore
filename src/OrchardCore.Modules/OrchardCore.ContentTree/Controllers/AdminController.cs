using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.ContentTree.Controllers
{
    public class AdminController: Controller
    {
        public AdminController()
        {

        }

        public IActionResult List()
        {
            return View();
        }
    }
}

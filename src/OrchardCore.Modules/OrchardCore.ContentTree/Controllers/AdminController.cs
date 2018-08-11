using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentTree.Services;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

using OrchardCore.ContentManagement.Records;
using YesSql;

using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.ContentTree.ViewModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OrchardCore.ContentTree.Models;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentTree.Controllers
{
    public class AdminController: Controller, IUpdateModel
    {
        private readonly IEnumerable<ITreeNodeProvider> _treeNodeProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentManager _contentManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public AdminController(
            IEnumerable<ITreeNodeProvider> treeNodeProviders,
            IAuthorizationService authorizationService,
            ISession session,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IStringLocalizer<AdminController> stringLocalizer,
            ILogger<AdminController> logger)
        {
            _treeNodeProviders = treeNodeProviders;
            _authorizationService = authorizationService;
            _session = session;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;

            T = stringLocalizer;
            Logger = logger;
        }

        public IStringLocalizer T { get; set; }
        public ILogger Logger { get; set; }
        public dynamic New { get; set; }

        public IActionResult List()
        {
            return View();
        }

        public async Task<ActionResult> GetTreeNodeProviders()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseContentTree))
            {
                return Unauthorized();
            }

            var result = _treeNodeProviders.Select(x => x.GetChildren("root", string.Empty).FirstOrDefault());

            return Json(result);
            //return Json(_treeNodeProviders.Select(x => x.Name).ToArray());
        }

        public async Task<ActionResult> GetChildren(string parentId, string parentType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseContentTree))
            {
                return Unauthorized();
            }

            var children = _treeNodeProviders
                .SelectMany(p => p.GetChildren(parentType, parentId)).ToArray();
            return Json(children);
        }
        //, Dictionary<string,string> providerSpecificParams
        public async Task<IActionResult> GetContentItems(
            string providerId, 
            Dictionary<string, string> providerParams,
            string contentStatusSelectedOption,
            bool ownedByMe,
            ContentsOrder sortBy, SortDirection sortDir,
            string returnUrl)
        {


            var provider = _treeNodeProviders.Where(x => x.Id == providerId).FirstOrDefault();
            if (provider == null)
            {
                return NotFound();
            }

            // todo avoid this ugly parsing
            var contentStatusFilter = ContentsStatusFilter.AllVersions;
            Enum.TryParse<ContentsStatusFilter>(contentStatusSelectedOption, out contentStatusFilter);
            
            var commonParams = new CommonContentTreeParams
            {
                ContentStatusFilter = contentStatusFilter,
                OwnedByMe = ownedByMe,
                ReturnUrl = returnUrl,
                SortBy = sortBy,
                SortDirection = sortDir
            };

            // use try-catch to enable providers to provide meaningful errors.
            var pageOfContentItems = await provider.GetContentItems(providerParams, commonParams);

            if (pageOfContentItems == null)
            {
                return GetProblemDetailsResult(T["Error getting content items"], T["Can't get a list of content items."], 400);
            }

            // todo: enable returning shapes instead of json. Right now we are using VueJS only for UI composition
            var contentItemSummaries = new List<ContentItemViewModel>();
            foreach (var ci in pageOfContentItems)
            {
                 contentItemSummaries.Add(await getContentItemViewModel(ci, returnUrl));
            }

            return Json(contentItemSummaries);
        }


        private async  Task<ContentItemViewModel> getContentItemViewModel(ContentItem ci, string returnUrl)
        {
            //contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "SummaryAdmin"));
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var displayText = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci)).DisplayText;
            var editUrl = "";
            var displayUrl = "";
            var removeUrl = "";

            var hasPublished = await _contentManager.HasPublishedVersionAsync(ci);
            var hasDraft = ci.HasDraft();

            ContentStatus status;
            if (hasPublished && hasDraft)
            {
                status = ContentStatus.PublishedWithDraft;
            }
            else if (hasPublished)
            {
                status = ContentStatus.PublishedOnly;
            }
            else
            {
                status = ContentStatus.DraftOnly;
            }
            string FullRequestPath = HttpContext.Request.PathBase + HttpContext.Request.Path + HttpContext.Request.QueryString;

            ContentItemMetadata metadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);
            // todo: handle return Url. metadata.EditorRouteValues.Add("returnUrl", returnUrl);
            if (metadata.EditorRouteValues != null)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    metadata.EditorRouteValues.Add("returnUrl", returnUrl);
                }
                editUrl = urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues);
            }
            if (metadata.RemoveRouteValues != null)
            {
                removeUrl = urlHelper.Action(metadata.RemoveRouteValues["action"].ToString(), metadata.RemoveRouteValues);
            }
            if (metadata.DisplayRouteValues != null)
            {
                displayUrl = urlHelper.Action(metadata.DisplayRouteValues["action"].ToString(), metadata.DisplayRouteValues);
            }


            return new ContentItemViewModel
            {
                ContentItemId = ci.ContentItemId,
                DisplayText = displayText,
                ContentType = ci.ContentType,
                Author = ci.Author,
                Owner = ci.Owner,
                Status = status.ToString(),
                CreatedUtc = String.Format("{0:g}", ci.CreatedUtc),
                ModifiedUtc = String.Format("{0:g}", ci.ModifiedUtc),
                PublishedUtc = String.Format("{0:g}", ci.PublishedUtc),
                EditUrl = editUrl,
                RemoveUrl = removeUrl,
                DisplayUrl = displayUrl
            };

        }

        private ObjectResult GetProblemDetailsResult(string title, string detail, int statusCode = 400, string typeUrl = "", string instance = "")
        {
            var details = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = typeUrl,
                Instance = instance
            };


            return new ObjectResult(details)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = statusCode
            };
        }
    }
}

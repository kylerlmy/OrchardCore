using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.ContentTree.ViewModels
{
    public class ContentItemViewModel
    {
        public string ContentItemId { get; set; }
        public string DisplayText { get; set; }
        public string ContentType { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
        public string CreatedUtc { get; set; }
        public string ModifiedUtc { get; set; }
        public string PublishedUtc { get; set; }
        public string EditUrl { get; set; }
        public string RemoveUrl { get; set; }
        public string DisplayUrl { get; set; }
    }
    public enum ContentsOrder
    {
        Modified,
        Published,
        Created
    }
    // the 3 possible status in which a content item can be
    public enum ContentStatus
    {
        DraftOnly,
        PublishedOnly,
        PublishedWithDraft
    }

    // the 3 options that an user can select on an input select.
    public enum ContentsStatusFilter
    {
        Draft, // show DraftOnly and PublishedWithDraft 
        Published, // show PublishedOnly and PublishedWithDraft
        AllVersions // show DraftOnly, PublishedOnly and PublishedWithDraft
    }

    public enum SortDirection
    {
        Descending,
        Ascending
    }

    public class CommonContentTreeParams
    {
        public ContentsStatusFilter ContentStatusFilter { get; set; }
        public bool OwnedByMe { get; set; }
        public ContentsOrder SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
        public string ReturnUrl { get; set; }
    }
}

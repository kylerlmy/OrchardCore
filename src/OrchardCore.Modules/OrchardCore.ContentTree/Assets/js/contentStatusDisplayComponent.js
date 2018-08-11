// <content-status-display> component
Vue.component('contentStatusDisplay', {
    template: '\
        <span :title="status">\
            <i class="fa fa-circle mr-2 status-mark published" :class="{ exist : publishedExist }" ></i>\
            <i class="fa fa-circle mr-2 status-mark draft" :class="{ exist : draftExist }"></i>\
        </span>\
    ',
    props: {
        status: String
    },
    data: function () {
        return {
            contentStatusEnumValues: {}
        }
    },
    mounted: function () {
        this.contentStatusEnumValues = content_tree.ContentStatusEnumValues;        
    },
    computed: {
        statusText: function () {
            return this.contentStatusEnumValues[this.status];
        },
        publishedExist: function () {
            return (this.status.toLowerCase() == 'publishedwithdraft') || (this.status.toLowerCase() == 'publishedonly');
        },
        draftExist: function () {
            return (this.status.toLowerCase() == 'publishedwithdraft') || (this.status.toLowerCase() == 'draftonly');
        }        
    }
});

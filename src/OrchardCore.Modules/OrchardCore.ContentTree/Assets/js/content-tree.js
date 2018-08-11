var bus = new Vue();
var contentTreeApp = new Vue({
    el: '#content-tree',
    data: {
        res: {},
        sortDirections: {},
        providers: [],        
        contentItems: [],
        currentNode: {},
        filterBoxParams: '',
        sortBy: '',
        sortAsc: false,        
        queryUrl: '',
        returnUrl: '',
        noResults: false,
        errors: []
    },
    created: function () {
        var self = this;

        bus.$on('currentNodeChanged', function (currentNode) {
            console.log('currentNodeChanged');
            console.log(currentNode);
            if (!currentNode) {                
                return;
            }
            self.currentNode = currentNode;

            if (currentNode.url) {                
                self.queryUrl = currentNode.url;
            }

        });
        bus.$on('leafUrlChanged', function (url) {            
            self.queryUrl = url;
        });

        bus.$on('filterBoxChanged', function (filterBoxParams) {
            self.filterBoxParams = filterBoxParams;            
        });

    },
    mounted: function () {
        this.res = content_tree.res;
        this.sortDirections = content_tree.SortDirections;
        this.returnUrl = content_tree.returnUrl;

        this.loadProviders();        
    },
    computed: {
        currentQuery: function () {
            var sortDirection = this.sortAsc ? this.sortDirections.asc : this.sortDirections.desc;            
            return this.queryUrl
                    + '&' + this.filterBoxParams
                    + '&sortBy=' + this.sortBy
                    + '&sortDir=' + sortDirection
                    + '&returnUrl=' + this.returnUrl ;
        }
    },
    watch: {
        currentQuery: function () {
            this.queryContentItems();
        }
    },
    methods: {
        loadProviders: function () {
            var self = this;
            var url = content_tree.GetTreeNodeProvidersUrl;
            $.ajax({
                url: url,
                method: 'GET',
                success: function (data) {
                    self.providers = data;                   
                },
                error: function (error) {
                    console.log('error getting providers');
                }
            });
        },
        queryContentItems: function () {
            var self = this;
            self.noResults = false;
            bus.$emit('clearNotifications');
            if (self.queryUrl === '') {
                return; // not a leaf selected yet?
            }

            $.ajax({
                url: self.currentQuery,
                method: 'GET',
                success: function (data) {
                    console.log('new content items are:');
                    console.log(data);
                    self.contentItems = data;
                    self.noResults = data.length < 1;
                },
                error: function (error) {
                    emtpy = false;
                    self.contentItems = [];                    
                    bus.$emit('ajaxErrorNotificaton', error);
                }
            });
        },
        changeSort: function (newSort) {
            if (this.sortBy === newSort) {
                this.sortAsc = !this.sortAsc;
            } else {
                this.sortAsc = true;
                this.sortBy = newSort;
            }
        }
    } 
})
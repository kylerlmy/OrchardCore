var bus = new Vue();
var contentTreeApp = new Vue({
    el: '#content-tree',
    data: {
        providers: [],
        message: 'Hello Content Tree from VueJs'
    },
    mounted: function() {
        this.loadProviders();
    },
    methods: {
        loadProviders: function () {
            console.log('loading providers');
            //this.selectedMedias = [];
            var self = this;
            $.ajax({
                url: $('#GetTreeNodeProvidersUrl').val(),
                method: 'GET',
                success: function (data) {
                    console.log('data is ' + data);
                    self.providers = data;
                    //data.forEach(function (item) {
                    //    item.open = false;
                    //});
                    //self.mediaItems = data;
                    //self.selectedMedias = [];
                    //self.sortBy = '';
                    //self.sortAsc = true;
                },
                error: function (error) {
                    console.log('error getting providers');                    
                }
            });
        }
    } 
})
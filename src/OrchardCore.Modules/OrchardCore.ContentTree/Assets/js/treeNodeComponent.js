// <treenode> component
Vue.component('treeNode', {
    template: '\
        <li :class="{selected: isSelected}" >\
            <div :class="{treeroot: level == 1}">\
                <a href="javascript:;" :style="{ paddingLeft: padding + \'px\' }" v-on:click="select"  draggable="false" >\
                  <span v-on:click.stop="toggle" class="expand" :class="{opened: open, closed: !open, empty: empty}"><i class="fas fa-chevron-right"></i></span>  \
                  {{model.title}}\
                </a>\
            </div>\
            <ol v-show="open">\
                <tree-node v-for="folder in children"\
                        :key="folder.path"\
                        :model="folder" \
                        :selected-in-media-app="selectedInMediaApp" \
                        :level="level + 1">\
                </tree-node>\
            </ol>\
        </li>\
        ',
    name: 'treeNode',
    props: {
        model: Object,
        selectedInMediaApp: Object,
        level: Number
    },
    data: function () {
        return {
            open: false,
            children: null, // not initialized state (for lazy-loading)
            parent: null,
            isHovered: false,
            padding: 0
        }
    },
    computed: {
        empty: function () {
            return !this.children || this.children.length == 0;
        },
        isSelected: function () {
            return false; //return (this.selectedInMediaApp.name == this.model.name) && (this.selectedInMediaApp.path == this.model.path);
        }
    },
    mounted: function () {
        this.padding = this.level < 3 ?  26 : 26 + (this.level * 8);
    },
    created: function () {
        var self = this;
    },
    methods: {
        toggle: function () {
            console.log('toggling');
            this.open = !this.open;
            if (this.open && !this.children) {
                this.loadChildren();
            }
        },
        select: function () {
            console.log('selecting: first loadchildren, then call Url if not null');
            var self = this;
            bus.$emit('folderSelected', this.model);
            
            this.loadChildren();

            if (self.model.url) {
                console.log('calling this urlfff');
                console.log(self.model.url);
                $.ajax({
                    url: self.model.url,
                    method: 'GET',
                    success: function (data) {
                        console.log('children are:');
                        console.log(data);
                        //self.children = data;
                        //self.children.forEach(function (c) {
                        //    c.parent = self.model;
                        //});
                    },
                    error: function (error) {
                        emtpy = false;
                        console.error(error.responseText);
                    }
                });

                //$.ajax({
                //    url: $('#GetChildrenUrl').val() +
                //        '?parentId=' + encodeURIComponent(self.model.id)
                //        + '&parentType=' + encodeURIComponent(self.model.type),
                //    method: 'GET',
                //    success: function (data) {
                //        console.log('children are:');
                //        console.log(data);
                //        self.children = data;
                //        //self.children.forEach(function (c) {
                //        //    c.parent = self.model;
                //        //});
                //    },
                //    error: function (error) {
                //        emtpy = false;
                //        console.error(error.responseText);
                //    }
                //});
            } else {
                console.log('no url on this treenode, no call for content items');
            }
            
        },
        loadChildren: function () {            
            var self = this;
            if (this.open == false) {
                this.open = true;
            }
            $.ajax({
                url: $('#GetChildrenUrl').val() +
                    '?parentId=' + encodeURIComponent(self.model.id)
                    + '&parentType=' + encodeURIComponent(self.model.type),
                method: 'GET',
                success: function (data) {
                    console.log('children are:');
                    console.log(data);
                    self.children = data;
                    //self.children.forEach(function (c) {
                    //    c.parent = self.model;
                    //});
                },
                error: function (error) {
                    emtpy = false;
                    console.error(error.responseText);
                }
            });
        }
    }
});

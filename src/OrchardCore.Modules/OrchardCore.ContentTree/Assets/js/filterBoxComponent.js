// <filter-box> component
Vue.component('filterBox', {
    template: '\
        <nav class="nav filter-box ocf align-items-stretch form-inline ">\
            <div class="nav-item">\
                <div class="form-group mr-4 mb-2">\
                    <label for="contentStatusSelect" class="label mr-2">{{res.contentStatusLabelText}}</label>\
                    <select id="contentStatusSelect" class="form-control form-control-sm" v-model="selectedOptions.contentStatusSelectedOption">\
                        <option v-for="o in contentStatusFilterOptions" :value="o.value">{{o.text}}</option>\
                    </select>\
                </div>\
            </div>\
            <div class="nav-item form-inline">\
                <div class="form-check mr-4 mb-2">\
                    <input id="ownedByMeCheckbox" class="form-check-input" type="checkbox" :checked="selectedOptions.ownedByMe" v-model="selectedOptions.ownedByMe"/>\
                    <label for="ownedByMeCheckbox" class="form-check-label">{{res.ownedByMeLabelText}}</label>\
                </div>\
            </div>\
        </nav>\
    ',
    data: function () {
        return {
            res: {},
            contentStatusFilterOptions: [],
            selectedOptions: {
                contentStatusSelectedOption: {},
                ownedByMe: true
            }            
        }
    },
    mounted: function () {

        // retrieve strings
        this.res = content_tree.res;
        this.contentStatusFilterOptions = content_tree.ContentStatusFilterOptions;
        this.selectedOptions.contentStatusSelectedOption = this.contentStatusFilterOptions[0].value;
        console.log(content_tree.ContentStatusFilterOptions);
    },
    watch: {
        'selectedOptions.contentStatusSelectedOption': function () {
            this.emitChangeInfo();
        },
        'selectedOptions.ownedByMe': function () {
            this.emitChangeInfo();
        }
    },
    methods: {
        emitChangeInfo: function () {
            bus.$emit('filterBoxChanged', $.param(this.selectedOptions));
        }
    }
});

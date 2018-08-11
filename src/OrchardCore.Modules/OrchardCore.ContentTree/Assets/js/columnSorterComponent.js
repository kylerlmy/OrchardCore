// <column-sorter> component
Vue.component('columnSorter', {
    template: '\
        <div v-show="isActive" style="display: inline-block; margin-left: 8px;"> \
            <span v-show="asc"><i class="small fa fa-chevron-up"></i></span> \
            <span v-show="!asc"><i class="small fa fa-chevron-down"></i></span> \
        </div> \
        ',
    props: {
        colname: String,
        selectedcolname: String,
        asc: Boolean
    },
    computed: {
        isActive: function () {
            if (this.selectedcolname == '') {                
                return false;
            }
            return this.colname.toLowerCase() == this.selectedcolname.toLowerCase();
        }
    }
});

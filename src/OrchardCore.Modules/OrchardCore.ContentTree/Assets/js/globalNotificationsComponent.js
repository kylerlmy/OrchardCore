// <global-notifications> component  
Vue.component('globalNotifications', {
    template: '\
        <div id="global-notifications" v-if="notifications.length > 0">\
                <div class="alert" :class="alertClass(n)" v-for="n in notifications">\
                    <span>{{ n.title }} : {{n.detail}}</span>\
                </div>\
        </div>\
        ',
    data: function () {
        return {
            notifications: []
        }
    },
    created: function () {
        var self = this;

        // we expect an ajaxError that has a responseText an object like this https://tools.ietf.org/html/rfc7807        
        bus.$on('ajaxErrorNotificaton', function (ajaxError) {
            var notification = {
                isWarning: true,
                title: 'Error',
                detail: 'There was an error when calling the server.'
            };
            
            try {
                parsedResponse = JSON.parse(ajaxError.responseText);
                if (parsedResponse.title) { notification.title = parsedResponse.title; }
                if (parsedResponse.detail) { notification.detail = parsedResponse.detail; }
            } finally {
                self.notifications.push(notification);
            }

        });

        bus.$on('successNotificaton', function (notification) {
            if (!notification) {
                notification = { title: 'OK', detail: 'The operation completed successfully.' };
            }
            notification.isSuccess = true;
            self.notifications.push(notification);
        });

        bus.$on('clearNotifications', function () {
            self.notifications = [];
        })
    },
    methods: {
        alertClass: function (notification) {
            if (!notification) { return 'message-information'; }

            if (notification.isWarning) { return 'message-warning'; }
            else if (notification.isSuccess) { return 'message-success'; }
            else { return 'message-information'; }
        }
    }
});

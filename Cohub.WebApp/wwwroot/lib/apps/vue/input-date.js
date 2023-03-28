/// <reference path="../../vue/vue.js" />
Vue.component('input-date', {
    template: `
      <input
        type="date"
        ref="input"
        v-bind:value="dateToYYYYMMDD(value)"
        v-on:input="updateValue($event.target)"
        v-on:focus="selectAll"
      >
  `,
    props: {
        value: {
            type: String,
            default: null
        }
    },
    computed: {
        date: () => this.dateToYYYYMMDD(this.value)
    },
    methods: {
        dateToYYYYMMDD(d) {
            // Alternative implementations in https://stackoverflow.com/q/23593052/1850609
            return d && new Date(d).toISOString().split('T')[0];
        },
        updateValue: function (target) {
            this.$emit('input', this.value);
        },
        selectAll: function (event) {
            // Workaround for Safari bug https://stackoverflow.com/q/1269722
            setTimeout(function () {
                event.target.select()
            }, 0)
        }
    }
});

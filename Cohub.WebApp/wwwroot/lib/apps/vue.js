/// <reference path="utils.js" />
/// <reference path="../vue/vue.js" />

Vue.directive('focus', {
    // When the bound element is inserted into the DOM...
    inserted: function (el) {
        // Focus the element
        el.focus()
    }
})

Vue.directive('select', {
    // When the bound element is inserted into the DOM...
    inserted: function (el) {
        // Focus and select the element
        el.focus()
        el.select()
    }
})

/**
 * Executes the method name(s) passed as a value the element.
 */
Vue.directive('activate', {
    // See https://vuejs.org/v2/guide/custom-directive.html#Directive-Hook-Arguments
    inserted: function (el, binding) {
        const methods =
            typeof binding.value == "function" ? [binding.value] :
                typeof binding.value == "object" ? binding.value :
                    []
        setTimeout(() => {
            for (var method of methods) {
                method(el)
            }
        }, 1)
    }
})

Vue.component("autocomplete", {
    props: ["value"],
    template: "<input />",
    mounted: function () {
        const vm = this
        this.$el.value = this.value
        autocompleteWithDatalist(this.$el)
        this.$el.addEventListener("input", function () {
            vm.$emit("input", this.value);
        })
        this.$el.addEventListener("change", function () {
            vm.$emit("change", this.value);
        })
    },
    watch: {
        value: function (value) {
            this.$el.value = value
        }
        //options: function (options) {
        //    // update options
        //    $(this.$el)
        //        .empty()
        //        .select2({ data: options });
        //}
    },
    destroyed: function () {
        //$(this.$el)
        //    .off()
        //    .select2("destroy");
    }
});
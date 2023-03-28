/// <reference path="../utils.js" />
/// <reference path="../../vue/vue.js" />
/// <reference path="../vue.js" />
(() => {
    const root = document.getElementById("fin-deposit")
    const data = JSON.parse(root.getAttribute("data"))
    data.stash = []
    data.appAlert = "";
    var alertTimeout = null;
    var app = new Vue({
        el: root,
        data: () => data,
        methods: {

            setAlert: function (message) {
                this.appAlert = message
                alertTimeout = setTimeout(() => { this.appAlert = "" }, 10000)
            },
            dismissAlert: function () {
                alertTimeout = null
                this.appAlert = ""
            },

            addDeposit: debounce(async function () {
                if (this.model.newDeposit.depositorId) {
                    const deposit = JSON.parse(JSON.stringify(this.new.deposit))

                    deposit.depositorId = this.model.newDeposit.depositorId;
                    deposit.depositDate = this.model.defaultDepositDate;
                    deposit.newPayment = JSON.parse(JSON.stringify(this.new.depositPayment))

                    await this.refreshDeposit(deposit)

                    this.model.deposits.push(deposit)
                    this.stash = []

                    this.model.newDeposit.depositorId = null

                    return deposit
                }
            }),
            removeDeposit: function (iDeposit) {
                this.stash.push(this.model.deposits.splice(iDeposit, 1)[0])
            },
            refreshDeposit: debounce(async function (deposit) {
                const formData = new FormData()

                formData.append('isZero', this.isZero)

                formData.append('depositorId', deposit.depositorId)
                formData.append('depositDate', deposit.depositDate)
                formData.append('depositAmount', deposit.depositAmount)

                const response = await fetch(`/fin/deposit/refresh-deposit`, {
                    method: 'POST',
                    body: formData
                })

                if (response.ok) {
                    var json = await response.json()

                    deposit.depositorId = json.depositorId
                    deposit.depositDate = json.depositDate
                    deposit.depositAmount = json.depositAmount

                    if (deposit.payments.length == 0) {
                        deposit.payments = json.payments
                    }
                }
                else {
                    this.setAlert("An error occurred and the deposit was not refreshed.")
                }
            }),

            addPayment: debounce(async function (deposit) {
                if (deposit.depositDate && deposit.newPayment.organizationId && deposit.newPayment.categoryId && deposit.newPayment.periodId) {
                    const payment = JSON.parse(JSON.stringify(this.new.depositPayment))

                    payment.organizationId = deposit.newPayment.organizationId
                    payment.categoryId = deposit.newPayment.categoryId
                    payment.periodId = deposit.newPayment.periodId

                    await this.refreshPayment(payment, deposit, false, function (success) {
                        if (success) {
                            deposit.payments.push(payment)

                            deposit.newPayment.organizationId = null
                            deposit.newPayment.categoryId = null
                            deposit.newPayment.periodId = null
                        }
                    })

                }
                else {
                    this.setAlert("The Deposit Date and the return's Organization, Category and Period must be entered to add a payment.")
                }
            }),
            removePayment: function (deposit, paymentIndex) {
                deposit.payments.splice(paymentIndex, 1)[0]
            },
            refreshPaymentIfReady: async function (payment, deposit) {
                if (payment.organizationId && payment.categoryId && payment.periodId) {
                    await this.refreshPayment(payment, deposit)
                }
            },
            refreshPayment: async function (payment, deposit, createReturn, callback) {
                if (typeof callback == "undefined") callback = function () { }

                if (!payment.organizationId || !payment.categoryId || !payment.periodId) {
                    this.setAlert("The Organization, Category and Period must be entered.")
                    callback(false)
                    return
                }

                const formData = new FormData()

                formData.append('createReturn', Boolean(createReturn || false))

                formData.append('isZero', this.isZero)

                formData.append('deposit.depositorId', deposit.depositorId)
                formData.append('deposit.depositDate', deposit.depositDate)
                formData.append('deposit.depositAmount', deposit.depositAmount)

                formData.append('payment.organizationId', payment.organizationId)
                formData.append('payment.categoryId', payment.categoryId)
                formData.append('payment.periodId', payment.periodId)
                formData.append('payment.returnId', payment.returnId)
                formData.append('payment.paymentAmount', payment.paymentAmount)
                formData.append('payment.taxable', payment.taxable)
                formData.append('payment.excess', payment.excess)
                formData.append('payment.assessment', payment.assessment)
                formData.append('payment.fees', payment.fees)

                const response = await fetch(`/fin/deposit/refresh-payment`, {
                    method: 'POST',
                    body: formData
                })

                if (response.ok) {
                    var json = await response.json()

                    payment.organizationId = json.organizationId
                    payment.categoryId = json.categoryId
                    payment.periodId = json.periodId
                    payment.returnId = json.returnId
                    payment.paymentAmount = json.paymentAmount
                    payment.taxable = json.taxable
                    payment.excess = json.excess
                    payment.assessment = json.assessment
                    payment.fees = json.fees
                    payment.snapshot = json.snapshot

                    if (!payment.returnId || payment.returnId <= 0) {
                        this.setAlert("A matching return does not currently exist. Missing returns will be created when the deposit form is submitted.")
                    }

                    callback(true)
                    return
                }
                else {
                    let errorMessage = await response.text()
                    this.setAlert("An error occurred and the deposit was not refreshed. " + errorMessage)
                }

                callback(false)
                return
            },

            popStash: function () {
                if (this.stash.length > 0)
                    this.model.deposits.push(this.stash.pop())
            },

            sumPayments: function (deposit) {
                let sum = 0
                for (var payment of deposit.payments) {
                    sum += payment.paymentAmount ? Number(payment.paymentAmount) : 0
                }
                return sum.toFixed(2)
            }
        },
        computed: {
            depositPaymentSum: function () {
                let sums = []

                for (var i in this.model.deposits) {
                    let deposit = this.model.deposits[i]
                    sums.push(this.sumPayments(deposit))
                }

                return sums
            }
        }
    })
})()
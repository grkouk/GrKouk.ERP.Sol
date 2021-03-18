var productLib = (function () {
    let numberFormatter;
    let currencyFormatter;
    let numberParser;

    const setParsers = (parserNumber) => {
        numberParser = parserNumber;
    };
    const setFormatters = (formatterNumber, formatterCurrency) => {
        numberFormatter = formatterNumber;
        currencyFormatter = formatterCurrency;
    };
    const spinnerLoaderShow = ($spinnerElement) => {
        $spinnerElement.show();
    };
    const spinnerLoaderHide = ($spinnerElement) => {
        $spinnerElement.hide();
    };
    const spinnerLoaderIsVisible = ($spinnerElement) => {
        return ($spinnerElement.is(':visible'));
    };

    const makeAjaxCall = (uri, $SpElement) => {
        var timeout;
        return new Promise((resolve, reject) => {
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "GET",
                url: uri,

                success: function (data) {
                    resolve(data);
                },
                error: function (error) {
                    reject(error);
                },
                beforeSend: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    timeout = setTimeout(function () {
                        spinnerLoaderShow($SpElement);
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    spinnerLoaderHide($SpElement);
                    setTimeout(function () {
                        var isOpen = spinnerLoaderIsVisible($SpElement);
                        if (isOpen) {
                            spinnerLoaderHide($SpElement);
                        }
                    }, 2000);
                },
            });
        });
    };
    const getProductItemInfo = (itemId, transactorId, companyId) => {
        let uri = "/api/materials/productdata";
        uri += `?warehouseItemId=${itemId}`;
        uri += `&transactorId=${transactorId}`;
        uri += `&companyId=${companyId}`;
        var timeout;
        return new Promise((resolve, reject) => {
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "GET",
                url: uri,

                success: function (data) {
                    resolve(data);
                },
                error: function (error) {
                    reject(error);
                },
                beforeSend: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    timeout = setTimeout(function () {
                        $("#loadMe").modal({
                            backdrop: "static", //remove ability to close modal with click
                            keyboard: false, //remove option to close with keyboard
                            show: true, //Display loader!
                        });
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        //console.log("Checking for open modals");
                        var isOpen = $("#loadMe").hasClass("show");
                        if (isOpen) {
                            //console.log("There is an open Modal");
                            $("#loadMe").modal("hide");
                        }
                        /*else {
                            //console.log("No open modal");
                        }*/
                    }, 2000);
                },
            });
        });
    };
    const getCompanyBaseCurrencyInfoV0 = (companyId) => {
        let uri = "/api/materials/companyBaseCurrencyInfo";
        uri += `?companyId=${companyId}`;
        var timeout;
        return new Promise((resolve, reject) => {
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "GET",
                url: uri,

                success: function (data) {
                    resolve(data);
                },
                error: function (error) {
                    reject(error);
                },
                beforeSend: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    timeout = setTimeout(function () {
                        $("#loadMe").modal({
                            backdrop: "static", //remove ability to close modal with click
                            keyboard: false, //remove option to close with keyboard
                            show: true, //Display loader!
                        });
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        //console.log("Checking for open modals");
                        var isOpen = $("#loadMe").hasClass("show");
                        if (isOpen) {
                            //console.log("There is an open Modal");
                            $("#loadMe").modal("hide");
                        }
                        /*else {
                            //console.log("No open modal");
                        }*/
                    }, 2000);
                },
            });
        });
    };
    const getCompanyBaseCurrencyInfo = (companyId, spinnerElement) => {
        let uri = "/api/materials/companyBaseCurrencyInfo";
        uri += `?companyId=${companyId}`;

        return new Promise((resolve, reject) => {
            makeAjaxCall(uri, spinnerElement)
                .then((data) => {
                    resolve(data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    };
    const getCompanyCashFlowAccounts = (companyId, spinnerElement) => {
        let uri = "/api/materials/CashFlowAccountsForCompany";
        uri += `?companyId=${companyId}`;

        return new Promise((resolve, reject) => {
            makeAjaxCall(uri, spinnerElement)
                .then((data) => {
                    resolve(data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    };
    const setCompanyIdInSession = (companyId) => {
        let uri = "/api/Materials/SetCompanyInSession";
        uri += `?companyId=${companyId}`;
        var timeout;
        return new Promise((resolve, reject) => {
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "GET",
                url: uri,

                success: function (data) {
                    resolve(data);
                },
                error: function (error) {
                    reject(error);
                },
                beforeSend: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    timeout = setTimeout(function () {
                        $("#loadMe").modal({
                            backdrop: "static", //remove ability to close modal with click
                            keyboard: false, //remove option to close with keyboard
                            show: true, //Display loader!
                        });
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        //console.log("Checking for open modals");
                        var isOpen = $("#loadMe").hasClass("show");
                        if (isOpen) {
                            //console.log("There is an open Modal");
                            $("#loadMe").modal("hide");
                        }
                        /*else {
                            //console.log("No open modal");
                        }*/
                    }, 2000);
                },
            });
        });
    };
    const setGlobalizeLocale = (culture) => {
        return new Promise((resolve, reject) => {
            $.when(
                $.get("/lib/cldr-data/supplemental/likelySubtags.json"),
                $.get("/lib/cldr-data/main/" + culture + "/numbers.json"),
                $.get("/lib/cldr-data/main/" + culture + "/currencies.json"),
                $.get("/lib/cldr-data/supplemental/numberingSystems.json"),
                $.get("/lib/cldr-data/main/" + culture + "/ca-gregorian.json"),
                $.get("/lib/cldr-data/main/" + culture + "/timeZoneNames.json"),
                $.get("/lib/cldr-data/supplemental/timeData.json"),
                $.get("/lib/cldr-data/supplemental/currencyData.json"),
                $.get("/lib/cldr-data/supplemental/weekData.json")
            ).then(function () {
                // Normalize $.get results, we only need the JSON, not the request statuses.
                return [].slice.apply(arguments, [0]).map(function (result) {
                    return result[0];
                });
            }).then(Globalize.load)
                .then(function () {
                    console.log("Globalize culture loaded " + culture);
                    Globalize.locale(culture);
                    resolve();

                });
        });

    };
    const prepCurrencyInputs = () => {
        let $elementsToUpdateVal = $('.currency-input');
        $elementsToUpdateVal.each(function() {
            let $el = $(this);
            $el.blur((e) => {
                console.log("Inside blur event");
                let jEl = e.target;
                let jElInputValue = jEl.value;
                let typeOfInput = typeof jElInputValue;
                console.log(`typeof e.target=${typeOfInput}`);
                let jElValGlParser = numberParser(jElInputValue);
                jEl.dataset.actualvalue = jElValGlParser;
                jEl.value = currencyFormatter(jElValGlParser);
            });
            $el.focus((e) => {
                console.log("Inside focus event");
                let jEl = e.target;
                let jElAttrValue = jEl.dataset.actualvalue;
                let jElAttrValueParsed = parseFloat(jElAttrValue);
                let jElValFormatted = numberFormatter(jElAttrValueParsed);
                jEl.value = jElValFormatted;
                jEl.select();
            });

        });
    };
    const prepNumericInputs = () => {
        let $elementsToUpdateVal = $('.numeric-input');
        $elementsToUpdateVal.each(function () {
            let $el = $(this);
            $el.blur((e) => {
                console.log("Inside blur event");
                let jEl = e.target;
                let jElInputValue = jEl.value;
                let typeOfInput = typeof jElInputValue;
                console.log(`typeof e.target=${typeOfInput}`);
                let jElValGlParser = numberParser(jElInputValue);
                jEl.dataset.actualvalue = jElValGlParser;
                jEl.value = numberFormatter(jElValGlParser);
            });
            $el.focus((e) => {
                console.log("Inside focus event");
                let jEl = e.target;
                let jElAttrValue = jEl.dataset.actualvalue;
                let jElAttrValueParsed = parseFloat(jElAttrValue);
                let jElValFormatted = numberFormatter(jElAttrValueParsed);
                jEl.value = jElValFormatted;
                jEl.select();
            });

        });
    };
    const prepCurrencyInputsForPost = () => {
        let $elementsToUpdateVal = $('.currency-input');
        $elementsToUpdateVal.each(function () {

            var jEl = this;
            let jElAttrValue = jEl.dataset.actualvalue;
            let jElAttrValueParsed = parseFloat(jElAttrValue);
            let jElValFormatted = numberFormatter(jElAttrValueParsed);
            jEl.value = jElValFormatted;

        });
    };

    return {
        getProductItemInfo: getProductItemInfo,
        setCompanyIdInSession: setCompanyIdInSession,
        getCompanyBaseCurrencyInfo: getCompanyBaseCurrencyInfo,
        getCompanyCashFlowAccounts: getCompanyCashFlowAccounts,
        prepCurrencyInputsForPost: prepCurrencyInputsForPost,
        prepCurrencyInputs: prepCurrencyInputs,
        prepNumericInputs: prepNumericInputs,
        setGlobalizeLocale: setGlobalizeLocale,
        setParsers: setParsers,
        setFormatters: setFormatters
    };
})();
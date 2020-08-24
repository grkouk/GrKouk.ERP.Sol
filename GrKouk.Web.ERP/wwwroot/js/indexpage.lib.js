//Index Page library functions
const indexPageLib = {
     reallyIsNaN : function(x) {
        return x !== x;
    },
  pagerElementsClassName: "page-link",
  getPageElements: function () {
    let pageElements = document.getElementsByClassName(this.pagerElementsClassName);
    return pageElements;
  },
  getTableData: function (uri, pgIndex, pgSize, sortData, dateRange, companyFlt, searchFlt, currencyFlt) {
    uri += `?pageIndex=${pgIndex}`;
    uri += `&pageSize=${pgSize}`;
    uri += `&companyFilter=${companyFlt}`;
    uri += `&dateRange=${dateRange}`;
    uri += `&sortData=${sortData}`;
    uri += `&searchFilter=${searchFlt}`;
    uri += `&displayCurrencyId=${currencyFlt}`;
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
            console.log("Checking for open modals");
            var isOpen = $("#loadMe").hasClass("show");
            if (isOpen) {
              console.log("There is an open Modal");
              $("#loadMe").modal("hide");
            } else {
              console.log("No open modal");
            }
          }, 2000);
        },
      });
    });
  },
  bindDataToTable: (colDefs, result) => {
      console.log("inside bindDataToTable");
      console.log(result);
  },
  refreshTableData: (uri, colDefs) => {
    var pageIndexVal = parseInt($("#pageIndex").val());

    var pageIndex = this.reallyIsNaN(pageIndexVal) ? 1 : pageIndexVal;
    $("#pageIndex").val(pageIndex);

    var pageSize =
      $("#PageSize").val() == null || $("#PageSize").val().length == 0 ? 10 : parseInt($("#PageSize").val());

    var companyFlt = $("#CompanyFilter").val();
    var datePeriod = $("#DatePeriodFilter").val();
    var sortData = $("#currentSort").val();
    var searchFlt = $(".search_input").val();
    var $dcId = $("#CurrencySelector");

    var currencyFlt = $dcId.val() == null || $dcId.val().length == 0 ? 1 : parseInt($dcId.val());

    this.getTableData(uri, pageIndex, pageSize, sortData, datePeriod, companyFlt, searchFlt, currencyFlt)
      .then((data) => {
        this.bindDataToTable(colDefs, data);
      })
      .catch((error) => {
        console.log(error);
      });
  },
  addPagerElementEventListener: function () {
    let pagerElements = this.getPageElements();
    Array.from(pagerElements).forEach((item) => {
      item.addEventListener("click", (event) => {
        console.log("From pager event handler");
        let pagerAction = event.currentTarget.dataset.pageraction;
        if (pagerAction === undefined) {
          return;
        }
        var pageIndexEl = document.getElementById("pageIndex");
        let pageIndex = pageIndexEl.value;
        var pageSizeEl = document.getElementById("PageSize");
        let pageSize = pageSizeEl.value;
        var totalPagesEl = document.getElementById("totalPages");
        let totalPages = totalPagesEl.value;
        var totalRecordsEl = document.getElementById("totalRecords");
        let totalRecords = totalRecordsEl.value;

        switch (pagerAction) {
          case "GoToFirst":
            break;
          case "GoToPrevious":
            break;
          case "GoToNext":
            break;
          case "GoToLast":
            break;
          default:
        }
      });
    });
  },
  setPagerElementsClassName: function (className) {
    this.pagerElementsClassName = className;
  },
  gotoFirstPage: function () {},
  gotoLastPage: function () {},
  gotoNextPage: function () {},
  gotoPreviousPage: function () {},
  handlePagingUi: (totalPages, totalRecords, pageIndex, hasPrevious, hasNext) => {
    //$totalPages.val(totalPages);
    //$totalRecords.val(totalRecords);
    var pagingInfo = ` Page:${pageIndex} of ${totalPages} Total Items ${totalRecords}`;
    $("[name=PagingInfo]").text(pagingInfo);

    if (hasPrevious) {
      $("#GoToFirst, #GoToPrevious").parent().removeClass("disabled");
    } else {
      $("#GoToFirst, #GoToPrevious").parent().addClass("disabled");
    }
    if (hasNext) {
      $("#GoToLast, #GoToNext").parent().removeClass("disabled");
    } else {
      $("#GoToLast, #GoToNext").parent().addClass("disabled");
    }
  },
};

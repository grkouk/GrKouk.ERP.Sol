// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
//I put it function closure for not poluting global namespace
var loadOptionsForSelect = () => {
    $('select[data-source]').each(function () {
        var $select = $(this);

        $select.append('<option></option>');

        $.ajax({
            url: $select.attr('data-source')
        }).then(function (options) {
            options.map(function (option) {
                var $option = $('<option>');

                $option
                    .val(option[$select.attr('data-valueKey')])
                    .text(option[$select.attr('data-displayKey')]);

                $select.append($option);
            });
        });
    });
};

var grkoukCommon = {
    editPrototypeUrl: function (curSectionCode, sectionCode, creatorId) {
        if (curSectionCode === sectionCode) {
            return "#";
        }
        if (creatorId === -1) {
            return "#";
        }
        let t = "";
        switch (sectionCode) {
            case "SCNTRANSACTORTRANS":
                t = `/transactions/TransactorTransMng/Edit?id=${creatorId}`;
                break;
            case "SCNWARHSETRANS":
                t = `/transactions/WarehouseTransMng/Edit?id=${creatorId}`;
                break;
            case "SCNSELLCOMBINED":
                t = `/transactions/sellmaterialdoc/Edit?id=${creatorId}`;
                break;
            case "SCNBUYMATTRANS":
                t = `/transactions/buymaterialsdoc/Edit?id=${creatorId}`;
                break;

            default:
        }
        return t;
    }


};
let grkoukTableColumns = {
    getSelectAllRowsCol: function() {
        let cl = '<th name="selectAllRowsColumn"> <label class="custom-control custom-checkbox">  ';
        cl += '<input type="checkbox" class="custom-control-input" name="checkAllRows" >';
        cl += '<span class="custom-control-indicator"></span></label></th>';
        let $cl = $(cl);
        return $cl;
    },
   aLinkTemplate: function (strings, ...keys) {
        return (function (...values) {
            let dict = values[values.length - 1] || {};
            let result = [strings[0]];
            keys.forEach(function (key, i) {
                let value = Number.isInteger(key) ? values[key] : dict[key];
                result.push(value, strings[i + 1]);
            });
            return result.join('');
        });
    }
};


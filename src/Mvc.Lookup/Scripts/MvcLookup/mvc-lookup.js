/*!
 * Mvc.Lookup 2.0.0
 * https://github.com/NonFactors/MVC6.Lookup
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
var MvcLookupFilter = (function () {
    function MvcLookupFilter(control) {
        this.page = control.attr('data-page');
        this.rows = control.attr('data-rows');
        this.sort = control.attr('data-sort');
        this.order = control.attr('data-order');
        this.search = control.attr('data-search');
        this.additionalFilters = control.attr('data-filters').split(',').filter(Boolean);
    }

    MvcLookupFilter.prototype = {
        getQuery: function (search) {
            var filter = $.extend({}, this, search);
            var query = '?search=' + encodeURIComponent(filter.search) +
                '&sort=' + encodeURIComponent(filter.sort) +
                '&order=' + encodeURIComponent(filter.order) +
                '&rows=' + encodeURIComponent(filter.rows) +
                '&page=' + encodeURIComponent(filter.page) +
                (filter.ids ? filter.ids : '');

            for (var i = 0; i < this.additionalFilters.length; i++) {
                var filters = $('[name="' + this.additionalFilters[i] + '"]');
                for (var j = 0; j < filters.length; j++) {
                    query += '&' + encodeURIComponent(this.additionalFilters[i]) + '=' + encodeURIComponent(filters[j].value);
                }
            }

            return query;
        }
    };

    return MvcLookupFilter;
}());
var MvcLookupDialog = (function () {
    function MvcLookupDialog(lookup) {
        this.lookup = lookup;
        this.filter = lookup.filter;
        this.title = lookup.control.attr('data-title');
        this.instance = $('#' + lookup.control.attr('data-dialog'));

        this.pager = this.instance.find('ul');
        this.table = this.instance.find('table');
        this.tableHead = this.instance.find('thead');
        this.tableBody = this.instance.find('tbody');
        this.error = this.instance.find('.mvc-lookup-error');
        this.search = this.instance.find('.mvc-lookup-search');
        this.rows = this.instance.find('.mvc-lookup-rows input');
        this.loader = this.instance.find('.mvc-lookup-dialog-loader');
        this.selector = this.instance.find('.mvc-lookup-selector button');

        this.initOptions();
    }

    MvcLookupDialog.prototype = {
        set: function (options) {
            options = options || {};
            $.extend(this.options.dialog, options.dialog);
            $.extend(this.options.spinner, options.spinner);
            $.extend(this.options.resizable, options.resizable);
        },
        initOptions: function () {
            var dialog = this;

            this.options = {
                dialog: {
                    classes: { 'ui-dialog': 'mvc-lookup-dialog' },
                    dialogClass: 'mvc-lookup-dialog',
                    title: dialog.title,
                    autoOpen: false,
                    minWidth: 455,
                    width: 'auto',
                    modal: true
                },
                spinner: {
                    min: 1,
                    max: 99,
                    change: function () {
                        this.value = dialog.limitRows(this.value);
                        dialog.filter.rows = this.value;
                        dialog.filter.page = 0;

                        dialog.refresh();
                    }
                },
                resizable: {
                    handles: 'w,e',
                    stop: function () {
                        $(this).css('height', 'auto');
                    }
                }
            }
        },

        open: function () {
            this.search.val(this.filter.search);
            this.error.hide().html(this.lang('error'));
            this.selected = this.lookup.selected.slice();
            this.rows.val(this.limitRows(this.filter.rows));
            this.search.attr('placeholder', this.lang('search'));
            this.selector.parent().css('display', this.lookup.multi ? '' : 'none');
            this.selector.text(this.lang('select').replace('{0}', this.lookup.selected.length));

            this.bind();
            this.refresh();

            setTimeout(function (instance) {
                var dialog = instance.dialog('open').parent();
                var visibleLeft = $(document).scrollLeft();
                var visibleTop = $(document).scrollTop();

                if (parseInt(dialog.css('left')) < visibleLeft) {
                    dialog.css('left', visibleLeft);
                }
                if (parseInt(dialog.css('top')) > visibleTop + 100) {
                    dialog.css('top', visibleTop + 100);
                }
                else if (parseInt(dialog.css('top')) < visibleTop) {
                    dialog.css('top', visibleTop);
                }
            }, 100, this.instance);
        },
        close: function () {
            this.instance.dialog('close');
        },

        refresh: function () {
            var dialog = this;
            dialog.error.fadeOut(300);
            var loading = setTimeout(function (dialog) {
                dialog.loader.fadeIn(300);
            }, 300, dialog);

            $.ajax({
                cache: false,
                url: dialog.lookup.url + dialog.filter.getQuery() + dialog.selected.map(function (x) { return '&selected=' + x.LookupIdKey; }).join(''),
                success: function (data) {
                    clearTimeout(loading);
                    dialog.render(data);
                },
                error: function () {
                    clearTimeout(loading);
                    dialog.render();
                }
            });
        },

        render: function (data) {
            this.loader.fadeOut(300);
            this.tableHead.empty();
            this.tableBody.empty();
            this.pager.empty();

            if (data) {
                this.renderHeader(data.columns);
                this.renderBody(data.columns, data.rows);
                this.renderFooter(data.filteredRows);
            } else {
                this.error.fadeIn(300);
            }
        },
        renderHeader: function (columns) {
            var tr = document.createElement('tr');
            var selection = document.createElement('th');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].hidden) {
                    tr.appendChild(this.createHeaderColumn(columns[i]));
                }
            }

            tr.appendChild(selection);
            this.tableHead.append(tr);
        },
        renderBody: function (columns, rows) {
            if (rows.length == 0) {
                var empty = this.createEmptyRow(columns);
                empty.children[0].innerHTML = this.lang('noData');
                empty.className = 'mvc-lookup-empty';

                this.tableBody.append(empty);
            }

            for (var i = 0; i < rows.length; i++) {
                var tr = this.createDataRow(rows[i]);
                var selection = document.createElement('td');

                for (var j = 0; j < columns.length; j++) {
                    if (!columns[j].hidden) {
                        var td = document.createElement('td');
                        td.className = columns[j].cssClass || '';
                        td.innerText = rows[i][columns[j].key];

                        tr.appendChild(td);
                    }
                }

                tr.appendChild(selection);
                this.tableBody.append(tr);

                if (i == this.selected.length - 1) {
                    var separator = this.createEmptyRow(columns);
                    separator.className = 'mvc-lookup-split';

                    this.tableBody.append(separator);
                }
            }
        },
        renderFooter: function (filteredRows) {
            this.totalRows = filteredRows + this.selected.length;
            var totalPages = Math.ceil(filteredRows / this.filter.rows);

            if (totalPages > 0) {
                var startingPage = Math.floor(this.filter.page / 5) * 5;

                if (totalPages > 5 && this.filter.page > 0) {
                    this.renderPage('&laquo', 0);
                    this.renderPage('&lsaquo;', this.filter.page - 1);
                }

                for (var i = startingPage; i < totalPages && i < startingPage + 5; i++) {
                    this.renderPage(i + 1, i);
                }

                if (totalPages > 5 && this.filter.page < totalPages - 1) {
                    this.renderPage('&rsaquo;', this.filter.page + 1);
                    this.renderPage('&raquo;', totalPages - 1);
                }
            } else {
                this.renderPage(1, 0);
            }
        },

        createDataRow: function (data) {
            var dialog = this;
            var lookup = this.lookup;
            var row = document.createElement('tr');
            if (lookup.indexOf(dialog.selected, data.LookupIdKey) >= 0) {
                row.className = 'selected';
            }

            $(row).on('click.mvclookup', function (e) {
                var index = lookup.indexOf(dialog.selected, data.LookupIdKey);
                if (index >= 0) {
                    dialog.selected.splice(index, 1);

                    $(this).removeClass('selected');
                } else {
                    if (lookup.multi) {
                        dialog.selected.push(data);
                    } else {
                        dialog.selected = [data];
                    }

                    $(this).addClass('selected');
                }

                if (lookup.multi) {
                    dialog.selector.text(dialog.lang('select').replace('{0}', dialog.selected.length));
                } else {
                    lookup.select(dialog.selected, false);

                    dialog.close();
                }
            });

            return row;
        },
        createEmptyRow: function (columns) {
            var row = document.createElement('tr');
            var td = document.createElement('td');
            row.appendChild(td);

            td.setAttribute('colspan', columns.length + 1);

            return row;
        },
        createHeaderColumn: function (column) {
            var header = document.createElement('th');
            header.innerText = column.header;
            var filter = this.filter;
            var dialog = this;

            if (column.cssClass) {
                header.className = column.cssClass;
            }

            if (filter.sort == column.key) {
                header.className += ' mvc-lookup-' + filter.order.toLowerCase();
            }

            $(header).on('click.mvclookup', function () {
                if (filter.sort == column.key) {
                    filter.order = filter.order == 'Asc' ? 'Desc' : 'Asc';
                } else {
                    filter.order = 'Asc';
                }

                filter.sort = column.key;
                dialog.refresh();
            });

            return header;
        },
        renderPage: function (text, value) {
            var content = document.createElement('span');
            var page = document.createElement('li');
            page.appendChild(content);
            content.innerHTML = text;
            var dialog = this;

            if (dialog.filter.page == value) {
                page.className = 'active';
            } else {
                $(content).on('click.mvclookup', function (e) {
                    var expectedPages = Math.ceil((dialog.totalRows - dialog.selected.length) / dialog.filter.rows);
                    if (value < expectedPages) {
                        dialog.filter.page = value;
                    } else {
                        dialog.filter.page = expectedPages - 1;
                    }

                    dialog.refresh();
                });
            }

            dialog.pager.append(page);
        },

        limitRows: function (value) {
            var spinner = this.options.spinner;

            return Math.min(Math.max(parseInt(value), spinner.min), spinner.max) || this.filter.rows;
        },

        lang: function (key) {
            return $.fn.mvclookup.lang[key];
        },
        bind: function () {
            var timeout;
            var dialog = this;

            dialog.instance.dialog(dialog.options.dialog);
            dialog.instance.dialog('option', 'close', function () {
                if (dialog.lookup.multi) {
                    dialog.lookup.select(dialog.selected, true);
                }
            });
            dialog.instance.parent().resizable(dialog.options.resizable);

            dialog.search.off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                if (e.keyCode < 112 || e.keyCode > 126) {
                    var input = this;
                    clearTimeout(timeout);
                    timeout = setTimeout(function () {
                        dialog.filter.search = input.value;
                        dialog.filter.page = 0;

                        dialog.refresh();
                    }, 500);
                }
            });

            dialog.rows.spinner(dialog.options.spinner);
            dialog.rows.off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                if (e.which == 13) {
                    this.blur();
                }
            });

            dialog.selector.off('click').on('click', function () {
                dialog.close();
            });
        }
    };

    return MvcLookupDialog;
}());
var MvcLookup = (function () {
    function MvcLookup(control, options) {
        this.multi = control.attr('data-multi') == 'true';
        this.filter = new MvcLookupFilter(control);
        this.for = control.attr('data-for');
        this.url = control.attr('data-url');
        this.selected = [];

        this.browse = $('.mvc-lookup-browse[data-for="' + this.for + '"]');
        this.valueContainer = $('.mvc-lookup-values[data-for="' + this.for + '"]');
        this.values = this.valueContainer.find('.mvc-lookup-value');
        this.search = control.find('.mvc-lookup-input');
        this.control = control;

        this.dialog = new MvcLookupDialog(this);
        this.initOptions();
        this.set(options);

        this.reload(false);
        this.cleanUp();
        this.bind();
    }

    MvcLookup.prototype = {
        set: function (options) {
            options = options || {};
            this.dialog.set(options);
            this.events = $.extend(this.events, options.events);
            this.search.autocomplete($.extend(this.options.autocomplete, options.autocomplete));
        },
        initOptions: function () {
            var lookup = this;

            this.options = {
                autocomplete: {
                    source: function (request, response) {
                        $.ajax({
                            url: lookup.url + lookup.filter.getQuery({ search: request.term, rows: 20 }),
                            success: function (data) {
                                response($.grep(data.rows, function (row) {
                                    return lookup.indexOf(lookup.selected, row.LookupIdKey) < 0;
                                }).map(function (row) {
                                    return {
                                        label: row.LookupAcKey,
                                        value: row.LookupAcKey,
                                        data: row
                                    };
                                }));
                            },
                            error: function () {
                                lookup.stopLoading();
                            }
                        });
                    },
                    search: function () {
                        lookup.startLoading(300);
                    },
                    response: function () {
                        lookup.stopLoading();
                    },
                    select: function (e, selection) {
                        lookup.selected.push(selection.item.data);
                        lookup.select(lookup.selected, true);
                        e.preventDefault();
                    },
                    minLength: 1,
                    delay: 500
                }
            };
        },

        reload: function (triggerChanges) {
            var lookup = this;
            var ids = $.grep(lookup.values.map(function (i, e) { return encodeURIComponent(e.value); }).get(), Boolean);

            if (ids.length > 0) {
                lookup.startLoading(300);

                $.ajax({
                    url: lookup.url + lookup.filter.getQuery({ ids: '&ids=' + ids.join('&ids='), rows: ids.length }),
                    cache: false,
                    success: function (data) {
                        lookup.stopLoading();

                        if (data.rows.length > 0) {
                            var selected = [];
                            for (var i = 0; i < data.rows.length; i++) {
                                var index = ids.indexOf(data.rows[i].LookupIdKey);
                                selected[index] = data.rows[i];
                            }

                            lookup.select(selected, triggerChanges);
                        }
                    },
                    error: function () {
                        lookup.stopLoading();
                    }
                });
            } else {
                lookup.select([], triggerChanges);
            }
        },
        select: function (data, triggerChanges) {
            if (this.events.select) {
                var e = $.Event('select.mvclookup');
                this.events.select.apply(this, [e, data, triggerChanges]);

                if (e.isDefaultPrevented()) {
                    return;
                }
            }

            this.selected = data;

            if (this.multi) {
                this.search.val('');
                this.values.remove();
                this.control.find('.mvc-lookup-item').remove();
                this.createSelectedItems(data).insertBefore(this.search);

                this.values = this.createValues(data);
                this.valueContainer.append(this.values);
                this.resizeLookupSearch();
            } else if (data.length > 0) {
                this.values.val(data[0].LookupIdKey);
                this.search.val(data[0].LookupAcKey);
            } else {
                this.values.val('');
                this.search.val('');
            }

            if (triggerChanges) {
                this.search.change().focus();
                this.values.change();
            }
        },

        createSelectedItems: function (data) {
            var items = [];

            for (var i = 0; i < data.length; i++) {
                var close = document.createElement('span');
                close.className = 'mvc-lookup-close';
                close.innerHTML = 'x';

                var item = document.createElement('div');
                item.innerText = data[i].LookupAcKey;
                item.className = 'mvc-lookup-item';
                item.appendChild(close);

                this.bindDeselect($(close), data[i].LookupIdKey);

                items[i] = item;
            }

            return $(items);
        },
        createValues: function (data) {
            var inputs = [];

            for (var i = 0; i < data.length; i++) {
                var input = document.createElement('input');
                input.className = 'mvc-lookup-value';
                input.setAttribute('type', 'hidden');
                input.setAttribute('name', this.for);
                input.value = data[i].LookupIdKey;

                inputs[i] = input;
            }

            return $(inputs);
        },

        startLoading: function (delay) {
            this.loading = setTimeout(function (lookup) {
                lookup.search.addClass('mvc-lookup-loading');
            }, delay, this);
        },
        stopLoading: function () {
            clearTimeout(this.loading);
            this.search.removeClass('mvc-lookup-loading');
        },

        bindDeselect: function (close, id) {
            var lookup = this;

            close.on('click.mvclookup', function (e) {
                lookup.selected.splice(lookup.indexOf(lookup.selected, id), 1);

                lookup.select(lookup.selected, true);
            });
        },
        indexOf: function (selection, id) {
            for (var i = 0; i < selection.length; i++) {
                if (selection[i].LookupIdKey == id) {
                    return i;
                }
            }

            return -1;
        },
        resizeLookupSearch: function () {
            var total = this.control.width();
            var lastItem = this.control.find('.mvc-lookup-item:last');

            if (lastItem.length > 0) {
                var widthLeft = Math.floor(total - lastItem.position().left - lastItem.outerWidth(true));

                if (widthLeft > total / 3) {
                    this.search.outerWidth(widthLeft, true);
                } else {
                    this.search.css('width', '');
                }
            } else {
                this.search.css('width', '');
            }
        },
        cleanUp: function () {
            this.control.removeAttr('data-filters');
            this.control.removeAttr('data-dialog');
            this.control.removeAttr('data-search');
            this.control.removeAttr('data-multi');
            this.control.removeAttr('data-order');
            this.control.removeAttr('data-title');
            this.control.removeAttr('data-page');
            this.control.removeAttr('data-rows');
            this.control.removeAttr('data-sort');
            this.control.removeAttr('data-url');
        },
        bind: function () {
            var lookup = this;

            $(window).on('resize.mvclookup', function () {
                lookup.resizeLookupSearch();
            });

            lookup.search.on('keydown.mvclookup', function (e) {
                if (e.which == 8 && this.value.length == 0 && lookup.selected.length > 0) {
                    lookup.selected.pop();

                    lookup.select(lookup.selected, true);
                }
            });
            lookup.search.on('keyup.mvclookup', function (e) {
                if (!lookup.multi && e.which != 9 && this.value.length == 0 && lookup.selected.length > 0) {
                    lookup.select([], true);
                }
            });

            lookup.browse.on('click.mvclookup', function (e) {
                lookup.dialog.open();
            });

            var filters = lookup.filter.additionalFilters;
            for (var i = 0; i < filters.length; i++) {
                $('[name="' + filters[i] + '"]').on('change.mvclookup', function (e) {
                    if (lookup.events.filterChange) {
                        lookup.events.filterChange.apply(lookup, [e]);
                    }

                    if (!e.isDefaultPrevented()) {
                        lookup.select([], true);
                    }
                });
            }
        }
    };

    return MvcLookup;
}());

$.fn.mvclookup = function (options) {
    return this.each(function () {
        if (!$.data(this, 'mvc-lookup')) {
            $.data(this, 'mvc-lookup', new MvcLookup($(this), options));
        } else if (options) {
            $.data(this, 'mvc-lookup').set(options);
        }
    });
};

$.fn.mvclookup.lang = {
    error: 'Error while retrieving records',
    noData: 'No data found',
    select: 'Select ({0})',
    search: 'Search...'
};

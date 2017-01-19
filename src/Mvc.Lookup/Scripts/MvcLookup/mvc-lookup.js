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
    function MvcLookupFilter(element) {
        this.page = element.attr('data-page');
        this.rows = element.attr('data-rows');
        this.sort = element.attr('data-sort');
        this.order = element.attr('data-order');
        this.search = element.attr('data-search');
        this.additionalFilters = element.attr('data-filters').split(',').filter(Boolean);
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
        this.instance = $('#MvcLookup');
        this.pager = this.instance.find('ul');
        this.table = this.instance.find('table');
        this.tableHead = this.instance.find('thead');
        this.tableBody = this.instance.find('tbody');
        this.error = this.instance.find('.mvc-lookup-error');
        this.search = this.instance.find('.mvc-lookup-search');
        this.loader = this.instance.find('.mvc-lookup-loading');
        this.rows = this.instance.find('.mvc-lookup-rows input');
        this.selector = this.instance.find('.mvc-lookup-selector button');

        this.instance.dialog({
            classes: { 'ui-dialog': 'mvc-lookup-dialog' },
            dialogClass: 'mvc-lookup-dialog',
            autoOpen: false,
            minHeight: 210,
            minWidth: 455,
            width: 'auto',
            modal: true
        }).parent().resizable({
            handles: 'w,e',
            stop: function (event, ui) {
                $(this).css('height', 'auto');
            }
        });
    }

    MvcLookupDialog.prototype = {
        open: function () {
            this.error.html(this.lang('error'));
            this.search.val(this.filter.search);
            this.selected = this.lookup.selected.slice();
            this.search.attr('placeholder', this.lang('search'));
            this.rows.val(this.limitTo(this.filter.rows, 1, 99));
            this.instance.dialog('option', 'title', this.lookup.title);
            this.selector.parent().css('display', this.lookup.multi ? '' : 'none');
            this.selector.text(this.lang('select').replace('{0}', this.lookup.selected.length));

            this.bind();
            this.refresh();

            setTimeout(function (instance) {
                var dialog = instance.dialog('open').parent();

                if (parseInt(dialog.css('left')) < 0) {
                    dialog.css('left', 0);
                }
                if (parseInt(dialog.css('top')) > 100) {
                    dialog.css('top', '100px');
                }
                else if (parseInt(dialog.css('top')) < 0) {
                    dialog.css('top', 0);
                }
            }, 100, this.instance);
        },
        close: function () {
            this.instance.dialog('close');
        },

        refresh: function () {
            var dialog = this;
            this.error.fadeOut(300);
            var loading = setTimeout(function (dialog) {
                dialog.loader.fadeIn(300);
                dialog.table.fadeOut(300);
                dialog.pager.fadeOut(300);
            }, 500, dialog);

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

                this.table.fadeIn(300);
                this.pager.fadeIn(300);
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

            if (!this.filter.sort && columns.length > 0) {
                tr.children[0].className += ' mvc-lookup-' + this.filter.order.toLowerCase();
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
            }
        },

        createDataRow: function (data) {
            var dialog = this;
            var lookup = this.lookup;
            var row = document.createElement('tr');
            if (dialog.indexOfSelected(data.LookupIdKey) >= 0) {
                row.className = 'selected';
            }

            $(row).on('click.mvclookup', function (e) {
                var index = dialog.indexOfSelected(data.LookupIdKey);
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
                    if (value <= expectedPages) {
                        dialog.filter.page = value - 1;
                    } else {
                        dialog.filter.page = value;
                    }

                    dialog.refresh();
                });
            }

            dialog.pager.append(page);
        },

        indexOfSelected: function (id) {
            for (var i = 0; i < this.selected.length; i++) {
                if (this.selected[i].LookupIdKey == id) {
                    return i;
                }
            }

            return -1;
        },
        limitTo: function (value, min, max) {
            return Math.min(Math.max(parseInt(value), min), max) || 20;
        },

        lang: function (key) {
            return $.fn.mvclookup.lang[key];
        },
        bind: function () {
            var timeout;
            var dialog = this;
            var filter = this.filter;

            dialog.instance.dialog('option', 'close', function () {
                if (dialog.lookup.multi) {
                    dialog.lookup.select(dialog.selected, true);
                }
            });
            dialog.selector.off('click').on('click', function () {
                dialog.close();
            });

            this.search.off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                if (e.keyCode < 112 || e.keyCode > 126) {
                    var input = this;
                    clearTimeout(timeout);
                    timeout = setTimeout(function () {
                        filter.search = input.value;
                        filter.page = 0;

                        dialog.refresh();
                    }, 500);
                }
            });

            this.rows.spinner({
                min: 1,
                max: 99,
                change: function () {
                    this.value = dialog.limitTo(this.value, 1, 99);
                    filter.rows = this.value;
                    filter.page = 0;

                    dialog.refresh();
                }
            }).off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                if (e.which == 13) {
                    this.blur();
                }
            });
        }
    };

    return MvcLookupDialog;
}());
var MvcLookup = (function () {
    function MvcLookup(element, options) {
        this.multi = element.attr('data-multi') == 'true';
        this.filter = new MvcLookupFilter(element);
        this.title = element.attr('data-title');
        this.for = element.attr('data-for');
        this.url = element.attr('data-url');
        this.selected = [];

        this.element = element;
        this.hiddenElements = $('[name="' + this.for + '"]');
        this.browse = $('.mvc-lookup-browse[data-for="' + this.for + '"]');

        this.dialog = new MvcLookupDialog(this);
        this.events = {};

        this.set(options);

        this.reload(false);
        this.cleanUp();
        this.bind();
    }

    MvcLookup.prototype = {
        set: function (options) {
            options = options || {};
            this.events.select = options.select || this.events.select;
            this.events.filterChange = options.filterChange || this.events.filterChange;
        },
        reload: function (triggerChanges) {
            var lookup = this;
            var ids = $.grep(lookup.hiddenElements.map(function (i, e) { return encodeURIComponent(e.value); }).get(), Boolean);
            if (ids.length > 0) {
                $.ajax({
                    url: lookup.url + lookup.filter.getQuery({ ids: '&ids=' + ids.join('&ids='), rows: ids.length }),
                    cache: false,
                    success: function (data) {
                        if (data.rows.length > 0) {
                            var selected = [];
                            for (var i = 0; i < data.rows.length; i++) {
                                var index = ids.indexOf(data.rows[i].LookupIdKey);
                                selected[index] = data.rows[i];
                            }

                            lookup.select(selected, triggerChanges);
                        }
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
                this.hiddenElements.remove();
            }

            if (data.length > 0) {
                if (this.multi) {
                    this.element.val(data.map(function (x) { return '"' + x.LookupAcKey + '"'; }).join(', '));
                    this.hiddenElements = this.createHiddenElements(data);
                    this.element.before(this.hiddenElements);
                } else {
                    this.hiddenElements.val(data[0].LookupIdKey);
                    this.element.val(data[0].LookupAcKey);
                }
            } else {
                this.hiddenElements.val('');
                this.element.val('');
            }

            if (triggerChanges) {
                this.hiddenElements.change();
                this.element.change();
            }
        },

        createHiddenElements: function (data) {
            var elements = [];

            for (var i = 0; i < data.length; i++) {
                var element = document.createElement('input');
                element.className = 'mvc-lookup-hidden-input';
                element.setAttribute('type', 'hidden');
                element.setAttribute('name', this.for);
                element.value = data[i].LookupIdKey;

                elements[i] = element;
            }

            return $(elements);
        },
        cleanUp: function () {
            this.element.removeAttr('data-filters');
            this.element.removeAttr('data-search');
            this.element.removeAttr('data-multi');
            this.element.removeAttr('data-order');
            this.element.removeAttr('data-title');
            this.element.removeAttr('data-page');
            this.element.removeAttr('data-rows');
            this.element.removeAttr('data-sort');
            this.element.removeAttr('data-url');
        },
        bind: function () {
            var lookup = this;

            lookup.element.autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: lookup.url + lookup.filter.getQuery({ search: request.term, rows: 20 }),
                        success: function (data) {
                            response($.map(data.rows, function (item) {
                                return {
                                    label: item.LookupAcKey,
                                    value: item.LookupAcKey,
                                    item: item
                                };
                            }));
                        }
                    });
                },
                select: function (e, selection) {
                    lookup.select([selection.item.item], true);
                    e.preventDefault();
                },
                minLength: 1,
                delay: 500
            }).on('keyup.mvclookup', function (e) {
                if (e.which != 9 && this.value.length == 0 && lookup.hiddenElements.val()) {
                    lookup.select([], true);
                }
            });

            lookup.browse.on('click.mvclookup', function (e) {
                if (lookup.element.is('[readonly]') || lookup.element.is('[disabled]')) {
                    return;
                }

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
        } else {
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

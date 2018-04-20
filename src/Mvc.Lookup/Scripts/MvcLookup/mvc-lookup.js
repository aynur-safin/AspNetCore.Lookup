/*!
 * Mvc.Lookup 2.5.0
 * https://github.com/NonFactors/MVC6.Lookup
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
var MvcLookupFilter = (function () {
    function MvcLookupFilter(group) {
        this.page = group.attr('data-page');
        this.rows = group.attr('data-rows');
        this.sort = group.attr('data-sort');
        this.order = group.attr('data-order');
        this.search = group.attr('data-search');
        this.additionalFilters = group.attr('data-filters').split(',').filter(Boolean);
    }

    MvcLookupFilter.prototype = {
        getQuery: function (search) {
            var filter = $.extend({}, this, search);
            var query = '?search=' + encodeURIComponent(filter.search) +
                '&sort=' + encodeURIComponent(filter.sort) +
                '&order=' + encodeURIComponent(filter.order) +
                '&rows=' + encodeURIComponent(filter.rows) +
                '&page=' + encodeURIComponent(filter.page) +
                (filter.checkIds || '') +
                (filter.ids || '');

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
        this.title = lookup.group.attr('data-title');
        this.instance = $('#' + lookup.group.attr('data-dialog'));

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

            dialog.options = {
                dialog: {
                    position: { my: 'center top', at: 'center top+50px', of: window, within: window },
                    classes: { 'ui-dialog': 'mvc-lookup-widget' },
                    dialogClass: 'mvc-lookup-widget',
                    title: dialog.title,
                    draggable: false,
                    autoOpen: false,
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
                    minWidth: 455,
                    handles: 'w,e',
                    stop: function () {
                        $(this).css('height', 'auto');
                    }
                }
            };
        },

        open: function () {
            var dialog = this;
            dialog.loader.hide();
            dialog.error.hide().html(dialog.lang('error'));
            dialog.selected = dialog.lookup.selected.slice();
            dialog.rows.val(dialog.limitRows(dialog.filter.rows));
            dialog.search.attr('placeholder', dialog.lang('search'));
            dialog.selector.parent().css('display', dialog.lookup.multi ? '' : 'none');
            dialog.filter.search = dialog.lookup.options.preserveSearch ? dialog.filter.search : '';
            dialog.selector.text(dialog.lang('select').replace('{0}', dialog.lookup.selected.length));

            dialog.bind();
            dialog.refresh();
            dialog.search.val(dialog.filter.search);

            setTimeout(function () {
                if (dialog.loading) {
                    dialog.loader.show();
                }

                dialog.instance.dialog('open');
            }, 100);
        },
        close: function () {
            this.instance.dialog('close');
        },

        refresh: function () {
            var dialog = this;
            dialog.loading = true;
            dialog.error.fadeOut(300);
            var loading = setTimeout(function () {
                dialog.loader.fadeIn(300);
            }, 300);

            $.ajax({
                cache: false,
                url: dialog.lookup.url + dialog.filter.getQuery() + dialog.selected.map(function (x) { return '&selected=' + x.LookupIdKey; }).join(''),
                success: function (data) {
                    dialog.loading = false;
                    clearTimeout(loading);
                    dialog.render(data);
                },
                error: function () {
                    dialog.loading = false;
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

                this.instance.parent().position({ my: 'left top', at: 'left top+50px', of: window, within: window });
                this.instance.dialog({
                    position: this.options.dialog.position || { my: 'center top', at: 'center top+50px', of: window, within: window }
                });
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

            var hasSplit = false;
            var hasSelection = rows.length && this.lookup.indexOf(this.selected, rows[0].LookupIdKey) >= 0;

            for (var i = 0; i < rows.length; i++) {
                var tr = this.createDataRow(rows[i]);
                var selection = document.createElement('td');

                for (var j = 0; j < columns.length; j++) {
                    if (!columns[j].hidden) {
                        var td = document.createElement('td');
                        td.className = columns[j].cssClass || '';
                        td.innerText = rows[i][columns[j].key] || '';

                        tr.appendChild(td);
                    }
                }

                tr.appendChild(selection);

                if (!hasSplit && hasSelection && this.lookup.indexOf(this.selected, rows[i].LookupIdKey) < 0) {
                    var separator = this.createEmptyRow(columns);
                    separator.className = 'mvc-lookup-split';
                    hasSplit = true;

                    this.tableBody.append(separator);
                }

                this.tableBody.append(tr);
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

            $(row).on('click.mvclookup', function () {
                var index = lookup.indexOf(dialog.selected, data.LookupIdKey);
                if (index >= 0) {
                    if (lookup.multi) {
                        dialog.selected.splice(index, 1);

                        $(this).removeClass('selected');
                    }
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
                    lookup.select(dialog.selected, true);

                    dialog.close();

                    lookup.search.focus();
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
            header.innerText = column.header || '';
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
            var content = document.createElement('a');
            var page = document.createElement('li');
            content.setAttribute('href', '#');
            page.appendChild(content);
            content.innerHTML = text;
            var dialog = this;

            if (dialog.filter.page == value) {
                page.className = 'active';
            } else {
                $(content).on('click.mvclookup', function (e) {
                    e.preventDefault();

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

            dialog.instance.dialog().dialog('destroy');
            dialog.instance.dialog(dialog.options.dialog);
            dialog.instance.dialog('option', 'close', function () {
                if (dialog.lookup.multi) {
                    dialog.lookup.select(dialog.selected, true);
                    dialog.lookup.search.focus();
                }
            });

            dialog.instance.parent().resizable().resizable('destroy');
            dialog.instance.parent().resizable(dialog.options.resizable);

            dialog.search.off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                var input = this;
                clearTimeout(timeout);
                timeout = setTimeout(function () {
                    if (dialog.filter.search != input.value || e.keyCode == 13) {
                        dialog.filter.search = input.value;
                        dialog.filter.page = 0;

                        dialog.refresh();
                    }
                }, 500);
            });

            dialog.rows.spinner().spinner('destroy');
            dialog.rows.spinner(dialog.options.spinner);
            dialog.rows.off('keyup.mvclookup').on('keyup.mvclookup', function (e) {
                if (e.which == 13) {
                    this.blur();
                    this.focus();
                }
            });

            dialog.selector.off('click.mvclookup').on('click.mvclookup', function () {
                dialog.close();
            });
        }
    };

    return MvcLookupDialog;
}());
var MvcLookup = (function () {
    function MvcLookup(group, options) {
        this.readonly = group.attr('data-readonly') == 'true';
        this.multi = group.attr('data-multi') == 'true';
        this.filter = new MvcLookupFilter(group);
        this.for = group.attr('data-for');
        this.url = group.attr('data-url');
        this.selected = [];

        this.group = group;
        this.search = group.find('.mvc-lookup-input');
        this.browse = group.find('.mvc-lookup-browse');
        this.control = group.find('.mvc-lookup-control');
        this.valueContainer = group.find('.mvc-lookup-values');
        this.values = this.valueContainer.find('.mvc-lookup-value');

        this.dialog = new MvcLookupDialog(this);
        this.initOptions();
        this.set(options);

        this.methods = {
            selectSingle: this.selectSingle,
            selectFirst: this.selectFirst,
            reload: this.reload,
            browse: this.open
        };
        this.reload(false);
        this.cleanUp();
        this.bind();
    }

    MvcLookup.prototype = {
        set: function (options) {
            options = options || {};

            this.options.preserveSearch = options.preserveSearch == null ? this.options.preserveSearch : options.preserveSearch;
            this.search.autocomplete($.extend(this.options.autocomplete, options.autocomplete));
            this.setReadonly(options.readonly == null ? this.readonly : options.readonly);
            this.events = $.extend(this.events, options.events);
            this.dialog.set(options);
        },
        initOptions: function () {
            var lookup = this;

            this.options = {
                preserveSearch: true,
                autocomplete: {
                    source: function (request, response) {
                        $.ajax({
                            url: lookup.url + lookup.filter.getQuery({ search: request.term, rows: 20 }),
                            success: function (data) {
                                response($.grep(data.rows, function (row) {
                                    return !lookup.multi || lookup.indexOf(lookup.selected, row.LookupIdKey) < 0;
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
                        if (lookup.multi) {
                            lookup.select(lookup.selected.concat(selection.item.data), true);
                        } else {
                            lookup.select([selection.item.data], true);
                        }

                        e.preventDefault();
                    },
                    minLength: 1,
                    delay: 500
                }
            };
        },
        setReadonly: function (readonly) {
            this.readonly = readonly;

            if (readonly) {
                this.search.autocomplete('disable').attr({ readonly: 'readonly', tabindex: -1 });
                this.group.addClass('mvc-lookup-readonly');
            } else {
                this.search.autocomplete('enable').removeAttr('readonly').removeAttr('tabindex');
                this.group.removeClass('mvc-lookup-readonly');
            }

            this.resizeLookupSearch();
        },

        open: function () {
            if (!this.readonly) {
                this.dialog.open();
            }
        },
        selectFirst: function (triggerChanges) {
            var lookup = this;
            triggerChanges = triggerChanges == null || triggerChanges;

            $.ajax({
                url: lookup.url + lookup.filter.getQuery({ rows: 1 }),
                cache: false,
                success: function (data) {
                    lookup.stopLoading();

                    lookup.select(data.rows, triggerChanges);
                },
                error: function () {
                    lookup.stopLoading();
                }
            });
        },
        selectSingle: function (triggerChanges) {
            var lookup = this;
            triggerChanges = triggerChanges == null || triggerChanges;

            $.ajax({
                url: lookup.url + lookup.filter.getQuery({ rows: 2 }),
                cache: false,
                success: function (data) {
                    lookup.stopLoading();

                    if (data.rows.length == 1) {
                        lookup.select(data.rows, triggerChanges);
                    } else {
                        lookup.select([], triggerChanges);
                    }
                },
                error: function () {
                    lookup.stopLoading();
                }
            });
        },
        reload: function (triggerChanges) {
            var lookup = this;
            triggerChanges = triggerChanges == null || triggerChanges;
            var ids = $.grep(lookup.values, function (e) { return e.value; });

            if (ids.length > 0) {
                lookup.startLoading(300);
                var encodedIds = ids.map(function (e) { return encodeURIComponent(e.value); });

                $.ajax({
                    url: lookup.url + lookup.filter.getQuery({ ids: '&ids=' + encodedIds.join('&ids='), rows: ids.length }),
                    cache: false,
                    success: function (data) {
                        lookup.stopLoading();

                        var rows = [];
                        for (var i = 0; i < ids.length; i++) {
                            var index = lookup.indexOf(data.rows, ids[i].value);
                            if (index >= 0) {
                                rows.push(data.rows[index]);
                            }
                        }

                        lookup.select(rows, triggerChanges);
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

            if (triggerChanges && data.length == this.selected.length) {
                triggerChanges = false;
                for (var i = 0; i < data.length && !triggerChanges; i++) {
                    triggerChanges = data[i].LookupIdKey != this.selected[i].LookupIdKey;
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
                this.search.change();
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
                item.innerText = data[i].LookupAcKey || '';
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
            this.stopLoading();

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

            close.on('click.mvclookup', function () {
                lookup.select(lookup.selected.filter(function (value) { return value.LookupIdKey != id; }), true);
                lookup.search.focus();
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
            this.group.removeAttr('data-readonly');
            this.group.removeAttr('data-filters');
            this.group.removeAttr('data-dialog');
            this.group.removeAttr('data-search');
            this.group.removeAttr('data-multi');
            this.group.removeAttr('data-order');
            this.group.removeAttr('data-title');
            this.group.removeAttr('data-page');
            this.group.removeAttr('data-rows');
            this.group.removeAttr('data-sort');
            this.group.removeAttr('data-url');
        },
        bind: function () {
            var lookup = this;

            $(window).on('resize.mvclookup', function () {
                lookup.resizeLookupSearch();
            });

            lookup.group.on('focusin.mvclookup', function () {
                $(this).addClass('mvc-lookup-focus');
            });

            lookup.group.on('focusout.mvclookup', function () {
                $(this).removeClass('mvc-lookup-focus');
            });

            lookup.search.on('keydown.mvclookup', function (e) {
                if (e.which == 8 && this.value.length == 0 && lookup.selected.length > 0) {
                    lookup.select(lookup.selected.slice(0, -1), true);
                }
            });
            lookup.search.on('keyup.mvclookup', function (e) {
                if (!lookup.multi && e.which != 9 && this.value.length == 0 && lookup.selected.length > 0) {
                    lookup.select([], true);
                }
            });
            lookup.search.on('blur.mvclookup', function () {
                if (!lookup.multi && lookup.selected.length) {
                    this.value = lookup.selected[0].LookupAcKey;
                } else {
                    this.value = '';
                }
            });

            lookup.browse.on('click.mvclookup', function () {
                lookup.open();
            });

            var filters = lookup.filter.additionalFilters;
            for (var i = 0; i < filters.length; i++) {
                $('[name="' + filters[i] + '"]').on('change.mvclookup', function (e) {
                    if (lookup.events.filterChange) {
                        lookup.events.filterChange.apply(lookup, [e]);
                    }

                    if (!e.isDefaultPrevented() && lookup.selected.length > 0) {
                        lookup.startLoading(300);
                        var ids = $.grep(lookup.values, function (e) { return e.value; });
                        var encodedIds = ids.map(function (e) { return encodeURIComponent(e.value); });

                        $.ajax({
                            url: lookup.url + lookup.filter.getQuery({ checkIds: '&checkIds=' + encodedIds.join('&checkIds='), rows: ids.length }),
                            cache: false,
                            success: function (data) {
                                lookup.stopLoading();

                                var rows = [];
                                for (var i = 0; i < ids.length; i++) {
                                    var index = lookup.indexOf(data.rows, ids[i].value);
                                    if (index >= 0) {
                                        rows.push(data.rows[index]);
                                    }
                                }

                                lookup.select(rows, true);
                            },
                            error: function () {
                                lookup.select([], true);
                                lookup.stopLoading();
                            }
                        });
                    }
                });
            }
        }
    };

    return MvcLookup;
}());

$.fn.mvclookup = function (options) {
    var args = arguments;

    if (options === 'instance') {
        var instances = [];

        for (var i = 0; i < this.length; i++) {
            var lookup = $(this[i]).closest('.mvc-lookup');
            if (!lookup.length)
                continue;

            var instance = lookup.data('mvc-lookup');

            if (!instance) {
                lookup.data('mvc-lookup', instance = new MvcLookup(lookup, options));
            }

            instances.push(instance);
        }

        return this.length <= 1 ? instances[0] : instances;
    }

    return this.each(function () {
        var lookup = $(this).closest('.mvc-lookup');
        if (!lookup.length)
            return;

        var instance = lookup.data('mvc-lookup');

        if (!instance) {
            if (typeof options == 'string') {
                instance = new MvcLookup(lookup);
                instance.methods[options].apply(instance, [].slice.call(args, 1));
            } else {
                instance = new MvcLookup(lookup, options);
            }

            lookup.data('mvc-lookup', instance);
        } else {
            if (typeof options == 'string') {
                instance.methods[options].apply(instance, [].slice.call(args, 1));
            } else if (options) {
                instance.set(options);
            }
        }
    });
};

$.fn.mvclookup.lang = {
    error: 'Error while retrieving records',
    noData: 'No data found',
    select: 'Select ({0})',
    search: 'Search...'
};

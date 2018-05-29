/*!
 * Mvc.Lookup 3.0.0
 * https://github.com/NonFactors/MVC6.Lookup
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
var MvcLookupFilter = (function () {
    function MvcLookupFilter(lookup) {
        var group = lookup.group;

        this.lookup = lookup;
        this.page = group.dataset.page;
        this.rows = group.dataset.rows;
        this.sort = group.dataset.sort;
        this.order = group.dataset.order;
        this.search = group.dataset.search;
        this.additionalFilters = group.dataset.filters.split(',').filter(Boolean);
    }

    MvcLookupFilter.prototype = {
        formUrl: function (search) {
            var filter = this.lookup.extend({ ids: [], checkIds: [], selected: [] }, this, search);
            var query = '?search=' + encodeURIComponent(filter.search);

            for (var i = 0; i < this.additionalFilters.length; i++) {
                var filters = document.querySelectorAll('[name="' + this.additionalFilters[i] + '"]');
                for (var j = 0; j < filters.length; j++) {
                    query += '&' + encodeURIComponent(this.additionalFilters[i]) + '=' + encodeURIComponent(filters[j].value);
                }
            }

            for (i = 0; i < filter.selected.length; i++) {
                query += '&selected=' + encodeURIComponent(filter.selected[i].LookupIdKey);
            }

            for (i = 0; i < filter.checkIds.length; i++) {
                query += '&checkIds=' + encodeURIComponent(filter.checkIds[i].value);
            }

            for (i = 0; i < filter.ids.length; i++) {
                query += '&ids=' + encodeURIComponent(filter.ids[i].value);
            }

            query += '&sort=' + encodeURIComponent(filter.sort) +
                '&order=' + encodeURIComponent(filter.order) +
                '&rows=' + encodeURIComponent(filter.rows) +
                '&page=' + encodeURIComponent(filter.page) +
                '&_=' + Date.now();

            return this.lookup.url + query;
        }
    };

    return MvcLookupFilter;
}());
var MvcLookupDialog = (function () {
    function MvcLookupDialog(lookup) {
        this.lookup = lookup;
        this.title = lookup.group.dataset.title;
        this.instance = document.getElementById(lookup.group.dataset.dialog);
        this.options = { preserveSearch: true, rows: { min: 1, max: 99 }, openDelay: 100 };

        this.overlay = new MvcLookupOverlay(this);
        this.table = this.instance.querySelector('table');
        this.tableHead = this.instance.querySelector('thead');
        this.tableBody = this.instance.querySelector('tbody');
        this.rows = this.instance.querySelector('.mvc-lookup-rows');
        this.error = this.instance.querySelector('.mvc-lookup-error');
        this.pager = this.instance.querySelector('.mvc-lookup-pager');
        this.header = this.instance.querySelector('.mvc-lookup-title');
        this.search = this.instance.querySelector('.mvc-lookup-search');
        this.selector = this.instance.querySelector('.mvc-lookup-selector');
        this.closeButton = this.instance.querySelector('.mvc-lookup-close');
        this.loader = this.instance.querySelector('.mvc-lookup-dialog-loader');
    }

    MvcLookupDialog.prototype = {
        lang: {
            search: 'Search...',
            select: 'Select ({0})',
            noData: 'No data found',
            error: 'Error while retrieving records'
        },

        open: function () {
            var dialog = this;
            var filter = dialog.lookup.filter;
            MvcLookupDialog.prototype.current = this;

            dialog.error.style.display = 'none';
            dialog.loader.style.display = 'none';
            dialog.header.innerText = dialog.title;
            dialog.error.innerHTML = dialog.lang['error'];
            dialog.selected = dialog.lookup.selected.slice();
            dialog.rows.value = dialog.limitRows(filter.rows);
            dialog.search.setAttribute('placeholder', dialog.lang['search']);
            dialog.selector.style.display = dialog.lookup.multi ? '' : 'none';
            filter.search = dialog.options.preserveSearch ? filter.search : '';
            dialog.selector.innerText = dialog.lang['select'].replace('{0}', dialog.lookup.selected.length);

            dialog.bind();
            dialog.refresh();
            dialog.search.value = filter.search;

            setTimeout(function () {
                if (dialog.loading) {
                    dialog.loader.style.opacity = 1;
                    dialog.loader.style.display = '';
                }

                dialog.overlay.show();
            }, dialog.options.openDelay);
        },
        close: function () {
            var dialog = MvcLookupDialog.prototype.current;
            dialog.overlay.hide();

            if (dialog.lookup.multi) {
                dialog.lookup.select(dialog.selected, true);
                dialog.lookup.search.focus();
            }

            MvcLookupDialog.prototype.current = null;
        },
        refresh: function () {
            var dialog = this;
            dialog.loading = true;
            dialog.error.style.opacity = 0;
            dialog.error.style.display = '';
            dialog.loader.style.display = '';
            var loading = setTimeout(function () {
                dialog.loader.style.opacity = 1;
            }, dialog.lookup.options.loadingDelay);

            dialog.lookup.load({ selected: dialog.selected }, function (data) {
                dialog.loading = false;
                clearTimeout(loading);
                dialog.render(data);
            }, function () {
                dialog.loading = false;
                clearTimeout(loading);
                dialog.render();
            });
        },

        render: function (data) {
            var dialog = this;
            dialog.pager.innerHTML = '';
            dialog.tableBody.innerHTML = '';
            dialog.tableHead.innerHTML = '';
            dialog.loader.style.opacity = 0;
            setTimeout(function () {
                dialog.loader.style.display = 'none';
            }, dialog.lookup.options.loadingDelay);

            if (data) {
                dialog.error.style.display = 'none';

                dialog.renderHeader(data.columns);
                dialog.renderBody(data.columns, data.rows);
                dialog.renderFooter(data.filteredRows);
            } else {
                dialog.error.style.opacity = 1;
            }
        },
        renderHeader: function (columns) {
            var row = document.createElement('tr');
            var selection = document.createElement('th');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].hidden) {
                    row.appendChild(this.createHeaderColumn(columns[i]));
                }
            }

            row.appendChild(selection);
            this.tableHead.appendChild(row);
        },
        renderBody: function (columns, rows) {
            if (rows.length == 0) {
                var empty = document.createElement('td');
                var row = document.createElement('tr');
                empty.innerHTML = this.lang['noData'];

                empty.setAttribute('colspan', columns.length + 1);
                row.className = 'mvc-lookup-empty';

                this.tableBody.appendChild(row);
                row.appendChild(empty);
            }

            var hasSplit = false;
            var hasSelection = rows.length && this.lookup.indexOf(this.selected, rows[0].LookupIdKey) >= 0;

            for (var i = 0; i < rows.length; i++) {
                var row = this.createDataRow(rows[i]);
                var selection = document.createElement('td');

                for (var j = 0; j < columns.length; j++) {
                    if (!columns[j].hidden) {
                        var data = document.createElement('td');
                        data.className = columns[j].cssClass || '';
                        data.innerText = rows[i][columns[j].key] || '';

                        row.appendChild(data);
                    }
                }

                row.appendChild(selection);

                if (!hasSplit && hasSelection && this.lookup.indexOf(this.selected, rows[i].LookupIdKey) < 0) {
                    var separator = document.createElement('tr');
                    separator.className = 'mvc-lookup-split';
                    var empty = document.createElement('td');
                    separator.appendChild(empty);

                    empty.setAttribute('colspan', columns.length + 1);
                    this.tableBody.appendChild(separator);

                    hasSplit = true;
                }

                this.tableBody.appendChild(row);
            }
        },
        renderFooter: function (filteredRows) {
            var dialog = this;
            var filter = dialog.lookup.filter;
            var totalPages = Math.ceil(filteredRows / filter.rows);
            dialog.totalRows = filteredRows + dialog.selected.length;

            if (totalPages > 0) {
                var startingPage = Math.floor(filter.page / 4) * 4;

                if (0 < filter.page && 4 < totalPages) {
                    dialog.renderPage('&laquo', 0);
                    dialog.renderPage('&lsaquo;', filter.page - 1);
                }

                for (var i = startingPage; i < totalPages && i < startingPage + 4; i++) {
                    dialog.renderPage(i + 1, i);
                }

                if (4 < totalPages && filter.page < totalPages - 1) {
                    dialog.renderPage('&rsaquo;', filter.page + 1);
                    dialog.renderPage('&raquo;', totalPages - 1);
                }
            } else {
                dialog.renderPage(1, 0);
            }
        },
        renderPage: function (text, value) {
            var page = document.createElement('button');
            var filter = this.lookup.filter;
            page.innerHTML = text;
            var dialog = this;

            if (filter.page == value) {
                page.className = 'active';
            }

            page.addEventListener('click', function () {
                if (filter.page != value) {
                    var expectedPages = Math.ceil((dialog.totalRows - dialog.selected.length) / filter.rows);
                    if (value < expectedPages) {
                        filter.page = value;
                    } else {
                        filter.page = expectedPages - 1;
                    }

                    dialog.refresh();
                }
            });

            dialog.pager.appendChild(page);
        },

        createHeaderColumn: function (column) {
            var header = document.createElement('th');
            header.innerText = column.header || '';
            var filter = this.lookup.filter;
            var dialog = this;

            if (column.cssClass) {
                header.className = column.cssClass;
            }

            if (filter.sort == column.key) {
                header.className += ' mvc-lookup-' + filter.order.toLowerCase();
            }

            header.addEventListener('click', function () {
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
        createDataRow: function (data) {
            var dialog = this;
            var lookup = this.lookup;
            var row = document.createElement('tr');
            if (lookup.indexOf(dialog.selected, data.LookupIdKey) >= 0) {
                row.className = 'selected';
            }

            row.addEventListener('click', function () {
                if (!window.getSelection().isCollapsed) {
                    return;
                }

                var index = lookup.indexOf(dialog.selected, data.LookupIdKey);
                if (index >= 0) {
                    if (lookup.multi) {
                        dialog.selected.splice(index, 1);

                        this.classList.remove('selected');
                    }
                } else {
                    if (lookup.multi) {
                        dialog.selected.push(data);
                    } else {
                        dialog.selected = [data];
                    }

                    this.classList.add('selected');
                }

                if (lookup.multi) {
                    dialog.selector.innerText = dialog.lang['select'].replace('{0}', dialog.selected.length);
                } else {
                    lookup.select(dialog.selected, true);

                    dialog.close();

                    lookup.search.focus();
                }
            });

            return row;
        },

        limitRows: function (value) {
            var options = this.options.rows;

            return Math.min(Math.max(parseInt(value), options.min), options.max) || this.lookup.filter.rows;
        },

        bind: function () {
            this.search.removeEventListener('keyup', this.searchChanged);
            this.closeButton.removeEventListener('click', this.close);
            this.rows.removeEventListener('change', this.rowsChanged);
            this.selector.removeEventListener('click', this.close);

            this.search.addEventListener('keyup', this.searchChanged);
            this.closeButton.addEventListener('click', this.close);
            this.rows.addEventListener('change', this.rowsChanged);
            this.selector.addEventListener('click', this.close);
        },
        rowsChanged: function () {
            var dialog = MvcLookupDialog.prototype.current;

            this.value = dialog.limitRows(this.value);

            if (dialog.lookup.filter.rows != this.value) {
                dialog.lookup.filter.rows = this.value;
                dialog.lookup.filter.page = 0;

                dialog.refresh();
            }
        },
        searchChanged: function (e) {
            var input = this;
            var dialog = MvcLookupDialog.prototype.current;

            clearTimeout(dialog.searching);
            dialog.searching = setTimeout(function () {
                if (dialog.lookup.filter.search != input.value || e.keyCode == 13) {
                    dialog.lookup.filter.search = input.value;
                    dialog.lookup.filter.page = 0;

                    dialog.refresh();
                }
            }, dialog.lookup.options.searchDelay);
        }
    };

    return MvcLookupDialog;
}());
var MvcLookupOverlay = (function () {
    function MvcLookupOverlay(dialog) {
        this.instance = this.getClosestOverlay(dialog.instance);
        this.dialog = dialog;

        this.bind();
    }

    MvcLookupOverlay.prototype = {
        getClosestOverlay: function (element) {
            var overlay = element;
            while (overlay.parentNode && !overlay.classList.contains('mvc-lookup-overlay')) {
                overlay = overlay.parentNode;
            }

            if (overlay == document) {
                throw new Error('Lookup dialog has to be inside a mvc-lookup-overlay.');
            }

            return overlay;
        },

        show: function () {
            var body = document.body.getBoundingClientRect();
            if (body.left + body.right < window.innerWidth) {
                var paddingRight = parseFloat(getComputedStyle(document.body).paddingRight);
                document.body.style.paddingRight = (paddingRight + 17) + 'px';
            }

            document.body.classList.add('mvc-lookup-open');
            this.instance.style.display = 'block';
        },
        hide: function () {
            document.body.classList.remove('mvc-lookup-open');
            document.body.style.paddingRight = '';
            this.instance.style.display = '';
        },

        bind: function () {
            this.instance.removeEventListener('click', this.onClick);
            this.instance.addEventListener('click', this.onClick);
        },
        onClick: function (e) {
            var targetClasses = (e.target || e.srcElement).classList;

            if (targetClasses.contains('mvc-lookup-overlay') || targetClasses.contains('mvc-lookup-wrapper')) {
                MvcLookupDialog.prototype.current.close();
            }
        }
    };

    return MvcLookupOverlay;
}());
var MvcLookupAutocomplete = (function () {
    function MvcLookupAutocomplete(lookup) {
        this.instance = document.createElement('ul');
        this.instance.className = 'mvc-lookup-autocomplete';
        this.options = { minLength: 1, rows: 20 };
        this.activeItem = null;
        this.lookup = lookup;
        this.items = [];
    }

    MvcLookupAutocomplete.prototype = {
        search: function (term) {
            var autocomplete = this;
            var lookup = autocomplete.lookup;

            clearTimeout(autocomplete.searching);
            autocomplete.searching = setTimeout(function () {
                if (term.length < autocomplete.options.minLength || lookup.readonly) {
                    return;
                }

                lookup.load({ search: term, rows: autocomplete.options.rows }, function (data) {
                    autocomplete.clear();

                    data = data.rows.filter(function (row) {
                        return !lookup.multi || lookup.indexOf(lookup.selected, row.LookupIdKey) < 0;
                    });

                    for (var i = 0; i < data.length; i++) {
                        var item = document.createElement('li');
                        item.dataset.id = data[i].LookupIdKey;
                        item.innerText = data[i].LookupAcKey;

                        autocomplete.instance.appendChild(item);
                        autocomplete.bind(item, data[i]);
                        autocomplete.items.push(item);
                    }

                    if (data.length) {
                        autocomplete.show();
                    } else {
                        autocomplete.hide();
                    }
                });
            }, autocomplete.lookup.options.searchDelay);
        },
        previous: function () {
            if (!this.instance.parentNode) {
                this.search(this.lookup.search.value);

                return;
            }

            if (this.activeItem) {
                this.activeItem.classList.remove('active');

                this.activeItem = this.activeItem.previousSibling || this.items[this.items.length - 1];
                this.activeItem.classList.add('active');
            } else if (this.items.length) {
                this.activeItem = this.items[this.items.length - 1];
                this.activeItem.classList.add('active');
            }
        },
        next: function () {
            if (!this.instance.parentNode) {
                this.search(this.lookup.search.value);

                return;
            }

            if (this.activeItem) {
                this.activeItem.classList.remove('active');

                this.activeItem = this.activeItem.nextSibling || this.items[0];
                this.activeItem.classList.add('active');
            } else if (this.items.length) {
                this.activeItem = this.items[0];
                this.activeItem.classList.add('active');
            }
        },
        clear: function () {
            this.items = [];
            this.activeItem = null;
            this.instance.innerHTML = '';
        },
        show: function () {
            var search = this.lookup.search.getBoundingClientRect();

            this.instance.style.left = (search.left + window.pageXOffset) + 'px';
            this.instance.style.top = (search.top + search.height + window.pageYOffset) + 'px';

            document.body.appendChild(this.instance);
        },
        hide: function () {
            this.clear();

            if (this.instance.parentNode) {
                document.body.removeChild(this.instance);
            }
        },

        bind: function (item, data) {
            var autocomplete = this;
            var lookup = autocomplete.lookup;

            item.addEventListener('mousedown', function (e) {
                e.preventDefault();
            });

            item.addEventListener('click', function (e) {
                e.preventDefault();

                if (lookup.multi) {
                    lookup.select(lookup.selected.concat(data), true);
                } else {
                    lookup.select([data], true);
                }

                autocomplete.hide();
            });

            item.addEventListener('mouseenter', function () {
                if (autocomplete.activeItem) {
                    autocomplete.activeItem.classList.remove('active');
                }

                this.classList.add('active');
                autocomplete.activeItem = this;
            });
        }
    };

    return MvcLookupAutocomplete;
}());
var MvcLookup = (function () {
    function MvcLookup(element, options) {
        var group = this.closestGroup(element);
        if (group.dataset.id) {
            return this.instances[parseInt(group.dataset.id)];
        }

        this.items = [];
        this.events = {};
        this.group = group;
        this.selected = [];
        this.for = group.dataset.for;
        this.url = group.dataset.url;
        this.multi = group.dataset.multi == 'true';
        this.group.dataset.id = this.instances.length;
        this.readonly = group.dataset.readonly == 'true';
        this.options = { searchDelay: 500, loadingDelay: 300 };

        this.search = group.querySelector('.mvc-lookup-input');
        this.browser = group.querySelector('.mvc-lookup-browser');
        this.control = group.querySelector('.mvc-lookup-control');
        this.valueContainer = group.querySelector('.mvc-lookup-values');
        this.values = this.valueContainer.querySelectorAll('.mvc-lookup-value');

        this.autocomplete = new MvcLookupAutocomplete(this);
        this.filter = new MvcLookupFilter(this);
        this.dialog = new MvcLookupDialog(this);
        this.instances.push(this);
        this.set(options || {});

        this.reload(false);
        this.cleanUp();
        this.bind();
    }

    MvcLookup.prototype = {
        instances: [],

        closestGroup: function (element) {
            var lookup = element;
            while (lookup.parentNode && !lookup.classList.contains('mvc-lookup')) {
                lookup = lookup.parentNode;
            }

            if (lookup == document) {
                throw new Error('Lookup can only be created from within mvc-lookup structure.');
            }

            return lookup;
        },

        extend: function () {
            var options = {};

            for (var i = 0; i < arguments.length; i++) {
                for (var key in arguments[i]) {
                    if (arguments[i].hasOwnProperty(key)) {
                        if (Object.prototype.toString.call(options[key]) == '[object Object]') {
                            options[key] = this.extend(options[key], arguments[i][key]);
                        } else {
                            options[key] = arguments[i][key];
                        }
                    }
                }
            }

            return options;
        },
        set: function (options) {
            this.autocomplete.options = this.extend(this.autocomplete.options, options.autocomplete);
            this.setReadonly(options.readonly == null ? this.readonly : options.readonly);
            this.dialog.options = this.extend(this.dialog.options, options.dialog);
            this.events = this.extend(this.events, options.events);
        },
        setReadonly: function (readonly) {
            this.readonly = readonly;

            if (readonly) {
                this.search.setAttribute('tabindex', -1);
                this.search.setAttribute('readonly', 'readonly');
                this.group.classList.add('mvc-lookup-readonly');

                if (this.browser) {
                    this.browser.setAttribute('tabindex', -1);
                }
            } else {
                this.search.removeAttribute('readonly');
                this.search.removeAttribute('tabindex');
                this.group.classList.remove('mvc-lookup-readonly');

                if (this.browser) {
                    this.browser.removeAttribute('tabindex');
                }
            }

            this.resizeSearch();
        },

        load: function (search, success, error) {
            var lookup = this;
            lookup.startLoading();

            var request = new XMLHttpRequest();
            request.open('GET', lookup.filter.formUrl(search), true);

            request.onload = function () {
                lookup.stopLoading();

                if (200 <= request.status && request.status < 400) {
                    success(JSON.parse(request.responseText))
                } else {
                    request.onerror();
                }
            };

            request.onerror = function () {
                lookup.stopLoading();

                if (error) {
                    error();
                }
            };

            request.send();
        },

        browse: function () {
            if (!this.readonly) {
                this.dialog.open();
            }
        },
        reload: function (triggerChanges) {
            var rows = [];
            var lookup = this;
            var ids = [].filter.call(lookup.values, function (element) { return element.value; });

            if (ids.length) {
                lookup.load({ ids: ids, rows: ids.length }, function (data) {
                    for (var i = 0; i < ids.length; i++) {
                        var index = lookup.indexOf(data.rows, ids[i].value);
                        if (index >= 0) {
                            rows.push(data.rows[index]);
                        }
                    }

                    lookup.select(rows, triggerChanges);
                });
            } else {
                lookup.select(rows, triggerChanges);
            }
        },
        select: function (data, triggerChanges) {
            var lookup = this;
            triggerChanges = triggerChanges == null || triggerChanges;

            if (lookup.events.select) {
                var e = new CustomEvent('select', { cancelable: true });

                lookup.events.select.apply(lookup, [e, data, triggerChanges]);

                if (e.defaultPrevented) {
                    return;
                }
            }

            if (triggerChanges && data.length == lookup.selected.length) {
                triggerChanges = false;
                for (var i = 0; i < data.length && !triggerChanges; i++) {
                    triggerChanges = data[i].LookupIdKey != lookup.selected[i].LookupIdKey;
                }
            }

            lookup.selected = data;

            if (lookup.multi) {
                lookup.search.value = '';
                lookup.valueContainer.innerHTML = '';;
                lookup.items.forEach(function (item) {
                    item.parentNode.removeChild(item);
                });

                lookup.items = lookup.createSelectedItems(data);
                lookup.items.forEach(function (item) {
                    lookup.control.insertBefore(item, lookup.search);
                });

                lookup.values = lookup.createValues(data);
                lookup.values.forEach(function (value) {
                    lookup.valueContainer.appendChild(value);
                });

                lookup.resizeSearch();
            } else if (data.length > 0) {
                lookup.values[0].value = data[0].LookupIdKey;
                lookup.search.value = data[0].LookupAcKey;
            } else {
                lookup.values[0].value = '';
                lookup.search.value = '';
            }

            if (triggerChanges) {
                if (typeof (Event) === 'function') {
                    var change = new Event('change');
                } else {
                    var change = document.createEvent('Event');
                    change.initEvent('change', true, true);
                }

                lookup.search.dispatchEvent(change);
                [].forEach.call(lookup.values, function (value) {
                    value.dispatchEvent(change);
                });
            }
        },
        selectFirst: function (triggerChanges) {
            var lookup = this;

            lookup.load({ rows: 1 }, function (data) {
                lookup.select(data.rows, triggerChanges);
            });
        },
        selectSingle: function (triggerChanges) {
            var lookup = this;

            lookup.load({ rows: 2 }, function (data) {
                if (data.rows.length == 1) {
                    lookup.select(data.rows, triggerChanges);
                } else {
                    lookup.select([], triggerChanges);
                }
            });
        },

        createSelectedItems: function (data) {
            var items = [];

            for (var i = 0; i < data.length; i++) {
                var close = document.createElement('button');
                close.className = 'mvc-lookup-deselect';
                close.innerText = '×';

                var item = document.createElement('div');
                item.innerText = data[i].LookupAcKey || '';
                item.className = 'mvc-lookup-item';
                item.appendChild(close);
                items.push(item);

                this.bindDeselect(close, data[i].LookupIdKey);
            }

            return items;
        },
        createValues: function (data) {
            var inputs = [];

            for (var i = 0; i < data.length; i++) {
                var input = document.createElement('input');
                input.className = 'mvc-lookup-value';
                input.setAttribute('type', 'hidden');
                input.setAttribute('name', this.for);
                input.value = data[i].LookupIdKey;

                inputs.push(input);
            }

            return inputs;
        },

        startLoading: function () {
            this.stopLoading();

            this.loading = setTimeout(function (lookup) {
                lookup.search.classList.add('mvc-lookup-loading');
            }, this.options.loadingDelay, this);
        },
        stopLoading: function () {
            clearTimeout(this.loading);
            this.search.classList.remove('mvc-lookup-loading');
        },

        bindDeselect: function (close, id) {
            var lookup = this;

            close.addEventListener('click', function () {
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
        resizeSearch: function () {
            if (this.items.length > 0) {
                var style = getComputedStyle(this.control);
                var contentWidth = this.control.clientWidth;
                var lastItem = this.items[this.items.length - 1];
                contentWidth -= parseFloat(style.paddingLeft) + parseFloat(style.paddingRight);
                var widthLeft = Math.floor(contentWidth - lastItem.offsetLeft - lastItem.offsetWidth);

                if (widthLeft > contentWidth / 3) {
                    style = getComputedStyle(this.search);
                    widthLeft -= parseFloat(style.marginLeft) + parseFloat(style.marginRight) + 4;
                    this.search.style.width = widthLeft + 'px';
                } else {
                    this.search.style.width = '';
                }
            } else {
                this.search.style.width = '';
            }
        },
        cleanUp: function () {
            delete this.group.dataset.readonly;
            delete this.group.dataset.filters;
            delete this.group.dataset.dialog;
            delete this.group.dataset.search;
            delete this.group.dataset.multi;
            delete this.group.dataset.order;
            delete this.group.dataset.title;
            delete this.group.dataset.page;
            delete this.group.dataset.rows;
            delete this.group.dataset.sort;
            delete this.group.dataset.url;
        },
        bind: function () {
            var lookup = this;

            window.addEventListener('resize', function () {
                lookup.resizeSearch();
            });

            lookup.search.addEventListener('focusin', function () {
                lookup.group.classList.add('mvc-lookup-focus');
            });

            lookup.search.addEventListener('focusout', function () {
                lookup.group.classList.remove('mvc-lookup-focus');
            });

            lookup.search.addEventListener('keydown', function (e) {
                if (e.which == 8 && this.value.length == 0 && lookup.selected.length > 0) {
                    lookup.select(lookup.selected.slice(0, -1), true);
                } else if (e.which == 38) {
                    lookup.autocomplete.previous();

                    e.preventDefault();
                } else if (e.which == 40) {
                    lookup.autocomplete.next();

                    e.preventDefault();
                } else if (e.which == 13 && lookup.autocomplete.activeItem) {
                    if (typeof (Event) === 'function') {
                        var click = new Event('click');
                    } else {
                        var click = document.createEvent('Event');
                        click.initEvent('click', true, true);
                    }

                    lookup.autocomplete.activeItem.dispatchEvent(click);

                    e.preventDefault();
                }
            });
            lookup.search.addEventListener('keyup', function (e) {
                if (e.which != 9 && this.value.length == 0 && !lookup.multi && lookup.selected.length > 0) {
                    lookup.autocomplete.hide();
                    lookup.select([], true);
                }
            });
            lookup.search.addEventListener('input', function (e) {
                lookup.autocomplete.search(this.value);
            });
            lookup.search.addEventListener('blur', function () {
                if (!lookup.multi && lookup.selected.length) {
                    this.value = lookup.selected[0].LookupAcKey;
                } else {
                    this.value = '';
                }

                lookup.autocomplete.hide();
            });

            if (lookup.browser) {
                lookup.browser.addEventListener('click', function (e) {
                    e.preventDefault();

                    lookup.browse();
                });
            }

            for (var i = 0; i < lookup.filter.additionalFilters.length; i++) {
                var inputs = document.querySelectorAll('[name="' + lookup.filter.additionalFilters[i] + '"]');

                for (var j = 0; j < inputs.length; j++) {
                    inputs[j].addEventListener('change', function (e) {
                        if (lookup.events.filterChange) {
                            lookup.events.filterChange.apply(lookup, [e]);
                        }

                        if (!e.defaultPrevented && lookup.selected.length > 0) {
                            var rows = [];
                            var ids = [].filter.call(lookup.values, function (element) { return element.value; });

                            lookup.load({ checkIds: ids, rows: ids.length }, function (data) {
                                for (var i = 0; i < ids.length; i++) {
                                    var index = lookup.indexOf(data.rows, ids[i].value);
                                    if (index >= 0) {
                                        rows.push(data.rows[index]);
                                    }
                                }

                                lookup.select(rows, true);
                            }, function () {
                                lookup.select(rows, true);
                            });
                        }
                    });
                }
            }
        }
    };

    return MvcLookup;
}());

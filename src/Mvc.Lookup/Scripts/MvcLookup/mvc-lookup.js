﻿/*!
 * Mvc.Lookup 4.0.0
 * https://github.com/NonFactors/MVC6.Lookup
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
var MvcLookupFilter = (function () {
    function MvcLookupFilter(lookup) {
        var data = lookup.group.dataset;

        this.offset = 0;
        this.lookup = lookup;
        this.sort = data.sort || '';
        this.order = data.order || '';
        this.search = data.search || '';
        this.rows = parseInt(data.rows) || 20;
        this.additional = (data.filters || '').split(',').filter(Boolean);
    }

    MvcLookupFilter.prototype = {
        formUrl: function (search) {
            var encode = encodeURIComponent;
            var url = this.lookup.url.split('?')[0];
            var urlQuery = this.lookup.url.split('?')[1];
            var filter = this.lookup.extend({ ids: [], checkIds: [], selected: [] }, this, search);
            var query = '?' + (urlQuery ? urlQuery + '&' : '') + 'search=' + encode(filter.search);

            for (var i = 0; i < filter.additional.length; i++) {
                var filters = document.querySelectorAll('[name="' + filter.additional[i] + '"]');
                for (var j = 0; j < filters.length; j++) {
                    query += '&' + encode(filter.additional[i]) + '=' + encode(filters[j].value);
                }
            }

            for (i = 0; i < filter.selected.length; i++) {
                query += '&selected=' + encode(filter.selected[i].Id);
            }

            for (i = 0; i < filter.checkIds.length; i++) {
                query += '&checkIds=' + encode(filter.checkIds[i].value);
            }

            for (i = 0; i < filter.ids.length; i++) {
                query += '&ids=' + encode(filter.ids[i].value);
            }

            query += '&sort=' + encode(filter.sort) +
                '&order=' + encode(filter.order) +
                '&offset=' + encode(filter.offset) +
                '&rows=' + encode(filter.rows) +
                '&_=' + Date.now();

            return url + query;
        }
    };

    return MvcLookupFilter;
}());
var MvcLookupDialog = (function () {
    function MvcLookupDialog(lookup) {
        var dialog = this;
        var element = document.getElementById(lookup.group.dataset.dialog || 'MvcLookupDialog');

        dialog.lookup = lookup;
        dialog.element = element;
        dialog.title = lookup.group.dataset.title || '';
        dialog.options = { preserveSearch: true, rows: { min: 1, max: 99 }, openDelay: 100 };

        dialog.overlay = new MvcLookupOverlay(this);
        dialog.table = element.querySelector('table');
        dialog.tableHead = element.querySelector('thead');
        dialog.tableBody = element.querySelector('tbody');
        dialog.rows = element.querySelector('.mvc-lookup-rows');
        dialog.header = element.querySelector('.mvc-lookup-title');
        dialog.search = element.querySelector('.mvc-lookup-search');
        dialog.footer = element.querySelector('.mvc-lookup-load-more');
        dialog.selector = element.querySelector('.mvc-lookup-selector');
        dialog.closeButton = element.querySelector('.mvc-lookup-close');
        dialog.error = element.querySelector('.mvc-lookup-dialog-error');
        dialog.loader = element.querySelector('.mvc-lookup-dialog-loader');
    }

    MvcLookupDialog.prototype = {
        open: function () {
            var dialog = this;
            var filter = dialog.lookup.filter;
            MvcLookupDialog.prototype.current = this;

            filter.offset = 0;
            filter.search = dialog.options.preserveSearch ? filter.search : '';

            dialog.error.style.display = 'none';
            dialog.loader.style.display = 'none';
            dialog.header.innerText = dialog.title;
            dialog.selected = dialog.lookup.selected.slice();
            dialog.rows.value = dialog.limitRows(filter.rows);
            dialog.error.innerHTML = dialog.lookup.lang.error;
            dialog.footer.innerText = dialog.lookup.lang.more;
            dialog.search.placeholder = dialog.lookup.lang.search;
            dialog.selector.style.display = dialog.lookup.multi ? '' : 'none';
            dialog.selector.innerText = dialog.lookup.lang.select.replace('{0}', dialog.lookup.selected.length);

            dialog.bind();
            dialog.refresh();
            dialog.search.value = filter.search;

            setTimeout(function () {
                if (dialog.isLoading) {
                    dialog.loader.style.opacity = 1;
                    dialog.loader.style.display = '';
                }

                dialog.overlay.show();
                dialog.search.focus();
            }, dialog.options.openDelay);
        },
        close: function () {
            var dialog = MvcLookupDialog.prototype.current;
            dialog.lookup.group.classList.remove('mvc-lookup-error');

            dialog.lookup.select(dialog.selected, true);
            dialog.lookup.search.focus();
            dialog.lookup.stopLoading();
            dialog.overlay.hide();

            MvcLookupDialog.prototype.current = null;
        },
        refresh: function () {
            var dialog = this;
            dialog.isLoading = true;
            dialog.error.style.opacity = 0;
            dialog.error.style.display = '';
            dialog.loader.style.display = '';
            var loading = setTimeout(function () {
                dialog.loader.style.opacity = 1;
            }, dialog.lookup.options.loadingDelay);

            dialog.lookup.startLoading({ selected: dialog.selected, rows: dialog.lookup.filter.rows + 1 }, function (data) {
                dialog.isLoading = false;
                clearTimeout(loading);
                dialog.render(data);
            }, function () {
                dialog.isLoading = false;
                clearTimeout(loading);
                dialog.render();
            });
        },

        render: function (data) {
            var dialog = this;

            if (!dialog.lookup.filter.offset) {
                dialog.tableBody.innerHTML = '';
                dialog.tableHead.innerHTML = '';
            }

            dialog.loader.style.opacity = 0;

            setTimeout(function () {
                dialog.loader.style.display = 'none';
            }, dialog.lookup.options.loadingDelay);

            if (data) {
                dialog.error.style.display = 'none';

                if (!dialog.lookup.filter.offset) {
                    dialog.renderHeader(data.columns);
                }

                dialog.renderBody(data);

                if (data.rows.length <= dialog.lookup.filter.rows) {
                    dialog.footer.style.display = 'none';
                } else {
                    dialog.footer.style.display = '';
                }
            } else {
                dialog.error.style.opacity = 1;
            }
        },
        renderHeader: function (columns) {
            var row = document.createElement('tr');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].hidden) {
                    row.appendChild(this.createHeaderCell(columns[i]));
                }
            }

            row.appendChild(document.createElement('th'));
            this.tableHead.appendChild(row);
        },
        renderBody: function (data) {
            var dialog = this;

            for (var i = 0; i < data.selected.length; i++) {
                var row = dialog.createDataRow(data.columns, data.selected[i]);
                row.className = 'selected';

                dialog.tableBody.appendChild(row);
            }

            if (data.selected.length) {
                var separator = document.createElement('tr');
                var content = document.createElement('td');

                content.colSpan = data.columns.length + 1;
                separator.className = 'mvc-lookup-split';

                dialog.tableBody.appendChild(separator);
                separator.appendChild(content);
            }

            for (i = 0; i < data.rows.length && i < dialog.lookup.filter.rows; i++) {
                dialog.tableBody.appendChild(dialog.createDataRow(data.columns, data.rows[i]));
            }

            if (!data.rows.length && !dialog.lookup.filter.offset) {
                var container = document.createElement('tr');
                var empty = document.createElement('td');

                empty.innerHTML = dialog.lookup.lang.noData;
                container.className = 'mvc-lookup-empty';
                empty.colSpan = data.columns.length + 1;

                dialog.tableBody.appendChild(container);
                container.appendChild(empty);
            }
        },

        createHeaderCell: function (column) {
            var dialog = this;
            var filter = dialog.lookup.filter;
            var header = document.createElement('th');

            if (column.cssClass) {
                header.classList.add(column.cssClass);
            }

            if (filter.sort == column.key) {
                header.classList.add('mvc-lookup-' + filter.order.toLowerCase());
            }

            header.innerText = column.header || '';
            header.addEventListener('click', function () {
                filter.order = filter.sort == column.key && filter.order == 'Asc' ? 'Desc' : 'Asc';
                filter.sort = column.key;
                filter.offset = 0;

                dialog.refresh();
            });

            return header;
        },
        createDataRow: function (columns, data) {
            var dialog = this;
            var lookup = this.lookup;
            var row = document.createElement('tr');

            for (var i = 0; i < columns.length; i++) {
                if (!columns[i].hidden) {
                    var cell = document.createElement('td');
                    cell.className = columns[i].cssClass || '';
                    cell.innerText = data[columns[i].key] || '';

                    row.appendChild(cell);
                }
            }

            row.appendChild(document.createElement('td'));

            row.addEventListener('click', function () {
                if (!window.getSelection().isCollapsed) {
                    return;
                }

                var index = lookup.indexOf(dialog.selected, data.Id);
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
                    dialog.selector.innerText = lookup.lang.select.replace('{0}', dialog.selected.length);
                } else {
                    dialog.close();
                }
            });

            return row;
        },

        limitRows: function (value) {
            value = Math.max(this.options.rows.min, Math.min(parseInt(value), this.options.rows.max));

            return isNaN(value) ? this.lookup.filter.rows : value;
        },

        bind: function () {
            var dialog = this;

            dialog.selector.addEventListener('click', dialog.close);
            dialog.footer.addEventListener('click', dialog.loadMore);
            dialog.rows.addEventListener('change', dialog.rowsChanged);
            dialog.closeButton.addEventListener('click', dialog.close);
            dialog.search.addEventListener('keyup', dialog.searchChanged);
        },
        loadMore: function () {
            var dialog = MvcLookupDialog.prototype.current;

            dialog.lookup.filter.offset += dialog.lookup.filter.rows;

            dialog.refresh();
        },
        rowsChanged: function () {
            var dialog = MvcLookupDialog.prototype.current;
            var rows = dialog.limitRows(this.value);
            this.value = rows;

            if (dialog.lookup.filter.rows != rows) {
                dialog.lookup.filter.rows = rows;
                dialog.lookup.filter.offset = 0;

                dialog.refresh();
            }
        },
        searchChanged: function (e) {
            var input = this;
            var dialog = MvcLookupDialog.prototype.current;

            dialog.lookup.stopLoading();
            clearTimeout(dialog.searching);
            dialog.searching = setTimeout(function () {
                if (dialog.lookup.filter.search != input.value || e.keyCode == 13) {
                    dialog.lookup.filter.search = input.value;
                    dialog.lookup.filter.offset = 0;

                    dialog.refresh();
                }
            }, dialog.lookup.options.searchDelay);
        }
    };

    return MvcLookupDialog;
}());
var MvcLookupOverlay = (function () {
    function MvcLookupOverlay(dialog) {
        this.element = this.findOverlay(dialog.element);
        this.dialog = dialog;

        this.bind();
    }

    MvcLookupOverlay.prototype = {
        findOverlay: function (element) {
            var overlay = element;

            if (!overlay) {
                throw new Error('Lookup dialog element was not found.');
            }

            while (overlay && !overlay.classList.contains('mvc-lookup-overlay')) {
                overlay = overlay.parentElement;
            }

            if (!overlay) {
                throw new Error('Lookup dialog has to be inside a mvc-lookup-overlay.');
            }

            return overlay;
        },

        show: function () {
            var body = document.body.getBoundingClientRect();
            if (body.left + body.right < window.innerWidth) {
                var scrollWidth = window.innerWidth - document.body.clientWidth;
                var paddingRight = parseFloat(getComputedStyle(document.body).paddingRight);

                document.body.style.paddingRight = paddingRight + scrollWidth + 'px';
            }

            document.body.classList.add('mvc-lookup-open');
            this.element.style.display = 'block';
        },
        hide: function () {
            document.body.classList.remove('mvc-lookup-open');
            document.body.style.paddingRight = '';
            this.element.style.display = '';
        },

        bind: function () {
            this.element.addEventListener('click', this.onClick);
            document.addEventListener('keydown', this.onKeyDown);
        },
        onClick: function (e) {
            var targetClasses = (e.target || e.srcElement).classList;

            if (targetClasses.contains('mvc-lookup-overlay') || targetClasses.contains('mvc-lookup-wrapper')) {
                MvcLookupDialog.prototype.current.close();
            }
        },
        onKeyDown: function (e) {
            if (e.which == 27 && MvcLookupDialog.prototype.current) {
                MvcLookupDialog.prototype.current.close();
            }
        }
    };

    return MvcLookupOverlay;
}());
var MvcLookupAutocomplete = (function () {
    function MvcLookupAutocomplete(lookup) {
        this.lookup = lookup;
        this.activeItem = null;
        this.element = document.createElement('ul');
        this.element.className = 'mvc-lookup-autocomplete';
        this.options = { minLength: 1, rows: 20, sort: lookup.filter.sort, order: lookup.filter.order };
    }

    MvcLookupAutocomplete.prototype = {
        search: function (term) {
            var autocomplete = this;
            var lookup = autocomplete.lookup;

            lookup.stopLoading();
            clearTimeout(autocomplete.searching);
            autocomplete.searching = setTimeout(function () {
                if (term.length < autocomplete.options.minLength || lookup.readonly) {
                    autocomplete.hide();

                    return;
                }

                lookup.startLoading({
                    search: term,
                    selected: lookup.multi ? lookup.selected : [],
                    sort: autocomplete.options.sort,
                    order: autocomplete.options.order,
                    offset: 0,
                    rows: autocomplete.options.rows
                }, function (data) {
                    autocomplete.hide();

                    for (var i = 0; i < data.rows.length; i++) {
                        var item = document.createElement('li');
                        item.innerText = data.rows[i].Label;
                        item.dataset.id = data.rows[i].Id;

                        autocomplete.element.appendChild(item);
                        autocomplete.bind(item, [data.rows[i]]);

                        if (i == 0) {
                            autocomplete.activeItem = item;
                            item.classList.add('active');
                        }
                    }

                    if (data.rows.length) {
                        autocomplete.show();
                    }
                });
            }, autocomplete.lookup.options.searchDelay);
        },
        previous: function () {
            if (!this.element.parentElement) {
                this.search(this.lookup.search.value);

                return;
            }

            this.activeItem.classList.remove('active');
            this.activeItem = this.activeItem.previousElementSibling || this.element.lastElementChild;
            this.activeItem.classList.add('active');
        },
        next: function () {
            if (!this.element.parentElement) {
                this.search(this.lookup.search.value);

                return;
            }

            this.activeItem.classList.remove('active');
            this.activeItem = this.activeItem.nextElementSibling || this.element.firstElementChild;
            this.activeItem.classList.add('active');
        },
        show: function () {
            var search = this.lookup.search.getBoundingClientRect();

            this.element.style.left = search.left + window.pageXOffset + 'px';
            this.element.style.top = search.top + search.height + window.pageYOffset + 'px';

            document.body.appendChild(this.element);
        },
        hide: function () {
            this.activeItem = null;
            this.element.innerHTML = '';

            if (this.element.parentElement) {
                document.body.removeChild(this.element);
            }
        },

        bind: function (item, data) {
            var autocomplete = this;
            var lookup = autocomplete.lookup;

            item.addEventListener('mousedown', function (e) {
                e.preventDefault();
            });

            item.addEventListener('click', function () {
                if (lookup.multi) {
                    lookup.select(lookup.selected.concat(data), true);
                } else {
                    lookup.select(data, true);
                }

                lookup.stopLoading();
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
        var lookup = this;
        var group = lookup.findLookup(element);
        if (group.dataset.id) {
            return lookup.instances[parseInt(group.dataset.id)].set(options || {});
        }

        lookup.items = [];
        lookup.events = {};
        lookup.group = group;
        lookup.selected = [];
        lookup.for = group.dataset.for;
        lookup.url = group.dataset.url;
        lookup.multi = group.dataset.multi == 'True';
        lookup.group.dataset.id = lookup.instances.length;
        lookup.readonly = group.dataset.readonly == 'True';
        lookup.options = { searchDelay: 300, loadingDelay: 300 };

        lookup.search = group.querySelector('.mvc-lookup-input');
        lookup.browser = group.querySelector('.mvc-lookup-browser');
        lookup.control = group.querySelector('.mvc-lookup-control');
        lookup.error = group.querySelector('.mvc-lookup-control-error');
        lookup.valueContainer = group.querySelector('.mvc-lookup-values');
        lookup.values = lookup.valueContainer.querySelectorAll('.mvc-lookup-value');

        lookup.instances.push(lookup);
        lookup.filter = new MvcLookupFilter(lookup);
        lookup.dialog = new MvcLookupDialog(lookup);
        lookup.autocomplete = new MvcLookupAutocomplete(lookup);

        lookup.set(options || {});
        lookup.reload(false);
        lookup.cleanUp();
        lookup.bind();
    }

    MvcLookup.prototype = {
        instances: [],
        lang: {
            more: 'More...',
            search: 'Search...',
            select: 'Select ({0})',
            noData: 'No data found',
            error: 'Error while retrieving records'
        },

        findLookup: function (element) {
            var lookup = element;

            if (!lookup) {
                throw new Error('Lookup element was not specified.');
            }

            while (lookup && !lookup.classList.contains('mvc-lookup')) {
                lookup = lookup.parentElement;
            }

            if (!lookup) {
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
            this.options.loadingDelay = options.loadingDelay == null ? this.options.loadingDelay : options.loadingDelay;
            this.options.searchDelay = options.searchDelay == null ? this.options.searchDelay : options.searchDelay;
            this.autocomplete.options = this.extend(this.autocomplete.options, options.autocomplete);
            this.setReadonly(options.readonly == null ? this.readonly : options.readonly);
            this.dialog.options = this.extend(this.dialog.options, options.dialog);
            this.events = this.extend(this.events, options.events);

            return this;
        },
        setReadonly: function (readonly) {
            var lookup = this;
            lookup.readonly = readonly;

            if (readonly) {
                lookup.search.tabIndex = -1;
                lookup.search.readOnly = true;
                lookup.group.classList.add('mvc-lookup-readonly');

                if (lookup.browser) {
                    lookup.browser.tabIndex = -1;
                }
            } else {
                lookup.search.removeAttribute('readonly');
                lookup.search.removeAttribute('tabindex');
                lookup.group.classList.remove('mvc-lookup-readonly');

                if (lookup.browser) {
                    lookup.browser.removeAttribute('tabindex');
                }
            }

            lookup.resize();
        },

        browse: function () {
            if (!this.readonly) {
                if (this.browser) {
                    this.browser.blur();
                }

                this.dialog.open();
            }
        },
        reload: function (triggerChanges) {
            var lookup = this;
            var originalValue = lookup.search.value;
            var ids = [].filter.call(lookup.values, function (element) {
                return element.value;
            });

            if (ids.length) {
                lookup.startLoading({ ids: ids, offset: 0, rows: ids.length }, function (data) {
                    lookup.select(data.rows, triggerChanges);
                });
            } else {
                lookup.stopLoading();
                lookup.select([], triggerChanges);

                if (!lookup.multi && lookup.search.name) {
                    lookup.search.value = originalValue;
                }
            }
        },
        select: function (data, triggerChanges) {
            var lookup = this;
            triggerChanges = triggerChanges == null || triggerChanges;

            if (lookup.events.select && lookup.events.select.call(lookup, data, triggerChanges) === false) {
                return;
            }

            if (triggerChanges && data.length == lookup.selected.length) {
                triggerChanges = false;
                for (var i = 0; i < data.length && !triggerChanges; i++) {
                    triggerChanges = data[i].Id != lookup.selected[i].Id;
                }
            }

            lookup.selected = data;

            if (lookup.multi) {
                lookup.search.value = '';
                lookup.valueContainer.innerHTML = '';
                lookup.items.forEach(function (item) {
                    item.parentElement.removeChild(item);
                });

                lookup.items = lookup.createSelectedItems(data);
                lookup.items.forEach(function (item) {
                    lookup.control.insertBefore(item, lookup.search);
                });

                lookup.values = lookup.createValues(data);
                lookup.values.forEach(function (value) {
                    lookup.valueContainer.appendChild(value);
                });

                lookup.resize();
            } else if (data.length) {
                lookup.values[0].value = data[0].Id;
                lookup.search.value = data[0].Label;
            } else {
                lookup.values[0].value = '';
                lookup.search.value = '';
            }

            if (triggerChanges) {
                var change;
                if (typeof (Event) === 'function') {
                    change = new Event('change');
                } else {
                    change = document.createEvent('Event');
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

            lookup.startLoading({ search: '', offset: 0, rows: 1 }, function (data) {
                lookup.select(data.rows, triggerChanges);
            });
        },
        selectSingle: function (triggerChanges) {
            var lookup = this;

            lookup.startLoading({ search: '', offset: 0, rows: 2 }, function (data) {
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
                var button = document.createElement('button');
                button.className = 'mvc-lookup-deselect';
                button.innerText = '×';
                button.type = 'button';

                var item = document.createElement('div');
                item.innerText = data[i].Label || '';
                item.className = 'mvc-lookup-item';
                item.appendChild(button);
                items.push(item);

                this.bindDeselect(button, data[i].Id);
            }

            return items;
        },
        createValues: function (data) {
            var inputs = [];

            for (var i = 0; i < data.length; i++) {
                var input = document.createElement('input');
                input.className = 'mvc-lookup-value';
                input.value = data[i].Id;
                input.type = 'hidden';
                input.name = this.for;

                inputs.push(input);
            }

            return inputs;
        },

        startLoading: function (search, success, error) {
            var lookup = this;

            lookup.stopLoading();
            lookup.loading = setTimeout(function () {
                lookup.autocomplete.hide();
                lookup.group.classList.add('mvc-lookup-loading');
            }, lookup.options.loadingDelay);
            lookup.group.classList.remove('mvc-lookup-error');

            lookup.request = new XMLHttpRequest();
            lookup.request.open('GET', lookup.filter.formUrl(search), true);
            lookup.request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

            lookup.request.onload = function () {
                if (200 <= lookup.request.status && lookup.request.status < 400) {
                    lookup.stopLoading();

                    success(JSON.parse(lookup.request.responseText));
                } else {
                    lookup.request.onerror();
                }
            };

            lookup.request.onerror = function () {
                lookup.group.classList.add('mvc-lookup-error');
                lookup.error.title = lookup.lang.error;
                lookup.autocomplete.hide();
                lookup.stopLoading();

                if (error) {
                    error();
                }
            };

            lookup.request.send();
        },
        stopLoading: function () {
            if (this.request && this.request.readyState != 4) {
                this.request.abort();
            }

            clearTimeout(this.loading);
            this.group.classList.remove('mvc-lookup-loading');
        },

        bindDeselect: function (close, id) {
            var lookup = this;

            close.addEventListener('click', function () {
                lookup.select(lookup.selected.filter(function (value) { return value.Id != id; }), true);
                lookup.search.focus();
            });
        },
        indexOf: function (selection, id) {
            for (var i = 0; i < selection.length; i++) {
                if (selection[i].Id == id) {
                    return i;
                }
            }

            return -1;
        },
        cleanUp: function () {
            var data = this.group.dataset;

            delete data.readonly;
            delete data.filters;
            delete data.dialog;
            delete data.search;
            delete data.multi;
            delete data.order;
            delete data.title;
            delete data.rows;
            delete data.sort;
            delete data.url;
        },
        resize: function () {
            var lookup = this;

            if (lookup.items.length) {
                var style = getComputedStyle(lookup.control);
                var contentWidth = lookup.control.clientWidth;
                var lastItem = lookup.items[lookup.items.length - 1];
                contentWidth -= parseFloat(style.paddingLeft) + parseFloat(style.paddingRight);
                var widthLeft = Math.floor(contentWidth - lastItem.offsetLeft - lastItem.offsetWidth);

                if (widthLeft > contentWidth / 3) {
                    style = getComputedStyle(lookup.search);
                    widthLeft -= parseFloat(style.marginLeft) + parseFloat(style.marginRight) + 4;
                    lookup.search.style.width = widthLeft + 'px';
                } else {
                    lookup.search.style.width = '';
                }
            } else {
                lookup.search.style.width = '';
            }
        },
        bind: function () {
            var lookup = this;

            window.addEventListener('resize', function () {
                lookup.resize();
            });

            lookup.search.addEventListener('focus', function () {
                lookup.group.classList.add('mvc-lookup-focus');
            });

            lookup.search.addEventListener('blur', function () {
                lookup.stopLoading();
                lookup.autocomplete.hide();
                lookup.group.classList.remove('mvc-lookup-focus');

                var originalValue = this.value;
                if (!lookup.multi && lookup.selected.length) {
                    if (lookup.selected[0].Label != this.value) {
                        lookup.select([], true);
                    }
                } else {
                    this.value = '';
                }

                if (!lookup.multi && lookup.search.name) {
                    this.value = originalValue;
                }
            });

            lookup.search.addEventListener('keydown', function (e) {
                if (e.which == 8 && !this.value.length && lookup.selected.length) {
                    lookup.select(lookup.selected.slice(0, -1), true);
                } else if (e.which == 38) {
                    e.preventDefault();

                    lookup.autocomplete.previous();
                } else if (e.which == 40) {
                    e.preventDefault();

                    lookup.autocomplete.next();
                } else if (e.which == 13 && lookup.autocomplete.activeItem) {
                    e.preventDefault();

                    var click;
                    if (typeof (Event) === 'function') {
                        click = new Event('click');
                    } else {
                        click = document.createEvent('Event');
                        click.initEvent('click', true, true);
                    }

                    lookup.autocomplete.activeItem.dispatchEvent(click);
                }
            });
            lookup.search.addEventListener('input', function () {
                if (!this.value.length && !lookup.multi && lookup.selected.length) {
                    lookup.autocomplete.hide();
                    lookup.select([], true);
                }

                lookup.autocomplete.search(this.value);
            });

            if (lookup.browser) {
                lookup.browser.addEventListener('click', function () {
                    lookup.browse();
                });
            }

            for (var i = 0; i < lookup.filter.additional.length; i++) {
                var inputs = document.querySelectorAll('[name="' + lookup.filter.additional[i] + '"]');

                for (var j = 0; j < inputs.length; j++) {
                    inputs[j].addEventListener('change', function () {
                        if (lookup.events.filterChange && lookup.events.filterChange.call(lookup, this) === false) {
                            return;
                        }

                        lookup.stopLoading();
                        lookup.filter.page = 0;

                        var ids = [].filter.call(lookup.values, function (element) {
                            return element.value;
                        });

                        if (ids.length || lookup.selected.length) {
                            lookup.startLoading({ checkIds: ids, offset: 0, rows: ids.length }, function (data) {
                                lookup.select(data.rows, true);
                            }, function () {
                                lookup.select([], true);
                            });
                        }
                    });
                }
            }
        }
    };

    return MvcLookup;
}());

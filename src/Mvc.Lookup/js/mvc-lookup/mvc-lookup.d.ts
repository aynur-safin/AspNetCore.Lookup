/*!
 * Mvc.Lookup 5.0.0
 * https://github.com/NonFactors/AspNetCore.Lookup
 *
 * Copyright © NonFactors
 *
 * Licensed under the terms of the MIT License
 * http://www.opensource.org/licenses/mit-license.php
 */
interface MvcLookupData {
    selected: MvcLookupDataRow[];
    columns: MvcLookupColumn[];
    rows: MvcLookupDataRow[];
}
interface MvcLookupColumn {
    key: string;
    header: string;
    hidden: boolean;
    cssClass: string;
}
interface MvcLookupDataRow {
    Id: string;
    Label: string;
    [column: string]: string | null;
}
interface MvcLookupOptions {
    readonly: boolean;
    searchDelay: number;
    loadingDelay: number;
    dialog: MvcLookupDialogOptions;
    autocomplete: MvcLookupAutocompleteOptions;
}
interface MvcLookupDialogOptions {
    preserveSearch: boolean;
    openDelay: number;
    rows: {
        min: number;
        max: number;
    };
}
interface MvcLookupAutocompleteOptions {
    minLength: number;
    rows: number;
    sort: string;
    order: "Asc" | "Desc" | "";
}
interface MvcLookupLanguage {
    more: string;
    error: string;
    search: string;
    select: string;
    noData: string;
}
declare class MvcLookupFilter {
    lookup: MvcLookup;
    search: string;
    sort: string;
    order: "Asc" | "Desc" | "";
    rows: number;
    offset: number;
    additional: string[];
    ids: HTMLInputElement[];
    checkIds: HTMLInputElement[];
    selected: MvcLookupDataRow[];
    constructor(lookup: MvcLookup);
    formUrl(search: Partial<MvcLookupFilter>): string;
}
declare class MvcLookupDialog {
    static current: MvcLookupDialog | null;
    lookup: MvcLookup;
    element: HTMLElement;
    error: HTMLDivElement;
    rows: HTMLInputElement;
    loader: HTMLDivElement;
    table: HTMLTableElement;
    header: HTMLSpanElement;
    search: HTMLInputElement;
    overlay: MvcLookupOverlay;
    footer: HTMLButtonElement;
    selector: HTMLButtonElement;
    closeButton: HTMLButtonElement;
    tableHead: HTMLTableSectionElement;
    tableBody: HTMLTableSectionElement;
    options: MvcLookupDialogOptions;
    selected: MvcLookupDataRow[];
    title: string;
    isLoading: boolean;
    searchTimerId?: number;
    constructor(lookup: MvcLookup);
    open(): void;
    close(): void;
    refresh(): void;
    private render;
    private renderHeader;
    private renderBody;
    private createHeaderCell;
    private createDataRow;
    private limitRows;
    private searchChanged;
    private rowsChanged;
    private loadMore;
    private bind;
}
declare class MvcLookupOverlay {
    element: HTMLElement;
    constructor(dialog: MvcLookupDialog);
    show(): void;
    hide(): void;
    private findOverlay;
    private onMouseDown;
    private onKeyDown;
    private bind;
}
declare class MvcLookupAutocomplete {
    lookup: MvcLookup;
    searchTimerId?: number;
    element: HTMLUListElement;
    activeItem: HTMLLIElement | null;
    options: MvcLookupAutocompleteOptions;
    constructor(lookup: MvcLookup);
    search(term: string): void;
    previous(): void;
    next(): void;
    hide(): void;
    private bind;
}
declare class MvcLookup {
    static instances: MvcLookup[];
    static lang: MvcLookupLanguage;
    url: URL;
    for: string;
    multi: boolean;
    readonly: boolean;
    loadingTimerId?: number;
    group: HTMLElement;
    error: HTMLDivElement;
    items: HTMLDivElement[];
    control: HTMLDivElement;
    dialog: MvcLookupDialog;
    filter: MvcLookupFilter;
    search: HTMLInputElement;
    options: MvcLookupOptions;
    values: HTMLInputElement[];
    controller: AbortController;
    selected: MvcLookupDataRow[];
    valueContainer: HTMLDivElement;
    browser: HTMLButtonElement | null;
    autocomplete: MvcLookupAutocomplete;
    constructor(element: HTMLElement, options?: Partial<MvcLookupOptions>);
    set(options: Partial<MvcLookupOptions>): MvcLookup;
    setReadonly(readonly: boolean): void;
    browse(): void;
    reload(triggerChanges?: boolean): void;
    select(data: MvcLookupDataRow[], triggerChanges?: boolean): void;
    selectFirst(triggerChanges?: boolean): void;
    selectSingle(triggerChanges?: boolean): void;
    startLoading(search: Partial<MvcLookupFilter>): Promise<MvcLookupData>;
    stopLoading(): void;
    private createSelectedItems;
    private createValues;
    private bindDeselect;
    private findLookup;
    private cleanUp;
    private resize;
    private bind;
}

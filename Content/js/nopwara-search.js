/*
 * Nopwara instant search (Alpine.js).
 * Attaches to the header search form rendered by the SearchBox view component:
 *   input.search-box-text, optional select.search-box-category.
 * Settings and labels come from data-* attributes on the component root (Views/PublicInfo.cshtml).
 * Must be loaded before Alpine so the alpine:init listener is registered in time.
 */
document.addEventListener('alpine:init', () => {
  Alpine.data('nopwaraSearch', () => ({
    open: false,
    loading: false,
    term: '',
    sections: [],
    seeAll: null,
    noResults: false,
    active: -1,
    status: '',
    showImages: true,
    labels: {},

    init() {
      const ds = this.$root.dataset;
      this.cfg = {
        suggestUrl: ds.suggestUrl,
        deleteUrl: ds.deleteUrl,
        minLength: parseInt(ds.minLength, 10) || 1,
        debounce: parseInt(ds.debounce, 10) || 0
      };
      this.showImages = ds.showImages === 'true';
      this.labels = {
        seeAll: ds.seeAll,
        noResults: ds.noResults,
        products: ds.titleProducts,
        categories: ds.titleCategories,
        manufacturers: ds.titleManufacturers,
        recent: ds.titleRecent,
        popular: ds.titlePopular,
        loading: ds.loading
      };

      this.form = this.$root.closest('form');
      this.input = this.form?.querySelector('.search-box-text');
      if (!this.input) return;
      this.category = this.form.querySelector('.search-box-category');

      this.cache = new Map();
      this.items = [];
      this.timer = null;
      this.request = null;

      //combobox semantics on the existing input
      const panelId = this.$refs.panel.id;
      this.input.setAttribute('role', 'combobox');
      this.input.setAttribute('aria-autocomplete', 'list');
      this.input.setAttribute('aria-controls', panelId);
      this.input.setAttribute('aria-expanded', 'false');

      this.input.addEventListener('input', () => this.schedule());
      this.input.addEventListener('focus', () => this.schedule(0));
      this.input.addEventListener('keydown', (e) => this.onKeydown(e));
      this.category?.addEventListener('change', () => this.schedule(0));
      document.addEventListener('click', (e) => {
        if (!this.form.contains(e.target)) this.close();
      });
      //tabbing out of the search form closes the panel
      this.form.addEventListener('focusout', (e) => {
        if (e.relatedTarget && !this.form.contains(e.relatedTarget)) this.close();
      });

      this.$watch('open', (value) => this.input.setAttribute('aria-expanded', String(value)));
      this.$watch('active', (value) => {
        if (value < 0) {
          this.input.removeAttribute('aria-activedescendant');
          return;
        }
        const id = 'nopwara-search-opt-' + value;
        this.input.setAttribute('aria-activedescendant', id);
        this.$nextTick(() => document.getElementById(id)?.scrollIntoView({ block: 'nearest' }));
      });
    },

    //---------- helpers used by the template

    format(text, value) {
      return (text || '').replace('{0}', value);
    },

    escape(text) {
      return String(text ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    },

    //match on the raw text and escape each piece (matching escaped text would split entities such as &amp;)
    highlight(text) {
      const raw = String(text ?? '');
      const words = this.term.split(/\s+/).filter((w) => w.length > 0).map((w) => w.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'));
      if (!words.length) return this.escape(raw);
      return raw.split(new RegExp('(' + words.join('|') + ')', 'gi'))
        .map((part, index) => (index % 2 === 1 ? '<mark>' + this.escape(part) + '</mark>' : this.escape(part)))
        .join('');
    },

    //---------- fetching

    categoryId() {
      return this.category ? parseInt(this.category.value, 10) || 0 : 0;
    },

    searchPageUrl(base, term) {
      const params = new URLSearchParams({ q: term });
      const cid = this.categoryId();
      if (cid > 0) {
        //same parameters as the core search box submit
        params.set('cid', cid);
        params.set('advs', 'true');
        params.set('isc', 'true');
      }
      return base + '?' + params.toString();
    },

    schedule(delay = this.cfg.debounce) {
      clearTimeout(this.timer);
      const term = this.input.value.trim();

      if (term.length > 0 && term.length < this.cfg.minLength) {
        this.close();
        return;
      }

      this.timer = setTimeout(() => this.fetch(term), term.length === 0 ? 0 : delay);
    },

    async fetch(term) {
      const key = term.toLowerCase() + '|' + this.categoryId();

      if (this.cache.has(key)) {
        //a request for another term / category may still be running: it must not replace these results
        this.request?.abort();
        this.loading = false;
        this.render(term, this.cache.get(key));
        return;
      }

      this.request?.abort();
      this.request = new AbortController();
      this.loading = true;

      try {
        const url = this.cfg.suggestUrl + '?' + new URLSearchParams({ q: term, cid: this.categoryId() });
        const response = await fetch(url, { signal: this.request.signal, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!response.ok) throw new Error(response.statusText);
        const data = await response.json();

        //recent searches change after every search, so only cache term results
        if (term.length > 0) this.cache.set(key, data);

        //the box or the category may have changed while waiting
        if (this.input.value.trim() === term && (term.toLowerCase() + '|' + this.categoryId()) === key) this.render(term, data);
      } catch (e) {
        if (e.name !== 'AbortError') this.close();
      } finally {
        this.loading = false;
      }
    },

    render(term, data) {
      if (data.tooShort) {
        this.close();
        return;
      }

      this.term = term;
      let i = 0;
      const sections = [];
      const add = (key, items) => {
        if (items?.length) sections.push({ key, title: this.labels[key], items: items.map((item) => ({ ...item, i: i++ })) });
      };
      const terms = (list) => (list || []).map((name) => ({ name, url: this.searchPageUrl(data.searchUrl, name) }));

      if (term.length === 0) {
        add('recent', terms(data.recent));
        add('popular', terms(data.popular));
      } else {
        add('products', data.products);
        add('categories', data.categories);
        add('manufacturers', data.manufacturers);
        add('popular', terms(data.popular));
      }

      this.noResults = term.length > 0 && !data.products?.length && !data.categories?.length && !data.manufacturers?.length;
      this.seeAll = term.length > 0 && data.totalCount > 0
        ? { url: this.searchPageUrl(data.searchUrl, term), count: data.totalCount, i: i++ }
        : null;
      this.sections = sections;

      //flat list for the keyboard
      this.items = sections.flatMap((s) => s.items);
      if (this.seeAll) this.items.push(this.seeAll);
      this.active = -1;

      this.status = this.noResults ? this.format(this.labels.noResults, term) : (data.totalCount ? this.format(this.labels.seeAll, data.totalCount) : '');
      this.open = this.items.length > 0 || this.noResults;
    },

    close() {
      //cancel a pending search too, or its results would reopen the panel
      clearTimeout(this.timer);
      this.request?.abort();
      this.loading = false;
      this.open = false;
      this.active = -1;
    },

    //---------- keyboard

    onKeydown(e) {
      if (e.key === 'Escape') {
        if (this.open) e.preventDefault();
        //also cancels a search that is still debouncing / loading
        this.close();
        return;
      }

      if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
        if (!this.open) {
          this.schedule(0);
          return;
        }
        if (!this.items.length) return;
        e.preventDefault();
        const step = e.key === 'ArrowDown' ? 1 : -1;
        const count = this.items.length;
        this.active = this.active < 0 ? (step > 0 ? 0 : count - 1) : (this.active + step + count) % count;
        return;
      }

      //Enter on a highlighted item opens it; otherwise the form submits as usual
      if (e.key === 'Enter' && this.open && this.active >= 0) {
        const item = this.items.find((x) => x.i === this.active);
        if (item?.url) {
          e.preventDefault();
          window.location.href = item.url;
        }
      }
    },

    //---------- recent searches

    async removeTerm(term) {
      const body = new FormData();
      body.append('term', term);
      const token = document.querySelector('input[name="__RequestVerificationToken"]');
      if (token) body.append('__RequestVerificationToken', token.value);

      try {
        await fetch(this.cfg.deleteUrl, { method: 'POST', body, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
      } catch (e) {
        //ignore, the list is refreshed anyway
      }

      this.input.focus();
      this.fetch('');
    }
  }));
});

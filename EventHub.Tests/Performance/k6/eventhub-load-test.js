import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { login } from './helpers/auth.js';

// Run from the repository root:
//   k6 run EventHub.Tests/Performance/k6/eventhub-load-test.js
// Set FLOW to public, client, organizer, supplier, auth, admin, or all.
// PROFILE may be smoke, heavy, stress, spike, or soak.

const BASE_URL = (__ENV.BASE_URL || 'https://staging-eventhub.tryasp.net').replace(/\/$/, '');
const ADMIN_USERNAME = __ENV.ADMIN_USERNAME || '';
const ADMIN_PASSWORD = __ENV.ADMIN_PASSWORD || '';
const CLIENT_USERNAME = __ENV.CLIENT_USERNAME || '';
const CLIENT_PASSWORD = __ENV.CLIENT_PASSWORD || '';
const ORGANIZER_USERNAME = __ENV.ORGANIZER_USERNAME || '';
const ORGANIZER_PASSWORD = __ENV.ORGANIZER_PASSWORD || '';
const SUPPLIER_USERNAME = __ENV.SUPPLIER_USERNAME || '';
const SUPPLIER_PASSWORD = __ENV.SUPPLIER_PASSWORD || '';
const EVENT_ID = __ENV.EVENT_ID || '';
const SERVICE_SEARCH_TERM = __ENV.SERVICE_SEARCH_TERM || '';
const PROFILE = (__ENV.PROFILE || 'smoke').toLowerCase();
const FLOW = (__ENV.FLOW || (ADMIN_USERNAME && ADMIN_PASSWORD ? 'all' : 'public')).toLowerCase();
const THINK_TIME_SECONDS = Number(__ENV.THINK_TIME_SECONDS || 1);

const publicPageDuration = new Trend('public_page_duration', true);
const authenticationDuration = new Trend('authentication_duration', true);
const adminPageDuration = new Trend('admin_page_duration', true);
const clientPageDuration = new Trend('client_page_duration', true);
const organizerPageDuration = new Trend('organizer_page_duration', true);
const supplierPageDuration = new Trend('supplier_page_duration', true);
const serviceCatalogDuration = new Trend('service_catalog_duration', true);
const flowFailures = new Rate('flow_failures');

const profiles = {
    smoke: [
        { duration: '1m', target: 5 },
        { duration: '1m', target: 5 },
        { duration: '1m', target: 0 },
    ],
    heavy: [
        { duration: '1m', target: 20 },
        { duration: '2m', target: 20 },
        { duration: '1m', target: 0 },
    ],
    stress: [
        { duration: '1m', target: 10 },
        { duration: '2m', target: 25 },
        { duration: '2m', target: 50 },
        { duration: '1m', target: 0 },
    ],
    spike: [
        { duration: '30s', target: 5 },
        { duration: '15s', target: 100 },
        { duration: '30s', target: 100 },
        { duration: '1m', target: 5 },
        { duration: '30s', target: 0 },
    ],
    soak: [
        { duration: '1m', target: 10 },
        { duration: '30m', target: 10 },
        { duration: '1m', target: 0 },
    ],
};

const thresholds = {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<2000'],
    checks: ['rate>0.95'],
    flow_failures: ['rate<0.01'],
};

if (FLOW === 'public' || FLOW === 'all') {
    thresholds.public_page_duration = ['p(95)<2000'];
}

if (['client', 'organizer', 'supplier', 'auth', 'admin', 'all'].includes(FLOW)
    || (FLOW === 'public' && CLIENT_USERNAME && CLIENT_PASSWORD)) {
    thresholds.authentication_duration = ['p(95)<2500'];
}

if (FLOW === 'admin' || FLOW === 'all') {
    thresholds.admin_page_duration = ['p(95)<2000'];
}

if (FLOW === 'client' || ((FLOW === 'public' || FLOW === 'all') && CLIENT_USERNAME && CLIENT_PASSWORD)) {
    thresholds.client_page_duration = ['p(95)<2000'];
    thresholds.service_catalog_duration = ['p(95)<2000'];
}

if (FLOW === 'organizer' || (FLOW === 'all' && ORGANIZER_USERNAME && ORGANIZER_PASSWORD)) {
    thresholds.organizer_page_duration = ['p(95)<2000'];
    thresholds.service_catalog_duration = ['p(95)<2000'];
}

if (FLOW === 'supplier' || (FLOW === 'all' && SUPPLIER_USERNAME && SUPPLIER_PASSWORD)) {
    thresholds.supplier_page_duration = ['p(95)<2000'];
}

export const options = {
    stages: profiles[PROFILE] || profiles.smoke,
    thresholds,
};

const englishHeaders = {
    'Accept-Language': 'en-US,en;q=0.9',
};

const adminPages = [
    { path: '/Admin/Users/Index', content: 'Users', name: 'users' },
    { path: '/Admin/Venues/Index', content: 'Venues', name: 'venues' },
    { path: '/Admin/Rooms/Index', content: 'Rooms', name: 'rooms' },
    { path: '/Admin/Events/Index', content: 'Events', name: 'events' },
    { path: '/Admin/Tickets/Index', content: 'Tickets', name: 'tickets' },
];

export function setup() {
    const validFlows = ['public', 'client', 'organizer', 'supplier', 'auth', 'admin', 'all'];
    if (!validFlows.includes(FLOW)) {
        throw new Error(`Unsupported FLOW "${FLOW}". Use public, client, organizer, supplier, auth, admin, or all.`);
    }

    if (!profiles[PROFILE]) {
        throw new Error(`Unsupported PROFILE "${PROFILE}". Use smoke, heavy, stress, spike, or soak.`);
    }

    if (['auth', 'admin', 'all'].includes(FLOW) && (!ADMIN_USERNAME || !ADMIN_PASSWORD)) {
        throw new Error('ADMIN_USERNAME and ADMIN_PASSWORD are required for authenticated Admin flows.');
    }

    validateCredentialPair('CLIENT', CLIENT_USERNAME, CLIENT_PASSWORD, FLOW === 'client');
    validateCredentialPair('ORGANIZER', ORGANIZER_USERNAME, ORGANIZER_PASSWORD, FLOW === 'organizer');
    validateCredentialPair('SUPPLIER', SUPPLIER_USERNAME, SUPPLIER_PASSWORD, FLOW === 'supplier');
}

export default function () {
    http.cookieJar().clear(BASE_URL);

    let eventId = EVENT_ID;
    if (FLOW === 'public' || FLOW === 'all') {
        eventId = browsePublicEvents(eventId);
    }

    if (FLOW === 'client' || ((FLOW === 'public' || FLOW === 'all') && CLIENT_USERNAME && CLIENT_PASSWORD)) {
        browseProtectedEventPages(eventId);
        http.cookieJar().clear(BASE_URL);
    }

    if (FLOW === 'organizer' || (FLOW === 'all' && ORGANIZER_USERNAME && ORGANIZER_PASSWORD)) {
        browseOrganizerPages();
        http.cookieJar().clear(BASE_URL);
    }

    if (FLOW === 'supplier' || (FLOW === 'all' && SUPPLIER_USERNAME && SUPPLIER_PASSWORD)) {
        browseSupplierPages();
        http.cookieJar().clear(BASE_URL);
    }

    if (FLOW === 'auth' || FLOW === 'admin' || FLOW === 'all') {
        const authenticated = authenticateAdmin();
        if (!authenticated) {
            flowFailures.add(1);
            return;
        }

        if (FLOW === 'admin' || FLOW === 'all') {
            browseAdminPages();
        }
    }

    sleep(THINK_TIME_SECONDS);
}

function browsePublicEvents(configuredEventId) {
    let discoveredEventId = configuredEventId;

    group('Public event browsing', () => {
        const home = http.get(`${BASE_URL}/`, {
            headers: englishHeaders,
            tags: { flow: 'public', page: 'home' },
        });
        publicPageDuration.add(home.timings.duration, { page: 'home' });
        recordCheck(check(home, {
            'home returns HTTP 200': (response) => response.status === 200,
            'home contains EventHub content': (response) => response.body.includes('EventHub') && response.body.includes('eh-hero'),
        }));

        const events = http.get(`${BASE_URL}/api/Events`, {
            headers: englishHeaders,
            tags: { flow: 'public', page: 'events-api' },
        });
        publicPageDuration.add(events.timings.duration, { page: 'events-api' });
        recordCheck(check(events, {
            'public events API returns HTTP 200': (response) => response.status === 200,
            'public events API returns JSON': (response) => (response.headers['Content-Type'] || '').includes('application/json'),
        }));

        if (!discoveredEventId && events.status === 200) {
            try {
                const payload = events.json();
                discoveredEventId = payload.length > 0 ? payload[0].id : '';
            } catch (_) {
                recordCheck(false);
            }
        }

    });

    return discoveredEventId;
}

function browseProtectedEventPages(eventId) {
    group('Client read-only journey', () => {
        const result = login(BASE_URL, CLIENT_USERNAME, CLIENT_PASSWORD, { flow: 'client-events' });
        authenticationDuration.add(result.response.timings.duration, { role: 'client' });
        recordCheck(check(result.response, {
            'client login succeeds': () => result.succeeded,
            'client login reaches Client area': (response) => response.url.includes('/Client/') || response.body.includes('Welcome back'),
        }));

        if (!result.succeeded) {
            return;
        }

        const dashboard = getReadOnlyPage('/Client/Home/Index', 'client', 'dashboard');
        clientPageDuration.add(dashboard.timings.duration, { page: 'dashboard' });
        recordCheck(check(dashboard, {
            'client dashboard returns HTTP 200': (response) => response.status === 200,
            'client dashboard contains expected content': (response) => response.body.includes('Welcome back') && response.body.includes('My Tickets'),
        }));

        const listing = http.get(`${BASE_URL}/Client/Events/Index`, {
            headers: englishHeaders,
            tags: { flow: 'client', page: 'event-listing' },
        });
        clientPageDuration.add(listing.timings.duration, { page: 'event-listing' });
        recordCheck(check(listing, {
            'event listing returns HTTP 200': (response) => response.status === 200,
            'event listing contains browse content': (response) => response.body.includes('Browse Events') || response.body.includes('eh-page-header'),
        }));

        const discoveredDetailsPath = listing
            .html()
            .find('a.eh-detail-btn')
            .first()
            .attr('href');
        const detailsPath = eventId
            ? `/Client/Events/Details/${encodeURIComponent(eventId)}`
            : discoveredDetailsPath;

        if (detailsPath) {
            const detailsUrl = detailsPath.startsWith('http')
                ? detailsPath
                : `${BASE_URL}${detailsPath.startsWith('/') ? '' : '/'}${detailsPath}`;
            const details = http.get(detailsUrl, {
                headers: englishHeaders,
                tags: { flow: 'client', page: 'event-details' },
            });
            clientPageDuration.add(details.timings.duration, { page: 'event-details' });
            recordCheck(check(details, {
                'event details returns HTTP 200': (response) => response.status === 200,
                'event details contains event information': (response) => response.body.includes('eh-info-label') && response.body.includes('eh-back-link'),
            }));
        }

        browseServiceCatalog('Client', 'client', clientPageDuration);

        const tickets = getReadOnlyPage('/Client/Tickets/Index', 'client', 'tickets');
        clientPageDuration.add(tickets.timings.duration, { page: 'tickets' });
        recordCheck(check(tickets, {
            'client tickets returns HTTP 200': (response) => response.status === 200,
            'client tickets contains expected content': (response) => response.body.includes('My Tickets'),
        }));

        const payments = getReadOnlyPage('/Client/Payment/History', 'client', 'payment-history');
        clientPageDuration.add(payments.timings.duration, { page: 'payment-history' });
        recordCheck(check(payments, {
            'payment history returns HTTP 200': (response) => response.status === 200,
            'payment history contains expected content': (response) => response.body.includes('Payment History'),
        }));
    });
}

function browseOrganizerPages() {
    group('Organizer read-only journey', () => {
        const result = login(BASE_URL, ORGANIZER_USERNAME, ORGANIZER_PASSWORD, { flow: 'organizer' });
        authenticationDuration.add(result.response.timings.duration, { role: 'organizer' });
        recordCheck(check(result.response, {
            'organizer login succeeds': () => result.succeeded,
        }));

        if (!result.succeeded) {
            return;
        }

        const dashboard = getReadOnlyPage('/Organizer/Home/Index', 'organizer', 'dashboard');
        organizerPageDuration.add(dashboard.timings.duration, { page: 'dashboard' });
        recordCheck(check(dashboard, {
            'organizer dashboard returns HTTP 200': (response) => response.status === 200,
            'organizer dashboard contains expected content': (response) => response.body.includes('Your Events Dashboard') && response.body.includes('My Events'),
        }));

        const events = getReadOnlyPage('/Organizer/Events/Index', 'organizer', 'events');
        organizerPageDuration.add(events.timings.duration, { page: 'events' });
        recordCheck(check(events, {
            'organizer events returns HTTP 200': (response) => response.status === 200,
            'organizer events contains expected content': (response) => response.body.includes('Events') && response.body.includes('<table'),
        }));

        browseServiceCatalog('Organizer', 'organizer', organizerPageDuration);
    });
}

function browseSupplierPages() {
    group('Supplier read-only journey', () => {
        const result = login(BASE_URL, SUPPLIER_USERNAME, SUPPLIER_PASSWORD, { flow: 'supplier' });
        authenticationDuration.add(result.response.timings.duration, { role: 'supplier' });
        recordCheck(check(result.response, {
            'supplier login succeeds': () => result.succeeded,
        }));

        if (!result.succeeded) {
            return;
        }

        const pages = [
            { path: '/Supplier/Home/Index', name: 'dashboard', content: 'Supplier Dashboard' },
            { path: '/Supplier/Services/Index', name: 'services', content: 'My Services' },
            { path: '/Supplier/Requests/Index', name: 'requests', content: 'Service Requests' },
        ];

        for (const page of pages) {
            const response = getReadOnlyPage(page.path, 'supplier', page.name);
            supplierPageDuration.add(response.timings.duration, { page: page.name });
            recordCheck(check(response, {
                [`supplier ${page.name} returns HTTP 200`]: (resultPage) => resultPage.status === 200,
                [`supplier ${page.name} contains expected content`]: (resultPage) => resultPage.body.includes(page.content),
            }));
        }
    });
}

function browseServiceCatalog(area, flow, pageMetric) {
    const basePath = `/${area}/Services/Index`;
    const catalog = getReadOnlyPage(basePath, flow, 'service-catalog');
    pageMetric.add(catalog.timings.duration, { page: 'service-catalog' });
    serviceCatalogDuration.add(catalog.timings.duration, { role: flow, filtered: 'false' });
    recordCheck(check(catalog, {
        [`${flow} service catalog returns HTTP 200`]: (response) => response.status === 200,
        [`${flow} service catalog contains expected content`]: (response) => response.body.includes('Find Services'),
    }));

    if (SERVICE_SEARCH_TERM) {
        const searchPath = `${basePath}?searchTerm=${encodeURIComponent(SERVICE_SEARCH_TERM)}`;
        const search = getReadOnlyPage(searchPath, flow, 'service-search');
        pageMetric.add(search.timings.duration, { page: 'service-search' });
        serviceCatalogDuration.add(search.timings.duration, { role: flow, filtered: 'true' });
        recordCheck(check(search, {
            [`${flow} service search returns HTTP 200`]: (response) => response.status === 200,
            [`${flow} service search contains results area`]: (response) => response.body.includes('Find Services'),
        }));
    }
}

function authenticateAdmin() {
    let succeeded = false;

    group('Admin authentication', () => {
        const result = login(BASE_URL, ADMIN_USERNAME, ADMIN_PASSWORD, { flow: 'admin' });
        authenticationDuration.add(result.response.timings.duration);
        succeeded = result.succeeded;
        recordCheck(check(result.response, {
            'admin login succeeds': () => result.succeeded,
            'admin login reaches dashboard': (response) => response.url.includes('/Admin/Home/Index') || response.body.includes('Welcome back'),
        }));
    });

    return succeeded;
}

function browseAdminPages() {
    group('Admin dashboard smoke load', () => {
        const dashboard = http.get(`${BASE_URL}/Admin/Home/Index`, {
            headers: englishHeaders,
            tags: { flow: 'admin', page: 'dashboard' },
        });
        adminPageDuration.add(dashboard.timings.duration, { page: 'dashboard' });
        recordCheck(check(dashboard, {
            'admin dashboard returns HTTP 200': (response) => response.status === 200,
            'admin dashboard contains key widgets': (response) => response.body.includes('Welcome back') && response.body.includes('Total Users') && response.body.includes('Total Events'),
        }));
    });

    group('Admin read-only pages', () => {
        for (const page of adminPages) {
            const response = http.get(`${BASE_URL}${page.path}`, {
                headers: englishHeaders,
                tags: { flow: 'admin', page: page.name },
            });
            adminPageDuration.add(response.timings.duration, { page: page.name });
            recordCheck(check(response, {
                [`admin ${page.name} page returns HTTP 200`]: (result) => result.status === 200,
                [`admin ${page.name} page contains expected content`]: (result) => result.body.includes(page.content),
            }));
        }
    });
}

function recordCheck(passed) {
    flowFailures.add(passed ? 0 : 1);
}

function getReadOnlyPage(path, flow, page) {
    return http.get(`${BASE_URL}${path}`, {
        headers: englishHeaders,
        tags: { flow, page },
    });
}

function validateCredentialPair(name, username, password, required) {
    if (required && (!username || !password)) {
        throw new Error(`${name}_USERNAME and ${name}_PASSWORD are required for the ${name.toLowerCase()} flow.`);
    }

    if (Boolean(username) !== Boolean(password)) {
        throw new Error(`${name}_USERNAME and ${name}_PASSWORD must be supplied together.`);
    }
}

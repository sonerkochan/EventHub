import http from 'k6/http';
import { check } from 'k6';

const REQUEST_HEADERS = {
    'Accept-Language': 'en-US,en;q=0.9',
};

export function login(baseUrl, username, password, requestTags = {}) {
    const loginPage = http.get(`${baseUrl}/User/Login`, {
        headers: REQUEST_HEADERS,
        tags: { page: 'login', ...requestTags },
    });

    const token = loginPage
        .html()
        .find('input[name="__RequestVerificationToken"]')
        .first()
        .attr('value');

    const loginPageReady = check(loginPage, {
        'login page returns HTTP 200': (response) => response.status === 200,
        'login page contains username field': (response) => response.body.includes('name="Username"'),
        'login page contains anti-forgery token': () => Boolean(token),
    });

    if (!loginPageReady || !token) {
        return { succeeded: false, response: loginPage };
    }

    const response = http.post(
        `${baseUrl}/User/Login`,
        {
            Username: username,
            Password: password,
            __RequestVerificationToken: token,
        },
        {
            headers: {
                ...REQUEST_HEADERS,
                Referer: `${baseUrl}/User/Login`,
            },
            redirects: 10,
            tags: { page: 'login-submit', ...requestTags },
        },
    );

    return {
        succeeded: response.status === 200 && !response.body.includes('name="Username"'),
        response,
    };
}

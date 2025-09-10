import axios from 'axios';
import camelcaseKeys from 'camelcase-keys';


const api = axios.create();


let getToken: (() => string | undefined) | null = null;

export function setTokenGetter(fn: () => string | undefined)
{
    getToken = fn;
}

api.interceptors.request.use(async (config) =>
{
    if (getToken)
    {
        const token = getToken();
        if (token && config.headers)
        {
            config.headers['Authorization'] = `Bearer ${token}`; // attaching authorization token to every request
        }
    }
    return config;
});

// intercepting 401/403 responses
api.interceptors.response.use(
    response => response,
    async error =>
    {
        const { response } = error;
        if (!response) throw error;

        // On persistent 401/403, redirect to login
        if (response.status === 401 || response.status === 403)
        {
            console.warn('Unauthorized, redirecting to login...');
            window.location.href = '/';
        }

        return Promise.reject(error);
    }
);
api.interceptors.response.use(
    (response) =>
    {
        if (response.data && typeof response.data === 'object')
        {
            response.data = camelcaseKeys(response.data, { deep: true });
        }
        return response;
    },
    (error) => Promise.reject(error)
);

export default api;
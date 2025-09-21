import axios from 'axios';
import camelcaseKeys from 'camelcase-keys';
import toast from 'react-hot-toast';
import i18n from './utils/18n'; // import your i18n instance


const api = axios.create();


let getToken: (() => string | undefined) | null = null;
export { getToken };
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
    // Accept-Language header
    if (config.headers)
    {
        config.headers['Accept-Language'] = i18n.language || 'en';
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

        if (response.status === 401)
        {
            const msg = 'Your session has expired. Please log in again.';
            (error as any).userMessage = msg;
            toast.error(msg);
            window.location.href = '/';
        }
        else if (response.status === 403)
        {
            const data = response.data;
            const msg =
                (data && (data.detail || data.title || data.message)) ||
                'You do not have permission to perform this action.';
            (error as any).userMessage = msg;
            toast.error(msg);
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
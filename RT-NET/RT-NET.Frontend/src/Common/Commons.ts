const API_BASE_URL = 'https://rt-net.azurewebsites.net';

export const API_ROUTES = {
    MESSAGES: {
        BASE: `${API_BASE_URL}/messages`,
        BY_ID: (id) => `${API_BASE_URL}/messages/${id}`,
    }
};

export default API_ROUTES;

export {API_BASE_URL};
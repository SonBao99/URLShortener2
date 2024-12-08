import axios from 'axios';

const API_URL = 'https://localhost:7214/api/auth';

const authService = {
    async login(email, password) {
        const response = await axios.post(`${API_URL}/login`, {
            email,
            password
        });
        if (response.data.token) {
            localStorage.setItem('user', JSON.stringify(response.data));
        }
        return response.data;
    },

    async register(username, email, password, fullName) {
        return axios.post(`${API_URL}/register`, {
            username,
            email,
            password,
            fullName
        });
    },

    logout() {
        localStorage.removeItem('user');
    },

    getCurrentUser() {
        return JSON.parse(localStorage.getItem('user'));
    }
};

export default authService;
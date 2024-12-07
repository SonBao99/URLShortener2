// src/axios.js
import axios from 'axios';

const instance = axios.create({
    baseURL: 'https://localhost:7162/api/Url', // Update to your API base URL
    headers: {
        'Content-Type': 'application/json',
    },
});

export default instance;

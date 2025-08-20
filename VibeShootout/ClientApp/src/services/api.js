import axios from 'axios';

const API_BASE_URL = 'http://localhost:5632/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const getConfig = async () => {
  const response = await api.get('/config');
  return response.data;
};

export const saveConfig = async (config) => {
  const response = await api.post('/config', config);
  return response.data;
};
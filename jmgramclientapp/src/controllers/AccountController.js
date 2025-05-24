 import axios from 'axios';
 
 const API_BASE_URL = 'https://localhost:5087'; //  Определите базовый URL
 
 export const register = async (firstName, lastName, phone, password) => {
  try {
   const response = await axios.post(`${API_BASE_URL}/Account/Register`, {
    firstName,
    lastName,
    phone,
    password
   }, { withCredentials: true });
   return response.data;
  } catch (error) {
   throw error;
  }
 };
 
// controllers/AccountController.js
export const login = async (phone, password) => {
  try {
    const response = await fetch('https://localhost:5087/Account/Login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ phone, password }),
      credentials: 'include',
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Login failed: ${response.status} - ${errorText}`);
    }

    // Добавим логирование тела ответа
    console.log('Login response body:', response);

    // Просто возвращаем `true` (если login действительно ничего не возвращает)
    return true;

  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
};
 
 export const logout = async () => {
  try {
   const response = await axios.post(`${API_BASE_URL}/Account/Logout`, null, { withCredentials: true });
   return response.data;
  } catch (error) {
   throw error;
  }
 };
 
 export const isAuthenticated = async () => {
  try {
   const response = await axios.post(`${API_BASE_URL}/Account/Authenticated`, null, { withCredentials: true });
   return response.status === 200;
  } catch (error) {
   return false;
  }
 };
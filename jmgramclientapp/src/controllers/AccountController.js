 import axios from 'axios';
 
 const API_BASE_URL = 'http://127.0.0.1:5087'; //  Определите базовый URL
 
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
 
 export const login = async (phone, password) => {
  try {
   const response = await axios.post(`${API_BASE_URL}/Account/Login`, {
    phone,
    password
   }, { withCredentials: true });
   return response.data;
  } catch (error) {
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
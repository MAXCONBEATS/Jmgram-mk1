 import axios from 'axios';
  axios.defaults.baseURL = 'https://localhost:5087';
 
 export const isAuthenticated = async () => {
  try {
   const response = await axios.post('https://localhost:5087/Account/Authenticated', null, { withCredentials: true });
   return response.status === 200; // Или response.data === true, в зависимости от вашего API
  } catch (error) {
   return false;
  }
 };
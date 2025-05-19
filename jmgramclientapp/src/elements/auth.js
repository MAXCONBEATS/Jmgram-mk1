 import axios from 'axios';
 
 export const isAuthenticated = async () => {
  try {
   const response = await axios.post('http://127.0.0.1:5087/Account/Authenticated', null, { withCredentials: true });
   return response.status === 200; // Или response.data === true, в зависимости от вашего API
  } catch (error) {
   return false;
  }
 };
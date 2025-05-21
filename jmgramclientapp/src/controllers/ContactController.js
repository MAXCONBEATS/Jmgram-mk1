 // src/controllers/ContactController.js
 import axios from 'axios';
  axios.defaults.baseURL = 'https://localhost:5087';
 
  // ContactController.js
 export async function getContactList() {
  try {
   const response = await axios.get(`https://localhost:5087/Contact/List`, { withCredentials: true });
   return response.data;
  } catch (error) {
   console.error('Ошибка при получении списка контактов:', error);
   throw error;
  }
 }
 
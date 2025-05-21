 // src/controllers/ContactController.js
 import axios from 'axios';
 
  // ContactController.js
 export async function getContactList() {
  try {
   const response = await axios.get(`http://localhost:5087/Contact/List`, { withCredentials: true });
   return response.data;
  } catch (error) {
   console.error('Ошибка при получении списка контактов:', error);
   throw error;
  }
 }
 
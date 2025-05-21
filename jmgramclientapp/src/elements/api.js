 import axios from 'axios';
 
 export const getUserChats = async () => {
  try {
   const response = await axios.get('https://localhost:5087/Chat/UserChats', { withCredentials: true });
   return response.data;
  } catch (error) {
   console.error('Ошибка при получении UserChats:', error);
   throw error; //  Пробросьте ошибку, чтобы ее можно было обработать в компоненте
  }
 };
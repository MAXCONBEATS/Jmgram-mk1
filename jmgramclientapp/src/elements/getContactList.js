import axios from 'axios';

// Функция для получения списка контактов
const getContactList = async () => {
    try {
        const response = await axios.get('/Contact/List'); // Замените на правильный URL API
        return response.data; // Возвращаем данные из ответа
    } catch (error) {
        console.error('Ошибка при получении списка контактов:', error);
        throw error; // Пробрасываем ошибку, чтобы ее можно было обработать в компоненте
    }
};

export default getContactList;
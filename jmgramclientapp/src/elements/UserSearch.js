import React, { useState } from 'react';
import axios from 'axios';
import '../css/UserSearch.css';

axios.defaults.baseURL = 'https://localhost:5087';
axios.defaults.withCredentials = true; // Добавляем withCredentials: true здесь

function UserSearch() {
    const [phoneNumber, setPhoneNumber] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [error, setError] = useState('');

    const handleSearch = async () => {
        try {
            const response = await axios.get(`/User/Search?phone=${encodeURIComponent(phoneNumber)}`);
            console.log('API response:', response.data); // Проверяем структуру ответа

            // Оборачиваем объект пользователя в массив
            setSearchResults([response.data]);
            setError('');
        } catch (error) {
            console.error('Ошибка при поиске пользователя:', error);
            setSearchResults([]);
            setError('Пользователь не найден');
        }
    };

    const handleAddContact = async (contactUserId) => {
    try {
        console.log('Sending contactUserId:', contactUserId); // Проверяем, что contactUserId имеет правильное значение
        await axios.post('/Contact/Add', { contactUserId });
        alert('Запрос на добавление в контакты отправлен');
    } catch (error) {
        console.error('Ошибка при отправке запроса:', error);
        if (error.response && error.response.status === 401) {
            setError('Вы не авторизованы. Пожалуйста, войдите в систему.');
        } else {
            setError('Не удалось отправить запрос на добавление в контакты');
        }
    }
};

    return (
        <div className="user-search">
            <input
                type="tel"
                placeholder="Номер телефона"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
            />
            <button onClick={handleSearch} className="btn btn-primary"><i className="bi bi-search"></i></button>

            {error && <p className="error-message">{error}</p>}

            {searchResults.length > 0 && (
                <div className="search-results-wrapper">
                  <ul>
                      {searchResults.map((user) => (
                        <li key={user.id}>
                            <div>{user.firstName} {user.lastName}</div>
                            <div>{user.phone}</div>
                            <button onClick={() => handleAddContact(user.id)}>Добавить в контакты</button>
                        </li>
                      ))}
                  </ul>
                </div>
            )}
        </div>
    );
}

export default UserSearch;
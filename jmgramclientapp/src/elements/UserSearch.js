import React, { useState } from 'react';
import axios from 'axios';
import { addContact } from '../controllers/ContactController';
import '../css/UserSearch.css';

function UserSearch() {
    const [phoneNumber, setPhoneNumber] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [loading, setLoading] = useState(false); // Состояние для индикатора загрузки
    const [error, setError] = useState('');
    const [searchPerformed, setSearchPerformed] = useState(false); // Отслеживание, был ли выполнен поиск

    const handleSearch = async () => {
        if (!phoneNumber.trim()) { // Проверяем, что номер не пустой
            setError('Пожалуйста, введите номер телефона для поиска.');
            setSearchResults([]);
            setSearchPerformed(false);
            return;
        }

        setLoading(true);
        setError('');
        setSearchResults([]); // Очищаем предыдущие результаты
        setSearchPerformed(true); // Отмечаем, что поиск был инициирован

        try {
            const response = await axios.get(`/User/Search?phone=${encodeURIComponent(phoneNumber.trim())}`);
            // Проверяем, что response.data содержит реальные данные пользователя
            if (response.data && response.data.id) {
                setSearchResults([response.data]);
            } else {
                // Если API вернул 200 OK, но данных нет или они невалидны
                setError('Пользователь не найден.');
            }
        } catch (error) {
            console.error('Ошибка при поиске пользователя:', error);
            setSearchResults([]);
            if (error.response) {
                if (error.response.status === 404) {
                    setError('Пользователь с таким номером не найден.');
                } else if (error.response.data && error.response.data.message) {
                    setError(error.response.data.message);
                } else {
                    setError('Произошла ошибка при поиске пользователя.');
                }
            } else {
                setError('Не удалось подключиться к серверу. Проверьте ваше интернет-соединение.');
            }
        } finally {
            setLoading(false);
        }
    };

    const handleAddContact = async (contactUserId) => {
        try {
            await addContact(contactUserId, localStorage.getItem('UserId'));
            alert('Запрос на добавление в контакты отправлен');
            // Возможно, здесь можно убрать пользователя из списка или изменить его статус
            // Например, filter(user => user.id !== contactUserId) или обновить user.status
        } catch (error) {
            console.error('Ошибка при отправке запроса:', error);
            if (error.response && error.response.status === 401) {
                setError('Вы не авторизованы. Пожалуйста, войдите в систему.');
            } else if (error.response && error.response.data && error.response.data.message) {
                setError(error.response.data.message);
            }
            else {
                setError('Не удалось отправить запрос на добавление в контакты.');
            }
        }
    };

    return (
            <div className="user-search-card">
                <div className="search-input-group">
                    <input
                        type="tel"
                        placeholder="Введите номер телефона..."
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                        onKeyPress={(e) => { // Поиск при нажатии Enter
                            if (e.key === 'Enter') {
                                handleSearch();
                            }
                        }}
                    />
                    <button onClick={handleSearch} className="search-button" disabled={loading}>
                        {loading ? <span className="spinner"></span> : <i className="bi bi-search"></i>}
                    </button>
                </div>

                {error && <p className="error-message">{error}</p>}
                {loading && <p className="loading-message">Поиск...</p>}

                {searchPerformed && !loading && !error && searchResults.length === 0 && (
                     // Показываем это сообщение, если поиск был выполнен, не загружается, нет ошибки и нет результатов
                    <p className="no-results-message">Пользователь не найден.</p>
                )}

                {searchResults.length > 0 && (
                    <div className="search-results-section"> {/* Секция для прокручиваемых результатов */}
                        <ul className="search-results-list">
                            {searchResults.map((user) => (
                                <li key={user.id} className="user-result-item">
                                    <div className="user-info">
                                        <span className="user-name">
                                            <i className="bi bi-person-fill"></i> {user.firstName} {user.lastName}
                                        </span>
                                        <span className="user-phone">
                                            <i className="bi bi-telephone-fill"></i> {user.phone}
                                        </span>
                                    </div>
                                    <button onClick={() => handleAddContact(user.id)} className="add-contact-button">
                                        <i className="bi bi-person-plus-fill"></i> Добавить
                                    </button>
                                </li>
                            ))}
                        </ul>
                    </div>
                )}
            </div>
    );
}

export default UserSearch;
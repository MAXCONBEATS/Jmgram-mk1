 import React, { useState } from 'react';
 import axios from 'axios';
 axios.defaults.baseURL = 'https://localhost:5087';
 
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
    <button onClick={handleSearch}>Поиск</button>
 
    {error && <p className="error-message">{error}</p>}
 
    {searchResults.length > 0 && (
     <ul>
      {searchResults.map((user) => (
       <li key={user.id}>
        {user.firstName} {user.lastName} ({user.phone})
        <button onClick={() => handleAddContact(user.id)}>Добавить в контакты</button>
       </li>
      ))}
     </ul>
    )}
   </div>
  );
 }
 
 export default UserSearch;
 import React, { useState } from 'react';
 import { useNavigate, Link } from 'react-router-dom'; //  <-- Добавьте Link
 import { register } from '../controllers/AccountController';
 
 function Register() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
 
  const handleSubmit = async (e) => {
   e.preventDefault();
 
   try {
    await register(firstName, lastName, phone, password);
    console.log('Регистрация успешна!');
    navigate('/login'); // Перенаправить на страницу входа
   } catch (error) {
    setError('Ошибка регистрации: ' + (error.response?.data?.error || error.message));
   }
  };
 
  return (
   <div>
    <h2>Регистрация</h2>
    {error && <p style={{ color: 'red' }}>{error}</p>}
    <form onSubmit={handleSubmit}>
     <div>
      <label>Имя:</label>
      <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
     </div>
     <div>
      <label>Фамилия:</label>
      <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} />
     </div>
     <div>
      <label>Телефон:</label>
      <input type="text" value={phone} onChange={(e) => setPhone(e.target.value)} />
     </div>
     <div>
      <label>Пароль:</label>
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
     </div>
     <button type="submit">Зарегистрироваться</button>
    </form>
 
    <p>Уже зарегистрированы?</p> {/* Текст */}
    <Link to="/login">
     <button>Войти</button> {/* Кнопка */}
    </Link>
   </div>
  );
 }
 
 export default Register;
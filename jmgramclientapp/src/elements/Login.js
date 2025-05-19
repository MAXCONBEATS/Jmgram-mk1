 import React, { useState } from 'react';
 import { useNavigate, Link } from 'react-router-dom'; //  <-- Добавьте Link
 import { login } from '../controllers/AccountController';
 
 function Login({ onLogin }) {
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
 
  const handleSubmit = async (e) => {
   e.preventDefault();
 
   try {
    await login(phone, password);
    console.log('Вход выполнен!');
    navigate('/'); //  Перенаправить на главную страницу
    onLogin(); //  <--- Вызываем функцию onLogin
   } catch (error) {
    setError('Ошибка входа: ' + (error.response?.data || error.message));
   }
  };
 
  return (
   <div>
    <h2>Вход</h2>
    {error && <p style={{ color: 'red' }}>{error}</p>}
    <form onSubmit={handleSubmit}>
     <div>
      <label>Телефон:</label>
      <input type="text" value={phone} onChange={(e) => setPhone(e.target.value)} />
     </div>
     <div>
      <label>Пароль:</label>
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
     </div>
     <button type="submit">Войти</button>
    </form>
 
    <p>Еще не зарегистрированы?</p> {/* Текст */}
    <Link to="/register">
     <button>Зарегистрироваться</button> {/* Кнопка */}
    </Link>
   </div>
  );
 }
 
 export default Login;
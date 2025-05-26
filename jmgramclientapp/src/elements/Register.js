 import React, { useState } from 'react';
 import { useNavigate, Link } from 'react-router-dom';
 import { register } from '../controllers/AccountController';
 import '../css/Register.css';
 
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
   <div className="register-container">
    <div className="register-form-area">
     <i className="bi bi-person register-logo" style={{ fontSize: '5em', color: 'white' }}></i>
     <h2>Регистрация</h2>
     {error && <p className="error-message">{error}</p>}
     <form onSubmit={handleSubmit}>
      <div className="form-group">
       <label htmlFor="firstName">Имя:</label>
       <input
        type="text"
        id="firstName"
        value={firstName}
        onChange={(e) => setFirstName(e.target.value)}
       />
      </div>
      <div className="form-group">
       <label htmlFor="lastName">Фамилия:</label>
       <input
        type="text"
        id="lastName"
        value={lastName}
        onChange={(e) => setLastName(e.target.value)}
       />
      </div>
      <div className="form-group">
       <label htmlFor="phone">Телефон:</label>
       <input
        type="text"
        id="phone"
        value={phone}
        onChange={(e) => setPhone(e.target.value)}
       />
      </div>
      <div className="form-group">
       <label htmlFor="password">Пароль:</label>
       <input
        type="password"
        id="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
       />
      </div>
      <button type="submit" className="register-button">
       Зарегистрироваться
      </button>
     </form>
     <p>
        Уже зарегистрированы?
         <Link to="/login" className="login-link">Войти</Link>
    </p>
    </div>
   </div>
  );
 }
 
 export default Register;
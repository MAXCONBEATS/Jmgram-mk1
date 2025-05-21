 import React, { useState } from 'react';
 import { useNavigate, Link } from 'react-router-dom';
 import { login } from '../controllers/AccountController';
 import login_icon from '../assets/images/login_icon.png';
 import '../css/Login.css';
 
 function Login({ onLogin }) {
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const navigate = useNavigate();
 
  const handleSubmit = async (e) => {
   e.preventDefault();
 
   try {
    await login(phone, password);
    console.log('Вход выполнен!');
    navigate('/');
    onLogin();
   } catch (error) {
    setError('Ошибка входа: ' + (error.response?.data || error.message));
   }
  };
 
  const togglePasswordVisibility = () => {
   setIsPasswordVisible(!isPasswordVisible);
  };
 
  return (
   <div className="login-container">
    <div className="login-form-area">
     <img src={login_icon} alt="Логотип" className="login-logo" />
     <h2>Вход</h2>
     {error && <p className="error-message">{error}</p>}
     <form onSubmit={handleSubmit}>
      <div className="form-group">
       <label htmlFor="phone">Телефон:</label>
       <input
        type="text"
        id="phone"
        value={phone}
        onChange={(e) => setPhone(e.target.value)}
       />
      </div>
      <div className="form-group password-group">
       <label htmlFor="password">Пароль:</label>
       <div className="password-input-group">
        <input
         type={isPasswordVisible ? 'text' : 'password'}
         id="password"
         value={password}
         onChange={(e) => setPassword(e.target.value)}
        />
        <button
         type="button"
         className="toggle-password"
         onClick={togglePasswordVisibility}
        >
         {isPasswordVisible ? 'Скрыть' : 'Показать'}
        </button>
       </div>
      </div>
      <button type="submit" className="login-button">
       Войти
      </button>
     </form>
 
     <p>Еще не зарегистрированы?</p>
     <Link to="/register">
      <button className="register-button">Зарегистрироваться</button>
     </Link>
    </div>
   </div>
  );
 }
 
 export default Login;
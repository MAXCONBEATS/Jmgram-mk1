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
    console.log('handleSubmit called with phone:', phone);

     try {
      const response = await login(phone, password);
      console.log('Вход выполнен!', response);
      if (response) {
       try {
         // After login, fetch current user info from /Account/Me
         console.log('Fetching current user info...');
         const userResponse = await fetch('https://localhost:5087/Account/GetCurrentUser/Me', {
           method: 'GET',
           credentials: 'include',
           headers: {
             'Accept': 'application/json',
           },
         });
         console.log('User response status:', userResponse.status);
         if (userResponse.ok) {
           const userData = await userResponse.json();
           console.log('Current user data:', userData);
           localStorage.setItem('UserId', userData.id || '');
           localStorage.setItem('UserName', userData.userName || userData.name || '');
         } else {
           console.error('Failed to fetch current user info:', userResponse.status);
         }
       } catch (fetchError) {
         console.error('Error fetching current user info:', fetchError);
       }
      }
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
 import React from 'react';
 import axios from 'axios';
 import { useNavigate } from 'react-router-dom';
 import Cookies from 'js-cookie'; //  <-- Import Cookies
 
 function LogoutButton({ onLogout }) {
  const navigate = useNavigate();
 
  const handleLogout = async () => {
   try {
    await axios.post('http://127.0.0.1:5087/Account/Logout', null, { withCredentials: true });
    Cookies.remove('.AspNetCore.Identity.Application', { path: '/', domain: 'localhost' }); //  <-- Удалите Cookie
    navigate('/login');
    onLogout();
   } catch (error) {
    console.error('Ошибка при выходе из системы:', error);
   }
  };
 
  return <button onClick={handleLogout}>Выйти</button>;
 }
 
 export default LogoutButton;
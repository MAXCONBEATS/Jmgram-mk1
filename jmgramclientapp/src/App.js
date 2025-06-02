import React, { useState, useEffect } from 'react';
 import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
 import './css/App.css';
 import Login from './elements/Login';
 import Register from './elements/Register';
 import { isAuthenticated } from './controllers/AccountController';
 import { getUserChats } from './controllers/ChatController';
 import Main from './elements/Main';
 import axios from 'axios';
  axios.defaults.baseURL = 'https://localhost:5087';
  axios.defaults.withCredentials = true;
 
 function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userChats, setUserChats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [contacts, setContacts] = useState(['Контакт 1', 'Контакт 2', 'Контакт 3']);
 
  useEffect(() => {
   const checkAuth = async () => {
    setLoading(true);
    const auth = await isAuthenticated();
    setIsLoggedIn(auth);
    setLoading(false);
   };
 
   checkAuth();
  }, []);
 
 
  useEffect(() => {
   const fetchUserChats = async () => {
    try {
     const data = await getUserChats();
     setUserChats(data);
     setError(null);
    } catch (error) {
     console.error('Error fetching user chats:', error);
     setError(error.message || 'Ошибка при получении данных UserChats');
     setUserChats(null);
    }
   };
 
   if (isLoggedIn) {
    fetchUserChats();
   }
  }, [isLoggedIn]);

  const handleLogout = () => {
   setIsLoggedIn(false);
  };
 
  const handleLogin = () => {
   setIsLoggedIn(true);
  };

  return (
   loading ? (
    <div>Загрузка...</div>
   ) : (
    <BrowserRouter>
     <Routes>
      <Route path="/register" element={<Register />} />
      <Route path="/login" element={<Login onLogin={handleLogin} />} />
      <Route
       path="/"
       element={
        isLoggedIn ? (
         <Main
          userChats={userChats}
          contacts={contacts}
          error={error}
          onLogout={handleLogout}
         />
        ) : (
         <Navigate to="/login" />
        )
       }
      />
     </Routes>
    </BrowserRouter>
   )
  );
 }
 
 export default App;

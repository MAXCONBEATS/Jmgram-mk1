import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

function App() {
 const [chats, setChats] = useState([]);

 useEffect(() => {
  const fetchData = async () => {
   try {
    const token = localStorage.getItem('token'); // Получите токен из localStorage

    const response = await axios.get('http://localhost:7142/api/Chat/UserChats', {
     headers: {
      Authorization: `Bearer ${token}` // Добавьте токен в заголовок Authorization
     }
    });
    setChats(response.data);
   } catch (error) {
    console.error('Error fetching data:', error);
   }
  };
  fetchData();
 }, []);

 return (
  <div className="App">
   <h1>Chats</h1>
   <ul>
    {chats.map(chat => (
     <li key={chat.id}>{chat.name}</li>
    ))}
   </ul>
  </div>
 );
}

export default App;
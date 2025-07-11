import { useState, useEffect } from "react"
import "../css/Main.css"
import "../css/ContextMenu.css"
import axios from "axios"
import ChatListContainer from "./ChatListContainer"
import ChatInvitationsList from "./ChatInvitationsList"
import CreateChatButton from "./CreateChatButton"
import ChatWindow from "./ChatWindow"
import ContactsPanel from "./ContactsPanel"
import NotificationsPanel from "./NotificationsPanel"
import ProfileModal from "./ProfileModal"
import SettingsModal from "./SettingsModal"
import { getContactList, getContactRequests, acceptContactRequest } from "../controllers/ContactController"
import { UserController } from "../controllers/UserController"

axios.defaults.baseURL = "https://localhost:5087"

function Main({ error, onLogout }) {
    const [selectedChat, setSelectedChat] = useState(null)
    const [selectedChatInvitation, setSelectedChatInvitation] = useState(null)
    const [refreshChats, setRefreshChats] = useState(false)
    const [refreshInvitations, setRefreshInvitations] = useState(0)
    const [profileUserId, setProfileUserId] = useState(null)
    const [showSettings, setShowSettings] = useState(false)
    const [refreshContacts, setRefreshContacts] = useState(false)

    const [contacts, setContacts] = useState([])
    const [contactRequests, setContactRequests] = useState([])

    const userId = localStorage.getItem("UserId")
    const userName = localStorage.getItem("UserName") || "Пользователь"

    const refreshContactsAndChats = async () => {
        try {
            const updatedContacts = await getContactList()
            setContacts(updatedContacts)
            setRefreshChats((prev) => !prev)
            setRefreshInvitations((prev) => prev + 1)
        } catch (error) {
            console.error("Ошибка при обновлении контактов и чатов:", error)
        }
    }

    useEffect(() => {
        const fetchContacts = async () => {
            try {
                const contactList = await getContactList()
                setContacts(contactList)
            } catch (error) {
                console.error("Ошибка при получении списка контактов:", error)
            }
        }
        fetchContacts()
    }, [refreshContacts])

    useEffect(() => {
        const fetchContactRequests = async () => {
            try {
                const requestsData = await getContactRequests()
                const mappedRequestsPromises = requestsData.map(async (request) => {
                    let senderName = "Неизвестный пользователь"; // По умолчанию
                    try {
                        // Здесь мы получаем профиль отправителя запроса
                        const senderProfile = await UserController.getProfile(request.senderUserId);
                        senderName = `${senderProfile.firstName} ${senderProfile.lastName}`;
                    } catch (profileError) {
                        console.error(`Ошибка при получении профиля для отправителя ${request.senderUserId}:`, profileError);
                        // Если профиль не найден или ошибка, используем fallback
                        senderName = `Неизвестный пользователь (${request.senderUserId.substring(0, 4)}...)`;
                    }
                    return {
                        ...request,
                        senderName: senderName,
                    };
                });
                const mappedRequests = await Promise.all(mappedRequestsPromises); // Ждем выполнения всех промисов
                setContactRequests(mappedRequests);
            } catch (error) {
                console.error("Ошибка при получении запросов в контакты:", error)
            }
        }
        fetchContactRequests()
    }, [refreshContacts, userId]) // userId в зависимостях, хотя для входящих запросов senderUserId ≠ userId

    const handleAcceptContactRequest = async (contactRequestId) => {
        try {
            const result = await acceptContactRequest(contactRequestId)
            if (typeof result === "string" || (result && result.isSuccess)) {
                setContactRequests((prev) => prev.filter((req) => req.id !== contactRequestId))
                await refreshContactsAndChats()
            } else {
                alert(`Ошибка при принятии запроса: ${result.errorMessage || "Неизвестная ошибка"}`)
            }
        } catch (error) {
            console.error("Ошибка при принятии запроса в контакты:", error)
        }
    }

    const handleCreateChat = (newChat) => {
        console.log("handleCreateChat called with newChat:", newChat)
        setRefreshChats((prev) => !prev)
        setRefreshInvitations((prev) => prev + 1)
        setSelectedChat(newChat)
        setSelectedChatInvitation(null)
    }

    const handleChatSelect = (chat) => {
        setSelectedChat(chat)
        setSelectedChatInvitation(null)
    }

    const handleInvitationSelect = (invitation) => {
        setSelectedChatInvitation(invitation)
        setSelectedChat(null)
    }

    const handleCloseChat = () => {
        setSelectedChat(null)
    }

    const handleRefreshInvitations = () => {
        setRefreshInvitations((prev) => prev + 1)
    }

    return (
        <div className="main-container">
            <div className="header-buttons">
                <button onClick={() => setProfileUserId(userId)} className="btn btn-outline-light btn-sm">
                    <i className="bi bi-person-circle"></i> Мой профиль
                </button>

                <button onClick={() => setShowSettings(true)} className="btn btn-outline-light btn-sm">
                    <i className="bi bi-gear"></i> Настройки
                </button>
            </div>

            {error && <p className="error-message">{error}</p>}

            <div className="main-content">
                <ContactsPanel
                    userId={userId}
                    contacts={contacts}
                    contactRequests={contactRequests}
                    onAcceptContactRequest={handleAcceptContactRequest}
                    refreshTrigger={refreshContacts}
                    setProfileUserId={setProfileUserId}
                    refreshContactsAndChats={refreshContactsAndChats}
                />

                <div className="chat-section"> {/* Переименовал div для ясности */}
                    <h2>Чаты</h2>

                    <div>
                        <div>
                            <ChatListContainer key={refreshChats} selectedChat={selectedChat} setSelectedChat={handleChatSelect} />
                        </div>
                        <ChatInvitationsList
                            selectedChatInvitation={selectedChatInvitation}
                            setSelectedChatInvitation={handleInvitationSelect}
                            refreshTrigger={refreshInvitations}
                        />
                    </div>

                    <div>
                        <CreateChatButton contacts={contacts} onCreateChat={handleCreateChat} />
                    </div>
                </div>
            </div>

            <NotificationsPanel />

            {selectedChat && (
                <ChatWindow
                    chat={selectedChat}
                    onClose={handleCloseChat}
                    senderId={userId}
                    senderName={userName}
                    currentUserId={userId}
                    contacts={contacts}
                />
            )}

            {profileUserId && (
                <ProfileModal
                    userId={profileUserId}
                    onClose={() => setProfileUserId(null)}
                    onContactDeleted={refreshContactsAndChats}
                />
            )}

            {showSettings && (
                <SettingsModal
                    onClose={() => setShowSettings(false)}
                    onLogout={onLogout}
                />
            )}
        </div>
    )
}

export default Main
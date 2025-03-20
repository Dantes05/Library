import React, { useState } from "react";
import { login, register } from "../api";
import { useNavigate } from "react-router-dom";

const Login: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [showRegister, setShowRegister] = useState(false); 
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      const response = await login(email, password);
      console.log("📌 Ответ API:", response);
  
      if (!response.token) {
        throw new Error("❌ Токен не получен. Проверь API!");
      }
  
      localStorage.setItem("token", response.token);
      navigate("/books");
    } catch (error: any) {
      setError("Ошибка авторизации");
      console.error("🛑 Ошибка авторизации:", error);
    }
  };
  

  return (
    <div className="flex flex-col items-center justify-center h-screen">
      <h2 className="text-2xl font-bold mb-4">Вход</h2>
      {error && <p className="text-red-500">{error}</p>}
      <input className="border p-2 mb-2" type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
      <input className="border p-2 mb-2" type="password" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
      <button className="bg-blue-500 text-white px-4 py-2 mb-2" onClick={handleLogin}>Войти</button>
      <button className="text-blue-500" onClick={() => setShowRegister(true)}>Зарегистрироваться</button>

      {showRegister && <RegisterModal onClose={() => setShowRegister(false)} />}
    </div>
  );
};

const RegisterModal: React.FC<{ onClose: () => void }> = ({ onClose }) => {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [regEmail, setRegEmail] = useState("");
  const [regPassword, setRegPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleRegister = async () => {
    if (regPassword !== confirmPassword) {
      setError("Пароли не совпадают");
      return;
    }

    try {
      await register(firstName, lastName, regEmail, regPassword, confirmPassword);
      setSuccess("Регистрация успешна! Теперь войдите.");
      setError("");
    } catch (error: any) {
      setError("Ошибка регистрации");
      setSuccess("");
      console.error("Ошибка:", error);
    }
  };

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
      <div className="bg-white p-6 rounded-lg shadow-lg">
        <h2 className="text-xl font-bold mb-4">Регистрация</h2>
        {error && <p className="text-red-500">{error}</p>}
        {success && <p className="text-green-500">{success}</p>}
        <input className="border p-2 mb-2 w-full" type="text" placeholder="Имя" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
        <input className="border p-2 mb-2 w-full" type="text" placeholder="Фамилия" value={lastName} onChange={(e) => setLastName(e.target.value)} />
        <input className="border p-2 mb-2 w-full" type="email" placeholder="Email" value={regEmail} onChange={(e) => setRegEmail(e.target.value)} />
        <input className="border p-2 mb-2 w-full" type="password" placeholder="Пароль" value={regPassword} onChange={(e) => setRegPassword(e.target.value)} />
        <input className="border p-2 mb-2 w-full" type="password" placeholder="Подтвердите пароль" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} />
        <div className="flex justify-between">
          <button className="bg-blue-500 text-white px-4 py-2" onClick={handleRegister}>Зарегистрироваться</button>
          <button className="bg-gray-500 text-white px-4 py-2" onClick={onClose}>Закрыть</button>
        </div>
      </div>
    </div>
  );
};


export default Login;

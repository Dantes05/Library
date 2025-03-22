import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

interface Book {
  id: number;
  title: string;
  genre: string;
  authorName: string;
  borrowedAt: string;
  returnAt: string;
}

const ProfilePage: React.FC = () => {
  const [books, setBooks] = useState<Book[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetchRentedBooks();
  }, []);

  const fetchRentedBooks = async () => {
    try {
      const token = localStorage.getItem("token");

      if (!token) {
        alert("Пользователь не авторизован.");
        navigate("/login");
        return;
      }


      const response = await axios.get("http://localhost:8080/api/books/user/rentals", {
        headers: { Authorization: `Bearer ${token}` },
      });

      setBooks(response.data);
    } catch (error) {
      console.error("Ошибка при загрузке книг:", error);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Личный кабинет</h1>

      <button
        className="bg-blue-500 text-white px-4 py-2 mb-4"
        onClick={() => navigate("/books")}
      >
        🔙 Назад к списку книг
      </button>

      <div className="grid grid-cols-3 gap-4">
        {books.length === 0 ? (
          <p>Вы не взяли ни одной книги.</p>
        ) : (
          books.map((book) => (
            <div key={book.id} className="border p-4 rounded shadow">
              <h2 className="text-xl font-bold">{book.title}</h2>
              <p>{book.genre}</p>
              <p>Автор: {book.authorName}</p>
              <p>Взята: {new Date(book.borrowedAt).toLocaleDateString()}</p>
              <p>Вернуть до: {new Date(book.returnAt).toLocaleDateString()}</p>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default ProfilePage;
import React, { useEffect, useState } from "react";
import { getBooks } from "../api";
import { useNavigate } from "react-router-dom";
import axios from "axios";

interface Book {
  id: number;
  title: string;
  genre: string;
  authorName: string;
  isAvailable: boolean;
}

const BooksList: React.FC = () => {
  const [books, setBooks] = useState<Book[]>([]);
  const [search, setSearch] = useState("");
  const [genre, setGenre] = useState("");
  const [author, setAuthor] = useState("");
  const [page, setPage] = useState(1);
  const [isAdmin, setIsAdmin] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    fetchBooks();
    checkAdmin();
  }, [search, genre, author, page]);

  const fetchBooks = async () => {
    try {
      const data = await getBooks(search, genre, author, page);
      console.log("Данные с сервера:", data);

      const booksWithAvailability = await Promise.all(
        data.map(async (book: any) => {
          try {
            const response = await axios.get(`http://localhost:7143/api/books/${book.id}/rentals`);
            console.log("Ответ от bookrentals:", response.data);

            const isRented = response.data.length > 0;
            return { ...book, isAvailable: !isRented };
          } catch (error) {
            console.error("Ошибка при проверке доступности книги:", error);
            return { ...book, isAvailable: false };
          }
        })
      );

      setBooks(booksWithAvailability);
    } catch (error) {
      console.error("Ошибка при загрузке книг:", error);
    }
  };

  const checkAdmin = () => {
    const token = localStorage.getItem("token");

    if (!token) {
      console.warn("🔴 Токен отсутствует.");
      setIsAdmin(false);
      return;
    }

    console.log("📌 Полученный токен:", token);

    try {
      const parts = token.split(".");
      if (parts.length !== 3) {
        throw new Error("⚠️ Некорректный формат токена (JWT должен содержать 3 части)");
      }

      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      const decodedPayload = JSON.parse(atob(base64));

      console.log("📌 Декодированные данные токена:", decodedPayload);

      const userRole =
        decodedPayload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

      if (userRole) {
        setIsAdmin(userRole.toLowerCase() === "admin");
      } else {
        console.warn("⚠️ Роль отсутствует в токене.");
        setIsAdmin(false);
      }
    } catch (error) {
      console.error("🛑 Ошибка декодирования токена:", error);
      setIsAdmin(false);
    }
  };

  useEffect(() => {
    console.log("Текущие книги:", books);
  }, [books]);

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Список книг</h1>

      <div className="flex gap-2 mb-4">
        <button
          className="bg-blue-500 text-white px-4 py-2"
          onClick={() => navigate("/profile")}
        >
          👤 Личный кабинет
        </button>

        {isAdmin && (
          <button
            className="bg-green-500 text-white px-4 py-2"
            onClick={() => navigate("/add-book")}
          >
            ➕ Добавить книгу
          </button>
        )}
      </div>

      <div className="flex gap-2 mb-4">
        <input
          type="text"
          placeholder="Поиск по названию..."
          className="border p-2"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <input
          type="text"
          placeholder="Введите жанр..."
          className="border p-2"
          value={genre}
          onChange={(e) => setGenre(e.target.value)}
        />
        <input
          type="text"
          placeholder="Введите имя автора..."
          className="border p-2"
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
        />
      </div>

      <div className="grid grid-cols-3 gap-4">
        {books.length === 0 ? (
          <p>Книги не найдены</p>
        ) : (
          books.map((book) => (
            <div
              key={book.id}
              className="border p-4 rounded shadow cursor-pointer"
              onClick={() => navigate(`/books/${book.id}`)}
            >
              <h2 className="text-xl font-bold">{book.title}</h2>
              <p>{book.genre}</p>
              <p>Автор: {book.authorName}</p>
              <p className={book.isAvailable ? "text-green-500" : "text-red-500"}>
                {book.isAvailable ? "В наличии" : "Нет в наличии"}
              </p>
            </div>
          ))
        )}
      </div>

      <div className="flex justify-center mt-4">
        <button className="p-2 border" onClick={() => setPage(page - 1)} disabled={page === 1}>
          ⬅️ Назад
        </button>
        <span className="p-2">{page}</span>
        <button className="p-2 border" onClick={() => setPage(page + 1)}>
          Вперед ➡️
        </button>
      </div>
    </div>
  );
};

export default BooksList;
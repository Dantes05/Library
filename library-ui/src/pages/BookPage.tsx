import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";

const BookPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [book, setBook] = useState<any>(null);
  const [author, setAuthor] = useState<any>(null); 
  const [isAvailable, setIsAvailable] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [returnDate, setReturnDate] = useState("");
  const [isBookRentedByUser, setIsBookRentedByUser] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    fetchBook();
    checkAvailability();
    checkIfBookRentedByUser();
    checkAdmin();
  }, [id]);

  const checkAdmin = () => {
    const token = localStorage.getItem("token");
    if (!token) return;

    try {
      const base64Url = token.split(".")[1];
      const decodedPayload = JSON.parse(atob(base64Url.replace(/-/g, "+").replace(/_/g, "/")));
      console.log("Декодированный токен:", decodedPayload);

      const role = decodedPayload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      setIsAdmin(role === "Admin");
    } catch (error) {
      console.error("Ошибка при проверке роли:", error);
    }
  };

  const fetchBook = async () => {
    try {
      const response = await axios.get(`http://localhost:8080/api/books/${id}`);
      console.log("Данные книги:", response.data); 
      setBook(response.data);


      if (response.data.authorId) {
        const authorResponse = await axios.get(`http://localhost:8080/api/authors/${response.data.authorId}`);
        console.log("Данные автора:", authorResponse.data); 
        setAuthor(authorResponse.data);
      }
    } catch (error) {
      console.error("Ошибка при загрузке книги:", error);
    }
  };

  const checkAvailability = async () => {
    try {
      const response = await axios.get(`http://localhost:8080/api/books/${id}/rentals`);
      setIsAvailable(response.data.length === 0);
    } catch (error) {
      console.error("Ошибка при проверке наличия книги:", error);
      setIsAvailable(false);
    }
  };

  const checkIfBookRentedByUser = async () => {
    try {
      const token = localStorage.getItem("token");
      if (!token) return;

      const base64Url = token.split(".")[1];
      const decodedPayload = JSON.parse(atob(base64Url.replace(/-/g, "+").replace(/_/g, "/")));

      console.log("Декодированный токен:", decodedPayload);

      const response = await axios.get(`http://localhost:8080/api/books/${id}/is-rented`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      console.log("Ответ API (книга арендована?):", response.data);

      setIsBookRentedByUser(response.data.isRented);
    } catch (error) {
      console.error("Ошибка при проверке, взял ли пользователь книгу:", error);
    }
  };

  const handleBorrowBook = async () => {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        console.error("Токен отсутствует.");
        return;
      }

      const response = await axios.post(
        "http://localhost:8080/api/books/borrow",
        {
          bookId: id,
          returnAt: returnDate,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      console.log("Книга успешно взята:", response.data);
      setShowModal(false);
      setIsAvailable(false);
      setIsBookRentedByUser(true);
    } catch (error) {
      console.error("Ошибка при взятии книги:", error);
    }
  };

  const handleReturnBook = async () => {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        console.error("Токен отсутствует.");
        return;
      }

      console.log("запрос на возврат книги:", id);

      const response = await axios.post(
        `http://localhost:8080/api/books/return/${id}`,
        {},
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      console.log("Книга успешно возвращена:", response.data);
      setIsAvailable(true);
      setIsBookRentedByUser(false);
    } catch (error: any) {
      console.error("Ошибка при возврате книги:", error.response?.data || error.message);
    }
  };

  const handleDeleteBook = async () => {
    try {
      const token = localStorage.getItem("token");
      if (!token) return;

      await axios.delete(`http://localhost:8080/api/books/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      console.log("Книга успешно удалена");
      navigate("/books");
    } catch (error) {
      console.error("Ошибка при удалении книги:", error);
    }
  };

  if (!book) return <p>Загрузка...</p>;


  const authorName = author ? `${author.firstName} ${author.lastName}` : "Автор неизвестен";

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold">{book.title}</h1>

      {book.imagePath ? (
        <img
          src={`http://localhost:8080${book.imagePath}`}
          alt={book.title}
          className="w-64 h-80 object-cover rounded-lg shadow-lg my-4"
        />
      ) : (
        <p className="text-gray-500">Нет изображения</p>
      )}

      <p><strong>ISBN:</strong> {book.isbn}</p>
      <p><strong>Жанр:</strong> {book.genre}</p>
      <p><strong>Описание:</strong> {book.description}</p>
      <p><strong>Автор:</strong> {authorName}</p>
      <p>
        <strong>Статус:</strong>{" "}
        <span className={isAvailable ? "text-green-500" : "text-red-500"}>
          {isAvailable ? "В наличии" : "Нет в наличии"}
        </span>
      </p>

      {isAvailable ? (
        <button className="bg-blue-500 text-white px-4 py-2 mt-4" onClick={() => setShowModal(true)}>
          📖 Взять книгу
        </button>
      ) : isBookRentedByUser ? (
        <button className="bg-red-500 text-white px-4 py-2 mt-4" onClick={handleReturnBook}>
          📚 Вернуть книгу
        </button>
      ) : (
        <p className="text-red-500 font-bold">❌ Нет в наличии</p>
      )}

      {isAdmin && (
        <div className="mt-4 flex gap-4">
          <button className="bg-yellow-500 text-white px-4 py-2" onClick={() => navigate(`/edit-book/${id}`)}>
            ✏️ Редактировать
          </button>
          <button
            className={`bg-red-500 text-white px-4 py-2 ${!isAvailable ? "opacity-50 cursor-not-allowed" : ""}`}
            onClick={() => isAvailable && setShowDeleteModal(true)}
            disabled={!isAvailable}
          >
            🗑 Удалить
          </button>
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h2 className="text-xl font-bold mb-4">Выберите дату возврата</h2>
            <input
              className="border p-2 mb-2 w-full"
              type="date"
              min={new Date().toISOString().split("T")[0]}
              max={new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0]}
              value={returnDate}
              onChange={(e) => setReturnDate(e.target.value)}
            />
            <div className="flex justify-between">
              <button className="bg-green-500 text-white px-4 py-2" onClick={handleBorrowBook}>
                ✅ Взять книгу
              </button>
              <button className="bg-gray-500 text-white px-4 py-2" onClick={() => setShowModal(false)}>
                ❌ Отмена
              </button>
            </div>
          </div>
        </div>
      )}

      {showDeleteModal && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h2 className="text-xl font-bold mb-4">Вы уверены, что хотите удалить книгу?</h2>
            <div className="flex justify-between">
              <button className="bg-red-500 text-white px-4 py-2" onClick={handleDeleteBook}>
                ✅ Удалить
              </button>
              <button className="bg-gray-500 text-white px-4 py-2" onClick={() => setShowDeleteModal(false)}>
                ❌ Отмена
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default BookPage;